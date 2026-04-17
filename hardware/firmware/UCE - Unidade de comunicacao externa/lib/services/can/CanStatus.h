#pragma once

#include <stdint.h>

#include "services/can/CanTypes.h"

struct CanStatus {
  UceCan::InterfaceState state = UceCan::InterfaceState::Disabled;
  UceCan::Mode mode = UceCan::Mode::Normal;
  uint32_t bitrateKbps = 0;
  uint32_t controllerStatus = 0;
  uint8_t txErrorCount = 0;
  uint8_t rxErrorCount = 0;
  bool configured = false;
  bool interfaceEnabled = false;
  bool synchronized = false;
  bool busOff = false;
  bool errorPassive = false;
  bool errorWarning = false;
  bool transceiverFault = false;
};
