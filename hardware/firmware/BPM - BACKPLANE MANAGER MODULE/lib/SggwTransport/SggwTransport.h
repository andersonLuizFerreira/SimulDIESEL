#pragma once
#include <stdint.h>
#include <stddef.h>
#include <Arduino.h>
#include "Sggw.defs.h"
#include "ISggwEndpoint.h"

class SggwTransport : public ISggwEndpoint {
public:
    explicit SggwTransport(HardwareSerial& serial)
    : _ser(serial), _textEnabled(true) {}

    void begin() { _ser.begin(SGGW_UART_BAUDRATE, SGGW_UART_CONFIG); }
    void begin(uint32_t baud) { _ser.begin(baud, SGGW_UART_CONFIG); } // opcional

    SggwEndpointKind kind() const override { return SGGW_ENDPOINT_SERIAL; }
    const char* name() const override { return "Serial"; }

    int  available() override { return _ser.available(); }
    int  readByte() override { return _ser.read(); }

    void writeBytes(const uint8_t* data, size_t len) override {
        if (!data || len == 0) return;
        _ser.write(data, len);
    }

    void flushTx() override { _ser.flush(); }

    // =========================
    // (6) TEXTO: habilita/desabilita logs
    // =========================
    void setTextEnabled(bool en) override { _textEnabled = en; }
    bool isTextEnabled() const override { return _textEnabled; }
    bool isConnected() const override { return true; }
    bool shouldClaimOwnership() override { return _ser.available() > 0; }

    // wrappers seguros (use estes no seu código ao invés de Serial.print)
    void print(const char* s) {
        if (!_textEnabled || s == nullptr) return;
        _ser.print(s);
    }
    void println(const char* s) {
        if (!_textEnabled || s == nullptr) return;
        _ser.println(s);
    }
    void println() {
        if (!_textEnabled) return;
        _ser.println();
    }

private:
    HardwareSerial& _ser;
    bool _textEnabled;
};
