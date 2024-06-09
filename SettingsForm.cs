/*
This is free and unencumbered software released into the public domain under The Unlicense.
You have complete freedom to do anything you want with the software, for any purpose.
Please refer to <http://unlicense.org/>
*/

using System;
using System.Windows.Forms;

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

    private void browseButton_Click(object sender, EventArgs e)
    {
      using var dialog = new FolderBrowserDialog();
      try
      {
        dialog.SelectedPath = _textBox.Text;
      }
      catch
      {
        // oh well
      }
      var result = dialog.ShowDialog(this);
      if (result == DialogResult.OK)
      {
        _textBox.Text = dialog.SelectedPath;
      }
    }
  }
}
