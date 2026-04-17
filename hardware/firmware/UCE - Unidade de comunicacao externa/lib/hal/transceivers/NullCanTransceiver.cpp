#include "hal/transceivers/NullCanTransceiver.h"

bool NullCanTransceiver::enable() {
  return true;
}

bool NullCanTransceiver::disable() {
  return true;
}

bool NullCanTransceiver::standby() {
  return true;
}

bool NullCanTransceiver::wake() {
  return true;
}

bool NullCanTransceiver::hasFault() const {
  return false;
}

bool NullCanTransceiver::isPresent() const {
  return false;
}
