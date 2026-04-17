#pragma once

#include "hal/transceivers/CanTransceiver.h"

class NullCanTransceiver : public CanTransceiver {
public:
  bool enable() override;
  bool disable() override;
  bool standby() override;
  bool wake() override;
  bool hasFault() const override;
  bool isPresent() const override;
};
