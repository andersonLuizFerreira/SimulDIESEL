#pragma once

#include <stdint.h>

#include "services/can/driver/CanDriverSelector.h"
#include "services/can/table/CanRxTableManager.h"

enum CanRxMode {
  CAN_RX_MODE_AUTO = 0,
  CAN_RX_MODE_DIRECT_ONLY = 1
};

class CanRxHub {
public:
  struct ProcessResult {
    bool direct;
    bool tableFullFallback;
    CanRxTableManager::ProcessResult tableResult;
    CanRxTableManager::CrudEvent crudEvent;
  };

  void reset();
  void setMode(CanRxMode mode);
  CanRxMode mode() const;

  ProcessResult process(const CanDriverSelected::Frame& frame,
                        CanRxTableManager& table,
                        uint32_t nowMs);

  bool tableFullActive() const;
  uint32_t fallbackDirectCount() const;
  uint32_t tableFullCount() const;
  uint32_t lastFallbackCanId() const;

private:
  CanRxMode _mode = CAN_RX_MODE_AUTO;
  bool _tableFullActive = false;
  uint32_t _fallbackDirectCount = 0;
  uint32_t _tableFullCount = 0;
  uint32_t _lastFallbackCanId = 0;

  static CanRxTableManager::ObservedFrame toObservedFrame(const CanDriverSelected::Frame& frame);
};
