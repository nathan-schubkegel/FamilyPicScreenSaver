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
      _mediaPlayer.EndReached += MediaPlayer_EndReached;
      _mediaPlayer.Volume = 0; // start muted
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
      _commands.Writer.TryWrite(() =>
      {
        lock (_mediaPlayer)
        {
          _mediaPlayer.Volume = muted ? 0 : 100;
        }
      });
    }
  }
}
