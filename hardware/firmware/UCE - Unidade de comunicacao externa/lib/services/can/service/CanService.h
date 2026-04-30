#pragma once

#include <stdint.h>

#include "services/can/driver/CanDriver.h"

class CanService {
public:
  typedef bool (*EventPublisher)(void* context, uint8_t type, const uint8_t* value, uint8_t valueLen);

  void begin();
  void loop();
  void setEventPublisher(EventPublisher publisher, void* context);

  bool handleTlv(uint8_t type,
                 const uint8_t* value,
                 uint8_t valueLen,
                 uint8_t* responseValue,
                 uint8_t& responseValueLen,
                 uint8_t& errorCode);

private:
  struct PortState {
    uint8_t bitrateCode;
    uint8_t modeCode;
    uint8_t interfaceState;
  };

  static const uint8_t ControllerCount = 2;
  static const uint8_t RxQueueCapacity = 8;

  CanDriver _driver;
  PortState _ports[ControllerCount];
  struct QueuedRxFrame {
    uint8_t controller;
    CanDriver::Frame frame;
  };

  QueuedRxFrame _rxQueue[RxQueueCapacity];
  uint8_t _rxHead = 0;
  uint8_t _rxCount = 0;
  uint16_t _rxDropped = 0;
  EventPublisher _eventPublisher = nullptr;
  void* _eventPublisherContext = nullptr;

  struct PeriodicTxSlot {
    bool active;
    uint8_t controller;
    uint32_t lastSentMs;
    uint16_t periodMs;
    CanDriver::Frame frame;
  };

  PeriodicTxSlot _periodicTx;

  void resetPort(uint8_t controller);
  bool validateController(uint8_t controller) const;
  bool validateBitrate(uint8_t bitrateCode) const;
  bool validateMode(uint8_t modeCode) const;
  bool validateEnableState(uint8_t state) const;

  bool handleConfig(const uint8_t* value,
                    uint8_t valueLen,
                    uint8_t* responseValue,
                    uint8_t& responseValueLen,
                    uint8_t& errorCode);
  bool handleEnable(const uint8_t* value,
                    uint8_t valueLen,
                    uint8_t* responseValue,
                    uint8_t& responseValueLen,
                    uint8_t& errorCode);
  bool handleStatus(const uint8_t* value,
                    uint8_t valueLen,
                    uint8_t* responseValue,
                    uint8_t& responseValueLen,
                    uint8_t& errorCode);
  bool handleReset(const uint8_t* value,
                   uint8_t valueLen,
                   uint8_t* responseValue,
                   uint8_t& responseValueLen,
                   uint8_t& errorCode);
  bool handleRxPoll(const uint8_t* value,
                    uint8_t valueLen,
                    uint8_t* responseValue,
                    uint8_t& responseValueLen,
                    uint8_t& errorCode);
  bool handleDriverLogPoll(const uint8_t* value,
                           uint8_t valueLen,
                           uint8_t* responseValue,
                           uint8_t& responseValueLen,
                           uint8_t& errorCode);
  bool handleTx(const uint8_t* value,
                uint8_t valueLen,
                uint8_t* responseValue,
                uint8_t& responseValueLen,
                uint8_t& errorCode);
  bool handleTxStop(const uint8_t* value,
                    uint8_t valueLen,
                    uint8_t* responseValue,
                    uint8_t& responseValueLen,
                    uint8_t& errorCode);
  void collectRxFrames();
  bool enqueueRxFrame(uint8_t controller, const CanDriver::Frame& frame);
  bool dequeueRxFrame(QueuedRxFrame& queuedFrame);
  bool peekRxFrame(QueuedRxFrame& queuedFrame) const;
  bool publishNextRxEvent();
  uint8_t copyQueuedRxFrames(uint8_t controller, CanDriver::Frame* frames, uint8_t maxFrames);
  void encodeRxFrameLittleEndian(uint8_t* out, const CanDriver::Frame& frame) const;
  void encodeRxFrameBigEndian(uint8_t* out, const CanDriver::Frame& frame) const;
};
