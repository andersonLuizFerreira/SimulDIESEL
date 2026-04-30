#pragma once

#include <stdint.h>

class CanCrudProtocol {
public:
  struct Record {
    uint8_t index;
    uint8_t flags;
    uint32_t canId;
    uint8_t dlc;
    uint8_t data[8];
    uint16_t cycleTime;
    uint32_t messageOrder;
  };

  static const uint8_t EditMaskFlags = 0x01;
  static const uint8_t EditMaskCanId = 0x02;
  static const uint8_t EditMaskDlc = 0x04;
  static const uint8_t EditMaskData = 0x08;
  static const uint8_t EditMaskCycleTime = 0x10;
  static const uint8_t CreatePayloadLen = 21;
  static const uint8_t RowPayloadLen = 21;
  static const uint8_t EditPayloadMaxLen = 21;
  static const uint8_t ReadAllDonePayloadLen = 5;

  CanCrudProtocol();

  bool encodeCreate(const Record& record, uint8_t* out, uint8_t& outLen) const;
  bool encodeRow(const Record& record, uint8_t* out, uint8_t& outLen) const;
  bool encodeEdit(const Record& record, uint8_t mask, uint8_t* out, uint8_t& outLen) const;
  bool encodeReadAllDone(uint8_t count, uint32_t messageOrder, uint8_t* out, uint8_t& outLen) const;
  bool decodeReadAllRequest(const uint8_t* value, uint8_t valueLen) const;

private:
  static void writeUint16Le(uint16_t value, uint8_t* out);
  static void writeUint32Le(uint32_t value, uint8_t* out);
};
