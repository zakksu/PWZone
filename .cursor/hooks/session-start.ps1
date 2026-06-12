# Reminds agents about PW Companion dev workflow at session start
$root = if ($env:CURSOR_PROJECT_DIR) { $env:CURSOR_PROJECT_DIR } else { Get-Location }
$agentsPath = Join-Path $root "AGENTS.md"

$hint = @"
PW Companion project. Read AGENTS.md for dev workflow.
Quick start: powershell -File scripts/status.ps1
Web map: powershell -File scripts/dev.ps1 -Target webmap (foreground)
"@

if (Test-Path $agentsPath) {
    $hint += "`nFull instructions in AGENTS.md"
}

@{ additional_context = $hint } | ConvertTo-Json -Compress
