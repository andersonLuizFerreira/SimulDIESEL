#include "Tca9548Service.h"

#include <Wire.h>

namespace {
static const uint8_t TCA9548_ADDR = 0x70;
}

bool Tca9548Service::selectChannel(uint8_t channel) {
  Wire.beginTransmission(TCA9548_ADDR);
  Wire.write(switchMaskForChannel(channel));
  return Wire.endTransmission(true) == 0;
}

uint8_t Tca9548Service::switchIndexForChannel(uint8_t channel) {
  return (uint8_t)((channel - 1) / 2);
}

uint8_t Tca9548Service::switchMaskForChannel(uint8_t channel) {
  return (uint8_t)(1U << switchIndexForChannel(channel));
}
