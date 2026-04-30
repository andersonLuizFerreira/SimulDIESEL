#include <Arduino.h>
#include "core/link/SpiLink.h"
#include "core/services/UceServiceDispatcher.h"
#include "core/transport/UceTransport.h"
#include "services/can/service/CanService.h"
#include "services/led/LedService.h"

namespace {
SpiLink g_spiLink;
LedService g_ledService;
CanService g_canService;
UceServiceDispatcher g_dispatcher(g_ledService, g_canService);
UceTransport g_transport(g_spiLink, g_dispatcher);

bool publishUceEvent(void* context, uint8_t type, const uint8_t* value, uint8_t valueLen) {
  return static_cast<UceTransport*>(context)->publishEvent(type, value, valueLen);
}
}

void setup() {
  g_spiLink.begin();
  g_transport.begin();
  g_canService.setEventPublisher(publishUceEvent, &g_transport);
}

void loop() {
  g_transport.poll();
  g_dispatcher.loop();
}
