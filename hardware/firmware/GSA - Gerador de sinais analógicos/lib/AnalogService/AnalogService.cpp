#include "AnalogService.h"

#include "TlvBuilder.h"

namespace {
uint16_t channelMaxVoltageMv(uint8_t channel) {
  return (channel <= 8U) ? GSA_LOW_RANGE_MAX_MV : GSA_HIGH_RANGE_MAX_MV;
}

int32_t simulatedPhysicalVoutMv(uint8_t channel) {
  return (int32_t)GSA_SIM_BASE_VOUT_MV + ((int32_t)channel * (int32_t)GSA_SIM_VOUT_STEP_MV);
}

int32_t simulatedPhysicalIreadMa(uint8_t channel) {
  return (int32_t)GSA_SIM_BASE_IREAD_MA + ((int32_t)channel * (int32_t)GSA_SIM_IREAD_STEP_MA);
}

uint8_t scaleToRaw(int32_t value, int32_t fullScale) {
  return (uint8_t)((value * 255L + (fullScale / 2L)) / fullScale);
}

int16_t readI16Le(const uint8_t* data) {
  return (int16_t)((uint16_t)data[0] | ((uint16_t)data[1] << 8));
}

void writeI16Le(uint8_t* out, int16_t value) {
  out[0] = (uint8_t)(value & 0xFF);
  out[1] = (uint8_t)((uint16_t)value >> 8);
}
}

AnalogService::AnalogService(EepromService& eeprom, BusArbiterService& busArbiter)
  : _eeprom(eeprom),
    _busArbiter(busArbiter),
    _eventHead(0),
    _eventTail(0),
    _eventCount(0)
{
}

void AnalogService::begin() {
  GsaChannelOffsets persistedOffsets[GSA_CHANNEL_COUNT];
  _eeprom.loadOffsets(persistedOffsets, GSA_CHANNEL_COUNT);

  bool normalizedOffsets = false;
  _eventHead = 0;
  _eventTail = 0;
  _eventCount = 0;

  for (uint8_t index = 0; index < GSA_CHANNEL_COUNT; index++) {
    _channels[index].setpointRaw = 0;
    _channels[index].requestedEnable = false;
    _channels[index].effectiveEnable = false;
    _channels[index].faultLatched = false;
    _channels[index].offsets = persistedOffsets[index];

    uint8_t voutRaw = 0;
    uint8_t ireadRaw = 0;
    uint8_t channel = (uint8_t)(index + 1);
    if (!evaluateTelemetry(channel, _channels[index].offsets, voutRaw, ireadRaw)) {
      _channels[index].offsets = { 0, 0, 0 };
      normalizedOffsets = true;
    }
  }

  _busArbiter.begin();
  refreshAllTelemetry();

  if (normalizedOffsets) {
    persistOffsets();
  }
}

void AnalogService::tick() {
  _busArbiter.tick();
  processCompletedHardwareOperations();
  refreshAllTelemetry();
}

bool AnalogService::handleTlv(const TlvFrame& tlv, uint8_t* txOut, uint8_t& txLenOut) {
  switch (tlv.t) {
    case CMD_SETPOINT:
      return handleSetpoint(tlv, txOut, txLenOut);

    case CMD_ENABLE_CHANNEL:
      return handleEnableChannel(tlv, txOut, txLenOut);

    case CMD_ENABLE_GLOBAL:
      return handleEnableGlobal(tlv, txOut, txLenOut);

    case CMD_FAULT_RESET:
      return handleFaultReset(tlv, txOut, txLenOut);

    case CMD_OFFSET_SET:
      return handleOffsetSet(tlv, txOut, txLenOut);

    case CMD_OFFSET_SAVE:
      return handleOffsetSave(tlv, txOut, txLenOut);

    case CMD_OFFSET_RESET_ALL:
      return handleOffsetResetAll(tlv, txOut, txLenOut);

    case CMD_STATUS_CHANNEL:
      return handleStatus(tlv, txOut, txLenOut);

    default:
      return false;
  }
}

bool AnalogService::popPendingEvent(uint8_t* txOut, uint8_t& txLenOut) {
  if (_eventCount == 0U) {
    txLenOut = 0;
    return false;
  }

  GsaEvent& event = _events[_eventHead];
  txLenOut = TlvBuilder::build(event.type, event.payload, event.payloadLen, txOut, TLV_MAX_LEN);
  if (txLenOut == 0) {
    return false;
  }

  _eventHead = (uint8_t)((_eventHead + 1U) % GSA_EVENT_QUEUE_SIZE);
  _eventCount--;
  return true;
}

bool AnalogService::hasPendingEvent() const {
  return _eventCount != 0U;
}

bool AnalogService::handleSetpoint(const TlvFrame& tlv, uint8_t* txOut, uint8_t& txLenOut) {
  if (tlv.l != 2 || tlv.v == nullptr) {
    return buildFunctionalError(CMD_SETPOINT, 0, GSA_ERROR_INVALID_PAYLOAD, txOut, txLenOut);
  }

  uint8_t channel = tlv.v[0];
  uint8_t newValue = tlv.v[1];
  int8_t index = channelToIndex(channel);
  if (index < 0) {
    return buildFunctionalError(CMD_SETPOINT, channel, GSA_ERROR_INVALID_CHANNEL, txOut, txLenOut);
  }

  GsaChannelState& state = _channels[index];
  if (!state.effectiveEnable) {
    state.setpointRaw = newValue;
    refreshChannelTelemetry((uint8_t)index);
  } else if (state.setpointRaw != newValue) {
    GsaPhysicalOperation operation = {};
    operation.originType = CMD_SETPOINT;
    operation.channel = channel;
    operation.requestedSetpointRaw = newValue;
    operation.applySetpoint = true;

    if (!queuePhysicalOperation(operation)) {
      return buildFunctionalError(CMD_SETPOINT, channel, GSA_ERROR_OPERATION_NOT_ALLOWED, txOut, txLenOut);
    }
  }

  // A resposta síncrona só confirma o setpoint aceito para processamento.
  // Se houver etapa elétrica, a conclusão real virá depois no evento 0x31.
  uint8_t payload[2] = { channel, newValue };
  txLenOut = TlvBuilder::build(CMD_SETPOINT, payload, sizeof(payload), txOut, TLV_MAX_LEN);
  return txLenOut != 0;
}

bool AnalogService::handleEnableChannel(const TlvFrame& tlv, uint8_t* txOut, uint8_t& txLenOut) {
  if (tlv.l != 2 || tlv.v == nullptr) {
    return buildFunctionalError(CMD_ENABLE_CHANNEL, 0, GSA_ERROR_INVALID_PAYLOAD, txOut, txLenOut);
  }

  uint8_t channel = tlv.v[0];
  uint8_t requestedState = tlv.v[1];
  int8_t index = channelToIndex(channel);

  if (index < 0) {
    return buildFunctionalError(CMD_ENABLE_CHANNEL, channel, GSA_ERROR_INVALID_CHANNEL, txOut, txLenOut);
  }

  if (!isValidState(requestedState)) {
    return buildFunctionalError(CMD_ENABLE_CHANNEL, channel, GSA_ERROR_INVALID_STATE, txOut, txLenOut);
  }

  GsaChannelState& state = _channels[index];
  if (requestedState == 1U && state.faultLatched) {
    return buildFunctionalError(CMD_ENABLE_CHANNEL, channel, GSA_ERROR_FAULT_LATCHED, txOut, txLenOut);
  }

  bool desiredEnable = requestedState != 0U;
  if (state.effectiveEnable != desiredEnable) {
    GsaPhysicalOperation operation = {};
    operation.originType = CMD_ENABLE_CHANNEL;
    operation.channel = channel;
    operation.requestedSetpointRaw = state.setpointRaw;
    operation.requestedEnable = desiredEnable;
    operation.applyEnable = true;
    operation.disableOutput = !desiredEnable;

    if (!queuePhysicalOperation(operation)) {
      return buildFunctionalError(CMD_ENABLE_CHANNEL, channel, GSA_ERROR_OPERATION_NOT_ALLOWED, txOut, txLenOut);
    }
  }

  // A resposta síncrona informa apenas o estado aceito.
  // A aplicação elétrica no DAC/hub é confirmada depois via 0x31.
  uint8_t payload[2] = { channel, requestedState };
  txLenOut = TlvBuilder::build(CMD_ENABLE_CHANNEL, payload, sizeof(payload), txOut, TLV_MAX_LEN);
  return txLenOut != 0;
}

bool AnalogService::handleEnableGlobal(const TlvFrame& tlv, uint8_t* txOut, uint8_t& txLenOut) {
  if (tlv.l != 1 || tlv.v == nullptr) {
    return buildFunctionalError(CMD_ENABLE_GLOBAL, 0, GSA_ERROR_INVALID_PAYLOAD, txOut, txLenOut);
  }

  uint8_t requestedState = tlv.v[0];
  if (!isValidState(requestedState)) {
    return buildFunctionalError(CMD_ENABLE_GLOBAL, 0, GSA_ERROR_INVALID_STATE, txOut, txLenOut);
  }

  GsaPhysicalOperation operations[GSA_CHANNEL_COUNT];
  uint8_t operationCount = 0;

  for (uint8_t index = 0; index < GSA_CHANNEL_COUNT; index++) {
    GsaChannelState& state = _channels[index];
    bool desiredEnable = requestedState != 0U && !state.faultLatched;
    if (state.effectiveEnable == desiredEnable) {
      continue;
    }

    GsaPhysicalOperation& operation = operations[operationCount++];
    operation.originType = CMD_ENABLE_GLOBAL;
    operation.channel = (uint8_t)(index + 1U);
    operation.requestedSetpointRaw = state.setpointRaw;
    operation.requestedEnable = desiredEnable;
    operation.applyEnable = true;
    operation.disableOutput = !desiredEnable;
  }

  if (!queuePhysicalOperations(operations, operationCount)) {
    return buildFunctionalError(CMD_ENABLE_GLOBAL, 0, GSA_ERROR_OPERATION_NOT_ALLOWED, txOut, txLenOut);
  }

  // O retorno síncrono só confirma o pedido aceito e quantos canais foram enfileirados.
  uint8_t payload[2] = { requestedState, operationCount };
  txLenOut = TlvBuilder::build(CMD_ENABLE_GLOBAL, payload, sizeof(payload), txOut, TLV_MAX_LEN);
  return txLenOut != 0;
}

bool AnalogService::handleFaultReset(const TlvFrame& tlv, uint8_t* txOut, uint8_t& txLenOut) {
  if (tlv.l != 1 || tlv.v == nullptr) {
    return buildFunctionalError(CMD_FAULT_RESET, 0, GSA_ERROR_INVALID_PAYLOAD, txOut, txLenOut);
  }

  uint8_t channel = tlv.v[0];
  int8_t index = channelToIndex(channel);
  if (index < 0) {
    return buildFunctionalError(CMD_FAULT_RESET, channel, GSA_ERROR_INVALID_CHANNEL, txOut, txLenOut);
  }

  GsaChannelState& state = _channels[index];
  if (state.faultLatched || state.effectiveEnable) {
    GsaPhysicalOperation operation = {};
    operation.originType = CMD_FAULT_RESET;
    operation.channel = channel;
    operation.requestedEnable = false;
    operation.applyEnable = true;
    operation.clearFault = true;
    operation.disableOutput = true;

    if (!queuePhysicalOperation(operation)) {
      return buildFunctionalError(CMD_FAULT_RESET, channel, GSA_ERROR_OPERATION_NOT_ALLOWED, txOut, txLenOut);
    }
  } else {
    state.faultLatched = false;
  }

  // O retorno síncrono confirma o reset aceito; o resultado físico segue por 0x31.
  uint8_t payload[2] = { channel, 0 };
  txLenOut = TlvBuilder::build(CMD_FAULT_RESET, payload, sizeof(payload), txOut, TLV_MAX_LEN);
  return txLenOut != 0;
}

bool AnalogService::handleOffsetSet(const TlvFrame& tlv, uint8_t* txOut, uint8_t& txLenOut) {
  if (tlv.l != 4 || tlv.v == nullptr) {
    return buildFunctionalError(CMD_OFFSET_SET, 0, GSA_ERROR_INVALID_PAYLOAD, txOut, txLenOut);
  }

  uint8_t channel = tlv.v[0];
  uint8_t kind = tlv.v[1];
  int16_t value = readI16Le(&tlv.v[2]);
  int8_t index = channelToIndex(channel);

  if (index < 0) {
    return buildFunctionalError(CMD_OFFSET_SET, channel, GSA_ERROR_INVALID_CHANNEL, txOut, txLenOut);
  }

  if (!isValidOffsetKind(kind)) {
    return buildFunctionalError(CMD_OFFSET_SET, channel, GSA_ERROR_INVALID_KIND, txOut, txLenOut);
  }

  GsaChannelOffsets candidate = _channels[index].offsets;
  switch (kind) {
    case GSA_OFFSET_KIND_VOUT:
      candidate.vout = value;
      break;

    case GSA_OFFSET_KIND_VREAD:
      candidate.vread = value;
      break;

    case GSA_OFFSET_KIND_IREAD:
      candidate.iread = value;
      break;
  }

  uint8_t voutRaw = 0;
  uint8_t ireadRaw = 0;
  if (!evaluateTelemetry(channel, candidate, voutRaw, ireadRaw)) {
    return buildFunctionalError(CMD_OFFSET_SET, channel, GSA_ERROR_INVALID_VALUE, txOut, txLenOut);
  }

  _channels[index].offsets = candidate;
  _channels[index].voutRaw = voutRaw;
  _channels[index].ireadRaw = ireadRaw;

  uint8_t payload[4] = { channel, kind, 0, 0 };
  writeI16Le(&payload[2], value);
  txLenOut = TlvBuilder::build(CMD_OFFSET_SET, payload, sizeof(payload), txOut, TLV_MAX_LEN);
  return txLenOut != 0;
}

bool AnalogService::handleOffsetSave(const TlvFrame& tlv, uint8_t* txOut, uint8_t& txLenOut) {
  if (tlv.l != 1 || tlv.v == nullptr) {
    return buildFunctionalError(CMD_OFFSET_SAVE, 0, GSA_ERROR_INVALID_PAYLOAD, txOut, txLenOut);
  }

  uint8_t channel = tlv.v[0];
  if (channelToIndex(channel) < 0) {
    return buildFunctionalError(CMD_OFFSET_SAVE, channel, GSA_ERROR_INVALID_CHANNEL, txOut, txLenOut);
  }

  if (!persistOffsets()) {
    return buildFunctionalError(CMD_OFFSET_SAVE, channel, GSA_ERROR_EEPROM_WRITE_FAILED, txOut, txLenOut);
  }

  uint8_t payload[2] = { channel, 1 };
  txLenOut = TlvBuilder::build(CMD_OFFSET_SAVE, payload, sizeof(payload), txOut, TLV_MAX_LEN);
  return txLenOut != 0;
}

bool AnalogService::handleOffsetResetAll(const TlvFrame& tlv, uint8_t* txOut, uint8_t& txLenOut) {
  if (tlv.l != 0) {
    return buildFunctionalError(CMD_OFFSET_RESET_ALL, 0, GSA_ERROR_INVALID_PAYLOAD, txOut, txLenOut);
  }

  for (uint8_t index = 0; index < GSA_CHANNEL_COUNT; index++) {
    _channels[index].offsets = { 0, 0, 0 };
  }

  refreshAllTelemetry();

  if (!persistOffsets()) {
    return buildFunctionalError(CMD_OFFSET_RESET_ALL, 0, GSA_ERROR_EEPROM_WRITE_FAILED, txOut, txLenOut);
  }

  txLenOut = TlvBuilder::buildU8(CMD_OFFSET_RESET_ALL, 1, txOut, TLV_MAX_LEN);
  return txLenOut != 0;
}

bool AnalogService::handleStatus(const TlvFrame& tlv, uint8_t* txOut, uint8_t& txLenOut) {
  if (tlv.l != 1 || tlv.v == nullptr) {
    return buildFunctionalError(CMD_STATUS_CHANNEL, 0, GSA_ERROR_INVALID_PAYLOAD, txOut, txLenOut);
  }

  uint8_t channel = tlv.v[0];
  int8_t index = channelToIndex(channel);
  if (index < 0) {
    return buildFunctionalError(CMD_STATUS_CHANNEL, channel, GSA_ERROR_INVALID_CHANNEL, txOut, txLenOut);
  }

  refreshChannelTelemetry((uint8_t)index);
  return buildChannelStatus(channel, txOut, txLenOut);
}

bool AnalogService::buildFunctionalError(uint8_t requestType, uint8_t channel, uint8_t errorCode, uint8_t* txOut, uint8_t& txLenOut) const {
  uint8_t payload[3] = { requestType, channel, errorCode };
  txLenOut = TlvBuilder::build(CMD_FUNCTIONAL_ERROR, payload, sizeof(payload), txOut, TLV_MAX_LEN);
  return txLenOut != 0;
}

bool AnalogService::buildChannelStatus(uint8_t channel, uint8_t* txOut, uint8_t& txLenOut) const {
  const GsaChannelState& state = channelState(channel);
  uint8_t payload[6] = {
    channel,
    state.setpointRaw,
    state.voutRaw,
    state.ireadRaw,
    (uint8_t)(state.effectiveEnable ? 1 : 0),
    (uint8_t)(state.faultLatched ? 1 : 0)
  };

  txLenOut = TlvBuilder::build(CMD_STATUS_CHANNEL, payload, sizeof(payload), txOut, TLV_MAX_LEN);
  return txLenOut != 0;
}

bool AnalogService::queuePhysicalOperation(const GsaPhysicalOperation& operation) {
  return _busArbiter.queueOperation(operation);
}

bool AnalogService::queuePhysicalOperations(const GsaPhysicalOperation* operations, uint8_t operationCount) {
  if (operationCount == 0U) {
    return true;
  }

  if (!operations || !_busArbiter.canQueue(operationCount)) {
    return false;
  }

  for (uint8_t index = 0; index < operationCount; index++) {
    if (!_busArbiter.queueOperation(operations[index])) {
      return false;
    }
  }

  return true;
}

void AnalogService::processCompletedHardwareOperations() {
  GsaPhysicalOperationResult result = {};
  while (_busArbiter.popCompletedOperation(result)) {
    applyCompletedOperation(result);

    // Toda conclusão física, inclusive falha, é publicada por evento assíncrono.
    uint8_t payload[3] = {
      result.originType,
      result.channel,
      result.status
    };
    enqueueEvent(CMD_PHYSICAL_EVENT, payload, sizeof(payload));
  }
}

void AnalogService::applyCompletedOperation(const GsaPhysicalOperationResult& result) {
  if (result.channel < GSA_CHANNEL_FIRST || result.channel > GSA_CHANNEL_LAST) {
    return;
  }

  if (result.status != GSA_PHYSICAL_STATUS_OK) {
    return;
  }

  GsaChannelState& state = _channels[result.channel - 1U];
  if (result.applySetpoint) {
    state.setpointRaw = result.requestedSetpointRaw;
  }

  if (result.applyEnable) {
    state.requestedEnable = result.requestedEnable;
    state.effectiveEnable = result.requestedEnable;
  }

  if (result.clearFault) {
    state.faultLatched = false;
  }

  refreshChannelTelemetry((uint8_t)(result.channel - 1U));
}

bool AnalogService::evaluateTelemetry(uint8_t channel, const GsaChannelOffsets& offsets, uint8_t& voutRaw, uint8_t& ireadRaw) const {
  int32_t maxVoltageMv = channelMaxVoltageMv(channel);
  int32_t physicalVoutMv = simulatedPhysicalVoutMv(channel) + (int32_t)offsets.vout;
  int32_t reportedVoutMv = physicalVoutMv + (int32_t)offsets.vread;
  int32_t reportedIreadMa = simulatedPhysicalIreadMa(channel) + (int32_t)offsets.iread;

  if (physicalVoutMv < 0 || physicalVoutMv > maxVoltageMv) {
    return false;
  }

  if (reportedVoutMv < 0 || reportedVoutMv > maxVoltageMv) {
    return false;
  }

  if (reportedIreadMa < 0 || reportedIreadMa > GSA_IREAD_MAX_MA) {
    return false;
  }

  voutRaw = scaleToRaw(reportedVoutMv, maxVoltageMv);
  ireadRaw = scaleToRaw(reportedIreadMa, GSA_IREAD_MAX_MA);
  return true;
}

bool AnalogService::persistOffsets() {
  GsaChannelOffsets offsets[GSA_CHANNEL_COUNT];
  snapshotOffsets(offsets);
  return _eeprom.saveOffsets(offsets, GSA_CHANNEL_COUNT);
}

void AnalogService::snapshotOffsets(GsaChannelOffsets* outOffsets) const {
  if (!outOffsets) {
    return;
  }

  for (uint8_t index = 0; index < GSA_CHANNEL_COUNT; index++) {
    outOffsets[index] = _channels[index].offsets;
  }
}

void AnalogService::refreshAllTelemetry() {
  for (uint8_t index = 0; index < GSA_CHANNEL_COUNT; index++) {
    refreshChannelTelemetry(index);
  }
}

void AnalogService::refreshChannelTelemetry(uint8_t channelIndex) {
  if (channelIndex >= GSA_CHANNEL_COUNT) {
    return;
  }

  uint8_t channel = (uint8_t)(channelIndex + 1);
  uint8_t voutRaw = 0;
  uint8_t ireadRaw = 0;
  if (!evaluateTelemetry(channel, _channels[channelIndex].offsets, voutRaw, ireadRaw)) {
    _channels[channelIndex].voutRaw = 0;
    _channels[channelIndex].ireadRaw = 0;
    return;
  }

  _channels[channelIndex].voutRaw = voutRaw;
  _channels[channelIndex].ireadRaw = ireadRaw;
}

void AnalogService::setChannelFaultLatched(uint8_t channelIndex, bool faultLatched) {
  if (channelIndex >= GSA_CHANNEL_COUNT) {
    return;
  }

  bool previousState = _channels[channelIndex].faultLatched;
  _channels[channelIndex].faultLatched = faultLatched;

  if (faultLatched) {
    _channels[channelIndex].requestedEnable = false;
    _channels[channelIndex].effectiveEnable = false;
  }

  if (!previousState && faultLatched) {
    queueFaultEvent(channelIndex);
  }
}

void AnalogService::queueFaultEvent(uint8_t channelIndex) {
  if (channelIndex >= GSA_CHANNEL_COUNT) {
    return;
  }

  const GsaChannelState& state = _channels[channelIndex];
  uint8_t payload[6] = {
    (uint8_t)(channelIndex + 1U),
    state.setpointRaw,
    state.voutRaw,
    state.ireadRaw,
    (uint8_t)(state.effectiveEnable ? 1 : 0),
    (uint8_t)(state.faultLatched ? 1 : 0)
  };

  enqueueEvent(CMD_FAULT_EVENT, payload, sizeof(payload));
}

bool AnalogService::enqueueEvent(uint8_t type, const uint8_t* payload, uint8_t payloadLen) {
  if (!payload || payloadLen > sizeof(_events[0].payload) || _eventCount >= GSA_EVENT_QUEUE_SIZE) {
    return false;
  }

  GsaEvent& event = _events[_eventTail];
  event.type = type;
  event.payloadLen = payloadLen;
  for (uint8_t index = 0; index < payloadLen; index++) {
    event.payload[index] = payload[index];
  }

  _eventTail = (uint8_t)((_eventTail + 1U) % GSA_EVENT_QUEUE_SIZE);
  _eventCount++;
  return true;
}

int8_t AnalogService::channelToIndex(uint8_t channel) const {
  if (channel < GSA_CHANNEL_FIRST || channel > GSA_CHANNEL_LAST) {
    return -1;
  }

  return (int8_t)(channel - 1);
}

bool AnalogService::isValidState(uint8_t state) const {
  return state == 0U || state == 1U;
}

bool AnalogService::isValidOffsetKind(uint8_t kind) const {
  return kind == GSA_OFFSET_KIND_VOUT ||
         kind == GSA_OFFSET_KIND_VREAD ||
         kind == GSA_OFFSET_KIND_IREAD;
}

GsaChannelState& AnalogService::channelState(uint8_t channel) {
  return _channels[channel - 1U];
}

const GsaChannelState& AnalogService::channelState(uint8_t channel) const {
  return _channels[channel - 1U];
}
