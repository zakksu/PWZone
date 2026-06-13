# Free port 5173 from Vite dev so the companion can serve the built map.
$ErrorActionPreference = "SilentlyContinue"
. "$PSScriptRoot\lib\DevState.ps1"

$port = 5173
$connections = Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue

foreach ($conn in $connections) {
    $proc = Get-Process -Id $conn.OwningProcess -ErrorAction SilentlyContinue
    if (-not $proc) { continue }

    if ($proc.ProcessName -eq "node") {
        Write-Host "Stopping Vite dev server on port $port (PID $($proc.Id))..." -ForegroundColor Yellow
        Stop-Process -Id $proc.Id -Force
        Start-Sleep -Milliseconds 500
        Write-DevLog "Stopped node on port $port (PID $($proc.Id))"
    }
}

$webmapIndex = Join-Path (Get-ProjectRoot) "publish\companion\webmap\index.html"
if (-not (Test-Path $webmapIndex)) {
    Write-Host ""
    Write-Host "WARNING: publish/companion/webmap missing." -ForegroundColor Red
    Write-Host "Download PWCompanion-win-x64.zip from GitHub Actions, or run scripts/build-all.ps1" -ForegroundColor Yellow
}

Write-DevLog "prepare-launch.ps1 finished"
