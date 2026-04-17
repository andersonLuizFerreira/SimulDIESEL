#pragma once

#include <string.h>

#include "defs.h"
#include "services/can/CanMailbox.h"

class CanIoBuffer {
public:
  CanIoBuffer() {
    clear();
  }

  void clear() {
    for (uint8_t index = 0; index < UCE_CAN_MAX_MAILBOXES; ++index) {
      _mailboxes[index] = CanMailbox{};
      _mailboxes[index].index = index;
    }
  }

  void storeMailbox(const CanMailbox& mailbox) {
    if (mailbox.index >= UCE_CAN_MAX_MAILBOXES) return;
    _mailboxes[mailbox.index] = mailbox;
  }

  bool loadMailbox(uint8_t index, CanMailbox& out) const {
    if (index >= UCE_CAN_MAX_MAILBOXES) return false;
    out = _mailboxes[index];
    return true;
  }

private:
  CanMailbox _mailboxes[UCE_CAN_MAX_MAILBOXES];
};
