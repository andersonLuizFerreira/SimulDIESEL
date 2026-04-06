#include "SdgwLink.h"
#include "IGatewayApp.h"

// Construtor da camada de link do gateway, inicializa variáveis membro e buffers.
SdgwLink::SdgwLink(ISdgwEndpoint &transport, SdgwSessionOwner& sessionOwner)
    : _tr(transport),
      _sessionOwner(sessionOwner),
      _parser(),
      _app(nullptr),
      _hs(WaitingBanner),
      _bannerLen(0),
      _txSeq(SDGW_SEQ_START),
      _haveLastResp(false),
      _lastRxSeq(0),
      _lastRespLen(0),
      _activeEndpointKind(SDGW_ENDPOINT_NONE),
      _lastActivityMs(0),
      _activityTimeoutMs((uint16_t)SDGW_LINK_ACTIVITY_TIMEOUT_MS)
{
    // Inicializa os buffers de banner e resposta com zeros.
    memset(_bannerBuf, 0, sizeof(_bannerBuf));
    memset(_lastRespBuf, 0, sizeof(_lastRespBuf));
    pinMode(LED_BUILTIN, OUTPUT);
}

// Reseta o estado do link e o prepara para operação.
void SdgwLink::begin()
{
    _hs = WaitingBanner; // Define o estado de handshake para WaitingBanner.
    _bannerLen = 0;      // Reseta o comprimento do banner.
    _activeEndpointKind = SDGW_ENDPOINT_NONE;

    _txSeq = (uint8_t)SDGW_SEQ_START; // Reseta a sequência de transmissão.

    _haveLastResp = false; // Limpa o estado da última resposta.
    _lastRxSeq = 0;        // Reseta a sequência recebida.
    _lastRespLen = 0;      // Reseta o comprimento da última resposta.

    _parser.reset(); // Reseta o analisador.

    _tr.setTextEnabled(true); // Habilita o modo texto no transporte.

    _lastActivityMs = millis(); // Inicializa o watchdog de atividade do link.

    digitalWrite(LED_BUILTIN, LOW); // Garante que o LED esteja desligado inicialmente.
}

// Faz a leitura de dados recebidos e os processa.
void SdgwLink::poll()
{
    const SdgwEndpointKind endpointKind = _tr.kind();
    if (endpointKind != SDGW_ENDPOINT_NONE)
    {
        if (_activeEndpointKind != SDGW_ENDPOINT_NONE && endpointKind != _activeEndpointKind)
        {
            begin();
        }

        _activeEndpointKind = endpointKind;
    }

    if (endpointKind == SDGW_ENDPOINT_NONE || !_sessionOwner.isOwner(endpointKind))
    {
        checkHandshakeWatchdog();
        checkActivityWatchdog();
        setLedState();
        return;
    }

    while (_tr.available() > 0)
    {
        int v = _tr.readByte(); // Lê um byte do transporte.
        if (v < 0)
            break;
        uint8_t b = (uint8_t)v;

        // Processa o handshake se não estiver no estado Linked.
        if (_hs != Linked)
        {
            processHandshakeByte(b);
            continue;
        }

        // Processa os quadros do protocolo binário.
        SdgwParser::Result r = _parser.push(b);

        if (r == SdgwParser::FrameOk)
        {
            SdgwParser::Frame f;
            if (_parser.getFrame(f))
            {
                onValidFrameReceived(); // Qualquer frame SDGW valido renova a sessao.
                handleFrameOk(f); // Processa quadro válido.
            }
            continue;
        }

        // Trata erros nos quadros e responde com ERR se necessário.
        if (r == SdgwParser::FrameBadCrc ||
            r == SdgwParser::FrameTooSmall ||
            r == SdgwParser::FrameTooLarge ||
            r == SdgwParser::FrameBadCobs)
        {
            if (_parser.hasHeader())
            {
                uint8_t cmd, flags, seq;
                _parser.getHeader(cmd, flags, seq);

                const bool ackReq = (flags & (uint8_t)SDGW_FLAG_ACK_REQUIRED) != 0;

                // Retransmite a última resposta se a sequência coincidir.
                if (ackReq && _haveLastResp && seq == _lastRxSeq)
                {
                    _tr.writeBytes(_lastRespBuf, _lastRespLen);
                    continue;
                }

                if (ackReq)
                {
                    uint8_t errCode = (uint8_t)SDGW_ERR_BAD_CRC;

                    if (r == SdgwParser::FrameBadCrc)
                        errCode = (uint8_t)SDGW_ERR_BAD_CRC;
                    else if (r == SdgwParser::FrameTooSmall)
                        errCode = (uint8_t)SDGW_ERR_TOO_SMALL;
                    else if (r == SdgwParser::FrameTooLarge)
                        errCode = (uint8_t)SDGW_ERR_TOO_LARGE;
                    else if (r == SdgwParser::FrameBadCobs)
                        errCode = (uint8_t)SDGW_ERR_BAD_COBS;

                    sendErr(seq, errCode); // Envia resposta de erro.

                    _haveLastResp = true;
                    _lastRxSeq = seq;
                }
            }
        }
    }

    checkHandshakeWatchdog();

    // Verifica inatividade do link para evitar manter sessao zumbi.
    checkActivityWatchdog();

    // Atualiza o estado do LED para indicar o status do link.
    setLedState();
}

// Processa um byte durante a fase de handshake.
void SdgwLink::processHandshakeByte(uint8_t b)
{
    if (_hs != WaitingBanner)
        return;

    _lastActivityMs = millis();

    if (_bannerLen < (SDGW_HANDSHAKE_BUFFER - 1))
    {
        _bannerBuf[_bannerLen++] = (char)b;
        _bannerBuf[_bannerLen] = '\0';
    }
    else
    {
        _bannerLen = 0;
        memset(_bannerBuf, 0, sizeof(_bannerBuf));
        return;
    }

    const size_t need = strlen(SDGW_PC_BANNER);
    if (_bannerLen >= need)
    {
        const char *tail = &_bannerBuf[_bannerLen - need];
        if (memcmp(tail, SDGW_PC_BANNER, need) == 0)
        {

            sendBanner(); // Responde com o banner do dispositivo.

            _hs = Linked; // Transiciona para o estado Linked.

            _tr.setTextEnabled(false); // Desabilita o modo texto.

            _lastActivityMs = millis(); // Reseta o watchdog de atividade.

            _bannerLen = 0;
            memset(_bannerBuf, 0, sizeof(_bannerBuf));
        }
    }
}

// Envia o banner do dispositivo pelo transporte.
void SdgwLink::sendBanner()
{
    if (_activeEndpointKind == SDGW_ENDPOINT_NONE || !_sessionOwner.isOwner(_activeEndpointKind))
        return;

    _tr.writeBytes((const uint8_t *)SDGW_DEVICE_BANNER, strlen(SDGW_DEVICE_BANNER));
}

// Atualiza o tempo da ultima atividade SDGW valida recebida.
void SdgwLink::onValidFrameReceived()
{
    _lastActivityMs = millis();
}

// Altera o estado do LED_BUILTIN para indicar o status do link (opcional, pode ser usado para debug).
void SdgwLink::setLedState()
{
    static long tempoLed = millis();
    static bool state = false;
    switch (_hs)
    {
    case WaitingBanner:
        if (millis() - tempoLed >= 500)
        {
            state = !state;
            tempoLed = millis();
        }
        break;
    case Linked:
        if (state == false)
        {
            if (millis() - tempoLed >= 1950)
            {
                state = !state;
                tempoLed = millis();
            }
        }
        else
        {
            if (millis() - tempoLed >= 50)
            {
                state = !state;
                tempoLed = millis();
            }
        }

        break;
    }
    digitalWrite(LED_BUILTIN, state);
}

// Verifica se o watchdog de atividade expirou e reseta o link se necessário.
void SdgwLink::checkActivityWatchdog()
{
    if (_hs != Linked)
        return;
    if (_activityTimeoutMs == 0)
        return;

    uint32_t now = millis();
    if ((uint32_t)(now - _lastActivityMs) > (uint32_t)_activityTimeoutMs)
    {
        logout(); // Reseta o link em caso de timeout.
        _lastActivityMs = now;
    }
}

void SdgwLink::checkHandshakeWatchdog()
{
    if (_hs != WaitingBanner)
        return;

    if (_activeEndpointKind == SDGW_ENDPOINT_NONE)
        return;

    const uint32_t now = millis();
    if ((uint32_t)(now - _lastActivityMs) <= (uint32_t)SDGW_HANDSHAKE_TIMEOUT_MS)
        return;

    if (_sessionOwner.isOwner(_activeEndpointKind))
        _sessionOwner.release(_activeEndpointKind);

    begin();
    _lastActivityMs = now;
}

// Processa um quadro válido recebido pelo analisador.
void SdgwLink::handleFrameOk(const SdgwParser::Frame &f)
{
    if (f.cmd == (uint8_t)SDGW_CMD_ACK || f.cmd == (uint8_t)SDGW_CMD_ERR)
    {
        return; // Ignora comandos ACK/ERR vindos do PC.
    }

    const bool ackReq = (f.flags & (uint8_t)SDGW_FLAG_ACK_REQUIRED) != 0;

    if (f.cmd == (uint8_t)SDGW_CMD_PING)
    {
        if (ackReq)
        {
            if (_haveLastResp && f.seq == _lastRxSeq)
            {
                _tr.writeBytes(_lastRespBuf, _lastRespLen);
                return;
            }

            sendAck(f.seq);
            _haveLastResp = true;
            _lastRxSeq = f.seq;
        }

        return; // Não encaminha o PING para o dispositivo.
    }

    if (ackReq && _haveLastResp && f.seq == _lastRxSeq)
    {
        _tr.writeBytes(_lastRespBuf, _lastRespLen);
        return;
    }

    if (ackReq)
    {
        sendAck(f.seq);
        _haveLastResp = true;
        _lastRxSeq = f.seq;
    }

    if (_app)
    {
        _app->onCommand(f.cmd, f.flags, f.seq, f.data, f.dataLen); // Encaminha para o dispositivo.
    }
}

// Gera o próximo número de sequência de transmissão.
uint8_t SdgwLink::nextTxSeq()
{
    _txSeq++;
    if (_txSeq == 0)
        _txSeq = (uint8_t)SDGW_SEQ_START;
    return _txSeq;
}

// Envia um quadro de evento pelo transporte.
bool SdgwLink::sendEvent(uint8_t cmd, const uint8_t *payload, uint8_t payloadLen)
{
    const uint8_t seq = nextTxSeq();
    return sendFrame(cmd, (uint8_t)SDGW_FLAG_IS_EVENT, seq, payload, payloadLen, false);
}

bool SdgwLink::sendResponse(uint8_t cmd, const uint8_t *payload, uint8_t payloadLen)
{
    const uint8_t seq = nextTxSeq();
    return sendFrame(cmd, 0, seq, payload, payloadLen, false);
}

// Envia um quadro de reconhecimento (ACK).
void SdgwLink::sendAck(uint8_t rxSeq)
{
    sendFrame((uint8_t)SDGW_CMD_ACK, 0, rxSeq, nullptr, 0, true);
}

// Envia um quadro de erro com um código específico.
void SdgwLink::sendErr(uint8_t rxSeq, uint8_t errCode)
{
    uint8_t p[1] = {errCode};
    sendErrRaw(rxSeq, p, 1);
}

// Envia um quadro de erro bruto com um payload.
void SdgwLink::sendErrRaw(uint8_t rxSeq, const uint8_t *payload, uint8_t payloadLen)
{
    sendFrame((uint8_t)SDGW_CMD_ERR, 0, rxSeq, payload, payloadLen, true);
}

// Envia um quadro pelo transporte, opcionalmente armazenando-o como última resposta.
bool SdgwLink::sendFrame(uint8_t cmd, uint8_t flags, uint8_t seq,
                         const uint8_t *payload, uint8_t payloadLen,
                         bool cacheAsLastResp)
{
    if (_activeEndpointKind == SDGW_ENDPOINT_NONE || !_sessionOwner.isOwner(_activeEndpointKind))
        return false;

    if (payloadLen > (uint8_t)SDGW_MAX_PAYLOAD)
        return false;

    uint8_t logical[SDGW_MAX_LOGICAL_FRAME];
    size_t logicalLen = 0;

    logical[0] = cmd;
    logical[1] = flags;
    logical[2] = seq;
    logicalLen = 3;

    for (uint8_t i = 0; i < payloadLen; i++)
    {
        logical[logicalLen++] = payload[i];
    }

    const uint8_t crc = SdgwCrc8::compute(logical, logicalLen);
    logical[logicalLen++] = crc;

    uint8_t encoded[SDGW_MAX_ENCODED_FRAME];
    size_t encLen = 0;

    if (!SdgwCobs::encode(logical, logicalLen, encoded, sizeof(encoded), encLen))
        return false;

    if (encLen + 1 > sizeof(encoded))
        return false;
    encoded[encLen++] = (uint8_t)SDGW_COBS_DELIMITER;

    _tr.writeBytes(encoded, encLen);

    if (cacheAsLastResp)
    {
        if (encLen <= sizeof(_lastRespBuf))
        {
            memcpy(_lastRespBuf, encoded, encLen);
            _lastRespLen = encLen;
        }
    }

    return true;
}

// Faz logout e reseta o link para o estado inicial.
void SdgwLink::logout()
{
    if (_activeEndpointKind != SDGW_ENDPOINT_NONE)
    {
        _sessionOwner.release(_activeEndpointKind);
        _activeEndpointKind = SDGW_ENDPOINT_NONE;
    }

    begin(); // Reseta para WaitingBanner e limpa os buffers.
}
