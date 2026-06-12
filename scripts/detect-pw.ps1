# Detect running Perfect World / TCG client processes
$ErrorActionPreference = "SilentlyContinue"

$candidates = @("elementclient_64", "elementclient", "pwprotector", "PWVoiceChat")
Write-Host "=== PW Process Detection ===" -ForegroundColor Cyan

$found = $false
foreach ($name in $candidates) {
    $procs = Get-Process -Name $name -ErrorAction SilentlyContinue
    if ($procs) {
        $found = $true
        foreach ($p in $procs) {
            Write-Host "[RUNNING] $name PID $($p.Id)" -ForegroundColor Green
        }
    }
}

if (-not $found) {
    Write-Host "[NONE] No PW client processes found. Launch the game first." -ForegroundColor Yellow
    exit 1
}

$main = Get-Process -Name elementclient_64 -ErrorAction SilentlyContinue | Select-Object -First 1
if ($main) {
    Write-Host ""
    Write-Host "Main client: elementclient_64 (PID $($main.Id))" -ForegroundColor Green
    Write-Host "Companion should attach to this process." -ForegroundColor Gray
}

exit 0
