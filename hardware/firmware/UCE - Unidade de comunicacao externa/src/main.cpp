#include <Arduino.h>
#include "core/transport/Transport.h"

namespace {
Transport g_transport;
}

void setup() {
  g_transport.begin();
}

void loop() {
  delay(1);
}
