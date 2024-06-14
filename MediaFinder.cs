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

namespace FamilyPicScreenSaver
{
  public class MediaFinder
  {
    public static readonly string LoadingPicPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "loading.jpg");

    public static readonly string BrokenPicPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "broken.jpg");

    private volatile ImmutableList<string> _media = ImmutableList<string>.Empty.Add(LoadingPicPath);

    public IReadOnlyList<string> Media => _media;

    public MediaFinder(IEnumerable<string> directories)
    {
      var myDirs = directories.ToList();
      Task.Run(() => Scan(myDirs));
    }

    private void Scan(List<string> directories)
    {
      _media = ImmutableList<string>.Empty.Add(LoadingPicPath);

      foreach (var dir in directories)
      {
        // NOTE: doing this the hard way (rather than using SearchOption.AllDirectories)
        // because when it encounters a folder it doesn't have access to, I want it to keep chugging
        Stack<string> pictureFoldersToSearch = new();
        pictureFoldersToSearch.Push(dir);
        while (pictureFoldersToSearch.Count > 0)
        {
          var myPictureFolder = pictureFoldersToSearch.Pop();
          try
          {
            var files = System.IO.Directory.EnumerateFiles(myPictureFolder, "*")
              .Where(file => FilePathIsProbablyPicture(file) || FilePathIsProbablyVideo(file))
              // sort by date so pictures and videos are more likely to be played back
              // in the order they were recorded. (not always the case when Rachel rotates images
              // while going through them later, but it's the best I can do)
              .Select(x =>
              {
                try
                {
                  var fi = new FileInfo(x);
                  var a = fi.CreationTimeUtc;
                  var b = fi.LastWriteTimeUtc;
                  return (path: x, date: a < b ? a : b);
                }
                catch
                {
                  return (path: null, date: default);
                }
              })
              .Where(x => x.path != null)
              .OrderBy(x => x.date)
              .Select(x => x.path)
              .ToList();

            if (files.Count > 0) // not sure if this buys much
            {
              _media = _media.AddRange(files);
            }
          }
          catch // usually it's something like "don't have access to the directory"
          {
            // ok whatever
          }

          try
          {
            var dirs = System.IO.Directory.EnumerateDirectories(myPictureFolder, "*");
            foreach (var durrr in dirs)
            {
              pictureFoldersToSearch.Push(durrr);
            }
          }
          catch // usually it's something like "don't have access to the directory"
          {
            // ok whatever
          }
        }
      }

      if (_media.Count == 1)
      {
        _media = ImmutableList<string>.Empty.Add(BrokenPicPath);
      }
      else
      {
        // remove loading pic, put last pic there (so all other indexes are unchanged
        // so forward/backward history remains largely unbroken)
        var last = _media.Last();
        _media = _media.SetItem(0, last).RemoveAt(_media.Count - 1);
      }
    }

    public static bool FilePathIsProbablyVideo(string file)
    {
      return file.EndsWith(".avi", StringComparison.OrdinalIgnoreCase) ||
        file.EndsWith(".mpg", StringComparison.OrdinalIgnoreCase) ||
        file.EndsWith(".mkv", StringComparison.OrdinalIgnoreCase) ||
        file.EndsWith(".webm", StringComparison.OrdinalIgnoreCase) ||
        file.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase);
    }

    public static bool FilePathIsProbablyPicture(string file)
    {
      return file.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
        file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
        file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
        file.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase);
    }
  }
}
