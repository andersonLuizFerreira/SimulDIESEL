#include "services/can/table/CanTxTableManager.h"

#include <string.h>

CanTxTableManager::CanTxTableManager() {
  reset();
}

void CanTxTableManager::reset() {
  memset(_rows, 0, sizeof(_rows));
  for (uint8_t index = 0; index < Capacity; ++index) {
    _rows[index].index = index;
  }
}

CanTxTableManager::Status CanTxTableManager::create(const Row& row, uint32_t nowMs) {
  if (!validIndex(row.index) || !validateFrameFields(row.flags, row.dlc)) {
    return StatusInvalidPayload;
  }

  Row& target = _rows[row.index];
  target.valid = true;
  target.enabled = row.enabled && row.periodMs > 0;
  target.index = row.index;
  target.flags = row.flags & 0x03;
  target.canId = row.canId;
  target.dlc = row.dlc;
  for (uint8_t dataIndex = 0; dataIndex < 8; ++dataIndex) {
    target.data[dataIndex] = row.data[dataIndex];
  }
  target.periodMs = row.periodMs;
  target.lastSentMs = nowMs;
  target.txCount = 0;
  target.lastError = StatusOk;
  return StatusOk;
}

CanTxTableManager::Status CanTxTableManager::edit(uint8_t index,
                                                  uint8_t mask,
                                                  const EditFields& fields,
                                                  uint32_t nowMs) {
  if (!validIndex(index) || (mask & 0xC0) != 0) {
    return StatusInvalidPayload;
  }

  Row& row = _rows[index];
  if (!row.valid) {
    return StatusLineMissing;
  }

  const uint8_t newFlags = (mask & MaskFlags) != 0 ? (fields.flags & 0x03) : row.flags;
  const uint8_t newDlc = (mask & MaskDlc) != 0 ? fields.dlc : row.dlc;
  if (!validateFrameFields(newFlags, newDlc)) {
    return StatusInvalidPayload;
  }

  if ((mask & MaskFlags) != 0) {
    row.flags = newFlags;
  }
  if ((mask & MaskCanId) != 0) {
    row.canId = fields.canId;
  }
  if ((mask & MaskDlc) != 0) {
    row.dlc = newDlc;
  }
  if ((mask & MaskData) != 0) {
    if (fields.dataMask == 0) {
      return StatusInvalidPayload;
    }

    for (uint8_t dataIndex = 0; dataIndex < 8; ++dataIndex) {
      if ((fields.dataMask & (uint8_t)(1U << dataIndex)) != 0) {
        row.data[dataIndex] = fields.data[dataIndex];
      }
    }
  }
  if ((mask & MaskPeriodMs) != 0) {
    row.periodMs = fields.periodMs;
    row.lastSentMs = nowMs;
  }
  if ((mask & MaskEnabled) != 0) {
    row.enabled = fields.enabled != 0;
    row.lastSentMs = nowMs;
  }
  if (row.periodMs == 0) {
    row.enabled = false;
  }

  row.lastError = StatusOk;
  return StatusOk;
}

CanTxTableManager::Status CanTxTableManager::remove(uint8_t index) {
  if (!validIndex(index)) {
    return StatusInvalidIndex;
  }

  memset(&_rows[index], 0, sizeof(_rows[index]));
  _rows[index].index = index;
  return StatusOk;
}

void CanTxTableManager::tick(uint32_t nowMs,
                             CanDriverSelected& driver,
                             uint8_t controller,
                             bool controllerOpen) {
  if (!controllerOpen) {
    return;
  }

  for (uint8_t index = 0; index < Capacity; ++index) {
    Row& row = _rows[index];
    if (!row.valid || !row.enabled || row.periodMs == 0) {
      continue;
    }

    if ((uint32_t)(nowMs - row.lastSentMs) < row.periodMs) {
      continue;
    }

    CanDriverSelected::Frame frame;
    buildFrame(row, frame);
    if (driver.send(controller, frame)) {
      row.lastSentMs = nowMs;
      ++row.txCount;
      row.lastError = StatusOk;
    } else {
      row.lastSentMs = nowMs;
      row.lastError = StatusDriverFailed;
    }
  }
}

bool CanTxTableManager::snapshot(uint8_t index, Row& row) const {
  if (!validIndex(index) || !_rows[index].valid) {
    return false;
  }

  row = _rows[index];
  return true;
}

bool CanTxTableManager::validIndex(uint8_t index) const {
  return index < Capacity;
}

bool CanTxTableManager::validateFrameFields(uint8_t flags, uint8_t dlc) const {
  return (flags & 0xFC) == 0 && dlc <= 8;
}

void CanTxTableManager::buildFrame(const Row& row, CanDriverSelected::Frame& frame) const {
  const bool extended = (row.flags & 0x01) != 0;
  frame.extended = extended;
  frame.rtr = (row.flags & 0x02) != 0;
  frame.dlc = row.dlc;
  frame.id = row.canId & (extended ? 0x1FFFFFFFUL : 0x7FFUL);
  for (uint8_t dataIndex = 0; dataIndex < 8; ++dataIndex) {
    frame.data[dataIndex] = row.data[dataIndex];
  }
}
