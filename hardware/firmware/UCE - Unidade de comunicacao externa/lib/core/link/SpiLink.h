#pragma once

#include <stddef.h>
#include <stdint.h>

#include "defs.h"

class SpiLink {
public:
  static constexpr size_t BufferSize = 64;

  void begin();

  bool available() const;
  bool txPending() const;
  bool read(uint8_t* dst, size_t& len);
  bool write(const uint8_t* src, size_t len);

  static void onSpiInterrupt();
  static void onCsFalling();
  static void _csThunk();

  static SpiLink* _self;

private:
  static void primeTxByte();
  static void setIrqReady(bool ready);
  static void pulseAttention();

  static volatile uint8_t _rxBuf[BufferSize];
  static volatile uint8_t _txBuf[BufferSize];
  static volatile uint16_t _index;
  static volatile bool _rxReady;
  static volatile bool _txPending;
  static volatile bool _transferActive;
};

extern "C" void SPI0_Handler(void);
