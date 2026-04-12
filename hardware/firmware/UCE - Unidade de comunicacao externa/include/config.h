#pragma once

#include <Arduino.h>
#include <stdint.h>

// Pinagem congelada BPM -> UCE.
// SCK/MOSI/MISO usam obrigatoriamente o conector SPI da Due.
static const uint8_t UCE_SPI_CS_PIN = 10;
static const uint8_t UCE_IRQ_PIN = 2;

// O reset da BPM entra no reset fisico da Due e não passa por GPIO de firmware.
static const uint8_t UCE_IRQ_ACTIVE_LEVEL = LOW;
static const uint8_t UCE_IRQ_IDLE_LEVEL = HIGH;
