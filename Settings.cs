/*
This is free and unencumbered software released into the public domain under The Unlicense.
You have complete freedom to do anything you want with the software, for any purpose.
Please refer to <http://unlicense.org/>
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Rope;

namespace FamilyPicScreenSaver
{
  public static class Settings
  {
    private static readonly string _settingsFolderPath = Path.Combine(
      Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
      "FamilyPicScreenSaver");

    private static readonly string _mediaFoldersSettingFilePath = Path.Combine(
      _settingsFolderPath, "MediaFolders.txt");

    private static readonly string _enumeratedMediaFoldersSettingFilePath = Path.Combine(
      _settingsFolderPath, "EnumeratedMediaFolders.txt");

    private static readonly string _enumeratedMediaFilesSettingFilePath = Path.Combine(
      _settingsFolderPath, "EnumeratedMediaFiles.txt");

    private static readonly string _currentMediaIndexSettingFilePath = Path.Combine(
      _settingsFolderPath, "CurrentMediaFileIndex.txt");

    public static IEnumerable<string> LoadMediaFolders()
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
              .Distinct(StringComparer.Ordinal);
          }
        }
      }
      catch
      {
        // better to not show any pictures if you have bogus settings
        return [];
      }

      return
      [
        Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
        Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
      ];
    }

    public static void SaveMediaFolders(IEnumerable<string> folders)
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

    public static Rope<string> LoadEnumeratedMediaFolders()
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
              .ToRope();
          }
        }
      }
      catch
      {
      }
      return Rope<string>.Empty;
    }

    public static void SaveEnumeratedMediaFolders(Rope<string> folders)
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

    public static Rope<string> LoadEnumeratedMediaFiles()
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
              .ToRope();
          }
        }
      }
      catch
      {
      }
      return Rope<string>.Empty;
    }

    public static void SaveEnumeratedMediaFiles(Rope<string> allMedia)
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
          File.WriteAllLines(_enumeratedMediaFilesSettingFilePath, allMedia);
        }
      }
      catch
      {
      }
    }

    public static long LoadCurrentMediaIndex()
    {
      long currentIndex = -1;
      try
      {
        lock (_currentMediaIndexSettingFilePath)
        {
          if (File.Exists(_currentMediaIndexSettingFilePath))
          {
            var text = File.ReadAllText(_currentMediaIndexSettingFilePath);
            currentIndex = long.Parse(text, CultureInfo.InvariantCulture);
            currentIndex = Math.Max(-1, currentIndex);
          }
        }
      }
      catch
      {
      }
      return currentIndex;
    }

    public static void SaveCurrentMediaIndex(long currentIndex)
    {
      try
      {
        lock (_currentMediaIndexSettingFilePath)
        {
          var dirPath = Path.GetDirectoryName(_currentMediaIndexSettingFilePath);
          if (!Directory.Exists(dirPath))
          {
            Directory.CreateDirectory(dirPath);
          }

          File.WriteAllLines(_currentMediaIndexSettingFilePath,
            [currentIndex.ToString(CultureInfo.InvariantCulture)]);
        }
      }
      catch
      {
      }
    }
  }
}