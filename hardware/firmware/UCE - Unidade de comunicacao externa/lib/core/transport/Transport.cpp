#include "core/transport/Transport.h"

#include <Arduino.h>

#include "config.h"
#include "diag/trace/DiagTrace.h"

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
volatile uint16_t Transport::_txLen = 0;
volatile uint16_t Transport::_txSentCount = 0;
volatile bool Transport::_txActive = false;
volatile bool Transport::_txPrimed = false;
volatile uint8_t Transport::_txPrimedByte = 0;

volatile uint8_t Transport::_mode = Transport::ModeIdle;

void Transport::begin() {
  _self = this;
  _rxWorkLen = 0;
  _rxLen = 0;
  _rxPending = false;
  _txLen = 0;
  _txSentCount = 0;
  _txActive = false;
  _txPrimed = false;
  _txPrimedByte = 0;
  _mode = ModeIdle;

  pinMode(UCE_IRQ_PIN, OUTPUT);
  digitalWrite(UCE_IRQ_PIN, UCE_IRQ_IDLE_LEVEL);

  pmc_enable_periph_clk(ID_SPI0);

  PIOA->PIO_PDR = kUceSpiSignalPins;
  PIOA->PIO_ABSR &= ~kUceSpiSignalPins;
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
  for (uint8_t index = 0; index < count; ++index) {
    out[index] = _rxBuf[index];
  }
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
  if (_txActive) {
    interrupts();
    return;
  }
  for (uint8_t index = 0; index < len; ++index) {
    _txBuf[index] = data[index];
  }
  _txLen = len;
  _txSentCount = 0;
  _txActive = true;
  _txPrimed = false;
  _txPrimedByte = 0;
  _mode = ModeSending;
  primeTxForCurrentPosition();
  interrupts();

  DiagTrace::logBytes(DiagTrace::EvTransportSetTx, (const uint8_t*)_txBuf, len);
  DiagTrace::logState(DiagTrace::EvTransportPreload, _txPrimedByte, (uint8_t)_txSentCount, _txActive ? 1 : 0);
  setIrqActive(true);
}

void Transport::setIrqActive(bool active) {
  digitalWrite(UCE_IRQ_PIN, active ? UCE_IRQ_ACTIVE_LEVEL : UCE_IRQ_IDLE_LEVEL);
}

void Transport::clearTxState() {
  const uint16_t previousLen = _txLen;
  SPI0->SPI_IDR = SPI_IDR_TDRE;
  _txActive = false;
  _txLen = 0;
  _txSentCount = 0;
  _txPrimed = false;
  _txPrimedByte = 0;
  for (uint16_t index = 0; index < previousLen && index < TLV_MAX_LEN; ++index) {
    _txBuf[index] = 0;
  }
  SPI0->SPI_TDR = 0;
}

void Transport::primeTxByte(uint8_t value) {
  _txPrimedByte = value;
  _txPrimed = true;
  SPI0->SPI_TDR = value;
}

void Transport::primeTxForCurrentPosition() {
  if (!_txActive || _txSentCount >= _txLen) {
    _txPrimed = false;
    _txPrimedByte = 0;
    SPI0->SPI_TDR = 0;
    return;
  }

  primeTxByte(_txBuf[_txSentCount]);
}

void Transport::rearmTxForNextBurst() {
  if (!_txActive || _txSentCount >= _txLen) return;

  // O SPI slave atual responde em mais de um burst. Reescrevemos
  // explicitamente o primeiro byte pendente para o proximo burst em vez de
  // confiar que o valor residual do TDR sobrevivera ao NSSR.
  primeTxForCurrentPosition();
  DiagTrace::logState(DiagTrace::EvTransportPreload, _txPrimedByte, (uint8_t)_txSentCount, _txActive ? 1 : 0);
}

void Transport::onSpiInterrupt() {
  const uint32_t status = SPI0->SPI_SR;

  if (status & SPI_SR_RDRF) {
    const uint8_t value = (uint8_t)(SPI0->SPI_RDR & 0xFF);

    if (_txActive) {
      _mode = ModeSending;
      // Durante TX ativo, os bytes recebidos na MOSI sao apenas dummy bytes do
      // master. Eles nao podem iniciar um novo request.
      if (_txPrimed && _txSentCount < _txLen) {
        ++_txSentCount;
      }

      if (_txSentCount < _txLen) {
        primeTxForCurrentPosition();
        DiagTrace::logState(DiagTrace::EvTransportAdvance, _txPrimedByte, (uint8_t)_txSentCount, _txActive ? 1 : 0);
      } else {
        _txPrimed = false;
        _txPrimedByte = 0;
        SPI0->SPI_TDR = 0;
      }
    } else {
      if (_mode != ModeReceiving) {
        _mode = ModeReceiving;
        _rxWorkLen = 0;
      }

      if (_rxWorkLen < TLV_MAX_LEN) _rxWorkBuf[_rxWorkLen++] = value;
      SPI0->SPI_TDR = 0;
    }
  }

  if (status & SPI_SR_TDRE) {
    SPI0->SPI_IDR = SPI_IDR_TDRE;
  }

  if (status & SPI_SR_NSSR) {
    if (_mode == ModeReceiving && _rxWorkLen > 0) {
      const uint8_t count = _rxWorkLen;
      for (uint8_t index = 0; index < count; ++index) {
        _rxBuf[index] = _rxWorkBuf[index];
      }
      _rxLen = count;
      _rxPending = true;
      _rxWorkLen = 0;
    }

    if (_txActive && _txSentCount >= _txLen) {
      const uint16_t txLen = _txLen;
      const uint16_t txSentCount = _txSentCount;
      clearTxState();
      setIrqActive(false);
      DiagTrace::logState(DiagTrace::EvTransportTxComplete, (uint8_t)txLen, (uint8_t)txSentCount, 0);
    } else if (_txActive) {
      rearmTxForNextBurst();
    }

    _mode = _txActive ? ModeSending : ModeIdle;
  }
}

extern "C" void SPI0_Handler(void) {
  if (Transport::_self) {
    Transport::onSpiInterrupt();
  }
}
