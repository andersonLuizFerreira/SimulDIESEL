#requires -Version 7.0
<#
ScriptGitLab.ps1 - SimulDIESEL Git Sync (PowerShell 7)

Recursos:
- Abre já na branch ativa (detecta automaticamente).
- Push / Pull (rebase) / Reset HARD / Force-with-lease / Status detalhado.
- Configurar branch por lista enumerada (sem erro de digitação).
- Listar branches locais e remotas.
- Mostra ahead/behind (quando possível) e últimos commits.
- Proteção: bloqueia Force Push em branches protegidas e exige confirmação extra em Push/Reset.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# -----------------------------
# Configurações
# -----------------------------
$script:Remote = "origin"

# Branches "protegidas" (regras locais do script).
# Ajuste se quiser: ex. @("main","master","develop","release")
$script:ProtectedBranches = @("main", "master")

# Quantidade de commits mostrados no cabeçalho (últimos commits)
$script:RecentCommitsToShow = 5

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

function Try-Git {
    param(
        [Parameter(Mandatory)][string[]]$Args
    )
    # Retorna texto; não explode em erro (usado para consultas)
    $out = & git @Args 2>$null
    return $out
}

function Get-CurrentBranch {
    if (-not (Test-Path -LiteralPath ".git")) { return "main" }
    $b = Try-Git @("rev-parse", "--abbrev-ref", "HEAD")
    if ([string]::IsNullOrWhiteSpace($b)) { return "main" }
    return ($b | Select-Object -First 1).Trim()
}

function Has-UncommittedChanges {
    $out = Try-Git @("status", "--porcelain")
    return -not [string]::IsNullOrWhiteSpace(($out | Out-String).Trim())
}

function Confirm {
    param([Parameter(Mandatory)][string]$Message)
    $ans = Read-Host $Message
    return ($ans -eq "y" -or $ans -eq "yes" -or $ans -eq "Y" -or $ans -eq "YES")
}

function Is-ProtectedBranch {
    param([Parameter(Mandatory)][string]$BranchName)
    return $script:ProtectedBranches -contains $BranchName
}

function Get-UpstreamRef {
    # retorna ex.: "origin/main" ou "" se nao tiver
    $up = Try-Git @("rev-parse", "--abbrev-ref", "--symbolic-full-name", "@{u}")
    if ([string]::IsNullOrWhiteSpace(($up | Out-String).Trim())) { return "" }
    return ($up | Select-Object -First 1).Trim()
}

function Get-AheadBehind {
    # Retorna objeto com Ahead/Behind quando possível; senão null.
    $up = Get-UpstreamRef
    if ([string]::IsNullOrWhiteSpace($up)) {
        return $null
    }

    # git rev-list --left-right --count UPSTREAM...HEAD  => "behind ahead"
    $counts = Try-Git @("rev-list", "--left-right", "--count", "$up...HEAD")
    $line = ($counts | Out-String).Trim()
    if ([string]::IsNullOrWhiteSpace($line)) { return $null }

    $parts = $line -split "\s+"
    if ($parts.Count -lt 2) { return $null }

    return [pscustomobject]@{
        Upstream = $up
        Behind   = [int]$parts[0]
        Ahead    = [int]$parts[1]
    }
}

function Show-RecentCommits {
    param([int]$Count = 5)

    $log = Try-Git @("log", "--oneline", "-n", "$Count")
    $txt = ($log | Out-String).Trim()
    if ([string]::IsNullOrWhiteSpace($txt)) {
        Write-Host "Sem commits para mostrar." -ForegroundColor DarkGray
        return
    }

    $log | ForEach-Object { Write-Host $_ }
}

function Ensure-Branch-CheckedOut {
    param([Parameter(Mandatory)][string]$TargetBranch)

    $current = Get-CurrentBranch
    if ($current -eq $TargetBranch) { return }

    # Se branch local não existe, tenta criar rastreando remoto
    & git show-ref --verify --quiet "refs/heads/$TargetBranch"
    if ($LASTEXITCODE -ne 0) {
        Run-Git @("fetch", $script:Remote)
        & git show-ref --verify --quiet "refs/remotes/$script:Remote/$TargetBranch"
        if ($LASTEXITCODE -ne 0) {
            throw "Branch remoto '$script:Remote/$TargetBranch' nao encontrado."
        }
        Run-Git @("checkout", "-b", $TargetBranch, "$script:Remote/$TargetBranch")
    } else {
        Run-Git @("checkout", $TargetBranch)
    }
}

function Ensure-Upstream {
    param([Parameter(Mandatory)][string]$BranchName)

    $up = Get-UpstreamRef
    if (-not [string]::IsNullOrWhiteSpace($up)) { return }

    # Tenta setar upstream automaticamente para origin/<branch>
    & git show-ref --verify --quiet "refs/remotes/$script:Remote/$BranchName"
    if ($LASTEXITCODE -eq 0) {
        Run-Git @("branch", "--set-upstream-to", "$script:Remote/$BranchName", $BranchName)
    }
}

function Get-RemoteBranches {
    Run-Git @("fetch", $script:Remote)
    $branches = Try-Git @("branch", "-r") | ForEach-Object { $_.Trim() } | Where-Object { $_ -notmatch "HEAD" }
    # Converte para array estável
    return @($branches)
}

function Header-Info {
    $branch = Get-CurrentBranch
    $dirty  = Has-UncommittedChanges
    $prot   = Is-ProtectedBranch -BranchName $branch
    $up     = Get-UpstreamRef
    $ab     = Get-AheadBehind

    $statusLine = if ($dirty) { "DIRTY" } else { "LIMPO" }
    $protLine   = if ($prot)  { "PROTEGIDA" } else { "NORMAL" }

    Write-Host "==============================================="
    Write-Host " SimulDIESEL Git Sync"
    Write-Host " Pasta:        $(Get-Location)"
    Write-Host " Branch atual: $branch  [$protLine]"
    Write-Host " Estado:       $statusLine"
    if (-not [string]::IsNullOrWhiteSpace($up)) {
        Write-Host " Upstream:     $up"
    } else {
        Write-Host " Upstream:     (nao configurado)"
    }

    if ($null -ne $ab) {
        Write-Host (" Ahead/Behind: +{0} / -{1}" -f $ab.Ahead, $ab.Behind)
    } else {
        Write-Host " Ahead/Behind: (indisponivel)"
    }

    Write-Host " Remote:       $script:Remote"
    Write-Host "==============================================="

    Write-Host " Ultimos commits:" -ForegroundColor DarkGray
    Show-RecentCommits -Count $script:RecentCommitsToShow
    Write-Host "-----------------------------------------------" -ForegroundColor DarkGray
}

# -----------------------------
# Operations
# -----------------------------

function Op-Status {
    if (-not (Require-Repo)) { return }

    Write-Host ""
    Write-Host "===============================" -ForegroundColor DarkGray
    Write-Host " STATUS" -ForegroundColor Cyan
    Write-Host "===============================" -ForegroundColor DarkGray

    Run-Git @("status")
    $ab = Get-AheadBehind
    if ($null -ne $ab) {
        Write-Host ""
        Write-Host ("Ahead/Behind vs {0}: +{1} / -{2}" -f $ab.Upstream, $ab.Ahead, $ab.Behind) -ForegroundColor Green
    } else {
        Write-Host ""
        Write-Host "Ahead/Behind: indisponivel (sem upstream configurado)." -ForegroundColor Yellow
    }

    Write-Host ""
    Write-Host "Ultimos commits:" -ForegroundColor DarkGray
    Show-RecentCommits -Count $script:RecentCommitsToShow

    Pause-Here
}

function Op-Push {
    if (-not (Require-Repo)) { return }

    $branch = Get-CurrentBranch
    Ensure-Upstream -BranchName $branch

    # Confirmação extra em branch protegida
    if (Is-ProtectedBranch -BranchName $branch) {
        Write-Host ""
        Write-Host "[AVISO] Voce esta em uma branch PROTEGIDA ($branch)." -ForegroundColor Yellow
        if (-not (Confirm "Confirmar PUSH nesta branch? (y/N)")) { return }
    }

    Run-Git @("push", "-u", $script:Remote, $branch)
    Pause-Here
}

function Op-Pull {
    if (-not (Require-Repo)) { return }

    $branch = Get-CurrentBranch
    Ensure-Upstream -BranchName $branch

    Run-Git @("pull", "--rebase", $script:Remote, $branch)
    Pause-Here
}

function Op-ResetHard {
    if (-not (Require-Repo)) { return }

    $branch = Get-CurrentBranch

    Write-Host ""
    Write-Host "ATENCAO: Reset HARD descarta alteracoes locais." -ForegroundColor Red
    Write-Host "Alvo: $script:Remote/$branch" -ForegroundColor Red

    # Confirmação extra em branch protegida
    if (Is-ProtectedBranch -BranchName $branch) {
        Write-Host ""
        Write-Host "[AVISO] Branch PROTEGIDA ($branch): confirmacao dupla exigida." -ForegroundColor Yellow
        if (-not (Confirm "Confirmar RESET HARD na PROTEGIDA? (y/N)")) { return }
        $type = Read-Host "Digite exatamente o nome da branch para confirmar: $branch"
        if ($type -ne $branch) {
            Write-Host "Confirmacao falhou. Operacao cancelada." -ForegroundColor Red
            Pause-Here
            return
        }
    } else {
        if (-not (Confirm "Confirmar RESET HARD? (y/N)")) { return }
    }

    Run-Git @("fetch", $script:Remote)
    Run-Git @("reset", "--hard", "$script:Remote/$branch")
    Pause-Here
}

function Op-ForcePush {
    if (-not (Require-Repo)) { return }

    $branch = Get-CurrentBranch

    if (Is-ProtectedBranch -BranchName $branch) {
        Write-Host ""
        Write-Host "[BLOQUEADO] Force Push nao permitido em branch protegida ($branch)." -ForegroundColor Red
        Pause-Here
        return
    }

    Write-Host ""
    Write-Host "ATENCAO: isto sobrescreve o remoto $script:Remote/$branch (with-lease)." -ForegroundColor Red
    if (-not (Confirm "Confirmar FORCE PUSH? (y/N)")) { return }

    Run-Git @("push", "--force-with-lease", $script:Remote, $branch)
    Pause-Here
}

function Op-SetBranchByList {
    if (-not (Require-Repo)) { return }

    $branches = Get-RemoteBranches
    if ($branches.Count -eq 0) {
        Write-Host "Nenhuma branch remota encontrada." -ForegroundColor Red
        Pause-Here
        return
    }

    Write-Host ""
    Write-Host "Selecione a branch remota (lista do $script:Remote):" -ForegroundColor Cyan

    for ($i = 0; $i -lt $branches.Count; $i++) {
        $name = $branches[$i]
        $clean = $name.Replace("$script:Remote/", "")
        $tag = if (Is-ProtectedBranch -BranchName $clean) { " [PROTEGIDA]" } else { "" }
        Write-Host ("{0}) {1}{2}" -f ($i + 1), $name, $tag)
    }

    Write-Host ""
    $choice = Read-Host "Digite o numero da branch (ENTER cancela)"
    if ([string]::IsNullOrWhiteSpace($choice)) { return }

    $n = 0
    if (-not [int]::TryParse($choice, [ref]$n)) {
        Write-Host "Opcao invalida." -ForegroundColor Red
        Pause-Here
        return
    }

    $index = $n - 1
    if ($index -lt 0 -or $index -ge $branches.Count) {
        Write-Host "Opcao fora do intervalo." -ForegroundColor Red
        Pause-Here
        return
    }

    $selectedRemote = $branches[$index]
    $selectedLocal  = $selectedRemote.Replace("$script:Remote/", "")

    # Troca efetivamente para a branch escolhida (cria local se preciso)
    Ensure-Branch-CheckedOut -TargetBranch $selectedLocal
    Ensure-Upstream -BranchName $selectedLocal

    Write-Host ""
    Write-Host "Agora em: $selectedLocal (upstream: $script:Remote/$selectedLocal)" -ForegroundColor Green
    Pause-Here
}

function Op-ListBranches {
    if (-not (Require-Repo)) { return }

    Write-Host ""
    Write-Host "===============================" -ForegroundColor DarkGray
    Write-Host " BRANCHES LOCAIS" -ForegroundColor Cyan
    Write-Host "===============================" -ForegroundColor DarkGray
    Run-Git @("branch")

    Write-Host ""
    Write-Host "===============================" -ForegroundColor DarkGray
    Write-Host " BRANCHES REMOTAS" -ForegroundColor Yellow
    Write-Host "===============================" -ForegroundColor DarkGray
    Run-Git @("branch", "-r")

    Write-Host ""
    Write-Host "===============================" -ForegroundColor DarkGray
    Write-Host " TODAS (local + remoto)" -ForegroundColor Green
    Write-Host "===============================" -ForegroundColor DarkGray
    Run-Git @("branch", "-a")

    Pause-Here
}

# -----------------------------
# Init: abrir já na branch ativa
# -----------------------------
$null = Get-CurrentBranch  # apenas garante que git existe/retorna algo

# -----------------------------
# Menu
# -----------------------------
while ($true) {
    Clear-Host
    Header-Info

    Write-Host "1) Push              - Envia commits para o remoto"
    Write-Host "2) Pull              - Atualiza branch com rebase"
    Write-Host "3) Reset HARD        - Alinha local ao remoto (descarta alteracoes)"
    Write-Host "4) Force Push        - Sobrescreve remoto (with-lease) [bloqueado em protegidas]"
    Write-Host "5) Status            - Mostra estado atual + ahead/behind + ultimos commits"
    Write-Host "6) Trocar Branch     - Escolhe branch remota (enumerada) e faz checkout"
    Write-Host "7) Listar Branches   - Exibe locais e remotas"
    Write-Host "8) Sair              - Encerra o script"
    Write-Host ""

    $opt = Read-Host "Opcao"

    try {
        switch ($opt) {
            "1" { Op-Push }
            "2" { Op-Pull }
            "3" { Op-ResetHard }
            "4" { Op-ForcePush }
            "5" { Op-Status }
            "6" { Op-SetBranchByList }
            "7" { Op-ListBranches }
            "8" { break }
            default { Pause-Here }
        }
    }
    catch {
        Write-Host ""
        Write-Host "[ERRO] $($_.Exception.Message)" -ForegroundColor Red
        Pause-Here
    }
}
