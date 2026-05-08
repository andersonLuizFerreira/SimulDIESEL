#include "services/can/protocol/CanCrudProtocol.h"

#include <string.h>

CanCrudProtocol::CanCrudProtocol() = default;

bool CanCrudProtocol::encodeCreate(const Record& record, uint8_t* out, uint8_t& outLen) const {
  outLen = 0;
  if (!out) {
    return false;
  }

  out[0] = record.index;
  out[1] = record.flags;
  writeUint32Le(record.canId, &out[2]);
  out[6] = record.dlc;
  memcpy(&out[7], record.data, 8);
  writeUint16Le(record.cycleTime, &out[15]);
  writeUint32Le(record.messageOrder, &out[17]);
  outLen = CreatePayloadLen;
  return true;
}

bool CanCrudProtocol::encodeRow(const Record& record, uint8_t* out, uint8_t& outLen) const {
  return encodeCreate(record, out, outLen);
}

bool CanCrudProtocol::encodeEdit(const Record& record, uint8_t mask, uint8_t dataMask, uint8_t* out, uint8_t& outLen) const {
  outLen = 0;
  if (!out) {
    return false;
  }

  if ((mask & EditMaskData) != 0 && dataMask == 0) {
    return false;
  }

  uint8_t offset = 0;
  out[offset++] = record.index;
  out[offset++] = mask;
  writeUint32Le(record.messageOrder, &out[offset]);
  offset += 4;

  if ((mask & EditMaskFlags) != 0) {
    out[offset++] = record.flags;
  }
  if ((mask & EditMaskCanId) != 0) {
    writeUint32Le(record.canId, &out[offset]);
    offset += 4;
  }
  if ((mask & EditMaskDlc) != 0) {
    out[offset++] = record.dlc;
  }
  if ((mask & EditMaskData) != 0) {
    out[offset++] = dataMask;
    for (uint8_t index = 0; index < 8; ++index) {
      if ((dataMask & (uint8_t)(1U << index)) != 0) {
        out[offset++] = record.data[index];
      }
    }
  }
  if ((mask & EditMaskCycleTime) != 0) {
    writeUint16Le(record.cycleTime, &out[offset]);
    offset += 2;
  }

  outLen = offset;
  return true;
}

bool CanCrudProtocol::encodeTic(uint8_t index, uint8_t* out, uint8_t& outLen) const {
  outLen = 0;
  if (!out) {
    return false;
  }

  out[0] = index;
  outLen = TicPayloadLen;
  return true;
}

bool CanCrudProtocol::encodeDelete(uint8_t index, uint8_t reason, uint32_t messageOrder, uint8_t* out, uint8_t& outLen) const {
  outLen = 0;
  if (!out) {
    return false;
  }

  out[0] = index;
  out[1] = reason;
  writeUint32Le(messageOrder, &out[2]);
  outLen = DeletePayloadLen;
  return true;
}

bool CanCrudProtocol::encodeReadAllDone(uint8_t count, uint32_t messageOrder, uint8_t* out, uint8_t& outLen) const {
  outLen = 0;
  if (!out) {
    return false;
  }

  out[0] = count;
  writeUint32Le(messageOrder, &out[1]);
  outLen = ReadAllDonePayloadLen;
  return true;
}

bool CanCrudProtocol::decodeReadAllRequest(const uint8_t* value, uint8_t valueLen) const {
  return valueLen == 0 && value == nullptr;
}

void CanCrudProtocol::writeUint16Le(uint16_t value, uint8_t* out) {
  out[0] = (uint8_t)(value & 0xFFU);
  out[1] = (uint8_t)((value >> 8) & 0xFFU);
}

void CanCrudProtocol::writeUint32Le(uint32_t value, uint8_t* out) {
  out[0] = (uint8_t)(value & 0xFFU);
  out[1] = (uint8_t)((value >> 8) & 0xFFU);
  out[2] = (uint8_t)((value >> 16) & 0xFFU);
  out[3] = (uint8_t)((value >> 24) & 0xFFU);
}
