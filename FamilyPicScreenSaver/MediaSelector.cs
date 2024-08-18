/*
This is free and unencumbered software released into the public domain under The Unlicense.
You have complete freedom to do anything you want with the software, for any purpose.
Please refer to <http://unlicense.org/>
*/

using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Rope;
using Nito.Collections;
using System.IO;

namespace FamilyPicScreenSaver
{
  public class MediaSelector
  {
    private readonly MediaFinder _mediaFinder;
    private readonly MediaPlayerThreadedWrapper _mediaPlayerController;
    private readonly Random _random = new Random((int)(DateTime.UtcNow.Ticks % int.MaxValue));
    private readonly System.Threading.Timer _mediaChangeTimer;

    private readonly object _currentLock = new object();
    private readonly Deque<int> _previousIndexes = new();
    private readonly Deque<int> _nextIndexes = new();
    private Rope<Rope<char>> _lastObservedMedia = Rope<Rope<char>>.Empty;
    private int _currentAutomaticAdvanceCount;
    private int? _currentMediaIndex;
    private string _currentFilePath;
    private MediaType _currentMediaType;
    private Stopwatch _currentMediaDisplayedTime;
    private TimeSpan _currentMediaTimeout;
    private string _debugInfo;

    public string DebugInfo { get { lock (_currentLock) return _debugInfo; } }

    public event Action MediaChanged;

    public MediaSelector(LibVLC libVlc)
    {
      _mediaPlayerController = new MediaPlayerThreadedWrapper(libVlc);
      _mediaPlayerController.EndOfVideoReached += MediaPlayerController_EndOfVideoReached;
      _mediaFinder = new MediaFinder();
      _mediaChangeTimer = new System.Threading.Timer(MediaChangeTimerTick);
      _mediaChangeTimer.Change(100, 100);

      // get something displayed now
      Navigate(NavigationDirection.Random, NavigationActor.Automatic);
    }

    enum NavigationDirection
    {
      Forward,
      Back,
      Random,
    }

    enum NavigationActor
    {
      Manual,
      Automatic,
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

    private void Navigate(NavigationDirection navigationDirection, NavigationActor navigationActor)
    {
      lock (_currentLock)
      {
        StringBuilder debugInfo = new();
        debugInfo.Append($"{navigationDirection} {navigationActor}, ");

        var media = _mediaFinder.Media;

        if (navigationActor == NavigationActor.Automatic)
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
          switch (navigationDirection)
          {
            case NavigationDirection.Forward:
              _previousIndexes.AddToBack(_currentMediaIndex.Value);
              while (_previousIndexes.Count > 1000) _previousIndexes.RemoveFromFront();
              break;
            case NavigationDirection.Back:
              _nextIndexes.AddToBack(_currentMediaIndex.Value);
              while (_nextIndexes.Count > 1000) _nextIndexes.RemoveFromFront();
              break;
            case NavigationDirection.Random:
              _previousIndexes.AddToBack(_currentMediaIndex.Value);
              while (_previousIndexes.Count > 1000) _previousIndexes.RemoveFromFront();
              break;
          }
        }

        // decide what to view next
        if (navigationActor == NavigationActor.Automatic)
        {
          // try to have some randomness while images are being found
          if (_currentFilePath == MediaFinder.LoadingPicPath || _lastObservedMedia != media)
          {
            debugInfo.Append("loading or media count change");
            _currentMediaIndex = GetRandomIndex();
            _lastObservedMedia = media;
          }
          else if (_nextIndexes.Count > 0)
          {
            debugInfo.Append($"next index ({_nextIndexes.Count} remaining)");
            _currentMediaIndex = _nextIndexes.RemoveFromBack();
          }
          // pick a random new image every 10, so
          // 1.) reduce constant jarring changes in time period of images/videos shown
          // 2.) don't get stuck watching a hundred of young Alaric's pictures of the floor
          else if (_currentAutomaticAdvanceCount >= 10)
          {
            debugInfo.Append($"random after 10 advances");
            _currentMediaIndex = GetRandomIndex();
          }
          else if (navigationDirection == NavigationDirection.Random)
          {
            debugInfo.Append($"random");
            _currentMediaIndex = GetRandomIndex();
          }
          else if (_currentMediaIndex == null)
          {
            debugInfo.Append($"_currentMediaIndex == null");
            _currentMediaIndex = GetRandomIndex();
          }
          else
          {
            debugInfo.Append($"increment");
            _currentMediaIndex++;
          }
        }
        else if (navigationDirection == NavigationDirection.Forward)
        {
          if (_nextIndexes.Count > 0)
          {
            debugInfo.Append($"next index ({_nextIndexes.Count} remaining)");
            _currentMediaIndex = _nextIndexes.RemoveFromBack();
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
        }
        else if (navigationDirection == NavigationDirection.Back)
        {
          if (_previousIndexes.Count > 0)
          {
            debugInfo.Append($"previous index ({_previousIndexes.Count} remaining)");
            _currentMediaIndex = _previousIndexes.RemoveFromBack();
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
        }
        else // NavigationDirection == Random
        {
          debugInfo.Append($"random");
          _currentMediaIndex = GetRandomIndex();
          _nextIndexes.Clear();
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

        _currentFilePath = media[_currentMediaIndex.Value].ToString();
        _currentMediaType = MediaFinder.FilePathIsProbablyVideo(_currentFilePath) ? MediaType.Video : MediaType.Picture;

        debugInfo.AppendLine($", {_currentMediaType}");
        debugInfo.Append(_currentFilePath);

        var fileExists = File.Exists(_currentFilePath);
        if (!fileExists)
        {
          // start some async work to eliminate non-existing files
          _mediaFinder.PurgeMediaThatDoesntExist();
          debugInfo.Append(" (file does not exist!)");
        }
        
        if (_currentMediaType == MediaType.Video)
        {
          _mediaPlayerController.Play(_currentFilePath);
          _paused = false;
          _mediaPlayerController.SetPaused(false);
          _mediaPlayerController.SetMuted(_muted);

          // when navigating automatically, just play 30 seconds of video
          if (navigationActor == NavigationActor.Automatic)
          {
            _currentMediaDisplayedTime = Stopwatch.StartNew();
            _currentMediaTimeout = TimeSpan.FromSeconds(30);
            _mediaPlayerController.SeekToRandomTimeWithAtLeastThisMuchTimeLeft(_currentMediaTimeout);
          }
          else
          {
            _currentMediaDisplayedTime = null;
          }
        }
        else
        {
          _mediaPlayerController.Stop();
          _currentMediaDisplayedTime = Stopwatch.StartNew();
          _currentMediaTimeout = TimeSpan.FromSeconds(_currentFilePath == MediaFinder.LoadingPicPath ? 1 : 10);
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
      Navigate(NavigationDirection.Forward, NavigationActor.Automatic);
    }

    private void MediaChangeTimerTick(object state)
    {
      lock (_currentLock)
      {
        if (_currentMediaDisplayedTime?.Elapsed > _currentMediaTimeout && !Paused)
        {
          Navigate(NavigationDirection.Forward, NavigationActor.Automatic);
        }
      }
    }

    public void Next()
    {
      lock (_currentLock)
      {
        Navigate(NavigationDirection.Forward, NavigationActor.Manual);
      }
    }

    public void Previous()
    {
      lock (_currentLock)
      {
        Navigate(NavigationDirection.Back, NavigationActor.Manual);
      }
    }

    public void Random()
    {
      lock (_currentLock)
      {
        Navigate(NavigationDirection.Random, NavigationActor.Manual);
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
            _currentMediaDisplayedTime?.Stop();
          }
          else
          {
            _currentMediaDisplayedTime?.Start();
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
