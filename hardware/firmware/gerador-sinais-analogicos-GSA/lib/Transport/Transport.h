#pragma once
#include <Arduino.h>
#include <Wire.h>
#include "defs.h"

class Transport {
public:
  void begin(uint8_t i2cAddr);

  // Chamar no loop para pegar frame recebido (se houver)
  bool popRx(uint8_t* out, uint8_t& outLen);

  // Prepara resposta para o próximo requestFrom() do master
  void setTx(const uint8_t* data, uint8_t len);

private:
  static void onReceiveThunk(int count);
  static void onRequestThunk();

  static volatile uint8_t _rxBuf[TLV_MAX_LEN];
  static volatile uint8_t _rxLen;
  static volatile bool    _rxPending;

  static volatile uint8_t _txBuf[TLV_MAX_LEN];
  static volatile uint8_t _txLen;

  static Transport* _self;
};