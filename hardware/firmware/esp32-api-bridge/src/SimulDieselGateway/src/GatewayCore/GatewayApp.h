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
    : _link(link), _router(router) {}

    void onCommand(uint8_t cmd,
                   uint8_t flags,
                   uint8_t seq,
                   const uint8_t* data,
                   uint8_t dataLen) override;

private:
    SggwLink& _link;
    GwRouter& _router;

    void handleGatewayLocal(uint8_t cmd,
                            const uint8_t* data,
                            uint8_t dataLen);

    void sendGatewayErrAsEvent(uint8_t cmd, GwErr err);
};