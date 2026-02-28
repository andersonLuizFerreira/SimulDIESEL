#include "Transport.h"

Transport* Transport::_self = nullptr;

volatile uint8_t Transport::_rxBuf[TLV_MAX_LEN];
volatile uint8_t Transport::_rxLen = 0;
volatile bool    Transport::_rxPending = false;

volatile uint8_t Transport::_txBuf[TLV_MAX_LEN];
volatile uint8_t Transport::_txLen = 0;

void Transport::begin(uint8_t i2cAddr) {
  _self = this;
  _rxLen = 0;
  _rxPending = false;
  _txLen = 0;

  Wire.begin(i2cAddr);                  // UNO como SLAVE
  Wire.onReceive(onReceiveThunk);
  Wire.onRequest(onRequestThunk);
}

void Transport::onReceiveThunk(int count) {
  if (!_self) return;
  if (count <= 0) return;

  if (count > (int)TLV_MAX_LEN) count = TLV_MAX_LEN;

  _rxLen = 0;
  while (count-- > 0) {
    _rxBuf[_rxLen++] = (uint8_t)Wire.read();
  }
  _rxPending = true;
}

void Transport::onRequestThunk() {
  if (!_self) return;

  // Se não tiver resposta preparada, devolve TLV 0xFF vazio (no-data)
  if (_txLen == 0) {
    _txBuf[0] = 0xFF;
    _txBuf[1] = 0;
    _txLen = 2;
  }

  Wire.write((const uint8_t*)_txBuf, _txLen);

  // Limpa para não reenviar a mesma resposta indefinidamente
  _txLen = 0;
}

bool Transport::popRx(uint8_t* out, uint8_t& outLen) {
  if (!_rxPending) return false;

  noInterrupts();
  uint8_t n = _rxLen;
  if (n > TLV_MAX_LEN) n = TLV_MAX_LEN;
  for (uint8_t i = 0; i < n; i++) out[i] = _rxBuf[i];
  _rxPending = false;
  interrupts();

  outLen = n;
  return true;
}

void Transport::setTx(const uint8_t* data, uint8_t len) {
  if (!data) return;
  if (len > TLV_MAX_LEN) len = TLV_MAX_LEN;

  noInterrupts();
  for (uint8_t i = 0; i < len; i++) _txBuf[i] = data[i];
  _txLen = len;
  interrupts();
}