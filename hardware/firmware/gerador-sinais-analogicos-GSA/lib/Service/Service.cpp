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

      if (tlv.l < 1) {
        txLenOut = TlvBuilder::buildEmpty(CMD_SET_LED, txOut, TLV_MAX_LEN);
        return true;
      }
    
      int state = tlv.v[0];
    
      // toggle se for 2
      if (state == 2) {
        state = (_led.get() == 0) ? 1 : 0;
      }
    
      int applied = _led.set(state);
    
      txLenOut = TlvBuilder::buildU8(CMD_SET_LED, (uint8_t)applied, txOut, TLV_MAX_LEN);
      return true;
    }
    
    case CMD_GET_LED: {
      uint8_t st = (uint8_t)_led.get();
      txLenOut = TlvBuilder::buildU8(CMD_GET_LED, st, txOut, TLV_MAX_LEN);
      return true;
    }
  }


    
}