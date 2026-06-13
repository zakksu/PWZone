@echo off
title PW Companion
cd /d "%~dp0"

REM Stop broken Vite dev server — companion serves the built map on :5173
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\prepare-launch.ps1"

if exist "publish\companion\PWCompanion.exe" (
    echo Starting PW Companion...
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
