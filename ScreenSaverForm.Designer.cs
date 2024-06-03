/*
This is free and unencumbered software released into the public domain under The Unlicense.
You have complete freedom to do anything you want with the software, for any purpose.
Please refer to <http://unlicense.org/>
*/

namespace FamilyPicScreenSaver
{
  partial class ScreenSaverForm
  {
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      this._videoView1 = new LibVLCSharp.WinForms.VideoView();
      this._pictureBox1 = new System.Windows.Forms.PictureBox();
      ((System.ComponentModel.ISupportInitialize)(this._videoView1)).BeginInit();
      this._changePictureTimer = new System.Windows.Forms.Timer(this.components);
      this.SuspendLayout();
      // 
      // _videoView1
      // 
      this._videoView1.BackColor = System.Drawing.Color.Black;
      this._videoView1.Dock = System.Windows.Forms.DockStyle.Fill;
      this._videoView1.Location = new System.Drawing.Point(0, 0);
      this._videoView1.Name = "_videoView1";
      this._videoView1.TabIndex = 0;
      this._videoView1.Visible = false;
      //
      // _pictureBox1
      //
      this._pictureBox1.BackColor = System.Drawing.Color.Black;
      this._pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
      this._pictureBox1.Location = new System.Drawing.Point(0, 0);
      this._pictureBox1.Name = "_pictureBox1";
      this._pictureBox1.TabIndex = 0;
      this._pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this._pictureBox1.Visible = false;
      //
      // ScreenSaverForm
      //
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.Black;
      this.ClientSize = new System.Drawing.Size(508, 260);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
      this.Name = "ScreenSaverForm";
      this.KeyPreview = true;
      this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
      this.Controls.Add(this._videoView1);
      this.Controls.Add(this._pictureBox1);
      this.Text = "Form1";
      this.Load += new System.EventHandler(this.ScreenSaverForm_Load);
      this.FormClosed += this.ScreenSaverForm_FormClosed;
      this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.ScreenSaverForm_KeyUp);
      ((System.ComponentModel.ISupportInitialize)(this._videoView1)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();
    }

    private System.Windows.Forms.Timer _changePictureTimer;
    private System.Windows.Forms.PictureBox _pictureBox1;
    private LibVLCSharp.WinForms.VideoView _videoView1;
  }
}
