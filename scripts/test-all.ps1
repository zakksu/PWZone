# Run full PW Companion test suite
$ErrorActionPreference = "Stop"
. "$PSScriptRoot\lib\DevState.ps1"

$root = Get-ProjectRoot
Write-DevLog "test-all.ps1 started"

Write-Host "=== PW Companion Tests ===" -ForegroundColor Cyan
$failed = $false

# .NET tests (skip gracefully if SDK missing)
$dotnetSdks = @()
try { $dotnetSdks = @(dotnet --list-sdks 2>$null) } catch {}
if ($dotnetSdks.Count -gt 0) {
    Set-Location $root
    Write-Host ""
    Write-Host "[dotnet test]" -ForegroundColor Yellow
    dotnet test PWCompanion.sln -c Release --verbosity minimal
    if ($LASTEXITCODE -ne 0) { $failed = $true }
} else {
    Write-Host "[SKIP] dotnet not installed" -ForegroundColor Yellow
    Write-DevLog "dotnet test skipped - SDK missing" "WARN"
}

$nodeDir = Join-Path $root "tools\node\node-v20.18.1-win-x64"
$nodeExe = Join-Path $nodeDir "node.exe"
if (Test-Path $nodeExe) {
    $env:PATH = "$nodeDir;" + $env:PATH
    $webMap = Join-Path $root "src\WebMap"
    Set-Location $webMap

    if (-not (Test-Path "node_modules")) {
        Write-Host "[npm install]" -ForegroundColor Yellow
        npm install --silent
    }

    Write-Host ""
    Write-Host "[npm run typecheck]" -ForegroundColor Yellow
    npm run typecheck
    if ($LASTEXITCODE -ne 0) { $failed = $true }

    Write-Host ""
    Write-Host "[npm run lint]" -ForegroundColor Yellow
    npm run lint
    if ($LASTEXITCODE -ne 0) { $failed = $true }

    Write-Host ""
    Write-Host "[npm run test]" -ForegroundColor Yellow
    npm run test
    if ($LASTEXITCODE -ne 0) { $failed = $true }

    Write-Host ""
    Write-Host "[npm run build]" -ForegroundColor Yellow
    npm run build
    if ($LASTEXITCODE -ne 0) { $failed = $true }

    Write-Host ""
    Write-Host "[npm run test:smoke]" -ForegroundColor Yellow
    npx playwright install chromium 2>$null | Out-Null
    npm run test:smoke
    if ($LASTEXITCODE -ne 0) { $failed = $true }
} else {
    Write-Host "[SKIP] Node not in tools/node" -ForegroundColor Yellow
    Write-DevLog "WebMap tests skipped - Node missing" "WARN"
}

Write-DevLog "test-all.ps1 finished - failed=$failed"
Update-DevStateSnapshot | Out-Null

Write-Host ""
if ($failed) {
    Write-Host "TESTS FAILED" -ForegroundColor Red
    exit 1
}
Write-Host "ALL TESTS PASSED" -ForegroundColor Green
exit 0
