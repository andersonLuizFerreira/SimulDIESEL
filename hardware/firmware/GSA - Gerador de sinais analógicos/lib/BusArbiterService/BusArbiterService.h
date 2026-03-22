#pragma once

#include <stdint.h>

#include "..\Mcp4725Service\Mcp4725Service.h"
#include "..\Tca9548Service\Tca9548Service.h"

enum class GsaBusState
{
  IdleSlave,
  BusyMaster
};

class BusArbiterService {
public:
  BusArbiterService(Tca9548Service& tca9548, Mcp4725Service& mcp4725);

  void begin();
  void tick();

  bool queueSetpoint(uint8_t channel, uint8_t setpointRaw, uint16_t millivolts);
  bool queueDisable(uint8_t channel);
  bool queueBatch(const uint8_t* channels, const uint8_t* setpointsRaw, uint8_t count, uint8_t eventChannel);

  bool popPendingEvent(uint8_t* payloadOut, uint8_t& payloadLenOut);
  bool isBusy() const;
  GsaBusState state() const;
  bool executeSetpoint(uint8_t channel, uint16_t millivolts);

private:
  void beginBusy(uint8_t channel);
  void finishBusy(uint8_t channel);
  void recoverToSlave(uint8_t channel);
  bool hasTimedOut() const;

private:
  Tca9548Service& _tca9548;
  Mcp4725Service& _mcp4725;

  GsaBusState _state;
  uint32_t _busySinceMs;

  bool _taskPending;
  uint8_t _taskCount;
  uint8_t _taskEventChannel;
  uint8_t _taskChannels[16];
  uint8_t _taskSetpointsRaw[16];
  uint16_t _taskMillivolts[16];

  bool _eventPending;
  uint8_t _eventPayload[3];
  uint8_t _eventPayloadLen;
};
