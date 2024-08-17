/*
This is free and unencumbered software released into the public domain under The Unlicense.
You have complete freedom to do anything you want with the software, for any purpose.
Please refer to <http://unlicense.org/>
*/

using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace FamilyPicScreenSaver
{
  public class MediaSelector
  {
    private readonly MediaFinder _mediaFinder;
    private readonly MediaPlayerThreadedWrapper _mediaPlayerController;
    private readonly Random _random = new Random((int)(DateTime.UtcNow.Ticks % int.MaxValue));
    private readonly System.Threading.Timer _pictureChangeTimer;

    private readonly object _currentLock = new object();
    private readonly Stack<int> _previousIndexes = new();
    private readonly Stack<int> _nextIndexes = new();
    private int _lastObservedMediaCount;
    private int _currentAutomaticAdvanceCount;
    private int? _currentMediaIndex;
    private string _currentFilePath;
    private MediaType _currentMediaType;
    private Stopwatch _currentPictureDisplayedTime;
    private TimeSpan _currentPictureTimeout;
    private string _debugInfo;

    public string DebugInfo { get { lock (_currentLock) return _debugInfo; } }

    public event Action MediaChanged;

    public MediaSelector(LibVLC libVlc)
    {
      _mediaPlayerController = new MediaPlayerThreadedWrapper(libVlc);
      _mediaPlayerController.EndOfVideoReached += MediaPlayerController_EndOfVideoReached;
      _mediaFinder = new MediaFinder(Settings.LoadMediaFolders());
      _pictureChangeTimer = new System.Threading.Timer(PictureChangeTimerTick);
      _pictureChangeTimer.Change(100, 100);

      // get something displayed now
      Navigate(NavigationType.RandomManually);
    }

    enum NavigationType
    {
      ForwardAutomatically,
      ForwardManually,
      BackManually,
      RandomManually,
    }

    private int GetRandomIndex()
    {
      var newIndex = _random.Next(_mediaFinder.Media.Count);
      if (newIndex == _currentMediaIndex)
      {
        return newIndex + 1;
      }
      else
      {
        return newIndex;
      }
    }

    private void Navigate(NavigationType navigationType)
    {
      lock (_currentLock)
      {
        StringBuilder debugInfo = new();
        debugInfo.Append(navigationType.ToString() + ", ");

        var media = _mediaFinder.Media;

        if (navigationType == NavigationType.ForwardAutomatically)
        {
          _currentAutomaticAdvanceCount++;
        }
        else
        {
          _currentAutomaticAdvanceCount = 0;
        }

        // save the current image/video we're looking at for back/forward functionality
        if (_currentMediaIndex != null)
        {
          switch (navigationType)
          {
            case NavigationType.ForwardAutomatically:
            case NavigationType.ForwardManually:
              _previousIndexes.Push(_currentMediaIndex.Value);
              break;
            case NavigationType.BackManually:
              _nextIndexes.Push(_currentMediaIndex.Value);
              break;
            case NavigationType.RandomManually:
              _previousIndexes.Push(_currentMediaIndex.Value);
              break;
          }
        }

        // decide what to view next
        switch (navigationType)
        {
          case NavigationType.ForwardAutomatically:
            // try to have some randomness while images are being found
            if (_currentFilePath == MediaFinder.LoadingPicPath || _lastObservedMediaCount != media.Count)
            {
              debugInfo.Append("random (loading or media count change)");
              _currentMediaIndex = GetRandomIndex();
              _lastObservedMediaCount = media.Count;
            }
            else if (_nextIndexes.Count > 0)
            {
              debugInfo.Append($"_nextIndexes (Count = {_nextIndexes.Count})");
              _currentMediaIndex = _nextIndexes.Pop();
            }
            // pick a random new image every 10, so
            // 1.) reduce constant jarring changes in time period of images/videos shown
            // 2.) don't get stuck watching a hundred of young Alaric's pictures of the floor
            else if (_currentAutomaticAdvanceCount >= 10)
            {
              debugInfo.Append($"random (Count == {_currentAutomaticAdvanceCount})");
              _currentMediaIndex = GetRandomIndex();
            }
            else if (_currentMediaIndex == null)
            {
              debugInfo.Append($"_currentMediaIndex == null");
              _currentMediaIndex = 0;
            }
            else
            {
              debugInfo.Append($"increment");
              _currentMediaIndex++;
            }
            break;

          case NavigationType.ForwardManually:
            if (_nextIndexes.Count > 0)
            {
              debugInfo.Append($"_nextIndexes (Count = {_nextIndexes.Count})");
              _currentMediaIndex = _nextIndexes.Pop();
            }
            else if (_currentMediaIndex == null)
            {
              debugInfo.Append($"_currentMediaIndex == null");
              _currentMediaIndex = 0;
            }
            else
            {
              debugInfo.Append($"increment");
              _currentMediaIndex++;
            }
            break;

          case NavigationType.BackManually:
            if (_previousIndexes.Count > 0)
            {
              debugInfo.Append($"_previousIndexes (Count = {_previousIndexes.Count})");
              _currentMediaIndex = _previousIndexes.Pop();
            }
            else if (_currentMediaIndex == null)
            {
              debugInfo.Append($"_currentMediaIndex == null");
              _currentMediaIndex = media.Count - 1;
            }
            else
            {
              debugInfo.Append($"decrement");
              _currentMediaIndex--;
            }
            break;

          // case NavigationType.RandomManually:
          default:
            debugInfo.Append($"random");
            _currentMediaIndex = GetRandomIndex();
            _nextIndexes.Clear();
            break;
        }

        // sanity check the new index
        if (_currentMediaIndex < 0)
        {
          _currentMediaIndex = media.Count - 1;
        }
        if (_currentMediaIndex >= media.Count)
        {
          _currentMediaIndex = 0;
        }
        if (_currentMediaIndex == null)
        {
          _currentMediaIndex = 0;
        }
        // MediaFinder guarantees that media always has at least 1 item, so 0 is safe
        debugInfo.Append($", index={_currentMediaIndex}");

        _currentFilePath = media[_currentMediaIndex.Value];
        _currentMediaType = MediaFinder.FilePathIsProbablyVideo(_currentFilePath) ? MediaType.Video : MediaType.Picture;

        debugInfo.AppendLine($", {_currentMediaType}");
        debugInfo.Append(_currentFilePath);

        if (_currentMediaType == MediaType.Video)
        {
          _mediaPlayerController.Play(_currentFilePath);
          _paused = false;
          _mediaPlayerController.SetPaused(false);
          _mediaPlayerController.SetMuted(_muted);
          _currentPictureDisplayedTime = null;
        }
        else
        {
          _mediaPlayerController.Stop();
          _currentPictureDisplayedTime = Stopwatch.StartNew();
          _currentPictureTimeout = TimeSpan.FromSeconds(_currentFilePath == MediaFinder.LoadingPicPath ? 1 : 10);
        }

        _debugInfo = debugInfo.ToString();
      }

      // Task so it happens while the lock isn't held
      Task.Run(() =>
      {
        try
        {
          MediaChanged?.Invoke();
        }
        catch
        {
          // oh well
        }
      });
    }

    public void AssociateWithVideoView(VideoView videoView)
    {
      _mediaPlayerController.AssociateWithVideoView(videoView);
    }

    public (string FilePath, MediaType MediaType) GetCurrentMedia()
    {
      lock (_currentLock)
      {
        return (_currentFilePath, _currentMediaType);
      }
    }

    private void MediaPlayerController_EndOfVideoReached()
    {
      Navigate(NavigationType.ForwardAutomatically);
    }

    private void PictureChangeTimerTick(object state)
    {
      lock (_currentLock)
      {
        if (_currentPictureDisplayedTime?.Elapsed > _currentPictureTimeout && !Paused)
        {
          Navigate(NavigationType.ForwardAutomatically);
        }
      }
    }

    public void Next()
    {
      lock (_currentLock)
      {
        Navigate(NavigationType.ForwardManually);
      }
    }

    public void Previous()
    {
      lock (_currentLock)
      {
        Navigate(NavigationType.BackManually);
      }
    }

    public void Random()
    {
      lock (_currentLock)
      {
        Navigate(NavigationType.RandomManually);
      }
    }

    public bool Paused
    {
      get => _paused;
      set
      {
        lock (_currentLock)
        {
          _currentAutomaticAdvanceCount = 0;
          _paused = value;
          if (value)
          {
            _currentPictureDisplayedTime?.Stop();
          }
          else
          {
            _currentPictureDisplayedTime?.Start();
          }
          _mediaPlayerController.SetPaused(value);
        }
      }
    }
    private bool _paused;

    public bool Muted
    {
      get => _muted;
      set
      {
        lock (_currentLock)
        {
          _currentAutomaticAdvanceCount = 0;
          _muted = value;
          _mediaPlayerController.SetMuted(value);
        }
      }
    }
    private bool _muted = true;
  }
}
