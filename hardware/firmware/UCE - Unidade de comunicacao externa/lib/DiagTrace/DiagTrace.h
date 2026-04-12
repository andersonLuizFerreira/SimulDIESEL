#pragma once
#include <Arduino.h>
#include <stdint.h>
#include "defs.h"

class DiagTrace {
public:
  enum EventCode : uint8_t {
    EvTransportBegin = 1,
    EvTransportRequestCaptured = 2,
    EvTransportSetTx = 3,
    EvTransportPreload = 4,
    EvTransportAdvance = 5,
    EvTransportTxComplete = 6,
    EvLinkRxFrame = 7,
    EvLinkCrcError = 8,
    EvServiceHandle = 9,
    EvLinkResponseBuilt = 10
  };

  static void begin();
  static void flush();
  static void logState(uint8_t code, uint8_t a, uint8_t b, uint8_t c);
  static void logBytes(uint8_t code, const uint8_t* data, uint8_t len, uint8_t a = 0, uint8_t b = 0, uint8_t c = 0);

private:
  struct Event {
    uint32_t ts;
    uint8_t code;
    uint8_t a;
    uint8_t b;
    uint8_t c;
    uint8_t len;
    uint8_t data[TLV_MAX_LEN];
  };

  static bool enqueue(const Event& event);
  static void printEvent(const Event& event);
  static void printHexByte(uint8_t value);

  static volatile uint8_t _head;
  static volatile uint8_t _tail;
  static Event _queue[32];
  static bool _started;
};
