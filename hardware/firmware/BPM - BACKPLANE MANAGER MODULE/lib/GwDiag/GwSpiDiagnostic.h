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
