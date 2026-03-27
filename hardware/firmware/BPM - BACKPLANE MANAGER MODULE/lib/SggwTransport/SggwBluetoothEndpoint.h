#pragma once
#include <Arduino.h>
#include <BluetoothSerial.h>
#include "ISggwEndpoint.h"

class SggwBluetoothEndpoint : public ISggwEndpoint {
public:
    explicit SggwBluetoothEndpoint(const char* deviceName)
    : _deviceName(deviceName), _textEnabled(true), _started(false) {}

    bool begin()
    {
        if (_started)
            return true;

        _started = _serial.begin(_deviceName);
        return _started;
    }

    SggwEndpointKind kind() const override { return SGGW_ENDPOINT_BLUETOOTH; }
    const char* name() const override { return _deviceName; }

    int available() override { return _serial.available(); }
    int readByte() override { return _serial.read(); }

    void writeBytes(const uint8_t* data, size_t len) override
    {
        if (!data || len == 0 || !isConnected())
            return;

        _serial.write(data, len);
    }

    void flushTx() override
    {
        _serial.flush();
    }

    void setTextEnabled(bool en) override { _textEnabled = en; }
    bool isTextEnabled() const override { return _textEnabled; }

    bool isConnected() const override
    {
        return _started && const_cast<BluetoothSerial&>(_serial).hasClient();
    }

    bool shouldClaimOwnership() override
    {
        return isConnected();
    }

private:
    BluetoothSerial _serial;
    const char* _deviceName;
    bool _textEnabled;
    bool _started;
};
