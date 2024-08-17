/*
This is free and unencumbered software released into the public domain under The Unlicense.
You have complete freedom to do anything you want with the software, for any purpose.
Please refer to <http://unlicense.org/>
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Rope;

namespace FamilyPicScreenSaver
{
  public class MediaFinder
  {
    public static readonly string LoadingPicPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "loading.jpg");

    public static readonly string BrokenPicPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "broken.jpg");

    private class RopeHolder
    {
      public Rope<Rope<char>> Value { get; init; }
    }
    private volatile RopeHolder _media = new RopeHolder { Value = Rope<Rope<char>>.Empty.Add(LoadingPicPath) };

    public Rope<Rope<char>> Media => _media.Value;

    public MediaFinder()
    {
      Task.Run(Scan);
    }

    private void Scan()
    {
      var fileWriteTimer = Stopwatch.StartNew();
      Rope<char> lastPath = Rope<char>.Empty;

      // pick up from where we previously left off
      HashSet<string> enumeratedDirectories = Settings.LoadEnumeratedMediaFolders();
      _media = new RopeHolder { Value = _media.Value + Settings.LoadEnumeratedMediaFiles() };

      List<string> rootDirectories = Settings.LoadMediaFolders();
      foreach (var dir in rootDirectories)
      {
        // NOTE: doing this the hard way (rather than using SearchOption.AllDirectories)
        // because when it encounters a folder it doesn't have access to, I want it to keep chugging
        Stack<IPushedAction> stack = new();
        stack.Push(new DirToEnumerate { Path = dir });
        while (stack.Count > 0)
        {
          var next = stack.Pop();
          if (next is DirToEnumerate nextDir)
          {
            if (enumeratedDirectories.Contains(nextDir.Path))
            {
              continue;
            }

            Rope<Rope<char>> newFiles = Rope<Rope<char>>.Empty;
            try
            {
              newFiles = Directory.EnumerateFiles(nextDir.Path)
                .Where(file => FilePathIsProbablyPicture(file) || FilePathIsProbablyVideo(file))
                .Select(x =>
                {
                  var result = RopeUtils.HeuristicToPerfect(lastPath, x);
                  lastPath = result;
                  return result;
                })
                .ToRope();
            }
            catch // usually it's something like "don't have access to the directory"
            {
              // ok whatever
            }

            if (!newFiles.IsEmpty)
            {
              _media = new RopeHolder
              {
                Value = _media.Value + newFiles
              };
            }

            try
            {
              string[] dirs = Directory.GetDirectories(nextDir.Path);
              stack.Push(new DirToMarkEnumerated { Path = nextDir.Path, ChildPathsToRemove = dirs.ToList() });
              foreach (var durrr in dirs)
              {
                stack.Push(new DirToEnumerate { Path = durrr });
              }
            }
            catch // usually it's something like "don't have access to the directory"
            {
              // ok whatever
            }
          }
          else if (next is DirToMarkEnumerated doneDir)
          {
            enumeratedDirectories.Add(doneDir.Path);
            doneDir.ChildPathsToRemove?.ForEach(x => enumeratedDirectories.Remove(x));

            if (fileWriteTimer.ElapsedMilliseconds > 2000)
            {
              // prevent application shutdown until these files are done being written
              Program.PreventExitDuring(() =>
              {
                Settings.SaveEnumeratedMediaFolders(enumeratedDirectories);
                Settings.SaveEnumeratedMediaFiles(_media.Value);
              });
              fileWriteTimer.Restart();
            }
          }
        }
      }

      if (_media.Value.Count == 1)
      {
        _media = new RopeHolder { Value = Rope<Rope<char>>.Empty.Add(BrokenPicPath) };
      }
      else
      {
        // remove loading pic, put last pic there (so all other indexes are unchanged
        // so forward/backward history remains largely unbroken)
        var last = _media.Value.Last();
        _media = new RopeHolder
        {
          Value = _media.Value.SetItem(0, last).RemoveAt(_media.Value.Count - 1)
        };
      }

      // prevent application shutdown until these files are done being written
      Program.PreventExitDuring(() =>
      {
        Settings.SaveEnumeratedMediaFolders(new HashSet<string>(rootDirectories));
        Settings.SaveEnumeratedMediaFiles(_media.Value);
      });
    }

    private interface IPushedAction { }

    private class DirToEnumerate : IPushedAction
    {
      public string Path;
    }

    private class DirToMarkEnumerated : IPushedAction
    {
      public string Path;
      public List<string> ChildPathsToRemove;
    }

    public static bool FilePathIsProbablyVideo(string file)
    {
      return file.EndsWith(".avi", StringComparison.OrdinalIgnoreCase) ||
        file.EndsWith(".mpg", StringComparison.OrdinalIgnoreCase) ||
        file.EndsWith(".mkv", StringComparison.OrdinalIgnoreCase) ||
        file.EndsWith(".webm", StringComparison.OrdinalIgnoreCase) ||
        file.EndsWith(".wmv", StringComparison.OrdinalIgnoreCase) ||
        file.EndsWith(".mov", StringComparison.OrdinalIgnoreCase) ||
        file.EndsWith(".3gp", StringComparison.OrdinalIgnoreCase) ||
        file.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase);
    }

    public static bool FilePathIsProbablyPicture(string file)
    {
      return file.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
        file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
        file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
        file.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) ||
        file.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase);
    }
  }
}
