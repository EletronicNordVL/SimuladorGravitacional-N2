param([switch]$NoBrowser)

$ErrorActionPreference = 'Stop'
$projectDirectory = $PSScriptRoot
$localUrl = 'http://localhost:5080'
$projectFile = Join-Path $projectDirectory 'Web\TrabalhoN1.Web.csproj'
$runtimeDirectory = Join-Path $projectDirectory '.local-runtime'
$applicationDirectory = Join-Path $runtimeDirectory 'app'
$applicationFile = Join-Path $applicationDirectory 'TrabalhoN1.Web.dll'
$supervisorScript = Join-Path $projectDirectory 'scripts\serve-local.ps1'
$startupLock = $null

function Test-LocalApplication {
    try {
        $health = Invoke-RestMethod -Uri "$localUrl/api/health" -TimeoutSec 2
        return $health.status -eq 'ok' -and $health.application -eq 'TrabalhoN1' -and
            [string]::Equals($health.storagePath, $projectDirectory, [StringComparison]::OrdinalIgnoreCase)
    }
    catch { return $false }
}

function Test-Supervisor {
    $pidFile = Join-Path $runtimeDirectory 'supervisor.pid'
    if (-not (Test-Path -LiteralPath $pidFile)) { return $false }
    try {
        $supervisorId = [int][System.IO.File]::ReadAllText($pidFile)
        $supervisor = Get-CimInstance Win32_Process -Filter "ProcessId = $supervisorId"
        return $null -ne $supervisor -and $supervisor.Name -eq 'powershell.exe' -and
            $supervisor.CommandLine.Contains($supervisorScript)
    }
    catch { return $false }
}

try {
    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
        throw 'O SDK .NET 10 precisa estar instalado para preparar o projeto.'
    }
    New-Item -ItemType Directory -Path $runtimeDirectory -Force | Out-Null
    for ($attempt = 0; $attempt -lt 120; $attempt++) {
        try {
            $startupLock = [System.IO.File]::Open((Join-Path $runtimeDirectory 'startup.lock'), 'OpenOrCreate', 'ReadWrite', 'None')
            break
        }
        catch [System.IO.IOException] { Start-Sleep -Milliseconds 500 }
    }
    if ($null -eq $startupLock) { throw 'Outra inicializacao esta em andamento. Tente novamente em alguns segundos.' }

    if (-not (Test-Path -LiteralPath $applicationFile)) {
        Write-Host 'Preparando o simulador gravitacional local...'
        & dotnet publish $projectFile -c Release --nologo -o $applicationDirectory
        if ($LASTEXITCODE -ne 0) { throw 'A compilacao falhou. Consulte as mensagens acima.' }
    }

    if (-not (Test-Supervisor)) {
        $stopFile = Join-Path $runtimeDirectory 'stop.request'
        if (Test-Path -LiteralPath $stopFile) { Remove-Item -LiteralPath $stopFile }
        $supervisorArguments = @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', ('"' + $supervisorScript + '"'))
        Start-Process -FilePath 'powershell.exe' -ArgumentList $supervisorArguments -WorkingDirectory $projectDirectory -WindowStyle Hidden | Out-Null
    }

    $ready = $false
    for ($attempt = 0; $attempt -lt 60; $attempt++) {
        if (Test-LocalApplication) { $ready = $true; break }
        Start-Sleep -Milliseconds 500
    }
    if (-not $ready) {
        throw 'O servidor nao respondeu na porta 5080. Consulte .local-runtime\supervisor.log e server-error.log.'
    }
    Write-Host "Simulador pronto em $localUrl. O servidor continua ativo em segundo plano."
    if (-not $NoBrowser) { Start-Process $localUrl }
}
catch {
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}
finally {
    if ($null -ne $startupLock) { $startupLock.Dispose() }
}
