#pragma once

#include <stdint.h>

#include <SoftwareWire.h>

#include "..\Mcp4725Service\Mcp4725Service.h"
#include "..\Tca9548Service\Tca9548Service.h"
#include "config.h"

struct GsaPhysicalOperation {
  uint8_t originType = 0;
  uint8_t channel = 0;
  uint8_t requestedSetpointRaw = 0;
  bool applySetpoint = false;
  bool requestedEnable = false;
  bool applyEnable = false;
  bool clearFault = false;
  bool disableOutput = false;
};

struct GsaPhysicalOperationResult {
  uint8_t originType = 0;
  uint8_t channel = 0;
  uint8_t status = 0;
  uint8_t requestedSetpointRaw = 0;
  bool applySetpoint = false;
  bool requestedEnable = false;
  bool applyEnable = false;
  bool clearFault = false;
};

class BusArbiterService {
public:
  BusArbiterService(SoftwareWire& logicalBus, Tca9548Service& tca9548, Mcp4725Service& mcp4725);

  void begin();
  void tick();

  bool canQueue(uint8_t operationCount) const;
  bool queueOperation(const GsaPhysicalOperation& operation);
  bool popCompletedOperation(GsaPhysicalOperationResult& resultOut);

private:
  uint8_t executeOperation(const GsaPhysicalOperation& operation);
  uint16_t rawToOutputMillivolts(uint8_t channel, uint8_t setpointRaw) const;
  void pushCompletedResult(const GsaPhysicalOperation& operation, uint8_t status);

private:
  SoftwareWire& _logicalBus;
  Tca9548Service& _tca9548;
  Mcp4725Service& _mcp4725;

  GsaPhysicalOperation _pendingOperations[GSA_PHYSICAL_OP_QUEUE_SIZE];
  uint8_t _pendingHead;
  uint8_t _pendingTail;
  uint8_t _pendingCount;

  GsaPhysicalOperationResult _completedOperations[GSA_PHYSICAL_OP_QUEUE_SIZE];
  uint8_t _completedHead;
  uint8_t _completedTail;
  uint8_t _completedCount;
};
