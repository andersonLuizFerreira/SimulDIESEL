/*
  ESP32 - Handshake responder (SimulDIESEL)
  - Recebe: "\nSIMULDIESELAPI\n"
  - Responde: "SimulDIESEL ver x.y.z\n"
*/

#include <Arduino.h>

static const uint32_t BAUD = 115200;

// O que a API envia (sem os \n, porque vamos ler por linha)
static const char* API_TOKEN = "SIMULDIESELAPI";

// O que a API espera encontrar na resposta
static const char* OK_PREFIX = "SimulDIESEL ver";

// “Versão” de teste
static const char* VERSION_STR = "0.0.1-test";

String rxLine;

void setup() {
  Serial.begin(BAUD);
  delay(50);

  // Opcional: simula lixo/bootlog para testar o DRAIN de 300ms
  Serial.println("BOOT: ESP32 starting...");
  Serial.println("BOOT: initializing...");
  Serial.println("BOOT: ready.");
}

static void sendHandshakeReply() {
  // Precisa conter "SimulDIESEL ver" e terminar com \n
  Serial.print(OK_PREFIX);
  Serial.print(" ");
  Serial.print(VERSION_STR);
  Serial.print("\n");
}

void loop() {
  while (Serial.available() > 0) {
    char c = (char)Serial.read();

    // Ignora \r para lidar com CRLF
    if (c == '\r') continue;

    if (c == '\n') {
      // Linha completa recebida
      rxLine.trim(); // remove espaços extras
      if (rxLine.length() > 0) {
        // Se a linha for exatamente o token, responde
        // (Como sua API manda "\nSIMULDIESELAPI\n", vai chegar como "SIMULDIESELAPI")
        if (rxLine.equalsIgnoreCase(API_TOKEN)) {
          sendHandshakeReply();
        }
      }
      rxLine = "";
    } else {
      // Evita crescer infinito se chegar lixo sem '\n'
      if (rxLine.length() < 256) rxLine += c;
      else rxLine = ""; // limpa se estourar
    }
  }
}
