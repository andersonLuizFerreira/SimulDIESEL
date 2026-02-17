#pragma once
#include <stdint.h>
#include <stddef.h>
#include <string.h>

#include "Sggw.defs.h"
#include "SggwCobs.h"
#include "SggwCrc8.h"

class SggwParser {
public:
    struct Frame {
        uint8_t cmd;
        uint8_t flags;
        uint8_t seq;
        const uint8_t* data;
        uint8_t dataLen;
    };

    enum Result {
        None,
        FrameOk,
        FrameBadCobs,
        FrameTooLarge,
        FrameTooSmall,
        FrameBadCrc
    };

    SggwParser();

    // Alimenta o parser com bytes da UART.
    // Ao receber delimiter (0x00) tenta decodificar + validar.
    Result push(uint8_t b);

    // Frame válido completo
    bool getFrame(Frame& out) const;

    // Header best-effort: válido quando COBS decode ocorreu e _rxDecLen >= 3
    bool hasHeader() const { return _hasHeader; }

    // Retorna CMD/FLAGS/SEQ do header best-effort
    void getHeader(uint8_t& cmd, uint8_t& flags, uint8_t& seq) const {
        cmd = _hdrCmd;
        flags = _hdrFlags;
        seq = _hdrSeq;
    }

    void reset();

private:
    // Buffer do frame COBS (sem delimiter)
    uint8_t _rxEnc[SGGW_MAX_ENCODED_FRAME];
    size_t  _rxEncLen;

    // Buffer do frame lógico decodificado (CMD..CRC)
    uint8_t _rxDec[SGGW_MAX_LOGICAL_FRAME];
    size_t  _rxDecLen;

    Frame   _last;
    bool    _hasFrame;

    // header best-effort
    bool    _hasHeader;
    uint8_t _hdrCmd;
    uint8_t _hdrFlags;
    uint8_t _hdrSeq;

    Result tryDecodeAndValidate();
};
