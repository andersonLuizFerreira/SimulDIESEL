#pragma once
#include <stdint.h>
#include <stddef.h>

class Crc8 {
public:
  // CRC-8/ATM: poly 0x07, init 0x00, xorout 0x00
  static uint8_t calc(const uint8_t* data, size_t len) {
    uint8_t crc = 0x00;
    for (size_t i = 0; i < len; i++) {
      crc ^= data[i];
      for (uint8_t b = 0; b < 8; b++) {
        if (crc & 0x80) crc = (uint8_t)((crc << 1) ^ 0x07);
        else           crc = (uint8_t)(crc << 1);
      }
    }
    return crc;
  }
};