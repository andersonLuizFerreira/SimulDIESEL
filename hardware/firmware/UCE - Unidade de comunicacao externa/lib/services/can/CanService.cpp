#include "services/can/CanService.h"

#include "defs.h"

namespace {
const uint8_t CAN_CONTROLLER_CAN0 = 0x00;
const uint8_t CAN_CONTROLLER_CAN1 = 0x01;
const uint8_t CAN_BITRATE_250_KBPS = 0x01;
const uint8_t CAN_MODE_NORMAL = 0x00;
const uint8_t CAN_STATE_OFF = 0x00;
const uint8_t CAN_STATE_ON = 0x01;
const uint8_t CAN_INTERFACE_DISABLED = 0x00;
const uint8_t CAN_INTERFACE_CONFIGURED = 0x01;
const uint8_t CAN_INTERFACE_OPEN = 0x02;
}

void CanService::begin() {
  for (uint8_t controller = 0; controller < ControllerCount; ++controller) {
    resetPort(controller);
  }
}

bool CanService::handleTlv(uint8_t type,
                           const uint8_t* value,
                           uint8_t valueLen,
                           uint8_t* responseValue,
                           uint8_t& responseValueLen,
                           uint8_t& errorCode) {
  responseValueLen = 0;
  errorCode = 0;

  switch (type) {
    case CMD_CAN_CONFIG:
      return handleConfig(value, valueLen, responseValue, responseValueLen, errorCode);
    case CMD_CAN_ENABLE:
      return handleEnable(value, valueLen, responseValue, responseValueLen, errorCode);
    case CMD_CAN_STATUS:
      return handleStatus(value, valueLen, responseValue, responseValueLen, errorCode);
    case CMD_CAN_RESET:
      return handleReset(value, valueLen, responseValue, responseValueLen, errorCode);
    default:
      errorCode = UCE_ERROR_COMMAND_NOT_SUPPORTED;
      return false;
  }
}

void CanService::resetPort(uint8_t controller) {
  if (!validateController(controller)) {
    return;
  }

  _ports[controller].bitrateCode = CAN_BITRATE_250_KBPS;
  _ports[controller].modeCode = CAN_MODE_NORMAL;
  _ports[controller].interfaceState = CAN_INTERFACE_DISABLED;
}

bool CanService::validateController(uint8_t controller) const {
  return controller == CAN_CONTROLLER_CAN0 || controller == CAN_CONTROLLER_CAN1;
}

bool CanService::validateBitrate(uint8_t bitrateCode) const {
  return bitrateCode <= 0x03;
}

bool CanService::validateMode(uint8_t modeCode) const {
  return modeCode <= 0x01;
}

bool CanService::validateEnableState(uint8_t state) const {
  return state == CAN_STATE_OFF || state == CAN_STATE_ON;
}

bool CanService::handleConfig(const uint8_t* value,
                              uint8_t valueLen,
                              uint8_t* responseValue,
                              uint8_t& responseValueLen,
                              uint8_t& errorCode) {
  if (!value || valueLen != 3) {
    errorCode = UCE_ERROR_INVALID_PAYLOAD;
    return false;
  }

  const uint8_t controller = value[0];
  const uint8_t bitrateCode = value[1];
  const uint8_t modeCode = value[2];
  if (!validateController(controller) || !validateBitrate(bitrateCode) || !validateMode(modeCode)) {
    errorCode = UCE_ERROR_INVALID_STATE;
    return false;
  }

  PortState& port = _ports[controller];
  port.bitrateCode = bitrateCode;
  port.modeCode = modeCode;
  if (port.interfaceState != CAN_INTERFACE_OPEN) {
    port.interfaceState = CAN_INTERFACE_CONFIGURED;
  }

  if (responseValue) {
    responseValue[0] = controller;
    responseValue[1] = port.bitrateCode;
    responseValue[2] = port.modeCode;
  }
  responseValueLen = 3;
  return true;
}

bool CanService::handleEnable(const uint8_t* value,
                              uint8_t valueLen,
                              uint8_t* responseValue,
                              uint8_t& responseValueLen,
                              uint8_t& errorCode) {
  if (!value || valueLen != 2) {
    errorCode = UCE_ERROR_INVALID_PAYLOAD;
    return false;
  }

  const uint8_t controller = value[0];
  const uint8_t requestedState = value[1];
  if (!validateController(controller) || !validateEnableState(requestedState)) {
    errorCode = UCE_ERROR_INVALID_STATE;
    return false;
  }

  PortState& port = _ports[controller];
  port.interfaceState = (requestedState == CAN_STATE_ON) ? CAN_INTERFACE_OPEN : CAN_INTERFACE_DISABLED;

  if (responseValue) {
    responseValue[0] = controller;
    responseValue[1] = (port.interfaceState == CAN_INTERFACE_OPEN) ? CAN_STATE_ON : CAN_STATE_OFF;
  }
  responseValueLen = 2;
  return true;
}

bool CanService::handleStatus(const uint8_t* value,
                              uint8_t valueLen,
                              uint8_t* responseValue,
                              uint8_t& responseValueLen,
                              uint8_t& errorCode) {
  if (!value || valueLen != 1) {
    errorCode = UCE_ERROR_INVALID_PAYLOAD;
    return false;
  }

  const uint8_t controller = value[0];
  if (!validateController(controller)) {
    errorCode = UCE_ERROR_INVALID_STATE;
    return false;
  }

  const PortState& port = _ports[controller];
  if (responseValue) {
    responseValue[0] = controller;
    responseValue[1] = port.interfaceState;
    responseValue[2] = port.bitrateCode;
    responseValue[3] = port.modeCode;
  }
  responseValueLen = 4;
  return true;
}

bool CanService::handleReset(const uint8_t* value,
                             uint8_t valueLen,
                             uint8_t* responseValue,
                             uint8_t& responseValueLen,
                             uint8_t& errorCode) {
  if (!value || valueLen != 1) {
    errorCode = UCE_ERROR_INVALID_PAYLOAD;
    return false;
  }

  const uint8_t controller = value[0];
  if (!validateController(controller)) {
    errorCode = UCE_ERROR_INVALID_STATE;
    return false;
  }

  resetPort(controller);
  if (responseValue) {
    responseValue[0] = controller;
    responseValue[1] = 0x01;
  }
  responseValueLen = 2;
  return true;
}
