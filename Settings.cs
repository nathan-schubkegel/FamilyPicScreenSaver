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

namespace FamilyPicScreenSaver
{
  public static class Settings
  {
    public static ImmutableArray<string> PictureFolders;
    
    private static string PictureFoldersSettingFilePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FamilyPickScreenSaver", "PictureFolders.txt");
    
    static Settings()
    {
      // default to your pictures folder
      PictureFolders = new[] { Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) }.ToImmutableArray();

      try
      {
        var path = PictureFoldersSettingFilePath;
        if (File.Exists(path))
        {
          PictureFolders = File.ReadAllLines(path)
            .Where(x => x != null)
            .Select(x => x.Trim())
            .Distinct()
            .ToImmutableArray();
        }
      }
      catch
      {
        // better to not show any pictures if you have bogus settings
        PictureFolders = ImmutableArray<string>.Empty;
      }
    }
    
    public static void SetPictureFolders(IEnumerable<string> value)
    {
      PictureFolders = value
        .Where(x => x != null)
        .Select(x => x.Trim())
        .Distinct()
        .ToImmutableArray();
      var path = PictureFoldersSettingFilePath;
      var dirPath = Path.GetDirectoryName(path);
      if (!Directory.Exists(dirPath))
      {
        Directory.CreateDirectory(dirPath);
      }
      File.WriteAllLines(path, PictureFolders);
    }
  }
}
