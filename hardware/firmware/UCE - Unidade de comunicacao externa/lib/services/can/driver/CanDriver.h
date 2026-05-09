#pragma once

#include <stdint.h>

class CanDriver {
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
  };

  static const uint8_t ControllerCount = 2;
  static const uint8_t LogCapacity = 24;
  static const uint8_t LoopbackRxCapacity = 64;

  PortState _ports[ControllerCount];
  LogEntry _logs[ControllerCount][LogCapacity];
  uint8_t _logHead[ControllerCount];
  uint8_t _logCount[ControllerCount];
  uint8_t _logSequence;
  Frame _loopbackRxQueue[ControllerCount][LoopbackRxCapacity];
  uint8_t _loopbackRxHead[ControllerCount];
  uint8_t _loopbackRxCount[ControllerCount];

  bool isValidController(uint8_t controller) const;
  bool isPhysicalCan0(uint8_t controller) const;
  bool mapBitrate(uint8_t bitrateCode, uint32_t& bitrateKbps) const;
  bool validateMode(uint8_t modeCode) const;
  bool isLoopbackMode(uint8_t controller) const;
  bool enqueueLoopbackFrame(uint8_t controller, const Frame& frame);
  bool dequeueLoopbackFrame(uint8_t controller, Frame& frame);
  void clearLoopbackQueue(uint8_t controller);
  void configureRxMailboxes();
  void configureTxMailbox();
  uint32_t encodeId(const Frame& frame) const;
  bool readRxMailbox(uint8_t mailboxIndex, bool extended, Frame& frame);
  void logEvent(uint8_t controller,
                uint8_t eventCode,
                uint8_t detail0 = 0,
                uint8_t detail1 = 0,
                uint8_t detail2 = 0);
  void resetPort(uint8_t controller);
};
