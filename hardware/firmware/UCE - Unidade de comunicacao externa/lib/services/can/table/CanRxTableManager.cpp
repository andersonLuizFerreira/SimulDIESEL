#include "services/can/table/CanRxTableManager.h"

#include <string.h>

#include "defs.h"

CanRxTableManager::CanRxTableManager() {
  reset();
}

void CanRxTableManager::reset() {
  _nextMessageOrder = 1;
  for (uint8_t index = 0; index < Capacity; ++index) {
    _entries[index].index = index;
    _entries[index].valid = false;
    _entries[index].flags = 0;
    _entries[index].canId = 0;
    _entries[index].dlc = 0;
    memset(_entries[index].data, 0, sizeof(_entries[index].data));
    _entries[index].cycleTime = 0;
    _entries[index].messageOrder = 0;
    _entries[index].lastSeenMs = 0;
    _entries[index].previousSeenMs = 0;
  }
}

uint8_t CanRxTableManager::snapshotValidEntries(Entry* entries, uint8_t maxEntries) const {
  if (!entries || maxEntries == 0) {
    return 0;
  }

  uint8_t count = 0;
  for (uint8_t index = 0; index < Capacity && count < maxEntries; ++index) {
    if (_entries[index].valid) {
      entries[count++] = _entries[index];
    }
  }

  return count;
}

uint32_t CanRxTableManager::currentMessageOrder() const {
  return _nextMessageOrder > 0 ? (_nextMessageOrder - 1) : 0;
}

CanRxTableManager::ProcessResult CanRxTableManager::processFrame(const ObservedFrame& frame,
                                                                 uint32_t nowMs,
                                                                 CrudEvent& event) {
  event.valid = false;
  event.type = 0;
  event.mask = 0;

  if (frame.dlc > 8) {
    return ProcessIgnored;
  }

  const uint8_t flags = buildFlags(frame);
  Entry* entry = findByIdentity(frame.canId, flags);
  if (!entry) {
    entry = findFirstFree();
    if (!entry) {
      return ProcessTableFull;
    }

    fillEntry(*entry, frame, nowMs);
    entry->valid = true;
    entry->messageOrder = _nextMessageOrder++;
    event.valid = true;
    event.type = CMD_CAN_CREATE;
    event.mask = 0;
    event.entry = *entry;
    return ProcessCreate;
  }

  entry->previousSeenMs = entry->lastSeenMs;
  entry->lastSeenMs = nowMs;

  uint8_t mask = 0;
  if (entry->dlc != frame.dlc) {
    entry->dlc = frame.dlc;
    mask |= 0x04;
  }

  bool dataChanged = false;
  for (uint8_t index = 0; index < 8; ++index) {
    const uint8_t nextValue = (index < frame.dlc) ? frame.data[index] : 0;
    if (entry->data[index] != nextValue) {
      entry->data[index] = nextValue;
      dataChanged = true;
    }
  }
  if (dataChanged) {
    mask |= 0x08;
  }

  if (mask == 0) {
    return ProcessNoChange;
  }

  entry->messageOrder = _nextMessageOrder++;
  event.valid = true;
  event.type = CMD_CAN_EDIT;
  event.mask = mask;
  event.entry = *entry;
  return ProcessEdit;
}

uint8_t CanRxTableManager::buildFlags(const ObservedFrame& frame) {
  return (frame.extended ? 0x01 : 0x00) | (frame.rtr ? 0x02 : 0x00);
}

CanRxTableManager::Entry* CanRxTableManager::findByIdentity(uint32_t canId, uint8_t flags) {
  for (uint8_t index = 0; index < Capacity; ++index) {
    if (_entries[index].valid && _entries[index].canId == canId && _entries[index].flags == flags) {
      return &_entries[index];
    }
  }

  return nullptr;
}

CanRxTableManager::Entry* CanRxTableManager::findFirstFree() {
  for (uint8_t index = 0; index < Capacity; ++index) {
    if (!_entries[index].valid) {
      return &_entries[index];
    }
  }

  return nullptr;
}

void CanRxTableManager::fillEntry(Entry& entry, const ObservedFrame& frame, uint32_t nowMs) {
  entry.flags = buildFlags(frame);
  entry.canId = frame.canId;
  entry.dlc = frame.dlc;
  for (uint8_t index = 0; index < 8; ++index) {
    entry.data[index] = (index < frame.dlc) ? frame.data[index] : 0;
  }
  entry.cycleTime = 0;
  entry.previousSeenMs = entry.lastSeenMs;
  entry.lastSeenMs = nowMs;
}
