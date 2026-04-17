#pragma once

#include <Arduino.h>

#include "defs.h"

struct BoardCanPins {
  uint8_t controllerIndex;
  uint8_t peripheralPinIndex;
  uint8_t transceiverEnablePin;
  uint8_t transceiverStandbyPin;
  uint8_t transceiverWakePin;
  uint8_t transceiverFaultPin;
};

class BoardPins {
public:
  static uint8_t invalidPin();
  static const BoardCanPins& can0();
  static const BoardCanPins& can1();
  static void configureCanPins(uint8_t controllerIndex);
};
