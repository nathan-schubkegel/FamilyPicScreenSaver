[Setup]

; AppName is displayed throughout the Setup program and uninstaller in window titles, 
; wizard pages, and dialog boxes.
AppName=Family Pic Screen Saver
AppVersion=1.2.2

; Minimum windows version = any windows 10
MinVersion=10

; Only allow the installer to run on x64-compatible systems, and enable 64-bit install mode.
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

; something about simplifying the installer
AlwaysShowComponentsList=no

; This stuff is displayed on the "Support" dialog of the Add/Remove Programs Control Panel applet.
AppPublisher=Nathan Schubkegel
AppPublisherURL=https://github.com/nathan-schubkegel/FamilyPicScreenSaver
AppCopyright=This is free and unencumbered software released into the public domain.

; This is not used for display anywhere; it impacts uninstall logs and Uninstall registry key
; but you should try to keep it the same across all versions of one program
; or else in the control panel it can look like there are multiple of your app installed!
AppId=FamilyPicScreenSaver-164A6ABC-9D74-4279-B479-E1BB2E6EEC05

; Determines files Setup will check for being in use before uninstalling
CloseApplicationsFilter=*.exe,*.dll,*.scr

; pick where the application installs to by default - {autopf} means c:\Program Files
DefaultDirName={autopf}\Family Pic Screen Saver

; Make the user pick the directory each time
; (not because we're sadists, but without this the installer wizard doesn't show that page
;  for upgrade installs. It's a "flow" thing)
UsePreviousAppDir=no

; start menu folder name
DefaultGroupName=Family Pic Screen Saver

SetupIconFile=..\icon.ico
UninstallDisplayIcon={uninstallexe}

WizardStyle=modern
DisableProgramGroupPage=yes
OutputDir=build
OutputBaseFilename=FamilyPicScreenSaverInstaller
InfoBeforeFile=..\LICENSE
DisableReadyPage=yes

[Files]
; {sys} the system32 directory
; {commonpf} the program files directory
; {localappdata} what you'd think
; {app} user's chosen install directory
Source: "build\FamilyPicScreenSaver\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs
Source: "build\Launcher\Launcher.exe"; DestDir: "{sys}"; DestName: "Family Pic Screen Saver.scr"; Flags: ignoreversion

[Run]
; Set the screen saver and show the windows dialog
; see from https://stackoverflow.com/questions/49791065/silently-set-the-screensaver-on-windows-from-the-command-line
;
; Gotta use rundll32 from the 'sysnative' directory so it'll run as a 64-bit app not a 32-bit app like the installer
; and then the screen saver selections will be weirdly unavailable.
; see https://stackoverflow.com/questions/18982740/running-system32-programs-instead-of-syswow64-when-launching-from-mingw
Filename: "{win}\sysnative\rundll32.exe"; Parameters: "desk.cpl,InstallScreenSaver Family Pic Screen Saver.scr"; Description: "Change this computer's screen saver to Family Pic Screen Saver"; Flags: postinstall runasoriginaluser

[Icons]
Name: "{commondesktop}\Family Pic Screen Saver"; Filename: "{app}\FamilyPicScreenSaver.exe"; WorkingDir: "{app}"; Parameters: "/s"
Name: "{group}\Family Pic Screen Saver"; Filename: "{app}\FamilyPicScreenSaver.exe"; WorkingDir: "{app}"; Parameters: "/s"
Name: "{group}\Family Pic Screen Saver Settings"; Filename: "{app}\FamilyPicScreenSaver.exe"; WorkingDir: "{app}"; Parameters: "/c"

[INI]
Filename: "{commonappdata}\Family Pic Screen Saver\LauncherSettings.ini"; Section: "InstallSettings"; Key: "LaunchFilePath"; String: {app}\FamilyPicScreenSaver.exe

[UninstallDelete]
Type: filesandordirs; Name: "{commonappdata}\Family Pic Screen Saver"
