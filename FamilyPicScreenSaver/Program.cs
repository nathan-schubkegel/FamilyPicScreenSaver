/*
This is free and unencumbered software released into the public domain under The Unlicense.
You have complete freedom to do anything you want with the software, for any purpose.
Please refer to <http://unlicense.org/>
*/

using System;
using System.Linq;
using System.Windows.Forms;
using System.Globalization;
using LibVLCSharp.Shared;

namespace FamilyPicScreenSaver
{
  static class Program
  {
    private static readonly object _exitPreventionLock = new();
    private static int _exitPreventionCount;
    private static bool _exitDeferred;

    public static void Exit()
    {
      lock (_exitPreventionLock)
      {
        if (_exitPreventionCount > 0)
        {
          _exitDeferred = true;
          return;
        }
        Environment.Exit(0);
      }
    }

    public static void PreventExitDuring(Action a)
    {
      lock (_exitPreventionLock)
      {
        _exitPreventionCount++;
        try
        {
          a();
        }
        finally
        {
          _exitPreventionCount--;
          if (_exitDeferred)
          {
            Environment.Exit(0);
          }
        }
      }
    }

    [STAThread]
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

          var libVLC = new LibVLC(); // try to only create one of these per application
          var mediaSelector = new MediaSelector(libVLC); 
          Application.Run(new ScreenSaverForm(mediaSelector, previewWndHandle));
        }
        else if (arg1.StartsWith("/s", StringComparison.OrdinalIgnoreCase))
        {
          var libVLC = new LibVLC(); // try to only create one of these per application
          var mediaSelector = new MediaSelector(libVLC);
          foreach (Screen screen in Screen.AllScreens)
          {
            new ScreenSaverForm(mediaSelector, screen.Bounds).Show();
          }
          Application.Run();
        }  
        else
        {
          // technically there's a "/c" argument that drives this behavior
          // but it's also a fine behavior for when people double-click the application
          Application.Run(new SettingsForm());
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show($"Error in FamilyPicScreenSaver: {ex}");
      }
      finally
      {
        Exit();
      }
    }
  }
}
