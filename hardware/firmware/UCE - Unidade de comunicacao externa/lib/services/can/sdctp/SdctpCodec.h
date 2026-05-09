#pragma once

#include "services/can/protocol/CanCrudProtocol.h"

// SDCTP codec facade. CanCrudProtocol holds the validated CREATE/EDIT/TIC/
// DELETE/ROW payload encoder used by the official SDCTP wire contract.
typedef CanCrudProtocol SdctpCodec;
typedef CanCrudProtocol SdctpProtocol;
