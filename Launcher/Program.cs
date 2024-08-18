/*
This is free and unencumbered software released into the public domain under The Unlicense.
You have complete freedom to do anything you want with the software, for any purpose.
Please refer to <http://unlicense.org/>
*/

using System;
using System.Windows.Forms;
using System.Diagnostics;

namespace Launcher
{
  static class Program
  {
    [STAThread]
    static int Main(string[] args)
    {
      try
      {
        var proc = Process.Start(@"C:\Program Files\FamilyPicScreenSaver\FamilyPicScreenSaver.exe",
          string.Join(" ", args));
        proc.WaitForExit();
        return proc.ExitCode;
      }
      catch (Exception ex)
      {
        MessageBox.Show($"Error launching FamilyPicScreenSaver: {ex}");
        return -1;
      }
    }
  }
}
