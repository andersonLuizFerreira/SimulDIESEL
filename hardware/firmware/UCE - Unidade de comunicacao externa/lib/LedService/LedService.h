#pragma once
#include <Arduino.h>
#include "defs.h"

class LedService {
public:
  void begin();
  int get() const;
  int set(int state);

private:
  int _state = 0;
};
