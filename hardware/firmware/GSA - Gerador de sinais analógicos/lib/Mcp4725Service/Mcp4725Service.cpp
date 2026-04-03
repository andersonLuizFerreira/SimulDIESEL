#include "Mcp4725Service.h"

#include <math.h>

namespace {
static const uint8_t MCP4725_ADDR_GND = 0x60;
static const uint8_t MCP4725_ADDR_VCC = 0x61;
static const float MCP4725_DAC_REFERENCE_VOLTS = 5.0f;
static const float GSA_OUTPUT_AMPLIFIER_GAIN = 2.4f;
static const uint16_t MCP4725_DAC_MAX_CODE = 4095;

float clampVoltage(float voltage) {
  if (voltage < 0.0f) {
    return 0.0f;
  }

  if (voltage > MCP4725_DAC_REFERENCE_VOLTS) {
    return MCP4725_DAC_REFERENCE_VOLTS;
  }

  return voltage;
}
}

Mcp4725Service::Mcp4725Service(SoftwareWire& bus)
  : _bus(bus)
{
}

bool Mcp4725Service::probeChannel(uint8_t channel, uint8_t* ackCodeOut) {
  _bus.beginTransmission(addressForChannel(channel));

  uint8_t ackCode = _bus.endTransmission();
  if (ackCodeOut) {
    *ackCodeOut = ackCode;
  }

  return ackCode == 0;
}

bool Mcp4725Service::writeChannel(uint8_t channel, uint16_t outputMillivolts, uint8_t* ackCodeOut) {
  uint16_t dacCode = convertVoltageToDacCode(toDacVoltage(channel, outputMillivolts));

  _bus.beginTransmission(addressForChannel(channel));
  _bus.write((uint8_t)0x40);
  _bus.write((uint8_t)(dacCode >> 4));
  _bus.write((uint8_t)((dacCode & 0x0FU) << 4));

  uint8_t ackCode = _bus.endTransmission();
  if (ackCodeOut) {
    *ackCodeOut = ackCode;
  }

  return ackCode == 0;
}

bool Mcp4725Service::disableChannel(uint8_t channel, uint8_t* ackCodeOut) {
  _bus.beginTransmission(addressForChannel(channel));
  _bus.write((uint8_t)0x40);
  _bus.write((uint8_t)0x00);
  _bus.write((uint8_t)0x00);

  uint8_t ackCode = _bus.endTransmission();
  if (ackCodeOut) {
    *ackCodeOut = ackCode;
  }

  return ackCode == 0;
}

uint8_t Mcp4725Service::addressForChannel(uint8_t channel) {
  return ((channel & 0x01U) != 0) ? MCP4725_ADDR_VCC : MCP4725_ADDR_GND;
}

bool Mcp4725Service::usesAmplifiedOutput(uint8_t channel) {
  return channel >= 9U;
}

float Mcp4725Service::toDacVoltage(uint8_t channel, uint16_t outputMillivolts) {
  float requestedVoltage = ((float)outputMillivolts) / 1000.0f;
  if (usesAmplifiedOutput(channel)) {
    requestedVoltage /= GSA_OUTPUT_AMPLIFIER_GAIN;
  }

  return clampVoltage(requestedVoltage);
}

uint16_t Mcp4725Service::convertVoltageToDacCode(float dacVoltage) {
  long roundedCode = lroundf((clampVoltage(dacVoltage) / MCP4725_DAC_REFERENCE_VOLTS) * (float)MCP4725_DAC_MAX_CODE);
  if (roundedCode < 0L) {
    return 0U;
  }

  if (roundedCode > (long)MCP4725_DAC_MAX_CODE) {
    return MCP4725_DAC_MAX_CODE;
  }

  return (uint16_t)roundedCode;
}
