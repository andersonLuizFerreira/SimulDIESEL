#include "SggwLink.h"
#include "SggwDevice.h"

SggwLink::SggwLink(SggwTransport& transport)
: _tr(transport),
  _dev(nullptr),
  _hs(WaitingBanner),
  _bannerLen(0),
  _txSeq((uint8_t)SGGW_SEQ_START),
  _haveLastResp(false),
  _lastRxSeq(0),
  _lastRespLen(0)
{
    memset(_bannerBuf, 0, sizeof(_bannerBuf));
    memset(_lastRespBuf, 0, sizeof(_lastRespBuf));
}

void SggwLink::begin() {
    _hs = WaitingBanner;
    _bannerLen = 0;

    _txSeq = (uint8_t)SGGW_SEQ_START;

    _haveLastResp = false;
    _lastRxSeq = 0;
    _lastRespLen = 0;

    _parser.reset();

    // (6) fora do modo binário => texto permitido
    _tr.setTextEnabled(true);
}

void SggwLink::poll() {
    while (_tr.available() > 0) {
        int v = _tr.readByte();
        if (v < 0) break;
        uint8_t b = (uint8_t)v;

        // --------------------
        // HANDSHAKE
        // --------------------
        if (_hs != Linked) {
            processHandshakeByte(b);
            continue;
        }

        // --------------------
        // BINARY PROTOCOL
        // --------------------
        SggwParser::Result r = _parser.push(b);

        if (r == SggwParser::FrameOk) {
            SggwParser::Frame f;
            if (_parser.getFrame(f)) handleFrameOk(f);
            continue;
        }

        // --------------------
        // ERRO: tenta responder ERR se for possível extrair FLAGS/SEQ
        // --------------------
        if (r == SggwParser::FrameBadCrc ||
            r == SggwParser::FrameTooSmall ||
            r == SggwParser::FrameTooLarge ||
            r == SggwParser::FrameBadCobs)
        {
            if (_parser.hasHeader()) {
                uint8_t cmd, flags, seq;
                _parser.getHeader(cmd, flags, seq);

                (void)cmd;

                const bool ackReq = (flags & (uint8_t)SGGW_FLAG_ACK_REQUIRED) != 0;

                // Retransmissão: reenvia última resposta e não muda estado
                if (ackReq && _haveLastResp && seq == _lastRxSeq) {
                    _tr.writeBytes(_lastRespBuf, _lastRespLen);
                    continue;
                }

                if (ackReq) {
                    uint8_t errCode = (uint8_t)SGGW_ERR_BAD_CRC;

                    if      (r == SggwParser::FrameBadCrc)   errCode = (uint8_t)SGGW_ERR_BAD_CRC;
                    else if (r == SggwParser::FrameTooSmall) errCode = (uint8_t)SGGW_ERR_TOO_SMALL;
                    else if (r == SggwParser::FrameTooLarge) errCode = (uint8_t)SGGW_ERR_TOO_LARGE;
                    else if (r == SggwParser::FrameBadCobs)  errCode = (uint8_t)SGGW_ERR_BAD_COBS;

                    sendErr(seq, errCode);

                    _haveLastResp = true;
                    _lastRxSeq = seq;
                }
            }
        }
    }
}

void SggwLink::processHandshakeByte(uint8_t b) {
    if (_hs != WaitingBanner) return;

    if (_bannerLen < (SGGW_HANDSHAKE_BUFFER - 1)) {
        _bannerBuf[_bannerLen++] = (char)b;
        _bannerBuf[_bannerLen] = '\0';
    } else {
        _bannerLen = 0;
        memset(_bannerBuf, 0, sizeof(_bannerBuf));
        return;
    }

    const size_t need = strlen(SGGW_PC_BANNER);
    if (_bannerLen >= need) {
        const char* tail = &_bannerBuf[_bannerLen - need];
        if (memcmp(tail, SGGW_PC_BANNER, need) == 0) {

            // responde a linha de versão (texto) para o PC marcar Linked
            sendBanner();

            // entra em modo binário
            _hs = Linked;

            // (6) IMPORTANTÍSSIMO: a partir daqui, texto OFF
            _tr.setTextEnabled(false);

            _bannerLen = 0;
            memset(_bannerBuf, 0, sizeof(_bannerBuf));
        }
    }
}

void SggwLink::sendBanner() {
    _tr.writeBytes((const uint8_t*)SGGW_DEVICE_BANNER, strlen(SGGW_DEVICE_BANNER));
}

void SggwLink::handleFrameOk(const SggwParser::Frame& f) {
    // Ignora ACK/ERR vindos do PC
    if (f.cmd == (uint8_t)SGGW_CMD_ACK || f.cmd == (uint8_t)SGGW_CMD_ERR) {
        return;
    }

    const bool ackReq = (f.flags & (uint8_t)SGGW_FLAG_ACK_REQUIRED) != 0;

    // Retransmissão: mesmo SEQ com ACK_REQ => reenviar último ACK/ERR
    if (ackReq && _haveLastResp && f.seq == _lastRxSeq) {
        _tr.writeBytes(_lastRespBuf, _lastRespLen);
        return;
    }

    // Se válido e ACK requerido: manda ACK
    if (ackReq) {
        sendAck(f.seq);
        _haveLastResp = true;
        _lastRxSeq = f.seq;
    }

    // Dispatch
    if (_dev) {
        _dev->onCommand(f.cmd, f.flags, f.seq, f.data, f.dataLen);
    }
}

uint8_t SggwLink::nextTxSeq() {
    _txSeq++;
    if (_txSeq == 0) _txSeq = (uint8_t)SGGW_SEQ_START;
    return _txSeq;
}

bool SggwLink::sendEvent(uint8_t cmd, const uint8_t* payload, uint8_t payloadLen) {
    const uint8_t seq = nextTxSeq();
    return sendFrame(cmd, (uint8_t)SGGW_FLAG_IS_EVENT, seq, payload, payloadLen, false);
}

void SggwLink::sendAck(uint8_t rxSeq) {
    sendFrame((uint8_t)SGGW_CMD_ACK, 0, rxSeq, nullptr, 0, true);
}

void SggwLink::sendErr(uint8_t rxSeq, uint8_t errCode) {
    uint8_t p[1] = { errCode };
    sendErrRaw(rxSeq, p, 1);
}

void SggwLink::sendErrRaw(uint8_t rxSeq, const uint8_t* payload, uint8_t payloadLen) {
    sendFrame((uint8_t)SGGW_CMD_ERR, 0, rxSeq, payload, payloadLen, true);
}

bool SggwLink::sendFrame(uint8_t cmd, uint8_t flags, uint8_t seq,
                         const uint8_t* payload, uint8_t payloadLen,
                         bool cacheAsLastResp)
{
    if (payloadLen > (uint8_t)SGGW_MAX_PAYLOAD) return false;

    uint8_t logical[SGGW_MAX_LOGICAL_FRAME];
    size_t  logicalLen = 0;

    logical[0] = cmd;
    logical[1] = flags;
    logical[2] = seq;
    logicalLen = 3;

    for (uint8_t i = 0; i < payloadLen; i++) {
        logical[logicalLen++] = payload[i];
    }

    const uint8_t crc = SggwCrc8::compute(logical, logicalLen);
    logical[logicalLen++] = crc;

    uint8_t encoded[SGGW_MAX_ENCODED_FRAME];
    size_t encLen = 0;

    if (!SggwCobs::encode(logical, logicalLen, encoded, sizeof(encoded), encLen))
        return false;

    if (encLen + 1 > sizeof(encoded)) return false;
    encoded[encLen++] = (uint8_t)SGGW_COBS_DELIMITER;

    _tr.writeBytes(encoded, encLen);

    if (cacheAsLastResp) {
        if (encLen <= sizeof(_lastRespBuf)) {
            memcpy(_lastRespBuf, encoded, encLen);
            _lastRespLen = encLen;
        }
    }

    return true;
}

void SggwLink::logout() {
    begin(); // volta para WaitingBanner, zera buffers/seq/cache e reseta parser
}
