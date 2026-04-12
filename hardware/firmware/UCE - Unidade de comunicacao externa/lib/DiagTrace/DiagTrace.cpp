#include "DiagTrace.h"

volatile uint8_t DiagTrace::_head = 0;
volatile uint8_t DiagTrace::_tail = 0;
DiagTrace::Event DiagTrace::_queue[32];
bool DiagTrace::_started = false;

void DiagTrace::begin() {
  if (_started) return;

  SerialUSB.begin(115200);
  _started = true;
  logState(EvTransportBegin, 0, 0, 0);
}

void DiagTrace::logState(uint8_t code, uint8_t a, uint8_t b, uint8_t c) {
  Event event{};
  event.ts = millis();
  event.code = code;
  event.a = a;
  event.b = b;
  event.c = c;
  event.len = 0;
  enqueue(event);
}

void DiagTrace::logBytes(uint8_t code, const uint8_t* data, uint8_t len, uint8_t a, uint8_t b, uint8_t c) {
  Event event{};
  event.ts = millis();
  event.code = code;
  event.a = a;
  event.b = b;
  event.c = c;
  event.len = (len > TLV_MAX_LEN) ? TLV_MAX_LEN : len;
  for (uint8_t index = 0; index < event.len; index++) event.data[index] = data[index];
  enqueue(event);
}

bool DiagTrace::enqueue(const Event& event) {
  const uint32_t primask = __get_PRIMASK();
  noInterrupts();

  const uint8_t next = (uint8_t)((_head + 1U) % 32U);
  if (next == _tail) {
    if ((primask & 0x1U) == 0U) interrupts();
    return false;
  }

  _queue[_head] = event;
  _head = next;

  if ((primask & 0x1U) == 0U) interrupts();
  return true;
}

void DiagTrace::flush() {
  if (!_started) return;

  while (_tail != _head) {
    Event event{};

    const uint32_t primask = __get_PRIMASK();
    noInterrupts();
    event = _queue[_tail];
    _tail = (uint8_t)((_tail + 1U) % 32U);
    if ((primask & 0x1U) == 0U) interrupts();

    printEvent(event);
  }
}

void DiagTrace::printEvent(const Event& event) {
  SerialUSB.print('[');
  SerialUSB.print(event.ts);
  SerialUSB.print("] ");

  switch (event.code) {
    case EvTransportBegin:
      SerialUSB.println("Transport.begin");
      return;
    case EvTransportRequestCaptured:
      SerialUSB.print("Transport request captured RX=");
      break;
    case EvTransportSetTx:
      SerialUSB.print("Transport response staged TX=");
      break;
    case EvTransportPreload:
      SerialUSB.print("Transport preload firstByte=");
      printHexByte(event.a);
      SerialUSB.print(" txIndex=");
      SerialUSB.print(event.b);
      SerialUSB.print(" txPending=");
      SerialUSB.println(event.c);
      return;
    case EvTransportAdvance:
      SerialUSB.print("Transport advance nextByte=");
      printHexByte(event.a);
      SerialUSB.print(" txIndex=");
      SerialUSB.print(event.b);
      SerialUSB.print(" txPending=");
      SerialUSB.println(event.c);
      return;
    case EvTransportTxComplete:
      SerialUSB.print("Transport tx complete txLen=");
      SerialUSB.print(event.a);
      SerialUSB.print(" txIndex=");
      SerialUSB.print(event.b);
      SerialUSB.print(" txPending=");
      SerialUSB.println(event.c);
      return;
    case EvLinkRxFrame:
      SerialUSB.print("Link rx frame=");
      break;
    case EvLinkCrcError:
      SerialUSB.print("Link CRC fail calc=");
      printHexByte(event.a);
      SerialUSB.print(" rx=");
      printHexByte(event.b);
      SerialUSB.print(" bytes=");
      break;
    case EvServiceHandle:
      SerialUSB.print("Service request tlv=");
      break;
    case EvLinkResponseBuilt:
      SerialUSB.print("Link response built crc=");
      printHexByte(event.a);
      SerialUSB.print(" bytes=");
      break;
    default:
      SerialUSB.println("Unknown diag event");
      return;
  }

  if (event.len == 0) {
    SerialUSB.println("<empty>");
    return;
  }

  for (uint8_t index = 0; index < event.len; index++) {
    if (index > 0) SerialUSB.print(' ');
    printHexByte(event.data[index]);
  }
  SerialUSB.println();
}

void DiagTrace::printHexByte(uint8_t value) {
  if (value < 0x10) SerialUSB.print('0');
  SerialUSB.print(value, HEX);
}
