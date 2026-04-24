"""Emit a deterministic handshake log for the BPM <-> UCE SPI session model."""

from __future__ import annotations


def crc8(data: bytes) -> int:
    crc = 0
    for value in data:
        crc ^= value
        for _ in range(8):
            crc = ((crc << 1) ^ 0x07) & 0xFF if crc & 0x80 else (crc << 1) & 0xFF
    return crc


def tlv_packet(t: int, value: bytes) -> bytes:
    body = bytes([t, len(value)]) + value
    return body + bytes([crc8(body)])


def print_success_flow() -> None:
    response = tlv_packet(0x12, bytes([0x01]))
    print("[UCE] CS falling detectado: inicio de sessao RX")
    print("[BPM] request TX concluido; aguardando pulso IRQ 1-0-1")
    print("[UCE] response staged; IRQ pulse 1-0-1 emitido")
    print("[BPM] pulso IRQ detectado; baixando CS para burst de leitura")
    print("[UCE] CS falling detectado: sessao TX")
    print(f"[UCE] primeiro byte pre-carregado antes do clock: 0x{response[0]:02X}")
    print("[UCE] IRQ LOW somente apos slave-ready")
    print("[BPM] IRQ LOW detectado; clocks liberados")
    print("[BPM] pacote recebido: " + " ".join(f"{b:02X}" for b in response))
    print(f"[BPM] CRC correto: calculado=0x{crc8(response[:-1]):02X} recebido=0x{response[-1]:02X}")


def print_error_flow() -> None:
    print("[UCE] CS falling detectado: sessao TX")
    print("[UCE] primeiro byte pre-carregado antes do clock: 0x12")
    print("[UCE] erro de transmissao: CLOCK_TOO_FAST; IRQ sobe para HIGH")
    print("[BPM] IRQ HIGH antes do fim esperado; burst abortado e pacote descartado")
    print("[BPM] solicitando diagnostico em clock SPI seguro")
    print("[UCE] diagnostico retornado: CLOCK_TOO_FAST")
    print("[BPM] clock reduzido em 10% e nova tentativa autorizada")


if __name__ == "__main__":
    print("=== SPI HANDSHAKE ERROR FLOW ===")
    print_error_flow()
    print("=== SPI HANDSHAKE SUCCESS FLOW ===")
    print_success_flow()
