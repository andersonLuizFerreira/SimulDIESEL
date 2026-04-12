#include "Service.h"
#include "TlvBuilder.h"
#include "DiagTrace.h"

Service::Service(LedService& led)
  : _led(led)
{
}

void Service::begin() {
}

bool Service::handleOneTlv(const TlvFrame& tlv, uint8_t* txOut, uint8_t& txLenOut) {
  if (!txOut) return false;
  txLenOut = 0;

  uint8_t traceBuf[TLV_MAX_LEN];
  uint8_t traceLen = 0;
  traceBuf[traceLen++] = tlv.t;
  traceBuf[traceLen++] = tlv.l;
  for (uint8_t index = 0; index < tlv.l && traceLen < TLV_MAX_LEN; index++) traceBuf[traceLen++] = tlv.v[index];
  DiagTrace::logBytes(DiagTrace::EvServiceHandle, traceBuf, traceLen);

  switch (tlv.t) {
    case CMD_LED_BUILTIN:
      return handleBuiltinLed(tlv, txOut, txLenOut);
    default:
      return buildFunctionalError(tlv.t, UCE_ERROR_COMMAND_NOT_SUPPORTED, txOut, txLenOut);
  }
}

bool Service::handleBuiltinLed(const TlvFrame& tlv, uint8_t* txOut, uint8_t& txLenOut) {
  if (tlv.l != 1 || tlv.v == nullptr) {
    return buildFunctionalError(CMD_LED_BUILTIN, UCE_ERROR_INVALID_PAYLOAD, txOut, txLenOut);
  }

  if (tlv.v[0] > 1) {
    return buildFunctionalError(CMD_LED_BUILTIN, UCE_ERROR_INVALID_STATE, txOut, txLenOut);
  }

  uint8_t currentState = (uint8_t)_led.set(tlv.v[0]);
  txLenOut = TlvBuilder::buildU8(CMD_LED_BUILTIN, currentState, txOut, TLV_MAX_LEN);
  return txLenOut != 0;
}

bool Service::buildFunctionalError(uint8_t requestType, uint8_t errorCode, uint8_t* txOut, uint8_t& txLenOut) const {
  uint8_t payload[3] = { requestType, 0, errorCode };
  txLenOut = TlvBuilder::build(CMD_FUNCTIONAL_ERROR, payload, sizeof(payload), txOut, TLV_MAX_LEN);
  return txLenOut != 0;
}
