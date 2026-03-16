#include <Wire.h>

const uint8_t SLAVE_ADDR = 0x10;
const int SDA_PIN = 21;
const int SCL_PIN = 22;
const unsigned long BLINK_INTERVAL_MS = 500;

#define LED_BUILTIN 2
bool ledOn = false;
unsigned long lastToggleMs = 0;

void setup() {
  pinMode(LED_BUILTIN, OUTPUT);
  digitalWrite(LED_BUILTIN, LOW);

  Serial.begin(115200);
  Wire.begin(SDA_PIN, SCL_PIN, 100000);

  Serial.println("ESP32 I2C master blink test");
}

void loop() {
  unsigned long now = millis();
  if (now - lastToggleMs < BLINK_INTERVAL_MS) {
    return;
  }

  lastToggleMs = now;
  ledOn = !ledOn;

  digitalWrite(LED_BUILTIN, ledOn ? HIGH : LOW);

  Wire.beginTransmission(SLAVE_ADDR);
  Wire.write(ledOn ? 0x01 : 0x00);
  uint8_t error = Wire.endTransmission(true);

  if (error == 0) {
    Serial.println(ledOn ? "LED ON sent" : "LED OFF sent");
  } else {
    Serial.print("I2C error code = ");
    Serial.println(error);
  }
}
