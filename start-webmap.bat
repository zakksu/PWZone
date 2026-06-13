@echo off
title PW Companion WebMap - DEV ONLY
echo.
echo  ============================================
echo   DEV ONLY - do not use this to play PW
echo   Use launch-companion.bat instead
echo  ============================================
echo.
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\dev.ps1" -Target webmap
pause
