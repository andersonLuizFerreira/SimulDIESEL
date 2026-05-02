#pragma once

#include <stdint.h>

class CanDriverFake {
public:
  struct Status {
    uint8_t controller;
    uint8_t bitrateCode;
    uint8_t modeCode;
    uint8_t interfaceState;
    bool configured;
    bool open;
  };

  struct Frame {
    uint32_t id;
    bool extended;
    bool rtr;
    uint8_t dlc;
    uint8_t data[8];
  };

  struct LogEntry {
    uint8_t timestampLow;
    uint8_t eventCode;
    uint8_t interfaceState;
    uint8_t bitrateCode;
    uint8_t modeCode;
    uint8_t detail0;
    uint8_t detail1;
    uint8_t detail2;
  };

  void begin();

  bool configure(uint8_t controller, uint8_t bitrateCode, uint8_t modeCode);
  bool open(uint8_t controller);
  bool close(uint8_t controller);
  bool reset(uint8_t controller);
  bool getStatus(uint8_t controller, Status& status);
  bool pollReceived(uint8_t controller, Frame* frames, uint8_t maxFrames, uint8_t& frameCount);
  bool pollLog(uint8_t controller, LogEntry* entries, uint8_t maxEntries, uint8_t& entryCount);
  bool send(uint8_t controller, const Frame& frame);

private:
  struct PortState {
    uint8_t bitrateCode;
    uint8_t modeCode;
    uint8_t interfaceState;
    bool configured;
    bool enabled;
    uint8_t lastError;
    uint32_t openTimestampMs;
    uint32_t lastBurstTimestampMs;
    uint32_t lastSingleShotMs;
    uint32_t lastFastFrameMs;
    uint32_t lastFastOverflowLogMs;
    uint8_t dataNibble;
    uint32_t fakeIds[3];
    uint32_t fakeId4;
    uint32_t fakeFastId;
    uint8_t fakeFastData[8];
    uint8_t fakeFastByteIndex;
    bool fakeFastFirstFramePending;
  };

  struct QueuedFrame {
    uint8_t controller;
    Frame frame;
  };

  static const uint8_t ControllerCount = 2;
  static const uint8_t LogCapacity = 24;
  static const uint8_t RxCapacity = 24;
  static const uint8_t TxCapacity = 8;

  PortState _ports[ControllerCount];
  LogEntry _logs[ControllerCount][LogCapacity];
  uint8_t _logHead[ControllerCount];
  uint8_t _logCount[ControllerCount];
  uint8_t _logSequence;

  QueuedFrame _rxQueue[RxCapacity];
  uint8_t _rxHead;
  uint8_t _rxCount;

  Frame _txQueue[ControllerCount][TxCapacity];
  uint8_t _txHead[ControllerCount];
  uint8_t _txCount[ControllerCount];

  bool isValidController(uint8_t controller) const;
  bool isSupportedController(uint8_t controller) const;
  bool validateBitrate(uint8_t bitrateCode) const;
  bool validateMode(uint8_t modeCode) const;
  void resetPort(uint8_t controller);
  void logEvent(uint8_t controller,
                uint8_t eventCode,
                uint8_t detail0 = 0,
                uint8_t detail1 = 0,
                uint8_t detail2 = 0);
  void generateFakeIds(PortState& port);
  void seedEntropyOnce();
  void updatePortSimulation(uint8_t controller, PortState& port, uint32_t now);
  bool enqueueRxFrame(uint8_t controller, const Frame& frame);
  bool dequeueRxFrame(uint8_t controller, Frame& frame);
  void purgeControllerFrames(uint8_t controller);
  void buildBurstFrame(uint32_t id, uint8_t dataNibble, Frame& frame) const;
  void buildSingleShotFrame(uint32_t id, Frame& frame) const;
  void buildFastFrame(const PortState& port, Frame& frame) const;
  void resetFastData(PortState& port) const;
  void updateFastData(PortState& port) const;
  void rememberTxFrame(uint8_t controller, const Frame& frame);
};
