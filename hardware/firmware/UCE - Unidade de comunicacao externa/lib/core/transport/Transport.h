#pragma once

#include <stdint.h>

#include "defs.h"

class Transport {
public:
  void begin();
  bool popRx(uint8_t* out, uint8_t& outLen);
  void setTx(const uint8_t* data, uint8_t len);
  static void setIrqActive(bool active);
  static void onSpiInterrupt();

  static Transport* _self;

private:
  enum TransactionMode : uint8_t {
    ModeIdle = 0,
    ModeReceiving = 1,
    ModeSending = 2
  };

  static void clearTxState();
  static void primeTxByte(uint8_t value);
  static void primeTxForCurrentPosition();
  static void rearmTxForNextBurst();

  static volatile uint8_t _rxWorkBuf[TLV_MAX_LEN];
  static volatile uint8_t _rxWorkLen;
  static volatile uint8_t _rxBuf[TLV_MAX_LEN];
  static volatile uint8_t _rxLen;
  static volatile bool _rxPending;

  static volatile uint8_t _txBuf[TLV_MAX_LEN];
  static volatile uint16_t _txLen;
  static volatile uint16_t _txSentCount;
  static volatile bool _txActive;
  static volatile bool _txPrimed;
  static volatile uint8_t _txPrimedByte;

  static volatile uint8_t _mode;
};

extern "C" void SPI0_Handler(void);
