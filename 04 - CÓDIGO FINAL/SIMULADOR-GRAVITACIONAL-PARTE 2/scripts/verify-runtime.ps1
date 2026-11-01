$ErrorActionPreference = 'Stop'
$projectDirectory = Split-Path -Parent $PSScriptRoot
$expectedApplication = Join-Path $projectDirectory '.local-runtime\app\TrabalhoN1.Web.dll'
$supervisorScript = Join-Path $PSScriptRoot 'serve-local.ps1'
$checks = 0

function Assert-Runtime([bool]$condition, [string]$message) {
    if (-not $condition) { throw $message }
    $script:checks++
    Write-Host "OK $message"
}

function Get-Health {
    Invoke-RestMethod -Uri 'http://localhost:5080/api/health' -TimeoutSec 2
}

function Wait-Healthy([int]$previousProcessId = 0) {
    for ($attempt = 0; $attempt -lt 80; $attempt++) {
        try {
            $health = Get-Health
            if ($health.status -eq 'ok' -and $health.application -eq 'TrabalhoN1' -and $health.processId -ne $previousProcessId) { return $health }
        }
        catch { }
        Start-Sleep -Milliseconds 250
    }
    throw 'O servidor nao se recuperou no intervalo esperado.'
}

try {
    $originalHealth = Get-Health
    Assert-Runtime ([string]::Equals($originalHealth.storagePath, $projectDirectory, [StringComparison]::OrdinalIgnoreCase)) 'Universos salvos na pasta original do projeto'
    $originalProcess = Get-CimInstance Win32_Process -Filter "ProcessId = $($originalHealth.processId)"
    Assert-Runtime ($null -ne $originalProcess -and $originalProcess.CommandLine.Contains($expectedApplication)) 'Servidor usa a aplicacao publicada deste projeto'
    $supervisor = Get-CimInstance Win32_Process -Filter "ProcessId = $($originalProcess.ParentProcessId)"
    Assert-Runtime ($null -ne $supervisor -and $supervisor.CommandLine.Contains($supervisorScript)) 'Servidor pertence ao supervisor local'

    & (Join-Path $projectDirectory 'Iniciar.ps1') -NoBrowser
    Assert-Runtime ((Get-Health).processId -eq $originalHealth.processId) 'Abrir o atalho novamente reutiliza o servidor'

    # Apenas o processo identificado acima e encerrado para simular uma falha.
    Stop-Process -Id $originalProcess.ProcessId -ErrorAction Stop
    $recovered = Wait-Healthy $originalHealth.processId
    Assert-Runtime ($recovered.processId -ne $originalHealth.processId) 'Supervisor reinicia o servidor apos encerramento inesperado'
    $response = Invoke-RestMethod -Uri 'http://localhost:5080/api/universes/create' -Method Post -ContentType 'application/json' -Body '{"count":3}' -TimeoutSec 5
    Assert-Runtime ($response.corpos.Count -eq 3) 'Funcionalidades continuam disponiveis apos recuperacao'

    & (Join-Path $projectDirectory 'Parar.ps1')
    $stopped = $false
    try { $null = Get-Health } catch { $stopped = $true }
    Assert-Runtime $stopped 'Atalho Parar encerra o servidor e evita reinicializacao automatica'
    & (Join-Path $projectDirectory 'Iniciar.ps1') -NoBrowser
    $restarted = Wait-Healthy
    Assert-Runtime ($restarted.processId -ne $recovered.processId) 'Atalho Iniciar volta a disponibilizar o localhost'
    $supervisors = @(Get-CimInstance Win32_Process -Filter "Name = 'powershell.exe'" | Where-Object { $null -ne $_.CommandLine -and $_.CommandLine.Contains($supervisorScript) })
    Assert-Runtime ($supervisors.Count -eq 1) 'Existe apenas um supervisor deste projeto'
    for ($attempt = 0; $attempt -lt 5; $attempt++) {
        Start-Sleep -Seconds 2
        Assert-Runtime ((Get-Health).processId -eq $restarted.processId) 'Servidor permanece ativo e responde ao HTTP'
    }
    Write-Host "`n$checks verificacoes do servidor passaram. O localhost foi deixado em funcionamento."
}
catch {
    # Recupera o servidor mesmo se uma verificacao falhar.
    & (Join-Path $projectDirectory 'Iniciar.ps1') -NoBrowser
    throw
}
