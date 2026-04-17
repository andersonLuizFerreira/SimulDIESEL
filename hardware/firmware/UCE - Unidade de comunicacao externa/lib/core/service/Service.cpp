#include "core/service/Service.h"

#include "diag/trace/DiagTrace.h"
#include "services/can/CanConfig.h"
#include "services/can/CanStatus.h"
#include "services/can/CanTypes.h"

namespace {

constexpr uint8_t kCanControllerCan0 = 0x00;
constexpr uint8_t kCanControllerCan1 = 0x01;
constexpr uint8_t kCanBitrate125 = 0x00;
constexpr uint8_t kCanBitrate250 = 0x01;
constexpr uint8_t kCanBitrate500 = 0x02;
constexpr uint8_t kCanBitrate1000 = 0x03;
constexpr uint8_t kCanModeNormal = 0x00;
constexpr uint8_t kCanModeListen = 0x01;
constexpr uint8_t kCanStateOff = 0x00;
constexpr uint8_t kCanStateOn = 0x01;
constexpr uint8_t kCanResetFailed = 0x00;
constexpr uint8_t kCanResetSucceeded = 0x01;

bool decodeCanController(uint8_t code, UceCan::Controller& controller) {
  switch (code) {
    case kCanControllerCan0:
      controller = UceCan::Controller::Can0;
      return true;
    case kCanControllerCan1:
      controller = UceCan::Controller::Can1;
      return true;
    default:
      controller = UceCan::Controller::Can0;
      return false;
  }
}

bool decodeCanBitrate(uint8_t code, uint32_t& bitrateKbps) {
  switch (code) {
    case kCanBitrate125:
      bitrateKbps = 125;
      return true;
    case kCanBitrate250:
      bitrateKbps = 250;
      return true;
    case kCanBitrate500:
      bitrateKbps = 500;
      return true;
    case kCanBitrate1000:
      bitrateKbps = 1000;
      return true;
    default:
      bitrateKbps = 0;
      return false;
  }
}

bool decodeCanMode(uint8_t code, UceCan::Mode& mode) {
  switch (code) {
    case kCanModeNormal:
      mode = UceCan::Mode::Normal;
      return true;
    case kCanModeListen:
      mode = UceCan::Mode::ListenOnly;
      return true;
    default:
      mode = UceCan::Mode::Normal;
      return false;
  }
}

uint8_t encodeCanController(UceCan::Controller controller) {
  return controller == UceCan::Controller::Can1 ? kCanControllerCan1 : kCanControllerCan0;
}

bool encodeCanBitrate(uint32_t bitrateKbps, uint8_t& bitrateCode) {
  switch (bitrateKbps) {
    case 125:
      bitrateCode = kCanBitrate125;
      return true;
    case 250:
      bitrateCode = kCanBitrate250;
      return true;
    case 500:
      bitrateCode = kCanBitrate500;
      return true;
    case 1000:
      bitrateCode = kCanBitrate1000;
      return true;
    default:
      bitrateCode = kCanBitrate250;
      return false;
  }
}

uint8_t encodeCanMode(UceCan::Mode mode) {
  return mode == UceCan::Mode::ListenOnly ? kCanModeListen : kCanModeNormal;
}

uint8_t encodeCanInterfaceState(UceCan::InterfaceState state) {
  switch (state) {
    case UceCan::InterfaceState::Configured:
      return 0x01;
    case UceCan::InterfaceState::Open:
      return 0x02;
    case UceCan::InterfaceState::Fault:
      return 0x03;
    default:
      return 0x00;
  }
}

}  // namespace

Service::Service(LedService& led, CanService& can)
  : _led(led),
    _can(can)
{
}

void Service::begin() {
}

bool Service::handleOneTlv(const TlvFrame& tlv, uint8_t* txOut, uint8_t& txLenOut) {
  if (!txOut) return false;
  txLenOut = 0;

  uint8_t traceBuf[TLV_MAX_LEN];
  uint8_t traceLen = 0;
  traceBuf[traceLen++] = tlv.t;
  traceBuf[traceLen++] = tlv.l;
  for (uint8_t index = 0; index < tlv.l && traceLen < TLV_MAX_LEN; ++index) {
    traceBuf[traceLen++] = tlv.v[index];
  }
  DiagTrace::logBytes(DiagTrace::EvServiceHandle, traceBuf, traceLen);

  switch (tlv.t) {
    case CMD_LED_BUILTIN:
      return handleBuiltinLed(tlv, txOut, txLenOut);
    case CMD_CAN_CONFIG:
      return handleCanConfig(tlv, txOut, txLenOut);
    case CMD_CAN_ENABLE:
      return handleCanEnable(tlv, txOut, txLenOut);
    case CMD_CAN_STATUS:
      return handleCanStatus(tlv, txOut, txLenOut);
    case CMD_CAN_RESET:
      return handleCanReset(tlv, txOut, txLenOut);
    default:
      return buildFunctionalError(tlv.t, UCE_ERROR_COMMAND_NOT_SUPPORTED, txOut, txLenOut);
  }
}

bool Service::handleBuiltinLed(const TlvFrame& tlv, uint8_t* txOut, uint8_t& txLenOut) {
  if (tlv.l != 1 || tlv.v == nullptr) {
    return buildFunctionalError(CMD_LED_BUILTIN, UCE_ERROR_INVALID_PAYLOAD, txOut, txLenOut);
  }

  if (tlv.v[0] > 1) {
    return buildFunctionalError(CMD_LED_BUILTIN, UCE_ERROR_INVALID_STATE, txOut, txLenOut);
  }

  const uint8_t currentState = (uint8_t)_led.set(tlv.v[0]);
  txLenOut = Tlv::buildU8(CMD_LED_BUILTIN, currentState, txOut, TLV_MAX_LEN);
  return txLenOut != 0;
}

bool Service::handleCanConfig(const TlvFrame& tlv, uint8_t* txOut, uint8_t& txLenOut) {
  if (tlv.l != 3 || tlv.v == nullptr) {
    return buildFunctionalError(CMD_CAN_CONFIG, UCE_ERROR_INVALID_PAYLOAD, txOut, txLenOut);
  }

  UceCan::Controller controller;
  uint32_t bitrateKbps = 0;
  UceCan::Mode mode;
  if (!decodeCanController(tlv.v[0], controller) ||
      !decodeCanBitrate(tlv.v[1], bitrateKbps) ||
      !decodeCanMode(tlv.v[2], mode)) {
    return buildFunctionalError(CMD_CAN_CONFIG, UCE_ERROR_INVALID_STATE, txOut, txLenOut);
  }

  CanConfig config = _can.config();
  config.controller = controller;
  config.bitrateKbps = bitrateKbps;
  config.mode = mode;
  if (!_can.configure(config)) {
    return buildFunctionalError(CMD_CAN_CONFIG, UCE_ERROR_INVALID_STATE, txOut, txLenOut);
  }

  const uint8_t response[3] = {
    encodeCanController(config.controller),
    tlv.v[1],
    encodeCanMode(config.mode)
  };
  txLenOut = Tlv::build(CMD_CAN_CONFIG, response, sizeof(response), txOut, TLV_MAX_LEN);
  return txLenOut != 0;
}

bool Service::handleCanEnable(const TlvFrame& tlv, uint8_t* txOut, uint8_t& txLenOut) {
  if (tlv.l != 2 || tlv.v == nullptr) {
    return buildFunctionalError(CMD_CAN_ENABLE, UCE_ERROR_INVALID_PAYLOAD, txOut, txLenOut);
  }

  UceCan::Controller controller;
  if (!decodeCanController(tlv.v[0], controller)) {
    return buildFunctionalError(CMD_CAN_ENABLE, UCE_ERROR_INVALID_STATE, txOut, txLenOut);
  }

  if (tlv.v[1] != kCanStateOff && tlv.v[1] != kCanStateOn) {
    return buildFunctionalError(CMD_CAN_ENABLE, UCE_ERROR_INVALID_STATE, txOut, txLenOut);
  }

  const CanConfig& config = _can.config();
  if (config.controller != controller) {
    return buildFunctionalError(CMD_CAN_ENABLE, UCE_ERROR_INVALID_STATE, txOut, txLenOut);
  }

  if (tlv.v[1] == kCanStateOn) {
    if (!_can.open()) {
      return buildFunctionalError(CMD_CAN_ENABLE, UCE_ERROR_INVALID_STATE, txOut, txLenOut);
    }
  } else {
    _can.close();
  }

  const uint8_t response[2] = {
    encodeCanController(controller),
    _can.isOpen() ? kCanStateOn : kCanStateOff
  };
  txLenOut = Tlv::build(CMD_CAN_ENABLE, response, sizeof(response), txOut, TLV_MAX_LEN);
  return txLenOut != 0;
}

bool Service::handleCanStatus(const TlvFrame& tlv, uint8_t* txOut, uint8_t& txLenOut) {
  if (tlv.l != 1 || tlv.v == nullptr) {
    return buildFunctionalError(CMD_CAN_STATUS, UCE_ERROR_INVALID_PAYLOAD, txOut, txLenOut);
  }

  UceCan::Controller controller;
  if (!decodeCanController(tlv.v[0], controller)) {
    return buildFunctionalError(CMD_CAN_STATUS, UCE_ERROR_INVALID_STATE, txOut, txLenOut);
  }

  const CanConfig& config = _can.config();
  if (config.controller != controller) {
    return buildFunctionalError(CMD_CAN_STATUS, UCE_ERROR_INVALID_STATE, txOut, txLenOut);
  }

  const CanStatus status = _can.status();
  uint8_t bitrateCode = 0;
  if (!encodeCanBitrate(status.bitrateKbps, bitrateCode)) {
    return buildFunctionalError(CMD_CAN_STATUS, UCE_ERROR_INVALID_STATE, txOut, txLenOut);
  }

  const uint8_t response[4] = {
    encodeCanController(controller),
    encodeCanInterfaceState(status.state),
    bitrateCode,
    encodeCanMode(status.mode)
  };
  txLenOut = Tlv::build(CMD_CAN_STATUS, response, sizeof(response), txOut, TLV_MAX_LEN);
  return txLenOut != 0;
}

bool Service::handleCanReset(const TlvFrame& tlv, uint8_t* txOut, uint8_t& txLenOut) {
  if (tlv.l != 1 || tlv.v == nullptr) {
    return buildFunctionalError(CMD_CAN_RESET, UCE_ERROR_INVALID_PAYLOAD, txOut, txLenOut);
  }

  UceCan::Controller controller;
  if (!decodeCanController(tlv.v[0], controller)) {
    return buildFunctionalError(CMD_CAN_RESET, UCE_ERROR_INVALID_STATE, txOut, txLenOut);
  }

  const CanConfig& config = _can.config();
  if (config.controller != controller) {
    return buildFunctionalError(CMD_CAN_RESET, UCE_ERROR_INVALID_STATE, txOut, txLenOut);
  }

  const bool resetSucceeded = _can.reset();
  const uint8_t response[2] = {
    encodeCanController(controller),
    resetSucceeded ? kCanResetSucceeded : kCanResetFailed
  };
  txLenOut = Tlv::build(CMD_CAN_RESET, response, sizeof(response), txOut, TLV_MAX_LEN);
  return txLenOut != 0;
}

bool Service::buildFunctionalError(uint8_t requestType, uint8_t errorCode, uint8_t* txOut, uint8_t& txLenOut) const {
  const uint8_t payload[3] = { requestType, 0, errorCode };
  txLenOut = Tlv::build(CMD_FUNCTIONAL_ERROR, payload, sizeof(payload), txOut, TLV_MAX_LEN);
  return txLenOut != 0;
}
