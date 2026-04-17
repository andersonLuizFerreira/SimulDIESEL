#include <Arduino.h>
#include "app/UceApp.h"
#include "diag/trace/DiagTrace.h"

static UceApp g_app;

void setup() {
  g_app.begin();
}

void loop() {
  g_app.tick();
  g_app.poll();
  DiagTrace::flush();
}
