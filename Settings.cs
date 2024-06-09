/*
This is free and unencumbered software released into the public domain under The Unlicense.
You have complete freedom to do anything you want with the software, for any purpose.
Please refer to <http://unlicense.org/>
*/

using System;
using System.IO;

namespace FamilyPicScreenSaver
{
  public static class Settings
  {
    public static string PictureFolder;
    
    private static string PictureFolderSettingFilePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FamilyPickScreenSaver", "PictureFolder.txt");
    
    static Settings()
    {
      PictureFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
    
      var path = PictureFolderSettingFilePath;
      if (File.Exists(path))
      {
        path = File.ReadAllText(path);
        if (Directory.Exists(path))
        {
          PictureFolder = path;
        }
      }
    }
    
    public static void SetPictureFolder(string value)
    {
      PictureFolder = value;
      var path = PictureFolderSettingFilePath;
      var dirPath = Path.GetDirectoryName(path);
      if (!Directory.Exists(dirPath))
      {
        Directory.CreateDirectory(dirPath);
      }
      File.WriteAllText(path, value);
    }
  }
}
