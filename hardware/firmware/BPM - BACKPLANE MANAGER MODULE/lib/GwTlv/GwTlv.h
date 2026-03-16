#pragma once
#include <stdint.h>
#include <stddef.h>

// Utilitarios para os dois formatos internos ainda presentes no projeto:
// - pacote da baby board: T | L | V | CRC8
// - frame legado do gateway: CMD | LEN | PAYLOAD | CRC8
class GwTlv {
public:
    static bool validatePacket(const uint8_t* in, size_t inLen);

    // Packet TLV puro: T | L | V(L bytes) | CRC8
    static bool validatePacket(const uint8_t* in, size_t inLen,
                               uint8_t& typeOut,
                               const uint8_t*& valueOut, uint8_t& valueLenOut);

    // Frame legado do gateway: CMD | LEN | PAYLOAD(LEN bytes) | CRC8
    static bool buildFrame(uint8_t cmd,
                           const uint8_t* tlv, uint8_t tlvLen,
                           uint8_t* out, size_t outMax, size_t& outLen);

    static bool validateFrame(const uint8_t* in, size_t inLen,
                              uint8_t& cmdOut,
                              const uint8_t*& tlvOut, uint8_t& tlvLenOut);

    // CRC8/ATM (poly 0x07, init 0x00) — igual SGGW
    static uint8_t crc8(const uint8_t* data, size_t len);
};
