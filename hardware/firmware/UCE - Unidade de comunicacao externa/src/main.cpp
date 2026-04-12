#include <Arduino.h>
#include "Transport.h"
#include "DiagTrace.h"
#include "LedService.h"
#include "Service.h"
#include "Link.h"

static Transport g_transport;
static LedService g_led;
static Service g_service(g_led);
static Link g_link(g_transport, g_service);

void setup() {
  DiagTrace::begin();
  g_led.begin();
  g_service.begin();
  g_transport.begin();
  g_link.begin();
}

void loop() {
  g_link.tick();
  g_link.poll();
  DiagTrace::flush();
}
