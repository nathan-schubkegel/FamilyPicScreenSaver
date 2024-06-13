/*
This is free and unencumbered software released into the public domain under The Unlicense.
You have complete freedom to do anything you want with the software, for any purpose.
Please refer to <http://unlicense.org/>
*/

using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace FamilyPicScreenSaver
{
  public class MediaSelector
  {
    private readonly MediaFinder _mediaFinder;
    private readonly MediaPlayer _mediaPlayer;
    private readonly MediaPlayerThreadedWrapper _mediaPlayerController;
    private readonly Random _random = new Random((int)(DateTime.UtcNow.Ticks % int.MaxValue));
    private readonly System.Threading.Timer _pictureChangeTimer;

    private readonly object _currentLock = new object();
    private int _currentMediaIndex;
    private int _currentAutomaticAdvanceCount;
    private string _currentFilePath;
    private MediaType _currentMediaType;
    private Stopwatch _currentPictureDisplayedTime;
    private TimeSpan _currentPictureTimeout;

    public event Action MediaChanged;

    public MediaSelector(LibVLC libVlc)
    {
      _mediaPlayer = new MediaPlayer(libVlc);
      _mediaPlayerController = new MediaPlayerThreadedWrapper(libVlc, _mediaPlayer);
      _mediaPlayerController.EndOfVideoReached += MediaPlayerController_EndOfVideoReached;
      _mediaFinder = new MediaFinder(new[] { Settings.PictureFolder });
      _pictureChangeTimer = new System.Threading.Timer(PictureChangeTimerTick);
      _pictureChangeTimer.Change(100, 100);

      // if we're starting with a video, get the video playing now
      RespondToCurrentMediaIndexChanged();
    }

    private void RespondToCurrentMediaIndexChanged()
    {
      // this method assumes the caller has already locked
      // and has changed '_currentMediaIndex' to something fresh
      var media = _mediaFinder.Media;
      
      // pick stuff randomly if nobody's driving this thing
      if (_currentAutomaticAdvanceCount >= 10)
      {
        _currentAutomaticAdvanceCount = 0;
        var newIndex = _random.Next(media.Count + 1);
        if (newIndex == _currentMediaIndex)
        {
          _currentMediaIndex++;
        }
      }

      if (_currentMediaIndex < 0)
      {
        _currentMediaIndex = media.Count - 1;
      }
      if (_currentMediaIndex >= media.Count)
      {
        _currentMediaIndex = 0;
      }

      bool isLoadingPic = false;
      if (_currentMediaIndex < media.Count)
      {
        _currentFilePath = media[_currentMediaIndex];
        _currentMediaType = MediaFinder.FilePathIsProbablyVideo(_currentFilePath) ? MediaType.Video : MediaType.Picture;
      }
      else
      {
        _currentFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "loading.jpg");
        _currentMediaType = MediaType.Picture;
        isLoadingPic = true;
      }

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
        _currentPictureTimeout = TimeSpan.FromSeconds(isLoadingPic ? 1 : 10);
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
      videoView.MediaPlayer = _mediaPlayer;
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
      lock (_currentLock)
      {
        _currentMediaIndex++;
        _currentAutomaticAdvanceCount++;
        RespondToCurrentMediaIndexChanged();
      }
    }

    private void PictureChangeTimerTick(object state)
    {
      lock (_currentLock)
      {
        if (_currentPictureDisplayedTime?.Elapsed > _currentPictureTimeout && !Paused)
        {
          _currentMediaIndex++;
          _currentAutomaticAdvanceCount++;
          RespondToCurrentMediaIndexChanged();
        }
      }
    }

    public void Next()
    {
      lock (_currentLock)
      {
        _currentMediaIndex++;
        _currentAutomaticAdvanceCount = 0;
        RespondToCurrentMediaIndexChanged();
      }
    }

    public void Previous()
    {
      lock (_currentLock)
      {
        _currentMediaIndex--;
        _currentAutomaticAdvanceCount = 0;
        RespondToCurrentMediaIndexChanged();
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
    private bool _muted;
  }
}
