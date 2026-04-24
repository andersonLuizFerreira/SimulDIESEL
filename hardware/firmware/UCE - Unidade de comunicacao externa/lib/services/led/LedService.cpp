#include <Arduino.h>

#include "defs.h"
#include "services/led/LedService.h"

void LedService::begin() {
  pinMode(LED_PIN, OUTPUT);
  set(false);
}

bool LedService::set(bool on) {
  _state = on;
  digitalWrite(LED_PIN, on ? HIGH : LOW);
  return _state;
}

bool LedService::state() const {
  return _state;
}
