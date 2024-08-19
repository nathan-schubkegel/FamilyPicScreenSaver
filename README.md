# FamilyPicScreenSaver
A screen saver for windows that shows family pictures and videos

How to develop
-----
1. F5 in Visual Studio to debug `FamilyPicScreenSaver.csproj` - it ignores mouse and keystrokes while debugging, so you gotta press Alt-F4 to quit it.

How to install on your grandma's computer
-----
1. In folder `FamilyPicScreenSaver` run `dotnet publish --self-contained -r win-x64`
2. Copy and rename folder `FamilyPicScreenSaver\bin\Release\net8.0-windows\win-x64\publish` to somewhere like `C:\Program Files\FamilyPicScreenSaver`
3. Build `Launcher\Launcher.csproj` (could use `dotnet publish` or just build `FamilyPicScreenSaver.sln`)
4. Copy and rename file `Launcher\bin\Debug\net462\FamilyPicScreenSaver.exe` to `C:\Windows\System32\FamilyPicScreenSaver.scr`
5. Copy file `Launcher\bin\Debug\net462\FamilyPicScreenSaverLaunchCommand.txt` to `C:\Windows\System32`
6. Edit `C:\Windows\System32\FamilyPicScreenSaverLaunchCommand.txt` so the first line is the full path to the exe you copied in step 2
7. At this point, "FamilyPicScreenSaver" should show up in Windows's screen saver dialog.


Licensing
---------
The contents of this repo are free and unencumbered software released into the public domain under The Unlicense. You have complete freedom to do anything you want with the software, for any purpose. Please refer to <http://unlicense.org/> .