# Agent-readable GitHub + Pages status (no gh auth required for public repo)
param(
    [string]$Repo = "zakksu/PWZone"
)

$ErrorActionPreference = "SilentlyContinue"
. "$PSScriptRoot\lib\DevState.ps1"

$outPath = Join-Path (Get-ProjectRoot) ".cursor\github-status.json"
$dir = Split-Path $outPath -Parent
if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }

$status = @{
    repo = $Repo
    checkedAt = (Get-Date).ToUniversalTime().ToString("o")
    pagesUrl = "https://zakksu.github.io/PWZone/"
    repoUrl = "https://github.com/$Repo"
    workflows = @()
    pagesLive = $false
    pagesHttpStatus = $null
}

try {
    $runs = Invoke-RestMethod -Uri "https://api.github.com/repos/$Repo/actions/runs?per_page=6" -Headers @{ "User-Agent" = "PWCompanion-Agent" }
    foreach ($run in $runs.workflow_runs) {
        $status.workflows += @{
            name = $run.name
            conclusion = $run.conclusion
            status = $run.status
            url = $run.html_url
            createdAt = $run.created_at
        }
    }
} catch {
    $status.apiError = $_.Exception.Message
}

try {
    $r = Invoke-WebRequest -Uri $status.pagesUrl -UseBasicParsing -TimeoutSec 5
    $status.pagesLive = $r.StatusCode -eq 200
    $status.pagesHttpStatus = $r.StatusCode
} catch {
    $status.pagesLive = $false
    $status.pagesError = $_.Exception.Message
    $status.pagesHint = "Enable Settings -> Pages -> Source: GitHub Actions, then re-run Deploy Web Map workflow"
}

($status | ConvertTo-Json -Depth 5) | Set-Content -Path $outPath -Encoding UTF8
Update-DevStateSnapshot | Out-Null

Write-Host "=== GitHub Agent Status ===" -ForegroundColor Cyan
Write-Host "Pages: $(if ($status.pagesLive) { 'LIVE' } else { 'DOWN - ' + $status.pagesHint })" -ForegroundColor $(if ($status.pagesLive) { 'Green' } else { 'Yellow' })
Write-Host "URL:   $($status.pagesUrl)"
Write-Host "Saved: $outPath"
Write-Host ""
foreach ($w in $status.workflows | Select-Object -First 4) {
    $color = if ($w.conclusion -eq 'success') { 'Green' } elseif ($w.conclusion -eq 'failure') { 'Red' } else { 'Gray' }
    Write-Host "[$($w.conclusion)] $($w.name)" -ForegroundColor $color
}
