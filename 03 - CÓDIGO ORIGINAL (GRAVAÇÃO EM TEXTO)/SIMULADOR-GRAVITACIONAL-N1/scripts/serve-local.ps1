$ErrorActionPreference = 'Stop'
$projectDirectory = Split-Path -Parent $PSScriptRoot
$runtimeDirectory = Join-Path $projectDirectory '.local-runtime'
$applicationDirectory = Join-Path $runtimeDirectory 'app'
$applicationFile = Join-Path $applicationDirectory 'TrabalhoN1.Web.dll'
$stopFile = Join-Path $runtimeDirectory 'stop.request'
$supervisorLog = Join-Path $runtimeDirectory 'supervisor.log'
$supervisorLock = $null
$serverProcess = $null

function Write-SupervisorLog([string]$message) {
    Add-Content -LiteralPath $supervisorLog -Value ('{0:u} {1}' -f [DateTime]::Now, $message)
}

function Test-LocalApplication {
    try {
        $health = Invoke-RestMethod -Uri 'http://localhost:5080/api/health' -TimeoutSec 2
        return $health.status -eq 'ok' -and $health.application -eq 'TrabalhoN1' -and
            [string]::Equals($health.storagePath, $projectDirectory, [StringComparison]::OrdinalIgnoreCase)
    }
    catch { return $false }
}

try {
    New-Item -ItemType Directory -Path $runtimeDirectory -Force | Out-Null
    try {
        $supervisorLock = [System.IO.File]::Open((Join-Path $runtimeDirectory 'supervisor.lock'), 'OpenOrCreate', 'ReadWrite', 'None')
    }
    catch [System.IO.IOException] { exit 0 }
    [System.IO.File]::WriteAllText((Join-Path $runtimeDirectory 'supervisor.pid'), $PID.ToString())
    Write-SupervisorLog 'Supervisor local iniciado.'

    while (-not (Test-Path -LiteralPath $stopFile)) {
        if ($null -ne $serverProcess) {
            $serverProcess.Refresh()
            if ($serverProcess.HasExited) {
                Write-SupervisorLog ('Servidor encerrou (codigo {0}). Reiniciando.' -f $serverProcess.ExitCode)
                $serverProcess.Dispose()
                $serverProcess = $null
                Start-Sleep -Milliseconds 1000
            }
        }
        if ($null -eq $serverProcess) {
            if (Test-LocalApplication) { Start-Sleep -Milliseconds 1000; continue }
            $serverArguments = @(('"' + $applicationFile + '"'), '--contentRoot', ('"' + $applicationDirectory + '"'), '--UniverseStoragePath', ('"' + $projectDirectory + '"'))
            $serverProcess = Start-Process -FilePath 'dotnet' -ArgumentList $serverArguments -WorkingDirectory $applicationDirectory -WindowStyle Hidden -RedirectStandardOutput (Join-Path $runtimeDirectory 'server.log') -RedirectStandardError (Join-Path $runtimeDirectory 'server-error.log') -PassThru
            [System.IO.File]::WriteAllText((Join-Path $runtimeDirectory 'server.pid'), $serverProcess.Id.ToString())
            Write-SupervisorLog ('Servidor iniciado. PID {0}.' -f $serverProcess.Id)
        }
        Start-Sleep -Milliseconds 500
    }
    Write-SupervisorLog 'Encerramento solicitado pelo usuario.'
}
catch {
    Write-SupervisorLog $_.Exception.Message
}
finally {
    if ($null -ne $serverProcess) {
        $serverProcess.Refresh()
        if (-not $serverProcess.HasExited) {
            $serverProcess.Kill()
            $serverProcess.WaitForExit(5000) | Out-Null
        }
        $serverProcess.Dispose()
    }
    if ($null -ne $supervisorLock) { $supervisorLock.Dispose() }
}
