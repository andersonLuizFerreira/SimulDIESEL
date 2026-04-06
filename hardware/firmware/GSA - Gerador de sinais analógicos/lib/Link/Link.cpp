#include "Link.h"
#include "TlvBuilder.h"

Link::Link(Transport& tr, Service& svc)
: _tr(tr), _svc(svc)
{}

void Link::begin() {
  Transport::setIrqActive(false);
}

void Link::tick() {
  if (Transport::hasTxPending()) {
    Transport::setIrqActive(Transport::hasAsyncEventTxPending() || _svc.hasPendingEvent());
    return;
  }

  uint8_t txTlv[TLV_MAX_LEN];
  uint8_t txTlvLen = 0;
  if (!_svc.popPendingEvent(txTlv, txTlvLen) || txTlvLen < 2) {
    Transport::setIrqActive(false);
    return;
  }

  uint8_t out[TLV_MAX_LEN];
  for (uint8_t i = 0; i < txTlvLen; i++) {
    out[i] = txTlv[i];
  }

  out[txTlvLen] = Crc8::calc(out, txTlvLen);
  _tr.setTx(out, (uint8_t)(txTlvLen + 1), true);
  Transport::setIrqActive(true);
}

bool Link::parseAndValidate(const uint8_t* rx, uint8_t rxLen, TlvFrame& out) {
  if (!rx || rxLen < 3) {
    return false;
  }

  uint8_t t = rx[0];
  uint8_t l = rx[1];

  uint8_t expected = (uint8_t)(2 + l + 1);
  if (expected != rxLen) {
    return false;
  }

  out.t = t;
  out.l = l;
  out.v = (l > 0) ? &rx[2] : nullptr;

  uint8_t crcRx = rx[rxLen - 1];
  uint8_t crcCalc = Crc8::calc(rx, rxLen - 1);

  if (crcCalc != crcRx) {
    return false;
  }

  return true;
}

void Link::poll() {
  uint8_t rxBuf[TLV_MAX_LEN];
  uint8_t rxLen = 0;

  if (!_tr.popRx(rxBuf, rxLen)) return;

  TlvFrame tlv;
  if (!parseAndValidate(rxBuf, rxLen, tlv)) {
    return;
  }

  uint8_t txTlv[TLV_MAX_LEN];
  uint8_t txTlvLen = 0;

  if (!_svc.handleOneTlv(tlv, txTlv, txTlvLen)) {
    uint8_t payload[3] = { tlv.t, 0, GSA_ERROR_COMMAND_NOT_SUPPORTED };
    txTlvLen = TlvBuilder::build(CMD_FUNCTIONAL_ERROR, payload, sizeof(payload), txTlv, TLV_MAX_LEN);
  }

  if (txTlvLen >= 2) {
    if ((uint8_t)(txTlvLen + 1) > TLV_MAX_LEN) {
      return;
    }

    uint8_t out[TLV_MAX_LEN];
    for (uint8_t i = 0; i < txTlvLen; i++) out[i] = txTlv[i];
    out[txTlvLen] = Crc8::calc(out, txTlvLen);

    _tr.setTx(out, txTlvLen + 1, false);
  }
}
