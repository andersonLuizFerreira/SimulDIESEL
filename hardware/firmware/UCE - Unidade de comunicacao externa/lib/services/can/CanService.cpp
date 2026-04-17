#include "services/can/CanService.h"

CanService::CanService(Sam3xCanDriver& driver, CanTransceiver& transceiver)
  : _driver(driver), _transceiver(transceiver), _config(), _ioBuffer()
{
}

void CanService::begin() {
  _driver.attachIoBuffer(&_ioBuffer);
  _ioBuffer.clear();
}

bool CanService::configure(const CanConfig& config) {
  _config = config;
  return _driver.configure(_config);
}

bool CanService::open() {
  if (!_driver.configure(_config) && !_driver.status().configured) return false;
  if (!_transceiver.enable()) return false;
  if (!_transceiver.wake()) return false;

  const bool opened = _driver.enable();
  if (!opened) {
    _transceiver.standby();
    _transceiver.disable();
  }
  return opened;
}

void CanService::close() {
  _driver.disable();
  _transceiver.standby();
  _transceiver.disable();
}

bool CanService::reset() {
  if (!_transceiver.enable()) return false;
  if (!_transceiver.wake()) return false;
  return _driver.reset();
}

bool CanService::applyFilters(const CanFilter* filters, uint8_t filterCount) {
  return _driver.applyFilters(filters, filterCount);
}

const CanConfig& CanService::config() const {
  return _config;
}

CanStatus CanService::status() const {
  CanStatus status = _driver.status();
  status.transceiverFault = _transceiver.hasFault();
  return status;
}

bool CanService::isOpen() const {
  return status().state == UceCan::InterfaceState::Open;
}

CanIoBuffer& CanService::ioBuffer() {
  return _ioBuffer;
}

const CanIoBuffer& CanService::ioBuffer() const {
  return _ioBuffer;
}
