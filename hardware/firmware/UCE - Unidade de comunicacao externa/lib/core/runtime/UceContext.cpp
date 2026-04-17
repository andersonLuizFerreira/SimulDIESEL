#include "core/runtime/UceContext.h"

UceStatus& UceContext::status() {
  return _status;
}

const UceStatus& UceContext::status() const {
  return _status;
}
