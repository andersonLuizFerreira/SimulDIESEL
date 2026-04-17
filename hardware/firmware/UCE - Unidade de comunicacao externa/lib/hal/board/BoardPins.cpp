#include "hal/board/BoardPins.h"

namespace {
const BoardCanPins kCan0Pins = {
  0,
  (uint8_t)PINS_CAN0,
  UCE_PIN_NOT_CONNECTED,
  UCE_PIN_NOT_CONNECTED,
  UCE_PIN_NOT_CONNECTED,
  UCE_PIN_NOT_CONNECTED
};

const BoardCanPins kCan1Pins = {
  1,
  (uint8_t)PINS_CAN1,
  UCE_PIN_NOT_CONNECTED,
  UCE_PIN_NOT_CONNECTED,
  UCE_PIN_NOT_CONNECTED,
  UCE_PIN_NOT_CONNECTED
};
}

uint8_t BoardPins::invalidPin() {
  return UCE_PIN_NOT_CONNECTED;
}

const BoardCanPins& BoardPins::can0() {
  return kCan0Pins;
}

const BoardCanPins& BoardPins::can1() {
  return kCan1Pins;
}

void BoardPins::configureCanPins(uint8_t controllerIndex) {
  const BoardCanPins& pins = (controllerIndex == 0) ? can0() : can1();
  if (pins.peripheralPinIndex == invalidPin()) return;

  pmc_enable_periph_clk(g_APinDescription[pins.peripheralPinIndex].ulPeripheralId);
  PIO_Configure(
      g_APinDescription[pins.peripheralPinIndex].pPort,
      g_APinDescription[pins.peripheralPinIndex].ulPinType,
      g_APinDescription[pins.peripheralPinIndex].ulPin,
      g_APinDescription[pins.peripheralPinIndex].ulPinConfiguration);
}
