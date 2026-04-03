#include <Arduino.h>
#include <SoftwareWire.h>
#include "defs.h"
#include "config.h"

#include "AnalogService.h"
#include "BusArbiterService.h"
#include "EepromService.h"
#include "Mcp4725Service.h"
#include "Tca9548Service.h"
#include "Transport.h"
#include "LedService.h"
#include "Service.h"
#include "Link.h"

static Transport     g_transport;
static LedService    g_led;
static EepromService g_eeprom;
static SoftwareWire  g_logicalI2c(GSA_LOGICAL_I2C_SDA_PIN, GSA_LOGICAL_I2C_SCL_PIN, false);
static Tca9548Service g_tca9548(g_logicalI2c);
static Mcp4725Service g_mcp4725(g_logicalI2c);
static BusArbiterService g_busArbiter(g_logicalI2c, g_tca9548, g_mcp4725);
static AnalogService g_analog(g_eeprom, g_busArbiter);
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
  g_link.tick();
  g_link.poll();
}
