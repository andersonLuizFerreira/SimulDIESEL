#include "drivers/can/Sam3xCanDriver.h"

#include <can.h>

#include "defs.h"
#include "hal/board/BoardPins.h"

Sam3xCanDriver::Sam3xCanDriver() {
  _status.bitrateKbps = _config.bitrateKbps;
  _status.mode = _config.mode;
}

void Sam3xCanDriver::attachIoBuffer(CanIoBuffer* ioBuffer) {
  _ioBuffer = ioBuffer;
}

bool Sam3xCanDriver::configure(const CanConfig& config) {
  const Sam3xCanBitTiming::RegisterConfig timing = Sam3xCanBitTiming::compute(SystemCoreClock, config.bitrateKbps);
  if (!timing.valid) {
    _status.state = UceCan::InterfaceState::Fault;
    _status.configured = false;
    return false;
  }

  _config = config;
  _status.state = UceCan::InterfaceState::Configured;
  _status.mode = _config.mode;
  _status.bitrateKbps = _config.bitrateKbps;
  _status.configured = true;
  _status.interfaceEnabled = false;
  _status.synchronized = false;
  _status.busOff = false;
  _status.errorPassive = false;
  _status.errorWarning = false;
  return true;
}

bool Sam3xCanDriver::enable() {
  if (!_status.configured) {
    if (!configure(_config)) return false;
  }

  Can* const canController = Sam3xCanRegisters::controller(_config.controller);
  const Sam3xCanBitTiming::RegisterConfig timing = Sam3xCanBitTiming::compute(SystemCoreClock, _config.bitrateKbps);
  if (!timing.valid) {
    _status.state = UceCan::InterfaceState::Fault;
    return false;
  }

  pmc_enable_periph_clk(Sam3xCanRegisters::peripheralId(_config.controller));
  BoardPins::configureCanPins((uint8_t)_config.controller);

  can_disable(canController);
  can_disable_interrupt(canController, CAN_DISABLE_ALL_INTERRUPT_MASK);
  canController->CAN_BR = timing.baudrateRegister;
  can_reset_all_mailbox(canController);

  if (_config.mode == UceCan::Mode::ListenOnly) {
    can_enable_autobaud_listen_mode(canController);
  } else {
    can_disable_autobaud_listen_mode(canController);
  }

  can_enable(canController);
  refreshStatus();
  _status.state = UceCan::InterfaceState::Open;
  _status.interfaceEnabled = true;
  return true;
}

void Sam3xCanDriver::disable() {
  Can* const canController = Sam3xCanRegisters::controller(_config.controller);
  can_disable_interrupt(canController, CAN_DISABLE_ALL_INTERRUPT_MASK);
  can_disable(canController);
  refreshStatus();
  _status.interfaceEnabled = false;
  _status.state = _status.configured ? UceCan::InterfaceState::Configured : UceCan::InterfaceState::Disabled;
}

bool Sam3xCanDriver::reset() {
  disable();
  return enable();
}

bool Sam3xCanDriver::applyFilters(const CanFilter* filters, uint8_t filterCount) {
  if (!_status.configured) return false;
  if (filterCount > UCE_CAN_MAX_FILTERS) return false;

  Can* const canController = Sam3xCanRegisters::controller(_config.controller);
  can_reset_all_mailbox(canController);
  if (_ioBuffer) _ioBuffer->clear();

  for (uint8_t index = 0; index < filterCount; ++index) {
    const CanFilter& filter = filters[index];
    if (!filter.enabled || filter.mailboxIndex >= UCE_CAN_MAX_MAILBOXES) continue;

    can_mb_conf_t mailbox{};
    mailbox.ul_mb_idx = filter.mailboxIndex;
    mailbox.uc_obj_type = filter.overwrite ? CAN_MB_RX_OVER_WR_MODE : CAN_MB_RX_MODE;
    mailbox.uc_id_ver = filter.extendedId ? 1U : 0U;
    mailbox.ul_id = encodeId(filter.id, filter.extendedId);
    mailbox.ul_id_msk = encodeMask(filter.mask, filter.extendedId);
    can_mailbox_init(canController, &mailbox);
    storeMailboxShadow(filter);
  }

  refreshStatus();
  return true;
}

CanStatus Sam3xCanDriver::status() const {
  return _status;
}

const CanConfig& Sam3xCanDriver::config() const {
  return _config;
}

uint32_t Sam3xCanDriver::encodeId(uint32_t id, bool extendedId) {
  if (extendedId) {
    return CAN_MID_MIDE | CAN_MID_MIDvA((id >> 18) & 0x7FFU) | CAN_MID_MIDvB(id & 0x3FFFFU);
  }

  return CAN_MID_MIDvA(id & 0x7FFU);
}

uint32_t Sam3xCanDriver::encodeMask(uint32_t mask, bool extendedId) {
  if (extendedId) {
    return CAN_MAM_MIDvA((mask >> 18) & 0x7FFU) | CAN_MAM_MIDvB(mask & 0x3FFFFU);
  }

  return CAN_MAM_MIDvA(mask & 0x7FFU);
}

void Sam3xCanDriver::refreshStatus() {
  Can* const canController = Sam3xCanRegisters::controller(_config.controller);
  const uint32_t controllerStatus = can_get_status(canController);

  _status.controllerStatus = controllerStatus;
  _status.txErrorCount = can_get_tx_error_cnt(canController);
  _status.rxErrorCount = can_get_rx_error_cnt(canController);
  _status.busOff = (controllerStatus & CAN_SR_BOFF) != 0U;
  _status.errorPassive = (controllerStatus & CAN_SR_ERRP) != 0U;
  _status.errorWarning = (controllerStatus & CAN_SR_ERRA) != 0U;
  _status.synchronized = (controllerStatus & CAN_SR_WAKEUP) != 0U;
}

void Sam3xCanDriver::storeMailboxShadow(const CanFilter& filter) {
  if (!_ioBuffer) return;

  CanMailbox mailbox{};
  mailbox.index = filter.mailboxIndex;
  mailbox.enabled = filter.enabled;
  mailbox.direction = filter.overwrite ? UceCan::MailboxDirection::RxOverwrite : UceCan::MailboxDirection::Rx;
  mailbox.extendedId = filter.extendedId;
  mailbox.id = filter.id;
  mailbox.mask = filter.mask;
  _ioBuffer->storeMailbox(mailbox);
}
