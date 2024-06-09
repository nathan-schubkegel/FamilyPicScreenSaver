/*
This is free and unencumbered software released into the public domain under The Unlicense.
You have complete freedom to do anything you want with the software, for any purpose.
Please refer to <http://unlicense.org/>
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibVLCSharp.Shared;
using System.Diagnostics;

namespace FamilyPicScreenSaver
{
  public partial class ScreenSaverForm : Form
  {
    [DllImport("user32.dll")]
    static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

    [DllImport("user32.dll")]
    static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    static extern bool GetClientRect(IntPtr hWnd, out Rectangle lpRect);
    
    [DllImport("user32.dll")]
    static extern bool GetCursorPos(ref Point lpPoint);
    
    private static Point GetMouseLocation()
    {
      var point = new Point();
      GetCursorPos(ref point);
      return point;
    }

    private static LibVLC _libVLC;
    private static List<string> _pictureFilePaths = new List<string>();

    private Random _random = new Random();
    private MediaPlayerThreadedWrapper _mp;
    private Stopwatch _pictureStopwatch = Stopwatch.StartNew();
    private Point _mouseLocation;
    private bool _previewMode = false;
    private bool _forceNext = true;

    static ScreenSaverForm()
    {
      _libVLC = new LibVLC();
      RelearnPictures();
    }
    
    static void RelearnPictures()
    {
      // start a thread to learn the picture file paths
      Task.Run(() => 
      {
        lock (_pictureFilePaths) 
        {
          _pictureFilePaths.Clear();
        }
        string rootPictureFolder = Settings.PictureFolder;

        // NOTE: doing this the hard way (rather than using SearchOption.AllDirectories)
        // because when it encounters a folder it doesn't have access to, I want it to keep chugging
        Stack<string> pictureFoldersToSearch = new();
        pictureFoldersToSearch.Push(rootPictureFolder);
        while (pictureFoldersToSearch.Count > 0)
        {
          var myPictureFolder = pictureFoldersToSearch.Pop();
          try
          {
            var files = System.IO.Directory.EnumerateFiles(myPictureFolder, "*");
            foreach (var file in files)
            {
              // stop when setting changes
              if (Settings.PictureFolder != rootPictureFolder)
              {
                return;
              }

              if (file.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                  file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                  file.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase) ||
                  FilePathIsProbablyVideo(file))
              {
                lock (_pictureFilePaths) _pictureFilePaths.Add(file);
              }
            }
          }
          catch // usually it's something like "don't have access to the directory"
          {
            // ok whatever
          }

          try
          {
            var dirs = System.IO.Directory.EnumerateDirectories(myPictureFolder, "*");
            foreach (var dir in dirs)
            {
              // stop when setting changes
              if (Settings.PictureFolder != rootPictureFolder)
              {
                return;
              }
              pictureFoldersToSearch.Push(dir);
            }
          }
          catch // usually it's something like "don't have access to the directory"
          {
            // ok whatever
          }
        }
      });
    }
    
    private static bool FilePathIsProbablyVideo(string file)
    {
      return  file.EndsWith(".avi", StringComparison.OrdinalIgnoreCase) ||
              file.EndsWith(".mpg", StringComparison.OrdinalIgnoreCase) ||
              file.EndsWith(".mkv", StringComparison.OrdinalIgnoreCase) ||
              file.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase);
    }

    public ScreenSaverForm()
    {
      _mp = new MediaPlayerThreadedWrapper(_libVLC, new MediaPlayer(_libVLC));
      InitializeComponent();
    }

    public ScreenSaverForm(Rectangle bounds) : this()
    {
      this.Bounds = bounds;
    }

    public ScreenSaverForm(IntPtr previewWndHandle) : this()
    {
      // Set the preview window as the parent of this window
      SetParent(this.Handle, previewWndHandle);

      // Make this a child window so it will close when the parent dialog closes
      SetWindowLong(this.Handle, -16, new IntPtr(GetWindowLong(this.Handle, -16) | 0x40000000));

      // Place our window inside the parent
      Rectangle ParentRect;
      GetClientRect(previewWndHandle, out ParentRect);
      Size = ParentRect.Size;
      Location = new Point(0, 0);

      _previewMode = true;
    }

    private void ScreenSaverForm_Load(object sender, EventArgs e)
    {
      _videoView1.MediaPlayer = _mp.MediaPlayer;

      if (!Debugger.IsAttached)
      {
        Cursor.Hide();
        TopMost = true;
      }

      try { this.Focus(); } catch { }

      _mouseLocation = GetMouseLocation();
      _changePictureTimer.Interval = 100;
      _changePictureTimer.Tick += new EventHandler(changePictureTimer_Tick);
      _changePictureTimer.Start();
      changePictureTimer_Tick(null, null);
    }
    
    private void ScreenSaverForm_FormClosed(object sender, FormClosedEventArgs e)
    {
      // there's no guarantee this event isn't being fired from a LibLVC event
      // https://github.com/videolan/libvlcsharp/blob/3.8.5/docs/best_practices.md#do-not-call-libvlc-from-a-libvlc-event-without-switching-thread-first
      // so avoid the risk of hanging this thread by just hard-killing the application on form close
      Environment.Exit(0);
    }

    private void changePictureTimer_Tick(object sender, EventArgs e)
    {
      if (!_previewMode)
      {
        QuitIfMouseMoved();
      }

      // decide whether to keep letting the currently-displayed thing be displayed
      if (!_forceNext)
      {
        if (_videoView1.Visible)
        {
          if (_mp.IsPlaying)
          {
            return;
          }
          // else done playing, so load something else
        }
        else // I last set it up to show a picture, or nothing has been shown yet
        {
          if (_pictureStopwatch.ElapsedMilliseconds < 10000)
          {
            return;
          }
          // else that image has displayed long enough, so load something else
        }
      }
      _forceNext = false;

      string myPictureFilePath;
      lock (_pictureFilePaths)
      {
        if (_pictureFilePaths.Count == 0)
        {
          myPictureFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "loading.jpg");
          _pictureStopwatch.Reset(); // so it stays alive as long as needed
        }
        else
        {
          var nextIndex = _random.Next(0, _pictureFilePaths.Count);
          while (nextIndex == _lastIndex && _pictureFilePaths.Count > 1)
          {
            nextIndex = _random.Next(0, _pictureFilePaths.Count);
          }
          myPictureFilePath = _pictureFilePaths[nextIndex];
          _lastIndex = nextIndex;
        }
      }

      try
      {
        if (FilePathIsProbablyVideo(myPictureFilePath))
        {
          _pictureBox1.Visible = false;
          _videoView1.Visible = true;
          _mp.Play(myPictureFilePath);
        }
        else
        {
          _videoView1.Visible = false;
          _mp.Stop();
          using var oldImage = _pictureBox1.Image;
          _pictureBox1.Image = null;
          _pictureBox1.Image = Image.FromFile(myPictureFilePath);
          _pictureBox1.Visible = true;
          _pictureStopwatch.Restart();
        }
      }
      catch
      {
        _pictureBox1.Visible = false;
        _videoView1.Visible = false;
        _mp.Stop();
      }
    }

    private void QuitIfMouseMoved()
    {
      var e = GetMouseLocation();

      // better hope you don't have a super high res mouse...
      if (Math.Abs(_mouseLocation.X - e.X) >= 3 ||
          Math.Abs(_mouseLocation.Y - e.Y) >= 3)
      {
        // there's no guarantee this event isn't being fired from a LibLVC event
        // https://github.com/videolan/libvlcsharp/blob/3.8.5/docs/best_practices.md#do-not-call-libvlc-from-a-libvlc-event-without-switching-thread-first
        // so avoid the risk of hanging this thread by just hard-killing the application on form close
        Environment.Exit(0);
      }

      _mouseLocation = e;
    }

    private void ScreenSaverForm_KeyPress(object sender, KeyPressEventArgs e)
    {
      if (!_previewMode)
      {
        // there's no guarantee this event isn't being fired from a LibLVC event
        // https://github.com/videolan/libvlcsharp/blob/3.8.5/docs/best_practices.md#do-not-call-libvlc-from-a-libvlc-event-without-switching-thread-first
        // so avoid the risk of hanging this thread by just hard-killing the application on form close
        Environment.Exit(0);
      }
    }
    
    // NOTE: we're subscribing to KeyUp rather than KeyDown
    // because many controls consume KeyDown of arrow keys (and a few other input keys)
    // without forwarding them to the parent control to respect how this form has KeyPreview = true.
    // (And we're depending on KeyPreview because key events in the native window
    //  otherwise go into a black hole)
    private void ScreenSaverForm_KeyUp(object sender, KeyEventArgs e)
    {
      //if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down || e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
      if (e.KeyCode == Keys.Right)
      {
        _forceNext = true;
        changePictureTimer_Tick(null, null);
      }
      else if (!_previewMode)
      {
        // there's no guarantee this event isn't being fired from a LibLVC event
        // https://github.com/videolan/libvlcsharp/blob/3.8.5/docs/best_practices.md#do-not-call-libvlc-from-a-libvlc-event-without-switching-thread-first
        // so avoid the risk of hanging this thread by just hard-killing the application on form close
        Environment.Exit(0);
      }
    }
  }
}
