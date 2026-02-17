#include "SggwParser.h"

SggwParser::SggwParser()
: _rxEncLen(0),
  _rxDecLen(0),
  _hasFrame(false),
  _hasHeader(false),
  _hdrCmd(0),
  _hdrFlags(0),
  _hdrSeq(0) {
    memset(&_last, 0, sizeof(_last));
}

void SggwParser::reset() {
    _rxEncLen = 0;
    _rxDecLen = 0;
    _hasFrame = false;

    _hasHeader = false;
    _hdrCmd = 0;
    _hdrFlags = 0;
    _hdrSeq = 0;
}

bool SggwParser::getFrame(Frame& out) const {
    if (!_hasFrame) return false;
    out = _last;
    return true;
}

SggwParser::Result SggwParser::push(uint8_t b) {
    _hasFrame = false;
    _hasHeader = false;

    if (b == (uint8_t)SGGW_COBS_DELIMITER) {
        if (_rxEncLen == 0) {
            return None; // delimitador vazio, ignora
        }

        Result r = tryDecodeAndValidate();

        // sempre reseta o acumulador após delimiter
        _rxEncLen = 0;
        return r;
    }

    // overflow antes do delimiter: não dá para responder ERR com segurança
    if (_rxEncLen >= sizeof(_rxEnc)) {
        _rxEncLen = 0;
        _hasHeader = false;
        return FrameTooLarge;
    }

    _rxEnc[_rxEncLen++] = b;
    return None;
}

SggwParser::Result SggwParser::tryDecodeAndValidate() {
    _rxDecLen = 0;

    // 1) decode COBS
    if (!SggwCobs::decode(_rxEnc, _rxEncLen, _rxDec, sizeof(_rxDec), _rxDecLen)) {
        _hasHeader = false;
        return FrameBadCobs;
    }

    // 2) salva header best-effort (CMD/FLAGS/SEQ) se existir
    if (_rxDecLen >= 3) {
        _hasHeader = true;
        _hdrCmd = _rxDec[0];
        _hdrFlags = _rxDec[1];
        _hdrSeq = _rxDec[2];
    } else {
        _hasHeader = false;
    }

    // 3) valida tamanho mínimo/máximo lógico
    // mínimo: CMD, FLAGS, SEQ, CRC = 4 bytes
    if (_rxDecLen < 4) return FrameTooSmall;
    if (_rxDecLen > (size_t)SGGW_MAX_LOGICAL_FRAME) return FrameTooLarge;

    // 4) valida CRC: último byte é CRC
    const uint8_t gotCrc = _rxDec[_rxDecLen - 1];
    const uint8_t calc = SggwCrc8::compute(_rxDec, _rxDecLen - 1);
    if (gotCrc != calc) return FrameBadCrc;

    // 5) frame válido
    _last.cmd = _rxDec[0];
    _last.flags = _rxDec[1];
    _last.seq = _rxDec[2];

    const size_t dataLen = _rxDecLen - 4; // remove CMD,FLAGS,SEQ,CRC
    if (dataLen > (size_t)SGGW_MAX_PAYLOAD) return FrameTooLarge;

    _last.dataLen = (uint8_t)dataLen;
    _last.data = (dataLen > 0) ? &_rxDec[3] : nullptr;

    _hasFrame = true;
    return FrameOk;
}
