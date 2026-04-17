#pragma once

#include <stdint.h>

namespace UceCan {

enum class Controller : uint8_t {
  Can0 = 0,
  Can1 = 1
};

enum class Mode : uint8_t {
  Normal = 0,
  ListenOnly = 1
};

enum class InterfaceState : uint8_t {
  Disabled = 0,
  Configured = 1,
  Open = 2,
  Fault = 3
};

enum class MailboxDirection : uint8_t {
  Disabled = 0,
  Rx = 1,
  RxOverwrite = 2,
  Tx = 3
};

}  // namespace UceCan
