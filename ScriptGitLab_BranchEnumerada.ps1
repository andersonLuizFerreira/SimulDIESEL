#requires -Version 7.0

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$script:Remote = "origin"
$script:Branch = "main"

function Pause-Here {
    Write-Host ""
    Read-Host "Pressione ENTER para continuar"
}

function Require-Repo {
    if (-not (Test-Path -LiteralPath ".git")) {
        Write-Host ""
        Write-Host "[ERRO] Esta pasta nao parece ser um repositorio Git." -ForegroundColor Red
        Pause-Here
        return $false
    }
    return $true
}

function Run-Git {
    param([Parameter(Mandatory)][string[]]$Args)

    $cmd = "git " + ($Args -join " ")
    Write-Host ""
    Write-Host ">>> $cmd" -ForegroundColor Cyan

    & git @Args
    if ($LASTEXITCODE -ne 0) {
        throw "Falha ao executar: $cmd"
    }
}

function Get-RemoteBranches {
    $branches = & git branch -r | ForEach-Object { $_.Trim() } |
                Where-Object { $_ -notmatch "HEAD" }
    return $branches
}

function Ensure-Branch-CheckedOut {
    $current = (& git rev-parse --abbrev-ref HEAD).Trim()
    if ($current -eq $script:Branch) { return }

    Run-Git @("checkout", $script:Branch)
}

function Op-SetRemoteBranch {

    if (-not (Require-Repo)) { return }

    Run-Git @("fetch", $script:Remote)

    $branches = Get-RemoteBranches

    if ($branches.Count -eq 0) {
        Write-Host "Nenhuma branch remota encontrada." -ForegroundColor Red
        Pause-Here
        return
    }

    Write-Host ""
    Write-Host "Selecione a branch remota:" -ForegroundColor Cyan

    for ($i = 0; $i -lt $branches.Count; $i++) {
        Write-Host ("{0}) {1}" -f ($i+1), $branches[$i])
    }

    Write-Host ""

    $choice = Read-Host "Digite o numero da branch"

    if (-not [int]::TryParse($choice, [ref]$null)) {
        Write-Host "Opcao invalida." -ForegroundColor Red
        Pause-Here
        return
    }

    $index = [int]$choice - 1

    if ($index -lt 0 -or $index -ge $branches.Count) {
        Write-Host "Opcao fora do intervalo." -ForegroundColor Red
        Pause-Here
        return
    }

    $selected = $branches[$index]

    $script:Branch = $selected.Replace("$script:Remote/", "")

    Write-Host ""
    Write-Host "Branch configurada: $script:Remote/$script:Branch" -ForegroundColor Green
    Pause-Here
}

function Op-Status {
    if (-not (Require-Repo)) { return }
    Run-Git @("status")
    Pause-Here
}

function Op-Push {
    if (-not (Require-Repo)) { return }
    Ensure-Branch-CheckedOut
    Run-Git @("push", "-u", $script:Remote, $script:Branch)
    Pause-Here
}

function Op-Pull {
    if (-not (Require-Repo)) { return }
    Ensure-Branch-CheckedOut
    Run-Git @("pull", "--rebase", $script:Remote, $script:Branch)
    Pause-Here
}

function Op-ResetHard {
    if (-not (Require-Repo)) { return }
    Ensure-Branch-CheckedOut

    Write-Host "ATENCAO: isto descarta alteracoes locais." -ForegroundColor Red
    $confirm = Read-Host "Confirmar RESET HARD? (y/N)"
    if ($confirm -ne "y") { return }

    Run-Git @("fetch", $script:Remote)
    Run-Git @("reset", "--hard", "$script:Remote/$script:Branch")
    Pause-Here
}

function Op-ForcePush {
    if (-not (Require-Repo)) { return }
    Ensure-Branch-CheckedOut

    Write-Host "ATENCAO: isto sobrescreve o remoto." -ForegroundColor Red
    $confirm = Read-Host "Confirmar FORCE PUSH? (y/N)"
    if ($confirm -ne "y") { return }

    Run-Git @("push", "--force-with-lease", $script:Remote, $script:Branch)
    Pause-Here
}

function Op-ListBranches {
    if (-not (Require-Repo)) { return }
    Run-Git @("branch")
    Run-Git @("branch", "-r")
    Run-Git @("branch", "-a")
    Pause-Here
}

while ($true) {

    Clear-Host
    Write-Host "==============================================="
    Write-Host " SimulDIESEL Git Sync"
    Write-Host " Pasta: $(Get-Location)"
    Write-Host " Alvo:  $script:Remote/$script:Branch"
    Write-Host "==============================================="
    Write-Host "1) Push              - Envia commits para o remoto"
    Write-Host "2) Pull              - Atualiza branch com rebase"
    Write-Host "3) Reset HARD        - Alinha local ao remoto"
    Write-Host "4) Force Push        - Sobrescreve remoto"
    Write-Host "5) Status            - Mostra estado atual"
    Write-Host "6) Configurar Branch - Escolher branch remota"
    Write-Host "7) Listar Branches   - Exibe locais e remotas"
    Write-Host "8) Sair              - Encerra o script"
    Write-Host ""

    $opt = Read-Host "Opcao"

    switch ($opt) {
        "1" { Op-Push }
        "2" { Op-Pull }
        "3" { Op-ResetHard }
        "4" { Op-ForcePush }
        "5" { Op-Status }
        "6" { Op-SetRemoteBranch }
        "7" { Op-ListBranches }
        "8" { break }
        default { Pause-Here }
    }
}
