#include "BusArbiterService.h"

#include "config.h"
#include "defs.h"

namespace {
uint16_t channelFullScaleMillivolts(uint8_t channel) {
  return (channel <= 8U) ? GSA_LOW_RANGE_MAX_MV : GSA_HIGH_RANGE_MAX_MV;
}
}

BusArbiterService::BusArbiterService(SoftwareWire& logicalBus, Tca9548Service& tca9548, Mcp4725Service& mcp4725)
  : _logicalBus(logicalBus),
    _tca9548(tca9548),
    _mcp4725(mcp4725),
    _pendingHead(0),
    _pendingTail(0),
    _pendingCount(0),
    _completedHead(0),
    _completedTail(0),
    _completedCount(0)
{
}

void BusArbiterService::begin() {
  _pendingHead = 0;
  _pendingTail = 0;
  _pendingCount = 0;
  _completedHead = 0;
  _completedTail = 0;
  _completedCount = 0;

  _tca9548.begin();
  _logicalBus.setTimeout(20U);
  _logicalBus.setClock(100000UL);
  _logicalBus.begin();
}

void BusArbiterService::tick() {
  if (_pendingCount == 0U) {
    return;
  }

  GsaPhysicalOperation operation = _pendingOperations[_pendingHead];
  _pendingHead = (uint8_t)((_pendingHead + 1U) % GSA_PHYSICAL_OP_QUEUE_SIZE);
  _pendingCount--;

  pushCompletedResult(operation, executeOperation(operation));
}

bool BusArbiterService::canQueue(uint8_t operationCount) const {
  return operationCount <= (uint8_t)(GSA_PHYSICAL_OP_QUEUE_SIZE - _pendingCount);
}

bool BusArbiterService::queueOperation(const GsaPhysicalOperation& operation) {
  if (!canQueue(1U)) {
    return false;
  }

  _pendingOperations[_pendingTail] = operation;
  _pendingTail = (uint8_t)((_pendingTail + 1U) % GSA_PHYSICAL_OP_QUEUE_SIZE);
  _pendingCount++;
  return true;
}

bool BusArbiterService::popCompletedOperation(GsaPhysicalOperationResult& resultOut) {
  if (_completedCount == 0U) {
    return false;
  }

  resultOut = _completedOperations[_completedHead];
  _completedHead = (uint8_t)((_completedHead + 1U) % GSA_PHYSICAL_OP_QUEUE_SIZE);
  _completedCount--;
  return true;
}

uint8_t BusArbiterService::executeOperation(const GsaPhysicalOperation& operation) {
  uint8_t ackCode = 0xFF;
  if (!_tca9548.selectChannel(operation.channel, &ackCode)) {
    return GSA_PHYSICAL_STATUS_TCA_NO_ACK;
  }

  if (!_mcp4725.probeChannel(operation.channel, &ackCode)) {
    return GSA_PHYSICAL_STATUS_MCP_NO_ACK;
  }

  bool ok = operation.disableOutput
    ? _mcp4725.disableChannel(operation.channel, &ackCode)
    : _mcp4725.writeChannel(operation.channel, rawToOutputMillivolts(operation.channel, operation.requestedSetpointRaw), &ackCode);

  if (!ok) {
    return GSA_PHYSICAL_STATUS_MCP_NO_ACK;
  }

  return GSA_PHYSICAL_STATUS_OK;
}

uint16_t BusArbiterService::rawToOutputMillivolts(uint8_t channel, uint8_t setpointRaw) const {
  uint32_t scaled = ((uint32_t)setpointRaw * (uint32_t)channelFullScaleMillivolts(channel)) + 127U;
  return (uint16_t)(scaled / 255U);
}

void BusArbiterService::pushCompletedResult(const GsaPhysicalOperation& operation, uint8_t status) {
  if (_completedCount >= GSA_PHYSICAL_OP_QUEUE_SIZE) {
    return;
  }

  GsaPhysicalOperationResult& result = _completedOperations[_completedTail];
  result.originType = operation.originType;
  result.channel = operation.channel;
  result.status = status;
  result.requestedSetpointRaw = operation.requestedSetpointRaw;
  result.applySetpoint = operation.applySetpoint;
  result.requestedEnable = operation.requestedEnable;
  result.applyEnable = operation.applyEnable;
  result.clearFault = operation.clearFault;

  _completedTail = (uint8_t)((_completedTail + 1U) % GSA_PHYSICAL_OP_QUEUE_SIZE);
  _completedCount++;
}
