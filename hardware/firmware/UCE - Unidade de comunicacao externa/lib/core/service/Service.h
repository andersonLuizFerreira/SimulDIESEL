#pragma once

#include <stdint.h>

#include "defs.h"
#include "protocol/tlv/Tlv.h"
#include "services/can/CanService.h"
#include "services/led/LedService.h"

class Service {
public:
  Service(LedService& led, CanService& can);

  void begin();
  bool handleOneTlv(const TlvFrame& tlv, uint8_t* txOut, uint8_t& txLenOut);

private:
  bool handleBuiltinLed(const TlvFrame& tlv, uint8_t* txOut, uint8_t& txLenOut);
  bool handleCanConfig(const TlvFrame& tlv, uint8_t* txOut, uint8_t& txLenOut);
  bool handleCanEnable(const TlvFrame& tlv, uint8_t* txOut, uint8_t& txLenOut);
  bool handleCanStatus(const TlvFrame& tlv, uint8_t* txOut, uint8_t& txLenOut);
  bool handleCanReset(const TlvFrame& tlv, uint8_t* txOut, uint8_t& txLenOut);
  bool buildFunctionalError(uint8_t requestType, uint8_t errorCode, uint8_t* txOut, uint8_t& txLenOut) const;

  LedService& _led;
  CanService& _can;
};
