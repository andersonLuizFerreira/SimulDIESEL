#pragma once

#include <stdint.h>
#include <Arduino.h>

static const uint8_t GSA_CHANNEL_COUNT = 16;
static const uint8_t GSA_CHANNEL_FIRST = 1;
static const uint8_t GSA_CHANNEL_LAST = 16;

static const uint16_t GSA_LOW_RANGE_MAX_MV = 5000;
static const uint16_t GSA_HIGH_RANGE_MAX_MV = 12000;
static const uint16_t GSA_IREAD_MAX_MA = 200;

static const uint16_t GSA_SIM_BASE_VOUT_MV = 1000;
static const uint16_t GSA_SIM_VOUT_STEP_MV = 100;
static const uint16_t GSA_SIM_BASE_IREAD_MA = 100;
static const uint16_t GSA_SIM_IREAD_STEP_MA = 1;

static const uint16_t GSA_EEPROM_SIGNATURE = 0x4753;
static const uint8_t GSA_EEPROM_VERSION = 1;

// Pinagem oficial GSA <-> BPM.
// I2C fisico com a BPM: Wire / A4-A5 (slave).
// I2C logico com TCA9548 + MCP4725: SoftwareWire / D2-D3 (master).
static const uint8_t GSA_PHYSICAL_I2C_SDA_PIN = A4;
static const uint8_t GSA_PHYSICAL_I2C_SCL_PIN = A5;
static const uint8_t GSA_LOGICAL_I2C_SDA_PIN = 2;
static const uint8_t GSA_LOGICAL_I2C_SCL_PIN = 3;
static const uint8_t GSA_IRQ_PIN = 4;
static const uint8_t GSA_TCA_RESET_PIN = 8;

static const uint16_t GSA_LOGICAL_I2C_DELAY_US = 5;
static const uint8_t GSA_TCA_RESET_PULSE_MS = 1;
static const uint8_t GSA_TCA_RESET_SETTLE_MS = 1;
static const uint8_t GSA_PHYSICAL_OP_QUEUE_SIZE = 24;
static const uint8_t GSA_EVENT_QUEUE_SIZE = 24;
