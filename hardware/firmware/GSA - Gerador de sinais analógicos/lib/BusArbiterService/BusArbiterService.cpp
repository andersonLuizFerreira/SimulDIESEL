#include "BusArbiterService.h"

#include <Arduino.h>
#include <Wire.h>

#include "config.h"
#include "defs.h"
#include "Transport.h"

namespace {
uint16_t channelFullScaleMv(uint8_t channel) {
  return (channel <= 8) ? GSA_LOW_RANGE_MAX_MV : GSA_HIGH_RANGE_MAX_MV;
}
}

BusArbiterService::BusArbiterService(Tca9548Service& tca9548, Mcp4725Service& mcp4725)
  : _tca9548(tca9548),
    _mcp4725(mcp4725),
    _state(GsaBusState::IdleSlave),
    _busySinceMs(0),
    _taskPending(false),
    _taskCount(0),
    _taskEventChannel(0),
    _eventPending(false),
    _eventPayloadLen(0)
{
}

void BusArbiterService::begin() {
  _state = GsaBusState::IdleSlave;
  _busySinceMs = 0;
  _taskPending = false;
  _taskCount = 0;
  _taskEventChannel = 0;
  _eventPending = false;
  _eventPayloadLen = 0;
}

void BusArbiterService::tick() {
  if (!_taskPending) {
    return;
  }

  if (Transport::hasTxPending()) {
    if (hasTimedOut()) {
      recoverToSlave(_taskEventChannel);
    }
    return;
  }

  if (hasTimedOut()) {
    recoverToSlave(_taskEventChannel);
    return;
  }

  delay(GSA_BUS_SWITCH_DELAY_MS);
  Wire.end();
  delay(GSA_BUS_SWITCH_DELAY_MS);
  Wire.begin();

  bool ok = true;
  for (uint8_t index = 0; index < _taskCount; index++) {
    uint8_t channel = _taskChannels[index];
    ok = ok && _tca9548.selectChannel(channel);
    if (!ok) {
      break;
    }

    if (_taskSetpointsRaw[index] == 0) {
      ok = ok && _mcp4725.disableChannel(channel);
    } else {
      ok = ok && executeSetpoint(channel, _taskMillivolts[index]);
    }

    if (!ok) {
      break;
    }
  }

  (void)ok;
  Wire.end();
  delay(GSA_BUS_SWITCH_DELAY_MS);
  Transport::resumeSlave(I2C_GSA_ADDR);

  finishBusy(_taskEventChannel);
}

bool BusArbiterService::queueSetpoint(uint8_t channel, uint8_t setpointRaw, uint16_t millivolts) {
  return queueBatch(&channel, &setpointRaw, 1, channel) &&
         ((_taskMillivolts[0] = millivolts), true);
}

bool BusArbiterService::queueDisable(uint8_t channel) {
  uint8_t setpointRaw = 0;
  return queueBatch(&channel, &setpointRaw, 1, channel);
}

bool BusArbiterService::queueBatch(const uint8_t* channels, const uint8_t* setpointsRaw, uint8_t count, uint8_t eventChannel) {
  if (_state != GsaBusState::IdleSlave || _taskPending || !channels || !setpointsRaw || count == 0 || count > 16) {
    return false;
  }

  for (uint8_t index = 0; index < count; index++) {
    _taskChannels[index] = channels[index];
    _taskSetpointsRaw[index] = setpointsRaw[index];
    _taskMillivolts[index] = (uint16_t)(((uint32_t)setpointsRaw[index] * channelFullScaleMv(channels[index]) + 127U) / 255U);
  }

  _taskCount = count;
  _taskEventChannel = eventChannel;
  _taskPending = true;
  beginBusy(eventChannel);
  return true;
}

bool BusArbiterService::popPendingEvent(uint8_t* payloadOut, uint8_t& payloadLenOut) {
  if (!_eventPending || !payloadOut) {
    payloadLenOut = 0;
    return false;
  }

  for (uint8_t index = 0; index < _eventPayloadLen; index++) {
    payloadOut[index] = _eventPayload[index];
  }

  payloadLenOut = _eventPayloadLen;
  _eventPending = false;
  _eventPayloadLen = 0;
  return true;
}

bool BusArbiterService::isBusy() const {
  return _state == GsaBusState::BusyMaster;
}

GsaBusState BusArbiterService::state() const {
  return _state;
}

bool BusArbiterService::executeSetpoint(uint8_t channel, uint16_t millivolts) {
  (void)millivolts;
  for (uint8_t index = 0; index < _taskCount; index++) {
    if (_taskChannels[index] == channel) {
      return _mcp4725.writeChannel(channel, _taskSetpointsRaw[index]);
    }
  }

  return false;
}

void BusArbiterService::beginBusy(uint8_t channel) {
  _state = GsaBusState::BusyMaster;
  _busySinceMs = millis();
  _eventPending = false;
  _eventPayloadLen = 0;

  _eventPayload[0] = GSA_EVENT_BUSY;
  _eventPayload[1] = channel;
  _eventPayload[2] = GSA_EVENT_STATE_BUSY;
}

void BusArbiterService::finishBusy(uint8_t channel) {
  _taskPending = false;
  _taskCount = 0;
  _taskEventChannel = 0;
  _state = GsaBusState::IdleSlave;
  _busySinceMs = 0;

  _eventPayload[0] = GSA_EVENT_IDLE;
  _eventPayload[1] = channel;
  _eventPayload[2] = GSA_EVENT_STATE_IDLE;
  _eventPayloadLen = 3;
  _eventPending = true;
}

void BusArbiterService::recoverToSlave(uint8_t channel) {
  Wire.end();
  delay(GSA_BUS_SWITCH_DELAY_MS);
  Transport::resumeSlave(I2C_GSA_ADDR);
  finishBusy(channel);
}

bool BusArbiterService::hasTimedOut() const {
  return _state == GsaBusState::BusyMaster &&
         (uint32_t)(millis() - _busySinceMs) > (uint32_t)GSA_BUS_BUSY_TIMEOUT_MS;
}
