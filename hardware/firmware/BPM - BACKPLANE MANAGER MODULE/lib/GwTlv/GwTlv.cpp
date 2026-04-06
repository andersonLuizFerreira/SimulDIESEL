#include "GwTlv.h"

uint8_t GwTlv::crc8(const uint8_t* data, size_t len) {
    uint8_t crc = 0x00;
    for (size_t i = 0; i < len; i++) {
        crc ^= data[i];
        for (uint8_t b = 0; b < 8; b++) {
            if (crc & 0x80) crc = (uint8_t)((crc << 1) ^ 0x07);
            else           crc = (uint8_t)(crc << 1);
        }
    }
    return crc;
}

bool GwTlv::validatePacket(const uint8_t* in, size_t inLen)
{
    uint8_t type = 0;
    const uint8_t* value = nullptr;
    uint8_t valueLen = 0;
    return validatePacket(in, inLen, type, value, valueLen);
}

bool GwTlv::validatePacket(const uint8_t* in, size_t inLen,
                           uint8_t& typeOut,
                           const uint8_t*& valueOut, uint8_t& valueLenOut)
{
    typeOut = 0;
    valueOut = nullptr;
    valueLenOut = 0;

    if (!in) return false;
    if (inLen < 3) return false; // t, l, crc

    const uint8_t type = in[0];
    const uint8_t len = in[1];
    const size_t need = (size_t)2 + (size_t)len + (size_t)1;
    if (inLen != need) return false;

    const uint8_t got = in[need - 1];
    const uint8_t calc = crc8(in, need - 1);
    if (got != calc) return false;

    typeOut = type;
    valueLenOut = len;
    valueOut = (len > 0) ? &in[2] : nullptr;
    return true;
}
