/*
This is free and unencumbered software released into the public domain under The Unlicense.
You have complete freedom to do anything you want with the software, for any purpose.
Please refer to <http://unlicense.org/>
*/

using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;
using System;
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
      Task.Run(() =>
      {
        // Changing the audio track seems to only work when the media is playing,
        // and we WERE doing that for mute (but now we're not... it didn't reliably work),
        // so just always re-apply mute when we start playing
        lock (_mediaPlayer)
        {
          _mediaPlayer.Mute = _muted;
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
