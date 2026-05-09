#pragma once

#include <stdint.h>

#include "services/can/service/CanService.h"

// SDCTP (SimulDIESEL CAN Transport Protocol) is the official CAN transport
// protocol layer for the UCE. This wrapper keeps the validated CanService
// implementation intact while exposing the protocol name to upper layers.
class SdctpService {
public:
  typedef CanService::EventPublisher EventPublisher;

  void begin() {
    _canService.begin();
  }

  void loop() {
    _canService.loop();
  }

  void setEventPublisher(EventPublisher publisher, void* context) {
    _canService.setEventPublisher(publisher, context);
  }

  bool handleTlv(uint8_t type,
                 const uint8_t* value,
                 uint8_t valueLen,
                 uint8_t* responseValue,
                 uint8_t& responseValueLen,
                 uint8_t& errorCode) {
    return _canService.handleTlv(type, value, valueLen, responseValue, responseValueLen, errorCode);
  }

  CanService& innerCanService() {
    return _canService;
  }

private:
  CanService _canService;
};
