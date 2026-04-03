#include <Arduino.h>

#include "config.h"
#include "Tca9548Service.h"

namespace {
static const uint8_t TCA9548_ADDR = 0x70;
}

Tca9548Service::Tca9548Service(SoftwareWire& bus)
  : _bus(bus)
{
}

void Tca9548Service::begin() {
  pinMode(GSA_TCA_RESET_PIN, OUTPUT);
  digitalWrite(GSA_TCA_RESET_PIN, LOW);
  delay(GSA_TCA_RESET_PULSE_MS);
  digitalWrite(GSA_TCA_RESET_PIN, HIGH);
  delay(GSA_TCA_RESET_SETTLE_MS);
}

bool Tca9548Service::selectChannel(uint8_t channel, uint8_t* ackCodeOut) {
  _bus.beginTransmission(TCA9548_ADDR);
  _bus.write(switchMaskForChannel(channel));

  uint8_t ackCode = _bus.endTransmission();
  if (ackCodeOut) {
    *ackCodeOut = ackCode;
  }

  return ackCode == 0;
}

uint8_t Tca9548Service::switchIndexForChannel(uint8_t channel) {
  return (uint8_t)((channel - 1) / 2);
}

uint8_t Tca9548Service::switchMaskForChannel(uint8_t channel) {
  return (uint8_t)(1U << switchIndexForChannel(channel));
}
