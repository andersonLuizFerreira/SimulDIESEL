#include "Service.h"

Service::Service(LedService &led)
    : _led(led)
{
}

bool Service::handleOneTlv(const TlvFrame &tlv, uint8_t *txOut, uint8_t &txLenOut)
{
  if (!txOut)
    return false;
  txLenOut = 0;

  switch (tlv.t)
  {
    case CMD_SET_LED: {
      if (tlv.l != 1 || tlv.v == nullptr) {
        return false;
      }

      int applied = _led.set(tlv.v[0]);
      txLenOut = TlvBuilder::buildU8(CMD_SET_LED, (uint8_t)applied, txOut, TLV_MAX_LEN);
      return true;
    }

    case CMD_GET_LED: {
      if (tlv.l != 0) {
        return false;
      }

      uint8_t st = (uint8_t)_led.get();
      txLenOut = TlvBuilder::buildU8(CMD_GET_LED, st, txOut, TLV_MAX_LEN);
      return true;
    }

    default:
      return false;
  }
}
