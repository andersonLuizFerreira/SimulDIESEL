#include "core/services/UceServiceDispatcher.h"

#include <string.h>

#include "defs.h"

void UceServiceDispatcher::begin() {
  _led.begin();
  _can.setEventPublisher(publishAsyncEvent, this);
  _can.begin();
}

void UceServiceDispatcher::loop() {
  _can.loop();
}

bool UceServiceDispatcher::takePendingEvent(uint8_t& eventType, uint8_t* eventValue, uint8_t& eventValueLen) {
  eventType = 0;
  eventValueLen = 0;
  if (!_pendingEvent || (_pendingEventValueLen > 0 && !eventValue)) {
    return false;
  }

  eventType = _pendingEventType;
  eventValueLen = _pendingEventValueLen;
  if (_pendingEventValueLen > 0) {
    memcpy(eventValue, _pendingEventValue, _pendingEventValueLen);
  }
  _pendingEvent = false;
  return true;
}

bool UceServiceDispatcher::dispatch(uint8_t type,
                                    const uint8_t* value,
                                    uint8_t valueLen,
                                    uint8_t& responseType,
                                    uint8_t* responseValue,
                                    uint8_t& responseValueLen,
                                    uint8_t& errorCode,
                                    uint8_t& eventType,
                                    uint8_t* eventValue,
                                    uint8_t& eventValueLen) {
  responseType = type;
  responseValueLen = 0;
  errorCode = 0;
  eventType = 0;
  eventValueLen = 0;

  switch (type) {
    case CMD_LED_BUILTIN:
      responseType = CMD_LED_BUILTIN;
      eventType = CMD_LED_EVENT;
      return _led.handleTlv(type, value, valueLen, responseValue, responseValueLen, errorCode, eventValue, eventValueLen);

    case CMD_CAN_CONFIG:
    case CMD_CAN_ENABLE:
    case CMD_CAN_STATUS:
    case CMD_CAN_RESET:
    case CMD_CAN_RX_POLL:
    case CMD_CAN_DRIVER_LOG_POLL:
    case CMD_CAN_TX:
    case CMD_CAN_TX_STOP:
      responseType = type;
      return _can.handleTlv(type, value, valueLen, responseValue, responseValueLen, errorCode);

    default:
      errorCode = UCE_ERROR_COMMAND_NOT_SUPPORTED;
      return false;
  }
}

bool UceServiceDispatcher::publishAsyncEvent(void* context, uint8_t type, const uint8_t* value, uint8_t valueLen) {
  if (!context || type == 0 || valueLen > TLV_MAX_LEN || (valueLen > 0 && !value)) {
    return false;
  }

  UceServiceDispatcher* dispatcher = static_cast<UceServiceDispatcher*>(context);
  if (dispatcher->_pendingEvent) {
    return false;
  }

  dispatcher->_pendingEventType = type;
  dispatcher->_pendingEventValueLen = valueLen;
  if (valueLen > 0) {
    memcpy(dispatcher->_pendingEventValue, value, valueLen);
  }
  dispatcher->_pendingEvent = true;
  return true;
}
