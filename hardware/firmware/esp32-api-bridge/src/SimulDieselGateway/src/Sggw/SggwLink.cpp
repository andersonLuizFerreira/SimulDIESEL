#include "SggwLink.h"
#include "SggwDevice.h"
#include "src/GatewayCore/IGatewayApp.h"

// Construtor da classe SggwLink, inicializa as variáveis membro e buffers.
SggwLink::SggwLink(SggwTransport &transport)
    : _tr(transport),
      _parser(),
      _app(nullptr),
      _hs(WaitingBanner),
      _bannerLen(0),
      _txSeq(SGGW_SEQ_START),
      _haveLastResp(false),
      _lastRxSeq(0),
      _lastRespLen(0),
      _lastPingMs(0),
      _pingTimeoutMs(2000)
{
    // Inicializa os buffers de banner e resposta com zeros.
    memset(_bannerBuf, 0, sizeof(_bannerBuf));
    memset(_lastRespBuf, 0, sizeof(_lastRespBuf));
    pinMode(LED_BUILTIN, OUTPUT);
}

// Reseta o estado do link e o prepara para operação.
void SggwLink::begin()
{
    _hs = WaitingBanner; // Define o estado de handshake para WaitingBanner.
    _bannerLen = 0;      // Reseta o comprimento do banner.

    _txSeq = (uint8_t)SGGW_SEQ_START; // Reseta a sequência de transmissão.

    _haveLastResp = false; // Limpa o estado da última resposta.
    _lastRxSeq = 0;        // Reseta a sequência recebida.
    _lastRespLen = 0;      // Reseta o comprimento da última resposta.

    _parser.reset(); // Reseta o analisador.

    _tr.setTextEnabled(true); // Habilita o modo texto no transporte.

    _lastPingMs = millis(); // Inicializa o temporizador de watchdog.

    digitalWrite(LED_BUILTIN, LOW); // Garante que o LED esteja desligado inicialmente.
}

// Faz a leitura de dados recebidos e os processa.
void SggwLink::poll()
{
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
        SggwParser::Result r = _parser.push(b);

        if (r == SggwParser::FrameOk)
        {
            SggwParser::Frame f;
            if (_parser.getFrame(f))
                handleFrameOk(f); // Processa quadro válido.
            continue;
        }

        // Trata erros nos quadros e responde com ERR se necessário.
        if (r == SggwParser::FrameBadCrc ||
            r == SggwParser::FrameTooSmall ||
            r == SggwParser::FrameTooLarge ||
            r == SggwParser::FrameBadCobs)
        {
            if (_parser.hasHeader())
            {
                uint8_t cmd, flags, seq;
                _parser.getHeader(cmd, flags, seq);

                const bool ackReq = (flags & (uint8_t)SGGW_FLAG_ACK_REQUIRED) != 0;

                // Retransmite a última resposta se a sequência coincidir.
                if (ackReq && _haveLastResp && seq == _lastRxSeq)
                {
                    _tr.writeBytes(_lastRespBuf, _lastRespLen);
                    continue;
                }

                if (ackReq)
                {
                    uint8_t errCode = (uint8_t)SGGW_ERR_BAD_CRC;

                    if (r == SggwParser::FrameBadCrc)
                        errCode = (uint8_t)SGGW_ERR_BAD_CRC;
                    else if (r == SggwParser::FrameTooSmall)
                        errCode = (uint8_t)SGGW_ERR_TOO_SMALL;
                    else if (r == SggwParser::FrameTooLarge)
                        errCode = (uint8_t)SGGW_ERR_TOO_LARGE;
                    else if (r == SggwParser::FrameBadCobs)
                        errCode = (uint8_t)SGGW_ERR_BAD_COBS;

                    sendErr(seq, errCode); // Envia resposta de erro.

                    _haveLastResp = true;
                    _lastRxSeq = seq;
                }
            }
        }
    }

    // Sempre verifica o watchdog de ping para garantir que o link está ativo.
    checkPingWatchdog();

    // Atualiza o estado do LED para indicar o status do link.
    setLedState();
}

// Processa um byte durante a fase de handshake.
void SggwLink::processHandshakeByte(uint8_t b)
{
    if (_hs != WaitingBanner)
        return;

    if (_bannerLen < (SGGW_HANDSHAKE_BUFFER - 1))
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

    const size_t need = strlen(SGGW_PC_BANNER);
    if (_bannerLen >= need)
    {
        const char *tail = &_bannerBuf[_bannerLen - need];
        if (memcmp(tail, SGGW_PC_BANNER, need) == 0)
        {

            sendBanner(); // Responde com o banner do dispositivo.

            _hs = Linked; // Transiciona para o estado Linked.

            _tr.setTextEnabled(false); // Desabilita o modo texto.

            _lastPingMs = millis(); // Reseta o temporizador de watchdog.

            _bannerLen = 0;
            memset(_bannerBuf, 0, sizeof(_bannerBuf));
        }
    }
}

// Envia o banner do dispositivo pelo transporte.
void SggwLink::sendBanner()
{
    _tr.writeBytes((const uint8_t *)SGGW_DEVICE_BANNER, strlen(SGGW_DEVICE_BANNER));
}

// Atualiza o tempo do último ping recebido.
void SggwLink::onPingReceived()
{
    _lastPingMs = millis();
}

// Altera o estado do LED_BUILTIN para indicar o status do link (opcional, pode ser usado para debug).
void SggwLink::setLedState()
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

// Verifica se o watchdog de ping expirou e reseta o link se necessário.
void SggwLink::checkPingWatchdog()
{
    if (_hs != Linked)
        return;
    if (_pingTimeoutMs == 0)
        return;

    uint32_t now = millis();
    if ((uint32_t)(now - _lastPingMs) > (uint32_t)_pingTimeoutMs)
    {
        logout(); // Reseta o link em caso de timeout.
        _lastPingMs = now;
    }
}

// Processa um quadro válido recebido pelo analisador.
void SggwLink::handleFrameOk(const SggwParser::Frame &f)
{
    if (f.cmd == (uint8_t)SGGW_CMD_ACK || f.cmd == (uint8_t)SGGW_CMD_ERR)
    {
        return; // Ignora comandos ACK/ERR vindos do PC.
    }

    const bool ackReq = (f.flags & (uint8_t)SGGW_FLAG_ACK_REQUIRED) != 0;

    if (f.cmd == (uint8_t)SGGW_CMD_PING)
    {
        onPingReceived();

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
uint8_t SggwLink::nextTxSeq()
{
    _txSeq++;
    if (_txSeq == 0)
        _txSeq = (uint8_t)SGGW_SEQ_START;
    return _txSeq;
}

// Envia um quadro de evento pelo transporte.
bool SggwLink::sendEvent(uint8_t cmd, const uint8_t *payload, uint8_t payloadLen)
{
    const uint8_t seq = nextTxSeq();
    return sendFrame(cmd, (uint8_t)SGGW_FLAG_IS_EVENT, seq, payload, payloadLen, false);
}

// Envia um quadro de reconhecimento (ACK).
void SggwLink::sendAck(uint8_t rxSeq)
{
    sendFrame((uint8_t)SGGW_CMD_ACK, 0, rxSeq, nullptr, 0, true);
}

// Envia um quadro de erro com um código específico.
void SggwLink::sendErr(uint8_t rxSeq, uint8_t errCode)
{
    uint8_t p[1] = {errCode};
    sendErrRaw(rxSeq, p, 1);
}

// Envia um quadro de erro bruto com um payload.
void SggwLink::sendErrRaw(uint8_t rxSeq, const uint8_t *payload, uint8_t payloadLen)
{
    sendFrame((uint8_t)SGGW_CMD_ERR, 0, rxSeq, payload, payloadLen, true);
}

// Envia um quadro pelo transporte, opcionalmente armazenando-o como última resposta.
bool SggwLink::sendFrame(uint8_t cmd, uint8_t flags, uint8_t seq,
                         const uint8_t *payload, uint8_t payloadLen,
                         bool cacheAsLastResp)
{
    if (payloadLen > (uint8_t)SGGW_MAX_PAYLOAD)
        return false;

    uint8_t logical[SGGW_MAX_LOGICAL_FRAME];
    size_t logicalLen = 0;

    logical[0] = cmd;
    logical[1] = flags;
    logical[2] = seq;
    logicalLen = 3;

    for (uint8_t i = 0; i < payloadLen; i++)
    {
        logical[logicalLen++] = payload[i];
    }

    const uint8_t crc = SggwCrc8::compute(logical, logicalLen);
    logical[logicalLen++] = crc;

    uint8_t encoded[SGGW_MAX_ENCODED_FRAME];
    size_t encLen = 0;

    if (!SggwCobs::encode(logical, logicalLen, encoded, sizeof(encoded), encLen))
        return false;

    if (encLen + 1 > sizeof(encoded))
        return false;
    encoded[encLen++] = (uint8_t)SGGW_COBS_DELIMITER;

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
void SggwLink::logout()
{
    begin(); // Reseta para WaitingBanner e limpa os buffers.
}
