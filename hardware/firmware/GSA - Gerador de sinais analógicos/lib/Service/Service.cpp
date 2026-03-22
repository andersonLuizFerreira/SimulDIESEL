#include "Service.h"

#include "TlvBuilder.h"

Service::Service(LedService &led, AnalogService& analog)
    : _led(led), _analog(analog)
{
}

void Service::begin()
{
  _analog.begin();
}

void Service::tick()
{
  _analog.tick();
}

bool Service::handleOneTlv(const TlvFrame &tlv, uint8_t *txOut, uint8_t &txLenOut)
{
  if (!txOut)
    return false;
  txLenOut = 0;

  switch (tlv.t)
  {
    case CMD_LED_BUILTIN:
      return handleBuiltinLed(tlv, txOut, txLenOut);

    case CMD_SETPOINT:
    case CMD_ENABLE_CHANNEL:
    case CMD_ENABLE_GLOBAL:
    case CMD_FAULT_RESET:
    case CMD_OFFSET_SET:
    case CMD_OFFSET_SAVE:
    case CMD_OFFSET_RESET_ALL:
    case CMD_STATUS_CHANNEL:
      return _analog.handleTlv(tlv, txOut, txLenOut);

    default:
      return buildFunctionalError(tlv.t, 0, GSA_ERROR_COMMAND_NOT_SUPPORTED, txOut, txLenOut);
  }
}

bool Service::handleBuiltinLed(const TlvFrame& tlv, uint8_t* txOut, uint8_t& txLenOut)
{
  if (tlv.l != 1 || tlv.v == nullptr) {
    return buildFunctionalError(CMD_LED_BUILTIN, 0, GSA_ERROR_INVALID_PAYLOAD, txOut, txLenOut);
  }

  if (tlv.v[0] > 1) {
    return buildFunctionalError(CMD_LED_BUILTIN, 0, GSA_ERROR_INVALID_STATE, txOut, txLenOut);
  }

  uint8_t applied = (uint8_t)_led.set(tlv.v[0]);
  txLenOut = TlvBuilder::buildU8(CMD_LED_BUILTIN, applied, txOut, TLV_MAX_LEN);
  return txLenOut != 0;
}

bool Service::buildFunctionalError(uint8_t requestType, uint8_t channel, uint8_t errorCode, uint8_t* txOut, uint8_t& txLenOut) const
{
  uint8_t payload[3] = { requestType, channel, errorCode };
  txLenOut = TlvBuilder::build(CMD_FUNCTIONAL_ERROR, payload, sizeof(payload), txOut, TLV_MAX_LEN);
  return txLenOut != 0;
}
