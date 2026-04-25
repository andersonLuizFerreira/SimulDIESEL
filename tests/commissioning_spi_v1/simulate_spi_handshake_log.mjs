function crc8(data) {
  let crc = 0;
  for (const value of data) {
    crc ^= value;
    for (let bit = 0; bit < 8; bit += 1) {
      crc = (crc & 0x80) ? ((crc << 1) ^ 0x07) & 0xff : (crc << 1) & 0xff;
    }
  }
  return crc;
}

function tlvPacket(type, value) {
  const body = Uint8Array.from([type, value.length, ...value]);
  return Uint8Array.from([...body, crc8(body)]);
}

function hex(bytes) {
  return [...bytes].map((value) => value.toString(16).toUpperCase().padStart(2, "0")).join(" ");
}

const response = tlvPacket(0x12, [0x01]);

console.log("=== SPI HANDSHAKE ERROR FLOW ===");
console.log("[UCE] CS falling detectado: sessao TX");
console.log("[UCE] primeiro byte pre-carregado antes do clock: 0x12");
console.log("[UCE] erro de transmissao: CLOCK_TOO_FAST; IRQ sobe para HIGH");
console.log("[BPM] IRQ HIGH antes do fim esperado; burst abortado e pacote descartado");
console.log("[BPM] solicitando diagnostico em clock SPI seguro");
console.log("[UCE] diagnostico retornado: CLOCK_TOO_FAST");
console.log("[BPM] clock reduzido em 10% e nova tentativa autorizada");

console.log("=== SPI HANDSHAKE SUCCESS FLOW ===");
console.log("[UCE] CS falling detectado: inicio de sessao RX");
console.log("[BPM] request TX concluido; aguardando pulso IRQ 1-0-1");
console.log("[UCE] response staged; IRQ pulse 1-0-1 emitido");
console.log("[BPM] pulso IRQ detectado; baixando CS para burst de leitura");
console.log("[UCE] CS falling detectado: sessao TX");
console.log(`[UCE] primeiro byte pre-carregado antes do clock: 0x${response[0].toString(16).toUpperCase().padStart(2, "0")}`);
console.log("[UCE] IRQ LOW somente apos slave-ready");
console.log("[BPM] IRQ LOW detectado; clocks liberados");
console.log(`[BPM] pacote recebido: ${hex(response)}`);
console.log(`[BPM] CRC correto: calculado=0x${crc8(response.slice(0, -1)).toString(16).toUpperCase().padStart(2, "0")} recebido=0x${response.at(-1).toString(16).toUpperCase().padStart(2, "0")}`);
