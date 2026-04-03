#include <Arduino.h>

#include "Sggw.defs.h"
#include "SggwTransport.h"
#include "SggwBluetoothEndpoint.h"
#include "SggwEndpointMux.h"
#include "SggwSessionOwner.h"
#include <SggwLink.h>

// GatewayCore
#include "IGatewayApp.h"
#include "GatewayApp.h"
#include "GwI2cBus.h"
#include "GwSpiBus.h"
#include "GwRouter.h"

static SggwTransport serialTransport(Serial);
static SggwBluetoothEndpoint bluetoothTransport("SimulDIESEL-BPM");
static SggwSessionOwner sessionOwner(SGGW_ENDPOINT_NONE);
static SggwEndpointMux transportMux(sessionOwner, serialTransport, bluetoothTransport);
static SggwLink sggwLink(transportMux, sessionOwner);

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
  pinMode(GSA_RESET_PIN, OUTPUT);
  digitalWrite(GSA_RESET_PIN, HIGH);

  serialTransport.begin();
  serialTransport.setTextEnabled(true);
  bluetoothTransport.begin();
  bluetoothTransport.setTextEnabled(true);

  i2cBus.begin(BPM_GSA_I2C_SDA_PIN, BPM_GSA_I2C_SCL_PIN, 400000);
  spiBus.begin(8000000);

  sggwLink.attachApp(&app);
  app.begin();
  sggwLink.begin();
}

void loop() {
  sggwLink.poll();
  app.tick();
}
