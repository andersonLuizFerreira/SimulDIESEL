#pragma once
#include <stdint.h>

class IGatewayApp {
public:
    virtual ~IGatewayApp() = default;

    virtual void onCommand(uint8_t cmd,
                           uint8_t flags,
                           uint8_t seq,
                           const uint8_t* data,
                           uint8_t dataLen) = 0;
};