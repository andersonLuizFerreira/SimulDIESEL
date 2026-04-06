#pragma once
#include <stdint.h>
#include "defs.h"
#include "Tlv.h"
#include "crc8.h"
#include "Transport.h"
#include "Service.h"

class Link {
public:
  Link(Transport& tr, Service& svc);

  void begin();
  void tick();
  void poll();

private:
  bool parseAndValidate(const uint8_t* rx, uint8_t rxLen, TlvFrame& out);

private:
  Transport& _tr;
  Service& _svc;
};
