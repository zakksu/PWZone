# Production build for WebMap only
param(
    [string]$BasePath = "/"
)

$ErrorActionPreference = "Stop"
. "$PSScriptRoot\lib\DevState.ps1"

$root = Get-ProjectRoot
$nodeDir = Join-Path $root "tools\node\node-v20.18.1-win-x64"
if (-not (Test-Path (Join-Path $nodeDir "node.exe"))) {
    throw "Node.js missing in tools/node"
}

$env:PATH = "$nodeDir;" + $env:PATH
$env:VITE_BASE_PATH = $BasePath
$env:GITHUB_PAGES = "true"
$env:VITE_GITHUB_PAGES = "true"

Set-Location (Join-Path $root "src\WebMap")
if (-not (Test-Path "node_modules")) { npm install }

Write-Host "Building WebMap (base=$BasePath)..." -ForegroundColor Cyan
npm run build
Write-Host "Output: src/WebMap/dist" -ForegroundColor Green
