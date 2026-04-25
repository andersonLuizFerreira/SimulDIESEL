#pragma once

#include <stdint.h>

#include "services/led/LedService.h"

class UceServiceDispatcher {
public:
  explicit UceServiceDispatcher(LedService& led)
      : _led(led) {}

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
};
