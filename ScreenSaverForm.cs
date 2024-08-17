/*
This is free and unencumbered software released into the public domain under The Unlicense.
You have complete freedom to do anything you want with the software, for any purpose.
Please refer to <http://unlicense.org/>
*/

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
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

    private Point _mouseLocation;
    private bool _previewMode = false;
    private MediaSelector _mediaSelector;
    private bool _showDebugInfo;

    private ScreenSaverForm(MediaSelector mediaSelector)
    {
      _mediaSelector = mediaSelector;
      InitializeComponent();
    }
    
    public ScreenSaverForm(MediaSelector mediaSelector, Rectangle bounds) : this(mediaSelector)
    {
      Bounds = bounds;
    }

    public ScreenSaverForm(MediaSelector mediaSelector, IntPtr previewWndHandle) : this(mediaSelector)
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
      _mediaSelector.AssociateWithVideoView(_videoView1);

      if (!Debugger.IsAttached)
      {
        Cursor.Hide();
        TopMost = true;
      }

      try { Focus(); } catch { }

      _mouseLocation = GetMouseLocation();

      _mediaSelector.MediaChanged += MediaSelector_MediaChanged;
      MediaSelector_MediaChanged();
    }

    private void MediaSelector_MediaChanged()
    {
      BeginInvoke(() =>
      {
        try
        {
          var currentMedia = _mediaSelector.GetCurrentMedia();
          if (currentMedia.MediaType == MediaType.Video)
          {
            _pictureBox1.Visible = false;
            _videoView1.Visible = true;
          }
          else
          {
            _videoView1.Visible = false;
            using var oldImage = _pictureBox1.Image;
            _pictureBox1.Image = null;
            _pictureBox1.Image = Image.FromFile(currentMedia.FilePath);
            _pictureBox1.Visible = true;
          }
          _debugInfoLabel.Text = _mediaSelector.DebugInfo;
          _debugInfoLabel.Visible = _showDebugInfo;
          _debugInfoLabel.BringToFront();
        }
        catch (Exception ex)
        {
          _pictureBox1.Visible = false;
          _videoView1.Visible = false;
          _debugInfoLabel.Text = $"{ex.GetType().Name}: {ex.Message}";
          _debugInfoLabel.Visible = true;
          _debugInfoLabel.BringToFront();
        }
      });
    }

    private void ScreenSaverForm_FormClosed(object sender, FormClosedEventArgs e)
    {
      // there's no guarantee this event isn't being fired from a LibLVC event
      // https://github.com/videolan/libvlcsharp/blob/3.8.5/docs/best_practices.md#do-not-call-libvlc-from-a-libvlc-event-without-switching-thread-first
      // so avoid the risk of hanging this thread by just hard-killing the application on form close
      Program.Exit();
    }

    private void CheckForMouseMovementTimerTick(object sender, EventArgs e)
    {
      if (!_previewMode && !Debugger.IsAttached)
      {
        var m = GetMouseLocation();

        // better hope you don't have a super high res mouse...
        if (Math.Abs(_mouseLocation.X - m.X) >= 3 ||
            Math.Abs(_mouseLocation.Y - m.Y) >= 3)
        {
          // there's no guarantee this event isn't being fired from a LibLVC event
          // https://github.com/videolan/libvlcsharp/blob/3.8.5/docs/best_practices.md#do-not-call-libvlc-from-a-libvlc-event-without-switching-thread-first
          // so avoid the risk of hanging this thread by just hard-killing the application on form close
          Program.Exit();
        }

        _mouseLocation = m;
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
        _mediaSelector.Next();
      }
      else if (e.KeyCode == Keys.Left)
      {
        _mediaSelector.Previous();
      }
      else if (e.KeyCode == Keys.Up)
      {
        _mediaSelector.Random();
      }
      else if (e.KeyCode == Keys.M)
      {
        _mediaSelector.Muted = !_mediaSelector.Muted;
      }
      else if (e.KeyCode == Keys.D)
      {
        _showDebugInfo = !_showDebugInfo;
        _debugInfoLabel.BringToFront();
        _debugInfoLabel.Visible = _showDebugInfo;
      }
      else if (e.KeyCode == Keys.O || e.KeyCode == Keys.F)
      {
        using (Process.Start("explorer.exe", "/select, \"" + _mediaSelector.GetCurrentMedia().FilePath + "\"")) { }

        // there's no guarantee this event isn't being fired from a LibLVC event
        // https://github.com/videolan/libvlcsharp/blob/3.8.5/docs/best_practices.md#do-not-call-libvlc-from-a-libvlc-event-without-switching-thread-first
        // so avoid the risk of hanging this thread by just hard-killing the application on form close
        Program.Exit();
      }
      else if (e.KeyCode == Keys.Space)
      {
        _mediaSelector.Paused = !_mediaSelector.Paused;
      }
      else if (!_previewMode && !Debugger.IsAttached)
      {
        // there's no guarantee this event isn't being fired from a LibLVC event
        // https://github.com/videolan/libvlcsharp/blob/3.8.5/docs/best_practices.md#do-not-call-libvlc-from-a-libvlc-event-without-switching-thread-first
        // so avoid the risk of hanging this thread by just hard-killing the application on form close
        Program.Exit();
      }
    }
  }
}
