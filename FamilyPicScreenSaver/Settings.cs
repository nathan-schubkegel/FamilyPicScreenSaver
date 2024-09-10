/*
This is free and unencumbered software released into the public domain under The Unlicense.
You have complete freedom to do anything you want with the software, for any purpose.
Please refer to <http://unlicense.org/>
*/

using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace FamilyPicScreenSaver
{
  public static class Settings
  {
    private static readonly string _settingsFolderPath = Path.Combine(
      Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
      "Family Pic Screen Saver");

    private static readonly string _mediaFoldersSettingFilePath = Path.Combine(
      _settingsFolderPath, "MediaFolders.txt");

    private static readonly string _enumeratedMediaFoldersSettingFilePath = Path.Combine(
      _settingsFolderPath, "EnumeratedMediaFolders.txt");

    private static readonly string _enumeratedMediaFilesSettingFilePath = Path.Combine(
      _settingsFolderPath, "EnumeratedMediaFiles.txt");

    public static ImmutableList<string> LoadMediaFolders()
    {
      try
      {
        lock (_mediaFoldersSettingFilePath)
        {
          if (File.Exists(_mediaFoldersSettingFilePath))
          {
            return File.ReadAllLines(_mediaFoldersSettingFilePath)
              .Select(x => x?.Trim())
              .Where(x => !string.IsNullOrEmpty(x))
              .Distinct(StringComparer.Ordinal)
              .ToImmutableList();
          }
        }
      }
      catch
      {
        // better to not show any pictures if you have bogus settings
        return ImmutableList<string>.Empty;
      }

      return
      [
        Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
        Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
      ];
    }

    public static void SaveMediaFolders(ImmutableList<string> folders)
    {
      try
      {
        lock (_mediaFoldersSettingFilePath)
        {
          var dirPath = Path.GetDirectoryName(_mediaFoldersSettingFilePath);
          if (!Directory.Exists(dirPath))
          {
            Directory.CreateDirectory(dirPath);
          }
          File.WriteAllLines(_mediaFoldersSettingFilePath, folders);
        }
      }
      catch
      {
      }
    }

    public static ImmutableList<string> LoadEnumeratedMediaFolders()
    {
      try
      {
        lock (_enumeratedMediaFoldersSettingFilePath)
        {
          if (File.Exists(_enumeratedMediaFoldersSettingFilePath))
          {
            return File.ReadLines(_enumeratedMediaFoldersSettingFilePath)
              .Select(x => x?.Trim())
              .Where(x => !string.IsNullOrEmpty(x))
              .ToImmutableList();
          }
        }
      }
      catch
      {
      }
      return ImmutableList<string>.Empty;
    }

    public static void SaveEnumeratedMediaFolders(ImmutableList<string> folders)
    {
      try
      {
        lock (_enumeratedMediaFoldersSettingFilePath)
        {
          var dirPath = Path.GetDirectoryName(_enumeratedMediaFoldersSettingFilePath);
          if (!Directory.Exists(dirPath))
          {
            Directory.CreateDirectory(dirPath);
          }
          File.WriteAllLines(_enumeratedMediaFoldersSettingFilePath, folders);
        }
      }
      catch
      {
      }
    }

    public static TimeSpan? LoadAgeOfEnumeratedMediaFiles()
    {
      try
      {
        if (File.Exists(_enumeratedMediaFilesSettingFilePath))
        {
          var fi = new FileInfo(_enumeratedMediaFilesSettingFilePath);
          var time = fi.LastWriteTime;
          var now = DateTime.Now;
          if (now > time)
          {
            return now - time;
          }
          return TimeSpan.Zero;
        }
      }
      catch { }
      return null;
    }

    public static ImmutableList<string> LoadEnumeratedMediaFiles()
    {
      try
      {
        lock (_enumeratedMediaFilesSettingFilePath)
        {
          if (File.Exists(_enumeratedMediaFilesSettingFilePath))
          {
            return File.ReadLines(_enumeratedMediaFilesSettingFilePath)
              .Select(x => x?.Trim())
              .Where(x => !string.IsNullOrEmpty(x))
              .ToImmutableList();
          }
        }
      }
      catch
      {
      }
      return ImmutableList<string>.Empty;
    }

    public static void SaveEnumeratedMediaFiles(ImmutableList<string> allMedia)
    {
      try
      {
        lock (_enumeratedMediaFilesSettingFilePath)
        {
          var dirPath = Path.GetDirectoryName(_enumeratedMediaFilesSettingFilePath);
          if (!Directory.Exists(dirPath))
          {
            Directory.CreateDirectory(dirPath);
          }
          File.WriteAllLines(_enumeratedMediaFilesSettingFilePath, allMedia.Select(x => x.ToString()));
        }
      }
      catch
      {
      }
    }
  }
}