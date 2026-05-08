#pragma once

#include <stdint.h>

#include "defs.h"
#include "services/can/driver/CanDriverSelector.h"

class CanTxTableManager {
public:
  static const uint8_t Capacity = MAX_CAN_TX_ROWS;

  static const uint8_t MaskFlags = 0x01;
  static const uint8_t MaskCanId = 0x02;
  static const uint8_t MaskDlc = 0x04;
  static const uint8_t MaskData = 0x08;
  static const uint8_t MaskPeriodMs = 0x10;
  static const uint8_t MaskEnabled = 0x20;

  enum Status : uint8_t {
    StatusOk = 0x00,
    StatusInvalidPayload = 0x01,
    StatusControllerDisabled = 0x02,
    StatusDriverFailed = 0x03,
    StatusInvalidIndex = 0x04,
    StatusLineMissing = 0x05
  };

  struct Row {
    bool valid;
    bool enabled;
    uint8_t index;
    uint8_t flags;
    uint32_t canId;
    uint8_t dlc;
    uint8_t data[8];
    uint16_t periodMs;
    uint32_t lastSentMs;
    uint32_t txCount;
    uint8_t lastError;
  };

  struct EditFields {
    uint8_t flags;
    uint32_t canId;
    uint8_t dlc;
    uint8_t dataMask;
    uint8_t data[8];
    uint16_t periodMs;
    uint8_t enabled;
  };

  CanTxTableManager();

  void reset();
  Status create(const Row& row, uint32_t nowMs);
  Status edit(uint8_t index, uint8_t mask, const EditFields& fields, uint32_t nowMs);
  Status remove(uint8_t index);
  void tick(uint32_t nowMs, CanDriverSelected& driver, uint8_t controller, bool controllerOpen);
  bool snapshot(uint8_t index, Row& row) const;

private:
  Row _rows[Capacity];

  bool validIndex(uint8_t index) const;
  bool validateFrameFields(uint8_t flags, uint8_t dlc) const;
  void buildFrame(const Row& row, CanDriverSelected::Frame& frame) const;
};
