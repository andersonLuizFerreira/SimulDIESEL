# Dump técnico do código SPI BPM ↔ UCE

Material gerado diretamente do workspace para auditoria manual externa. O foco foi manter blocos contínuos, com caminho absoluto, intervalo de linhas e contexto suficiente para análise linha a linha da pilha SPI BPM ↔ UCE.

## BLOCO 1 — BPM / SPI MASTER

Arquivos incluídos:
- `C:\PROJETOS\SimulDIESEL\hardware\firmware\BPM - BACKPLANE MANAGER MODULE\lib\GwSpiBus\GwSpiBus.h`
- `C:\PROJETOS\SimulDIESEL\hardware\firmware\BPM - BACKPLANE MANAGER MODULE\lib\GwSpiBus\GwSpiBus.cpp`

Dump do master SPI da BPM: seleção da board, escrita do request, espera por IRQ, leitura em dois bursts, validação de header/comprimento, CRC e snapshot diagnóstico.

Arquivo: `C:\PROJETOS\SimulDIESEL\hardware\firmware\BPM - BACKPLANE MANAGER MODULE\lib\GwSpiBus\GwSpiBus.h`  
Trecho: linhas 1-47  
Papel: Interface pública/privada do barramento SPI master, enums auxiliares e contrato do snapshot.

```cpp
#pragma once
#include "GwBus.h"
#include "GwSpiDiagnostic.h"
#include <SPI.h>

class GwSpiBus : public IGwBus {
public:
    enum class TransactError : uint8_t {
        None = 0,
        AddrUnmapped,
        WrongBus,
        MissingCs,
        TimeoutWaitingIrq,
        HeaderInvalid,
        LengthInvalid,
        FrameIncomplete,
    };

    explicit GwSpiBus(SPIClass& spi = SPI)
        : _spi(spi), _ok(true), _hz(8000000UL), _sckPin(-1), _misoPin(-1), _mosiPin(-1), _lastError(TransactError::None) {}

    void begin(uint32_t hz, int8_t sckPin, int8_t misoPin, int8_t mosiPin);

    bool transact(uint8_t addr,
                  const uint8_t* tx, size_t txLen,
                  uint8_t* rx, size_t rxMax, size_t& rxLen,
                  uint16_t timeoutMs) override;

    bool isOk() const override { return _ok; }
    const GwSpiDiagnostic::Snapshot& lastSnapshot() const { return _lastSnapshot; }
    TransactError lastError() const { return _lastError; }

private:
    SPIClass& _spi;
    bool _ok;
    uint32_t _hz;
    int8_t _sckPin;
    int8_t _misoPin;
    int8_t _mosiPin;
    GwSpiDiagnostic::Snapshot _lastSnapshot;
    TransactError _lastError;

    void csLow(int cs);
    void csHigh(int cs);
    void resetSnapshot(uint8_t addr);
    void captureBytes(uint8_t* dest, const uint8_t* src, size_t len, uint8_t& lenOut);
};
```

Arquivo: `C:\PROJETOS\SimulDIESEL\hardware\firmware\BPM - BACKPLANE MANAGER MODULE\lib\GwSpiBus\GwSpiBus.cpp`  
Trecho: linhas 1-173  
Papel: Implementação do fluxo de transação SPI master, bursts de leitura, validações e preenchimento do snapshot.

```cpp
#include <Arduino.h>
#include "GwSpiBus.h"
#include "GwDeviceTable.h"
#include "GwTlv.h"

namespace {
constexpr uint8_t kCurrentUceMinPayloadLen = 1;
constexpr uint8_t kCurrentUceMaxPayloadLen = (uint8_t)(GwSpiDiagnostic::kMaxFrameBytes - 3);
}

void GwSpiBus::csLow(int cs){ digitalWrite(cs, LOW); }
void GwSpiBus::csHigh(int cs){ digitalWrite(cs, HIGH); }

void GwSpiBus::resetSnapshot(uint8_t addr)
{
    _lastSnapshot = GwSpiDiagnostic::Snapshot{};
    _lastSnapshot.valid = true;
    _lastSnapshot.addr = addr;
    _lastSnapshot.layer = GwSpiDiagnostic::LayerGwSpiBus;
}

void GwSpiBus::captureBytes(uint8_t* dest, const uint8_t* src, size_t len, uint8_t& lenOut)
{
    lenOut = 0;
    if (!dest || !src) return;

    const size_t count = (len > GwSpiDiagnostic::kMaxFrameBytes)
        ? GwSpiDiagnostic::kMaxFrameBytes
        : len;

    for (size_t index = 0; index < count; index++) {
        dest[index] = src[index];
    }

    lenOut = (uint8_t)count;
}

void GwSpiBus::begin(uint32_t hz, int8_t sckPin, int8_t misoPin, int8_t mosiPin)
{
    _hz = hz;
    _sckPin = sckPin;
    _misoPin = misoPin;
    _mosiPin = mosiPin;

    // Inicializa o barramento SPI com pinagem explicita para evitar o
    // mapeamento padrao do ESP32, que conflita com o reset global em GPIO23.
    _spi.begin(_sckPin, _misoPin, _mosiPin, -1);
}

bool GwSpiBus::transact(uint8_t addr,
                        const uint8_t* tx, size_t txLen,
                        uint8_t* rx, size_t rxMax, size_t& rxLen,
                        uint16_t timeoutMs)
{
    rxLen = 0;
    _lastError = TransactError::None;
    resetSnapshot(addr);
    captureBytes(_lastSnapshot.tx, tx, txLen, _lastSnapshot.txLen);
    _lastSnapshot.phase = GwSpiDiagnostic::PhaseWrite;

    GwDeviceEntry e{};
    if (!GwDeviceTable::get(addr, e)) {
        _lastError = TransactError::AddrUnmapped;
        return false;
    }
    if (e.bus != GW_BUS_SPI) {
        _lastError = TransactError::WrongBus;
        return false;
    }
    if (e.spiCsPin < 0) {
        _lastError = TransactError::MissingCs;
        return false;
    }

    const int cs = e.spiCsPin;
    pinMode(cs, OUTPUT);
    csHigh(cs);

    SPISettings st(_hz, SPI_MSBFIRST, SPI_MODE0);

    // FASE 1: WRITE
    _spi.beginTransaction(st);
    csLow(cs);
    for (size_t i = 0; i < txLen; i++) _spi.transfer(tx[i]);
    csHigh(cs);
    _spi.endTransaction();

    // A UCE sinaliza resposta pronta via IRQ, mas a leitura continua
    // síncrona e curta em dois bursts SPI para preservar o contrato atual.
    uint32_t t0 = millis();
    if (e.spiUseIrq && e.spiIrqPin >= 0) {
        _lastSnapshot.phase = GwSpiDiagnostic::PhaseWaitResponseReady;
        pinMode(e.spiIrqPin, INPUT_PULLUP);
        while (digitalRead(e.spiIrqPin) == HIGH) {
            if ((uint32_t)(millis() - t0) > timeoutMs) {
                _lastSnapshot.cause = GwSpiDiagnostic::CauseTimeoutWaitingIrq;
                _lastError = TransactError::TimeoutWaitingIrq;
                return false;
            }
            delay(1);
        }
    } else {
        delay(1);
    }

    // FASE 2: READ header (CMD, LEN)
    uint8_t hdr[2] = {0,0};

    _lastSnapshot.phase = GwSpiDiagnostic::PhaseReadHeader;
    _spi.beginTransaction(st);
    csLow(cs);
    hdr[0] = _spi.transfer(0x00);
    hdr[1] = _spi.transfer(0x00);
    csHigh(cs);
    _spi.endTransaction();

    _lastSnapshot.rx[0] = hdr[0];
    _lastSnapshot.rx[1] = hdr[1];
    _lastSnapshot.rxLen = 2;
    _lastSnapshot.receivedLen = 2;

    if (hdr[0] == 0x00) {
        _lastSnapshot.cause = GwSpiDiagnostic::CauseFirstByteMisaligned;
        _lastError = TransactError::HeaderInvalid;
        return false;
    }

    const uint8_t len = hdr[1];
    if (len < kCurrentUceMinPayloadLen) {
        _lastSnapshot.cause = GwSpiDiagnostic::CauseLengthMismatch;
        _lastError = TransactError::HeaderInvalid;
        return false;
    }
    if (len > kCurrentUceMaxPayloadLen) {
        _lastSnapshot.cause = GwSpiDiagnostic::CauseLengthMismatch;
        _lastError = TransactError::LengthInvalid;
        return false;
    }

    const size_t total = (size_t)2 + (size_t)len + (size_t)1;
    _lastSnapshot.expectedLen = (uint8_t)total;
    if (total > rxMax) {
        _lastSnapshot.cause = GwSpiDiagnostic::CauseLengthMismatch;
        _lastError = TransactError::LengthInvalid;
        return false;
    }

    rx[0] = hdr[0];
    rx[1] = hdr[1];

    // READ payload + CRC
    _lastSnapshot.phase = GwSpiDiagnostic::PhaseReadPayload;
    _spi.beginTransaction(st);
    csLow(cs);
    for (size_t i = 0; i < (size_t)len + 1; i++) {
        rx[2 + i] = _spi.transfer(0x00);
        if ((size_t)(2 + i) < GwSpiDiagnostic::kMaxFrameBytes) {
            _lastSnapshot.rx[2 + i] = rx[2 + i];
        }
    }
    csHigh(cs);
    _spi.endTransaction();

    rxLen = total;
    _lastSnapshot.rxLen = (uint8_t)((total > GwSpiDiagnostic::kMaxFrameBytes)
        ? GwSpiDiagnostic::kMaxFrameBytes
        : total);
    _lastSnapshot.receivedLen = (uint8_t)total;
    if (total >= 1) _lastSnapshot.crcReceived = rx[total - 1];
    if (total >= 3) _lastSnapshot.crcCalculated = GwTlv::crc8(rx, total - 1);

    return true;
}
```

## BLOCO 2 — BPM / ROTEAMENTO E ERROS

Arquivos incluídos:
- `C:\PROJETOS\SimulDIESEL\hardware\firmware\BPM - BACKPLANE MANAGER MODULE\lib\GwRouter\GwRouter.h`
- `C:\PROJETOS\SimulDIESEL\hardware\firmware\BPM - BACKPLANE MANAGER MODULE\lib\GwRouter\GwRouter.cpp`
- `C:\PROJETOS\SimulDIESEL\hardware\firmware\BPM - BACKPLANE MANAGER MODULE\lib\GwErr\GwErr.h`
- `C:\PROJETOS\SimulDIESEL\hardware\firmware\BPM - BACKPLANE MANAGER MODULE\lib\GwDiag\GwSpiDiagnostic.h`
- `C:\PROJETOS\SimulDIESEL\hardware\firmware\BPM - BACKPLANE MANAGER MODULE\lib\GwDeviceTable\GwDeviceTable.h`
- `C:\PROJETOS\SimulDIESEL\hardware\firmware\BPM - BACKPLANE MANAGER MODULE\lib\GwDeviceTable\GwDeviceTable.cpp`
- `C:\PROJETOS\SimulDIESEL\hardware\firmware\BPM - BACKPLANE MANAGER MODULE\include\SdgwDefs.h`

Trechos que publicam a UCE como device SPI, roteiam comandos SDGW para o barramento e classificam os erros/diagnósticos associados.

Arquivo: `C:\PROJETOS\SimulDIESEL\hardware\firmware\BPM - BACKPLANE MANAGER MODULE\lib\GwRouter\GwRouter.h`  
Trecho: linhas 1-37  
Papel: Contrato do roteador e helpers de mapeamento de erro SPI para erro de gateway.

```cpp
#pragma once
#include <stdint.h>
#include <stddef.h>

#include "GwBus.h"
#include "GwErr.h"
#include "GwSpiDiagnostic.h"

class GwSpiBus;

class GwRouter {
public:
    GwRouter(IGwBus& i2c, IGwBus& spi)
    : _i2c(i2c), _spi(spi), _lastDiag() {}

    // Roteia uma requisicao para a baby board externa indicada por
    // GW_CMD_ADDR(cmd). A BPM local (ADDR 0) nao passa por aqui.
    // Entrada e saida sao os payloads internos da baby board (TLV+CRC).
    GwErr route(uint8_t cmd,
                const uint8_t* reqTlv, uint8_t reqTlvLen,
                uint8_t* respBuf, size_t respMax, size_t& respLen,
                uint16_t timeoutMs);

    bool pollGsaEvent(uint8_t* respBuf, size_t respMax, size_t& respLen);
    bool buildGatewayErrorPayload(uint8_t cmd, GwErr err, uint8_t* out, size_t outMax, size_t& outLen) const;

private:
    void resetLastDiag();
    void captureSpiDiag(uint8_t addr, GwErr err, const GwSpiDiagnostic::Snapshot& snapshot);
    void finalizeSpiCrcDiag(uint8_t addr, GwErr err, const uint8_t* respBuf, size_t respLen, bool spiUseIrq);
    uint8_t detectPossibleCause(const GwSpiDiagnostic::Snapshot& snapshot, bool spiUseIrq) const;
    GwErr mapSpiTransactFailure(const GwSpiBus& spiBus, bool spiUseIrq) const;

    IGwBus& _i2c;
    IGwBus& _spi;
    GwSpiDiagnostic::Snapshot _lastDiag;
};
```

Arquivo: `C:\PROJETOS\SimulDIESEL\hardware\firmware\BPM - BACKPLANE MANAGER MODULE\lib\GwRouter\GwRouter.cpp`  
Trecho: linhas 1-230  
Papel: Roteamento efetivo das operações para SPI/I2C e classificação do retorno da UCE.

```cpp
#include "GwRouter.h"
#include "GwDeviceTable.h"
#include "GwSpiBus.h"
#include "GwI2cBus.h"
#include "GwTlv.h"
#include "SdgwDefs.h"

void GwRouter::resetLastDiag()
{
    _lastDiag = GwSpiDiagnostic::Snapshot{};
}

void GwRouter::captureSpiDiag(uint8_t addr, GwErr err, const GwSpiDiagnostic::Snapshot& snapshot)
{
    _lastDiag = snapshot;
    _lastDiag.valid = snapshot.valid;
    _lastDiag.addr = addr;
    _lastDiag.status = (uint8_t)err;
}

GwErr GwRouter::mapSpiTransactFailure(const GwSpiBus& spiBus, bool spiUseIrq) const
{
    switch (spiBus.lastError()) {
        case GwSpiBus::TransactError::TimeoutWaitingIrq:
            return GWERR_TIMEOUT;
        case GwSpiBus::TransactError::HeaderInvalid:
            return GWERR_HEADER_INVALID;
        case GwSpiBus::TransactError::LengthInvalid:
            return GWERR_LENGTH_INVALID;
        case GwSpiBus::TransactError::FrameIncomplete:
            return GWERR_FRAME_INCOMPLETE;
        default:
            break;
    }

    const GwSpiDiagnostic::Snapshot& snapshot = spiBus.lastSnapshot();
    const uint8_t cause = detectPossibleCause(snapshot, spiUseIrq);
    switch (cause) {
        case GwSpiDiagnostic::CauseIncompleteFrame:
            return GWERR_FRAME_INCOMPLETE;
        case GwSpiDiagnostic::CauseLengthMismatch:
            return GWERR_LENGTH_INVALID;
        case GwSpiDiagnostic::CauseFirstByteMisaligned:
            return GWERR_HEADER_INVALID;
        default:
            return GWERR_TIMEOUT;
    }
}

uint8_t GwRouter::detectPossibleCause(const GwSpiDiagnostic::Snapshot& snapshot, bool spiUseIrq) const
{
    if (snapshot.phase == GwSpiDiagnostic::PhaseWaitResponseReady) {
        return spiUseIrq
            ? GwSpiDiagnostic::CauseTimeoutWaitingIrq
            : GwSpiDiagnostic::CauseEarlyReadBeforeResponseReady;
    }

    if (snapshot.expectedLen > 0 && snapshot.receivedLen < snapshot.expectedLen) {
        return GwSpiDiagnostic::CauseIncompleteFrame;
    }

    if (snapshot.expectedLen != 0 && snapshot.receivedLen != 0 && snapshot.expectedLen != snapshot.receivedLen) {
        return GwSpiDiagnostic::CauseLengthMismatch;
    }

    if (snapshot.receivedLen >= 2 && snapshot.rx[0] == 0x00 && snapshot.rx[1] != 0x00) {
        return GwSpiDiagnostic::CauseFirstByteMisaligned;
    }

    if (!spiUseIrq) {
        return GwSpiDiagnostic::CauseEarlyReadBeforeResponseReady;
    }

    return GwSpiDiagnostic::CausePreloadFailure;
}

void GwRouter::finalizeSpiCrcDiag(uint8_t addr, GwErr err, const uint8_t* respBuf, size_t respLen, bool spiUseIrq)
{
    GwSpiDiagnostic::Snapshot snapshot = _lastDiag;
    snapshot.valid = true;
    snapshot.addr = addr;
    snapshot.layer = GwSpiDiagnostic::LayerCrcValidation;
    snapshot.phase = GwSpiDiagnostic::PhaseFinalCrcValidation;
    snapshot.status = (uint8_t)err;
    snapshot.receivedLen = (uint8_t)respLen;
    snapshot.rxLen = (uint8_t)((respLen > GwSpiDiagnostic::kMaxFrameBytes)
        ? GwSpiDiagnostic::kMaxFrameBytes
        : respLen);

    for (size_t index = 0; index < snapshot.rxLen; index++) {
        snapshot.rx[index] = respBuf[index];
    }

    if (respLen >= 2) {
        snapshot.expectedLen = (uint8_t)(2 + respBuf[1] + 1);
    }
    if (respLen >= 1) {
        snapshot.crcReceived = respBuf[respLen - 1];
    }
    if (respLen >= 3) {
        snapshot.crcCalculated = GwTlv::crc8(respBuf, respLen - 1);
    }
    snapshot.cause = detectPossibleCause(snapshot, spiUseIrq);
    _lastDiag = snapshot;
}

GwErr GwRouter::route(uint8_t cmd,
                      const uint8_t* reqTlv, uint8_t reqTlvLen,
                      uint8_t* respBuf, size_t respMax, size_t& respLen,
                      uint16_t timeoutMs)
{
    respLen = 0;
    resetLastDiag();

    const uint8_t addr = GW_CMD_ADDR(cmd);
    if (addr == GW_ADDR_BPM) return GWERR_ADDR_UNMAPPED; // nao roteia BPM local

    GwDeviceEntry e{};
    if (!GwDeviceTable::get(addr, e)) return GWERR_ADDR_UNMAPPED;

    // O host resolve o contrato interno da board; o gateway apenas roteia.
    if (!GwTlv::validatePacket(reqTlv, reqTlvLen))
        return GWERR_BAD_FRAME;

    // Escolha concreta hoje:
    // GSA (0x1) -> I2C
    // UCE (0x2) -> SPI
    IGwBus* bus = nullptr;
    switch (e.bus) {
        case GW_BUS_I2C:
            bus = &_i2c;
            break;
        case GW_BUS_SPI:
            bus = &_spi;
            break;
        default:
            return GWERR_BUS_DOWN;
    }

    if (!bus->isOk()) {
        // ainda pode tentar, mas aqui sinaliza já
        // (ou você decide tentar mesmo assim)
    }

    size_t rxLen = 0;
    if (!bus->transact(addr, reqTlv, reqTlvLen, respBuf, respMax, rxLen, timeoutMs)) {
        if (e.bus == GW_BUS_SPI) {
            GwSpiBus* spiBus = static_cast<GwSpiBus*>(&_spi);
            const GwErr mappedErr = mapSpiTransactFailure(*spiBus, e.spiUseIrq);
            captureSpiDiag(addr, mappedErr, spiBus->lastSnapshot());
            if (_lastDiag.cause == GwSpiDiagnostic::CauseUnknown) {
                _lastDiag.cause = detectPossibleCause(_lastDiag, e.spiUseIrq);
            }
            return mappedErr;
        }
        return GWERR_TIMEOUT;
    }

    if (!GwTlv::validatePacket(respBuf, rxLen)) {
        if (e.bus == GW_BUS_SPI) {
            GwSpiBus* spiBus = static_cast<GwSpiBus*>(&_spi);
            captureSpiDiag(addr, GWERR_BAD_CRC, spiBus->lastSnapshot());
            finalizeSpiCrcDiag(addr, GWERR_BAD_CRC, respBuf, rxLen, e.spiUseIrq);
        }
        return GWERR_BAD_CRC;
    }

    respLen = rxLen;
    return GWERR_OK;
}

bool GwRouter::buildGatewayErrorPayload(uint8_t cmd, GwErr err, uint8_t* out, size_t outMax, size_t& outLen) const
{
    outLen = 0;
    if (!out || outMax < 3) return false;

    out[0] = SDGW_TLV_GATEWAY_ERR;
    out[1] = 0x01;
    out[2] = (uint8_t)err;
    outLen = 3;

    const uint8_t addr = GW_CMD_ADDR(cmd);
    if (addr != GW_ADDR_UCE || !_lastDiag.valid) {
        return true;
    }

    const size_t valueLen = (size_t)11 + (size_t)_lastDiag.txLen + (size_t)_lastDiag.rxLen;
    const size_t totalLen = (size_t)2 + valueLen;
    if (totalLen > outMax || valueLen > 0xFF) {
        return true;
    }

    out[0] = SDGW_TLV_GATEWAY_ERR;
    out[1] = (uint8_t)valueLen;
    out[2] = (uint8_t)err;
    out[3] = GwSpiDiagnostic::kVersion;
    out[4] = _lastDiag.layer;
    out[5] = _lastDiag.phase;
    out[6] = _lastDiag.cause;
    out[7] = _lastDiag.txLen;
    out[8] = _lastDiag.rxLen;
    out[9] = _lastDiag.expectedLen;
    out[10] = _lastDiag.receivedLen;
    out[11] = _lastDiag.crcCalculated;
    out[12] = _lastDiag.crcReceived;

    size_t cursor = 13;
    for (uint8_t index = 0; index < _lastDiag.txLen; index++) {
        out[cursor++] = _lastDiag.tx[index];
    }
    for (uint8_t index = 0; index < _lastDiag.rxLen; index++) {
        out[cursor++] = _lastDiag.rx[index];
    }

    outLen = cursor;
    return true;
}

bool GwRouter::pollGsaEvent(uint8_t* respBuf, size_t respMax, size_t& respLen)
{
    respLen = 0;

    GwI2cBus* i2cBus = static_cast<GwI2cBus*>(&_i2c);
    if (!i2cBus) return false;

    if (!i2cBus->pollEvent(GW_ADDR_GSA, respBuf, respMax, respLen))
        return false;

    return GwTlv::validatePacket(respBuf, respLen);
}
```

Arquivo: `C:\PROJETOS\SimulDIESEL\hardware\firmware\BPM - BACKPLANE MANAGER MODULE\lib\GwErr\GwErr.h`  
Trecho: linhas 1-15  
Papel: Tabela de códigos de erro de gateway usados no retorno SDGW.

```cpp
#pragma once
#include <stdint.h>

// Códigos de erro do Gateway (roteamento / barramento)
enum GwErr : uint8_t {
    GWERR_OK               = 0x00,
    GWERR_ADDR_UNMAPPED    = 0xE1,
    GWERR_BUS_DOWN         = 0xE2,
    GWERR_TIMEOUT          = 0xE3,
    GWERR_BAD_CRC          = 0xE4,
    GWERR_BAD_FRAME        = 0xE5,
    GWERR_HEADER_INVALID   = 0xE6,
    GWERR_LENGTH_INVALID   = 0xE7,
    GWERR_FRAME_INCOMPLETE = 0xE8,
};
```

Arquivo: `C:\PROJETOS\SimulDIESEL\hardware\firmware\BPM - BACKPLANE MANAGER MODULE\lib\GwDiag\GwSpiDiagnostic.h`  
Trecho: linhas 1-52  
Papel: Estrutura do snapshot diagnóstico SPI exportado ao host.

```cpp
#pragma once
#include <stddef.h>
#include <stdint.h>

namespace GwSpiDiagnostic {
    static const uint8_t kVersion = 0x01;
    static const size_t kMaxFrameBytes = 32;

    enum Layer : uint8_t {
        LayerUnknown = 0x00,
        LayerBpm = 0x01,
        LayerGwSpiBus = 0x02,
        LayerCrcValidation = 0x03,
    };

    enum Phase : uint8_t {
        PhaseUnknown = 0x00,
        PhaseWrite = 0x01,
        PhaseWaitResponseReady = 0x02,
        PhaseReadHeader = 0x03,
        PhaseReadPayload = 0x04,
        PhaseFinalCrcValidation = 0x05,
    };

    enum PossibleCause : uint8_t {
        CauseUnknown = 0x00,
        CauseFirstByteMisaligned = 0x01,
        CausePreloadFailure = 0x02,
        CauseWrongCrcPolynomial = 0x03,
        CauseEarlyReadBeforeResponseReady = 0x04,
        CauseLengthMismatch = 0x05,
        CauseTimeoutWaitingIrq = 0x06,
        CauseIncompleteFrame = 0x07,
    };

    struct Snapshot {
        bool valid = false;
        uint8_t addr = 0;
        uint8_t layer = LayerUnknown;
        uint8_t phase = PhaseUnknown;
        uint8_t cause = CauseUnknown;
        uint8_t status = 0;
        uint8_t txLen = 0;
        uint8_t rxLen = 0;
        uint8_t expectedLen = 0;
        uint8_t receivedLen = 0;
        uint8_t crcCalculated = 0;
        uint8_t crcReceived = 0;
        uint8_t tx[kMaxFrameBytes] = {0};
        uint8_t rx[kMaxFrameBytes] = {0};
    };
}
```

Arquivo: `C:\PROJETOS\SimulDIESEL\hardware\firmware\BPM - BACKPLANE MANAGER MODULE\lib\GwDeviceTable\GwDeviceTable.h`  
Trecho: linhas 1-23  
Papel: Definição da tabela de dispositivos remotos e consulta por endereço lógico.

```cpp
#pragma once
#include <stdint.h>

enum GwBusType : uint8_t { GW_BUS_I2C = 1, GW_BUS_SPI = 2 };

struct GwDeviceEntry {
    uint8_t addr;       // 0x1..0xE
    GwBusType bus;
    // I2C
    uint8_t i2cAddr;    // 7-bit
    // SPI
    int8_t  spiCsPin;   // GPIO CS (se bus=SPI)
    int8_t  spiIrqPin;  // opcional
    int8_t  resetPin;   // reset fisico associado ao device
    bool    spiUseIrq;  // permite manter IRQ mapeada sem usá-la no fluxo síncrono atual
};

class GwDeviceTable {
public:
    // A tabela atual representa apenas os defaults de bootstrap das
    // baby boards externas. A BPM local (ADDR 0) nao participa daqui.
    static bool get(uint8_t addr, GwDeviceEntry& out);
};
```

Arquivo: `C:\PROJETOS\SimulDIESEL\hardware\firmware\BPM - BACKPLANE MANAGER MODULE\lib\GwDeviceTable\GwDeviceTable.cpp`  
Trecho: linhas 1-18  
Papel: Publicação concreta da UCE/GSA na tabela de devices com tipo de barramento e endereço.

```cpp
#include "GwDeviceTable.h"
#include "SdgwDefs.h"

// Defaults de bootstrap enquanto a BPM ainda nao persiste configuracao
// dinamica de enderecos. O host continua tratando o binding logico;
// a BPM consome apenas enderecos compactos ja resolvidos.
static const GwDeviceEntry kBootstrapDefaults[] = {
    // addr,        bus,        i2cAddr,      cs,                 irq,             reset,              useIrq
    {GW_ADDR_GSA,  GW_BUS_I2C, I2C_GSA_ADDR, -1,                 -1,              BPM_GLOBAL_RESET_PIN, false},
    {GW_ADDR_UCE,  GW_BUS_SPI, 0x00,         BPM_UCE_SPI_CS_PIN, BPM_UCE_IRQ_PIN, BPM_UCE_RESET_PIN,    true},
};

bool GwDeviceTable::get(uint8_t addr, GwDeviceEntry& out) {
    for (auto& e : kBootstrapDefaults) {
        if (e.addr == addr) { out = e; return true; }
    }
    return false;
}
```

Arquivo: `C:\PROJETOS\SimulDIESEL\hardware\firmware\BPM - BACKPLANE MANAGER MODULE\include\SdgwDefs.h`  
Trecho: linhas 1-155  
Papel: Constantes SDGW relevantes para endereço da UCE, timeouts, pinos SPI/IRQ/reset e limites de frame.

```cpp
#pragma once
#include <stdint.h>


// ============================================================
// I2C - Baby boards
// ============================================================
#define I2C_GSA_ADDR  0x23   // endereco fisico padrao do GSA no barramento I2C
#define BPM_GSA_I2C_SDA_PIN   21
#define BPM_GSA_I2C_SCL_PIN   22
#define BPM_GSA_IRQ_PIN       19
#define BPM_GLOBAL_RESET_PIN  23
#define BPM_GLOBAL_RESET_ACTIVE_LEVEL LOW
#define BPM_GLOBAL_RESET_INACTIVE_LEVEL HIGH

// SPI dedicado da BPM.
#define BPM_SPI_SCK_PIN       18
#define BPM_SPI_MISO_PIN      26
#define BPM_SPI_MOSI_PIN      25
#define BPM_SPI_CLOCK_HZ      8000000UL
#define BPM_UCE_SPI_CS_PIN    33
#define BPM_UCE_IRQ_PIN       27
#define BPM_UCE_RESET_PIN     BPM_GLOBAL_RESET_PIN

// ============================================================
// GSA - TLV commands (I2C payload)
// ============================================================
#define GSA_CMD_SETPOINT       0x10
#define GSA_CMD_ENABLE_CH      0x11
#define GSA_CMD_SET_LED        0x12
#define GSA_CMD_ENABLE_GLOBAL  0x14
#define GSA_CMD_FAULT_RESET    0x15
#define GSA_CMD_OFFSET_SET     0x16
#define GSA_CMD_OFFSET_SAVE    0x18
#define GSA_CMD_OFFSET_RESET   0x1A
#define GSA_CMD_STATUS_CH      0x1B
#define GSA_CMD_FAULT_EVENT    0x30
#define GSA_CMD_PHYSICAL_EVENT 0x31
#define GSA_CMD_FUNC_ERROR     0x7F
#define UCE_CMD_SET_LED        0x12
#define UCE_CMD_FUNC_ERROR     0x7F

#define LED_BUILTIN  2
// ============================================================
// UART
// ============================================================
#define SDGW_UART_BAUDRATE              115200
#define SDGW_UART_CONFIG                SERIAL_8N1


// Ajuste caso o LED da sua placa seja ativo em LOW
#define LED_ACTIVE_LOW 0


// ============================================================
// Protocol limits (logical = antes do COBS)
// ============================================================
#define SDGW_MAX_LOGICAL_FRAME          250   // CMD+FLAGS+SEQ+DATA(0..247)+CRC
#define SDGW_MAX_PAYLOAD                247

// COBS worst-case: len + len/254 + 1  (250 => 252) + delimiter => 253
// Mantemos folga para robustez
#define SDGW_MAX_ENCODED_FRAME          384

// Cache de resposta para retransmissão (ACK/ERR + delimiter)
// 64 é suficiente para ACK/ERR + 1 byte de erro, mas deixamos folga
#define SDGW_MAX_LAST_RESPONSE          64

// Handshake
#define SDGW_HANDSHAKE_BUFFER           64
#define SDGW_HANDSHAKE_TIMEOUT_MS       2000

// Sessao/atividade do link
#define SDGW_LINK_ACTIVITY_TIMEOUT_MS   4000

// Timeout interno do gateway ao rotear para a baby board
#define SDGW_GATEWAY_ROUTE_TIMEOUT_MS   100

// ============================================================
// Flags
// ============================================================
#define SDGW_FLAG_ACK_REQUIRED          0x01
#define SDGW_FLAG_IS_EVENT              0x02

// ============================================================
// Reserved commands
// ============================================================
#define SDGW_CMD_ACK                    0xF1
#define SDGW_CMD_ERR                    0xF2
#define SDGW_TLV_GATEWAY_ERR            0xFE

// ============================================================
// Reserved operational commands
// ============================================================
#define SDGW_CMD_PING                   0x55

// ============================================================
// Error codes (para payload do ERR)
// ============================================================
#define SDGW_ERR_UNKNOWN_CMD            0x01
#define SDGW_ERR_BAD_CRC                0x02
#define SDGW_ERR_BAD_COBS               0x03
#define SDGW_ERR_TOO_LARGE              0x04
#define SDGW_ERR_TOO_SMALL              0x05

// ============================================================
// CRC-8/ATM
// ============================================================
#define SDGW_CRC_POLY                   0x07
#define SDGW_CRC_INIT                   0x00

// ============================================================
// COBS framing
// ============================================================
#define SDGW_COBS_DELIMITER             0x00

// ============================================================
// Handshake strings
// ============================================================
#define SDGW_PC_BANNER                  "\nSIMULDIESELAPI\n"
#define SDGW_DEVICE_BANNER              "SimulDIESEL ver 0.0.1\n"

// ============================================================
// Sequence
// ============================================================
#define SDGW_SEQ_START                  1


// ============================================================
// Gateway compact commands (CMD byte = [ADDR:4][OP:4])
// A BPM sempre ocupa o endereco logico local 0x0.
// Os demais enderecos abaixo sao defaults de bootstrap do firmware
// e podem evoluir depois para configuracao persistida pela propria BPM.
// ============================================================
#define GW_ADDR_BPM         0x0
#define GW_ADDR_GSA         0x1
#define GW_ADDR_UCE         0x2
#define GW_ADDR_BROADCAST   0xF

#define GW_MAKE_CMD(addr, op4)   (uint8_t)((((addr) & 0x0F) << 4) | ((op4) & 0x0F))
#define GW_CMD_ADDR(cmd)         (uint8_t)(((cmd) >> 4) & 0x0F)
#define GW_CMD_OP(cmd)           (uint8_t)((cmd) & 0x0F)

// BPM local ops (0..15)
#define GW_OP_BPM_PING           0x0

// GSA ops (0..15)
#define GW_OP_GSA_TLV_TRANSACT   0x0
// UCE ops (0..15)
#define GW_OP_UCE_TLV_TRANSACT   0x0

// Comandos compactos resolvidos pelo host.
#define SDGW_CMD_BPM_PING        GW_MAKE_CMD(GW_ADDR_BPM, GW_OP_BPM_PING)
#define SDGW_CMD_GSA_TLV         GW_MAKE_CMD(GW_ADDR_GSA, GW_OP_GSA_TLV_TRANSACT)
#define SDGW_CMD_UCE_TLV         GW_MAKE_CMD(GW_ADDR_UCE, GW_OP_UCE_TLV_TRANSACT)
```

## BLOCO 3 — UCE / TRANSPORTE SPI SLAVE

Arquivos incluídos:
- `C:\PROJETOS\SimulDIESEL\hardware\firmware\UCE - Unidade de comunicacao externa\lib\core\transport\Transport.h`
- `C:\PROJETOS\SimulDIESEL\hardware\firmware\UCE - Unidade de comunicacao externa\lib\core\transport\Transport.cpp`

Dump integral do transporte SPI slave da UCE para inspeção da máquina RX/TX, preload, tratamento de RDRF/NSSR e persistência de estado entre bursts.

Arquivo: `C:\PROJETOS\SimulDIESEL\hardware\firmware\UCE - Unidade de comunicacao externa\lib\core\transport\Transport.h`  
Trecho: linhas 1-45  
Papel: Declaração do transporte SPI slave, atributos privados e helpers internos de TX/RX.

```cpp
#pragma once

#include <stdint.h>

#include "defs.h"

class Transport {
public:
  void begin();
  bool popRx(uint8_t* out, uint8_t& outLen);
  void setTx(const uint8_t* data, uint8_t len);
  static void setIrqActive(bool active);
  static void onSpiInterrupt();

  static Transport* _self;

private:
  enum TransactionMode : uint8_t {
    ModeIdle = 0,
    ModeReceiving = 1,
    ModeSending = 2
  };

  static void clearTxState();
  static void primeTxByte(uint8_t value);
  static void primeTxForCurrentPosition();
  static void rearmTxForNextBurst();

  static volatile uint8_t _rxWorkBuf[TLV_MAX_LEN];
  static volatile uint8_t _rxWorkLen;
  static volatile uint8_t _rxBuf[TLV_MAX_LEN];
  static volatile uint8_t _rxLen;
  static volatile bool _rxPending;

  static volatile uint8_t _txBuf[TLV_MAX_LEN];
  static volatile uint16_t _txLen;
  static volatile uint16_t _txSentCount;
  static volatile bool _txActive;
  static volatile bool _txPrimed;
  static volatile uint8_t _txPrimedByte;

  static volatile uint8_t _mode;
};

extern "C" void SPI0_Handler(void);
```

Arquivo: `C:\PROJETOS\SimulDIESEL\hardware\firmware\UCE - Unidade de comunicacao externa\lib\core\transport\Transport.cpp`  
Trecho: linhas 1-226  
Papel: Implementação do slave SPI, ISR/handler, preload explícito, setTx, takeRx e limpeza/rearme de estado.

```cpp
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
```

## BLOCO 4 — UCE / LINK E TLV

Arquivos incluídos:
- `C:\PROJETOS\SimulDIESEL\hardware\firmware\UCE - Unidade de comunicacao externa\lib\core\link\Link.h`
- `C:\PROJETOS\SimulDIESEL\hardware\firmware\UCE - Unidade de comunicacao externa\lib\core\link\Link.cpp`
- `C:\PROJETOS\SimulDIESEL\hardware\firmware\UCE - Unidade de comunicacao externa\lib\core\link\crc8.h`
- `C:\PROJETOS\SimulDIESEL\hardware\firmware\UCE - Unidade de comunicacao externa\lib\protocol\tlv\Tlv.h`
- `C:\PROJETOS\SimulDIESEL\hardware\firmware\UCE - Unidade de comunicacao externa\lib\protocol\tlv\Tlv.cpp`

Trechos que mostram como o request recebido pelo transporte vira um TLV, como a resposta é montada, como o CRC é calculado e como erros funcionais são gerados.

Arquivo: `C:\PROJETOS\SimulDIESEL\hardware\firmware\UCE - Unidade de comunicacao externa\lib\core\link\Link.h`  
Trecho: linhas 1-23  
Papel: Contrato do elo lógico entre transporte SPI, service e construção de resposta.

```cpp
#pragma once

#include <stdint.h>

#include "core/service/Service.h"
#include "core/transport/Transport.h"
#include "core/link/crc8.h"
#include "protocol/tlv/Tlv.h"

class Link {
public:
  Link(Transport& transport, Service& service);

  void begin();
  void tick();
  void poll();

private:
  bool parseAndValidate(const uint8_t* rx, uint8_t rxLen, TlvFrame& out, uint8_t& requestType, uint8_t& errorCode) const;

  Transport& _transport;
  Service& _service;
};
```

Arquivo: `C:\PROJETOS\SimulDIESEL\hardware\firmware\UCE - Unidade de comunicacao externa\lib\core\link\Link.cpp`  
Trecho: linhas 1-72  
Papel: Fluxo de recebimento do request, despacho ao service, montagem do frame TLV+CRC e envio via Transport::setTx().

```cpp
#include "core/link/Link.h"

#include "diag/trace/DiagTrace.h"

Link::Link(Transport& transport, Service& service)
  : _transport(transport), _service(service)
{
}

void Link::begin() {
  Transport::setIrqActive(false);
}

void Link::tick() {
}

bool Link::parseAndValidate(const uint8_t* rx, uint8_t rxLen, TlvFrame& out, uint8_t& requestType, uint8_t& errorCode) const {
  requestType = (rx && rxLen > 0) ? rx[0] : 0;
  errorCode = UCE_ERROR_INVALID_PAYLOAD;

  if (!rx || rxLen < 3) return false;

  const uint8_t t = rx[0];
  const uint8_t l = rx[1];
  const uint8_t expectedLen = (uint8_t)(2 + l + 1);
  if (expectedLen != rxLen) return false;

  const uint8_t crcRx = rx[rxLen - 1];
  const uint8_t crcCalc = Crc8::calc(rx, (uint8_t)(rxLen - 1));
  if (crcCalc != crcRx) {
    errorCode = UCE_ERROR_INVALID_TLV_CRC;
    DiagTrace::logBytes(DiagTrace::EvLinkCrcError, rx, rxLen, crcCalc, crcRx, 0);
    return false;
  }

  out.t = t;
  out.l = l;
  out.v = (l > 0) ? &rx[2] : nullptr;
  return true;
}

void Link::poll() {
  uint8_t rxBuf[TLV_MAX_LEN];
  uint8_t rxLen = 0;

  if (!_transport.popRx(rxBuf, rxLen)) return;
  DiagTrace::logBytes(DiagTrace::EvLinkRxFrame, rxBuf, rxLen);

  TlvFrame tlv;
  uint8_t requestType = 0;
  uint8_t errorCode = UCE_ERROR_INVALID_PAYLOAD;
  uint8_t txTlv[TLV_MAX_LEN];
  uint8_t txTlvLen = 0;

  if (!parseAndValidate(rxBuf, rxLen, tlv, requestType, errorCode)) {
    const uint8_t payload[3] = { requestType, 0, errorCode };
    txTlvLen = Tlv::build(CMD_FUNCTIONAL_ERROR, payload, sizeof(payload), txTlv, TLV_MAX_LEN);
  } else if (!_service.handleOneTlv(tlv, txTlv, txTlvLen)) {
    const uint8_t payload[3] = { tlv.t, 0, UCE_ERROR_COMMAND_NOT_SUPPORTED };
    txTlvLen = Tlv::build(CMD_FUNCTIONAL_ERROR, payload, sizeof(payload), txTlv, TLV_MAX_LEN);
  }

  if (txTlvLen >= 2 && (uint8_t)(txTlvLen + 1) <= TLV_MAX_LEN) {
    uint8_t out[TLV_MAX_LEN];
    for (uint8_t index = 0; index < txTlvLen; ++index) {
      out[index] = txTlv[index];
    }
    out[txTlvLen] = Crc8::calc(out, txTlvLen);
    DiagTrace::logBytes(DiagTrace::EvLinkResponseBuilt, out, (uint8_t)(txTlvLen + 1), out[txTlvLen], 0, 0);
    _transport.setTx(out, (uint8_t)(txTlvLen + 1));
  }
}
```

Arquivo: `C:\PROJETOS\SimulDIESEL\hardware\firmware\UCE - Unidade de comunicacao externa\lib\core\link\crc8.h`  
Trecho: linhas 1-19  
Papel: Implementação inline do CRC8 usado no frame TLV da UCE.

```cpp
#pragma once

#include <stdint.h>

class Crc8 {
public:
  static uint8_t calc(const uint8_t* data, uint8_t len) {
    uint8_t crc = 0x00;

    for (uint8_t index = 0; index < len; ++index) {
      crc ^= data[index];
      for (uint8_t bit = 0; bit < 8; ++bit) {
        crc = (crc & 0x80U) ? (uint8_t)((crc << 1U) ^ 0x07U) : (uint8_t)(crc << 1U);
      }
    }

    return crc;
  }
};
```

Arquivo: `C:\PROJETOS\SimulDIESEL\hardware\firmware\UCE - Unidade de comunicacao externa\lib\protocol\tlv\Tlv.h`  
Trecho: linhas 1-15  
Papel: Struct TLV e assinaturas de parse/serialize/erro funcional.

```cpp
#pragma once

#include <stdint.h>

struct TlvFrame {
  uint8_t t = 0;
  uint8_t l = 0;
  const uint8_t* v = nullptr;
};

class Tlv {
public:
  static uint8_t build(uint8_t t, const uint8_t* payload, uint8_t payloadLen, uint8_t* out, uint8_t outMax);
  static uint8_t buildU8(uint8_t t, uint8_t value, uint8_t* out, uint8_t outMax);
};
```

Arquivo: `C:\PROJETOS\SimulDIESEL\hardware\firmware\UCE - Unidade de comunicacao externa\lib\protocol\tlv\Tlv.cpp`  
Trecho: linhas 1-22  
Papel: Validação do TLV, serialização e construção do erro funcional 0x7F.

```cpp
#include "protocol/tlv/Tlv.h"

uint8_t Tlv::build(uint8_t t, const uint8_t* payload, uint8_t payloadLen, uint8_t* out, uint8_t outMax) {
  if (!out) return 0;

  const uint8_t totalLen = (uint8_t)(payloadLen + 2);
  if (outMax < totalLen) return 0;

  out[0] = t;
  out[1] = payloadLen;

  for (uint8_t index = 0; index < payloadLen; ++index) {
    out[2 + index] = payload ? payload[index] : 0;
  }

  return totalLen;
}

uint8_t Tlv::buildU8(uint8_t t, uint8_t value, uint8_t* out, uint8_t outMax) {
  const uint8_t payload[1] = { value };
  return build(t, payload, 1, out, outMax);
}
```

## BLOCO 5 — UCE / SERVICE E APP

Arquivos incluídos:
- `C:\PROJETOS\SimulDIESEL\hardware\firmware\UCE - Unidade de comunicacao externa\lib\core\service\Service.h`
- `C:\PROJETOS\SimulDIESEL\hardware\firmware\UCE - Unidade de comunicacao externa\lib\core\service\Service.cpp`
- `C:\PROJETOS\SimulDIESEL\hardware\firmware\UCE - Unidade de comunicacao externa\lib\app\UceApp.h`
- `C:\PROJETOS\SimulDIESEL\hardware\firmware\UCE - Unidade de comunicacao externa\lib\app\UceApp.cpp`
- `C:\PROJETOS\SimulDIESEL\hardware\firmware\UCE - Unidade de comunicacao externa\include\defs.h`
- `C:\PROJETOS\SimulDIESEL\hardware\firmware\UCE - Unidade de comunicacao externa\include\config.h`
- `C:\PROJETOS\SimulDIESEL\hardware\firmware\UCE - Unidade de comunicacao externa\src\main.cpp`

Trechos que mostram a composição da UCE, comandos suportados, limites/configuração e ordem de chamada do loop principal até o ponto em que o Link é acionado.

Arquivo: `C:\PROJETOS\SimulDIESEL\hardware\firmware\UCE - Unidade de comunicacao externa\lib\core\service\Service.h`  
Trecho: linhas 1-27  
Papel: Contrato do serviço funcional que interpreta o TLV e produz respostas.

```cpp
#pragma once

#include <stdint.h>

#include "defs.h"
#include "protocol/tlv/Tlv.h"
#include "services/can/CanService.h"
#include "services/led/LedService.h"

class Service {
public:
  Service(LedService& led, CanService& can);

  void begin();
  bool handleOneTlv(const TlvFrame& tlv, uint8_t* txOut, uint8_t& txLenOut);

private:
  bool handleBuiltinLed(const TlvFrame& tlv, uint8_t* txOut, uint8_t& txLenOut);
  bool handleCanConfig(const TlvFrame& tlv, uint8_t* txOut, uint8_t& txLenOut);
  bool handleCanEnable(const TlvFrame& tlv, uint8_t* txOut, uint8_t& txLenOut);
  bool handleCanStatus(const TlvFrame& tlv, uint8_t* txOut, uint8_t& txLenOut);
  bool handleCanReset(const TlvFrame& tlv, uint8_t* txOut, uint8_t& txLenOut);
  bool buildFunctionalError(uint8_t requestType, uint8_t errorCode, uint8_t* txOut, uint8_t& txLenOut) const;

  LedService& _led;
  CanService& _can;
};
```

Arquivo: `C:\PROJETOS\SimulDIESEL\hardware\firmware\UCE - Unidade de comunicacao externa\lib\core\service\Service.cpp`  
Trecho: linhas 1-291  
Papel: Implementação dos comandos LED/CAN, montagem de respostas e erros funcionais.

```cpp
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
```

Arquivo: `C:\PROJETOS\SimulDIESEL\hardware\firmware\UCE - Unidade de comunicacao externa\lib\app\UceApp.h`  
Trecho: linhas 1-32  
Papel: Composição da aplicação UCE e injeção das dependências principais.

```cpp
#pragma once

#include "core/runtime/UceContext.h"
#include "core/link/Link.h"
#include "core/service/Service.h"
#include "core/transport/Transport.h"
#include "drivers/can/Sam3xCanDriver.h"
#include "hal/transceivers/NullCanTransceiver.h"
#include "services/can/CanService.h"
#include "services/led/LedService.h"

class UceApp {
public:
  UceApp();

  void begin();
  void tick();
  void poll();

  UceContext& context();
  const UceContext& context() const;

private:
  Transport _transport;
  LedService _led;
  NullCanTransceiver _canTransceiver;
  Sam3xCanDriver _canDriver;
  CanService _canService;
  Service _service;
  Link _link;
  UceContext _context;
};
```

Arquivo: `C:\PROJETOS\SimulDIESEL\hardware\firmware\UCE - Unidade de comunicacao externa\lib\app\UceApp.cpp`  
Trecho: linhas 1-52  
Papel: Inicialização da aplicação, begin/poll e encadeamento Transport -> Link -> Service.

```cpp
#include "app/UceApp.h"

#include "diag/trace/DiagTrace.h"
#include "services/can/CanConfig.h"

UceApp::UceApp()
  : _transport(),
    _led(),
    _canTransceiver(),
    _canDriver(),
    _canService(_canDriver, _canTransceiver),
    _service(_led, _canService),
    _link(_transport, _service),
    _context()
{
}

void UceApp::begin() {
  DiagTrace::begin();

  _led.begin();
  _service.begin();
  _transport.begin();
  _link.begin();

  _canService.begin();
  _canService.configure(CanConfig{});

  UceStatus& status = _context.status();
  status.markBootCompleted();
  status.markTransportReady(true);
  status.markServiceReady(true);
  status.markLinkReady(true);
  status.markCanReady(true);
}

void UceApp::tick() {
  _link.tick();
  _context.status().bumpLoopCounter();
}

void UceApp::poll() {
  _link.poll();
}

UceContext& UceApp::context() {
  return _context;
}

const UceContext& UceApp::context() const {
  return _context;
}
```

Arquivo: `C:\PROJETOS\SimulDIESEL\hardware\firmware\UCE - Unidade de comunicacao externa\include\defs.h`  
Trecho: linhas 1-22  
Papel: Defines de comandos, tipos TLV e limites funcionais da UCE.

```cpp
#pragma once
#include <stdint.h>

#define TLV_MAX_LEN               32
#define UCE_CAN_MAX_MAILBOXES     8
#define UCE_CAN_MAX_FILTERS       8

#define UCE_PIN_NOT_CONNECTED     0xFF

#define CMD_LED_BUILTIN           0x12
#define CMD_CAN_CONFIG            0x20
#define CMD_CAN_ENABLE            0x21
#define CMD_CAN_STATUS            0x22
#define CMD_CAN_RESET             0x23
#define CMD_FUNCTIONAL_ERROR      0x7F

#define UCE_ERROR_INVALID_STATE          0x03
#define UCE_ERROR_COMMAND_NOT_SUPPORTED  0x07
#define UCE_ERROR_INVALID_PAYLOAD        0x08
#define UCE_ERROR_INVALID_TLV_CRC        0x09

#define LED_PIN                   LED_BUILTIN
```

Arquivo: `C:\PROJETOS\SimulDIESEL\hardware\firmware\UCE - Unidade de comunicacao externa\include\config.h`  
Trecho: linhas 1-13  
Papel: Configuração de pinos e constantes de build ligadas ao transporte SPI.

```cpp
#pragma once

#include <Arduino.h>
#include <stdint.h>

// Pinagem congelada BPM -> UCE.
// SCK/MOSI/MISO usam obrigatoriamente o conector SPI da Due.
static const uint8_t UCE_SPI_CS_PIN = 10;
static const uint8_t UCE_IRQ_PIN = 2;

// O reset da BPM entra no reset fisico da Due e não passa por GPIO de firmware.
static const uint8_t UCE_IRQ_ACTIVE_LEVEL = LOW;
static const uint8_t UCE_IRQ_IDLE_LEVEL = HIGH;
```

Arquivo: `C:\PROJETOS\SimulDIESEL\hardware\firmware\UCE - Unidade de comunicacao externa\src\main.cpp`  
Trecho: linhas 1-15  
Papel: Loop principal da UCE e acionamento periódico da aplicação.

```cpp
#include <Arduino.h>
#include "app/UceApp.h"
#include "diag/trace/DiagTrace.h"

static UceApp g_app;

void setup() {
  g_app.begin();
}

void loop() {
  g_app.tick();
  g_app.poll();
  DiagTrace::flush();
}
```

## BLOCO 6 — HOST / CONTRATO DA UCE

Arquivos incluídos:
- `C:\PROJETOS\SimulDIESEL\local-api\src\SimulDIESEL\SimulDIESEL\DTL\Protocols\SDGW\GwProtocol.cs`
- `C:\PROJETOS\SimulDIESEL\local-api\src\SimulDIESEL\SimulDIESEL\DAL\Protocols\SDGW\SdhToSdgwMapper.cs`
- `C:\PROJETOS\SimulDIESEL\local-api\src\SimulDIESEL\SimulDIESEL\BLL\Boards\UCE\UceClient.cs`
- `C:\PROJETOS\SimulDIESEL\local-api\src\SimulDIESEL\SimulDIESEL\BLL\Boards\UCE\UceGatewayDiagnosticLog.cs`

Trechos do host que mostram o frame SDGW enviado à BPM, o mapeamento dos comandos UCE para TLV, o casamento da resposta esperada e o parser do diagnóstico de erro.

Arquivo: `C:\PROJETOS\SimulDIESEL\local-api\src\SimulDIESEL\SimulDIESEL\DTL\Protocols\SDGW\GwProtocol.cs`  
Trecho: linhas 1-77  
Papel: Constantes de protocolo usadas pelo host para endereçar a UCE e interpretar requests/responses.

```csharp
namespace SimulDIESEL.DTL.Protocols.SDGW
{
    /// <summary>
    /// Contrato compacto oficial compartilhado entre SDH host e BPM.
    /// </summary>
    public static class GwProtocol
    {
        public const byte BpmAddress = 0x0;
        public const byte GsaAddress = 0x1;
        public const byte UceAddress = 0x2;
        public const byte BroadcastAddress = 0xF;

        public const byte BpmPingOp = 0x0;
        public const byte GsaTlvTransactOp = 0x0;
        public const byte UceTlvTransactOp = 0x0;

        // O LED builtin permanece em 0x12 por contrato.
        // O status por canal foi migrado para 0x1B para remover a
        // ambiguidade histórica do contrato TLV da GSA.
        public const byte GsaSetLedType = 0x12;
        public const byte GsaChannelSetpointType = 0x10;
        public const byte GsaChannelEnableType = 0x11;
        public const byte GsaChannelStatusType = 0x1B;
        public const byte GsaChannelsStatusType = 0x13;
        public const byte GsaChannelsEnableType = 0x14;
        public const byte GsaChannelFaultResetType = 0x15;
        public const byte GsaChannelOffsetSetType = 0x16;
        public const byte GsaChannelOffsetGetType = 0x17;
        public const byte GsaChannelOffsetSaveType = 0x18;
        public const byte GsaChannelOffsetResetType = 0x19;
        public const byte GsaOffsetResetType = 0x1A;
        public const byte GsaChannelFaultEventType = 0x30;
        public const byte GsaPhysicalOperationEventType = 0x31;
        public const byte GsaErrorType = 0x7F;
        public const byte UceSetLedType = 0x12;
        public const byte UceCanConfigType = 0x20;
        public const byte UceCanEnableType = 0x21;
        public const byte UceCanStatusType = 0x22;
        public const byte UceCanResetType = 0x23;
        public const byte UceErrorType = 0x7F;
        public const byte GatewayErrorType = 0xFE;

        public const byte UceLedPayloadLength = 0x01;
        public const byte UceCanConfigPayloadLength = 0x03;
        public const byte UceCanEnablePayloadLength = 0x02;
        public const byte UceCanStatusRequestPayloadLength = 0x01;
        public const byte UceCanStatusResponsePayloadLength = 0x04;
        public const byte UceCanResetRequestPayloadLength = 0x01;
        public const byte UceCanResetResponsePayloadLength = 0x02;

        public const byte UceCanControllerCan0 = 0x00;
        public const byte UceCanControllerCan1 = 0x01;
        public const byte UceCanBitrate125Code = 0x00;
        public const byte UceCanBitrate250Code = 0x01;
        public const byte UceCanBitrate500Code = 0x02;
        public const byte UceCanBitrate1000Code = 0x03;
        public const byte UceCanModeNormal = 0x00;
        public const byte UceCanModeListen = 0x01;
        public const byte UceCanStateOff = 0x00;
        public const byte UceCanStateOn = 0x01;
        public const byte UceCanInterfaceDisabled = 0x00;
        public const byte UceCanInterfaceConfigured = 0x01;
        public const byte UceCanInterfaceOpen = 0x02;
        public const byte UceCanInterfaceFault = 0x03;
        public const byte UceCanResetFailed = 0x00;
        public const byte UceCanResetSucceeded = 0x01;

        public const byte GsaOffsetKindVout = 0x01;
        public const byte GsaOffsetKindVread = 0x02;
        public const byte GsaOffsetKindIread = 0x03;

        public static byte MakeCompactCommand(byte address, byte op)
        {
            return (byte)(((address & 0x0F) << 4) | (op & 0x0F));
        }
    }
}
```

Arquivo: `C:\PROJETOS\SimulDIESEL\local-api\src\SimulDIESEL\SimulDIESEL\DAL\Protocols\SDGW\SdhToSdgwMapper.cs`  
Trecho: linhas 1-307  
Papel: Mapeamento de comandos SDH em payload SDGW/TLV para a UCE.

```csharp
using System;
using System.Globalization;
using SimulDIESEL.DTL.Boards.GSA;
using SimulDIESEL.DTL.Boards.UCE;
using SimulDIESEL.DTL.Protocols.SDGW;

namespace SimulDIESEL.DAL.Protocols.SDGW
{
    /// <summary>
    /// Traduz comandos SDH para payload/cmd SDGW.
    /// Mantido nesta área por segurança nesta fase, embora a fronteira semântica
    /// possa ser refinada em rodadas futuras.
    /// </summary>
    public sealed class SdhToSdgwMapper
    {
        public sealed class MappedSdgwCommand
        {
            public byte Cmd { get; set; }
            public byte[] Payload { get; set; }
            public bool RequireAck { get; set; }
            public int TimeoutMs { get; set; }
            public int Retries { get; set; }
        }

        private const string BpmGatewayTarget = "BPM.gateway";
        private const string PingOp = "ping";
        private const int DefaultBoardTimeoutMs = 400;
        private const int DefaultBoardRetries = 2;

        public MappedSdgwCommand Map(SdhCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (string.Equals(command.Target, BpmGatewayTarget, StringComparison.OrdinalIgnoreCase))
                return MapBpmGateway(command);

            if (command.Target.StartsWith("GSA.", StringComparison.OrdinalIgnoreCase))
                return MapGsa(command);

            if (command.Target.StartsWith("UCE.", StringComparison.OrdinalIgnoreCase))
                return MapUce(command);

            throw new NotSupportedException("Mapeamento SDH->SDGW ainda não suporta target: " + command.Target + ".");
        }

        private static MappedSdgwCommand MapBpmGateway(SdhCommand command)
        {
            if (!string.Equals(command.Op, PingOp, StringComparison.OrdinalIgnoreCase))
                throw new NotSupportedException("Mapeamento SDH->SDGW ainda não suporta op: " + command.Op + ".");

            return new MappedSdgwCommand
            {
                Cmd = GwProtocol.MakeCompactCommand(GwProtocol.BpmAddress, GwProtocol.BpmPingOp),
                Payload = Array.Empty<byte>(),
                RequireAck = true,
                TimeoutMs = 150,
                Retries = 1
            };
        }

        private static MappedSdgwCommand MapGsa(SdhCommand command)
        {
            byte[] payload;

            if (string.Equals(command.Target, "GSA.led", StringComparison.OrdinalIgnoreCase))
            {
                payload = BuildTlvPayload(
                    GwProtocol.GsaSetLedType,
                    ParseState(command.Args["state"]) ? (byte)0x01 : (byte)0x00);
            }
            else if (string.Equals(command.Target, "GSA.channel.setpoint", StringComparison.OrdinalIgnoreCase))
            {
                payload = BuildTlvPayload(
                    GwProtocol.GsaChannelSetpointType,
                    ParseChannel(command),
                    ParseByte(command, "value"));
            }
            else if (string.Equals(command.Target, "GSA.channel.enable", StringComparison.OrdinalIgnoreCase))
            {
                payload = BuildTlvPayload(
                    GwProtocol.GsaChannelEnableType,
                    ParseChannel(command),
                    ParseState(command.Args["state"]) ? (byte)0x01 : (byte)0x00);
            }
            else if (string.Equals(command.Target, "GSA.channels.enable", StringComparison.OrdinalIgnoreCase))
            {
                payload = BuildTlvPayload(
                    GwProtocol.GsaChannelsEnableType,
                    ParseState(command.Args["state"]) ? (byte)0x01 : (byte)0x00);
            }
            else if (string.Equals(command.Target, "GSA.channel.status", StringComparison.OrdinalIgnoreCase))
            {
                payload = BuildTlvPayload(
                    GwProtocol.GsaChannelStatusType,
                    ParseChannel(command));
            }
            else if (string.Equals(command.Target, "GSA.channels.status", StringComparison.OrdinalIgnoreCase))
            {
                payload = BuildTlvPayload(GwProtocol.GsaChannelsStatusType);
            }
            else if (string.Equals(command.Target, "GSA.channel.fault", StringComparison.OrdinalIgnoreCase))
            {
                payload = BuildTlvPayload(
                    GwProtocol.GsaChannelFaultResetType,
                    ParseChannel(command));
            }
            else if (string.Equals(command.Target, "GSA.channel.offset", StringComparison.OrdinalIgnoreCase))
            {
                payload = MapGsaChannelOffset(command);
            }
            else if (string.Equals(command.Target, "GSA.offset", StringComparison.OrdinalIgnoreCase))
            {
                payload = BuildTlvPayload(GwProtocol.GsaOffsetResetType);
            }
            else
            {
                throw new NotSupportedException("Mapeamento SDH->SDGW ainda não suporta target: " + command.Target + ".");
            }

            return new MappedSdgwCommand
            {
                Cmd = GwProtocol.MakeCompactCommand(GwProtocol.GsaAddress, GwProtocol.GsaTlvTransactOp),
                Payload = payload,
                RequireAck = true,
                TimeoutMs = DefaultBoardTimeoutMs,
                Retries = DefaultBoardRetries
            };
        }

        private static MappedSdgwCommand MapUce(SdhCommand command)
        {
            byte[] payload;

            if (string.Equals(command.Target, "UCE.led", StringComparison.OrdinalIgnoreCase))
            {
                payload = BuildTlvPayload(
                    GwProtocol.UceSetLedType,
                    ParseState(command.Args["state"]) ? (byte)0x01 : (byte)0x00);
            }
            else if (string.Equals(command.Target, "UCE.can.config", StringComparison.OrdinalIgnoreCase))
            {
                payload = BuildTlvPayload(
                    GwProtocol.UceCanConfigType,
                    ParseUceController(command),
                    ParseUceBitrateCode(command),
                    ParseUceMode(command));
            }
            else if (string.Equals(command.Target, "UCE.can.enable", StringComparison.OrdinalIgnoreCase))
            {
                payload = BuildTlvPayload(
                    GwProtocol.UceCanEnableType,
                    ParseUceController(command),
                    ParseState(command.Args["state"]) ? GwProtocol.UceCanStateOn : GwProtocol.UceCanStateOff);
            }
            else if (string.Equals(command.Target, "UCE.can.status", StringComparison.OrdinalIgnoreCase))
            {
                payload = BuildTlvPayload(
                    GwProtocol.UceCanStatusType,
                    ParseUceController(command));
            }
            else if (string.Equals(command.Target, "UCE.can", StringComparison.OrdinalIgnoreCase) &&
                     string.Equals(command.Op, "reset", StringComparison.OrdinalIgnoreCase))
            {
                payload = BuildTlvPayload(
                    GwProtocol.UceCanResetType,
                    ParseUceController(command));
            }
            else
            {
                throw new NotSupportedException("Mapeamento SDH->SDGW ainda não suporta target: " + command.Target + ".");
            }

            return new MappedSdgwCommand
            {
                Cmd = GwProtocol.MakeCompactCommand(GwProtocol.UceAddress, GwProtocol.UceTlvTransactOp),
                Payload = payload,
                RequireAck = true,
                TimeoutMs = DefaultBoardTimeoutMs,
                Retries = DefaultBoardRetries
            };
        }

        private static byte[] MapGsaChannelOffset(SdhCommand command)
        {
            byte channel = ParseChannel(command);

            if (string.Equals(command.Op, "set", StringComparison.OrdinalIgnoreCase))
            {
                short value = ParseInt16(command, "value");
                byte[] offsetBytes = BitConverter.GetBytes(value);
                return BuildTlvPayload(
                    GwProtocol.GsaChannelOffsetSetType,
                    channel,
                    ParseOffsetKind(command.Args["kind"]),
                    offsetBytes[0],
                    offsetBytes[1]);
            }

            if (string.Equals(command.Op, "get", StringComparison.OrdinalIgnoreCase))
            {
                return BuildTlvPayload(
                    GwProtocol.GsaChannelOffsetGetType,
                    channel,
                    ParseOffsetKind(command.Args["kind"]));
            }

            if (string.Equals(command.Op, "save", StringComparison.OrdinalIgnoreCase))
                return BuildTlvPayload(GwProtocol.GsaChannelOffsetSaveType, channel);

            if (string.Equals(command.Op, "reset", StringComparison.OrdinalIgnoreCase))
                return BuildTlvPayload(GwProtocol.GsaChannelOffsetResetType, channel);

            throw new NotSupportedException("Mapeamento SDH->SDGW ainda não suporta op: " + command.Op + ".");
        }

        private static byte[] BuildTlvPayload(byte type, params byte[] data)
        {
            int payloadLength = data != null ? data.Length : 0;
            byte[] payload = new byte[payloadLength + 3];
            payload[0] = type;
            payload[1] = (byte)payloadLength;

            if (payloadLength > 0)
                Buffer.BlockCopy(data, 0, payload, 2, payloadLength);

            payload[payload.Length - 1] = SdgwFrameCodec.Crc8Atm(payload, 0, payload.Length - 1);
            return payload;
        }

        private static byte ParseChannel(SdhCommand command)
        {
            int channel = ParseInt(command, "channel");
            return checked((byte)channel);
        }

        private static byte ParseByte(SdhCommand command, string argName)
        {
            int value = ParseInt(command, argName);
            return checked((byte)value);
        }

        private static short ParseInt16(SdhCommand command, string argName)
        {
            int value = ParseInt(command, argName);
            return checked((short)value);
        }

        private static int ParseInt(SdhCommand command, string argName)
        {
            string rawValue = command.Args[argName];
            return int.Parse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        private static bool ParseState(string state)
        {
            return string.Equals(state, "on", StringComparison.OrdinalIgnoreCase);
        }

        private static byte ParseOffsetKind(string kind)
        {
            if (string.Equals(kind, "vout", StringComparison.OrdinalIgnoreCase))
                return GwProtocol.GsaOffsetKindVout;

            if (string.Equals(kind, "vread", StringComparison.OrdinalIgnoreCase))
                return GwProtocol.GsaOffsetKindVread;

            if (string.Equals(kind, "iread", StringComparison.OrdinalIgnoreCase))
                return GwProtocol.GsaOffsetKindIread;

            throw new InvalidOperationException("Kind inválido para mapeamento SDH->SDGW: " + kind + ".");
        }

        private static byte ParseUceController(SdhCommand command)
        {
            UceCanController controller;
            if (!UceCanProtocol.TryParseController(command.Args["controller"], out controller) ||
                !UceCanProtocol.TryEncodeController(controller, out byte code))
            {
                throw new InvalidOperationException("Controller inválido para mapeamento SDH->SDGW: " + command.Args["controller"] + ".");
            }

            return code;
        }

        private static byte ParseUceBitrateCode(SdhCommand command)
        {
            int bitrate = ParseInt(command, "bitrate");
            if (!UceCanProtocol.TryEncodeBitrate(bitrate, out byte code))
                throw new InvalidOperationException("Bitrate inválido para mapeamento SDH->SDGW: " + bitrate.ToString(CultureInfo.InvariantCulture) + ".");

            return code;
        }

        private static byte ParseUceMode(SdhCommand command)
        {
            UceCanMode mode;
            if (!UceCanProtocol.TryParseMode(command.Args["mode"], out mode) ||
                !UceCanProtocol.TryEncodeMode(mode, out byte code))
            {
                throw new InvalidOperationException("Mode inválido para mapeamento SDH->SDGW: " + command.Args["mode"] + ".");
            }

            return code;
        }
    }
}
```

Arquivo: `C:\PROJETOS\SimulDIESEL\local-api\src\SimulDIESEL\SimulDIESEL\BLL\Boards\UCE\UceClient.cs`  
Trecho: linhas 1-323  
Papel: Cliente de alto nível da UCE: cria comandos LED/CAN, aguarda resposta síncrona e valida type/len.

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using SimulDIESEL.DAL.Protocols.SDGW;
using SimulDIESEL.DTL.Boards.UCE;
using SimulDIESEL.DTL.Protocols.SDGW;

namespace SimulDIESEL.BLL.Boards.UCE
{
    public sealed class UceClient : IDisposable
    {
        private delegate bool UceResponseParser<T>(SdgwFrame frame, out T response, out string error)
            where T : class;

        private sealed class PendingUceRequest
        {
            public TaskCompletionSource<SdgwFrame> ResponseSource { get; set; }
            public Func<SdgwFrame, bool> MatchFrame { get; set; }
        }

        private const int ResponseTimeoutMs = 2000;

        private readonly SdhClient _sdh;
        private readonly SdgwSession _sdgwSession;
        private readonly SemaphoreSlim _requestGate = new SemaphoreSlim(1, 1);

        private PendingUceRequest _pendingRequest;
        private bool _disposed;

        public UceClient(SdhClient sdh, SdgwSession sdgwSession)
        {
            _sdh = sdh ?? throw new ArgumentNullException(nameof(sdh));
            _sdgwSession = sdgwSession ?? throw new ArgumentNullException(nameof(sdgwSession));

            _sdgwSession.FrameReceived += OnFrameReceived;
        }

        public async Task<UceCommandResult> SetBuiltinLedAsync(bool on)
        {
            UceOperationResult<UceLedResponse> result = await ExecuteOperationAsync<UceLedResponse>(
                CreateLedCommand(on),
                GwProtocol.UceSetLedType,
                GwProtocol.UceLedPayloadLength,
                "LED builtin da UCE",
                UceParsers.TryReadBuiltinLedResponse).ConfigureAwait(false);

            if (!result.Success || result.Response == null)
                return UceCommandResult.Fail(result.Message, result.SendOutcome);

            return UceCommandResult.Succeeded(
                result.Response.AcceptedState,
                result.SendOutcome ?? SdGwLinkEngine.SendOutcome.Acked,
                result.Message);
        }

        public Task<UceOperationResult<UceCanConfigResponse>> SetCanConfigAsync(string controller, int bitrateKbps, string mode)
        {
            return ExecuteOperationAsync<UceCanConfigResponse>(
                CreateCanConfigCommand(controller, bitrateKbps, mode),
                GwProtocol.UceCanConfigType,
                GwProtocol.UceCanConfigPayloadLength,
                "configuração CAN da UCE",
                UceParsers.TryReadCanConfigResponse);
        }

        public Task<UceOperationResult<UceCanEnableResponse>> SetCanEnabledAsync(string controller, bool enabled)
        {
            return ExecuteOperationAsync<UceCanEnableResponse>(
                CreateCanEnableCommand(controller, enabled),
                GwProtocol.UceCanEnableType,
                GwProtocol.UceCanEnablePayloadLength,
                "habilitação CAN da UCE",
                UceParsers.TryReadCanEnableResponse);
        }

        public Task<UceOperationResult<UceCanStatusResponse>> GetCanStatusAsync(string controller)
        {
            return ExecuteOperationAsync<UceCanStatusResponse>(
                CreateCanStatusCommand(controller),
                GwProtocol.UceCanStatusType,
                GwProtocol.UceCanStatusResponsePayloadLength,
                "status CAN da UCE",
                UceParsers.TryReadCanStatusResponse);
        }

        public Task<UceOperationResult<UceCanResetResponse>> ResetCanAsync(string controller)
        {
            return ExecuteOperationAsync<UceCanResetResponse>(
                CreateCanResetCommand(controller),
                GwProtocol.UceCanResetType,
                GwProtocol.UceCanResetResponsePayloadLength,
                "reset CAN da UCE",
                UceParsers.TryReadCanResetResponse);
        }

        private async Task<UceOperationResult<T>> ExecuteOperationAsync<T>(
            SdhCommand command,
            byte expectedType,
            byte expectedLen,
            string operationName,
            UceResponseParser<T> parser)
            where T : class
        {
            ThrowIfDisposed();

            await _requestGate.WaitAsync().ConfigureAwait(false);
            try
            {
                _pendingRequest = new PendingUceRequest
                {
                    ResponseSource = new TaskCompletionSource<SdgwFrame>(TaskCreationOptions.RunContinuationsAsynchronously),
                    MatchFrame = frame => MatchesExpectedResponse(frame, expectedType, expectedLen)
                };

                SdGwLinkEngine.SendOutcome outcome = await _sdh.SendAsync(
                    command,
                    SdGwTxPriority.High,
                    operationName).ConfigureAwait(false);

                if (outcome != SdGwLinkEngine.SendOutcome.Acked)
                    return UceOperationResult<T>.Fail(TranslateOutcome(outcome, operationName), outcome);

                SdgwFrame responseFrame = await WaitForResponseAsync(_pendingRequest).ConfigureAwait(false);

                string gatewayErrorMessage;
                string gatewayParseError;
                if (UceParsers.TryReadGatewayError(responseFrame, out gatewayErrorMessage, out gatewayParseError))
                    return UceOperationResult<T>.Fail(gatewayErrorMessage, outcome);

                string functionalErrorMessage;
                string functionalParseError;
                if (UceParsers.TryReadFunctionalError(responseFrame, out functionalErrorMessage, out functionalParseError))
                    return UceOperationResult<T>.Fail(functionalErrorMessage, outcome);

                T response;
                string error;
                if (!parser(responseFrame, out response, out error))
                    return UceOperationResult<T>.Fail(error, outcome);

                return UceOperationResult<T>.Succeeded(
                    response,
                    outcome,
                    "Resposta síncrona recebida da UCE para " + operationName + ".");
            }
            catch (OperationCanceledException)
            {
                return UceOperationResult<T>.Fail("Timeout aguardando a resposta da UCE para " + operationName + ".");
            }
            catch (Exception ex)
            {
                return UceOperationResult<T>.Fail("Falha ao processar a resposta da UCE para " + operationName + ": " + ex.Message);
            }
            finally
            {
                _pendingRequest = null;
                _requestGate.Release();
            }
        }

        private void OnFrameReceived(SdgwFrame frame)
        {
            PendingUceRequest pending = _pendingRequest;
            if (pending == null || frame == null)
                return;

            if ((frame.Flags & 0x02) != 0)
                return;

            if (frame.Cmd != GwProtocol.MakeCompactCommand(GwProtocol.UceAddress, GwProtocol.UceTlvTransactOp))
                return;

            Func<SdgwFrame, bool> matcher = pending.MatchFrame;
            if (matcher == null || !matcher(frame))
                return;

            pending.ResponseSource.TrySetResult(frame);
        }

        private static async Task<SdgwFrame> WaitForResponseAsync(PendingUceRequest pendingRequest)
        {
            if (pendingRequest == null)
                throw new OperationCanceledException();

            Task finished = await Task.WhenAny(
                pendingRequest.ResponseSource.Task,
                Task.Delay(ResponseTimeoutMs)).ConfigureAwait(false);

            if (finished != pendingRequest.ResponseSource.Task)
                throw new OperationCanceledException();

            return await pendingRequest.ResponseSource.Task.ConfigureAwait(false);
        }

        private static bool MatchesExpectedResponse(SdgwFrame frame, byte expectedType, byte expectedLen)
        {
            if (frame?.Payload == null)
                return false;

            if (frame.Payload.Length >= 2 &&
                frame.Payload[0] == expectedType &&
                frame.Payload[1] == expectedLen)
            {
                return true;
            }

            if (frame.Payload.Length >= 5 &&
                frame.Payload[0] == GwProtocol.UceErrorType &&
                frame.Payload[1] == 0x03 &&
                frame.Payload[2] == expectedType)
            {
                return true;
            }

            if (frame.Payload.Length >= 3 &&
                frame.Payload[0] == GwProtocol.GatewayErrorType &&
                frame.Payload[1] >= 0x01)
            {
                return true;
            }

            return false;
        }

        private static SdhCommand CreateLedCommand(bool on)
        {
            var command = new SdhCommand
            {
                Target = "UCE.led",
                Op = "set"
            };

            command.Args["state"] = on ? "on" : "off";
            return command;
        }

        private static SdhCommand CreateCanConfigCommand(string controller, int bitrateKbps, string mode)
        {
            var command = new SdhCommand
            {
                Target = "UCE.can.config",
                Op = "set"
            };

            command.Args["controller"] = controller;
            command.Args["bitrate"] = bitrateKbps.ToString(System.Globalization.CultureInfo.InvariantCulture);
            command.Args["mode"] = mode;
            return command;
        }

        private static SdhCommand CreateCanEnableCommand(string controller, bool enabled)
        {
            var command = new SdhCommand
            {
                Target = "UCE.can.enable",
                Op = "set"
            };

            command.Args["controller"] = controller;
            command.Args["state"] = enabled ? "on" : "off";
            return command;
        }

        private static SdhCommand CreateCanStatusCommand(string controller)
        {
            var command = new SdhCommand
            {
                Target = "UCE.can.status",
                Op = "get"
            };

            command.Args["controller"] = controller;
            return command;
        }

        private static SdhCommand CreateCanResetCommand(string controller)
        {
            var command = new SdhCommand
            {
                Target = "UCE.can",
                Op = "reset"
            };

            command.Args["controller"] = controller;
            return command;
        }

        private static string TranslateOutcome(SdGwLinkEngine.SendOutcome outcome, string operationName)
        {
            switch (outcome)
            {
                case SdGwLinkEngine.SendOutcome.Nacked:
                    return "A BPM rejeitou o comando para " + operationName + ".";
                case SdGwLinkEngine.SendOutcome.Timeout:
                    return "Timeout aguardando ACK do gateway para " + operationName + ".";
                case SdGwLinkEngine.SendOutcome.TransportDown:
                    return "O transporte ativo da BPM está indisponível no momento.";
                case SdGwLinkEngine.SendOutcome.Busy:
                    return "O link estava temporariamente ocupado. Tente novamente.";
                case SdGwLinkEngine.SendOutcome.Enqueued:
                    return "O comando foi enfileirado, mas não houve confirmação do gateway.";
                default:
                    return "Falha ao enviar comando para " + operationName + ".";
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(UceClient));
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _sdgwSession.FrameReceived -= OnFrameReceived;
            _requestGate.Dispose();
            _disposed = true;
        }

    }
}
```

Arquivo: `C:\PROJETOS\SimulDIESEL\local-api\src\SimulDIESEL\SimulDIESEL\BLL\Boards\UCE\UceGatewayDiagnosticLog.cs`  
Trecho: linhas 1-366  
Papel: Parser e logger do snapshot diagnóstico devolvido pela BPM em caso de erro, incluindo CRC inválido.

```csharp
using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace SimulDIESEL.BLL.Boards.UCE
{
    internal sealed class UceGatewayDiagnostic
    {
        public const byte ExtendedVersion = 0x01;
        public const byte LayerBpm = 0x01;
        public const byte LayerGwSpiBus = 0x02;
        public const byte LayerCrcValidation = 0x03;

        public const byte PhaseWrite = 0x01;
        public const byte PhaseWaitResponseReady = 0x02;
        public const byte PhaseReadHeader = 0x03;
        public const byte PhaseReadPayload = 0x04;
        public const byte PhaseFinalCrcValidation = 0x05;

        public const byte CauseFirstByteMisaligned = 0x01;
        public const byte CausePreloadFailure = 0x02;
        public const byte CauseWrongCrcPolynomial = 0x03;
        public const byte CauseEarlyReadBeforeResponseReady = 0x04;
        public const byte CauseLengthMismatch = 0x05;
        public const byte CauseTimeoutWaitingIrq = 0x06;
        public const byte CauseIncompleteFrame = 0x07;

        public static readonly string LogDirectory = @"C:\PROJETOS\SimulDIESEL\out\error_logs";
        public static readonly string LogFilePath = Path.Combine(LogDirectory, "uce_spi_crc_error_log.txt");

        public byte ErrorCode { get; set; }
        public bool HasExtendedData { get; set; }
        public byte Version { get; set; }
        public byte Layer { get; set; }
        public byte Phase { get; set; }
        public byte Cause { get; set; }
        public byte ExpectedLength { get; set; }
        public byte ReceivedLength { get; set; }
        public byte CrcCalculated { get; set; }
        public byte CrcReceived { get; set; }
        public byte[] TxBytes { get; set; } = Array.Empty<byte>();
        public byte[] RxBytes { get; set; } = Array.Empty<byte>();
        public byte[] RawDiagnosticValue { get; set; } = Array.Empty<byte>();
        public string ParseIssue { get; set; }
    }

    internal static class UceGatewayDiagnosticLog
    {
        public static UceGatewayDiagnostic Create(byte[] gatewayData)
        {
            var diagnostic = new UceGatewayDiagnostic();

            if (gatewayData == null || gatewayData.Length == 0)
            {
                diagnostic.ParseIssue = "Payload de diagnóstico da BPM ausente.";
                return diagnostic;
            }

            diagnostic.ErrorCode = gatewayData[0];
            diagnostic.RawDiagnosticValue = Copy(gatewayData);

            if (gatewayData.Length == 1)
                return diagnostic;

            diagnostic.HasExtendedData = gatewayData.Length >= 11;
            diagnostic.Version = gatewayData.Length > 1 ? gatewayData[1] : (byte)0x00;
            diagnostic.Layer = gatewayData.Length > 2 ? gatewayData[2] : (byte)0x00;
            diagnostic.Phase = gatewayData.Length > 3 ? gatewayData[3] : (byte)0x00;
            diagnostic.Cause = gatewayData.Length > 4 ? gatewayData[4] : (byte)0x00;
            byte txLen = gatewayData.Length > 5 ? gatewayData[5] : (byte)0x00;
            byte rxLen = gatewayData.Length > 6 ? gatewayData[6] : (byte)0x00;
            diagnostic.ExpectedLength = gatewayData.Length > 7 ? gatewayData[7] : (byte)0x00;
            diagnostic.ReceivedLength = gatewayData.Length > 8 ? gatewayData[8] : (byte)0x00;
            diagnostic.CrcCalculated = gatewayData.Length > 9 ? gatewayData[9] : (byte)0x00;
            diagnostic.CrcReceived = gatewayData.Length > 10 ? gatewayData[10] : (byte)0x00;

            if (gatewayData.Length < 11)
            {
                diagnostic.ParseIssue = "Payload estendido do gateway veio truncado.";
                return diagnostic;
            }

            int cursor = 11;
            int availableTx = Math.Max(0, gatewayData.Length - cursor);
            availableTx = Math.Min(availableTx, txLen);
            diagnostic.TxBytes = Slice(gatewayData, cursor, availableTx);
            cursor += availableTx;

            int availableRx = Math.Max(0, gatewayData.Length - cursor);
            availableRx = Math.Min(availableRx, rxLen);
            diagnostic.RxBytes = Slice(gatewayData, cursor, availableRx);

            if (availableTx != txLen || availableRx != rxLen)
            {
                diagnostic.ParseIssue = "Payload estendido do gateway não bate com os comprimentos declarados.";
            }

            return diagnostic;
        }

        public static void Append(UceGatewayDiagnostic diagnostic)
        {
            if (diagnostic == null)
                return;

            Directory.CreateDirectory(UceGatewayDiagnostic.LogDirectory);

            var builder = new StringBuilder();
            builder.AppendLine("============================================================");
            builder.Append("TIMESTAMP = ");
            builder.AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture));
            builder.Append("LAYER = ");
            builder.AppendLine(GetLayerText(diagnostic.Layer));
            builder.Append("STATUS = 0x");
            builder.AppendLine(diagnostic.ErrorCode.ToString("X2", CultureInfo.InvariantCulture));
            builder.AppendLine("WRITE PHASE:");
            builder.Append("TX: ");
            builder.AppendLine(FormatBytes(diagnostic.TxBytes));
            builder.AppendLine("READ HEADER PHASE:");
            builder.Append("TX: ");
            builder.AppendLine(GetHeaderPhaseTx(diagnostic));
            builder.Append("RX: ");
            builder.AppendLine(GetHeaderPhaseRx(diagnostic));
            builder.AppendLine("READ PAYLOAD PHASE:");
            builder.Append("TX: ");
            builder.AppendLine(GetPayloadPhaseTx(diagnostic));
            builder.Append("RX: ");
            builder.AppendLine(GetPayloadPhaseRx(diagnostic));
            builder.Append("RX RAW: ");
            builder.AppendLine(FormatBytes(diagnostic.RxBytes));

            AppendFrameInterpretation(builder, diagnostic);

            builder.Append("CRC CALCULATED = ");
            builder.AppendLine(FormatByteValue(diagnostic.CrcCalculated, diagnostic.HasExtendedData));
            builder.Append("CRC RECEIVED   = ");
            builder.AppendLine(FormatByteValue(diagnostic.CrcReceived, diagnostic.HasExtendedData));
            builder.Append("CRC MATCH      = ");
            builder.AppendLine(diagnostic.HasExtendedData
                ? (diagnostic.CrcCalculated == diagnostic.CrcReceived ? "TRUE" : "FALSE")
                : "UNKNOWN");
            builder.Append("EXPECTED LENGTH = ");
            builder.AppendLine(FormatLength(diagnostic.ExpectedLength, diagnostic.HasExtendedData));
            builder.Append("RECEIVED LENGTH = ");
            builder.AppendLine(FormatLength(diagnostic.ReceivedLength, diagnostic.HasExtendedData));
            builder.Append("ERROR PHASE = ");
            builder.AppendLine(GetPhaseText(diagnostic.Phase));
            builder.Append("POSSIBLE CAUSE = ");
            builder.AppendLine(GetCauseText(diagnostic.Cause));
            if (!string.IsNullOrWhiteSpace(diagnostic.ParseIssue))
            {
                builder.Append("PARSE NOTE = ");
                builder.AppendLine(diagnostic.ParseIssue);
            }
            builder.Append("RAW GATEWAY VALUE = ");
            builder.AppendLine(FormatBytes(diagnostic.RawDiagnosticValue));
            builder.AppendLine();

            File.AppendAllText(UceGatewayDiagnostic.LogFilePath, builder.ToString(), Encoding.ASCII);
        }

        public static string BuildCrcMessage(UceGatewayDiagnostic diagnostic)
        {
            if (diagnostic == null)
            {
                return "A BPM informou CRC inválido na resposta da UCE." +
                    Environment.NewLine +
                    "Consulte:" +
                    Environment.NewLine +
                    UceGatewayDiagnostic.LogFilePath;
            }

            if (!diagnostic.HasExtendedData)
            {
                return "A BPM informou CRC inválido na resposta da UCE." +
                    Environment.NewLine +
                    "Consulte:" +
                    Environment.NewLine +
                    UceGatewayDiagnostic.LogFilePath;
            }

            return "A BPM informou CRC inválido na resposta da UCE." +
                Environment.NewLine +
                "Esperado: " + FormatByteValue(diagnostic.CrcCalculated, true) +
                Environment.NewLine +
                "Recebido: " + FormatByteValue(diagnostic.CrcReceived, true) +
                Environment.NewLine +
                "Consulte:" +
                Environment.NewLine +
                UceGatewayDiagnostic.LogFilePath;
        }

        private static void AppendFrameInterpretation(StringBuilder builder, UceGatewayDiagnostic diagnostic)
        {
            if (diagnostic == null || diagnostic.RxBytes == null || diagnostic.RxBytes.Length < 2)
            {
                builder.AppendLine("FRAME INTERPRETATION = INSUFFICIENT DATA");
                return;
            }

            byte type = diagnostic.RxBytes[0];
            byte len = diagnostic.RxBytes[1];
            int availableValueLength = Math.Max(0, diagnostic.RxBytes.Length - 3);
            availableValueLength = Math.Min(availableValueLength, len);

            builder.Append("T = ");
            builder.AppendLine(FormatByteValue(type, true));
            builder.Append("L = ");
            builder.AppendLine(FormatByteValue(len, true));

            if (availableValueLength <= 0)
            {
                builder.AppendLine("V = <EMPTY>");
                return;
            }

            byte[] valueBytes = Slice(diagnostic.RxBytes, 2, availableValueLength);
            builder.Append("V = ");
            if (valueBytes.Length == 1)
                builder.AppendLine(FormatByteValue(valueBytes[0], true));
            else
                builder.AppendLine(FormatBytes(valueBytes));
        }

        private static string GetLayerText(byte layer)
        {
            switch (layer)
            {
                case UceGatewayDiagnostic.LayerBpm:
                    return "BPM";
                case UceGatewayDiagnostic.LayerGwSpiBus:
                    return "BPM / GwSpiBus";
                case UceGatewayDiagnostic.LayerCrcValidation:
                    return "BPM / GwSpiBus / CRC validation";
                default:
                    return "BPM / layer unknown";
            }
        }

        private static string GetPhaseText(byte phase)
        {
            switch (phase)
            {
                case UceGatewayDiagnostic.PhaseWrite:
                    return "WRITE PHASE";
                case UceGatewayDiagnostic.PhaseWaitResponseReady:
                    return "WAIT RESPONSE READY";
                case UceGatewayDiagnostic.PhaseReadHeader:
                    return "READ HEADER";
                case UceGatewayDiagnostic.PhaseReadPayload:
                    return "READ PAYLOAD";
                case UceGatewayDiagnostic.PhaseFinalCrcValidation:
                    return "FINAL CRC VALIDATION";
                default:
                    return "UNKNOWN";
            }
        }

        private static string GetCauseText(byte cause)
        {
            switch (cause)
            {
                case UceGatewayDiagnostic.CauseFirstByteMisaligned:
                    return "FIRST BYTE MISALIGNED";
                case UceGatewayDiagnostic.CausePreloadFailure:
                    return "PRELOAD FAILURE";
                case UceGatewayDiagnostic.CauseWrongCrcPolynomial:
                    return "WRONG CRC POLYNOMIAL";
                case UceGatewayDiagnostic.CauseEarlyReadBeforeResponseReady:
                    return "EARLY READ BEFORE RESPONSE READY";
                case UceGatewayDiagnostic.CauseLengthMismatch:
                    return "LENGTH MISMATCH";
                case UceGatewayDiagnostic.CauseTimeoutWaitingIrq:
                    return "TIMEOUT WAITING IRQ";
                case UceGatewayDiagnostic.CauseIncompleteFrame:
                    return "INCOMPLETE FRAME";
                default:
                    return "UNKNOWN";
            }
        }

        private static string GetHeaderPhaseTx(UceGatewayDiagnostic diagnostic)
        {
            if (diagnostic == null || diagnostic.RxBytes == null || diagnostic.RxBytes.Length == 0)
                return "<NOT CAPTURED>";

            return diagnostic.RxBytes.Length >= 2 ? "00 00" : "00";
        }

        private static string GetHeaderPhaseRx(UceGatewayDiagnostic diagnostic)
        {
            if (diagnostic == null || diagnostic.RxBytes == null || diagnostic.RxBytes.Length == 0)
                return "<NOT CAPTURED>";

            return FormatBytes(Slice(diagnostic.RxBytes, 0, Math.Min(2, diagnostic.RxBytes.Length)));
        }

        private static string GetPayloadPhaseTx(UceGatewayDiagnostic diagnostic)
        {
            if (diagnostic == null)
                return "<NOT CAPTURED>";

            int payloadPhaseLength = Math.Max(0, diagnostic.RxBytes.Length - 2);
            if (payloadPhaseLength == 0 && diagnostic.ExpectedLength > 2)
                payloadPhaseLength = diagnostic.ExpectedLength - 2;

            if (payloadPhaseLength <= 0)
                return "<NOT CAPTURED>";

            var bytes = new byte[payloadPhaseLength];
            return FormatBytes(bytes);
        }

        private static string GetPayloadPhaseRx(UceGatewayDiagnostic diagnostic)
        {
            if (diagnostic == null || diagnostic.RxBytes == null || diagnostic.RxBytes.Length <= 2)
                return "<NOT CAPTURED>";

            return FormatBytes(Slice(diagnostic.RxBytes, 2, diagnostic.RxBytes.Length - 2));
        }

        private static string FormatLength(byte value, bool available)
        {
            return available ? value.ToString(CultureInfo.InvariantCulture) : "UNKNOWN";
        }

        private static string FormatByteValue(byte value, bool available)
        {
            return available ? "0x" + value.ToString("X2", CultureInfo.InvariantCulture) : "UNKNOWN";
        }

        private static string FormatBytes(byte[] data)
        {
            if (data == null || data.Length == 0)
                return "<EMPTY>";

            var builder = new StringBuilder(data.Length * 3);
            for (int index = 0; index < data.Length; index++)
            {
                if (index > 0)
                    builder.Append(' ');

                builder.Append(data[index].ToString("X2", CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }

        private static byte[] Slice(byte[] source, int offset, int count)
        {
            if (source == null || count <= 0 || offset >= source.Length)
                return Array.Empty<byte>();

            int safeCount = Math.Min(count, source.Length - offset);
            var buffer = new byte[safeCount];
            Buffer.BlockCopy(source, offset, buffer, 0, safeCount);
            return buffer;
        }

        private static byte[] Copy(byte[] source)
        {
            return Slice(source, 0, source.Length);
        }
    }
}
```

