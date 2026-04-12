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
  static void loadNextTxByte();

  static volatile uint8_t _rxWorkBuf[TLV_MAX_LEN];
  static volatile uint8_t _rxWorkLen;
  static volatile uint8_t _rxBuf[TLV_MAX_LEN];
  static volatile uint8_t _rxLen;
  static volatile bool _rxPending;

  static volatile uint8_t _txBuf[TLV_MAX_LEN];
  static volatile uint8_t _txLen;
  static volatile uint8_t _txIndex;
  static volatile bool _txPending;

  static volatile uint8_t _mode;
};

extern "C" void SPI0_Handler(void);
