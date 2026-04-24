#pragma once

#include <stdint.h>

#include "defs.h"

class Transport {
public:
  void begin();

  static void onSpiInterrupt();
  static void onCsFalling();
  static void _csThunk();

  static Transport* _self;

private:
  static void primeTxByte();
  static void buildLedResponse(bool state);
  static void buildFunctionalError(uint8_t requestType, uint8_t errorCode);
  static bool validateLedRequest() ;

  static volatile uint8_t _rxBuf[TLV_MAX_LEN];
  static volatile uint16_t _rxLen;
  static volatile uint8_t _txBuf[TLV_MAX_LEN];
  static volatile uint16_t _txLen;
  static volatile uint16_t _txIndex;
  static volatile bool _responsePrepared;
  static volatile bool _sessionActive;
};

extern "C" void SPI0_Handler(void);
