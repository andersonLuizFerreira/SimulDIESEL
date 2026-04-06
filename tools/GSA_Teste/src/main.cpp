#include <Arduino.h>
#include <SoftwareWire.h>

#include <ctype.h>
#include <math.h>
#include <stdlib.h>
#include <string.h>

namespace {

constexpr unsigned long kSerialBaud = 115200;
constexpr size_t kLineBufferSize = 32;
constexpr uint8_t kSoftSdaPin = 2;
constexpr uint8_t kSoftSclPin = 3;
constexpr uint8_t kTca9548Address = 0x70;
constexpr uint8_t kMcp4725AddressA0Low = 0x60;
constexpr uint8_t kMcp4725AddressA0High = 0x61;
constexpr float kDacReferenceVolts = 5.0f;
constexpr float kAmplifierGain = 2.4f;
constexpr uint16_t kDacMaxCode = 4095;

struct ChannelMapping {
  uint8_t logicalChannel;
  uint8_t tcaChannel;
  uint8_t mcpAddress;
  bool usesAmplifiedOutput;
};

SoftwareWire gI2cBus(kSoftSdaPin, kSoftSclPin, false);

void trimWhitespace(char *text) {
  if (text == nullptr) {
    return;
  }

  char *start = text;
  while (*start != '\0' && isspace(static_cast<unsigned char>(*start)) != 0) {
    ++start;
  }

  char *end = start;
  while (*end != '\0') {
    ++end;
  }

  while (end > start && isspace(static_cast<unsigned char>(*(end - 1))) != 0) {
    --end;
  }

  const size_t length = static_cast<size_t>(end - start);
  if (start != text && length > 0) {
    memmove(text, start, length);
  }

  text[length] = '\0';
}

void waitForSerialMonitor() {
  const unsigned long startMs = millis();
  while (!Serial && (millis() - startMs) < 2000UL) {
    delay(10);
  }
}

bool readSerialLine(char *buffer, size_t bufferSize) {
  if (buffer == nullptr || bufferSize < 2) {
    return false;
  }

  static bool skipNextLineFeed = false;
  size_t length = 0;
  bool overflow = false;

  while (true) {
    while (Serial.available() == 0) {
      delay(1);
    }

    const int incoming = Serial.read();
    if (incoming < 0) {
      continue;
    }

    const char character = static_cast<char>(incoming);
    if (skipNextLineFeed && character == '\n') {
      skipNextLineFeed = false;
      continue;
    }

    skipNextLineFeed = false;

    if (character == '\r' || character == '\n') {
      if (character == '\r') {
        skipNextLineFeed = true;
      }

      if (overflow) {
        buffer[0] = '\0';
        return false;
      }

      buffer[length] = '\0';
      trimWhitespace(buffer);
      Serial.println();
      return buffer[0] != '\0';
    }

    if ((character == '\b' || character == 0x7FU)) {
      if (length > 0U) {
        --length;
        Serial.print(F("\b \b"));
      }
      continue;
    }

    if ((length + 1U) < bufferSize) {
      buffer[length++] = character;
      Serial.write(character);
    } else {
      overflow = true;
    }
  }
}

bool hasValidDecimalFormat(const char *text) {
  if (text == nullptr || *text == '\0') {
    return false;
  }

  const char *cursor = text;
  if (*cursor == '+' || *cursor == '-') {
    ++cursor;
  }

  bool hasDigit = false;
  bool hasSeparator = false;
  uint8_t decimalPlaces = 0;

  while (*cursor != '\0') {
    if (isdigit(static_cast<unsigned char>(*cursor)) != 0) {
      hasDigit = true;
      if (hasSeparator) {
        ++decimalPlaces;
        if (decimalPlaces > 2U) {
          return false;
        }
      }
      ++cursor;
      continue;
    }

    if (*cursor == '.' && !hasSeparator) {
      hasSeparator = true;
      ++cursor;
      continue;
    }

    return false;
  }

  return hasDigit;
}

bool parseAndValidateChannel(const char *text, uint8_t &channel) {
  if (text == nullptr || *text == '\0') {
    return false;
  }

  char *endPtr = nullptr;
  const long parsedValue = strtol(text, &endPtr, 10);
  if (endPtr == nullptr || *endPtr != '\0') {
    return false;
  }

  if (parsedValue < 1L || parsedValue > 16L) {
    return false;
  }

  channel = static_cast<uint8_t>(parsedValue);
  return true;
}

bool parseAndValidateVoltage(const char *text, float minimum, float maximum, float &voltage) {
  if (!hasValidDecimalFormat(text)) {
    return false;
  }

  char *endPtr = nullptr;
  const double parsedValue = strtod(text, &endPtr);
  if (endPtr == nullptr || *endPtr != '\0') {
    return false;
  }

  if (parsedValue < minimum || parsedValue > maximum) {
    return false;
  }

  voltage = static_cast<float>(parsedValue);
  return true;
}

bool mapLogicalChannel(uint8_t logicalChannel, ChannelMapping &mapping) {
  if (logicalChannel < 1U || logicalChannel > 16U) {
    return false;
  }

  mapping.logicalChannel = logicalChannel;
  mapping.tcaChannel = static_cast<uint8_t>((logicalChannel - 1U) / 2U);
  mapping.mcpAddress = (logicalChannel % 2U == 1U) ? kMcp4725AddressA0High : kMcp4725AddressA0Low;
  mapping.usesAmplifiedOutput = logicalChannel >= 9U;
  return true;
}

float clampVoltage(float voltage, float minimum, float maximum) {
  if (voltage < minimum) {
    return minimum;
  }

  if (voltage > maximum) {
    return maximum;
  }

  return voltage;
}

float toDacVoltage(const ChannelMapping &mapping, float requestedVoltage) {
  const float rawDacVoltage = mapping.usesAmplifiedOutput ? (requestedVoltage / kAmplifierGain) : requestedVoltage;
  return clampVoltage(rawDacVoltage, 0.0f, kDacReferenceVolts);
}

uint16_t convertVoltageToDacCode(float dacVoltage) {
  const float clampedVoltage = clampVoltage(dacVoltage, 0.0f, kDacReferenceVolts);
  const long roundedCode = lround((clampedVoltage / kDacReferenceVolts) * static_cast<float>(kDacMaxCode));

  if (roundedCode < 0L) {
    return 0U;
  }

  if (roundedCode > static_cast<long>(kDacMaxCode)) {
    return kDacMaxCode;
  }

  return static_cast<uint16_t>(roundedCode);
}

bool probeDevice(uint8_t address, uint8_t &ackCode) {
  gI2cBus.beginTransmission(address);
  ackCode = gI2cBus.endTransmission();
  return ackCode == 0U;
}

bool selectTca9548Channel(uint8_t tcaChannel, uint8_t &ackCode) {
  if (tcaChannel > 7U) {
    ackCode = 4U;
    return false;
  }

  gI2cBus.beginTransmission(kTca9548Address);
  gI2cBus.write(static_cast<uint8_t>(1U << tcaChannel));
  ackCode = gI2cBus.endTransmission();
  return ackCode == 0U;
}

bool writeMcp4725Dac(uint8_t address, uint16_t dacCode, uint8_t &ackCode) {
  gI2cBus.beginTransmission(address);
  gI2cBus.write(0x40);
  gI2cBus.write(static_cast<uint8_t>(dacCode >> 4));
  gI2cBus.write(static_cast<uint8_t>((dacCode & 0x0FU) << 4));
  ackCode = gI2cBus.endTransmission();
  return ackCode == 0U;
}

const __FlashStringHelper *ackStatusText(uint8_t ackCode) {
  switch (ackCode) {
    case 0:
      return F("OK");
    case 1:
      return F("BUFFER");
    case 2:
      return F("ADDR_NACK");
    case 3:
      return F("DATA_NACK");
    case 4:
      return F("OUTRO");
    default:
      return F("DESCONHECIDO");
  }
}

void printHexAddress(uint8_t address) {
  Serial.print(F("0x"));
  if (address < 0x10U) {
    Serial.print('0');
  }
  Serial.print(address, HEX);
}

void printOperationSummary(const ChannelMapping &mapping,
                           float requestedVoltage,
                           float dacVoltage,
                           uint16_t dacCode,
                           uint8_t tcaAck,
                           uint8_t probeAckBefore,
                           uint8_t writeAck,
                           uint8_t probeAckAfter) {
  Serial.print(F("Canal: "));
  Serial.print(mapping.logicalChannel);
  Serial.print(F(" | TCA: SC"));
  Serial.print(mapping.tcaChannel);
  Serial.print(F(" | MCP: "));
  printHexAddress(mapping.mcpAddress);
  Serial.print(F(" | Vout desejada: "));
  Serial.print(requestedVoltage, 2);
  Serial.print(F(" V | Vdac: "));
  Serial.print(dacVoltage, 2);
  Serial.print(F(" V | DAC: "));
  Serial.print(dacCode);
  Serial.print(F(" (0x"));
  Serial.print(dacCode, HEX);
  Serial.print(F(")"));
  Serial.print(F(" | Escrita: "));
  Serial.print(ackStatusText(writeAck));
  Serial.print(F(" | ACK final: "));
  Serial.println(ackStatusText(probeAckAfter));

  Serial.print(F("ACK TCA: "));
  Serial.print(ackStatusText(tcaAck));
  Serial.print(F(" | ACK MCP antes: "));
  Serial.print(ackStatusText(probeAckBefore));
  Serial.print(F(" | ACK escrita: "));
  Serial.print(ackStatusText(writeAck));
  Serial.print(F(" | ACK MCP depois: "));
  Serial.println(ackStatusText(probeAckAfter));
}

bool readValidatedChannelFromSerial(uint8_t &channel) {
  char buffer[kLineBufferSize];
  if (!readSerialLine(buffer, sizeof(buffer))) {
    return false;
  }

  return parseAndValidateChannel(buffer, channel);
}

bool readValidatedVoltageFromSerial(uint8_t logicalChannel, float &voltage) {
  char buffer[kLineBufferSize];
  if (!readSerialLine(buffer, sizeof(buffer))) {
    return false;
  }

  const float minimum = 0.0f;
  const float maximum = (logicalChannel <= 8U) ? 5.0f : 12.0f;
  return parseAndValidateVoltage(buffer, minimum, maximum, voltage);
}

void processInteractiveCommand() {
  uint8_t logicalChannel = 0;
  float requestedVoltage = 0.0f;

  Serial.println(F("Digite um canal (1-16):"));
  if (!readValidatedChannelFromSerial(logicalChannel)) {
    Serial.println(F("Valor inválido"));
    return;
  }

  Serial.println(F("Digite um valor com até duas casas:"));
  if (!readValidatedVoltageFromSerial(logicalChannel, requestedVoltage)) {
    Serial.println(F("Valor inválido"));
    return;
  }

  ChannelMapping mapping = {};
  if (!mapLogicalChannel(logicalChannel, mapping)) {
    Serial.println(F("Valor inválido"));
    return;
  }

  const float dacVoltage = toDacVoltage(mapping, requestedVoltage);
  const uint16_t dacCode = convertVoltageToDacCode(dacVoltage);

  uint8_t tcaAck = 4U;
  if (!selectTca9548Channel(mapping.tcaChannel, tcaAck)) {
    Serial.print(F("TCA9548 nao respondeu. ACK="));
    Serial.println(ackStatusText(tcaAck));
    return;
  }

  uint8_t probeAckBefore = 4U;
  if (!probeDevice(mapping.mcpAddress, probeAckBefore)) {
    Serial.print(F("MCP4725 nao respondeu no canal selecionado. ACK="));
    Serial.println(ackStatusText(probeAckBefore));
    return;
  }

  uint8_t writeAck = 4U;
  if (!writeMcp4725Dac(mapping.mcpAddress, dacCode, writeAck)) {
    Serial.print(F("Falha na escrita do MCP4725. ACK="));
    Serial.println(ackStatusText(writeAck));
    return;
  }

  uint8_t probeAckAfter = 4U;
  probeDevice(mapping.mcpAddress, probeAckAfter);

  printOperationSummary(mapping, requestedVoltage, dacVoltage, dacCode, tcaAck, probeAckBefore, writeAck, probeAckAfter);
}

}  // namespace

void setup() {
  Serial.begin(kSerialBaud);
  waitForSerialMonitor();

  pinMode(kSoftSdaPin, INPUT);
  pinMode(kSoftSclPin, INPUT);
  digitalWrite(kSoftSdaPin, LOW);
  digitalWrite(kSoftSclPin, LOW);
  gI2cBus.begin();

  Serial.println(F("===Teste da placa GSA==="));
}

void loop() {
  processInteractiveCommand();
}
