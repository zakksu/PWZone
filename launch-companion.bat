@echo off
title PW Companion
cd /d "%~dp0"

if exist "publish\companion\PWCompanion.exe" (
    echo Starting published PW Companion...
    start "" "publish\companion\PWCompanion.exe"
    exit /b 0
)

where dotnet >nul 2>&1
if errorlevel 1 (
    echo .NET SDK not found.
    echo Install from https://dotnet.microsoft.com/download
    echo OR download PWCompanion-win-x64.zip from GitHub Actions.
    pause
    exit /b 1
)

echo Starting PW Companion from source...
dotnet run --project src\Companion\UI\PWCompanion.UI.csproj
