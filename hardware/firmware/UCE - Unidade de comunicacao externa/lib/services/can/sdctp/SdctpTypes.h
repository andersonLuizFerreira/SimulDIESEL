#pragma once

#include "defs.h"
#include "services/can/rxhub/CanRxHub.h"

// SDCTP command IDs are defined in defs.h and mirrored by the API GwProtocol.
// RX: 0x28, 0x40..0x46. TX: 0x50..0x53. Diagnostics: 0x7E.
namespace SdctpTypes {
const uint8_t RxModeAuto = CAN_RX_MODE_AUTO;
const uint8_t RxModeDirectOnly = CAN_RX_MODE_DIRECT_ONLY;
}
