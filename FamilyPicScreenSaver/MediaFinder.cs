/*
This is free and unencumbered software released into the public domain under The Unlicense.
You have complete freedom to do anything you want with the software, for any purpose.
Please refer to <http://unlicense.org/>
*/

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FamilyPicScreenSaver.Lib;

namespace FamilyPicScreenSaver
{
  public class MediaFinder
  {
    public static readonly string LoadingPicPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "loading.jpg");

    public static readonly string BrokenPicPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "broken.jpg");

    private volatile ImmutableList<string> _media = null;

    public ImmutableList<string> Media
    {
      get
      {
        var media = _media;
        if (media == null) // null means the scan hasn't recorded any yet
        {
          media = [MediaFinder.LoadingPicPath];
        }
        else if (media.Count == 0) // zero means the scan found no files
        {
          media = [MediaFinder.BrokenPicPath];
        }
        return media;
      }
    }

    private readonly object _purgeTaskStartLock = new();
    private Task _scanTask;
    private Task _purgeTask;

    public MediaFinder()
    {
      _scanTask = Task.Run(Scan);
      _purgeTask = Task.CompletedTask;
    }

    private void Scan()
    {
      ImmutableList<string> initialMedia = Settings.LoadEnumeratedMediaFiles();
      TimeSpan? initialMediaAge = Settings.LoadAgeOfEnumeratedMediaFiles();
      ImmutableList<string> initialEnumeratedMediaFolders = Settings.LoadEnumeratedMediaFolders();
      ImmutableList<string> rootDirectories = Settings.LoadMediaFolders();

      // does it look like we don't need to rescan?
      if (rootDirectories.SequenceEqual(initialEnumeratedMediaFolders) &&
        !initialMedia.IsEmpty &&
        initialMediaAge < TimeSpan.FromDays(5))
      {
        // ok this is good
        _media = initialMedia;
      }
      else // re-scan
      {
        _media = null;

        var filesGroupedByFolder = ImmutableList<ImmutableList<string>>.Empty;
        foreach (var rootDir in rootDirectories)
        {
          // NOTE: doing this the hard way (rather than using SearchOption.AllDirectories)
          // because when it encounters a folder it doesn't have access to, I want it to keep chugging
          Stack<string> stack = new();
          stack.Push(rootDir);
          while (stack.Count > 0)
          {
            var next = stack.Pop();
            var newFiles = ImmutableList<string>.Empty;
            try
            {
              newFiles = Directory.EnumerateFiles(next)
                .Where(file => FilePathIsProbablyPicture(file) || FilePathIsProbablyVideo(file))
                .ToImmutableList();
            }
            catch // usually it's something like "don't have access to the directory"
            {
              // ok whatever
            }

            if (!newFiles.IsEmpty)
            {
              filesGroupedByFolder = filesGroupedByFolder.Add(newFiles);
              _media = (_media ?? ImmutableList<string>.Empty).AddRange(newFiles);
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
          _media = ImmutableList<string>.Empty;
        }
        else
        {
          // randomize by folder
          var randomized = Randomizer.Randomize(filesGroupedByFolder);
          _media = randomized.SelectMany(x => x).ToImmutableList();
        }

        // prevent application shutdown until these files are done being written
        Program.PreventExitDuring(() =>
        {
          Settings.SaveEnumeratedMediaFolders(rootDirectories);
          Settings.SaveEnumeratedMediaFiles(_media);
        });
      }
    }

    public void NotifyMediaFileDidNotExist()
    {
      lock (_purgeTaskStartLock)
      {
        if (_scanTask.IsCompleted && _purgeTask.IsCompleted)
        {
          _purgeTask = Task.Run(Purge);
        }
      }
    }

    private void Purge()
    {
      if (_media == null)
      {
        return; // nothing to purge
      }

      // the best case is the files are only temporarily unavailable
      // (I link to files on a network drive, and sometimes the network computer isn't on)
      // so if any root media directories don't exist, then remove all media from those directories
      foreach (var rootDir in Settings.LoadMediaFolders())
      {
        bool exists = false;
        try
        {
          exists = Directory.Exists(rootDir);
        }
        catch { }

        if (!exists)
        {
          // add trailing slash so simple string.StartsWith() check can be used and
          // will not be tricked by 2 root dirs named "cat" and "cats"
          var rootDir1 = (rootDir.EndsWith('/') || rootDir.EndsWith('\\')) ? rootDir : (rootDir + '\\');
          var rootDir2 = (rootDir.EndsWith('/') || rootDir.EndsWith('\\')) ? rootDir : (rootDir + '/');

          _media = _media
            .Where(x => !x.StartsWith(rootDir1) && !x.StartsWith(rootDir2))
            .ToImmutableList();
        }
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
