#include <Arduino.h>
#include <Wire.h>

#include "Transport.h"
#include "config.h"
#include "crc8.h"

Transport* Transport::_self = nullptr;

volatile uint8_t Transport::_rxBuf[TLV_MAX_LEN];
volatile uint8_t Transport::_rxLen = 0;
volatile bool    Transport::_rxPending = false;

volatile uint8_t Transport::_txBuf[TLV_MAX_LEN];
volatile uint8_t Transport::_txLen = 0;
volatile bool    Transport::_txIsAsyncEvent = false;
bool             Transport::_irqActive = false;

void Transport::begin(uint8_t i2cAddr) {
  _self = this;
  _rxLen = 0;
  _rxPending = false;
  _txLen = 0;
  _txIsAsyncEvent = false;

  setIrqActive(false);

  resumeSlave(i2cAddr);
}

void Transport::resumeSlave(uint8_t i2cAddr) {
  // No ATmega328P, Wire usa o barramento físico fixo em A4/A5.
  Wire.begin(i2cAddr);
  Wire.onReceive(onReceiveThunk);
  Wire.onRequest(onRequestThunk);
}

bool Transport::hasTxPending() {
  noInterrupts();
  bool pending = _txLen != 0;
  interrupts();
  return pending;
}

bool Transport::hasAsyncEventTxPending() {
  noInterrupts();
  bool pending = (_txLen != 0) && _txIsAsyncEvent;
  interrupts();
  return pending;
}

void Transport::setIrqActive(bool active) {
  if (_irqActive == active) {
    return;
  }

  if (active) {
    pinMode(GSA_IRQ_PIN, OUTPUT);
    digitalWrite(GSA_IRQ_PIN, LOW);
  } else {
    digitalWrite(GSA_IRQ_PIN, LOW);
    pinMode(GSA_IRQ_PIN, INPUT);
  }

  _irqActive = active;
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

  // Se ainda não houver resposta pronta, devolve o TLV vazio de "sem dados".
  // Isso permite que a BPM mantenha a coleta assíncrona sem bloquear a GSA.
  if (_txLen == 0) {
    _txBuf[0] = 0xFF;
    _txBuf[1] = 0;
    _txBuf[2] = Crc8::calc((const uint8_t*)_txBuf, 2);
    _txLen = 3;
    _txIsAsyncEvent = false;
  }

  Wire.write((const uint8_t*)_txBuf, _txLen);

  // Limpa para não reenviar a mesma resposta indefinidamente.
  _txLen = 0;
  _txIsAsyncEvent = false;
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

void Transport::setTx(const uint8_t* data, uint8_t len, bool isAsyncEvent) {
  if (!data) return;
  if (len > TLV_MAX_LEN) len = TLV_MAX_LEN;

  noInterrupts();
  for (uint8_t i = 0; i < len; i++) _txBuf[i] = data[i];
  _txLen = len;
  _txIsAsyncEvent = isAsyncEvent;
  interrupts();
}
