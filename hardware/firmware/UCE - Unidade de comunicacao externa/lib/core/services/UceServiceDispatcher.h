#pragma once

#include <stdint.h>

#include "services/can/CanService.h"
#include "services/led/LedService.h"

class UceServiceDispatcher {
public:
  UceServiceDispatcher(LedService& led, CanService& can)
      : _led(led), _can(can) {}

  void begin();

  bool dispatch(uint8_t type,
                const uint8_t* value,
                uint8_t valueLen,
                uint8_t& responseType,
                uint8_t* responseValue,
                uint8_t& responseValueLen,
                uint8_t& errorCode);

private:
  LedService& _led;
  CanService& _can;
};
