#include "EepromService.h"

#include <Arduino.h>
#include <EEPROM.h>

#include "config.h"
#include "crc8.h"

namespace {
struct GsaEepromImage {
  uint16_t signature;
  uint8_t version;
  GsaChannelOffsets offsets[GSA_CHANNEL_COUNT];
  uint8_t crc;
};

static_assert(sizeof(GsaChannelOffsets) == 6, "Unexpected offset layout.");

uint8_t calcImageCrc(const GsaEepromImage& image) {
  return Crc8::calc(reinterpret_cast<const uint8_t*>(&image), sizeof(GsaEepromImage) - 1);
}

void zeroOffsets(GsaChannelOffsets* offsets, uint8_t count) {
  if (!offsets) return;

  for (uint8_t index = 0; index < count; index++) {
    offsets[index].vout = 0;
    offsets[index].vread = 0;
    offsets[index].iread = 0;
  }
}
}

bool EepromService::loadOffsets(GsaChannelOffsets* outOffsets, uint8_t count) {
  if (!outOffsets || count != GSA_CHANNEL_COUNT) {
    return false;
  }

  GsaEepromImage image;
  EEPROM.get(0, image);

  bool validImage =
    image.signature == GSA_EEPROM_SIGNATURE &&
    image.version == GSA_EEPROM_VERSION &&
    image.crc == calcImageCrc(image);

  if (!validImage) {
    zeroOffsets(outOffsets, count);
    saveOffsets(outOffsets, count);
    return false;
  }

  for (uint8_t index = 0; index < count; index++) {
    outOffsets[index] = image.offsets[index];
  }

  return true;
}

bool EepromService::saveOffsets(const GsaChannelOffsets* offsets, uint8_t count) {
  if (!offsets || count != GSA_CHANNEL_COUNT) {
    return false;
  }

  GsaEepromImage image;
  image.signature = GSA_EEPROM_SIGNATURE;
  image.version = GSA_EEPROM_VERSION;

  for (uint8_t index = 0; index < count; index++) {
    image.offsets[index] = offsets[index];
  }

  image.crc = calcImageCrc(image);

  const uint8_t* bytes = reinterpret_cast<const uint8_t*>(&image);
  for (unsigned int index = 0; index < sizeof(GsaEepromImage); index++) {
    EEPROM.update(index, bytes[index]);
  }

  GsaEepromImage verify;
  EEPROM.get(0, verify);

  const uint8_t* verifyBytes = reinterpret_cast<const uint8_t*>(&verify);
  for (unsigned int index = 0; index < sizeof(GsaEepromImage); index++) {
    if (verifyBytes[index] != bytes[index]) {
      return false;
    }
  }

  return true;
}
