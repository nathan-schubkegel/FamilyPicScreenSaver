param (
   [string]$version = $(throw "-version is required.")
)

$ErrorActionPreference = "Stop"

if ([System.IO.Directory]::Exists("$PSScriptRoot\build")) {
  Write-Host "Wiping old 'build' directory..."
  [System.IO.Directory]::Delete("$PSScriptRoot\build", $true)
}

Write-Host "Building FamilyPicScreenSaver.csproj..."
&dotnet publish "$PSScriptRoot\..\FamilyPicScreenSaver\FamilyPicScreenSaver.csproj" -c Release -r win-x64 "-p:Version=$version" -o "$PSScriptRoot\build\FamilyPicScreenSaver" --self-contained
if ($LASTEXITCODE -ne 0) { throw "dotnet publish FamilyPicScreenSaver.csproj failed"; }

Write-Host "Removing x86 libvlcsharp files..."
[System.IO.Directory]::Delete("$PSScriptRoot\build\FamilyPicScreenSaver\libvlc\win-x86", $true)

Write-Host "Building Launcher.csproj..."
&dotnet publish "$PSScriptRoot\..\Launcher\Launcher.csproj" -o "$PSScriptRoot\build\Launcher" -c Release "-p:Version=$version"
if ($LASTEXITCODE -ne 0) { throw "dotnet publish Launcher.csproj failed"; }

Write-Host "Modifying version of installerScript.iss..."
$issPath = "$PSScriptRoot\installerScript.iss"
(Get-Content $issPath) -replace '^AppVersion=(.*)$', "AppVersion=$version" | Out-File $issPath -Encoding utf8

Write-Host "Running Inno Setup Compiler..."
&"C:\Program Files (x86)\Inno Setup 6\iscc.exe" $issPath
if ($LASTEXITCODE -ne 0) { throw "failed to produce installer"; }