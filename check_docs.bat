@echo off
powershell -ExecutionPolicy Bypass -NoProfile -Command ^
"$root='C:\PROJETOS\SimulDIESEL\new_docs'; ^
$report=\"$env:USERPROFILE\Desktop\SimulDiesel_new_docs_check.txt\"; ^
$files=Get-ChildItem $root -Recurse -File -Filter *.md; ^
$lines=@(); ^
$lines+='SimulDIESEL - Verificacao docs'; ^
$lines+='Data: '+(Get-Date); ^
$lines+=''; ^
$lines+='Arquivos encontrados:'; ^
foreach($f in $files){$lines+=$f.FullName.Replace($root,'')}; ^
$lines+=''; ^
$lines+='Total de arquivos .md: '+$files.Count; ^
[System.IO.File]::WriteAllLines($report,$lines); ^
Write-Host 'Relatorio gerado em:' $report"
pause
