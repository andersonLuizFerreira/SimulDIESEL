#pragma once

#include <stdint.h>

class Tca9548Service {
public:
  bool selectChannel(uint8_t channel);

  static uint8_t switchIndexForChannel(uint8_t channel);
  static uint8_t switchMaskForChannel(uint8_t channel);
};
