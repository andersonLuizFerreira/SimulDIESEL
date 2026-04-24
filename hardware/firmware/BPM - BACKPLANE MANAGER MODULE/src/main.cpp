#include <Arduino.h>

#include "SdgwDefs.h"
#include "SdgwTransport.h"
#include "SdgwBluetoothEndpoint.h"
#include "SdgwEndpointMux.h"
#include "SdgwSessionOwner.h"
#include <SdgwLink.h>

// GatewayCore
#include "IGatewayApp.h"
#include "GatewayApp.h"
#include "GwI2cBus.h"
#include "GwSpiBus.h"
#include "GwRouter.h"

static SdgwTransport serialTransport(Serial);
static SdgwBluetoothEndpoint bluetoothTransport("SimulDIESEL - BPM");
static SdgwSessionOwner sessionOwner(SDGW_ENDPOINT_NONE);
static SdgwEndpointMux transportMux(sessionOwner, serialTransport, bluetoothTransport);
static SdgwLink sdgwLink(transportMux, sessionOwner);

// Buses
static GwI2cBus i2cBus(Wire);
static GwSpiBus spiBus(SPI);

// Router
static GwRouter router(i2cBus, spiBus);

// App
static GatewayApp app(sdgwLink, router);

void setup() {
  // Reserva o GPIO23 exclusivamente para o reset global.
  digitalWrite(BPM_GLOBAL_RESET_PIN, BPM_GLOBAL_RESET_INACTIVE_LEVEL);
  pinMode(BPM_GLOBAL_RESET_PIN, OUTPUT);

  serialTransport.begin();
  serialTransport.setTextEnabled(true);
  bluetoothTransport.begin();
  bluetoothTransport.setTextEnabled(true);

  i2cBus.begin(BPM_GSA_I2C_SDA_PIN, BPM_GSA_I2C_SCL_PIN, 400000);
  spiBus.begin(1000000UL,
               BPM_SPI_SCK_PIN,
               BPM_SPI_MISO_PIN,
               BPM_SPI_MOSI_PIN);

  sdgwLink.attachApp(&app);
  app.begin();
  sdgwLink.begin();
}

void loop() {
  sdgwLink.poll();
  app.tick();
}
