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
        var iniFilePath = Path.Combine(
          Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
          "Family Pic Screen Saver",
          "LauncherSettings.ini");

        var lines = File.ReadAllLines(iniFilePath);
        var launchFilePath = lines
          .First(x => x.StartsWith("LaunchFilePath="))
          .Substring("LaunchFilePath=".Length);

        var argumentsLine = string.Join(" ", args.Select(
          x => $"\"{x.Replace("\\", "\\\\").Replace("\"", "\\\"")}\""));
        var proc = Process.Start(launchFilePath, argumentsLine);
        proc.WaitForExit();
        return proc.ExitCode;
      }
      catch (Exception ex)
      {
        // make sure we don't show the messagebox for all time
        var timer = new System.Windows.Forms.Timer();
        timer.Interval = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;
        timer.Tick += (o, s) => Environment.Exit(1);
        timer.Start();

        MessageBox.Show($"Error launching FamilyPicScreenSaver: {ex}");
        return 1;
      }
    }
  }
}
