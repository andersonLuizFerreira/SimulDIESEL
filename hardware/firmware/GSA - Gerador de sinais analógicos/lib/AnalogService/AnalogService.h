#pragma once

#include <stdint.h>

#include "..\BusArbiterService\BusArbiterService.h"
#include "..\EepromService\EepromService.h"
#include "Tlv.h"
#include "defs.h"

struct GsaEvent {
  uint8_t type = 0;
  uint8_t payloadLen = 0;
  uint8_t payload[6] = { 0 };
  bool pending = false;
};

struct GsaChannelState {
  uint8_t setpointRaw = 0;
  bool requestedEnable = false;
  bool effectiveEnable = false;
  bool faultLatched = false;
  uint8_t voutRaw = 0;
  uint8_t ireadRaw = 0;
  GsaChannelOffsets offsets = { 0, 0, 0 };
};

class AnalogService {
public:
  AnalogService(EepromService& eeprom, BusArbiterService& busArbiter);

  void begin();
  void tick();
  bool handleTlv(const TlvFrame& tlv, uint8_t* txOut, uint8_t& txLenOut);
  bool popPendingEvent(uint8_t* txOut, uint8_t& txLenOut);

private:
  bool handleSetpoint(const TlvFrame& tlv, uint8_t* txOut, uint8_t& txLenOut);
  bool handleEnableChannel(const TlvFrame& tlv, uint8_t* txOut, uint8_t& txLenOut);
  bool handleEnableGlobal(const TlvFrame& tlv, uint8_t* txOut, uint8_t& txLenOut);
  bool handleFaultReset(const TlvFrame& tlv, uint8_t* txOut, uint8_t& txLenOut);
  bool handleOffsetSet(const TlvFrame& tlv, uint8_t* txOut, uint8_t& txLenOut);
  bool handleOffsetSave(const TlvFrame& tlv, uint8_t* txOut, uint8_t& txLenOut);
  bool handleOffsetResetAll(const TlvFrame& tlv, uint8_t* txOut, uint8_t& txLenOut);
  bool handleStatus(const TlvFrame& tlv, uint8_t* txOut, uint8_t& txLenOut);
  bool buildBusEventResponse(uint8_t eventType, uint8_t channel, uint8_t state, uint8_t* txOut, uint8_t& txLenOut) const;

  bool buildFunctionalError(uint8_t requestType, uint8_t channel, uint8_t errorCode, uint8_t* txOut, uint8_t& txLenOut) const;
  bool buildChannelStatus(uint8_t channel, uint8_t* txOut, uint8_t& txLenOut) const;
  bool queueHardwareWriteForChannel(uint8_t channel, uint8_t setpointRaw);
  bool queueHardwareDisableForChannel(uint8_t channel);
  bool queueHardwareBatch(const uint8_t* channels, const uint8_t* setpointsRaw, uint8_t count, uint8_t eventChannel);
  bool evaluateTelemetry(uint8_t channel, const GsaChannelOffsets& offsets, uint8_t& voutRaw, uint8_t& ireadRaw) const;
  bool persistOffsets();
  void snapshotOffsets(GsaChannelOffsets* outOffsets) const;
  void refreshAllTelemetry();
  void refreshChannelTelemetry(uint8_t channelIndex);
  void setChannelFaultLatched(uint8_t channelIndex, bool faultLatched);
  void queueFaultEvent(uint8_t channelIndex);

  int8_t channelToIndex(uint8_t channel) const;
  bool isValidState(uint8_t state) const;
  bool isValidOffsetKind(uint8_t kind) const;
  GsaChannelState& channelState(uint8_t channel);
  const GsaChannelState& channelState(uint8_t channel) const;

private:
  EepromService& _eeprom;
  BusArbiterService& _busArbiter;
  GsaChannelState _channels[16];
  GsaEvent _pendingEvent;
};
