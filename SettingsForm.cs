/*
This is free and unencumbered software released into the public domain under The Unlicense.
You have complete freedom to do anything you want with the software, for any purpose.
Please refer to <http://unlicense.org/>
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Security.Permissions;

namespace FamilyPicScreenSaver
{
  public partial class SettingsForm : Form
  {
    public SettingsForm()
    {
      InitializeComponent();
      _textBox.Text = Settings.PictureFolder;
    }

    private void okButton_Click(object sender, EventArgs e)
    {
      Settings.SetPictureFolder(_textBox.Text);
      Close();
    }

    private void cancelButton_Click(object sender, EventArgs e)
    {
      Close();
    }
  }
}
