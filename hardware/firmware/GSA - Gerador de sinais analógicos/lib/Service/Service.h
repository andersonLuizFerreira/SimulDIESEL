#pragma once
#include <stdint.h>
#include "LedService.h"
#include "Tlv.h"
#include "TlvBuilder.h"
#include "defs.h"

class Service {
public:
  explicit Service(LedService& led);
  bool handleOneTlv(const TlvFrame& tlv, uint8_t* txOut, uint8_t& txLenOut);

private:
  LedService& _led;
};