#pragma once

#include <stdint.h>

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
