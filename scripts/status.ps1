# Refresh and print dev state for Cursor agents and humans
$ErrorActionPreference = "Stop"
. "$PSScriptRoot\lib\DevState.ps1"

Write-DevLog "status.ps1 - health check requested"

$state = Update-DevStateSnapshot

Write-Host ""
Write-Host "=== PW Companion Dev Status ===" -ForegroundColor Cyan
Write-Host ""

$wm = $state.webmap
$cp = $state.companion

if ($wm.running) {
    Write-Host "[OK]  Web map:  $($wm.url)" -ForegroundColor Green
} else {
    Write-Host "[--]  Web map:  NOT RUNNING ($($wm.lastError))" -ForegroundColor Yellow
    Write-Host "      Start: powershell -File scripts/dev.ps1 -Target webmap" -ForegroundColor Gray
}

if ($cp.running) {
    Write-Host "[OK]  Companion: $($cp.healthUrl)" -ForegroundColor Green
} else {
    Write-Host "[--]  Companion: NOT RUNNING ($($cp.lastError))" -ForegroundColor Yellow
    Write-Host "      Start: powershell -File scripts/dev.ps1 -Target companion" -ForegroundColor Gray
}

Write-Host ""
Write-Host "State file: $(Get-DevStatePath)" -ForegroundColor Gray
Write-Host "Log file:   $(Get-DevLogPath)" -ForegroundColor Gray

if ($state.logTail.Count -gt 0) {
    Write-Host ""
    Write-Host "--- Recent log ---" -ForegroundColor DarkGray
    $state.logTail | ForEach-Object { Write-Host $_ -ForegroundColor DarkGray }
}

Write-Host ""
