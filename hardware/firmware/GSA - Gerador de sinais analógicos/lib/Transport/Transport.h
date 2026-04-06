#pragma once
#include <stdint.h>
#include "defs.h"

class Transport {
public:
  void begin(uint8_t i2cAddr);
  static void resumeSlave(uint8_t i2cAddr);
  static bool hasTxPending();
  static bool hasAsyncEventTxPending();
  static void setIrqActive(bool active);

  // Chamar no loop para pegar frame recebido (se houver)
  bool popRx(uint8_t* out, uint8_t& outLen);

  // Prepara a proxima resposta a ser lida pela BPM.
  // Para comandos de escrita, esse payload deve representar apenas o aceite
  // síncrono; o resultado físico final é publicado depois por IRQ + evento.
  void setTx(const uint8_t* data, uint8_t len, bool isAsyncEvent = false);

private:
  static void onReceiveThunk(int count);
  static void onRequestThunk();

  static volatile uint8_t _rxBuf[TLV_MAX_LEN];
  static volatile uint8_t _rxLen;
  static volatile bool    _rxPending;

  static volatile uint8_t _txBuf[TLV_MAX_LEN];
  static volatile uint8_t _txLen;
  static volatile bool    _txIsAsyncEvent;
  static bool             _irqActive;

  static Transport* _self;
};
