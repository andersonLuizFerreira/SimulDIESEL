#pragma once

#include <stdint.h>

#include "IGatewayApp.h"
#include "GwRouter.h"
#include "GwErr.h"

class SdgwLink;

class GatewayApp : public IGatewayApp {
public:
    GatewayApp(SdgwLink& link, GwRouter& router);

    void begin();

    void onCommand(uint8_t cmd,
                   uint8_t flags,
                   uint8_t seq,
                   const uint8_t* data,
                   uint8_t dataLen) override;
    void tick();

private:
    static void onGsaIrqThunk();
    void onGsaIrq();
    void drainPendingGsaEvents();

    void handleGatewayLocal(uint8_t cmd,
                            const uint8_t* data,
                            uint8_t dataLen);

    void sendGatewayErrAsResponse(uint8_t cmd, GwErr err);

private:
    SdgwLink& _link;
    GwRouter& _router;
    volatile bool _gsaIrqLatched;

    static GatewayApp* _self;
};
