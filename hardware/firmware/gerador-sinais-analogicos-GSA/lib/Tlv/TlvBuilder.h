#pragma once
#include <stdint.h>

class TlvBuilder {
public:
  static uint8_t buildEmpty(uint8_t t, uint8_t* out, uint8_t outMax) {
    if (!out || outMax < 2) return 0;
    out[0] = t;
    out[1] = 0;
    return 2;
  }

  static uint8_t buildU8(uint8_t t, uint8_t v0, uint8_t* out, uint8_t outMax) {
    if (!out || outMax < 3) return 0;
    out[0] = t;
    out[1] = 1;
    out[2] = v0;
    return 3;
  }

  static uint8_t buildU8U8(uint8_t t, uint8_t v0, uint8_t v1, uint8_t* out, uint8_t outMax) {
    if (!out || outMax < 4) return 0;
    out[0] = t;
    out[1] = 2;
    out[2] = v0;
    out[3] = v1;
    return 4;
  }
};