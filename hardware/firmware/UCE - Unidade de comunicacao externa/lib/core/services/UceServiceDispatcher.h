#pragma once

#include <stdint.h>

#include "defs.h"
#include "services/can/service/CanService.h"
#include "services/led/LedService.h"

#define DISPATCHER_EVENT_QUEUE_SIZE 20
#define MAX_DISPATCH_EVENT_SIZE 32

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
  struct DispatcherEvent {
    uint8_t data[MAX_DISPATCH_EVENT_SIZE];
    uint8_t length;
  };

  static bool publishAsyncEvent(void* context, uint8_t type, const uint8_t* value, uint8_t valueLen);

  bool enqueueEvent(uint8_t type, const uint8_t* value, uint8_t valueLen);
  bool dequeueEvent(uint8_t& eventType, uint8_t* eventValue, uint8_t& eventValueLen);
  void markDispatcherOverflowDiagnosticPending();
  void enqueuePendingDispatcherOverflowDiagnostic();

  LedService& _led;
  CanService& _can;
  DispatcherEvent _eventQueue[DISPATCHER_EVENT_QUEUE_SIZE];
  uint8_t _eventQueueHead = 0;
  uint8_t _eventQueueTail = 0;
  uint8_t _eventQueueCount = 0;
  uint32_t _dispatcherOverflowCount = 0;
  uint32_t _pendingDispatcherOverflowReportCount = 0;
  bool _dispatcherOverflowDiagnosticPending = false;
};
