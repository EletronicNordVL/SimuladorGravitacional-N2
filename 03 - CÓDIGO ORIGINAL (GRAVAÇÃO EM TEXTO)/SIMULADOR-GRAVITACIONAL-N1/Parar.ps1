$ErrorActionPreference = 'Stop'
$runtimeDirectory = Join-Path $PSScriptRoot '.local-runtime'
$supervisor = $null
$scriptPath = Join-Path $PSScriptRoot 'scripts\serve-local.ps1'
try {
    New-Item -ItemType Directory -Path $runtimeDirectory -Force | Out-Null
    [System.IO.File]::WriteAllText((Join-Path $runtimeDirectory 'stop.request'), 'stop')
    for ($attempt = 0; $attempt -lt 30; $attempt++) {
        $pidFile = Join-Path $runtimeDirectory 'supervisor.pid'
        if (-not (Test-Path -LiteralPath $pidFile)) { break }
        $supervisorId = [int][System.IO.File]::ReadAllText($pidFile)
        $supervisor = Get-CimInstance Win32_Process -Filter "ProcessId = $supervisorId"
        if ($null -eq $supervisor -or -not $supervisor.CommandLine.Contains($scriptPath)) {
            Write-Host 'Servidor local encerrado.'
            exit 0
        }
        Start-Sleep -Milliseconds 500
    }
    if ($null -ne $supervisor -and $supervisor.CommandLine.Contains($scriptPath)) {
        throw 'O encerramento ainda esta em andamento. Tente novamente em alguns segundos.'
    }
    Write-Host 'Servidor local encerrado.'
}
catch {
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}
