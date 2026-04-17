#include "protocol/tlv/Tlv.h"

uint8_t Tlv::build(uint8_t t, const uint8_t* payload, uint8_t payloadLen, uint8_t* out, uint8_t outMax) {
  if (!out) return 0;

  const uint8_t totalLen = (uint8_t)(payloadLen + 2);
  if (outMax < totalLen) return 0;

  out[0] = t;
  out[1] = payloadLen;

  for (uint8_t index = 0; index < payloadLen; ++index) {
    out[2 + index] = payload ? payload[index] : 0;
  }

  return totalLen;
}

uint8_t Tlv::buildU8(uint8_t t, uint8_t value, uint8_t* out, uint8_t outMax) {
  const uint8_t payload[1] = { value };
  return build(t, payload, 1, out, outMax);
}
