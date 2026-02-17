#pragma once
#include <stdint.h>
#include <stddef.h>
#include <string.h>

#include "Sggw.defs.h"
#include "SggwTransport.h"
#include "SggwParser.h"
#include "SggwCobs.h"
#include "SggwCrc8.h"

class SggwDevice;

class SggwLink {
public:
    enum HandshakeState {
        WaitingBanner,
        Linked
    };

    explicit SggwLink(SggwTransport& transport);

    void attachDevice(SggwDevice* dev) { _dev = dev; }

    void begin();
    void poll();

    bool sendEvent(uint8_t cmd, const uint8_t* payload, uint8_t payloadLen);
    void sendErr(uint8_t rxSeq, uint8_t errCode);

    void logout();

    // (6) status/controle
    bool isLinked() const { return _hs == Linked; }

private:
    SggwTransport& _tr;
    SggwParser _parser;
    SggwDevice* _dev;

    HandshakeState _hs;

    char   _bannerBuf[SGGW_HANDSHAKE_BUFFER];
    size_t _bannerLen;

    uint8_t _txSeq;

    bool    _haveLastResp;
    uint8_t _lastRxSeq;
    uint8_t _lastRespBuf[SGGW_MAX_LAST_RESPONSE];
    size_t  _lastRespLen;

private:
    void processHandshakeByte(uint8_t b);
    void sendBanner();

    void handleFrameOk(const SggwParser::Frame& f);

    void sendAck(uint8_t rxSeq);
    void sendErrRaw(uint8_t rxSeq, const uint8_t* payload, uint8_t payloadLen);

    bool sendFrame(uint8_t cmd, uint8_t flags, uint8_t seq,
                   const uint8_t* payload, uint8_t payloadLen,
                   bool cacheAsLastResp);

    uint8_t nextTxSeq();
};
