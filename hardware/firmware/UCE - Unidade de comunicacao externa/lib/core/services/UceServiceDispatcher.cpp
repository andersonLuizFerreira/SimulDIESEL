#include "core/services/UceServiceDispatcher.h"

#include <Arduino.h>
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
  return dequeueEvent(eventType, eventValue, eventValueLen);
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
    case CMD_CAN_READ_ALL:
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
  if (!context) {
    return false;
  }

  UceServiceDispatcher* dispatcher = static_cast<UceServiceDispatcher*>(context);
  return dispatcher->enqueueEvent(type, value, valueLen);
}

bool UceServiceDispatcher::enqueueEvent(uint8_t type, const uint8_t* value, uint8_t valueLen) {
  if (type == 0 || valueLen > (MAX_DISPATCH_EVENT_SIZE - 2) || (valueLen > 0 && !value)) {
    return false;
  }

  if (_eventQueueCount >= DISPATCHER_EVENT_QUEUE_SIZE) {
    ++_dispatcherOverflowCount;
    markDispatcherOverflowDiagnosticPending();
    Serial.println("DISPATCHER FIFO OVERFLOW");
    return false;
  }

  DispatcherEvent& event = _eventQueue[_eventQueueTail];
  event.data[0] = type;
  event.data[1] = valueLen;
  if (valueLen > 0) {
    memcpy(&event.data[2], value, valueLen);
  }
  event.length = (uint8_t)(valueLen + 2);

  _eventQueueTail = (uint8_t)((_eventQueueTail + 1) % DISPATCHER_EVENT_QUEUE_SIZE);
  ++_eventQueueCount;
  return true;
}

bool UceServiceDispatcher::dequeueEvent(uint8_t& eventType, uint8_t* eventValue, uint8_t& eventValueLen) {
  eventType = 0;
  eventValueLen = 0;

  if (_eventQueueCount == 0) {
    enqueuePendingDispatcherOverflowDiagnostic();
    if (_eventQueueCount == 0) {
      return false;
    }
  }

  const DispatcherEvent& event = _eventQueue[_eventQueueHead];
  if (event.length < 2) {
    _eventQueueHead = (uint8_t)((_eventQueueHead + 1) % DISPATCHER_EVENT_QUEUE_SIZE);
    --_eventQueueCount;
    return false;
  }

  const uint8_t valueLen = event.data[1];
  if ((uint8_t)(valueLen + 2) != event.length || (valueLen > 0 && !eventValue)) {
    return false;
  }

  eventType = event.data[0];
  eventValueLen = valueLen;
  if (valueLen > 0) {
    memcpy(eventValue, &event.data[2], valueLen);
  }

  _eventQueueHead = (uint8_t)((_eventQueueHead + 1) % DISPATCHER_EVENT_QUEUE_SIZE);
  --_eventQueueCount;
  enqueuePendingDispatcherOverflowDiagnostic();
  return true;
}

void UceServiceDispatcher::markDispatcherOverflowDiagnosticPending() {
  if (_dispatcherOverflowCount == 1 || (_dispatcherOverflowCount % 10) == 0) {
    _pendingDispatcherOverflowReportCount = _dispatcherOverflowCount;
    _dispatcherOverflowDiagnosticPending = true;
  }
}

void UceServiceDispatcher::enqueuePendingDispatcherOverflowDiagnostic() {
  if (!_dispatcherOverflowDiagnosticPending || _eventQueueCount >= DISPATCHER_EVENT_QUEUE_SIZE) {
    return;
  }

  DispatcherEvent& event = _eventQueue[_eventQueueTail];
  event.data[0] = CMD_TRANSPORT_DIAG;
  event.data[1] = UCE_TRANSPORT_DIAG_DISPATCHER_FIFO_OVERFLOW_LEN;
  event.data[2] = UCE_TRANSPORT_DIAG_DISPATCHER_FIFO_OVERFLOW;
  event.data[3] = (uint8_t)(_pendingDispatcherOverflowReportCount & 0xFF);
  event.data[4] = (uint8_t)((_pendingDispatcherOverflowReportCount >> 8) & 0xFF);
  event.data[5] = (uint8_t)((_pendingDispatcherOverflowReportCount >> 16) & 0xFF);
  event.data[6] = (uint8_t)((_pendingDispatcherOverflowReportCount >> 24) & 0xFF);
  event.data[7] = DISPATCHER_EVENT_QUEUE_SIZE;
  event.data[8] = MAX_DISPATCH_EVENT_SIZE;
  event.length = (uint8_t)(UCE_TRANSPORT_DIAG_DISPATCHER_FIFO_OVERFLOW_LEN + 2);

  _eventQueueTail = (uint8_t)((_eventQueueTail + 1) % DISPATCHER_EVENT_QUEUE_SIZE);
  ++_eventQueueCount;
  _dispatcherOverflowDiagnosticPending = false;
}
