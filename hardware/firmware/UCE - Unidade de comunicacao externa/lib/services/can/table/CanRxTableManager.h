#pragma once

#include <stdint.h>

#include "defs.h"

class CanRxTableManager {
public:
  struct ObservedFrame {
    bool extended;
    bool rtr;
    uint32_t canId;
    uint8_t dlc;
    uint8_t data[8];
  };

  struct Entry {
    uint8_t index;
    bool valid;
    uint8_t flags;
    uint32_t canId;
    uint8_t dlc;
    uint8_t data[8];
    uint16_t cycleTime;
    uint32_t messageOrder;
    uint32_t lastSeenMs;
    uint32_t previousSeenMs;
  };

  struct CrudEvent {
    bool valid;
    uint8_t type;
    uint8_t mask;
    uint8_t dataMask;
    Entry entry;
  };

  enum ProcessResult {
    ProcessIgnored = 0,
    ProcessNoChange = 1,
    ProcessCreate = 2,
    ProcessEdit = 3,
    ProcessTableFull = 4,
    ProcessDelete = 5,
    ProcessTic = 6
  };

  CanRxTableManager();

  void reset();
  ProcessResult processFrame(const ObservedFrame& frame, uint32_t nowMs, CrudEvent& event);
  ProcessResult checkTimeouts(uint32_t nowMs, CrudEvent& event);
  uint8_t snapshotValidEntries(Entry* entries, uint8_t maxEntries) const;
  uint32_t currentMessageOrder() const;

  static const uint8_t Capacity = MAX_CAN_RX_ROWS;
  static const uint8_t DeleteReasonTimeout = 0x01;

private:
  Entry _entries[Capacity];
  uint32_t _nextMessageOrder;

  static uint8_t buildFlags(const ObservedFrame& frame);
  Entry* findByIdentity(uint32_t canId, uint8_t flags);
  Entry* findFirstFree();
  void fillEntry(Entry& entry, const ObservedFrame& frame, uint32_t nowMs);
  static uint32_t timeoutFor(const Entry& entry);
};
