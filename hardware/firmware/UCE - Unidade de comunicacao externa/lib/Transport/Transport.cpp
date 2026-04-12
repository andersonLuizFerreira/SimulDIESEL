#include <Arduino.h>
#include "Transport.h"
#include "config.h"
#include "DiagTrace.h"

namespace {
constexpr uint32_t kUceSpiSignalPins =
    PIO_PA25A_SPI0_MISO |
    PIO_PA26A_SPI0_MOSI |
    PIO_PA27A_SPI0_SPCK |
    PIO_PA28A_SPI0_NPCS0;

constexpr uint32_t kUceSpiNativeCsPin = PIO_PA28A_SPI0_NPCS0;
}

Transport* Transport::_self = nullptr;

volatile uint8_t Transport::_rxWorkBuf[TLV_MAX_LEN];
volatile uint8_t Transport::_rxWorkLen = 0;
volatile uint8_t Transport::_rxBuf[TLV_MAX_LEN];
volatile uint8_t Transport::_rxLen = 0;
volatile bool Transport::_rxPending = false;

volatile uint8_t Transport::_txBuf[TLV_MAX_LEN];
volatile uint8_t Transport::_txLen = 0;
volatile uint8_t Transport::_txIndex = 0;
volatile bool Transport::_txPending = false;

volatile uint8_t Transport::_mode = Transport::ModeIdle;

void Transport::begin() {
  _self = this;
  _rxWorkLen = 0;
  _rxLen = 0;
  _rxPending = false;
  _txLen = 0;
  _txIndex = 0;
  _txPending = false;
  _mode = ModeIdle;

  pinMode(UCE_IRQ_PIN, OUTPUT);
  digitalWrite(UCE_IRQ_PIN, UCE_IRQ_IDLE_LEVEL);

  pmc_enable_periph_clk(ID_SPI0);

  PIOA->PIO_PDR = kUceSpiSignalPins;
  PIOA->PIO_ABSR &= ~kUceSpiSignalPins;
  // Keep NPCS0 under SPI0 peripheral control while biasing the idle level high.
  PIOA->PIO_PUER = kUceSpiNativeCsPin;

  SPI0->SPI_CR = SPI_CR_SWRST;
  SPI0->SPI_CR = SPI_CR_SWRST;
  SPI0->SPI_MR = 0;
  SPI0->SPI_CSR[0] = SPI_CSR_NCPHA | SPI_CSR_BITS_8_BIT;
  SPI0->SPI_IDR = 0xFFFFFFFF;
  SPI0->SPI_TDR = 0;
  SPI0->SPI_IER = SPI_IER_RDRF | SPI_IER_NSSR;

  NVIC_ClearPendingIRQ(SPI0_IRQn);
  NVIC_EnableIRQ(SPI0_IRQn);

  SPI0->SPI_CR = SPI_CR_SPIEN;
  DiagTrace::logState(DiagTrace::EvTransportBegin, 0, 0, 0);
}

bool Transport::popRx(uint8_t* out, uint8_t& outLen) {
  if (!_rxPending || !out) return false;

  noInterrupts();
  uint8_t count = _rxLen;
  if (count > TLV_MAX_LEN) count = TLV_MAX_LEN;
  for (uint8_t index = 0; index < count; index++) out[index] = _rxBuf[index];
  _rxPending = false;
  interrupts();

  outLen = count;
  DiagTrace::logBytes(DiagTrace::EvTransportRequestCaptured, out, count);
  return true;
}

void Transport::setTx(const uint8_t* data, uint8_t len) {
  if (!data || len == 0) return;
  if (len > TLV_MAX_LEN) len = TLV_MAX_LEN;

  noInterrupts();
  SPI0->SPI_IDR = SPI_IDR_TDRE;
  for (uint8_t index = 0; index < len; index++) _txBuf[index] = data[index];
  _txLen = len;
  _txIndex = 1;
  _txPending = true;
  _mode = ModeSending;
  SPI0->SPI_TDR = _txBuf[0];
  SPI0->SPI_IER = SPI_IER_TDRE;
  interrupts();

  DiagTrace::logBytes(DiagTrace::EvTransportSetTx, (const uint8_t*)_txBuf, len);
  DiagTrace::logState(DiagTrace::EvTransportPreload, _txBuf[0], _txIndex, _txPending ? 1 : 0);
  setIrqActive(true);
}

void Transport::setIrqActive(bool active) {
  digitalWrite(UCE_IRQ_PIN, active ? UCE_IRQ_ACTIVE_LEVEL : UCE_IRQ_IDLE_LEVEL);
}

void Transport::clearTxState() {
  const uint8_t previousLen = _txLen;
  SPI0->SPI_IDR = SPI_IDR_TDRE;
  _txPending = false;
  _txLen = 0;
  _txIndex = 0;
  for (uint8_t index = 0; index < previousLen && index < TLV_MAX_LEN; index++) {
    _txBuf[index] = 0;
  }
  SPI0->SPI_TDR = 0;
}

void Transport::loadNextTxByte() {
  if (_txIndex >= _txLen) return;

  const uint8_t nextByte = _txBuf[_txIndex++];
  SPI0->SPI_TDR = nextByte;
  DiagTrace::logState(DiagTrace::EvTransportAdvance, nextByte, _txIndex, _txPending ? 1 : 0);
}

void Transport::onSpiInterrupt() {
  const uint32_t status = SPI0->SPI_SR;

  if (status & SPI_SR_TDRE) {
    if (_txPending && _txIndex < _txLen) {
      loadNextTxByte();
    } else {
      SPI0->SPI_IDR = SPI_IDR_TDRE;
    }
  }

  if (status & SPI_SR_RDRF) {
    const uint8_t value = (uint8_t)(SPI0->SPI_RDR & 0xFF);

    if (_txPending) {
      _mode = ModeSending;
    } else {
      if (_mode != ModeReceiving) {
        _mode = ModeReceiving;
        _rxWorkLen = 0;
      }

      if (_rxWorkLen < TLV_MAX_LEN) _rxWorkBuf[_rxWorkLen++] = value;
      SPI0->SPI_TDR = 0;
    }
  }

  if (status & SPI_SR_NSSR) {
    if (_mode == ModeReceiving && _rxWorkLen > 0) {
      const uint8_t count = _rxWorkLen;
      for (uint8_t index = 0; index < count; index++) _rxBuf[index] = _rxWorkBuf[index];
      _rxLen = count;
      _rxPending = true;
      _rxWorkLen = 0;
    }

    if (_txPending && _txIndex >= _txLen) {
      const uint8_t txLen = _txLen;
      const uint8_t txIndex = _txIndex;
      clearTxState();
      setIrqActive(false);
      DiagTrace::logState(DiagTrace::EvTransportTxComplete, txLen, txIndex, 0);
    }

    _mode = ModeIdle;
  }
}

extern "C" void SPI0_Handler(void) {
  if (Transport::_self) {
    Transport::onSpiInterrupt();
  }
}
