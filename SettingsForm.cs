﻿/*
This is free and unencumbered software released into the public domain under The Unlicense.
You have complete freedom to do anything you want with the software, for any purpose.
Please refer to <http://unlicense.org/>
*/

using System;
using System.Linq;
using System.Windows.Forms;

namespace FamilyPicScreenSaver
{
  public partial class SettingsForm : Form
  {
    public SettingsForm()
    {
      InitializeComponent();
      _pictureFolderPathsTextBox.Lines = Settings.PictureFolders.ToArray();
    }

    private void okButton_Click(object sender, EventArgs e)
    {
      Settings.SetPictureFolders(_pictureFolderPathsTextBox.Lines);
      Close();
    }

    private void cancelButton_Click(object sender, EventArgs e)
    {
      Close();
    }

    private void addButton_Click(object sender, EventArgs e)
    {
      using var dialog = new FolderBrowserDialog();
      var result = dialog.ShowDialog(this);
      if (result == DialogResult.OK)
      {
        _pictureFolderPathsTextBox.Lines = _pictureFolderPathsTextBox.Lines
          .Concat(new[] { dialog.SelectedPath })
          .Where(x => !string.IsNullOrWhiteSpace(x))
          .ToArray();
      }
    }
  }
}
