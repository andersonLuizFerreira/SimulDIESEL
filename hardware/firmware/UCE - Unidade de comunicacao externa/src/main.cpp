#include <Arduino.h>
#include "core/link/SpiLink.h"
#include "core/services/UceServiceDispatcher.h"
#include "core/transport/UceTransport.h"
#include "services/can/sdctp/SdctpService.h"
#include "services/led/LedService.h"

namespace {
SpiLink g_spiLink;
LedService g_ledService;
SdctpService g_sdctpService;
UceServiceDispatcher g_dispatcher(g_ledService, g_sdctpService);
UceTransport g_transport(g_spiLink, g_dispatcher);
}

void setup() {
  g_spiLink.begin();
  g_transport.begin();
}

void loop() {
  g_transport.poll();
  g_dispatcher.loop();
}
