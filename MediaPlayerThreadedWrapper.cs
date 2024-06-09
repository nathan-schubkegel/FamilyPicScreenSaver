/*
This is free and unencumbered software released into the public domain under The Unlicense.
You have complete freedom to do anything you want with the software, for any purpose.
Please refer to <http://unlicense.org/>
*/

using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;
using System;
using System.Threading.Tasks;

namespace FamilyPicScreenSaver
{
  public class MediaPlayerThreadedWrapper
  {
    private readonly LibVLC _libVLC;
    private readonly MediaPlayer _mediaPlayer;
    private volatile bool _isPlaying;
    private volatile bool _isTweeningPlaying;

    public bool IsPlaying => _isTweeningPlaying || _isPlaying;

    public MediaPlayerThreadedWrapper(LibVLC libVLC, MediaPlayer mediaPlayer)
    {
      // the guide says to avoid using the media player from its own events
      // https://github.com/videolan/libvlcsharp/blob/3.8.5/docs/best_practices.md#do-not-call-libvlc-from-a-libvlc-event-without-switching-thread-first
      // (but in Win32 it's hard to guarantee non-reentrance)
      // so this wrapper class only uses the media player from background threads
      _libVLC = libVLC;
      _mediaPlayer = mediaPlayer;
      _mediaPlayer.Playing += MediaPlayer_Playing;
      _mediaPlayer.Stopped += MediaPlayer_Stopped;
    }

    private void MediaPlayer_Stopped(object sender, EventArgs e)
    {
      _isPlaying = false;
      _isTweeningPlaying = false;
    }

    private void MediaPlayer_Playing(object sender, EventArgs e)
    {
      _isPlaying = true;
    }

    public void AssociateWithControl(VideoView videoView)
    {
      videoView.MediaPlayer = _mediaPlayer;
    }

    public void Play(string filePath)
    {
      _isTweeningPlaying = true;
      Task.Run(() =>
      {
        try
        {
          lock (_mediaPlayer)
          {
            using var media = new Media(_libVLC, new Uri(filePath));
            _mediaPlayer.Play(media);
            // NOTE: this returns false when it attempts to play a bogus file
            // but I observe the Playing and Stopped events still fire in that scenario (great!)
          }
        }
        catch
        {
          _isTweeningPlaying = false;
        }
      });
    }

    public void Stop()
    {
      Task.Run(() =>
      {
        try
        {
          lock (_mediaPlayer)
          {
            _mediaPlayer.Stop();
          }
        }
        catch
        {
          _isPlaying = false;
        }
      });
    }
  }
}
