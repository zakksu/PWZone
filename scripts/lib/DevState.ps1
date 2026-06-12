# Shared dev-state helpers for PW Companion (Cursor agent observability)

function Get-ProjectRoot {
    $root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
    if (-not (Test-Path (Join-Path $root "PWCompanion.sln"))) {
        throw "Could not find project root from $PSScriptRoot"
    }
    return $root
}

function Get-DevStatePath {
    Join-Path (Get-ProjectRoot) ".cursor\dev-state.json"
}

function Get-DevLogPath {
    $logs = Join-Path (Get-ProjectRoot) "logs"
    if (-not (Test-Path $logs)) { New-Item -ItemType Directory -Path $logs -Force | Out-Null }
    Join-Path $logs "dev-session.log"
}

function Write-DevLog {
    param([string]$Message, [string]$Level = "INFO")
    $line = "[{0}] [{1}] {2}" -f (Get-Date -Format "yyyy-MM-dd HH:mm:ss"), $Level, $Message
    Add-Content -Path (Get-DevLogPath) -Value $line -Encoding UTF8
}

function Test-PortOpen {
    param([string]$Address = "127.0.0.1", [int]$Port)
    try {
        $tcp = New-Object System.Net.Sockets.TcpClient
        $tcp.Connect($Address, $Port)
        $tcp.Close()
        return $true
    } catch { return $false }
}

function Get-HttpStatus {
    param([string]$Url, [int]$TimeoutSec = 3)
    try {
        $r = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec $TimeoutSec
        return @{ ok = $true; statusCode = $r.StatusCode; error = $null }
    } catch {
        return @{ ok = $false; statusCode = $null; error = $_.Exception.Message }
    }
}

function Read-DevState {
    $path = Get-DevStatePath
    if (-not (Test-Path $path)) { return $null }
    Get-Content $path -Raw | ConvertFrom-Json
}

function Write-DevState {
    param([hashtable]$State)
    $path = Get-DevStatePath
    $dir = Split-Path $path -Parent
    if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
    $State.updatedAt = (Get-Date).ToUniversalTime().ToString("o")
    ($State | ConvertTo-Json -Depth 6) | Set-Content -Path $path -Encoding UTF8
}

function Get-TailLog {
    param([int]$Lines = 30)
    $log = Get-DevLogPath
    if (-not (Test-Path $log)) { return @() }
    Get-Content $log -Tail $Lines
}

function Update-DevStateSnapshot {
    $root = Get-ProjectRoot
    $webmapPort = 5173
    $companionPort = 17847
    $webmapUrl = "http://127.0.0.1:$webmapPort/"
    $companionHealth = "http://127.0.0.1:$companionPort/health"

    $webmapPortOpen = Test-PortOpen -Port $webmapPort
    $webmapHttp = if ($webmapPortOpen) { Get-HttpStatus -Url $webmapUrl } else { @{ ok = $false; error = "port closed" } }

    $companionPortOpen = Test-PortOpen -Port $companionPort
    $companionHttp = if ($companionPortOpen) { Get-HttpStatus -Url $companionHealth } else { @{ ok = $false; error = "port closed" } }

    $state = @{
        project = "PW Companion"
        webmap = @{
            running = $webmapPortOpen -and $webmapHttp.ok
            url = $webmapUrl
            port = $webmapPort
            httpStatus = $webmapHttp.statusCode
            lastError = if (-not $webmapHttp.ok) { $webmapHttp.error } else { $null }
        }
        companion = @{
            running = $companionPortOpen -and $companionHttp.ok
            wsUrl = "ws://127.0.0.1:$companionPort/"
            healthUrl = $companionHealth
            port = $companionPort
            httpStatus = $companionHttp.statusCode
            lastError = if (-not $companionHttp.ok) { $companionHttp.error } else { $null }
        }
        logTail = @(Get-TailLog -Lines 15)
        agentHints = @(
            "Run: powershell -File scripts/status.ps1 to refresh health snapshot"
            "Run: powershell -File scripts/dev.ps1 -Target webmap to start map in foreground"
            "Run: powershell -File scripts/test-all.ps1 for full test suite"
            "Web map URL: http://127.0.0.1:5173/ (offline demo auto-starts)"
        )
    }

    Write-DevState -State $state
    return $state
}
