#pragma once

#include <stdint.h>

class LedService {
public:
  void begin();
  bool set(bool on);
  bool state() const;

private:
  bool _state = false;
};
