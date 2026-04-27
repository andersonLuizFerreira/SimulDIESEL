#pragma once

#include <stdint.h>

class LedService {
public:
  void begin();
  bool set(bool on);
  bool state() const;
  bool handleTlv(uint8_t type,
                 const uint8_t* value,
                 uint8_t valueLen,
                 uint8_t* responseValue,
                 uint8_t& responseValueLen,
                 uint8_t& errorCode,
                 uint8_t* eventValue,
                 uint8_t& eventValueLen);

private:
  bool _state = false;
  uint16_t _eventCounter = 0;
};
