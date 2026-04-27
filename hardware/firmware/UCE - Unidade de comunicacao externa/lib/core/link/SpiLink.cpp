#include "core/link/SpiLink.h"

#include <Arduino.h>
#include <string.h>

#include "config.h"

namespace {
constexpr uint32_t kUceSpiSignalPins =
    PIO_PA25A_SPI0_MISO |
    PIO_PA26A_SPI0_MOSI |
    PIO_PA27A_SPI0_SPCK |
    PIO_PA28A_SPI0_NPCS0;
}

SpiLink* SpiLink::_self = nullptr;
volatile uint8_t SpiLink::_rxBuf[SpiLink::BufferSize];
volatile uint8_t SpiLink::_txBuf[SpiLink::BufferSize];
volatile uint16_t SpiLink::_index = 0;
volatile bool SpiLink::_rxReady = false;
volatile bool SpiLink::_txPending = false;
volatile bool SpiLink::_transferActive = false;

void SpiLink::begin() {
  _self = this;
  _index = 0;
  _rxReady = false;
  _txPending = false;
  _transferActive = false;
  memset((void*)_rxBuf, 0, BufferSize);
  memset((void*)_txBuf, 0, BufferSize);

  pinMode(UCE_SPI_IRQ_PIN, OUTPUT);
  digitalWrite(UCE_SPI_IRQ_PIN, UCE_SPI_IRQ_IDLE_LEVEL);

  pmc_enable_periph_clk(ID_SPI0);
  PIOA->PIO_PDR = kUceSpiSignalPins;
  PIOA->PIO_ABSR &= ~kUceSpiSignalPins;
  PIOA->PIO_PUER = PIO_PA28A_SPI0_NPCS0;

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

  attachInterrupt(digitalPinToInterrupt(UCE_SPI_CS_PIN), SpiLink::_csThunk, FALLING);
}

bool SpiLink::available() const {
  return _rxReady;
}

bool SpiLink::txPending() const {
  return _txPending;
}

bool SpiLink::read(uint8_t* dst, size_t& len) {
  len = 0;
  if (!dst || !_rxReady) {
    return false;
  }

  noInterrupts();
  memcpy(dst, (const void*)_rxBuf, BufferSize);
  memset((void*)_rxBuf, 0, BufferSize);
  _rxReady = false;
  interrupts();

  len = BufferSize;
  return true;
}

bool SpiLink::write(const uint8_t* src, size_t len) {
  if (!src || len > BufferSize) {
    return false;
  }

  noInterrupts();
  if (_txPending || _transferActive) {
    interrupts();
    return false;
  }
  memset((void*)_txBuf, 0, BufferSize);
  memcpy((void*)_txBuf, src, len);
  _txPending = true;
  interrupts();

  pulseAttention();
  return true;
}

void SpiLink::primeTxByte() {
  SPI0->SPI_TDR = (_index < BufferSize) ? _txBuf[_index] : 0;
}

void SpiLink::setIrqReady(bool ready) {
  digitalWrite(UCE_SPI_IRQ_PIN, ready ? UCE_SPI_IRQ_ACTIVE_LEVEL : UCE_SPI_IRQ_IDLE_LEVEL);
}

void SpiLink::pulseAttention() {
  digitalWrite(UCE_SPI_IRQ_PIN, UCE_SPI_IRQ_IDLE_LEVEL);
  delayMicroseconds(2);
  digitalWrite(UCE_SPI_IRQ_PIN, UCE_SPI_IRQ_ACTIVE_LEVEL);
  delayMicroseconds(20);
  digitalWrite(UCE_SPI_IRQ_PIN, UCE_SPI_IRQ_IDLE_LEVEL);
}

void SpiLink::onCsFalling() {
  _index = 0;
  _transferActive = true;
  primeTxByte();
  setIrqReady(true);
}

void SpiLink::onSpiInterrupt() {
  const uint32_t status = SPI0->SPI_SR;

  if (status & SPI_SR_RDRF) {
    const uint8_t value = (uint8_t)(SPI0->SPI_RDR & 0xFF);
    if (_transferActive && _index < BufferSize) {
      _rxBuf[_index] = value;
      ++_index;
      primeTxByte();
    } else {
      SPI0->SPI_TDR = 0;
    }
  }

  if (status & SPI_SR_NSSR) {
    _transferActive = false;
    _rxReady = (_index == BufferSize);
    if (_txPending) {
      _txPending = false;
      _txBuf[0] = 0;
    }
    _index = 0;
    setIrqReady(false);
    SPI0->SPI_TDR = 0;
  }
}

void SpiLink::_csThunk() {
  if (_self) {
    _self->onCsFalling();
  }
}

extern "C" void SPI0_Handler(void) {
  if (SpiLink::_self) {
    SpiLink::onSpiInterrupt();
  }
}
