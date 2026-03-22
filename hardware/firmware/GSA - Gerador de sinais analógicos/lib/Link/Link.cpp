#include "Link.h"
#include "TlvBuilder.h"

Link::Link(Transport& tr, Service& svc)
: _tr(tr), _svc(svc),
  _errCode(LINK_ERR_NONE),
  _errLastT(0),
  _hasErr(false)
{}

void Link::begin() {
  clearError();
}

void Link::setError(uint8_t code, uint8_t lastT) {
  _hasErr = true;
  _errCode = code;
  _errLastT = lastT;
}

void Link::clearError() {
  _hasErr = false;
  _errCode = LINK_ERR_NONE;
  _errLastT = 0;
}

bool Link::parseAndValidate(const uint8_t* rx, uint8_t rxLen, TlvFrame& out) {
  if (!rx || rxLen < 3) {
    setError(LINK_ERR_BAD_LEN, 0);
    return false;
  }

  uint8_t t = rx[0];
  uint8_t l = rx[1];

  uint8_t expected = (uint8_t)(2 + l + 1);
  if (expected != rxLen) {
    setError(LINK_ERR_BAD_LEN, t);
    return false;
  }

  out.t = t;
  out.l = l;
  out.v = (l > 0) ? &rx[2] : nullptr;

  uint8_t crcRx = rx[rxLen - 1];
  uint8_t crcCalc = Crc8::calc(rx, rxLen - 1);

  if (crcCalc != crcRx) {
    setError(LINK_ERR_BAD_CRC, t);
    return false;
  }

  return true;
}

bool Link::handleLinkCmd(const TlvFrame& tlv, uint8_t* txTlvOut, uint8_t& txTlvLenOut) {
  (void)tlv;
  (void)txTlvOut;
  txTlvLenOut = 0;
  return false;
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

  if (!handleLinkCmd(tlv, txTlv, txTlvLen)) {
    if (!_svc.handleOneTlv(tlv, txTlv, txTlvLen)) {
      setError(LINK_ERR_BAD_TLV, tlv.t);
      txTlvLen = TlvBuilder::buildEmpty(tlv.t, txTlv, TLV_MAX_LEN);
    }
  }

  if (txTlvLen >= 2) {
    if ((uint8_t)(txTlvLen + 1) > TLV_MAX_LEN) {
      setError(LINK_ERR_BAD_LEN, tlv.t);
      return;
    }

    uint8_t out[TLV_MAX_LEN];
    for (uint8_t i = 0; i < txTlvLen; i++) out[i] = txTlv[i];
    out[txTlvLen] = Crc8::calc(out, txTlvLen);

    _tr.setTx(out, txTlvLen + 1);
  }
}
