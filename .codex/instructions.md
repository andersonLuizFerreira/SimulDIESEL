# Project Rules – SimulDIESEL

## Architecture
- Layered architecture must be preserved.
- Do not merge layers.
- Do not move files without explicit request.

## Embedded Rules
- Arduino/ESP32 code must remain deterministic.
- Avoid dynamic allocation unless requested.
- Prefer header/source separation (.h/.cpp).

## Includes
- Never change include paths automatically.
- Respect PlatformIO structure.

## Refactoring
- Always propose before modifying multiple files.

## Style
- Minimal changes.
- Preserve naming conventions.