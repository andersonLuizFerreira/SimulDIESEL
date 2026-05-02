#include "services/can/service/CanService.h"

#include <Arduino.h>
#include <string.h>

#include "defs.h"

namespace {
const uint8_t CAN_CONTROLLER_CAN0 = 0x00;
const uint8_t CAN_CONTROLLER_CAN1 = 0x01;
const uint8_t CAN_MODE_NORMAL = 0x00;
const uint8_t CAN_STATE_OFF = 0x00;
const uint8_t CAN_STATE_ON = 0x01;
const uint8_t CAN_INTERFACE_DISABLED = 0x00;
const uint8_t CAN_INTERFACE_CONFIGURED = 0x01;
const uint8_t CAN_INTERFACE_OPEN = 0x02;
const uint8_t CAN_RX_MAX_FRAMES_PER_RESPONSE = 3;
const uint8_t CAN_RX_FRAME_WIRE_LEN = 14;
const uint8_t CAN_RX_EVENT_FRAME_WIRE_LEN = 14;
const uint8_t CAN_RX_EVENT_MAX_FRAMES = 1;
const uint8_t CAN_DRIVER_LOG_MAX_ENTRIES_PER_RESPONSE = 6;
const uint8_t CAN_DRIVER_LOG_ENTRY_WIRE_LEN = 8;
const uint8_t CAN_TX_REQUEST_WIRE_LEN = 17;
const uint8_t CAN_TX_STATUS_ACCEPTED_SENT = 0x00;
const uint8_t CAN_TX_STATUS_INVALID_PAYLOAD = 0x01;
const uint8_t CAN_TX_STATUS_CONTROLLER_DISABLED = 0x02;
const uint8_t CAN_TX_STATUS_FAILED = 0x03;
const uint8_t CAN_TX_STATUS_PERIODIC_STARTED = 0x04;
const uint8_t CAN_TX_STATUS_PERIODIC_STOPPED = 0x05;
const uint8_t CAN_TX_STOP_ALL = 0xFF;
const uint8_t CAN_TX_SINGLE_SLOT = 0x00;
const uint32_t CAN_RX_TABLE_FULL_LOG_INTERVAL_MS = 5000UL;

#ifndef CAN_LEGACY_RX_EVENT_DIRECT
#define CAN_LEGACY_RX_EVENT_DIRECT 0
#endif

void traceCanReadAll(const char* message) {
  Serial.print("[CAN_READ_ALL] ");
  Serial.println(message);
}

void traceCanReadAllCount(const char* message, uint32_t value) {
  Serial.print("[CAN_READ_ALL] ");
  Serial.print(message);
  Serial.println(value);
}
}

void CanService::begin() {
  _driver.begin();
  _rxTable.reset();
  _rxHub.reset();
  for (uint8_t controller = 0; controller < ControllerCount; ++controller) {
    resetPort(controller);
  }
  _periodicTx.active = false;
  _periodicTx.controller = CAN_CONTROLLER_CAN0;
  _periodicTx.lastSentMs = 0;
  _periodicTx.periodMs = 0;
  _rxHead = 0;
  _rxCount = 0;
  _rxDropped = 0;
  _crudEventHead = 0;
  _crudEventCount = 0;
  _crudEventDropped = 0;
}

void CanService::loop() {
  collectRxFrames();
  checkRxTimeouts();
  publishNextCrudEvent();
  publishNextRxEvent();

  if (_periodicTx.active) {
    const uint32_t now = millis();
    if ((uint32_t)(now - _periodicTx.lastSentMs) < _periodicTx.periodMs) {
      return;
    }

    if (_ports[_periodicTx.controller].interfaceState != CAN_INTERFACE_OPEN) {
      _periodicTx.active = false;
      return;
    }

    _driver.send(_periodicTx.controller, _periodicTx.frame);
    _periodicTx.lastSentMs = now;
  }
}

void CanService::setEventPublisher(EventPublisher publisher, void* context) {
  _eventPublisher = publisher;
  _eventPublisherContext = context;
}

bool CanService::handleTlv(uint8_t type,
                           const uint8_t* value,
                           uint8_t valueLen,
                           uint8_t* responseValue,
                           uint8_t& responseValueLen,
                           uint8_t& errorCode) {
  responseValueLen = 0;
  errorCode = 0;

  switch (type) {
    case CMD_CAN_CONFIG:
      return handleConfig(value, valueLen, responseValue, responseValueLen, errorCode);
    case CMD_CAN_ENABLE:
      return handleEnable(value, valueLen, responseValue, responseValueLen, errorCode);
    case CMD_CAN_STATUS:
      return handleStatus(value, valueLen, responseValue, responseValueLen, errorCode);
    case CMD_CAN_RESET:
      return handleReset(value, valueLen, responseValue, responseValueLen, errorCode);
    case CMD_CAN_RX_POLL:
      return handleRxPoll(value, valueLen, responseValue, responseValueLen, errorCode);
    case CMD_CAN_DRIVER_LOG_POLL:
      return handleDriverLogPoll(value, valueLen, responseValue, responseValueLen, errorCode);
    case CMD_CAN_READ_ALL:
      return handleReadAll(value, valueLen, responseValue, responseValueLen, errorCode);
    case CMD_CAN_TX:
      return handleTx(value, valueLen, responseValue, responseValueLen, errorCode);
    case CMD_CAN_TX_STOP:
      return handleTxStop(value, valueLen, responseValue, responseValueLen, errorCode);
    default:
      errorCode = UCE_ERROR_COMMAND_NOT_SUPPORTED;
      return false;
  }
}

bool CanService::validateController(uint8_t controller) const {
  return controller == CAN_CONTROLLER_CAN0 || controller == CAN_CONTROLLER_CAN1;
}

void CanService::resetPort(uint8_t controller) {
  if (!validateController(controller)) {
    return;
  }

  _ports[controller].bitrateCode = CAN_BITRATE_250_KBPS;
  _ports[controller].modeCode = CAN_MODE_NORMAL;
  _ports[controller].interfaceState = CAN_INTERFACE_DISABLED;
}

bool CanService::validateBitrate(uint8_t bitrateCode) const {
  return bitrateCode <= CAN_BITRATE_1000_KBPS;
}

bool CanService::validateMode(uint8_t modeCode) const {
  return modeCode <= 0x01;
}

bool CanService::validateRxMode(uint8_t rxMode) const {
  return rxMode <= CAN_RX_MODE_DIRECT_ONLY;
}

bool CanService::validateEnableState(uint8_t state) const {
  return state == CAN_STATE_OFF || state == CAN_STATE_ON;
}

bool CanService::handleConfig(const uint8_t* value,
                              uint8_t valueLen,
                              uint8_t* responseValue,
                              uint8_t& responseValueLen,
                              uint8_t& errorCode) {
  if (!value || (valueLen != 3 && valueLen != 4)) {
    errorCode = UCE_ERROR_INVALID_PAYLOAD;
    return false;
  }

  const uint8_t controller = value[0];
  const uint8_t bitrateCode = value[1];
  const uint8_t modeCode = value[2];
  const uint8_t rxMode = valueLen >= 4 ? value[3] : CAN_RX_MODE_AUTO;
  if (!validateController(controller) || !validateBitrate(bitrateCode) || !validateMode(modeCode) || !validateRxMode(rxMode)) {
    errorCode = UCE_ERROR_INVALID_STATE;
    return false;
  }

  if (!_driver.configure(controller, bitrateCode, modeCode)) {
    errorCode = UCE_ERROR_INVALID_STATE;
    return false;
  }

  PortState& port = _ports[controller];
  port.bitrateCode = bitrateCode;
  port.modeCode = modeCode;
  _rxHub.setMode((CanRxMode)rxMode);
  if (port.interfaceState != CAN_INTERFACE_OPEN) {
    port.interfaceState = CAN_INTERFACE_CONFIGURED;
  }

  if (responseValue) {
    responseValue[0] = controller;
    responseValue[1] = port.bitrateCode;
    responseValue[2] = port.modeCode;
  }
  responseValueLen = 3;
  return true;
}

bool CanService::handleEnable(const uint8_t* value,
                              uint8_t valueLen,
                              uint8_t* responseValue,
                              uint8_t& responseValueLen,
                              uint8_t& errorCode) {
  if (!value || valueLen != 2) {
    errorCode = UCE_ERROR_INVALID_PAYLOAD;
    return false;
  }

  const uint8_t controller = value[0];
  const uint8_t requestedState = value[1];
  if (!validateController(controller) || !validateEnableState(requestedState)) {
    errorCode = UCE_ERROR_INVALID_STATE;
    return false;
  }

  const bool driverOk = (requestedState == CAN_STATE_ON) ? _driver.open(controller) : _driver.close(controller);
  if (!driverOk) {
    errorCode = UCE_ERROR_INVALID_STATE;
    return false;
  }

  PortState& port = _ports[controller];
  port.interfaceState = (requestedState == CAN_STATE_ON) ? CAN_INTERFACE_OPEN : CAN_INTERFACE_DISABLED;
  _rxTable.reset();
  _rxHead = 0;
  _rxCount = 0;
  _crudEventHead = 0;
  _crudEventCount = 0;

  if (responseValue) {
    responseValue[0] = controller;
    responseValue[1] = (port.interfaceState == CAN_INTERFACE_OPEN) ? CAN_STATE_ON : CAN_STATE_OFF;
  }
  responseValueLen = 2;
  return true;
}

bool CanService::handleStatus(const uint8_t* value,
                              uint8_t valueLen,
                              uint8_t* responseValue,
                              uint8_t& responseValueLen,
                              uint8_t& errorCode) {
  if (!value || valueLen != 1) {
    errorCode = UCE_ERROR_INVALID_PAYLOAD;
    return false;
  }

  const uint8_t controller = value[0];
  if (!validateController(controller)) {
    errorCode = UCE_ERROR_INVALID_STATE;
    return false;
  }

  CanDriverSelected::Status status;
  if (!_driver.getStatus(controller, status)) {
    errorCode = UCE_ERROR_INVALID_STATE;
    return false;
  }

  const PortState& port = _ports[controller];
  if (responseValue) {
    responseValue[0] = controller;
    responseValue[1] = port.interfaceState;
    responseValue[2] = port.bitrateCode;
    responseValue[3] = port.modeCode;
  }
  responseValueLen = 4;
  return true;
}

bool CanService::handleReset(const uint8_t* value,
                             uint8_t valueLen,
                             uint8_t* responseValue,
                             uint8_t& responseValueLen,
                             uint8_t& errorCode) {
  if (!value || valueLen != 1) {
    errorCode = UCE_ERROR_INVALID_PAYLOAD;
    return false;
  }

  const uint8_t controller = value[0];
  if (!validateController(controller)) {
    errorCode = UCE_ERROR_INVALID_STATE;
    return false;
  }

  if (!_driver.reset(controller)) {
    errorCode = UCE_ERROR_INVALID_STATE;
    return false;
  }

  resetPort(controller);
  _rxTable.reset();
  _rxHead = 0;
  _rxCount = 0;
  _crudEventHead = 0;
  _crudEventCount = 0;
  if (_periodicTx.active && _periodicTx.controller == controller) {
    _periodicTx.active = false;
  }
  if (responseValue) {
    responseValue[0] = controller;
    responseValue[1] = 0x01;
  }
  responseValueLen = 2;
  return true;
}

bool CanService::handleRxPoll(const uint8_t* value,
                              uint8_t valueLen,
                              uint8_t* responseValue,
                              uint8_t& responseValueLen,
                              uint8_t& errorCode) {
  if (!value || valueLen != 1) {
    errorCode = UCE_ERROR_INVALID_PAYLOAD;
    return false;
  }

  const uint8_t controller = value[0];
  if (!validateController(controller)) {
    errorCode = UCE_ERROR_INVALID_STATE;
    return false;
  }

  CanDriverSelected::Frame frames[CAN_RX_MAX_FRAMES_PER_RESPONSE];
  uint8_t frameCount = copyQueuedRxFrames(controller, frames, CAN_RX_MAX_FRAMES_PER_RESPONSE);
  if (frameCount < CAN_RX_MAX_FRAMES_PER_RESPONSE) {
    uint8_t physicalCount = 0;
    if (!_driver.pollReceived(controller, &frames[frameCount], CAN_RX_MAX_FRAMES_PER_RESPONSE - frameCount, physicalCount)) {
      errorCode = UCE_ERROR_INVALID_STATE;
      return false;
    }
    frameCount += physicalCount;
  }

  if (responseValue) {
    responseValue[0] = controller;
    responseValue[1] = frameCount;
    uint8_t offset = 2;
    for (uint8_t i = 0; i < frameCount; ++i) {
      encodeRxFrameBigEndian(&responseValue[offset], frames[i]);
      offset += CAN_RX_FRAME_WIRE_LEN;
    }
  }

  responseValueLen = 2 + (frameCount * CAN_RX_FRAME_WIRE_LEN);
  return true;
}

void CanService::collectRxFrames() {
  if (_readAllSnapshotActive) {
    return;
  }

  for (uint8_t controller = 0; controller < ControllerCount; ++controller) {
    if (_ports[controller].interfaceState != CAN_INTERFACE_OPEN) {
      continue;
    }

    CanDriverSelected::Frame frames[CAN_RX_MAX_FRAMES_PER_RESPONSE];
    uint8_t frameCount = 0;
    if (!_driver.pollReceived(controller, frames, CAN_RX_MAX_FRAMES_PER_RESPONSE, frameCount)) {
      continue;
    }

    for (uint8_t i = 0; i < frameCount; ++i) {
      const CanRxHub::ProcessResult hubResult = _rxHub.process(frames[i], _rxTable, millis());
      if (hubResult.direct) {
        enqueueRxFrame(controller, frames[i]);
      }

      if (hubResult.tableFullFallback) {
        publishTableFullFallbackLog(frames[i]);
      }

      const CanRxTableManager::CrudEvent& crudEvent = hubResult.crudEvent;
      if (!crudEvent.valid) {
        continue;
      }

      CanCrudProtocol::Record record;
      record.index = crudEvent.entry.index;
      record.flags = crudEvent.entry.flags;
      record.canId = crudEvent.entry.canId;
      record.dlc = crudEvent.entry.dlc;
      for (uint8_t dataIndex = 0; dataIndex < 8; ++dataIndex) {
        record.data[dataIndex] = crudEvent.entry.data[dataIndex];
      }
      record.cycleTime = crudEvent.entry.cycleTime;
      record.messageOrder = crudEvent.entry.messageOrder;

      uint8_t payload[CanCrudProtocol::EditPayloadMaxLen] = {0};
      uint8_t payloadLen = 0;
      const bool encoded = (hubResult.tableResult == CanRxTableManager::ProcessCreate)
          ? _crudProtocol.encodeCreate(record, payload, payloadLen)
          : _crudProtocol.encodeEdit(record, crudEvent.mask, payload, payloadLen);

      if (encoded) {
        enqueueCrudEvent(crudEvent.type, payload, payloadLen);
      }
    }
  }
}

void CanService::checkRxTimeouts() {
  if (_rxHub.mode() == CAN_RX_MODE_DIRECT_ONLY || _readAllSnapshotActive || _crudEventCount >= CrudEventQueueCapacity) {
    return;
  }

  CanRxTableManager::CrudEvent crudEvent;
  const CanRxTableManager::ProcessResult result = _rxTable.checkTimeouts(millis(), crudEvent);
  if (result != CanRxTableManager::ProcessDelete || !crudEvent.valid) {
    return;
  }

  uint8_t payload[CanCrudProtocol::DeletePayloadLen] = {0};
  uint8_t payloadLen = 0;
  if (_crudProtocol.encodeDelete(
          crudEvent.entry.index,
          crudEvent.mask,
          crudEvent.entry.messageOrder,
          payload,
          payloadLen)) {
    enqueueCrudEvent(crudEvent.type, payload, payloadLen);
  }
}

bool CanService::enqueueRxFrame(uint8_t controller, const CanDriverSelected::Frame& frame) {
  if (_rxCount >= RxQueueCapacity) {
    ++_rxDropped;
    return false;
  }

  const uint8_t index = (uint8_t)((_rxHead + _rxCount) % RxQueueCapacity);
  _rxQueue[index].controller = controller;
  _rxQueue[index].frame = frame;
  ++_rxCount;
  return true;
}

bool CanService::dequeueRxFrame(QueuedRxFrame& queuedFrame) {
  if (_rxCount == 0) {
    return false;
  }

  queuedFrame = _rxQueue[_rxHead];
  _rxHead = (uint8_t)((_rxHead + 1) % RxQueueCapacity);
  --_rxCount;
  return true;
}

bool CanService::peekRxFrame(QueuedRxFrame& queuedFrame) const {
  if (_rxCount == 0) {
    return false;
  }

  queuedFrame = _rxQueue[_rxHead];
  return true;
}

bool CanService::enqueueCrudEvent(uint8_t type, const uint8_t* payload, uint8_t payloadLen) {
  if (type == 0 || !payload || payloadLen == 0 || payloadLen > CanCrudProtocol::EditPayloadMaxLen) {
    return false;
  }

  if (_crudEventCount >= CrudEventQueueCapacity) {
    ++_crudEventDropped;
    return false;
  }

  const uint8_t index = (uint8_t)((_crudEventHead + _crudEventCount) % CrudEventQueueCapacity);
  _crudEventQueue[index].type = type;
  _crudEventQueue[index].payloadLen = payloadLen;
  memcpy(_crudEventQueue[index].payload, payload, payloadLen);
  ++_crudEventCount;
  return true;
}

bool CanService::dequeueCrudEvent(PendingCrudEvent& event) {
  if (_crudEventCount == 0) {
    return false;
  }

  event = _crudEventQueue[_crudEventHead];
  _crudEventHead = (uint8_t)((_crudEventHead + 1) % CrudEventQueueCapacity);
  --_crudEventCount;
  return true;
}

bool CanService::peekCrudEvent(PendingCrudEvent& event) const {
  if (_crudEventCount == 0) {
    return false;
  }

  event = _crudEventQueue[_crudEventHead];
  return true;
}

bool CanService::publishNextCrudEvent() {
  if (!_eventPublisher) {
    return false;
  }

  PendingCrudEvent event;
  if (!peekCrudEvent(event)) {
    return false;
  }

  if (!_eventPublisher(_eventPublisherContext, event.type, event.payload, event.payloadLen)) {
    return false;
  }

  if (event.type == CMD_CAN_ROW) {
    traceCanReadAllCount("UCE enviou CAN_ROW. pending=", (uint32_t)(_crudEventCount - 1));
  } else if (event.type == CMD_CAN_READ_ALL_DONE) {
    traceCanReadAll("UCE enviou CAN_READ_ALL_DONE.");
  }

  dequeueCrudEvent(event);
  if (_readAllSnapshotActive && _crudEventCount == 0) {
    _readAllSnapshotActive = false;
  }
  return true;
}

bool CanService::publishNextRxEvent() {
  if (!_eventPublisher) {
    return false;
  }

  QueuedRxFrame queuedFrame;
  if (!peekRxFrame(queuedFrame)) {
    return false;
  }

  uint8_t payload[2 + (CAN_RX_EVENT_MAX_FRAMES * CAN_RX_EVENT_FRAME_WIRE_LEN)] = {0};
  payload[0] = queuedFrame.controller;
  payload[1] = 1;
  encodeRxFrameLittleEndian(&payload[2], queuedFrame.frame);

  if (!_eventPublisher(_eventPublisherContext, CMD_CAN_RX_EVENT, payload, sizeof(payload))) {
    return false;
  }

  dequeueRxFrame(queuedFrame);
  return true;
}

void CanService::publishTableFullFallbackLog(const CanDriverSelected::Frame& frame) {
  static uint32_t lastLogMs = 0;
  const uint32_t nowMs = millis();
  if ((uint32_t)(nowMs - lastLogMs) < CAN_RX_TABLE_FULL_LOG_INTERVAL_MS) {
    return;
  }

  lastLogMs = nowMs;
  Serial.print("CAN RX TABLE FULL - DIRECT FALLBACK id=0x");
  Serial.println(frame.id, HEX);
}

uint8_t CanService::copyQueuedRxFrames(uint8_t controller, CanDriverSelected::Frame* frames, uint8_t maxFrames) {
  if (!frames || maxFrames == 0 || _rxCount == 0) {
    return 0;
  }

  uint8_t copied = 0;
  uint8_t kept = 0;
  QueuedRxFrame temp[RxQueueCapacity];
  while (_rxCount > 0) {
    QueuedRxFrame queuedFrame;
    dequeueRxFrame(queuedFrame);
    if (queuedFrame.controller == controller && copied < maxFrames) {
      frames[copied++] = queuedFrame.frame;
    } else if (kept < RxQueueCapacity) {
      temp[kept++] = queuedFrame;
    }
  }

  _rxHead = 0;
  _rxCount = 0;
  for (uint8_t i = 0; i < kept; ++i) {
    enqueueRxFrame(temp[i].controller, temp[i].frame);
  }

  return copied;
}

void CanService::encodeRxFrameLittleEndian(uint8_t* out, const CanDriverSelected::Frame& frame) const {
  const bool extended = frame.extended;
  const uint32_t id = frame.id & (extended ? 0x1FFFFFFFUL : 0x7FFUL);
  out[0] = (uint8_t)(id & 0xFFU);
  out[1] = (uint8_t)((id >> 8) & 0xFFU);
  out[2] = (uint8_t)((id >> 16) & 0xFFU);
  out[3] = (uint8_t)((id >> 24) & 0xFFU);
  out[4] = (extended ? 0x01 : 0x00) | (frame.rtr ? 0x02 : 0x00);
  out[5] = frame.dlc;
  for (uint8_t dataIndex = 0; dataIndex < 8; ++dataIndex) {
    out[6 + dataIndex] = frame.data[dataIndex];
  }
}

void CanService::encodeRxFrameBigEndian(uint8_t* out, const CanDriverSelected::Frame& frame) const {
  const bool extended = frame.extended;
  const uint32_t id = frame.id & (extended ? 0x1FFFFFFFUL : 0x7FFUL);
  out[0] = (uint8_t)((id >> 24) & 0xFFU);
  out[1] = (uint8_t)((id >> 16) & 0xFFU);
  out[2] = (uint8_t)((id >> 8) & 0xFFU);
  out[3] = (uint8_t)(id & 0xFFU);
  out[4] = (extended ? 0x01 : 0x00) | (frame.rtr ? 0x02 : 0x00);
  out[5] = frame.dlc;
  for (uint8_t dataIndex = 0; dataIndex < 8; ++dataIndex) {
    out[6 + dataIndex] = frame.data[dataIndex];
  }
}

bool CanService::handleDriverLogPoll(const uint8_t* value,
                                     uint8_t valueLen,
                                     uint8_t* responseValue,
                                     uint8_t& responseValueLen,
                                     uint8_t& errorCode) {
  if (!value || valueLen != 1) {
    errorCode = UCE_ERROR_INVALID_PAYLOAD;
    return false;
  }

  const uint8_t controller = value[0];
  if (!validateController(controller)) {
    errorCode = UCE_ERROR_INVALID_STATE;
    return false;
  }

  CanDriverSelected::LogEntry entries[CAN_DRIVER_LOG_MAX_ENTRIES_PER_RESPONSE];
  uint8_t entryCount = 0;
  if (!_driver.pollLog(controller, entries, CAN_DRIVER_LOG_MAX_ENTRIES_PER_RESPONSE, entryCount)) {
    errorCode = UCE_ERROR_INVALID_STATE;
    return false;
  }

  if (responseValue) {
    responseValue[0] = controller;
    responseValue[1] = entryCount;
    uint8_t offset = 2;
    for (uint8_t i = 0; i < entryCount; ++i) {
      responseValue[offset++] = entries[i].timestampLow;
      responseValue[offset++] = entries[i].eventCode;
      responseValue[offset++] = entries[i].interfaceState;
      responseValue[offset++] = entries[i].bitrateCode;
      responseValue[offset++] = entries[i].modeCode;
      responseValue[offset++] = entries[i].detail0;
      responseValue[offset++] = entries[i].detail1;
      responseValue[offset++] = entries[i].detail2;
    }
  }

  responseValueLen = 2 + (entryCount * CAN_DRIVER_LOG_ENTRY_WIRE_LEN);
  return true;
}

bool CanService::handleReadAll(const uint8_t* value,
                               uint8_t valueLen,
                               uint8_t* responseValue,
                               uint8_t& responseValueLen,
                               uint8_t& errorCode) {
  traceCanReadAll("UCE recebeu CAN_READ_ALL.");

  if (!_crudProtocol.decodeReadAllRequest(value, valueLen)) {
    errorCode = UCE_ERROR_INVALID_PAYLOAD;
    return false;
  }

  CanRxTableManager::Entry entries[CanRxTableManager::Capacity];
  const uint8_t validCount = _rxTable.snapshotValidEntries(entries, CanRxTableManager::Capacity);
  traceCanReadAllCount("UCE snapshot validCount=", validCount);

  _crudEventHead = 0;
  _crudEventCount = 0;
  _readAllSnapshotActive = false;

  uint8_t payload[CanCrudProtocol::RowPayloadLen] = {0};
  uint8_t payloadLen = 0;
  for (uint8_t index = 0; index < validCount; ++index) {
    CanCrudProtocol::Record record;
    record.index = entries[index].index;
    record.flags = entries[index].flags;
    record.canId = entries[index].canId;
    record.dlc = entries[index].dlc;
    for (uint8_t dataIndex = 0; dataIndex < 8; ++dataIndex) {
      record.data[dataIndex] = entries[index].data[dataIndex];
    }
    record.cycleTime = entries[index].cycleTime;
    record.messageOrder = entries[index].messageOrder;

    if (!_crudProtocol.encodeRow(record, payload, payloadLen) ||
        !enqueueCrudEvent(CMD_CAN_ROW, payload, payloadLen)) {
      errorCode = UCE_ERROR_INVALID_STATE;
      _crudEventHead = 0;
      _crudEventCount = 0;
      return false;
    }
  }

  uint8_t donePayload[CanCrudProtocol::ReadAllDonePayloadLen] = {0};
  uint8_t donePayloadLen = 0;
  if (!_crudProtocol.encodeReadAllDone(validCount, _rxTable.currentMessageOrder(), donePayload, donePayloadLen) ||
      !enqueueCrudEvent(CMD_CAN_READ_ALL_DONE, donePayload, donePayloadLen)) {
    errorCode = UCE_ERROR_INVALID_STATE;
    _crudEventHead = 0;
    _crudEventCount = 0;
    return false;
  }

  _readAllSnapshotActive = _crudEventCount > 0;
  traceCanReadAllCount("UCE enfileirou eventos do snapshot=", _crudEventCount);
  responseValueLen = 0;
  return true;
}

bool CanService::handleTx(const uint8_t* value,
                          uint8_t valueLen,
                          uint8_t* responseValue,
                          uint8_t& responseValueLen,
                          uint8_t& errorCode) {
  if (responseValue) {
    responseValue[0] = value && valueLen > 0 ? value[0] : 0x00;
    responseValue[1] = CAN_TX_STATUS_INVALID_PAYLOAD;
    responseValue[2] = CAN_TX_SINGLE_SLOT;
  }
  responseValueLen = 3;

  if (!value || valueLen != CAN_TX_REQUEST_WIRE_LEN) {
    return true;
  }

  const uint8_t controller = value[0];
  const uint8_t flags = value[1];
  const uint8_t dlc = value[2];
  const uint16_t periodMs = (uint16_t)value[3] | ((uint16_t)value[4] << 8);
  const bool extended = (flags & 0x01) != 0;
  const bool rtr = (flags & 0x02) != 0;
  const uint32_t id = ((uint32_t)value[5]) |
                      ((uint32_t)value[6] << 8) |
                      ((uint32_t)value[7] << 16) |
                      ((uint32_t)value[8] << 24);

  if (!validateController(controller) || (flags & 0xFC) != 0 || dlc > 8) {
    return true;
  }

  if (_ports[controller].interfaceState != CAN_INTERFACE_OPEN) {
    if (responseValue) {
      responseValue[0] = controller;
      responseValue[1] = CAN_TX_STATUS_CONTROLLER_DISABLED;
    }
    return true;
  }

  CanDriverSelected::Frame frame;
  frame.extended = extended;
  frame.rtr = rtr;
  frame.dlc = dlc;
  frame.id = id & (extended ? 0x1FFFFFFFUL : 0x7FFUL);
  for (uint8_t i = 0; i < 8; ++i) {
    frame.data[i] = value[9 + i];
  }

  if (periodMs == 0) {
    const bool sent = _driver.send(controller, frame);
    if (responseValue) {
      responseValue[0] = controller;
      responseValue[1] = sent ? CAN_TX_STATUS_ACCEPTED_SENT : CAN_TX_STATUS_FAILED;
    }
    return true;
  }

  _periodicTx.active = true;
  _periodicTx.controller = controller;
  _periodicTx.frame = frame;
  _periodicTx.periodMs = periodMs;
  _periodicTx.lastSentMs = millis();
  _driver.send(controller, frame);

  if (responseValue) {
    responseValue[0] = controller;
    responseValue[1] = CAN_TX_STATUS_PERIODIC_STARTED;
    responseValue[2] = CAN_TX_SINGLE_SLOT;
  }
  return true;
}

bool CanService::handleTxStop(const uint8_t* value,
                              uint8_t valueLen,
                              uint8_t* responseValue,
                              uint8_t& responseValueLen,
                              uint8_t& errorCode) {
  if (!value || valueLen != 2 || !validateController(value[0])) {
    errorCode = UCE_ERROR_INVALID_PAYLOAD;
    return false;
  }

  const uint8_t controller = value[0];
  const uint8_t slotOrAll = value[1];
  if (slotOrAll != CAN_TX_SINGLE_SLOT && slotOrAll != CAN_TX_STOP_ALL) {
    errorCode = UCE_ERROR_INVALID_PAYLOAD;
    return false;
  }

  if (_periodicTx.active && _periodicTx.controller == controller) {
    _periodicTx.active = false;
  }

  if (responseValue) {
    responseValue[0] = controller;
    responseValue[1] = CAN_TX_STATUS_PERIODIC_STOPPED;
  }
  responseValueLen = 2;
  return true;
}
