# PW Companion — start the web map (delegates to scripts/dev.ps1)
& (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "scripts\dev.ps1") -Target webmap
