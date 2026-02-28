#pragma once
#include <stdint.h>
#include "defs.h"
#include "Tlv.h"
#include "crc8.h"
#include "Transport.h"
#include "Service.h"

class Link {
public:
  Link(Transport& tr, Service& svc);

  void begin();
  void poll();

private:
  void setError(uint8_t code, uint8_t lastT);
  void clearError();

  bool parseAndValidate(const uint8_t* rx, uint8_t rxLen, TlvFrame& out);
  bool handleLinkCmd(const TlvFrame& tlv, uint8_t* txTlvOut, uint8_t& txTlvLenOut);

private:
  Transport& _tr;
  Service& _svc;

  uint8_t _errCode;
  uint8_t _errLastT;
  bool    _hasErr;
};