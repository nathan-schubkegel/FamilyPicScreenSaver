/*
This is free and unencumbered software released into the public domain under The Unlicense.
You have complete freedom to do anything you want with the software, for any purpose.
Please refer to <http://unlicense.org/>
*/

using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Launcher
{
  static class Program
  {
    [STAThread]
    static int Main(string[] args)
    {
      try
      {
        var launchCommand = File.ReadAllLines(Path.Combine(
          AppDomain.CurrentDomain.BaseDirectory, 
          "FamilyPicScreenSaverLaunchCommand.txt"
        )).First();

        var proc = Process.Start(launchCommand);
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
