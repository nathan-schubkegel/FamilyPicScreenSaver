# Family Pic Screen Saver
A screen saver for windows that shows family pictures and videos.

How to use
-----
1. Download latest release from https://github.com/octo-org/octo-repo/releases/latest
2. Install on your grandma's computer
3. (Optional/Recommended) In start menu, run "Family Pic Screen Saver Settings" and add more directories with pictures/videos!

How to develop
-----
1. F5 in Visual Studio to debug `FamilyPicScreenSaver.csproj` - it ignores mouse and keystrokes while debugging, so press Alt-F4 to quit.

How build installer
-----
1. Download and install Inno Setup 6 from https://jrsoftware.org/isinfo.php (version 6.3.3 is currently used)
2. Open a command prompt, run `powershell ./Installer/buildInstaller.ps1 -version X.Y.Z` for whatever version number you imagine
3. It produces a self-installing executable at `./Installer/build/FamilyPicScreenSaverInstaller-vX.Y.Z.exe`

Licensing
---------
The contents of this repo are free and unencumbered software released into the public domain under The Unlicense. You have complete freedom to do anything you want with the software, for any purpose. Please refer to <http://unlicense.org/> .