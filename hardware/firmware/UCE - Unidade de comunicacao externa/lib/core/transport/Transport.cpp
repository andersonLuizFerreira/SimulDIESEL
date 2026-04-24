#include "core/transport/Transport.h"

#include <Arduino.h>

#include "config.h"
#include "services/led/LedService.h"

namespace {
constexpr uint32_t kUceSpiSignalPins =
    PIO_PA25A_SPI0_MISO |
    PIO_PA26A_SPI0_MOSI |
    PIO_PA27A_SPI0_SPCK |
    PIO_PA28A_SPI0_NPCS0;

LedService* g_ledService = nullptr;

uint8_t crc8(const uint8_t* data, uint8_t len) {
  uint8_t crc = 0x00;
  for (uint8_t index = 0; index < len; ++index) {
    crc ^= data[index];
    for (uint8_t bit = 0; bit < 8; ++bit) {
      crc = (crc & 0x80U) ? (uint8_t)((crc << 1U) ^ 0x07U) : (uint8_t)(crc << 1U);
    }
  }
  return crc;
}
}

Transport* Transport::_self = nullptr;
volatile uint8_t Transport::_rxBuf[TLV_MAX_LEN];
volatile uint16_t Transport::_rxLen = 0;
volatile uint8_t Transport::_txBuf[TLV_MAX_LEN];
volatile uint16_t Transport::_txLen = 0;
volatile uint16_t Transport::_txIndex = 0;
volatile bool Transport::_responsePrepared = false;
volatile bool Transport::_sessionActive = false;

void Transport::begin() {
  _self = this;
  _rxLen = 0;
  _txLen = 0;
  _txIndex = 0;
  _responsePrepared = false;
  _sessionActive = false;

  if (g_ledService == nullptr) {
    static LedService ledService;
    g_ledService = &ledService;
    g_ledService->begin();
  }

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

  attachInterrupt(digitalPinToInterrupt(UCE_SPI_CS_PIN), Transport::_csThunk, FALLING);
}

void Transport::primeTxByte() {
  if (_txIndex >= _txLen) {
    SPI0->SPI_TDR = 0;
    return;
  }
  SPI0->SPI_TDR = _txBuf[_txIndex];
}

bool Transport::validateLedRequest() {
  return _rxLen == 4 &&
         _rxBuf[0] == CMD_LED_BUILTIN &&
         _rxBuf[1] == 0x01 &&
         (_rxBuf[2] == 0x00 || _rxBuf[2] == 0x01) &&
         crc8((const uint8_t*)_rxBuf, 3) == _rxBuf[3];
}

void Transport::buildLedResponse(bool state) {
  _txBuf[0] = CMD_LED_BUILTIN;
  _txBuf[1] = 0x01;
  _txBuf[2] = state ? 0x01 : 0x00;
  _txBuf[3] = crc8((const uint8_t*)_txBuf, 3);
  _txLen = 4;
  _txIndex = 0;
  _responsePrepared = true;
}

void Transport::buildFunctionalError(uint8_t requestType, uint8_t errorCode) {
  _txBuf[0] = CMD_FUNCTIONAL_ERROR;
  _txBuf[1] = 0x03;
  _txBuf[2] = requestType;
  _txBuf[3] = 0x00;
  _txBuf[4] = errorCode;
  _txBuf[5] = crc8((const uint8_t*)_txBuf, 5);
  _txLen = 6;
  _txIndex = 0;
  _responsePrepared = true;
}

void Transport::onCsFalling() {
  _rxLen = 0;
  _txLen = 0;
  _txIndex = 0;
  _responsePrepared = false;
  _sessionActive = true;
  digitalWrite(UCE_SPI_IRQ_PIN, UCE_SPI_IRQ_IDLE_LEVEL);
  SPI0->SPI_TDR = 0;
}

void Transport::onSpiInterrupt() {
  const uint32_t status = SPI0->SPI_SR;

  if (status & SPI_SR_RDRF) {
    const uint8_t value = (uint8_t)(SPI0->SPI_RDR & 0xFF);

    if (_rxLen < TLV_MAX_LEN) {
      _rxBuf[_rxLen++] = value;
    }

    if (_rxLen == 4 && !_responsePrepared) {
      if (validateLedRequest()) {
        const bool requestedState = _rxBuf[2] != 0;
        if (g_ledService != nullptr) {
          g_ledService->set(requestedState);
        }
        buildLedResponse(requestedState);
      } else {
        const uint8_t requestType = (_rxLen > 0) ? _rxBuf[0] : 0x00;
        buildFunctionalError(requestType, UCE_ERROR_COMMAND_NOT_SUPPORTED);
      }
      primeTxByte();
      digitalWrite(UCE_SPI_IRQ_PIN, UCE_SPI_IRQ_ACTIVE_LEVEL);
      return;
    }

    if (_responsePrepared && _txLen > 0) {
      ++_txIndex;
      if (_txIndex < _txLen) {
        primeTxByte();
      } else {
        SPI0->SPI_TDR = 0;
      }
      return;
    }

    SPI0->SPI_TDR = 0;
  }

  if (status & SPI_SR_NSSR) {
    _sessionActive = false;
    _txLen = 0;
    _txIndex = 0;
    _responsePrepared = false;
    digitalWrite(UCE_SPI_IRQ_PIN, UCE_SPI_IRQ_IDLE_LEVEL);
    SPI0->SPI_TDR = 0;
  }
}

void Transport::_csThunk() {
  if (_self) {
    _self->onCsFalling();
  }
}

extern "C" void SPI0_Handler(void) {
  if (Transport::_self) {
    Transport::onSpiInterrupt();
  }
}
