#include <Arduino.h>

#include "src/Sggw/Sggw.defs.h"
#include "src/Sggw/SggwTransport.h"
#include "src/Sggw/SggwLink.h"

// GatewayCore
#include "src/GatewayCore/IGatewayApp.h"
#include "src/GatewayCore/GatewayApp.h"
#include "src/GatewayCore/GwI2cBus.h"
#include "src/GatewayCore/GwSpiBus.h"
#include "src/GatewayCore/GwRouter.h"

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
