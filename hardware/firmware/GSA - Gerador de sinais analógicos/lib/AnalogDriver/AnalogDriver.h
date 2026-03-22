#pragma once

#include <stdint.h>

void analogWriteChannel(uint8_t channel, uint8_t setpoint_raw);
void analogDisableChannel(uint8_t channel);
