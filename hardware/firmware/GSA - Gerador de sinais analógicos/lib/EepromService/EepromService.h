#pragma once

#include <stdint.h>

#include "defs.h"

class EepromService {
public:
  bool loadOffsets(GsaChannelOffsets* outOffsets, uint8_t count);
  bool saveOffsets(const GsaChannelOffsets* offsets, uint8_t count);
};
