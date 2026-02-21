#pragma once
#include <stdint.h>
#include <stddef.h>

// Frame interno: CMD | LEN | TLVs(LEN bytes) | CRC8
class GwTlv {
public:
    static bool buildFrame(uint8_t cmd,
                           const uint8_t* tlv, uint8_t tlvLen,
                           uint8_t* out, size_t outMax, size_t& outLen);

    static bool validateFrame(const uint8_t* in, size_t inLen,
                              uint8_t& cmdOut,
                              const uint8_t*& tlvOut, uint8_t& tlvLenOut);

    // CRC8/ATM (poly 0x07, init 0x00) — igual SGGW
    static uint8_t crc8(const uint8_t* data, size_t len);
};