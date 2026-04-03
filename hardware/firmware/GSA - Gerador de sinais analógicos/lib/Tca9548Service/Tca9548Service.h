#pragma once

#include <stdint.h>

#include <SoftwareWire.h>

class Tca9548Service {
public:
  explicit Tca9548Service(SoftwareWire& bus);
  void begin();

  bool selectChannel(uint8_t channel, uint8_t* ackCodeOut = nullptr);

  static uint8_t switchIndexForChannel(uint8_t channel);
  static uint8_t switchMaskForChannel(uint8_t channel);

private:
  SoftwareWire& _bus;
};
