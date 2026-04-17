#pragma once

#include <stdint.h>

class Crc8 {
public:
  static uint8_t calc(const uint8_t* data, uint8_t len) {
    uint8_t crc = 0x00;

    for (uint8_t index = 0; index < len; ++index) {
      crc ^= data[index];
      for (uint8_t bit = 0; bit < 8; ++bit) {
        crc = (crc & 0x80U) ? (uint8_t)((crc << 1U) ^ 0x07U) : (uint8_t)(crc << 1U);
      }
    }

    return crc;
  }
};
