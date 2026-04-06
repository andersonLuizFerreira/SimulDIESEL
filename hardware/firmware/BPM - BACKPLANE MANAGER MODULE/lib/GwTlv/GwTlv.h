#pragma once
#include <stdint.h>
#include <stddef.h>

// Utilitarios do payload TLV interno das baby boards:
// pacote: T | L | V | CRC8
class GwTlv {
public:
    static bool validatePacket(const uint8_t* in, size_t inLen);

    // Packet TLV puro: T | L | V(L bytes) | CRC8
    static bool validatePacket(const uint8_t* in, size_t inLen,
                               uint8_t& typeOut,
                               const uint8_t*& valueOut, uint8_t& valueLenOut);

    // CRC8/ATM (poly 0x07, init 0x00) — igual ao enlace SDGW
    static uint8_t crc8(const uint8_t* data, size_t len);
};
