#pragma once
#include <stdint.h>

struct TlvFrame {
  uint8_t t = 0;
  uint8_t l = 0;
  const uint8_t* v = nullptr; // aponta para dentro do buffer RX (sem CRC)
};