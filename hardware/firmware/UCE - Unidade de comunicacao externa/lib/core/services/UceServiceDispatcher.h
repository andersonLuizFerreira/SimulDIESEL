#pragma once

#include <stdint.h>

#include "defs.h"
#include "services/can/service/CanService.h"
#include "services/led/LedService.h"

class UceServiceDispatcher {
public:
  UceServiceDispatcher(LedService& led, CanService& can)
      : _led(led), _can(can) {}

  void begin();
  void loop();
  bool takePendingEvent(uint8_t& eventType, uint8_t* eventValue, uint8_t& eventValueLen);

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
  static bool publishAsyncEvent(void* context, uint8_t type, const uint8_t* value, uint8_t valueLen);

  LedService& _led;
  CanService& _can;
  bool _pendingEvent = false;
  uint8_t _pendingEventType = 0;
  uint8_t _pendingEventValue[TLV_MAX_LEN] = {0};
  uint8_t _pendingEventValueLen = 0;
};
