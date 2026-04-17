#pragma once

#include "core/runtime/UceStatus.h"

class UceContext {
public:
  UceStatus& status();
  const UceStatus& status() const;

private:
  UceStatus _status;
};
