#include <Arduino.h>
#include "defs.h"

#include "Transport.h"
#include "LedService.h"
#include "Service.h"
#include "Link.h"

static Transport  g_transport;
static LedService g_led;
static Service    g_service(g_led);
static Link       g_link(g_transport, g_service);

void setup() {
  g_led.begin();
  g_transport.begin(I2C_GSA_ADDR);
  g_link.begin();
}

void loop() {
  g_link.poll();
}