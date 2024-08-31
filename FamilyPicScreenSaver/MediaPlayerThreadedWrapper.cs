/*
This is free and unencumbered software released into the public domain under The Unlicense.
You have complete freedom to do anything you want with the software, for any purpose.
Please refer to <http://unlicense.org/>
*/

using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;
using System;
using System.Security.Policy;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace FamilyPicScreenSaver
{
  // the guide says to avoid using the media player from its own events
  // https://github.com/videolan/libvlcsharp/blob/3.8.5/docs/best_practices.md#do-not-call-libvlc-from-a-libvlc-event-without-switching-thread-first
  // (but in Win32 it's hard to guarantee non-reentrance)
  // so this wrapper class exists to only use the media player from background threads

  public class MediaPlayerThreadedWrapper
  {
    private readonly LibVLC _libVLC;
    private readonly MediaPlayer _mediaPlayer;
    private readonly Random _random = new Random();
    private readonly Channel<Action> _commands = Channel.CreateUnbounded<Action>();
    
    public event Action EndOfVideoReached;

    public MediaPlayerThreadedWrapper(LibVLC libVLC)
    {
      _libVLC = libVLC;
      _mediaPlayer = new MediaPlayer(libVLC);
      _mediaPlayer.Playing += MediaPlayer_Playing;
      _mediaPlayer.EndReached += MediaPlayer_EndReached;
      Task.Run(HandleCommands);
    }

    private async Task HandleCommands()
    {
      while (true)
      {
        var command = await _commands.Reader.ReadAsync();
        
        try
        {
          command();
        }
        catch
        {
          // oh well
        }
      }
    }

    private void MediaPlayer_EndReached(object sender, EventArgs e)
    {
      Task.Run(() => EndOfVideoReached?.Invoke());
    }

    private void MediaPlayer_Playing(object sender, EventArgs e)
    {
      Task.Run(async () =>
      {
        // Changing the audio track and muting seem to only work reliably when the media is playing
        // so always re-apply mute when we start playing
        // and do it another 10 times because sometimes it's still not working 100% of the time on my wife's PC
        string originalMrl = null;
        for (int i = 0; i < 10; i++)
        {
          lock (_mediaPlayer)
          {
            // stop looping if the media changes
            var newMrl = _mediaPlayer.Media?.Mrl ?? "";
            if (originalMrl == null)
            {
              originalMrl = newMrl;
            }
            else if (originalMrl != newMrl)
            {
              break;
            }
            _mediaPlayer.Mute = _muted;
          }
          await Task.Delay(100);
        }
      });
    }

    public void AssociateWithVideoView(VideoView videoView)
    {
      videoView.MediaPlayer = _mediaPlayer;
    }

    public void Play(string filePath)
    {
      _commands.Writer.TryWrite(() =>
      {
        lock (_mediaPlayer)
        {
          using var media = new Media(_libVLC, new Uri(filePath));
          _mediaPlayer.Play(media);
        }
      });
    }

    public void SeekToRandomTimeWithAtLeastThisMuchTimeLeft(TimeSpan timeLeft, int retryCount = 10)
    {
      _commands.Writer.TryWrite(() =>
      {
        lock (_mediaPlayer)
        {
          var timeTotal = TimeSpan.FromMilliseconds(_mediaPlayer.Length);

          // zero seems to be the value for "the movie hasn't started yet and we don't know"
          // so delay a little and try again, at most 10 times
          if (timeTotal == TimeSpan.Zero && retryCount > 0)
          {
            Task.Delay(100).ContinueWith(t =>
              SeekToRandomTimeWithAtLeastThisMuchTimeLeft(timeLeft, retryCount - 1));
          }
          else
          {
            var viableTimes = (int)(timeTotal - timeLeft).TotalMilliseconds;
            if (viableTimes > 0)
            {
              var randoTime = TimeSpan.FromMilliseconds(_random.Next(viableTimes));
              _mediaPlayer.SeekTo(randoTime);
            }
          }
        }
      });
    }

    public void Stop()
    {
      _commands.Writer.TryWrite(() =>
      {
        lock (_mediaPlayer)
        {
          _mediaPlayer.Stop();
        }
      });
    }

    public void SetPaused(bool paused)
    {
      _commands.Writer.TryWrite(() =>
      {
        lock (_mediaPlayer)
        {
          if (_mediaPlayer.CanPause)
          {
            _mediaPlayer.SetPause(paused);
          }
        }
      });
    }

    public void SetMuted(bool muted)
    {
      _muted = muted;
      _commands.Writer.TryWrite(() =>
      {
        lock (_mediaPlayer)
        {
          _mediaPlayer.Mute = muted;
        }
      });
    }
    private volatile bool _muted;
  }
}
