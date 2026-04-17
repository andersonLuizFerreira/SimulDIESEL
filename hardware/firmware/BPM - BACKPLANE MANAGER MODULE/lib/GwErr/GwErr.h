#pragma once
#include <stdint.h>

// Códigos de erro do Gateway (roteamento / barramento)
enum GwErr : uint8_t {
    GWERR_OK               = 0x00,
    GWERR_ADDR_UNMAPPED    = 0xE1,
    GWERR_BUS_DOWN         = 0xE2,
    GWERR_TIMEOUT          = 0xE3,
    GWERR_BAD_CRC          = 0xE4,
    GWERR_BAD_FRAME        = 0xE5,
    GWERR_HEADER_INVALID   = 0xE6,
    GWERR_LENGTH_INVALID   = 0xE7,
    GWERR_FRAME_INCOMPLETE = 0xE8,
};
