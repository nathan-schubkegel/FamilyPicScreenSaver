$ErrorActionPreference = "Stop"

if ([System.IO.Directory]::Exists("$PSScriptRoot\build")) {
  Write-Host "Wiping old 'build' directory..."
  [System.IO.Directory]::Delete("$PSScriptRoot\build", $true)
}

Write-Host "Building FamilyPicScreenSaver.csproj..."
&dotnet publish "$PSScriptRoot\..\FamilyPicScreenSaver\FamilyPicScreenSaver.csproj" -c Release -r win-x64 -o "$PSScriptRoot\build\FamilyPicScreenSaver" --self-contained
if ($LASTEXITCODE -ne 0) { throw "dotnet publish FamilyPicScreenSaver.csproj failed"; }

Write-Host "Removing x86 libvlcsharp files..."
[System.IO.Directory]::Delete("$PSScriptRoot\build\FamilyPicScreenSaver\libvlc\win-x86", $true)

Write-Host "Building Launcher.csproj..."
&dotnet publish "$PSScriptRoot\..\Launcher\Launcher.csproj" -o "$PSScriptRoot\build\Launcher" -c Release
if ($LASTEXITCODE -ne 0) { throw "dotnet publish Launcher.csproj failed"; }

Write-Host "Running Inno Setup Compiler..."
&"C:\Program Files (x86)\Inno Setup 6\iscc.exe" installerScript.iss
if ($LASTEXITCODE -ne 0) { throw "failed to produce installer"; }