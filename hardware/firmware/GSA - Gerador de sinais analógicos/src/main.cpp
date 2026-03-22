#include <Arduino.h>
#include "defs.h"

#include "AnalogService.h"
#include "EepromService.h"
#include "Transport.h"
#include "LedService.h"
#include "Service.h"
#include "Link.h"

static Transport     g_transport;
static LedService    g_led;
static EepromService g_eeprom;
static AnalogService g_analog(g_eeprom);
static Service       g_service(g_led, g_analog);
static Link          g_link(g_transport, g_service);

void setup() {
  g_led.begin();
  g_service.begin();
  g_transport.begin(I2C_GSA_ADDR);
  g_link.begin();
}

void loop() {
  g_service.tick();
  g_link.poll();
}
