#include "SggwCobs.h"

bool SggwCobs::encode(const uint8_t* in, size_t len, uint8_t* out, size_t outMax, size_t& outLen) {
    outLen = 0;
    if (!out || (!in && len != 0)) return false;

    if (outMax < 1) return false;

    size_t codeIndex = 0;
    uint8_t code = 1;

    out[codeIndex] = 0; // placeholder
    outLen = 1;

    for (size_t i = 0; i < len; i++) {
        if (in[i] == 0) {
            out[codeIndex] = code;
            codeIndex = outLen;
            if (outLen >= outMax) return false;
            out[outLen++] = 0; // novo placeholder
            code = 1;
        } else {
            if (outLen >= outMax) return false;
            out[outLen++] = in[i];
            code++;
            if (code == 0xFF) {
                out[codeIndex] = code;
                codeIndex = outLen;
                if (outLen >= outMax) return false;
                out[outLen++] = 0; // novo placeholder
                code = 1;
            }
        }
    }

    out[codeIndex] = code;
    return true;
}

bool SggwCobs::decode(const uint8_t* in, size_t len, uint8_t* out, size_t outMax, size_t& outLen) {
    outLen = 0;
    if (!out || (!in && len != 0)) return false;
    if (len == 0) return true;

    size_t i = 0;
    while (i < len) {
        uint8_t code = in[i];
        if (code == 0) return false;
        i++;

        uint8_t copyLen = (uint8_t)(code - 1);
        if (i + copyLen > len) return false;

        for (uint8_t k = 0; k < copyLen; k++) {
            if (outLen >= outMax) return false;
            out[outLen++] = in[i++];
        }

        if (code != 0xFF && i < len) {
            if (outLen >= outMax) return false;
            out[outLen++] = 0x00;
        }
    }
    return true;
}
