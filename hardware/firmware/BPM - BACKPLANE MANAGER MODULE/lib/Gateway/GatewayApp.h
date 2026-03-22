#pragma once

#include <stdint.h>

#include "IGatewayApp.h"
#include "GwRouter.h"
#include "GwErr.h"

// Forward declaration para evitar include circular
class SggwLink;

class GatewayApp : public IGatewayApp {
public:
    GatewayApp(SggwLink& link, GwRouter& router)
    : _link(link), _router(router), _gsaState(GsaRemoteState::Idle) {}

    // O host envia o comando compacto ja resolvido.
    // A BPM trata ADDR 0 localmente e roteia os demais enderecos.
    void onCommand(uint8_t cmd,
                   uint8_t flags,
                   uint8_t seq,
                   const uint8_t* data,
                   uint8_t dataLen) override;
    void tick();

private:
    enum class GsaRemoteState {
        Idle,
        Busy
    };

    SggwLink& _link;
    GwRouter& _router;
    GsaRemoteState _gsaState;

    void handleGatewayLocal(uint8_t cmd,
                            const uint8_t* data,
                            uint8_t dataLen);

    bool isGsaBusEvent(const uint8_t* payload, size_t payloadLen, uint8_t* eventTypeOut = nullptr) const;
    void sendGatewayErrAsResponse(uint8_t cmd, GwErr err);
};
