#include "AnalogDriver.h"

#include <Wire.h>

#include "../Transport/Transport.h"
#include "defs.h"

namespace {
static const uint8_t TCA9548A_ADDR = 0x70;
static const uint8_t MCP4725_ADDR_GND = 0x60;
static const uint8_t MCP4725_ADDR_VCC = 0x61;

uint8_t resolveSwitchChannelIndex(uint8_t channel) {
  return (uint8_t)((channel - 1) / 2);
}

uint8_t resolveDacAddress(uint8_t channel) {
  return ((channel & 0x01) != 0) ? MCP4725_ADDR_VCC : MCP4725_ADDR_GND;
}

void writeDacRaw(uint8_t channel, uint16_t dacValue) {
  if (channel == 0 || channel > 16) {
    return;
  }

  uint8_t scIndex = resolveSwitchChannelIndex(channel);
  uint8_t dacAddr = resolveDacAddress(channel);

  Wire.begin();

  Wire.beginTransmission(TCA9548A_ADDR);
  Wire.write((uint8_t)(1U << scIndex));
  Wire.endTransmission();

  Wire.beginTransmission(dacAddr);
  Wire.write((uint8_t)(dacValue >> 4));
  Wire.write((uint8_t)((dacValue & 0x0F) << 4));
  Wire.endTransmission();

  Transport::resumeSlave(I2C_GSA_ADDR);
}
}

void analogWriteChannel(uint8_t channel, uint8_t setpoint_raw) {
  uint16_t dac = (uint16_t)setpoint_raw * 16U;
  writeDacRaw(channel, dac);
}

void analogDisableChannel(uint8_t channel) {
  writeDacRaw(channel, 0);
}
