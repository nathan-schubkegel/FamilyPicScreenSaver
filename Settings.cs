using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

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
