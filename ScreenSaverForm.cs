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

    private Point _mouseLocation;
    private bool _previewMode = false;
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
              file.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
          {
            lock (_pictureFilePaths) _pictureFilePaths.Add(file);
          }
        }
      });
    }

    public ScreenSaverForm()
    {
      InitializeComponent();
    }

    public ScreenSaverForm(Rectangle Bounds)
    {
      InitializeComponent();
      this.Bounds = Bounds;
    }

    public ScreenSaverForm(IntPtr PreviewWndHandle)
    {
      InitializeComponent();

      // Set the preview window as the parent of this window
      SetParent(this.Handle, PreviewWndHandle);

      // Make this a child window so it will close when the parent dialog closes
      SetWindowLong(this.Handle, -16, new IntPtr(GetWindowLong(this.Handle, -16) | 0x40000000));

      // Place our window inside the parent
      Rectangle ParentRect;
      GetClientRect(PreviewWndHandle, out ParentRect);
      Size = ParentRect.Size;
      Location = new Point(0, 0);

      _previewMode = true;
    }

    private void ScreenSaverForm_Load(object sender, EventArgs e)
    {
      Cursor.Hide();
      TopMost = true;
      try { this.Focus(); } catch { }

      changePictureTimer.Interval = 10000;
      changePictureTimer.Tick += new EventHandler(changePictureTimer_Tick);
      changePictureTimer.Start();
    }
    
    private void changePictureTimer_Tick(object sender, System.EventArgs e)
    {
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
        var imageBytes = File.ReadAllBytes(myPictureFilePath);
        _image?.Dispose();
        using var imageStream = new MemoryStream(imageBytes);
        _image = Image.FromStream(imageStream);
      }
      catch
      {
        _image?.Dispose();
        _image = Image.FromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "broken.jpg"));
      }
      Invalidate(); // so it repaints
    }

    private void ScreenSaverForm_MouseMove(object sender, MouseEventArgs e)
    {
      if (!_previewMode)
      {
        if (!_mouseLocation.IsEmpty)
        {
          // Terminate if mouse is moved a significant distance
          if (Math.Abs(_mouseLocation.X - e.X) > 5 ||
              Math.Abs(_mouseLocation.Y - e.Y) > 5)
          {
            Application.Exit();
          }
        }

        // Update current mouse location
        _mouseLocation = e.Location;
      }
    }

    private void ScreenSaverForm_KeyPress(object sender, KeyPressEventArgs e)
    {
      if (!_previewMode)
      {
        Application.Exit();
      }
    }
    
    private void ScreenSaverForm_KeyDown(object sender, KeyEventArgs e)
    {
      /*
      if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down || e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
      {
          // Display a pop-up Help topic to assist the user.
          Help.ShowPopup(textBox1, "Enter your name.", new Point(textBox1.Bottom, textBox1.Right));
      }
      */
      if (!_previewMode)
      {
        Application.Exit();
      }
    }

    private void ScreenSaverForm_MouseClick(object sender, MouseEventArgs e)
    {
      if (!_previewMode)
      {
        Application.Exit();
      }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
       // If there is an _image and it has a location, 
       // paint it when the Form is repainted.
       base.OnPaint(e);
       if (_image == null)
       {
         changePictureTimer_Tick(null,null); // this should get _image populated
       }
       
       double ws = ClientRectangle.Width;
       double hs = ClientRectangle.Height;
       double wp = _image.Size.Width;
       double hp = _image.Size.Height;
       double rs = ws / hs;
       double rp = wp / hp;
       if (rs > rp)
       {
         // then sync on height and letterboxes on sides
         double s = hs / hp;
         double x = s * wp;
         double j = (ws - x) / 2;
         e.Graphics.DrawImage(_image, new Rectangle((int)j, 0, (int)(ws - 2 * j), (int)hs), 0, 0, (int)wp, (int)hp, GraphicsUnit.Pixel);
       }
       else
       {
         // then sync on width and letterboxes on top/bottom
         double s = ws / wp;
         double x = s * hp;
         //double j = hs - x / 2;
         double j = (hs - x) / 2;
         e.Graphics.DrawImage(_image, new Rectangle(0, (int)j, (int)ws, (int)(hs - 2 * j)), 0, 0, (int)wp, (int)hp, GraphicsUnit.Pixel);
       }
       /*
       float _imageRatio = _image.Size.Width / _image.Size.Height;
       float clientRatio = ClientRectangle.Width / ClientRectangle.Height;

       //if (true)
       //{
       //  var r = ClientRectangle;
       //  r.X = 100;
       //  r.Y = 20;
       //  r.Height = r.Height / 2;
      //   r.Width = r.Width / 2;
       //  e.Graphics.DrawImage(_image, r, 0, 0, _image.Size.Width, _image.Size.Height, GraphicsUnit.Pixel);
       //}
       //else 
         if (_imageRatio == clientRatio)
       {
         e.Graphics.DrawImage(_image, ClientRectangle, 0, 0, _image.Size.Width, _image.Size.Height, GraphicsUnit.Pixel);
       }
       else if (_imageRatio > clientRatio)
       {
         // TODO: _image is relatively wider than screen, so find out how much height needs to be reduced
         var clientHeight = (int)((double)ClientRectangle.Width * (double)_image.Size.Height / (double)_image.Size.Width);
         var r = ClientRectangle;
         r.Height = clientHeight;
         e.Graphics.DrawImage(_image, r, 0, 0, _image.Size.Width, _image.Size.Height, GraphicsUnit.Pixel);
       }
       else
       {
         // TODO: _image is relatively taller than screen, so find out how much width needs to be reduced
         var clientWidth = (int)((double)ClientRectangle.Height * (double)_image.Size.Width / (double)_image.Size.Height);
         var r = ClientRectangle;
         r.Width = clientWidth;
         e.Graphics.DrawImage(_image, r, 0, 0, _image.Size.Width, _image.Size.Height, GraphicsUnit.Pixel);
       }
       */
    }
  }
}
