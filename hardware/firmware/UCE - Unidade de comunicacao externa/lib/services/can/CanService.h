#pragma once

#include "drivers/can/Sam3xCanDriver.h"
#include "hal/transceivers/CanTransceiver.h"
#include "services/can/CanConfig.h"
#include "services/can/CanFilter.h"
#include "services/can/CanIoBuffer.h"
#include "services/can/CanStatus.h"

class CanService {
public:
  CanService(Sam3xCanDriver& driver, CanTransceiver& transceiver);

  void begin();
  bool configure(const CanConfig& config);
  bool open();
  void close();
  bool reset();
  bool applyFilters(const CanFilter* filters, uint8_t filterCount);

  const CanConfig& config() const;
  CanStatus status() const;
  bool isOpen() const;

  CanIoBuffer& ioBuffer();
  const CanIoBuffer& ioBuffer() const;

private:
  Sam3xCanDriver& _driver;
  CanTransceiver& _transceiver;
  CanConfig _config;
  CanIoBuffer _ioBuffer;
};
