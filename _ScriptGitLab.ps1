#requires -Version 7.0
<#
ScriptGitLab.ps1 - SimulDIESEL Git Sync (PowerShell 7)

- Monorepo: cloud / local-api / hardware
- Default: origin/main
- Menu: Push / Pull (rebase) / Reset HARD / Force-with-lease / Status
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# -----------------------------
# Defaults (ajuste aqui se quiser)
# -----------------------------
$Remote = "origin"
$Branch = "main"

# -----------------------------
# Helpers
# -----------------------------
function Pause-Here {
    Write-Host ""
    Read-Host "Pressione ENTER para continuar"
}

function Require-Repo {
    if (-not (Test-Path -LiteralPath ".git")) {
        Write-Host ""
        Write-Host "[ERRO] Esta pasta nao parece ser um repositorio Git (nao encontrei .git)." -ForegroundColor Red
        Write-Host "Entre na pasta correta do repo (ex.: C:\PROJETOS\SimulDIESEL) e execute novamente."
        Pause-Here
        return $false
    }
    return $true
}

function Run-Git {
    param(
        [Parameter(Mandatory)][string[]]$Args
    )
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
    # garante que o branch local existe e está checked out
    $current = (& git rev-parse --abbrev-ref HEAD).Trim()

    if ($current -eq $Branch) {
        return
    }

    Write-Host ""
    Write-Host "Voce esta no branch '$current'." -ForegroundColor Yellow
    $ans = Read-Host "Trocar para '$Branch'? (y/N)"
    if ($ans -ne "y") {
        throw "Operacao cancelada (nao mudou de branch)."
    }

    # se branch local nao existe, tenta criar do remoto
    & git show-ref --verify --quiet "refs/heads/$Branch"
    if ($LASTEXITCODE -ne 0) {
        Run-Git @("fetch", $Remote)
        & git show-ref --verify --quiet "refs/remotes/$Remote/$Branch"
        if ($LASTEXITCODE -ne 0) {
            throw "Branch remoto '$Remote/$Branch' nao encontrado."
        }
        Run-Git @("checkout", "-b", $Branch, "$Remote/$Branch")
    }
    else {
        Run-Git @("checkout", $Branch)
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
    $doCommit = Confirm "Deseja dar git add + commit antes? (y/N)"
    if (-not $doCommit) { return }

    $msg = Read-Host "Mensagem do commit (vazio = 'chore: update')"
    if ([string]::IsNullOrWhiteSpace($msg)) { $msg = "chore: update" }

    Run-Git @("add", "-A")
    Run-Git @("commit", "-m", $msg)
}

function Handle-Merge-In-Progress {
    $mergeHead = Join-Path ".git" "MERGE_HEAD"
    if (Test-Path -LiteralPath $mergeHead) {
        Write-Host ""
        Write-Host "[AVISO] Merge em andamento detectado." -ForegroundColor Yellow
        $abort = Confirm "Abortar o merge agora? (y/N)"
        if (-not $abort) {
            throw "Operacao cancelada: finalize ou aborte o merge manualmente."
        }
        try {
            Run-Git @("merge", "--abort")
        } catch {
            # fallback
            Run-Git @("reset", "--merge")
        }
    }
}

# -----------------------------
# Operations
# -----------------------------
function Op-Status {
    if (-not (Require-Repo)) { return }
    Handle-Merge-In-Progress
    Run-Git @("status")
    Pause-Here
}

function Op-Push {
    if (-not (Require-Repo)) { return }
    Handle-Merge-In-Progress
    Ensure-Branch-CheckedOut
    Maybe-Commit

    # Push normal para origin/main
    Run-Git @("push", "-u", $Remote, $Branch)
    Pause-Here
}

function Op-Pull {
    if (-not (Require-Repo)) { return }
    Handle-Merge-In-Progress
    Ensure-Branch-CheckedOut

    Run-Git @("pull", "--rebase", $Remote, $Branch)
    Pause-Here
}

function Op-ResetHard {
    if (-not (Require-Repo)) { return }
    Handle-Merge-In-Progress
    Ensure-Branch-CheckedOut

    Write-Host ""
    Write-Host "ATENCAO: isto descarta alteracoes locais e alinha com $Remote/$Branch." -ForegroundColor Red
    if (-not (Confirm "Confirmar RESET HARD? (y/N)")) { return }

    Run-Git @("fetch", $Remote)
    Run-Git @("reset", "--hard", "$Remote/$Branch")
    Pause-Here
}

function Op-ForcePush {
    if (-not (Require-Repo)) { return }
    Handle-Merge-In-Progress
    Ensure-Branch-CheckedOut

    Write-Host ""
    Write-Host "ATENCAO: isto sobrescreve o remoto $Remote/$Branch com seu estado local." -ForegroundColor Red
    Write-Host "Use apenas se tiver certeza e se o branch nao estiver protegido no GitLab."
    if (-not (Confirm "Confirmar FORCE PUSH (with-lease)? (y/N)")) { return }

    Run-Git @("push", "--force-with-lease", $Remote, $Branch)
    Pause-Here
}

function Op-SetRemoteBranch {
    Write-Host ""
    $r = Read-Host "Remote (vazio = '$Remote')"
    if (-not [string]::IsNullOrWhiteSpace($r)) { $Remote = $r }

    $b = Read-Host "Branch (vazio = '$Branch')"
    if (-not [string]::IsNullOrWhiteSpace($b)) { $Branch = $b }

    Write-Host ""
    Write-Host "Configurado: $Remote/$Branch" -ForegroundColor Green
    Pause-Here
}

# -----------------------------
# Main menu
# -----------------------------
while ($true) {
    Clear-Host
    Write-Host "================================================"
    Write-Host " SimulDIESEL Git Sync (PowerShell 7)"
    Write-Host " Pasta:  $(Get-Location)"
    Write-Host " Alvo:   $Remote/$Branch"
    Write-Host "================================================"
    Write-Host "1) Push (enviar)"
    Write-Host "2) Pull --rebase (receber)"
    Write-Host "3) Reset HARD (sobrescrever LOCAL)"
    Write-Host "4) Force-with-lease (sobrescrever NUVEM)"
    Write-Host "5) Status"
    Write-Host "6) Ajustar Remote/Branch"
    Write-Host "7) Sair"
    Write-Host ""

    $opt = Read-Host "Opcao [1-7]"

    try {
        switch ($opt) {
            "1" { Op-Push }
            "2" { Op-Pull }
            "3" { Op-ResetHard }
            "4" { Op-ForcePush }
            "5" { Op-Status }
            "6" { Op-SetRemoteBranch }
            "7" { break }
            default { Write-Host "Opcao invalida."; Pause-Here }
        }
    }
    catch {
        Write-Host ""
        Write-Host "[ERRO] $($_.Exception.Message)" -ForegroundColor Red
        Pause-Here
    }
}
