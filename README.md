# FamilyPicScreenSaver
A screen saver for windows that shows family pictures and videos

How to use
-----
1. Build `FamilyPicScreenSaver.scr`
2. Copy and rename file `Launcher\bin\Debug\net462\FamilyPicScreenSaver.exe` to `C:\Windows\System32\FamilyPicScreenSaver.scr`
3. Copy and rename folder `FamilyPicScreenSaver\bin\Debug\net8.0-windows` to `C:\Program Files\FamilyPicScreenSaver`
4. To be able to run on a computer without needing .NET 8 installed, in `FamilyPicScreenSaver` run `dotnet publish --self-contained -r win-x64` and copy and rename folder `FamilyPicScreenSaver\bin\Release\net8.0-windows\win-x64\publish` to `C:\Program Files\FamilyPicScreenSaver`

Licensing
---------
The contents of this repo are free and unencumbered software released into the public domain under The Unlicense. You have complete freedom to do anything you want with the software, for any purpose. Please refer to <http://unlicense.org/> .