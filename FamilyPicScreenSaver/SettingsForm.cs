/*
This is free and unencumbered software released into the public domain under The Unlicense.
You have complete freedom to do anything you want with the software, for any purpose.
Please refer to <http://unlicense.org/>
*/

using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace FamilyPicScreenSaver
{
  public partial class SettingsForm : Form
  {
    public SettingsForm()
    {
      InitializeComponent();
      _pictureFolderPathsTextBox.Lines = Settings.LoadMediaFolders().ToArray();
    }

    private void okButton_Click(object sender, EventArgs e)
    {
      Settings.SaveMediaFolders(_pictureFolderPathsTextBox.Lines.ToList());
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

    private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      try
      {
        using var _ = Process.Start(new ProcessStartInfo(((LinkLabel)sender).Text)
        {
          UseShellExecute = true,
        });
      }
      catch
      {
        MessageBox.Show("Unable to launch browser window... I guess you can copy paste the link text manually eh?");
      }
    }

    private void SettingsForm_Load(object sender, EventArgs e)
    {
      Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
    }
  }
}
