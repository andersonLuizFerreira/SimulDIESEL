#include "LedService.h"

void LedService::begin() {
  pinMode(LED_PIN, OUTPUT);
  _state = 0;
  digitalWrite(LED_PIN, LOW);
}

int LedService::get() const {
  return _state;
}

int LedService::set(int state) {
  _state = (state != 0) ? 1 : 0;
  digitalWrite(LED_PIN, _state ? HIGH : LOW);
  return _state;
}