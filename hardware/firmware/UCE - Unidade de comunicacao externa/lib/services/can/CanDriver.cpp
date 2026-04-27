#include "services/can/CanDriver.h"

#include <Arduino.h>
#include "can.h"
#include "defs.h"

namespace {
const uint8_t CAN_CONTROLLER_CAN0 = 0x00;
const uint8_t CAN_MODE_DEFAULT = 0x00;
const uint8_t CAN_MODE_NORMAL = 0x00;
const uint8_t CAN_MODE_LISTEN = 0x01;
const uint8_t CAN_INTERFACE_DISABLED = 0x00;
const uint8_t CAN_INTERFACE_CONFIGURED = 0x01;
const uint8_t CAN_INTERFACE_OPEN = 0x02;
const uint8_t CAN_INTERFACE_FAULT = 0x03;
const uint8_t CAN_RX_MAILBOX_STD = 0;
const uint8_t CAN_RX_MAILBOX_EXT = 1;
const uint8_t CAN_TX_MAILBOX = 2;
const uint8_t CAN_EVENT_DRIVER_BEGIN = 0x01;
const uint8_t CAN_EVENT_CONFIG_REQUESTED = 0x02;
const uint8_t CAN_EVENT_CONFIG_OK = 0x03;
const uint8_t CAN_EVENT_CONFIG_FAULT = 0x04;
const uint8_t CAN_EVENT_OPEN_REQUESTED = 0x05;
const uint8_t CAN_EVENT_OPEN_OK = 0x06;
const uint8_t CAN_EVENT_OPEN_FAULT = 0x07;
const uint8_t CAN_EVENT_CLOSE_REQUESTED = 0x08;
const uint8_t CAN_EVENT_CLOSE_OK = 0x09;
const uint8_t CAN_EVENT_RESET_REQUESTED = 0x0A;
const uint8_t CAN_EVENT_RESET_OK = 0x0B;
const uint8_t CAN_EVENT_STATUS_SNAPSHOT = 0x0C;
const uint8_t CAN_EVENT_RX_POLL = 0x0D;
const uint8_t CAN_EVENT_RX_FRAME_READ = 0x0E;
const uint8_t CAN_EVENT_UNSUPPORTED_CONTROLLER = 0x0F;
const uint8_t CAN_EVENT_INVALID_BITRATE = 0x10;
const uint8_t CAN_EVENT_INVALID_MODE = 0x11;
const uint8_t CAN_EVENT_CAN_PHYSICAL_ERROR = 0x12;
const uint8_t CAN_EVENT_TX_REQUESTED = 0x13;
const uint8_t CAN_EVENT_TX_OK = 0x14;
const uint8_t CAN_EVENT_TX_FAULT = 0x15;
}

void CanDriver::begin() {
  _logSequence = 0;
  for (uint8_t controller = 0; controller < ControllerCount; ++controller) {
    _logHead[controller] = 0;
    _logCount[controller] = 0;
    resetPort(controller);
    logEvent(controller, CAN_EVENT_DRIVER_BEGIN);
  }
}

bool CanDriver::configure(uint8_t controller, uint8_t bitrateCode, uint8_t modeCode) {
  uint32_t physicalBitrate = 0;
  if (!isValidController(controller)) {
    return false;
  }

  logEvent(controller, CAN_EVENT_CONFIG_REQUESTED, bitrateCode, modeCode);
  if (!mapBitrate(bitrateCode, physicalBitrate)) {
    logEvent(controller, CAN_EVENT_INVALID_BITRATE, bitrateCode);
    logEvent(controller, CAN_EVENT_CONFIG_FAULT, bitrateCode, modeCode);
    return false;
  }

  if (!validateMode(modeCode)) {
    logEvent(controller, CAN_EVENT_INVALID_MODE, modeCode);
    logEvent(controller, CAN_EVENT_CONFIG_FAULT, bitrateCode, modeCode);
    return false;
  }

  PortState& port = _ports[controller];
  const bool wasOpen = port.interfaceState == CAN_INTERFACE_OPEN;
  if (!isPhysicalCan0(controller)) {
    port.interfaceState = CAN_INTERFACE_FAULT;
    logEvent(controller, CAN_EVENT_UNSUPPORTED_CONTROLLER);
    logEvent(controller, CAN_EVENT_CONFIG_FAULT, bitrateCode, modeCode);
    return false;
  }

  pmc_enable_periph_clk(ID_CAN0);
  can_disable(CAN0);
  can_disable_autobaud_listen_mode(CAN0);

  const bool initialized = can_init(CAN0, SystemCoreClock, physicalBitrate) != 0;
  if (!initialized) {
    port.bitrateCode = bitrateCode;
    port.modeCode = modeCode;
    port.interfaceState = CAN_INTERFACE_FAULT;
    logEvent(controller, CAN_EVENT_CAN_PHYSICAL_ERROR, 0x00, can_get_tx_error_cnt(CAN0), can_get_rx_error_cnt(CAN0));
    logEvent(controller, CAN_EVENT_CONFIG_FAULT, bitrateCode, modeCode);
    return false;
  }

  // Listen mode is accepted at the contract level, but physical silent/listen-only
  // behavior will be validated later before enabling CAN_MR_ABM in production.
  can_disable(CAN0);
  can_reset_all_mailbox(CAN0);
  configureRxMailboxes();
  configureTxMailbox();

  port.bitrateCode = bitrateCode;
  port.modeCode = modeCode;
  if (wasOpen) {
    can_enable(CAN0);
    port.interfaceState = CAN_INTERFACE_OPEN;
  } else {
    port.interfaceState = CAN_INTERFACE_CONFIGURED;
  }
  logEvent(controller, CAN_EVENT_CONFIG_OK, (uint8_t)(can_get_status(CAN0) & 0xFFU), can_get_tx_error_cnt(CAN0), can_get_rx_error_cnt(CAN0));
  return true;
}

bool CanDriver::open(uint8_t controller) {
  if (!isValidController(controller)) {
    return false;
  }

  PortState& port = _ports[controller];
  logEvent(controller, CAN_EVENT_OPEN_REQUESTED);
  if (!isPhysicalCan0(controller)) {
    port.interfaceState = CAN_INTERFACE_FAULT;
    logEvent(controller, CAN_EVENT_UNSUPPORTED_CONTROLLER);
    logEvent(controller, CAN_EVENT_OPEN_FAULT);
    return false;
  }

  if (port.interfaceState == CAN_INTERFACE_DISABLED) {
    if (!configure(controller, port.bitrateCode, port.modeCode)) {
      logEvent(controller, CAN_EVENT_OPEN_FAULT);
      return false;
    }
  } else if (port.interfaceState != CAN_INTERFACE_CONFIGURED && port.interfaceState != CAN_INTERFACE_OPEN) {
    logEvent(controller, CAN_EVENT_OPEN_FAULT);
    return false;
  }

  if (port.modeCode == CAN_MODE_LISTEN) {
    can_disable_autobaud_listen_mode(CAN0);
  }

  can_enable(CAN0);
  port.interfaceState = CAN_INTERFACE_OPEN;
  logEvent(controller, CAN_EVENT_OPEN_OK, (uint8_t)(can_get_status(CAN0) & 0xFFU), can_get_tx_error_cnt(CAN0), can_get_rx_error_cnt(CAN0));
  return true;
}

bool CanDriver::close(uint8_t controller) {
  if (!isValidController(controller)) {
    return false;
  }

  PortState& port = _ports[controller];
  logEvent(controller, CAN_EVENT_CLOSE_REQUESTED);
  if (!isPhysicalCan0(controller)) {
    port.interfaceState = CAN_INTERFACE_FAULT;
    logEvent(controller, CAN_EVENT_UNSUPPORTED_CONTROLLER);
    return false;
  }

  can_disable(CAN0);
  port.interfaceState = CAN_INTERFACE_DISABLED;
  logEvent(controller, CAN_EVENT_CLOSE_OK);
  return true;
}

bool CanDriver::reset(uint8_t controller) {
  if (!isValidController(controller)) {
    return false;
  }

  logEvent(controller, CAN_EVENT_RESET_REQUESTED);
  if (!isPhysicalCan0(controller)) {
    _ports[controller].interfaceState = CAN_INTERFACE_FAULT;
    logEvent(controller, CAN_EVENT_UNSUPPORTED_CONTROLLER);
    return false;
  }

  can_disable(CAN0);
  can_disable_autobaud_listen_mode(CAN0);
  can_reset_all_mailbox(CAN0);
  resetPort(controller);
  logEvent(controller, CAN_EVENT_RESET_OK);
  return true;
}

bool CanDriver::getStatus(uint8_t controller, Status& status) {
  if (!isValidController(controller)) {
    return false;
  }

  const PortState& port = _ports[controller];
  status.controller = controller;
  status.bitrateCode = port.bitrateCode;
  status.modeCode = port.modeCode;
  status.interfaceState = port.interfaceState;
  status.configured = port.interfaceState != CAN_INTERFACE_DISABLED;
  status.open = port.interfaceState == CAN_INTERFACE_OPEN;
  logEvent(controller, CAN_EVENT_STATUS_SNAPSHOT, isPhysicalCan0(controller) ? (uint8_t)(can_get_status(CAN0) & 0xFFU) : 0, isPhysicalCan0(controller) ? can_get_tx_error_cnt(CAN0) : 0, isPhysicalCan0(controller) ? can_get_rx_error_cnt(CAN0) : 0);
  return true;
}

bool CanDriver::pollReceived(uint8_t controller, Frame* frames, uint8_t maxFrames, uint8_t& frameCount) {
  frameCount = 0;
  if (!isValidController(controller)) {
    return false;
  }

  if (!isPhysicalCan0(controller)) {
    logEvent(controller, CAN_EVENT_UNSUPPORTED_CONTROLLER);
    return true;
  }

  if (!frames || maxFrames == 0 || _ports[controller].interfaceState != CAN_INTERFACE_OPEN) {
    logEvent(controller, CAN_EVENT_RX_POLL, 0);
    return true;
  }

  const uint8_t mailboxes[] = {CAN_RX_MAILBOX_STD, CAN_RX_MAILBOX_EXT};
  const bool extendedFlags[] = {false, true};
  for (uint8_t i = 0; i < sizeof(mailboxes) && frameCount < maxFrames; ++i) {
    Frame frame;
    if (readRxMailbox(mailboxes[i], extendedFlags[i], frame)) {
      frames[frameCount++] = frame;
      logEvent(controller, CAN_EVENT_RX_FRAME_READ, frame.extended ? 0x01 : 0x00, frame.rtr ? 0x01 : 0x00, frame.dlc);
    }
  }

  if (frameCount > 0) {
    logEvent(controller, CAN_EVENT_RX_POLL, frameCount);
  }

  return true;
}

bool CanDriver::pollLog(uint8_t controller, LogEntry* entries, uint8_t maxEntries, uint8_t& entryCount) {
  entryCount = 0;
  if (!isValidController(controller)) {
    return false;
  }

  if (!entries || maxEntries == 0) {
    return true;
  }

  while (_logCount[controller] > 0 && entryCount < maxEntries) {
    const uint8_t tail = (uint8_t)((_logHead[controller] + LogCapacity - _logCount[controller]) % LogCapacity);
    entries[entryCount++] = _logs[controller][tail];
    --_logCount[controller];
  }

  return true;
}

bool CanDriver::send(uint8_t controller, const Frame& frame) {
  if (!isValidController(controller)) {
    return false;
  }

  logEvent(controller, CAN_EVENT_TX_REQUESTED, frame.extended ? 0x01 : 0x00, frame.rtr ? 0x01 : 0x00, frame.dlc);
  if (!isPhysicalCan0(controller) || _ports[controller].interfaceState != CAN_INTERFACE_OPEN || frame.dlc > 8) {
    logEvent(controller, CAN_EVENT_TX_FAULT, 0x01);
    return false;
  }

  can_mb_conf_t mailbox;
  mailbox.ul_mb_idx = CAN_TX_MAILBOX;
  mailbox.uc_obj_type = CAN_MB_TX_MODE;
  mailbox.uc_id_ver = frame.extended ? 1 : 0;
  mailbox.uc_length = frame.dlc;
  mailbox.uc_tx_prio = 15;
  mailbox.ul_status = 0;
  mailbox.ul_id_msk = 0;
  mailbox.ul_id = encodeId(frame);
  mailbox.ul_fid = 0;
  mailbox.ul_datal = ((uint32_t)frame.data[0]) |
                      ((uint32_t)frame.data[1] << 8) |
                      ((uint32_t)frame.data[2] << 16) |
                      ((uint32_t)frame.data[3] << 24);
  mailbox.ul_datah = ((uint32_t)frame.data[4]) |
                      ((uint32_t)frame.data[5] << 8) |
                      ((uint32_t)frame.data[6] << 16) |
                      ((uint32_t)frame.data[7] << 24);

  can_mailbox_init(CAN0, &mailbox);
  const uint32_t writeStatus = frame.rtr
      ? can_mailbox_tx_remote_frame(CAN0, &mailbox)
      : can_mailbox_write(CAN0, &mailbox);
  if (writeStatus != CAN_MAILBOX_TRANSFER_OK) {
    logEvent(controller, CAN_EVENT_TX_FAULT, 0x02, (uint8_t)(writeStatus & 0xFFU));
    return false;
  }

  if (!frame.rtr) {
    can_global_send_transfer_cmd(CAN0, (uint8_t)(1U << CAN_TX_MAILBOX));
  }

  logEvent(controller, CAN_EVENT_TX_OK, (uint8_t)(can_get_status(CAN0) & 0xFFU), can_get_tx_error_cnt(CAN0), can_get_rx_error_cnt(CAN0));
  return true;
}

bool CanDriver::isValidController(uint8_t controller) const {
  return controller < ControllerCount;
}

bool CanDriver::isPhysicalCan0(uint8_t controller) const {
  return controller == CAN_CONTROLLER_CAN0;
}

bool CanDriver::mapBitrate(uint8_t bitrateCode, uint32_t& bitrateKbps) const {
  switch (bitrateCode) {
    case CAN_BITRATE_5_KBPS:
      bitrateKbps = CAN_BPS_5K;
      return true;
    case CAN_BITRATE_10_KBPS:
      bitrateKbps = CAN_BPS_10K;
      return true;
    case CAN_BITRATE_25_KBPS:
      bitrateKbps = CAN_BPS_25K;
      return true;
    case CAN_BITRATE_50_KBPS:
      bitrateKbps = CAN_BPS_50K;
      return true;
    case CAN_BITRATE_125_KBPS:
      bitrateKbps = CAN_BPS_125K;
      return true;
    case CAN_BITRATE_250_KBPS:
      bitrateKbps = CAN_BPS_250K;
      return true;
    case CAN_BITRATE_500_KBPS:
      bitrateKbps = CAN_BPS_500K;
      return true;
    case CAN_BITRATE_800_KBPS:
      bitrateKbps = CAN_BPS_800K;
      return true;
    case CAN_BITRATE_1000_KBPS:
      bitrateKbps = CAN_BPS_1000K;
      return true;
    default:
      bitrateKbps = 0;
      return false;
  }
}

bool CanDriver::validateMode(uint8_t modeCode) const {
  return modeCode == CAN_MODE_NORMAL || modeCode == CAN_MODE_LISTEN;
}

void CanDriver::configureRxMailboxes() {
  can_mb_conf_t mailbox;

  // Keep STD and EXT frames in separate mailboxes by filtering the MIDE bit.
  mailbox.ul_mb_idx = CAN_RX_MAILBOX_STD;
  mailbox.uc_obj_type = CAN_MB_RX_OVER_WR_MODE;
  mailbox.uc_id_ver = 0;
  mailbox.uc_length = 0;
  mailbox.uc_tx_prio = 0;
  mailbox.ul_status = 0;
  mailbox.ul_id_msk = CAN_MAM_MIDE;
  mailbox.ul_id = 0;
  mailbox.ul_fid = 0;
  mailbox.ul_datal = 0;
  mailbox.ul_datah = 0;
  can_mailbox_init(CAN0, &mailbox);

  mailbox.ul_mb_idx = CAN_RX_MAILBOX_EXT;
  mailbox.uc_id_ver = 1;
  can_mailbox_init(CAN0, &mailbox);
}

void CanDriver::configureTxMailbox() {
  can_mb_conf_t mailbox;
  mailbox.ul_mb_idx = CAN_TX_MAILBOX;
  mailbox.uc_obj_type = CAN_MB_TX_MODE;
  mailbox.uc_id_ver = 0;
  mailbox.uc_length = 0;
  mailbox.uc_tx_prio = 15;
  mailbox.ul_status = 0;
  mailbox.ul_id_msk = 0;
  mailbox.ul_id = 0;
  mailbox.ul_fid = 0;
  mailbox.ul_datal = 0;
  mailbox.ul_datah = 0;
  can_mailbox_init(CAN0, &mailbox);
}

uint32_t CanDriver::encodeId(const Frame& frame) const {
  if (frame.extended) {
    const uint32_t id = frame.id & 0x1FFFFFFFUL;
    return CAN_MID_MIDvA((id >> 18) & 0x7FFUL) | CAN_MID_MIDvB(id & 0x3FFFFUL);
  }

  return CAN_MID_MIDvA(frame.id & 0x7FFUL);
}

bool CanDriver::readRxMailbox(uint8_t mailboxIndex, bool extended, Frame& frame) {
  const uint32_t status = can_mailbox_get_status(CAN0, mailboxIndex);
  if ((status & CAN_MSR_MRDY) == 0) {
    return false;
  }

  const uint32_t rawId = CAN0->CAN_MB[mailboxIndex].CAN_MID;
  const bool rawExtended = (rawId & CAN_MID_MIDE) != 0;
  can_mb_conf_t mailbox;
  mailbox.ul_mb_idx = mailboxIndex;
  mailbox.uc_obj_type = CAN_MB_RX_OVER_WR_MODE;
  mailbox.uc_id_ver = rawExtended ? 1 : 0;
  mailbox.uc_length = 0;
  mailbox.uc_tx_prio = 0;
  mailbox.ul_status = status;
  mailbox.ul_id_msk = 0;
  mailbox.ul_id = 0;
  mailbox.ul_fid = 0;
  mailbox.ul_datal = 0;
  mailbox.ul_datah = 0;

  can_mailbox_read(CAN0, &mailbox);

  frame.extended = rawExtended;
  frame.rtr = (status & CAN_MSR_MRTR) != 0;
  frame.dlc = mailbox.uc_length > 8 ? 8 : mailbox.uc_length;

  const uint32_t idA = (rawId & CAN_MID_MIDvA_Msk) >> CAN_MID_MIDvA_Pos;
  if (rawExtended) {
    const uint32_t idB = (rawId & CAN_MID_MIDvB_Msk) >> CAN_MID_MIDvB_Pos;
    frame.id = ((idA << 18) | idB) & 0x1FFFFFFFUL;
  } else {
    frame.id = idA & 0x7FFUL;
  }

  for (uint8_t i = 0; i < 8; ++i) {
    frame.data[i] = 0;
  }

  if (!frame.rtr) {
    const uint32_t words[] = {mailbox.ul_datal, mailbox.ul_datah};
    for (uint8_t i = 0; i < frame.dlc; ++i) {
      frame.data[i] = (uint8_t)((words[i / 4] >> ((i % 4) * 8)) & 0xFFU);
    }
  }

  return true;
}

void CanDriver::logEvent(uint8_t controller,
                         uint8_t eventCode,
                         uint8_t detail0,
                         uint8_t detail1,
                         uint8_t detail2) {
  if (!isValidController(controller)) {
    return;
  }

  LogEntry& entry = _logs[controller][_logHead[controller]];
  const PortState& port = _ports[controller];
  entry.timestampLow = _logSequence++;
  entry.eventCode = eventCode;
  entry.interfaceState = port.interfaceState;
  entry.bitrateCode = port.bitrateCode;
  entry.modeCode = port.modeCode;
  entry.detail0 = detail0;
  entry.detail1 = detail1;
  entry.detail2 = detail2;

  _logHead[controller] = (uint8_t)((_logHead[controller] + 1) % LogCapacity);
  if (_logCount[controller] < LogCapacity) {
    ++_logCount[controller];
  }
}

void CanDriver::resetPort(uint8_t controller) {
  if (!isValidController(controller)) {
    return;
  }

  _ports[controller].bitrateCode = CAN_BITRATE_250_KBPS;
  _ports[controller].modeCode = CAN_MODE_DEFAULT;
  _ports[controller].interfaceState = CAN_INTERFACE_DISABLED;
}
