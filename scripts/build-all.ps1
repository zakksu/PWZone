# Build everything locally (WebMap + optional desktop companion)
$ErrorActionPreference = "Stop"
. "$PSScriptRoot\lib\DevState.ps1"

$root = Get-ProjectRoot
Write-DevLog "build-all.ps1 started"

Write-Host "=== PW Companion Build ===" -ForegroundColor Cyan
$failed = $false

# WebMap production build
$nodeDir = Join-Path $root "tools\node\node-v20.18.1-win-x64"
$nodeExe = Join-Path $nodeDir "node.exe"
if (Test-Path $nodeExe) {
    $env:PATH = "$nodeDir;" + $env:PATH
    $webMap = Join-Path $root "src\WebMap"
    Set-Location $webMap

    if (-not (Test-Path "node_modules")) { npm install }

    Write-Host ""
    Write-Host "[WebMap] typecheck + lint + build" -ForegroundColor Yellow
    npm run typecheck; if ($LASTEXITCODE -ne 0) { $failed = $true }
    npm run lint;      if ($LASTEXITCODE -ne 0) { $failed = $true }
    npm run build;     if ($LASTEXITCODE -ne 0) { $failed = $true }

    if (-not $failed) {
        Write-Host "[OK] WebMap dist: $webMap\dist" -ForegroundColor Green
        $env:VITE_BASE_PATH = "/"
    }
} else {
    Write-Host "[SKIP] Node not found in tools/node" -ForegroundColor Yellow
}

# Desktop companion (optional)
Set-Location $root
$dotnetSdks = @()
try { $dotnetSdks = @(dotnet --list-sdks 2>$null) } catch {}
if ($dotnetSdks.Count -gt 0) {
    Write-Host ""
    Write-Host "[Companion] dotnet publish" -ForegroundColor Yellow
    dotnet publish src/Companion/UI/PWCompanion.UI.csproj -c Release -r win-x64 --self-contained false -o publish/companion
    if ($LASTEXITCODE -ne 0) { $failed = $true }
    else {
        xcopy /E /I /Y data publish\companion\data | Out-Null
        $dist = Join-Path $root "src\WebMap\dist"
        if (Test-Path (Join-Path $dist "index.html")) {
            xcopy /E /I /Y $dist publish\companion\webmap | Out-Null
            Write-Host "[OK] Embedded webmap copied to publish/companion/webmap" -ForegroundColor Green
        }
        Write-Host "[OK] Companion: $root\publish\companion" -ForegroundColor Green
    }
} else {
    Write-Host "[SKIP] .NET SDK not installed" -ForegroundColor Yellow
}

Write-DevLog "build-all.ps1 finished - failed=$failed"
Update-DevStateSnapshot | Out-Null

if ($failed) { exit 1 }
Write-Host ""
Write-Host "BUILD COMPLETE" -ForegroundColor Green
Write-Host "Preview WebMap: npx serve src/WebMap/dist (from WebMap folder)" -ForegroundColor Gray
