#include "services/can/driver/CanDriver_fake.h"

#include <Arduino.h>

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
const uint8_t CAN_EVENT_FAKE_IDS_GENERATED = 0x16;
const uint8_t CAN_EVENT_FAKE_BURST_GENERATED = 0x17;
const uint8_t CAN_EVENT_FAKE_RX_OVERFLOW = 0x18;
const uint8_t CAN_EVENT_FAKE_CREATED = 0x19;
const uint8_t CAN_EVENT_FAKE_SINGLE_SHOT_SENT = 0x1A;
const uint32_t CAN_FAKE_BURST_PERIOD_MS = 500UL;
const uint32_t CAN_FAKE_DATA_STEP_MS = 3000UL;
const uint32_t CAN_FAKE_SINGLE_SHOT_PERIOD_MS = 10000UL;
const uint32_t CAN_FAKE_FAST_PERIOD_MS = 100UL;
const uint32_t CAN_FAKE_TEST_TIC_PERIOD_MS = 500UL;
const uint32_t CAN_FAKE_TEST_EDIT_PERIOD_MS = 500UL;
const uint32_t CAN_FAKE_TEST_EDIT_DATA_STEP_MS = 1000UL;
const uint32_t CAN_FAKE_TEST_TIMEOUT_PERIOD_MS = 5000UL;
const uint32_t CAN_FAKE_TEST_TIC_ID = 0x18FF1005UL;
const uint32_t CAN_FAKE_TEST_EDIT_ID = 0x18FF1006UL;
const uint32_t CAN_FAKE_TEST_TIMEOUT_ID = 0x18FF1007UL;
const uint8_t CAN_FAKE_BURST_SIZE = 3;
const uint8_t CAN_FAKE_FRAME_DLC = 8;
}

void CanDriverFake::begin() {
  seedEntropyOnce();
  _logSequence = 0;
  _rxHead = 0;
  _rxCount = 0;

  for (uint8_t controller = 0; controller < ControllerCount; ++controller) {
    _logHead[controller] = 0;
    _logCount[controller] = 0;
    _txHead[controller] = 0;
    _txCount[controller] = 0;
    resetPort(controller);
    logEvent(controller, CAN_EVENT_DRIVER_BEGIN);
    logEvent(controller, CAN_EVENT_FAKE_CREATED);
  }
}

bool CanDriverFake::configure(uint8_t controller, uint8_t bitrateCode, uint8_t modeCode) {
  if (!isValidController(controller)) {
    return false;
  }

  logEvent(controller, CAN_EVENT_CONFIG_REQUESTED, bitrateCode, modeCode);
  if (!validateBitrate(bitrateCode)) {
    _ports[controller].lastError = CAN_EVENT_INVALID_BITRATE;
    logEvent(controller, CAN_EVENT_INVALID_BITRATE, bitrateCode);
    logEvent(controller, CAN_EVENT_CONFIG_FAULT, bitrateCode, modeCode);
    return false;
  }

  if (!validateMode(modeCode)) {
    _ports[controller].lastError = CAN_EVENT_INVALID_MODE;
    logEvent(controller, CAN_EVENT_INVALID_MODE, modeCode);
    logEvent(controller, CAN_EVENT_CONFIG_FAULT, bitrateCode, modeCode);
    return false;
  }

  PortState& port = _ports[controller];
  if (!isSupportedController(controller)) {
    port.interfaceState = CAN_INTERFACE_FAULT;
    port.lastError = CAN_EVENT_UNSUPPORTED_CONTROLLER;
    logEvent(controller, CAN_EVENT_UNSUPPORTED_CONTROLLER);
    logEvent(controller, CAN_EVENT_CONFIG_FAULT, bitrateCode, modeCode);
    return false;
  }

  const bool wasOpen = port.interfaceState == CAN_INTERFACE_OPEN;
  port.bitrateCode = bitrateCode;
  port.modeCode = modeCode;
  port.configured = true;
  port.enabled = wasOpen;
  port.lastError = 0;
  port.interfaceState = wasOpen ? CAN_INTERFACE_OPEN : CAN_INTERFACE_CONFIGURED;
  logEvent(controller, CAN_EVENT_CONFIG_OK, bitrateCode, modeCode);
  return true;
}

bool CanDriverFake::open(uint8_t controller) {
  if (!isValidController(controller)) {
    return false;
  }

  PortState& port = _ports[controller];
  logEvent(controller, CAN_EVENT_OPEN_REQUESTED);
  if (!isSupportedController(controller)) {
    port.interfaceState = CAN_INTERFACE_FAULT;
    port.lastError = CAN_EVENT_UNSUPPORTED_CONTROLLER;
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
    port.lastError = CAN_EVENT_OPEN_FAULT;
    logEvent(controller, CAN_EVENT_OPEN_FAULT);
    return false;
  }

  port.interfaceState = CAN_INTERFACE_OPEN;
  port.configured = true;
  port.enabled = true;
  port.lastError = 0;
  purgeControllerFrames(controller);
  _txHead[controller] = 0;
  _txCount[controller] = 0;
  port.openTimestampMs = millis();
  port.lastBurstTimestampMs = port.openTimestampMs >= CAN_FAKE_BURST_PERIOD_MS
      ? port.openTimestampMs - CAN_FAKE_BURST_PERIOD_MS
      : 0;
  port.lastSingleShotMs = port.openTimestampMs - CAN_FAKE_SINGLE_SHOT_PERIOD_MS;
  port.lastFastFrameMs = port.openTimestampMs;
  port.lastFastOverflowLogMs = 0;
  port.lastTestTicFrameMs = port.openTimestampMs - CAN_FAKE_TEST_TIC_PERIOD_MS;
  port.lastTestEditFrameMs = port.openTimestampMs - CAN_FAKE_TEST_EDIT_PERIOD_MS;
  port.lastTestEditDataChangeMs = port.openTimestampMs;
  port.lastTestTimeoutFrameMs = port.openTimestampMs - CAN_FAKE_TEST_TIMEOUT_PERIOD_MS;
  port.dataNibble = 0;
  resetFastData(port);
  resetTestEditData(port);
  generateFakeIds(port);
  Serial.print("CAN_FAKE TEST IDS: TIC=0x");
  Serial.print(CAN_FAKE_TEST_TIC_ID, HEX);
  Serial.print(" EDIT_MASK=0x");
  Serial.print(CAN_FAKE_TEST_EDIT_ID, HEX);
  Serial.print(" TIMEOUT=0x");
  Serial.println(CAN_FAKE_TEST_TIMEOUT_ID, HEX);
  logEvent(controller, CAN_EVENT_FAKE_IDS_GENERATED,
           (uint8_t)(port.fakeIds[0] & 0xFFU),
           (uint8_t)(port.fakeIds[1] & 0xFFU),
           (uint8_t)(port.fakeIds[2] & 0xFFU));
  logEvent(controller, CAN_EVENT_OPEN_OK);
  return true;
}

bool CanDriverFake::close(uint8_t controller) {
  if (!isValidController(controller)) {
    return false;
  }

  PortState& port = _ports[controller];
  logEvent(controller, CAN_EVENT_CLOSE_REQUESTED);
  if (!isSupportedController(controller)) {
    port.interfaceState = CAN_INTERFACE_FAULT;
    port.lastError = CAN_EVENT_UNSUPPORTED_CONTROLLER;
    logEvent(controller, CAN_EVENT_UNSUPPORTED_CONTROLLER);
    return false;
  }

  port.interfaceState = CAN_INTERFACE_DISABLED;
  port.configured = false;
  port.enabled = false;
  port.lastError = 0;
  purgeControllerFrames(controller);
  _txHead[controller] = 0;
  _txCount[controller] = 0;
  logEvent(controller, CAN_EVENT_CLOSE_OK);
  return true;
}

bool CanDriverFake::reset(uint8_t controller) {
  if (!isValidController(controller)) {
    return false;
  }

  logEvent(controller, CAN_EVENT_RESET_REQUESTED);
  if (!isSupportedController(controller)) {
    _ports[controller].interfaceState = CAN_INTERFACE_FAULT;
    _ports[controller].lastError = CAN_EVENT_UNSUPPORTED_CONTROLLER;
    logEvent(controller, CAN_EVENT_UNSUPPORTED_CONTROLLER);
    return false;
  }

  purgeControllerFrames(controller);
  _txHead[controller] = 0;
  _txCount[controller] = 0;
  resetPort(controller);
  logEvent(controller, CAN_EVENT_RESET_OK);
  return true;
}

bool CanDriverFake::getStatus(uint8_t controller, Status& status) {
  if (!isValidController(controller)) {
    return false;
  }

  const PortState& port = _ports[controller];
  status.controller = controller;
  status.bitrateCode = port.bitrateCode;
  status.modeCode = port.modeCode;
  status.interfaceState = port.interfaceState;
  status.configured = port.configured;
  status.open = port.enabled && port.interfaceState == CAN_INTERFACE_OPEN;
  logEvent(controller, CAN_EVENT_STATUS_SNAPSHOT, port.lastError);
  return true;
}

bool CanDriverFake::pollReceived(uint8_t controller, Frame* frames, uint8_t maxFrames, uint8_t& frameCount) {
  frameCount = 0;
  if (!isValidController(controller)) {
    return false;
  }

  PortState& port = _ports[controller];
  if (!isSupportedController(controller)) {
    logEvent(controller, CAN_EVENT_UNSUPPORTED_CONTROLLER);
    return true;
  }

  if (port.interfaceState == CAN_INTERFACE_OPEN) {
    updatePortSimulation(controller, port, millis());
  }

  if (!frames || maxFrames == 0 || port.interfaceState != CAN_INTERFACE_OPEN) {
    logEvent(controller, CAN_EVENT_RX_POLL, 0);
    return true;
  }

  while (frameCount < maxFrames && dequeueRxFrame(controller, frames[frameCount])) {
    logEvent(controller, CAN_EVENT_RX_FRAME_READ,
             frames[frameCount].extended ? 0x01 : 0x00,
             frames[frameCount].rtr ? 0x01 : 0x00,
             frames[frameCount].dlc);
    ++frameCount;
  }

  if (frameCount > 0) {
    logEvent(controller, CAN_EVENT_RX_POLL, frameCount);
  }

  return true;
}

bool CanDriverFake::pollLog(uint8_t controller, LogEntry* entries, uint8_t maxEntries, uint8_t& entryCount) {
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

bool CanDriverFake::send(uint8_t controller, const Frame& frame) {
  if (!isValidController(controller)) {
    return false;
  }

  logEvent(controller, CAN_EVENT_TX_REQUESTED, frame.extended ? 0x01 : 0x00, frame.rtr ? 0x01 : 0x00, frame.dlc);
  if (!isSupportedController(controller) || _ports[controller].interfaceState != CAN_INTERFACE_OPEN || frame.dlc > 8) {
    _ports[controller].lastError = CAN_EVENT_TX_FAULT;
    logEvent(controller, CAN_EVENT_TX_FAULT, 0x01);
    return false;
  }

  rememberTxFrame(controller, frame);
  _ports[controller].lastError = 0;
  logEvent(controller, CAN_EVENT_TX_OK);
  return true;
}

bool CanDriverFake::isValidController(uint8_t controller) const {
  return controller < ControllerCount;
}

bool CanDriverFake::isSupportedController(uint8_t controller) const {
  return controller == CAN_CONTROLLER_CAN0;
}

bool CanDriverFake::validateBitrate(uint8_t bitrateCode) const {
  return bitrateCode <= CAN_BITRATE_1000_KBPS;
}

bool CanDriverFake::validateMode(uint8_t modeCode) const {
  return modeCode == CAN_MODE_NORMAL || modeCode == CAN_MODE_LISTEN;
}

void CanDriverFake::resetPort(uint8_t controller) {
  if (!isValidController(controller)) {
    return;
  }

  _ports[controller].bitrateCode = CAN_BITRATE_250_KBPS;
  _ports[controller].modeCode = CAN_MODE_DEFAULT;
  _ports[controller].interfaceState = CAN_INTERFACE_DISABLED;
  _ports[controller].configured = false;
  _ports[controller].enabled = false;
  _ports[controller].lastError = 0;
  _ports[controller].openTimestampMs = 0;
  _ports[controller].lastBurstTimestampMs = 0;
  _ports[controller].lastSingleShotMs = 0;
  _ports[controller].lastFastFrameMs = 0;
  _ports[controller].lastFastOverflowLogMs = 0;
  _ports[controller].lastTestTicFrameMs = 0;
  _ports[controller].lastTestEditFrameMs = 0;
  _ports[controller].lastTestEditDataChangeMs = 0;
  _ports[controller].lastTestTimeoutFrameMs = 0;
  _ports[controller].dataNibble = 0;
  _ports[controller].testEditByteIndex = 0;
  for (uint8_t i = 0; i < CAN_FAKE_BURST_SIZE; ++i) {
    _ports[controller].fakeIds[i] = 0;
  }
  _ports[controller].fakeId4 = 0;
  _ports[controller].fakeFastId = 0;
  resetFastData(_ports[controller]);
  resetTestEditData(_ports[controller]);
}

void CanDriverFake::logEvent(uint8_t controller,
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

void CanDriverFake::generateFakeIds(PortState& port) {
  const uint8_t suffixes[CAN_FAKE_BURST_SIZE] = {0x0, 0x1, 0x2};

  for (uint8_t i = 0; i < CAN_FAKE_BURST_SIZE; ++i) {
    uint32_t candidate = 0;
    bool duplicate = false;
    do {
      candidate = (((uint32_t)random(0, 0x7FFF) << 14) ^ (uint32_t)random(0, 0x3FFF)) & 0x1FFFFFFFUL;
      candidate = (candidate & 0x1FFFFFF0UL) | suffixes[i];
      duplicate = false;
      for (uint8_t other = 0; other < i; ++other) {
        if (port.fakeIds[other] == candidate) {
          duplicate = true;
          break;
        }
      }
    } while (duplicate);

    port.fakeIds[i] = candidate;
  }

  bool duplicate = false;
  do {
    port.fakeId4 = (((uint32_t)random(0, 0x7FFF) << 14) ^ (uint32_t)random(0, 0x3FFF)) & 0x1FFFFFFFUL;
    port.fakeId4 = (port.fakeId4 & 0x1FFFFFF0UL) | 0x03U;
    duplicate = false;
    for (uint8_t other = 0; other < CAN_FAKE_BURST_SIZE; ++other) {
      if (port.fakeIds[other] == port.fakeId4) {
        duplicate = true;
        break;
      }
    }
  } while (duplicate);

  do {
    port.fakeFastId = (((uint32_t)random(0, 0x7FFF) << 14) ^ (uint32_t)random(0, 0x3FFF)) & 0x1FFFFFFFUL;
    port.fakeFastId = (port.fakeFastId & 0x1FFFFFF0UL) | 0x04U;
    duplicate = port.fakeFastId == port.fakeId4;
    for (uint8_t other = 0; other < CAN_FAKE_BURST_SIZE && !duplicate; ++other) {
      if (port.fakeIds[other] == port.fakeFastId) {
        duplicate = true;
      }
    }
  } while (duplicate);
}

void CanDriverFake::seedEntropyOnce() {
  static bool seeded = false;
  if (seeded) {
    return;
  }

  randomSeed((unsigned long)(micros() ^ millis() ^ 0x5A17UL));
  seeded = true;
}

void CanDriverFake::updatePortSimulation(uint8_t controller, PortState& port, uint32_t now) {
  while ((uint32_t)(now - port.lastTestTicFrameMs) >= CAN_FAKE_TEST_TIC_PERIOD_MS) {
    Frame frame;
    buildTestTicFrame(frame);
    if (!enqueueRxFrame(controller, frame)) {
      port.lastError = CAN_EVENT_FAKE_RX_OVERFLOW;
      logEvent(controller, CAN_EVENT_FAKE_RX_OVERFLOW, 0x05, _rxCount, RxCapacity);
      return;
    }

    port.lastTestTicFrameMs += CAN_FAKE_TEST_TIC_PERIOD_MS;
  }

  while ((uint32_t)(now - port.lastTestEditFrameMs) >= CAN_FAKE_TEST_EDIT_PERIOD_MS) {
    if ((uint32_t)(port.lastTestEditFrameMs + CAN_FAKE_TEST_EDIT_PERIOD_MS - port.lastTestEditDataChangeMs) >=
        CAN_FAKE_TEST_EDIT_DATA_STEP_MS) {
      updateTestEditData(port);
      port.lastTestEditDataChangeMs = port.lastTestEditFrameMs + CAN_FAKE_TEST_EDIT_PERIOD_MS;
    }

    Frame frame;
    buildTestEditFrame(port, frame);
    if (!enqueueRxFrame(controller, frame)) {
      port.lastError = CAN_EVENT_FAKE_RX_OVERFLOW;
      logEvent(controller, CAN_EVENT_FAKE_RX_OVERFLOW, 0x06, _rxCount, RxCapacity);
      return;
    }

    port.lastTestEditFrameMs += CAN_FAKE_TEST_EDIT_PERIOD_MS;
  }

  while ((uint32_t)(now - port.lastTestTimeoutFrameMs) >= CAN_FAKE_TEST_TIMEOUT_PERIOD_MS) {
    Frame frame;
    buildTestTimeoutFrame(frame);
    if (!enqueueRxFrame(controller, frame)) {
      port.lastError = CAN_EVENT_FAKE_RX_OVERFLOW;
      logEvent(controller, CAN_EVENT_FAKE_RX_OVERFLOW, 0x07, _rxCount, RxCapacity);
      return;
    }

    port.lastTestTimeoutFrameMs += CAN_FAKE_TEST_TIMEOUT_PERIOD_MS;
  }

  while ((uint32_t)(now - port.lastBurstTimestampMs) >= CAN_FAKE_BURST_PERIOD_MS) {
    const uint32_t burstTimestamp = port.lastBurstTimestampMs + CAN_FAKE_BURST_PERIOD_MS;
    const uint32_t elapsedSinceOpen = burstTimestamp - port.openTimestampMs;
    port.dataNibble = (uint8_t)((elapsedSinceOpen / CAN_FAKE_DATA_STEP_MS) & 0x0FU);

    for (uint8_t frameIndex = 0; frameIndex < CAN_FAKE_BURST_SIZE; ++frameIndex) {
      Frame frame;
      buildBurstFrame(port.fakeIds[frameIndex], port.dataNibble, frame);
      if (!enqueueRxFrame(controller, frame)) {
        port.lastError = CAN_EVENT_FAKE_RX_OVERFLOW;
        logEvent(controller, CAN_EVENT_FAKE_RX_OVERFLOW, frameIndex, _rxCount, RxCapacity);
        break;
      }
    }

    port.lastBurstTimestampMs = burstTimestamp;
    logEvent(controller, CAN_EVENT_FAKE_BURST_GENERATED, port.dataNibble);
  }

  if ((uint32_t)(now - port.lastSingleShotMs) >= CAN_FAKE_SINGLE_SHOT_PERIOD_MS) {
    Frame frame;
    buildSingleShotFrame(port.fakeId4, frame);
    if (!enqueueRxFrame(controller, frame)) {
      port.lastError = CAN_EVENT_FAKE_RX_OVERFLOW;
      logEvent(controller, CAN_EVENT_FAKE_RX_OVERFLOW, 0x03, _rxCount, RxCapacity);
      return;
    }

    port.lastSingleShotMs = now;
    logEvent(controller, CAN_EVENT_FAKE_SINGLE_SHOT_SENT, (uint8_t)(port.fakeId4 & 0xFFU));
  }

  const bool shouldSendFastFrame = port.fakeFastFirstFramePending ||
      (uint32_t)(now - port.lastFastFrameMs) >= CAN_FAKE_FAST_PERIOD_MS;
  if (!shouldSendFastFrame) {
    return;
  }

  if (!port.fakeFastFirstFramePending) {
    updateFastData(port);
  }

  Frame fastFrame;
  buildFastFrame(port, fastFrame);
  if (!enqueueRxFrame(controller, fastFrame)) {
    port.lastError = CAN_EVENT_FAKE_RX_OVERFLOW;
    if ((uint32_t)(now - port.lastFastOverflowLogMs) >= 1000UL) {
      logEvent(controller, CAN_EVENT_FAKE_RX_OVERFLOW, 0x04, _rxCount, RxCapacity);
      port.lastFastOverflowLogMs = now;
    }
    return;
  }

  port.fakeFastFirstFramePending = false;
  port.lastFastFrameMs = now;
}

bool CanDriverFake::enqueueRxFrame(uint8_t controller, const Frame& frame) {
  if (_rxCount >= RxCapacity) {
    return false;
  }

  const uint8_t index = (uint8_t)((_rxHead + _rxCount) % RxCapacity);
  _rxQueue[index].controller = controller;
  _rxQueue[index].frame = frame;
  ++_rxCount;
  return true;
}

bool CanDriverFake::dequeueRxFrame(uint8_t controller, Frame& frame) {
  if (_rxCount == 0) {
    return false;
  }

  uint8_t kept = 0;
  QueuedFrame preserved[RxCapacity];
  bool found = false;

  while (_rxCount > 0) {
    const QueuedFrame queuedFrame = _rxQueue[_rxHead];
    _rxHead = (uint8_t)((_rxHead + 1) % RxCapacity);
    --_rxCount;

    if (!found && queuedFrame.controller == controller) {
      frame = queuedFrame.frame;
      found = true;
      continue;
    }

    preserved[kept++] = queuedFrame;
  }

  _rxHead = 0;
  _rxCount = 0;
  for (uint8_t i = 0; i < kept; ++i) {
    enqueueRxFrame(preserved[i].controller, preserved[i].frame);
  }

  return found;
}

void CanDriverFake::purgeControllerFrames(uint8_t controller) {
  if (_rxCount == 0) {
    return;
  }

  uint8_t kept = 0;
  QueuedFrame preserved[RxCapacity];
  while (_rxCount > 0) {
    const QueuedFrame queuedFrame = _rxQueue[_rxHead];
    _rxHead = (uint8_t)((_rxHead + 1) % RxCapacity);
    --_rxCount;

    if (queuedFrame.controller != controller) {
      preserved[kept++] = queuedFrame;
    }
  }

  _rxHead = 0;
  _rxCount = 0;
  for (uint8_t i = 0; i < kept; ++i) {
    enqueueRxFrame(preserved[i].controller, preserved[i].frame);
  }
}

void CanDriverFake::buildBurstFrame(uint32_t id, uint8_t dataNibble, Frame& frame) const {
  frame.id = id & 0x1FFFFFFFUL;
  frame.extended = true;
  frame.rtr = false;
  frame.dlc = CAN_FAKE_FRAME_DLC;
  for (uint8_t i = 0; i < CAN_FAKE_FRAME_DLC; ++i) {
    frame.data[i] = (uint8_t)((i << 4) | (dataNibble & 0x0F));
  }
}

void CanDriverFake::buildSingleShotFrame(uint32_t id, Frame& frame) const {
  frame.id = id & 0x1FFFFFFFUL;
  frame.extended = true;
  frame.rtr = false;
  frame.dlc = CAN_FAKE_FRAME_DLC;
  for (uint8_t i = 0; i < CAN_FAKE_FRAME_DLC; ++i) {
    frame.data[i] = (uint8_t)((i << 4) | 0x03U);
  }
}

void CanDriverFake::buildFastFrame(const PortState& port, Frame& frame) const {
  frame.id = port.fakeFastId & 0x1FFFFFFFUL;
  frame.extended = true;
  frame.rtr = false;
  frame.dlc = CAN_FAKE_FRAME_DLC;
  for (uint8_t i = 0; i < CAN_FAKE_FRAME_DLC; ++i) {
    frame.data[i] = port.fakeFastData[i];
  }
}

void CanDriverFake::buildTestTicFrame(Frame& frame) const {
  frame.id = CAN_FAKE_TEST_TIC_ID;
  frame.extended = true;
  frame.rtr = false;
  frame.dlc = CAN_FAKE_FRAME_DLC;
  const uint8_t pattern[CAN_FAKE_FRAME_DLC] = {0x05, 0x15, 0x25, 0x35, 0x45, 0x55, 0x65, 0x75};
  for (uint8_t i = 0; i < CAN_FAKE_FRAME_DLC; ++i) {
    frame.data[i] = pattern[i];
  }
}

void CanDriverFake::buildTestEditFrame(const PortState& port, Frame& frame) const {
  frame.id = CAN_FAKE_TEST_EDIT_ID;
  frame.extended = true;
  frame.rtr = false;
  frame.dlc = CAN_FAKE_FRAME_DLC;
  for (uint8_t i = 0; i < CAN_FAKE_FRAME_DLC; ++i) {
    frame.data[i] = port.testEditData[i];
  }
}

void CanDriverFake::buildTestTimeoutFrame(Frame& frame) const {
  frame.id = CAN_FAKE_TEST_TIMEOUT_ID;
  frame.extended = true;
  frame.rtr = false;
  frame.dlc = CAN_FAKE_FRAME_DLC;
  const uint8_t pattern[CAN_FAKE_FRAME_DLC] = {0x07, 0x17, 0x27, 0x37, 0x47, 0x57, 0x67, 0x77};
  for (uint8_t i = 0; i < CAN_FAKE_FRAME_DLC; ++i) {
    frame.data[i] = pattern[i];
  }
}

void CanDriverFake::resetFastData(PortState& port) const {
  for (uint8_t i = 0; i < CAN_FAKE_FRAME_DLC; ++i) {
    port.fakeFastData[i] = (uint8_t)(i << 4);
  }
  port.fakeFastByteIndex = 0;
  port.fakeFastFirstFramePending = true;
}

void CanDriverFake::updateFastData(PortState& port) const {
  const uint8_t index = port.fakeFastByteIndex;
  const uint8_t lowNibble = (uint8_t)((port.fakeFastData[index] + 1U) & 0x0FU);
  port.fakeFastData[index] = (uint8_t)((index << 4) | lowNibble);
  port.fakeFastByteIndex = (uint8_t)((index + 1U) % CAN_FAKE_FRAME_DLC);
}

void CanDriverFake::resetTestEditData(PortState& port) const {
  for (uint8_t i = 0; i < CAN_FAKE_FRAME_DLC; ++i) {
    port.testEditData[i] = (uint8_t)(i << 4);
  }
  port.testEditByteIndex = 0;
}

void CanDriverFake::updateTestEditData(PortState& port) const {
  const uint8_t index = port.testEditByteIndex;
  const uint8_t lowNibble = (uint8_t)((port.testEditData[index] + 1U) & 0x0FU);
  port.testEditData[index] = (uint8_t)((index << 4) | lowNibble);
  port.testEditByteIndex = (uint8_t)((index + 1U) % CAN_FAKE_FRAME_DLC);
}

void CanDriverFake::rememberTxFrame(uint8_t controller, const Frame& frame) {
  const uint8_t index = (uint8_t)((_txHead[controller] + _txCount[controller]) % TxCapacity);
  _txQueue[controller][index] = frame;
  if (_txCount[controller] < TxCapacity) {
    ++_txCount[controller];
    return;
  }

  _txHead[controller] = (uint8_t)((_txHead[controller] + 1) % TxCapacity);
}
