#pragma once

#include <stdint.h>

struct CanFilter {
  uint8_t mailboxIndex = 0;
  bool enabled = false;
  bool extendedId = false;
  bool overwrite = false;
  uint32_t id = 0;
  uint32_t mask = 0x7FF;
};
