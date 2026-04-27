#include "core/transport/UceTransport.h"

#include <string.h>

#include "defs.h"

void UceTransport::begin() {
  _dispatcher.begin();
}

void UceTransport::poll() {
  if (!_link.available()) {
    drainPendingEvent();
    return;
  }

  uint8_t rx[SpiLink::BufferSize] = {0};
  size_t rxLen = 0;
  if (!_link.read(rx, rxLen)) {
    return;
  }

  bool emptyFrame = true;
  for (size_t index = 0; index < rxLen; ++index) {
    if (rx[index] != 0x00) {
      emptyFrame = false;
      break;
    }
  }
  if (emptyFrame) {
    drainPendingEvent();
    return;
  }

  uint8_t type = 0;
  const uint8_t* value = nullptr;
  uint8_t valueLen = 0;

  uint8_t tx[SpiLink::BufferSize] = {0};
  size_t txLen = 0;

  if (!parsePacket(rx, rxLen, type, value, valueLen)) {
    const uint8_t requestType = type;
    txLen = buildFunctionalError(requestType, UCE_ERROR_INVALID_TLV_CRC, tx, sizeof(tx));
    _link.write(tx, txLen);
    return;
  }

  uint8_t responseValue[SpiLink::BufferSize - 3] = {0};
  uint8_t responseValueLen = 0;
  uint8_t eventValue[SpiLink::BufferSize - 3] = {0};
  uint8_t eventValueLen = 0;
  uint8_t eventType = 0;
  uint8_t responseType = type;
  uint8_t errorCode = 0;

  if (_dispatcher.dispatch(type, value, valueLen, responseType, responseValue, responseValueLen, errorCode, eventType, eventValue, eventValueLen)) {
    txLen = buildPacket(responseType, responseValue, responseValueLen, tx, sizeof(tx));
  } else {
    txLen = buildFunctionalError(type, errorCode ? errorCode : UCE_ERROR_COMMAND_NOT_SUPPORTED, tx, sizeof(tx));
  }

  if (txLen > 0) {
    _link.write(tx, txLen);
  }

  if (eventType != 0 && eventValueLen > 0) {
    queueEvent(eventType, eventValue, eventValueLen);
  }

  drainPendingEvent();
}

bool UceTransport::publishEvent(uint8_t type, const uint8_t* value, uint8_t valueLen) {
  if (!queueEvent(type, value, valueLen)) {
    return false;
  }

  drainPendingEvent();
  return true;
}

uint8_t UceTransport::crc8(const uint8_t* data, size_t len) {
  uint8_t crc = 0x00;
  for (size_t index = 0; index < len; ++index) {
    crc ^= data[index];
    for (uint8_t bit = 0; bit < 8; ++bit) {
      crc = (crc & 0x80U) ? (uint8_t)((crc << 1U) ^ 0x07U) : (uint8_t)(crc << 1U);
    }
  }
  return crc;
}

bool UceTransport::parsePacket(const uint8_t* frame, size_t frameLen, uint8_t& type, const uint8_t*& value, uint8_t& valueLen) {
  type = 0;
  value = nullptr;
  valueLen = 0;

  if (!frame || frameLen != SpiLink::BufferSize || frameLen < 3) {
    return false;
  }

  size_t cobsLen = 0;
  if (!findCobsFrame(frame, frameLen, cobsLen)) {
    return false;
  }

  static uint8_t decoded[SpiLink::BufferSize] = {0};
  size_t decodedLen = 0;
  if (!cobsDecode(frame, cobsLen, decoded, sizeof(decoded), decodedLen) || decodedLen < 3) {
    return false;
  }

  const uint8_t packetType = decoded[0];
  const uint8_t packetValueLen = decoded[1];
  const size_t packetLen = (size_t)2U + packetValueLen + 1U;
  if (packetLen != decodedLen || packetLen < 3) {
    return false;
  }

  type = packetType;
  valueLen = packetValueLen;
  value = (packetValueLen > 0) ? &decoded[2] : nullptr;

  if (crc8(decoded, packetLen - 1) != decoded[packetLen - 1]) {
    return false;
  }

  return true;
}

size_t UceTransport::buildPacket(uint8_t type, const uint8_t* value, uint8_t valueLen, uint8_t* out, size_t outMax) {
  const size_t packetLen = (size_t)2U + valueLen + 1U;
  if (!out || packetLen > outMax) {
    return 0;
  }

  uint8_t raw[SpiLink::BufferSize] = {0};
  memset(out, 0, outMax);
  raw[0] = type;
  raw[1] = valueLen;
  if (valueLen > 0 && value) {
    memcpy(&raw[2], value, valueLen);
  }
  raw[packetLen - 1] = crc8(raw, packetLen - 1);

  size_t encodedLen = 0;
  if (!cobsEncode(raw, packetLen, out, outMax, encodedLen)) {
    return 0;
  }
  return encodedLen;
}

size_t UceTransport::buildFunctionalError(uint8_t requestType, uint8_t errorCode, uint8_t* out, size_t outMax) {
  uint8_t payload[3] = {requestType, 0x00, errorCode};
  return buildPacket(CMD_FUNCTIONAL_ERROR, payload, sizeof(payload), out, outMax);
}

bool UceTransport::queueEvent(uint8_t type, const uint8_t* value, uint8_t valueLen) {
  if (type == 0 || valueLen > sizeof(_pendingEventValue) || (valueLen > 0 && !value)) {
    return false;
  }

  if (_pendingEvent) {
    return false;
  }

  _pendingEventType = type;
  _pendingEventValueLen = valueLen;
  if (valueLen > 0) {
    memcpy(_pendingEventValue, value, valueLen);
  }
  _pendingEvent = true;
  return true;
}

bool UceTransport::drainPendingEvent() {
  if (!_pendingEvent || _link.txPending()) {
    return false;
  }

  uint8_t tx[SpiLink::BufferSize] = {0};
  const size_t txLen = buildPacket(_pendingEventType, _pendingEventValue, _pendingEventValueLen, tx, sizeof(tx));
  if (txLen == 0) {
    _pendingEvent = false;
    return false;
  }

  if (!_link.write(tx, txLen)) {
    return false;
  }

  _pendingEvent = false;
  return true;
}

bool UceTransport::findCobsFrame(const uint8_t* frame, size_t frameLen, size_t& cobsLen) {
  cobsLen = 0;
  if (!frame || frameLen == 0 || frame[0] == 0x00) {
    return false;
  }

  while (cobsLen < frameLen && frame[cobsLen] != 0x00) {
    ++cobsLen;
  }

  return cobsLen > 0;
}

bool UceTransport::cobsEncode(const uint8_t* in, size_t inLen, uint8_t* out, size_t outMax, size_t& outLen) {
  outLen = 0;
  if (!out || (!in && inLen != 0) || outMax < 1) {
    return false;
  }

  size_t codeIndex = 0;
  uint8_t code = 1;
  out[codeIndex] = 0;
  outLen = 1;

  for (size_t index = 0; index < inLen; ++index) {
    if (in[index] == 0x00) {
      out[codeIndex] = code;
      codeIndex = outLen;
      if (outLen >= outMax) {
        return false;
      }
      out[outLen++] = 0;
      code = 1;
    } else {
      if (outLen >= outMax) {
        return false;
      }
      out[outLen++] = in[index];
      ++code;
      if (code == 0xFF) {
        out[codeIndex] = code;
        codeIndex = outLen;
        if (outLen >= outMax) {
          return false;
        }
        out[outLen++] = 0;
        code = 1;
      }
    }
  }

  out[codeIndex] = code;
  return true;
}

bool UceTransport::cobsDecode(const uint8_t* in, size_t inLen, uint8_t* out, size_t outMax, size_t& outLen) {
  outLen = 0;
  if (!out || (!in && inLen != 0)) {
    return false;
  }

  size_t index = 0;
  while (index < inLen) {
    const uint8_t code = in[index++];
    if (code == 0x00) {
      return false;
    }

    const uint8_t copyLen = (uint8_t)(code - 1);
    if (index + copyLen > inLen || outLen + copyLen > outMax) {
      return false;
    }

    for (uint8_t copyIndex = 0; copyIndex < copyLen; ++copyIndex) {
      out[outLen++] = in[index++];
    }

    if (code != 0xFF && index < inLen) {
      if (outLen >= outMax) {
        return false;
      }
      out[outLen++] = 0x00;
    }
  }

  return true;
}
