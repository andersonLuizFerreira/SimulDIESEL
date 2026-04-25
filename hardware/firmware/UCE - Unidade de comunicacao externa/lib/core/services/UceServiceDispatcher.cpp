#include "core/services/UceServiceDispatcher.h"

#include "defs.h"

void UceServiceDispatcher::begin() {
  _led.begin();
}

bool UceServiceDispatcher::dispatch(uint8_t type,
                                    const uint8_t* value,
                                    uint8_t valueLen,
                                    uint8_t& responseType,
                                    uint8_t* responseValue,
                                    uint8_t& responseValueLen,
                                    uint8_t& errorCode) {
  responseType = type;
  responseValueLen = 0;
  errorCode = 0;

  switch (type) {
    case CMD_LED_BUILTIN:
      responseType = CMD_LED_BUILTIN;
      return _led.handleTlv(type, value, valueLen, responseValue, responseValueLen, errorCode);

    default:
      errorCode = UCE_ERROR_COMMAND_NOT_SUPPORTED;
      return false;
  }
}
