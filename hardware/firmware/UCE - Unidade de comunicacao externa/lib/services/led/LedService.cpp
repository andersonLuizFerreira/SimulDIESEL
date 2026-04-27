#include <Arduino.h>

#include "defs.h"
#include "services/led/LedService.h"

void LedService::begin() {
  pinMode(LED_PIN, OUTPUT);
  set(false);
}

bool LedService::set(bool on) {
  _state = on;
  digitalWrite(LED_PIN, on ? HIGH : LOW);
  return _state;
}

bool LedService::state() const {
  return _state;
}

bool LedService::handleTlv(uint8_t type,
                           const uint8_t* value,
                           uint8_t valueLen,
                           uint8_t* responseValue,
                           uint8_t& responseValueLen,
                           uint8_t& errorCode,
                           uint8_t* eventValue,
                           uint8_t& eventValueLen) {
  responseValueLen = 0;
  errorCode = 0;
  eventValueLen = 0;

  if (type != CMD_LED_BUILTIN) {
    errorCode = UCE_ERROR_COMMAND_NOT_SUPPORTED;
    return false;
  }

  if (!value || valueLen != 1) {
    errorCode = UCE_ERROR_INVALID_PAYLOAD;
    return false;
  }

  if (value[0] > 1) {
    errorCode = UCE_ERROR_INVALID_STATE;
    return false;
  }

  const bool currentState = set(value[0] != 0);
  if (responseValue) {
    responseValue[0] = currentState ? 0x01 : 0x00;
  }
  responseValueLen = 1;

  ++_eventCounter;
  if (eventValue) {
    eventValue[0] = currentState ? 0x01 : 0x00;
    eventValue[1] = 0x01;
    eventValue[2] = (uint8_t)(_eventCounter & 0xFFU);
    eventValue[3] = (uint8_t)((_eventCounter >> 8) & 0xFFU);
  }
  eventValueLen = 4;
  return true;
}
