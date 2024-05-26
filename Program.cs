/*
This is free and unencumbered software released into the public domain under The Unlicense.
You have complete freedom to do anything you want with the software, for any purpose.
Please refer to <http://unlicense.org/>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Globalization;

namespace FamilyPicScreenSaver
{
  static class Program
  {
    static void Main(string[] args)
    {
      try
      {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        var arg1 = (args.FirstOrDefault() ?? "");
        if (arg1.StartsWith("/p", StringComparison.OrdinalIgnoreCase))
        {
          string handleText = arg1.StartsWith("/p:", StringComparison.OrdinalIgnoreCase) ? arg1.Substring("/p:".Length) : args.Skip(1).FirstOrDefault();
          IntPtr previewWndHandle = long.TryParse(handleText, CultureInfo.InvariantCulture, out var handleLong) ? new IntPtr(handleLong) :
            ulong.TryParse(handleText, CultureInfo.InvariantCulture, out var handleUlong) ? new IntPtr((long)handleUlong) :
            throw new ArgumentException("Invalid or missing window handle argument");

          Application.Run(new ScreenSaverForm(previewWndHandle));
        }
        else if (arg1.StartsWith("/s", StringComparison.OrdinalIgnoreCase))
        {
          ShowScreenSaverOnAllMonitors();
          Application.Run();
        }  
        else
        {
          // technically there's a "/c" argument that drives this behavior
          // but it's a fine behavior for when people double-click the application
          Application.Run(new SettingsForm());
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show($"Error in FamilyPicScreenSaver: {ex}");
      }
    }

    static void ShowScreenSaverOnAllMonitors()
    {
      foreach (Screen screen in Screen.AllScreens)
      {
        new ScreenSaverForm(screen.Bounds).Show();
      }           
    }
  }
}
