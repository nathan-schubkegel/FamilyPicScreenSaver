/*
This is free and unencumbered software released into the public domain under The Unlicense.
You have complete freedom to do anything you want with the software, for any purpose.
Please refer to <http://unlicense.org/>
*/

using System;
using System.Collections.Generic;
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
      Rope<Rope<char>> initialMedia = Settings.LoadEnumeratedMediaFiles();
      TimeSpan? initialMediaAge = Settings.LoadAgeOfEnumeratedMediaFiles();
      List<string> initialEnumeratedMediaFolders = Settings.LoadEnumeratedMediaFolders();
      List<string> rootDirectories = Settings.LoadMediaFolders();

      // does it look like we don't need to rescan?
      if (rootDirectories.SequenceEqual(initialEnumeratedMediaFolders) &&
        !initialMedia.IsEmpty &&
        initialMediaAge < TimeSpan.FromDays(5))
      {
        // ok this is good
        _media = new RopeHolder { Value = initialMedia };
      }
      else // re-scan
      {
        var filesGroupedByFolder = Rope<Rope<Rope<char>>>.Empty;
        Rope<char> lastPath = Rope<char>.Empty;
        foreach (var rootDir in rootDirectories)
        {
          // NOTE: doing this the hard way (rather than using SearchOption.AllDirectories)
          // because when it encounters a folder it doesn't have access to, I want it to keep chugging
          Stack<string> stack = new();
          stack.Push(rootDir);
          while (stack.Count > 0)
          {
            var next = stack.Pop();
            var newFiles = Rope<Rope<char>>.Empty;
            try
            {
              newFiles = Directory.EnumerateFiles(next)
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
              filesGroupedByFolder += newFiles;
              _media = new RopeHolder { Value = _media.Value + newFiles };
            }

            try
            {
              string[] dirs = Directory.GetDirectories(next);
              foreach (var durrr in dirs)
              {
                stack.Push(durrr);
              }
            }
            catch // usually it's something like "don't have access to the directory"
            {
              // ok whatever
            }
          }
        }

        if (filesGroupedByFolder.IsEmpty)
        {
          _media = new RopeHolder { Value = Rope<Rope<char>>.Empty.Add(BrokenPicPath) };
        }
        else
        {
          // randomize by folder
          var randomized = RopeUtils.Randomize(filesGroupedByFolder);
          _media = new RopeHolder { Value = RopeUtils.SelectMany(randomized) };
        }

        // prevent application shutdown until these files are done being written
        Program.PreventExitDuring(() =>
        {
          Settings.SaveEnumeratedMediaFolders(rootDirectories);
          Settings.SaveEnumeratedMediaFiles(_media.Value);
        });
      }
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
