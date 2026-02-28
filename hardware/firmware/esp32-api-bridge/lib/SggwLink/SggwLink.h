#include <Arduino.h>
#pragma once
#include <stdint.h>
#include <stddef.h>
#include <string.h>


#include "Sggw.defs.h"
#include "SggwTransport.h"
#include "SggwParser.h"
#include "SggwCobs.h"
#include "SggwCrc8.h"

class IGatewayApp;

/// Representa o link de comunicação entre o ESP32 e um dispositivo.
class SggwLink {
public:
    /// Define os estados do handshake do link.
    enum HandshakeState {
        WaitingBanner, ///< Aguardando o banner de handshake.
        Linked         ///< Handshake concluído, link estabelecido.
    };

    /// Construtor da classe SggwLink.
    explicit SggwLink(SggwTransport& transport);

    /// Ajusta timeout do watchdog de PING. (0 = desabilita)
    void setPingTimeoutMs(uint16_t ms) { _pingTimeoutMs = ms; }

    /// Anexa um dispositivo ao link.
    void attachApp(IGatewayApp* app) { _app = app; }

    /// Inicializa o link. Deve ser chamado antes de usar o link.
    void begin();

    /// Realiza a verificação de dados recebidos e processa-os.
    void poll();

    /// Envia um evento para o dispositivo.
    bool sendEvent(uint8_t cmd, const uint8_t* payload, uint8_t payloadLen);

    /// Envia uma resposta de erro para o dispositivo.
    void sendErr(uint8_t rxSeq, uint8_t errCode);

    /// Realiza o logout da sessão atual, redefinindo o estado do link.
    void logout();

    /// Verifica se o link está estabelecido.
    bool isLinked() const { return _hs == Linked; }

private:
    // Watchdog de PING (keep-alive)
    uint32_t _lastPingMs;
    uint16_t _pingTimeoutMs;

    void onPingReceived();
    void checkPingWatchdog();

private:
    SggwTransport& _tr; ///< Referência para a camada de transporte.
    SggwParser _parser; ///< Parser para lidar com os quadros recebidos.
    IGatewayApp* _app;   ///< Ponteiro para a aplicação anexada.

    HandshakeState _hs; ///< Estado atual do handshake.

    char   _bannerBuf[SGGW_HANDSHAKE_BUFFER]; ///< Buffer para armazenar o banner de handshake.
    size_t _bannerLen;                        ///< Comprimento do banner de handshake.

    uint8_t _txSeq; ///< Número de sequência atual para transmissão.

    bool    _haveLastResp; ///< Indica se a última resposta está em cache.
    uint8_t _lastRxSeq;    ///< Número de sequência do último quadro recebido.
    uint8_t _lastRespBuf[SGGW_MAX_LAST_RESPONSE]; ///< Buffer para a última resposta.
    size_t  _lastRespLen;                         ///< Comprimento da última resposta.

private:
    /// Controla o LED_BUILTIN do ESP32 para indicar o estado do link.
    void setLedState();
    
    /// Processa um byte recebido durante o handshake.
    void processHandshakeByte(uint8_t b);

    /// Envia o banner de handshake para o dispositivo.
    void sendBanner();

    /// Lida com um quadro recebido e analisado com sucesso.
    void handleFrameOk(const SggwParser::Frame& f);

    /// Envia um reconhecimento (ACK) para um quadro recebido.
    void sendAck(uint8_t rxSeq);

    /// Envia uma resposta de erro bruta.
    void sendErrRaw(uint8_t rxSeq, const uint8_t* payload, uint8_t payloadLen);

    /// Envia um quadro para o dispositivo.
    bool sendFrame(uint8_t cmd, uint8_t flags, uint8_t seq,
                   const uint8_t* payload, uint8_t payloadLen,
                   bool cacheAsLastResp);

    /// Gera o próximo número de sequência para transmissão.
    uint8_t nextTxSeq();
};
