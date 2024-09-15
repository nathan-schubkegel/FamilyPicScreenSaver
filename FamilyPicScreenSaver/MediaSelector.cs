/*
This is free and unencumbered software released into the public domain under The Unlicense.
You have complete freedom to do anything you want with the software, for any purpose.
Please refer to <http://unlicense.org/>
*/

using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
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

    private readonly object _lock = new object();
    private ImmutableList<string> _mediaAtLastNavigation = ImmutableList<string>.Empty.Add(MediaFinder.LoadingPicPath);
    private ImmutableList<string> _media = ImmutableList<string>.Empty.Add(MediaFinder.LoadingPicPath);
    private readonly Deque<string> _previousFilePaths = new();
    private readonly Deque<string> _nextFilePaths = new();
    private string _currentFilePath = MediaFinder.LoadingPicPath;
    private MediaType _currentFileType = MediaType.Picture;
    private Stopwatch _currentFileDisplayedTime;
    private TimeSpan _currentFileTimeout;
    private int _currentAutomaticAdvanceCount;
    private bool _hadUserInteraction;
    private string _debugInfo = "";

    public string DebugInfo { get { lock (_lock) return _debugInfo; } }

    public event Action SelectedMediaChanged;

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

    private string GetRandomMediaFilePath()
    {
      var newIndex = _random.Next(_media.Count);

      // try to not randomly choose the same thing as current
      if (_currentFilePath == _media[newIndex])
      {
        newIndex = (newIndex + 1) % _media.Count;
      }

      return _media[newIndex];
    }

    private void Navigate(NavigationDirection navigationDirection, NavigationActor navigationActor)
    {
      lock (_lock)
      {
        StringBuilder debugInfo = new();
        debugInfo.Append($"{navigationDirection} {navigationActor}, ");

        _media = _mediaFinder.Media;

        if (navigationActor == NavigationActor.Automatic)
        {
          _currentAutomaticAdvanceCount++;
        }
        else
        {
          _currentAutomaticAdvanceCount = 0;
        }

        // save the current image/video we're looking at for back/forward functionality
        if (_currentFilePath != MediaFinder.LoadingPicPath && 
            _currentFilePath != MediaFinder.BrokenPicPath)
        {
          switch (navigationDirection)
          {
            case NavigationDirection.Forward:
            case NavigationDirection.Random:
              _previousFilePaths.AddToBack(_currentFilePath);
              while (_previousFilePaths.Count > 1000) _previousFilePaths.RemoveFromFront();
              break;
            case NavigationDirection.Back:
              _nextFilePaths.AddToBack(_currentFilePath);
              while (_nextFilePaths.Count > 1000) _nextFilePaths.RemoveFromFront();
              break;
          }
        }

        // decide what to view next
        if (navigationActor == NavigationActor.Automatic)
        {
          // try to have some randomness while images are being found
          if (_currentFilePath == MediaFinder.LoadingPicPath || _mediaAtLastNavigation != _media)
          {
            debugInfo.Append("loading or found media change");
            _currentFilePath = GetRandomMediaFilePath();
          }
          else if (_nextFilePaths.Count > 0)
          {
            debugInfo.Append($"next index ({_nextFilePaths.Count} remaining)");
            _currentFilePath = _nextFilePaths.RemoveFromBack();
          }
          else if (navigationDirection == NavigationDirection.Random)
          {
            debugInfo.Append($"random");
            _currentFilePath = GetRandomMediaFilePath();
          }
          // pick a random new image every 10, so
          // 1.) reduce constant jarring changes in time period of images/videos shown
          // 2.) don't get stuck watching a hundred of young Alaric's pictures of the floor
          else if (_currentAutomaticAdvanceCount >= 10 && !_hadUserInteraction)
          {
            debugInfo.Append($"random after 10 advances");
            _currentFilePath = GetRandomMediaFilePath();
          }
          else
          {
            debugInfo.Append($"increment");
            _currentFilePath = _media[(_media.IndexOf(_currentFilePath) + 1) % _media.Count];
          }
        }
        else if (navigationDirection == NavigationDirection.Forward)
        {
          if (_nextFilePaths.Count > 0)
          {
            debugInfo.Append($"next index ({_nextFilePaths.Count} remaining)");
            _currentFilePath = _nextFilePaths.RemoveFromBack();
          }
          else
          {
            debugInfo.Append($"increment");
            _currentFilePath = _media[(_media.IndexOf(_currentFilePath) + 1) % _media.Count];
          }
        }
        else if (navigationDirection == NavigationDirection.Back)
        {
          if (_previousFilePaths.Count > 0)
          {
            debugInfo.Append($"previous index ({_previousFilePaths.Count} remaining)");
            _currentFilePath = _previousFilePaths.RemoveFromBack();
          }
          else
          {
            debugInfo.Append($"decrement");
            var index = _media.IndexOf(_currentFilePath);
            index = index <= 0 ? _media.Count - 1 : index - 1; // decrement
            _currentFilePath = _media[index % _media.Count];
          }
        }
        else // NavigationDirection == Random
        {
          debugInfo.Append($"random");
          _currentFilePath = GetRandomMediaFilePath();
          _nextFilePaths.Clear();
        }

        _currentFileType = MediaFinder.FilePathIsProbablyVideo(_currentFilePath) 
          ? MediaType.Video : MediaType.Picture;

        debugInfo.AppendLine($", {_currentFileType}");
        debugInfo.Append(_currentFilePath);

        var fileExists = File.Exists(_currentFilePath);
        if (!fileExists)
        {
          // start some async work to eliminate non-existing files
          _mediaFinder.NotifyMediaFileDidNotExist();
          debugInfo.Append(" (file does not exist!)");
        }
        
        if (_currentFileType == MediaType.Video)
        {
          _mediaPlayerController.Play(_currentFilePath);
          _paused = false;
          _mediaPlayerController.SetPaused(false);
          _mediaPlayerController.SetMuted(_muted);

          // when navigating automatically, just play 30 seconds of video
          if (navigationActor == NavigationActor.Automatic && !_hadUserInteraction)
          {
            _currentFileDisplayedTime = Stopwatch.StartNew();
            _currentFileTimeout = TimeSpan.FromSeconds(30);
            _mediaPlayerController.SeekToRandomTimeWithAtLeastThisMuchTimeLeft(_currentFileTimeout);
          }
          else
          {
            _currentFileDisplayedTime = null;
          }
        }
        else
        {
          _mediaPlayerController.Stop();
          _currentFileDisplayedTime = Stopwatch.StartNew();
          _currentFileTimeout = TimeSpan.FromSeconds(
            _currentFilePath == MediaFinder.LoadingPicPath ? 1 : 10);
        }

        _mediaAtLastNavigation = _media;
        _debugInfo = debugInfo.ToString();
      }

      // Task so it happens while the lock isn't held
      Task.Run(() =>
      {
        try
        {
          SelectedMediaChanged?.Invoke();
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

    public (string FilePath, MediaType MediaType) GetSelectedMedia()
    {
      lock (_lock)
      {
        return (_currentFilePath, _currentFileType);
      }
    }

    private void MediaPlayerController_EndOfVideoReached()
    {
      Navigate(NavigationDirection.Forward, NavigationActor.Automatic);
    }

    private void MediaChangeTimerTick(object state)
    {
      lock (_lock)
      {
        if (_currentFileDisplayedTime?.Elapsed > _currentFileTimeout && !Paused)
        {
          Navigate(NavigationDirection.Forward, NavigationActor.Automatic);
        }
      }
    }

    public void Next()
    {
      lock (_lock)
      {
        _hadUserInteraction = true;
        Navigate(NavigationDirection.Forward, NavigationActor.Manual);
      }
    }

    public void Previous()
    {
      lock (_lock)
      {
        _hadUserInteraction = true;
        Navigate(NavigationDirection.Back, NavigationActor.Manual);
      }
    }

    public void Random()
    {
      lock (_lock)
      {
        _hadUserInteraction = true;
        Navigate(NavigationDirection.Random, NavigationActor.Manual);
      }
    }

    public bool Paused
    {
      get => _paused;
      set
      {
        lock (_lock)
        {
          _hadUserInteraction = true;
          _currentAutomaticAdvanceCount = 0;
          _paused = value;
          if (value)
          {
            _currentFileDisplayedTime?.Stop();
          }
          else
          {
            _currentFileDisplayedTime?.Start();
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
        _hadUserInteraction = true;
        lock (_lock)
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
