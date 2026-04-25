#pragma once

#include <stddef.h>
#include <stdint.h>

#include "core/link/SpiLink.h"
#include "core/services/UceServiceDispatcher.h"

class UceTransport {
public:
  UceTransport(SpiLink& link, UceServiceDispatcher& dispatcher)
      : _link(link), _dispatcher(dispatcher) {}

  void begin();
  void poll();

private:
  static uint8_t crc8(const uint8_t* data, size_t len);
  static bool parsePacket(const uint8_t* frame, size_t frameLen, uint8_t& type, const uint8_t*& value, uint8_t& valueLen);
  static size_t buildPacket(uint8_t type, const uint8_t* value, uint8_t valueLen, uint8_t* out, size_t outMax);
  static size_t buildFunctionalError(uint8_t requestType, uint8_t errorCode, uint8_t* out, size_t outMax);
  static bool findCobsFrame(const uint8_t* frame, size_t frameLen, size_t& cobsLen);
  static bool cobsEncode(const uint8_t* in, size_t inLen, uint8_t* out, size_t outMax, size_t& outLen);
  static bool cobsDecode(const uint8_t* in, size_t inLen, uint8_t* out, size_t outMax, size_t& outLen);

  SpiLink& _link;
  UceServiceDispatcher& _dispatcher;
};
