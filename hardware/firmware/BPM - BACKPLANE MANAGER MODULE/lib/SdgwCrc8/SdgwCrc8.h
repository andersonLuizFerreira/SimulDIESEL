#pragma once
#include <stdint.h>
#include <stddef.h>
#include "SdgwDefs.h"

class SdgwCrc8 {
public:
    // CRC-8/ATM: poly=0x07, init=0x00, refin=false, refout=false, xorout=0x00
    static uint8_t compute(const uint8_t* data, size_t len) {
        uint8_t crc = (uint8_t)SDGW_CRC_INIT;
        for (size_t i = 0; i < len; i++) {
            crc ^= data[i];
            for (uint8_t b = 0; b < 8; b++) {
                if (crc & 0x80) crc = (uint8_t)((crc << 1) ^ SDGW_CRC_POLY);
                else           crc = (uint8_t)(crc << 1);
            }
        }
        return crc;
    }
};
