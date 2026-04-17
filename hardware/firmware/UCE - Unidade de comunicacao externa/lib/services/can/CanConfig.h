#pragma once

#include "services/can/CanTypes.h"

struct CanConfig {
  UceCan::Controller controller = UceCan::Controller::Can0;
  uint32_t bitrateKbps = 250;
  UceCan::Mode mode = UceCan::Mode::Normal;
  bool autoRecoverBusOff = false;
};
