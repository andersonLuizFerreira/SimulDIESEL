#include <Arduino.h>

#include "Sggw.defs.h"
#include "SggwTransport.h"
#include <SggwLink.h>

// GatewayCore
#include "IGatewayApp.h"
#include "GatewayApp.h"
#include "GwI2cBus.h"
#include "GwSpiBus.h"
#include "GwRouter.h"

static SggwTransport transport(Serial);
static SggwLink sggwLink(transport);

// Buses
static GwI2cBus i2cBus(Wire);
static GwSpiBus spiBus(SPI);

// Router
static GwRouter router(i2cBus, spiBus);

// App
static GatewayApp app(sggwLink, router);

void setup() {
  pinMode(LED_BUILTIN, OUTPUT);
  digitalWrite(LED_BUILTIN, LOW);

  transport.begin();
  transport.setTextEnabled(true);

  i2cBus.begin(400000);
  spiBus.begin(8000000);

  sggwLink.attachApp(&app);
  sggwLink.begin();
}

void loop() {
  sggwLink.poll();
}
