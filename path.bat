@echo off
setlocal EnableExtensions EnableDelayedExpansion

echo.
echo ================================================
echo  SimulDIESEL - Criando estrutura de pastas
echo  Pasta alvo: %CD%
echo ================================================
echo.

REM Protecao: precisa existir README.md
if not exist "README.md" (
  echo [ERRO] Nao encontrei README.md na pasta atual.
  echo Rode este script na raiz do repositorio clonado.
  pause
  exit /b 1
)

REM ==================================================
REM Criar arquivos principais (placeholders)
REM ==================================================
if not exist ".editorconfig" echo # EditorConfig > ".editorconfig"
if not exist ".gitattributes" echo # gitattributes > ".gitattributes"
if not exist ".gitignore" echo # gitignore sera preenchido depois > ".gitignore"
if not exist ".gitlab-ci.yml" echo # pipeline sera preenchido depois > ".gitlab-ci.yml"
if not exist "CHANGELOG.md" echo # Changelog > "CHANGELOG.md"
if not exist "VERSIONING.md" echo # Versionamento > "VERSIONING.md"

REM ==================================================
REM Criar pastas raiz
REM ==================================================
for %%D in (docs specs tools infra cloud local-api hardware tests) do (
  if not exist "%%D" mkdir "%%D"
  if not exist "%%D\.gitkeep" type nul > "%%D\.gitkeep"
)

REM ==================================================
REM docs/
REM ==================================================
for %%D in (
  "docs\00_visao-geral"
  "docs\01_arquitetura"
  "docs\02_instalacao"
  "docs\03_operacao"
  "docs\04_desenvolvimento"
  "docs\05_hardware"
  "docs\images"
) do (
  if not exist %%~D mkdir %%~D
  if not exist %%~D\.gitkeep type nul > %%~D\.gitkeep
)

REM ==================================================
REM specs/
REM ==================================================
for %%D in (
  "specs\requirements"
  "specs\adr"
  "specs\protocols"
  "specs\data-models"
) do (
  if not exist %%~D mkdir %%~D
  if not exist %%~D\.gitkeep type nul > %%~D\.gitkeep
)

echo # Protocolo: Local API -> ESP32 > "specs\protocols\local-api_to_esp32.md"
echo # Protocolo: ESP32 -> Arduino DUE > "specs\protocols\esp32_to_due.md"
echo # Protocolo: ESP32 -> Arduino MEGA > "specs\protocols\esp32_to_mega.md"

REM ==================================================
REM tools/
REM ==================================================
for %%D in ("tools\build" "tools\flash" "tools\release" "tools\dev") do (
  if not exist %%~D mkdir %%~D
  if not exist %%~D\.gitkeep type nul > %%~D\.gitkeep
)

REM ==================================================
REM infra/
REM ==================================================
for %%D in (
  "infra\cloud"
  "infra\cloud\docker"
  "infra\cloud\pipelines"
  "infra\local"
  "infra\local\installers"
  "infra\local\config-templates"
) do (
  if not exist %%~D mkdir %%~D
  if not exist %%~D\.gitkeep type nul > %%~D\.gitkeep
)

REM ==================================================
REM cloud/
REM ==================================================
for %%D in (
  "cloud\src\SimulDiesel.Cloud\Api"
  "cloud\src\SimulDiesel.Cloud\Application"
  "cloud\src\SimulDiesel.Cloud\Domain"
  "cloud\src\SimulDiesel.Cloud\Infrastructure"
  "cloud\src\SimulDiesel.Cloud\Contracts"
  "cloud\database\migrations"
  "cloud\database\schemas"
  "cloud\database\seed"
  "cloud\api-contracts\schemas"
  "cloud\deploy\docker"
) do (
  if not exist %%~D mkdir %%~D
  if not exist %%~D\.gitkeep type nul > %%~D\.gitkeep
)

echo openapi: 3.0.3 > "cloud\api-contracts\openapi.yaml"
echo info: >> "cloud\api-contracts\openapi.yaml"
echo   title: SimulDIESEL API >> "cloud\api-contracts\openapi.yaml"
echo   version: 0.1.0 >> "cloud\api-contracts\openapi.yaml"
echo paths: {} >> "cloud\api-contracts\openapi.yaml"

echo # Cloud (Monolito) - SimulDIESEL > "cloud\README.md"

REM ==================================================
REM local-api/
REM ==================================================
for %%D in (
  "local-api\src\SimulDiesel.LocalApi"
  "local-api\src\SimulDiesel.LocalApp"
  "local-api\src\SimulDiesel.Domain"
  "local-api\src\SimulDiesel.Application"
  "local-api\src\SimulDiesel.Infrastructure"
  "local-api\src\SimulDiesel.Drivers.Esp32"
  "local-api\src\SimulDiesel.Protocols"
  "local-api\src\SimulDiesel.Shared"
  "local-api\tests\unit"
  "local-api\tests\integration"
  "local-api\docs"
) do (
  if not exist %%~D mkdir %%~D
  if not exist %%~D\.gitkeep type nul > %%~D\.gitkeep
)

echo # Local API (.NET) - SimulDIESEL > "local-api\README.md"

REM ==================================================
REM hardware/
REM ==================================================
for %%D in (
  "hardware\boards\x-conn\kicad"
  "hardware\boards\x-conn\bom"
  "hardware\boards\x-conn\gerbers"
  "hardware\boards\x-conn\assembly"
  "hardware\boards\x-conn\docs"
  "hardware\boards\backplane"
  "hardware\boards\babyboards\gerador-niveis"
  "hardware\boards\babyboards\reles"
  "hardware\boards\babyboards\fonte-alimentacao"
  "hardware\boards\babyboards\comunicacao"
  "hardware\firmware\esp32-api-bridge\src"
  "hardware\firmware\arduino-due-vehicle-bus\src"
  "hardware\firmware\arduino-mega-peripherals\src"
  "hardware\firmware\shared"
  "hardware\test-jigs"
) do (
  if not exist %%~D mkdir %%~D
  if not exist %%~D\.gitkeep type nul > %%~D\.gitkeep
)

echo # Hardware - SimulDIESEL > "hardware\README.md"

REM ==================================================
REM tests/
REM ==================================================
for %%D in ("tests\e2e" "tests\hardware-in-the-loop" "tests\performance") do (
  if not exist %%~D mkdir %%~D
  if not exist %%~D\.gitkeep type nul > %%~D\.gitkeep
)

echo.
echo [OK] Estrutura criada com sucesso!
echo.
pause
exit /b 0
