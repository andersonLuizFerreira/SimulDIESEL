#pragma once

#include <stdint.h>

#include "services/can/CanService.h"
#include "services/led/LedService.h"

class UceServiceDispatcher {
public:
  UceServiceDispatcher(LedService& led, CanService& can)
      : _led(led), _can(can) {}

  void begin();
  void loop();

  bool dispatch(uint8_t type,
                const uint8_t* value,
                uint8_t valueLen,
                uint8_t& responseType,
                uint8_t* responseValue,
                uint8_t& responseValueLen,
                uint8_t& errorCode,
                uint8_t& eventType,
                uint8_t* eventValue,
                uint8_t& eventValueLen);

private:
  LedService& _led;
  CanService& _can;
};
