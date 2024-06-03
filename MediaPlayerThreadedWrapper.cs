using LibVLCSharp.Shared;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace FamilyPicScreenSaver
{
  public class MediaPlayerThreadedWrapper
  {
    private interface ICommand
    {
    }

    public class PlayCommand : ICommand
    {
      public string FilePath;
    }

    public class StopCommand : ICommand
    {

    }

    private readonly LibVLC _libVLC;
    private readonly MediaPlayer _mediaPlayer;
    private ConcurrentQueue<ICommand> _commandQueue = new();
    private Task _loop;
    private int _countOfToldToPlay; // outsiders increment and read this, Loop() method decrements it
    private volatile bool _isPlaying; // outsiders read this, Loop() assigns it

    // needed so it can be assocaited with the VideoView control
    public MediaPlayer MediaPlayer => _mediaPlayer;

    public bool IsPlaying => _isPlaying || Interlocked.CompareExchange(ref _countOfToldToPlay, 0, 0) > 0;

    public MediaPlayerThreadedWrapper(LibVLC libVLC, MediaPlayer mediaPlayer)
    {
      // the guide says to avoid using the media player from its own events
      // https://github.com/videolan/libvlcsharp/blob/3.8.5/docs/best_practices.md#do-not-call-libvlc-from-a-libvlc-event-without-switching-thread-first
      // (but in Win32 it's hard to guarantee non-reentrance)
      // so this wrapper class only uses the media player from background threads
      _libVLC = libVLC;
      _mediaPlayer = mediaPlayer;
    }

    public void Play(string filePath)
    {
      if (_loop == null)
      {
        _loop = Task.Run(Loop);
      }

      Interlocked.Increment(ref _countOfToldToPlay);
      _commandQueue.Enqueue(new PlayCommand { FilePath = filePath });
    }

    public void Stop()
    {
      if (_loop == null)
      {
        _loop = Task.Run(Loop);
      }

      _commandQueue.Enqueue(new StopCommand());
    }

    private void Loop()
    {
      while (true)
      {
        if (_commandQueue.TryDequeue(out var command))
        {
          switch (command)
          {
            case PlayCommand playCommand:
              try
              {
                using var media = new Media(_libVLC, new Uri(playCommand.FilePath));
                _mediaPlayer.Play(media);

                // gotta have this here earlier than decrement _countOfToldToPlay
                // or the public 'IsPlaying' property will be a lie sometimes
                _isPlaying = _mediaPlayer.IsPlaying;
              }
              catch
              {
                // oh well
              }
              finally
              {
                Interlocked.Decrement(ref _countOfToldToPlay);
              }
              break;

            case StopCommand:
              try
              {
                _mediaPlayer.Stop();
              }
              catch
              {
                // oh well
              }
              break;
          }
        }
        else
        {
          Thread.Sleep(100);
        }

        try
        {
          _isPlaying = _mediaPlayer.IsPlaying;
        }
        catch
        {
          // oh well
        }
      }
    }
  }
}
