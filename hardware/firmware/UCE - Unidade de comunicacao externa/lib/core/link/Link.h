#pragma once

#include <stdint.h>

#include "core/service/Service.h"
#include "core/transport/Transport.h"
#include "core/link/crc8.h"
#include "protocol/tlv/Tlv.h"

class Link {
public:
  Link(Transport& transport, Service& service);

  void begin();
  void tick();
  void poll();

private:
  bool parseAndValidate(const uint8_t* rx, uint8_t rxLen, TlvFrame& out, uint8_t& requestType, uint8_t& errorCode) const;

  Transport& _transport;
  Service& _service;
};
