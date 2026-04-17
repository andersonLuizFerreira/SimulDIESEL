#pragma once

#include <Arduino.h>

#include "services/can/CanTypes.h"

namespace Sam3xCanRegisters {

inline Can* controller(UceCan::Controller controllerId) {
  return (controllerId == UceCan::Controller::Can0) ? CAN0 : CAN1;
}

inline IRQn_Type irq(UceCan::Controller controllerId) {
  return (controllerId == UceCan::Controller::Can0) ? CAN0_IRQn : CAN1_IRQn;
}

inline uint32_t peripheralId(UceCan::Controller controllerId) {
  return (controllerId == UceCan::Controller::Can0) ? ID_CAN0 : ID_CAN1;
}

inline uint8_t boardPinIndex(UceCan::Controller controllerId) {
  return (controllerId == UceCan::Controller::Can0) ? (uint8_t)PINS_CAN0 : (uint8_t)PINS_CAN1;
}

}  // namespace Sam3xCanRegisters
