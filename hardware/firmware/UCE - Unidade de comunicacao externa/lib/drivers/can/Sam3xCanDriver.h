#pragma once

#include "drivers/can/Sam3xCanBitTiming.h"
#include "drivers/can/Sam3xCanRegisters.h"
#include "services/can/CanConfig.h"
#include "services/can/CanFilter.h"
#include "services/can/CanIoBuffer.h"
#include "services/can/CanStatus.h"

class Sam3xCanDriver {
public:
  Sam3xCanDriver();

  void attachIoBuffer(CanIoBuffer* ioBuffer);
  bool configure(const CanConfig& config);
  bool enable();
  void disable();
  bool reset();
  bool applyFilters(const CanFilter* filters, uint8_t filterCount);

  CanStatus status() const;
  const CanConfig& config() const;

private:
  static uint32_t encodeId(uint32_t id, bool extendedId);
  static uint32_t encodeMask(uint32_t mask, bool extendedId);
  void refreshStatus();
  void storeMailboxShadow(const CanFilter& filter);

  CanConfig _config;
  CanStatus _status;
  CanIoBuffer* _ioBuffer = nullptr;
};
