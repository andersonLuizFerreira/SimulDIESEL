#include <Arduino.h>
#pragma once
#include <stdint.h>
#include <stddef.h>
#include <string.h>


#include "SdgwDefs.h"
#include "ISdgwEndpoint.h"
#include "SdgwSessionOwner.h"
#include "SdgwParser.h"
#include "SdgwCobs.h"
#include "SdgwCrc8.h"

class IGatewayApp;

/// Representa o link de comunicação entre o ESP32 e um dispositivo.
class SdgwLink {
public:
    /// Define os estados do handshake do link.
    enum HandshakeState {
        WaitingBanner, ///< Aguardando o banner de handshake.
        Linked         ///< Handshake concluído, link estabelecido.
    };

    /// Construtor da camada de link do gateway.
    explicit SdgwLink(ISdgwEndpoint& transport, SdgwSessionOwner& sessionOwner);

    /// Ajusta timeout do watchdog de atividade do link. (0 = desabilita)
    void setActivityTimeoutMs(uint16_t ms) { _activityTimeoutMs = ms; }

    /// Anexa um dispositivo ao link.
    void attachApp(IGatewayApp* app) { _app = app; }

    /// Inicializa o link. Deve ser chamado antes de usar o link.
    void begin();

    /// Realiza a verificação de dados recebidos e processa-os.
    void poll();

    /// Envia um evento para o dispositivo.
    bool sendEvent(uint8_t cmd, const uint8_t* payload, uint8_t payloadLen);
    bool sendResponse(uint8_t cmd, const uint8_t* payload, uint8_t payloadLen);

    /// Envia uma resposta de erro para o dispositivo.
    void sendErr(uint8_t rxSeq, uint8_t errCode);

    /// Realiza o logout da sessão atual, redefinindo o estado do link.
    void logout();

    /// Verifica se o link está estabelecido.
    bool isLinked() const { return _hs == Linked; }

private:
    // Watchdog de atividade do link (qualquer frame SDGW valido recebido).
    uint32_t _lastActivityMs;
    uint16_t _activityTimeoutMs;

    void onValidFrameReceived();
    void checkActivityWatchdog();
    void checkHandshakeWatchdog();

private:
    ISdgwEndpoint& _tr; ///< Referência para a camada de transporte.
    SdgwSessionOwner& _sessionOwner; ///< Guarda o owner único da sessão SDGW.
    SdgwParser _parser; ///< Parser para lidar com os quadros recebidos.
    IGatewayApp* _app;   ///< Ponteiro para a aplicação anexada.

    HandshakeState _hs; ///< Estado atual do handshake.

    char   _bannerBuf[SDGW_HANDSHAKE_BUFFER]; ///< Buffer para armazenar o banner de handshake.
    size_t _bannerLen;                        ///< Comprimento do banner de handshake.

    uint8_t _txSeq; ///< Número de sequência atual para transmissão.

    bool    _haveLastResp; ///< Indica se a última resposta está em cache.
    uint8_t _lastRxSeq;    ///< Número de sequência do último quadro recebido.
    uint8_t _lastRespBuf[SDGW_MAX_LAST_RESPONSE]; ///< Buffer para a última resposta.
    size_t  _lastRespLen;                         ///< Comprimento da última resposta.
    SdgwEndpointKind _activeEndpointKind;        ///< Owner atual do link durante a sessao.

private:
    /// Controla o LED_BUILTIN do ESP32 para indicar o estado do link.
    void setLedState();
    
    /// Processa um byte recebido durante o handshake.
    void processHandshakeByte(uint8_t b);

    /// Envia o banner de handshake para o dispositivo.
    void sendBanner();

    /// Lida com um quadro recebido e analisado com sucesso.
    void handleFrameOk(const SdgwParser::Frame& f);

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
