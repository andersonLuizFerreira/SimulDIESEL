#pragma once
#include <stdint.h>
#include <stddef.h>

enum SggwEndpointKind : uint8_t {
    SGGW_ENDPOINT_NONE = 0x00,
    SGGW_ENDPOINT_SERIAL = 0x01,
    SGGW_ENDPOINT_BLUETOOTH = 0x02
};

class ISggwEndpoint {
public:
    virtual ~ISggwEndpoint() = default;

    virtual SggwEndpointKind kind() const = 0;
    virtual const char* name() const = 0;

    virtual int available() = 0;
    virtual int readByte() = 0;
    virtual void writeBytes(const uint8_t* data, size_t len) = 0;
    virtual void flushTx() = 0;

    virtual void setTextEnabled(bool en) = 0;
    virtual bool isTextEnabled() const = 0;

    virtual bool isConnected() const = 0;
    virtual bool shouldClaimOwnership() = 0;
};
