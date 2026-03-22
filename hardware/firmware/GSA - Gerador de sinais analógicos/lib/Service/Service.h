#pragma once
#include <stdint.h>

#include "AnalogService.h"
#include "LedService.h"
#include "Tlv.h"
#include "defs.h"

class Service {
public:
  Service(LedService& led, AnalogService& analog);

  void begin();
  void tick();
  bool handleOneTlv(const TlvFrame& tlv, uint8_t* txOut, uint8_t& txLenOut);

private:
  bool handleBuiltinLed(const TlvFrame& tlv, uint8_t* txOut, uint8_t& txLenOut);
  bool buildFunctionalError(uint8_t requestType, uint8_t channel, uint8_t errorCode, uint8_t* txOut, uint8_t& txLenOut) const;

  LedService& _led;
  AnalogService& _analog;
};
