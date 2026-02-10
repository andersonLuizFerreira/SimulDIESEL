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

function Has-UncommittedChanges {
    $out = & git status --porcelain
    return -not [string]::IsNullOrWhiteSpace($out)
}

function Ensure-Branch-CheckedOut {

    $current = (& git rev-parse --abbrev-ref HEAD).Trim()

    if ($current -eq $script:Branch) { return }

    Write-Host ""
    Write-Host "Voce esta no branch '$current'." -ForegroundColor Yellow
    $ans = Read-Host "Trocar para '$script:Branch'? (y/N)"
    if ($ans -ne "y") { throw "Operacao cancelada." }

    & git show-ref --verify --quiet "refs/heads/$script:Branch"
    if ($LASTEXITCODE -ne 0) {
        Run-Git @("fetch", $script:Remote)

        & git show-ref --verify --quiet "refs/remotes/$script:Remote/$script:Branch"
        if ($LASTEXITCODE -ne 0) {
            throw "Branch remoto '$script:Remote/$script:Branch' nao encontrado."
        }

        Run-Git @("checkout", "-b", $script:Branch, "$script:Remote/$script:Branch")
    }
    else {
        Run-Git @("checkout", $script:Branch)
    }
}

function Confirm {
    param([Parameter(Mandatory)][string]$Message)
    $ans = Read-Host $Message
    return ($ans -eq "y" -or $ans -eq "yes")
}

function Maybe-Commit {
    if (-not (Has-UncommittedChanges)) { return }

    Write-Host ""
    Write-Host "Ha alteracoes nao commitadas." -ForegroundColor Yellow

    if (-not (Confirm "Deseja dar git add + commit antes? (y/N)")) { return }

    $msg = Read-Host "Mensagem do commit (vazio = 'chore: update')"
    if ([string]::IsNullOrWhiteSpace($msg)) { $msg = "chore: update" }

    Run-Git @("add", "-A")
    Run-Git @("commit", "-m", $msg)
}

function Op-Status {
    if (-not (Require-Repo)) { return }
    Run-Git @("status")
    Pause-Here
}

function Op-Push {
    if (-not (Require-Repo)) { return }
    Ensure-Branch-CheckedOut
    Maybe-Commit
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
    if (-not (Confirm "Confirmar RESET HARD? (y/N)")) { return }

    Run-Git @("fetch", $script:Remote)
    Run-Git @("reset", "--hard", "$script:Remote/$script:Branch")
    Pause-Here
}

function Op-ForcePush {
    if (-not (Require-Repo)) { return }
    Ensure-Branch-CheckedOut

    Write-Host "ATENCAO: isto sobrescreve o remoto." -ForegroundColor Red
    if (-not (Confirm "Confirmar FORCE PUSH? (y/N)")) { return }

    Run-Git @("push", "--force-with-lease", $script:Remote, $script:Branch)
    Pause-Here
}

function Op-SetRemoteBranch {
    $r = Read-Host "Remote (vazio = '$script:Remote')"
    if (-not [string]::IsNullOrWhiteSpace($r)) { $script:Remote = $r }

    $b = Read-Host "Branch (vazio = '$script:Branch')"
    if (-not [string]::IsNullOrWhiteSpace($b)) { $script:Branch = $b }

    Write-Host "Configurado: $script:Remote/$script:Branch" -ForegroundColor Green
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
    Write-Host "6) Configurar Branch - Define remote e branch"
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
