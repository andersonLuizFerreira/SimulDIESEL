#include <Wire.h>

const uint8_t SLAVE_ADDR = 0x10;
volatile int pendingValue = -1;

void onReceiveHandler(int count) {
  while (count-- > 0 && Wire.available()) {
    pendingValue = Wire.read();
  }
}

void setup() {
  pinMode(LED_BUILTIN, OUTPUT);
  digitalWrite(LED_BUILTIN, LOW);

  Serial.begin(115200);
  Wire.begin(SLAVE_ADDR);
  Wire.onReceive(onReceiveHandler);

  Serial.println("Nano I2C slave LED test");
}

void loop() {
  int value = -1;

  noInterrupts();
  if (pendingValue >= 0) {
    value = pendingValue;
    pendingValue = -1;
  }
  interrupts();

  if (value < 0) {
    return;
  }

  if (value == 0x00) {
    digitalWrite(LED_BUILTIN, LOW);
    Serial.println("RX=0");
  } else {
    digitalWrite(LED_BUILTIN, HIGH);
    Serial.println("RX=1");
  }
}
