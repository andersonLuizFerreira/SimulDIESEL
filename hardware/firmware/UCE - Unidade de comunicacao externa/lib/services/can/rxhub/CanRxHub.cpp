#include "services/can/rxhub/CanRxHub.h"

#include <Arduino.h>

void CanRxHub::reset() {
  _mode = CAN_RX_MODE_AUTO;
  _tableFullActive = false;
  _fallbackDirectCount = 0;
  _tableFullCount = 0;
  _lastFallbackCanId = 0;
}

void CanRxHub::setMode(CanRxMode mode) {
  if (mode != CAN_RX_MODE_AUTO && mode != CAN_RX_MODE_DIRECT_ONLY) {
    return;
  }

  _mode = mode;
}

CanRxMode CanRxHub::mode() const {
  return _mode;
}

CanRxHub::ProcessResult CanRxHub::process(const CanDriverSelected::Frame& frame,
                                          CanRxTableManager& table,
                                          uint32_t nowMs) {
  ProcessResult result;
  result.direct = false;
  result.tableFullFallback = false;
  result.tableResult = CanRxTableManager::ProcessIgnored;
  result.crudEvent.valid = false;
  result.crudEvent.type = 0;
  result.crudEvent.mask = 0;
  result.crudEvent.dataMask = 0;

  if (_mode == CAN_RX_MODE_DIRECT_ONLY) {
    result.direct = true;
    return result;
  }

  const CanRxTableManager::ObservedFrame observedFrame = toObservedFrame(frame);
  result.tableResult = table.processFrame(observedFrame, nowMs, result.crudEvent);
  if (result.tableResult == CanRxTableManager::ProcessTableFull) {
    result.direct = true;
    result.tableFullFallback = true;
    _tableFullActive = true;
    ++_fallbackDirectCount;
    ++_tableFullCount;
    _lastFallbackCanId = frame.id;
  } else if (result.tableResult == CanRxTableManager::ProcessCreate) {
    _tableFullActive = false;
  }

  return result;
}

bool CanRxHub::tableFullActive() const {
  return _tableFullActive;
}

uint32_t CanRxHub::fallbackDirectCount() const {
  return _fallbackDirectCount;
}

uint32_t CanRxHub::tableFullCount() const {
  return _tableFullCount;
}

uint32_t CanRxHub::lastFallbackCanId() const {
  return _lastFallbackCanId;
}

CanRxTableManager::ObservedFrame CanRxHub::toObservedFrame(const CanDriverSelected::Frame& frame) {
  CanRxTableManager::ObservedFrame observedFrame;
  observedFrame.extended = frame.extended;
  observedFrame.rtr = frame.rtr;
  observedFrame.canId = frame.id;
  observedFrame.dlc = frame.dlc;
  for (uint8_t dataIndex = 0; dataIndex < 8; ++dataIndex) {
    observedFrame.data[dataIndex] = frame.data[dataIndex];
  }
  return observedFrame;
}
