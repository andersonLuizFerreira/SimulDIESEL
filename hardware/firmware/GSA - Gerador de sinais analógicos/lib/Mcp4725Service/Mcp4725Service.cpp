#include "Mcp4725Service.h"

#include <Wire.h>

namespace {
static const uint8_t MCP4725_ADDR_GND = 0x60;
static const uint8_t MCP4725_ADDR_VCC = 0x61;

bool fastWrite(uint8_t address, uint16_t dacValue) {
  Wire.beginTransmission(address);
  Wire.write((uint8_t)(dacValue >> 4));
  Wire.write((uint8_t)((dacValue & 0x0F) << 4));
  return Wire.endTransmission(true) == 0;
}
}

bool Mcp4725Service::writeChannel(uint8_t channel, uint8_t setpointRaw) {
  uint16_t dacValue = (uint16_t)setpointRaw * 16U;
  return fastWrite(addressForChannel(channel), dacValue);
}

bool Mcp4725Service::disableChannel(uint8_t channel) {
  return fastWrite(addressForChannel(channel), 0);
}

uint8_t Mcp4725Service::addressForChannel(uint8_t channel) {
  return ((channel & 0x01U) != 0) ? MCP4725_ADDR_VCC : MCP4725_ADDR_GND;
}
