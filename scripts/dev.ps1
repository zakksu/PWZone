# Unified dev launcher - foreground output for Cursor terminal visibility
param(
    [ValidateSet("webmap", "companion", "all")]
    [string]$Target = "webmap",

    [switch]$InstallDeps
)

$ErrorActionPreference = "Stop"
. "$PSScriptRoot\lib\DevState.ps1"

$root = Get-ProjectRoot
$nodeDir = Join-Path $root "tools\node\node-v20.18.1-win-x64"
$nodeExe = Join-Path $nodeDir "node.exe"

function Start-WebMapDev {
    if (-not (Test-Path $nodeExe)) {
        Write-DevLog "Node not found at $nodeDir" "ERROR"
        throw "Node.js missing. Run setup or install from https://nodejs.org/"
    }

    $env:PATH = "$nodeDir;" + $env:PATH
    $webMap = Join-Path $root "src\WebMap"
    Set-Location $webMap

    if ($InstallDeps -or -not (Test-Path "node_modules")) {
        Write-DevLog "npm install (WebMap)"
        Write-Host "Installing WebMap dependencies..." -ForegroundColor Cyan
        npm install 2>&1 | ForEach-Object { Write-Host $_; Write-DevLog $_ }
    }

    Write-DevLog "Starting Vite dev server on 127.0.0.1:5173"
    Write-Host ""
    Write-Host "=== PW Companion Web Map ===" -ForegroundColor Green
    Write-Host "URL:    http://127.0.0.1:5173/" -ForegroundColor Cyan
    Write-Host "Demo:   auto-starts (offline mode)" -ForegroundColor Gray
    Write-Host "Stop:   Ctrl+C" -ForegroundColor Gray
    Write-Host "Status: powershell -File scripts/status.ps1" -ForegroundColor Gray
    Write-Host ""

    Update-DevStateSnapshot | Out-Null

    npm run dev 2>&1 | ForEach-Object {
        Write-Host $_
        Write-DevLog $_
        if ($_ -match "error|Error|ERROR") {
            Update-DevStateSnapshot | Out-Null
        }
    }
}

function Start-CompanionDev {
    $dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
    if (-not $dotnet) {
        Write-DevLog ".NET SDK not found" "ERROR"
        throw ".NET 8 SDK required. Install from https://dotnet.microsoft.com/download"
    }

    Set-Location $root
    Write-DevLog "dotnet run - Companion UI (demo mode)"
    Write-Host ""
    Write-Host "=== PW Companion Desktop ===" -ForegroundColor Green
    Write-Host "Demo mode: enabled in appsettings.json" -ForegroundColor Gray
    Write-Host "WebSocket: ws://127.0.0.1:17847/" -ForegroundColor Cyan
    Write-Host "Debug:     F12" -ForegroundColor Gray
    Write-Host ""

    Update-DevStateSnapshot | Out-Null

    dotnet run --project src/Companion/UI/PWCompanion.UI.csproj 2>&1 | ForEach-Object {
        Write-Host $_
        Write-DevLog $_
    }
}

Write-DevLog "dev.ps1 started - Target=$Target"

try {
    switch ($Target) {
        "webmap"    { Start-WebMapDev }
        "companion" { Start-CompanionDev }
        "all" {
            Write-Host "Starting both requires two terminals:" -ForegroundColor Yellow
            Write-Host "  Terminal 1: scripts/dev.ps1 -Target webmap" -ForegroundColor Gray
            Write-Host "  Terminal 2: scripts/dev.ps1 -Target companion" -ForegroundColor Gray
            Start-WebMapDev
        }
    }
} catch {
    Write-DevLog $_.Exception.Message "ERROR"
    Update-DevStateSnapshot | Out-Null
    throw
}
