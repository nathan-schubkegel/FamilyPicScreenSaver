﻿/*
This is free and unencumbered software released into the public domain under The Unlicense.
You have complete freedom to do anything you want with the software, for any purpose.
Please refer to <http://unlicense.org/>
*/

using System;
using System.Drawing;

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
      this._debugInfoLabel = new System.Windows.Forms.Label();
      ((System.ComponentModel.ISupportInitialize)(this._videoView1)).BeginInit();
      this._checkForMouseMovementTimer = new System.Windows.Forms.Timer(this.components);
      this.SuspendLayout();
      //
      // _changePictureTimer
      //
      this._checkForMouseMovementTimer.Interval = 100;
      this._checkForMouseMovementTimer.Tick += new EventHandler(CheckForMouseMovementTimerTick);
      this._checkForMouseMovementTimer.Enabled = true;
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
      // _debugInfoLabel
      // 
      this._debugInfoLabel.AutoSize = true;
      this._debugInfoLabel.Location = new System.Drawing.Point(5, 5);
      this._debugInfoLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this._debugInfoLabel.Name = "_debugInfoLabel";
      this._debugInfoLabel.Size = new System.Drawing.Size(112, 15);
      this._debugInfoLabel.TabIndex = 3;
      this._debugInfoLabel.Text = "Debug Info";
      this._debugInfoLabel.Visible = false;
      this._debugInfoLabel.Font = new System.Drawing.Font(_debugInfoLabel.Font.FontFamily, 18, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
      this._debugInfoLabel.ForeColor = Color.White;
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
      this.Controls.Add(this._debugInfoLabel);
      this._debugInfoLabel.BringToFront();
      this.Text = "Form1";
      this.Load += new System.EventHandler(this.ScreenSaverForm_Load);
      this.FormClosed += this.ScreenSaverForm_FormClosed;
      this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.ScreenSaverForm_KeyUp);
      ((System.ComponentModel.ISupportInitialize)(this._videoView1)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();
    }

    private System.Windows.Forms.Label _debugInfoLabel;
    private System.Windows.Forms.Timer _checkForMouseMovementTimer;
    private System.Windows.Forms.PictureBox _pictureBox1;
    private LibVLCSharp.WinForms.VideoView _videoView1;
  }
}
