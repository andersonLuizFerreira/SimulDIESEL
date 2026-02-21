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

bool GwTlv::buildFrame(uint8_t cmd,
                       const uint8_t* tlv, uint8_t tlvLen,
                       uint8_t* out, size_t outMax, size_t& outLen)
{
    outLen = 0;
    if (!out) return false;
    if (tlvLen > 0 && tlv == nullptr) return false;

    // tamanho total: 2 + tlvLen + 1
    const size_t need = (size_t)2 + (size_t)tlvLen + (size_t)1;
    if (need > outMax) return false;

    out[0] = cmd;
    out[1] = tlvLen;

    for (uint8_t i = 0; i < tlvLen; i++) out[2 + i] = tlv[i];

    const uint8_t c = crc8(out, 2 + tlvLen);
    out[2 + tlvLen] = c;

    outLen = need;
    return true;
}

bool GwTlv::validateFrame(const uint8_t* in, size_t inLen,
                          uint8_t& cmdOut,
                          const uint8_t*& tlvOut, uint8_t& tlvLenOut)
{
    cmdOut = 0;
    tlvOut = nullptr;
    tlvLenOut = 0;

    if (!in) return false;
    if (inLen < 3) return false; // cmd,len,crc

    const uint8_t cmd = in[0];
    const uint8_t len = in[1];

    const size_t need = (size_t)2 + (size_t)len + (size_t)1;
    if (inLen < need) return false;

    const uint8_t got = in[2 + len];
    const uint8_t calc = crc8(in, 2 + len);
    if (got != calc) return false;

    cmdOut = cmd;
    tlvLenOut = len;
    tlvOut = (len > 0) ? &in[2] : nullptr;
    return true;
}