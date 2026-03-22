#pragma once

#include <stdint.h>

class Mcp4725Service {
public:
  bool writeChannel(uint8_t channel, uint8_t setpointRaw);
  bool disableChannel(uint8_t channel);

  static uint8_t addressForChannel(uint8_t channel);
};
