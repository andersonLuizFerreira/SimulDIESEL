#pragma once

#include <stdint.h>

#include <SoftwareWire.h>

class Mcp4725Service {
public:
  explicit Mcp4725Service(SoftwareWire& bus);

  bool probeChannel(uint8_t channel, uint8_t* ackCodeOut = nullptr);
  bool writeChannel(uint8_t channel, uint16_t outputMillivolts, uint8_t* ackCodeOut = nullptr);
  bool disableChannel(uint8_t channel, uint8_t* ackCodeOut = nullptr);

  static uint8_t addressForChannel(uint8_t channel);

private:
  static bool usesAmplifiedOutput(uint8_t channel);
  static float toDacVoltage(uint8_t channel, uint16_t outputMillivolts);
  static uint16_t convertVoltageToDacCode(float dacVoltage);

private:
  SoftwareWire& _bus;
};
