#pragma once

#include <stdint.h>

#include "defs.h"
#include "services/can/CanTypes.h"

struct CanMailbox {
  uint8_t index = 0;
  UceCan::MailboxDirection direction = UceCan::MailboxDirection::Disabled;
  bool enabled = false;
  bool extendedId = false;
  uint32_t id = 0;
  uint32_t mask = 0;
  uint8_t length = 0;
  uint8_t priority = 0;
  uint8_t data[8] = {0};
};
