# Family Pic Screen Saver
A screen saver for windows that shows family pictures and videos.

How to develop
-----
1. F5 in Visual Studio to debug `FamilyPicScreenSaver.csproj` - it ignores mouse and keystrokes while debugging, so you gotta press Alt-F4 to quit it.

How to install on your grandma's computer
-----
1. Download and install Inno Setup 6 from https://jrsoftware.org/isinfo.php (version 6.3.3 is currently used)
2. Open a command prompt, run `powershell ./Installer/buildInstaller.ps1`
3. It produces a self-installing executable at `./Installer/build/FamilyPicScreenSaverInstaller.exe`

Licensing
---------
The contents of this repo are free and unencumbered software released into the public domain under The Unlicense. You have complete freedom to do anything you want with the software, for any purpose. Please refer to <http://unlicense.org/> .