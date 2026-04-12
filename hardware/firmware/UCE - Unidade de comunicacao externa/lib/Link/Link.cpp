#include "Link.h"
#include "TlvBuilder.h"
#include "DiagTrace.h"

Link::Link(Transport& transport, Service& service)
  : _transport(transport), _service(service)
{
}

void Link::begin() {
  Transport::setIrqActive(false);
}

void Link::tick() {
}

bool Link::parseAndValidate(const uint8_t* rx, uint8_t rxLen, TlvFrame& out, uint8_t& requestType, uint8_t& errorCode) const {
  requestType = (rx && rxLen > 0) ? rx[0] : 0;
  errorCode = UCE_ERROR_INVALID_PAYLOAD;

  if (!rx || rxLen < 3) return false;

  const uint8_t t = rx[0];
  const uint8_t l = rx[1];
  const uint8_t expectedLen = (uint8_t)(2 + l + 1);
  if (expectedLen != rxLen) return false;

  const uint8_t crcRx = rx[rxLen - 1];
  const uint8_t crcCalc = Crc8::calc(rx, rxLen - 1);
  if (crcCalc != crcRx) {
    errorCode = UCE_ERROR_INVALID_TLV_CRC;
    DiagTrace::logBytes(DiagTrace::EvLinkCrcError, rx, rxLen, crcCalc, crcRx, 0);
    return false;
  }

  out.t = t;
  out.l = l;
  out.v = (l > 0) ? &rx[2] : nullptr;
  return true;
}

void Link::poll() {
  uint8_t rxBuf[TLV_MAX_LEN];
  uint8_t rxLen = 0;

  if (!_transport.popRx(rxBuf, rxLen)) return;
  DiagTrace::logBytes(DiagTrace::EvLinkRxFrame, rxBuf, rxLen);

  TlvFrame tlv;
  uint8_t requestType = 0;
  uint8_t errorCode = UCE_ERROR_INVALID_PAYLOAD;
  uint8_t txTlv[TLV_MAX_LEN];
  uint8_t txTlvLen = 0;

  if (!parseAndValidate(rxBuf, rxLen, tlv, requestType, errorCode)) {
    uint8_t payload[3] = { requestType, 0, errorCode };
    txTlvLen = TlvBuilder::build(CMD_FUNCTIONAL_ERROR, payload, sizeof(payload), txTlv, TLV_MAX_LEN);
  } else if (!_service.handleOneTlv(tlv, txTlv, txTlvLen)) {
    uint8_t payload[3] = { tlv.t, 0, UCE_ERROR_COMMAND_NOT_SUPPORTED };
    txTlvLen = TlvBuilder::build(CMD_FUNCTIONAL_ERROR, payload, sizeof(payload), txTlv, TLV_MAX_LEN);
  }

  if (txTlvLen >= 2 && (uint8_t)(txTlvLen + 1) <= TLV_MAX_LEN) {
    uint8_t out[TLV_MAX_LEN];
    for (uint8_t index = 0; index < txTlvLen; index++) out[index] = txTlv[index];
    out[txTlvLen] = Crc8::calc(out, txTlvLen);
    DiagTrace::logBytes(DiagTrace::EvLinkResponseBuilt, out, (uint8_t)(txTlvLen + 1), out[txTlvLen], 0, 0);
    _transport.setTx(out, (uint8_t)(txTlvLen + 1));
  }
}
