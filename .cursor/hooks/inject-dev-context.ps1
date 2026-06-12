# Injects PW Companion dev state into agent context after shell commands
$ErrorActionPreference = "SilentlyContinue"

$root = if ($env:CURSOR_PROJECT_DIR) { $env:CURSOR_PROJECT_DIR } else { Get-Location }
$statePath = Join-Path $root ".cursor\dev-state.json"
$logPath = Join-Path $root "logs\dev-session.log"

$contextParts = @("## PW Companion dev snapshot")

if (Test-Path $statePath) {
    $state = Get-Content $statePath -Raw | ConvertFrom-Json
    $wm = if ($state.webmap.running) { "UP $($state.webmap.url)" } else { "DOWN ($($state.webmap.lastError))" }
    $cp = if ($state.companion.running) { "UP $($state.companion.healthUrl)" } else { "DOWN ($($state.companion.lastError))" }
    $contextParts += "- Web map: $wm"
    $contextParts += "- Companion: $cp"
} else {
    $contextParts += "- No dev-state yet. Run: powershell -File scripts/status.ps1"
}

if (Test-Path $logPath) {
    $tail = Get-Content $logPath -Tail 8 -ErrorAction SilentlyContinue
    if ($tail) {
        $contextParts += "- Recent log:"
        $contextParts += ($tail | ForEach-Object { "  $_" })
    }
}

$context = $contextParts -join "`n"
@{ additional_context = $context } | ConvertTo-Json -Compress
