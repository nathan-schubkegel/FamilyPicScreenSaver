/*
This is free and unencumbered software released into the public domain under The Unlicense.
You have complete freedom to do anything you want with the software, for any purpose.
Please refer to <http://unlicense.org/>
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using LibVLCSharp.WinForms;
using LibVLCSharp;
using LibVLCSharp.Shared;


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

    System.Diagnostics.Stopwatch _pictureStopwatch = System.Diagnostics.Stopwatch.StartNew();
    private Point _mouseLocation;
    private bool _previewMode = false;
    private bool _forceNext = false;
    private Random _random = new Random();

    private static List<string> _pictureFilePaths = new List<string>();
    private Image _image;

    static ScreenSaverForm()
    {
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
        string myPictureFolder = Settings.PictureFolder;
        var files = System.IO.Directory.EnumerateFiles(myPictureFolder, "*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
          if (Settings.PictureFolder != myPictureFolder)
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
      Cursor.Hide();
      TopMost = true;
      try { this.Focus(); } catch { }

      _mouseLocation = GetMouseLocation();
      _changePictureTimer.Interval = 100;
      _changePictureTimer.Tick += new EventHandler(changePictureTimer_Tick);
      _changePictureTimer.Start();
    }
    
    private void ScreenSaverForm_FormClosed(object sender, FormClosedEventArgs e)
    {
      _mp.Stop();
      _mp.Dispose();
      _libVLC.Dispose();
    }

    private void changePictureTimer_Tick(object sender, System.EventArgs e)
    {
      if (!_previewMode)
      {
        QuitIfMouseMoved();
      }
      
      // decide whether to keep letting the currently-displayed thing be displayed
      if (!_forceNext)
      {
        if (_videoView1.Visible) // a video is showing
        {
          if (_mp.IsPlaying)
          {
            return;
          }
          // else done playing, so load something else
        }
        else if (_image != null) // a picture is showing
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
        }
        else
        {
          var nextIndex = _random.Next(0, _pictureFilePaths.Count);
          myPictureFilePath = _pictureFilePaths[nextIndex];
        }
      }

      try
      {
        if (FilePathIsProbablyVideo(myPictureFilePath))
        {
          _videoView1.Visible = true;
          using var media = new Media(_libVLC, new Uri(myPictureFilePath)); //new Uri("http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4"));
          if (!_mp.Play(media)) // Returns true if the playback will start successfully
          {
            throw new Exception("womp");
          }
        
          _pictureBox1.Visible = false;
          _pictureBox1.Image = null;
          _image?.Dispose();
          _image = null;
        }
        else
        {
          _pictureBox1.Image = null;
          _pictureBox1.Visible = true;
          _mp.Stop();
          _videoView1.Visible = false;
          _image?.Dispose();
          _image = Image.FromFile(myPictureFilePath);
          _pictureBox1.Image = _image;
          _pictureStopwatch.Restart();
        }
      }
      catch
      {
        _pictureBox1.Image = null;
        _pictureBox1.Visible = true;
        _mp.Stop();
        _videoView1.Visible = false;
        _image?.Dispose();
        _image = Image.FromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "broken.jpg"));
        _pictureBox1.Image = _image;
        _pictureStopwatch.Restart();
      }
    }

    private void QuitIfMouseMoved()
    {
      var e = GetMouseLocation();

      // better hope you don't have a super high res mouse...
      if (Math.Abs(_mouseLocation.X - e.X) >= 3 ||
          Math.Abs(_mouseLocation.Y - e.Y) >= 3)
      {
        Environment.Exit(0);
      }

      _mouseLocation = e;
    }

    private void ScreenSaverForm_KeyPress(object sender, KeyPressEventArgs e)
    {
      if (!_previewMode)
      {
        Environment.Exit(0);
      }
    }
    
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
        Environment.Exit(0);
      }
    }
  }
}
