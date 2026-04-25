#pragma once

#include <stdint.h>

class CanService {
public:
  void begin();

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

  PortState _ports[ControllerCount];

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
};
