#pragma once

#include <stdint.h>

struct TlvFrame {
  uint8_t t = 0;
  uint8_t l = 0;
  const uint8_t* v = nullptr;
};

class Tlv {
public:
  static uint8_t build(uint8_t t, const uint8_t* payload, uint8_t payloadLen, uint8_t* out, uint8_t outMax);
  static uint8_t buildU8(uint8_t t, uint8_t value, uint8_t* out, uint8_t outMax);
};
