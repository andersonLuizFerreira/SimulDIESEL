#include <Arduino.h>

#include "Sggw.defs.h"
#include "SggwTransport.h"
#include "SggwLink.h"
#include "SggwDevice.h"

static SggwTransport transport(Serial);
static SggwLink sggwLink(transport);
static SggwDevice device(sggwLink);

void setup() {
  pinMode(LED_BUILTIN, OUTPUT);
  digitalWrite(LED_BUILTIN, LOW); // opcional: estado inicial conhecido

  transport.begin();

  // opcional: garante que texto está ON antes do handshake
  transport.setTextEnabled(true);

  sggwLink.attachDevice(&device);
  sggwLink.begin();
}

void loop() {
  sggwLink.poll();
}
