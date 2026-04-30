#pragma once

#ifdef USE_CAN_DRIVER_FAKE
#include "services/can/driver/CanDriver_fake.h"
using CanDriverSelected = CanDriverFake;
#else
#include "services/can/driver/CanDriver.h"
using CanDriverSelected = CanDriver;
#endif
