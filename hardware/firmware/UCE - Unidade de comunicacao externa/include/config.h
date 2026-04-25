#pragma once

#include <Arduino.h>
#include <stdint.h>
#include "defs.h"

// Pinagem congelada BPM -> UCE.
// SCK/MOSI/MISO usam obrigatoriamente o conector SPI da Due.
static const uint8_t UCE_IRQ_PIN = UCE_SPI_IRQ_PIN;
static const uint8_t UCE_IRQ_ACTIVE_LEVEL = UCE_SPI_IRQ_ACTIVE_LEVEL;
static const uint8_t UCE_IRQ_IDLE_LEVEL = UCE_SPI_IRQ_IDLE_LEVEL;
