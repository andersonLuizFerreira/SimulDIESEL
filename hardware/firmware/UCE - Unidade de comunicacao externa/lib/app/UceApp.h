#pragma once

#include "core/runtime/UceContext.h"
#include "core/link/Link.h"
#include "core/service/Service.h"
#include "core/transport/Transport.h"
#include "drivers/can/Sam3xCanDriver.h"
#include "hal/transceivers/NullCanTransceiver.h"
#include "services/can/CanService.h"
#include "services/led/LedService.h"

class UceApp {
public:
  UceApp();

  void begin();
  void tick();
  void poll();

  UceContext& context();
  const UceContext& context() const;

private:
  Transport _transport;
  LedService _led;
  NullCanTransceiver _canTransceiver;
  Sam3xCanDriver _canDriver;
  CanService _canService;
  Service _service;
  Link _link;
  UceContext _context;
};
