#pragma once
#include "ISggwEndpoint.h"
#include "SggwSessionOwner.h"

class SggwEndpointMux : public ISggwEndpoint {
public:
    SggwEndpointMux(SggwSessionOwner& sessionOwner, ISggwEndpoint& first, ISggwEndpoint& second)
    : _sessionOwner(sessionOwner), _count(0), _textEnabled(true)
    {
        _endpoints[0] = nullptr;
        _endpoints[1] = nullptr;
        addEndpoint(first);
        addEndpoint(second);
    }

    SggwEndpointKind kind() const override
    {
        return const_cast<SggwEndpointMux*>(this)->refreshOwnerAndGetKind();
    }

    const char* name() const override
    {
        ISggwEndpoint* endpoint = const_cast<SggwEndpointMux*>(this)->resolveActiveEndpoint();
        return endpoint ? endpoint->name() : "Nenhum";
    }

    int available() override
    {
        ISggwEndpoint* endpoint = resolveActiveEndpoint();
        return endpoint ? endpoint->available() : 0;
    }

    int readByte() override
    {
        ISggwEndpoint* endpoint = resolveActiveEndpoint();
        return endpoint ? endpoint->readByte() : -1;
    }

    void writeBytes(const uint8_t* data, size_t len) override
    {
        ISggwEndpoint* endpoint = resolveActiveEndpoint();
        if (endpoint)
            endpoint->writeBytes(data, len);
    }

    void flushTx() override
    {
        ISggwEndpoint* endpoint = resolveActiveEndpoint();
        if (endpoint)
            endpoint->flushTx();
    }

    void setTextEnabled(bool en) override
    {
        _textEnabled = en;
        for (uint8_t i = 0; i < _count; ++i)
        {
            if (_endpoints[i])
                _endpoints[i]->setTextEnabled(en);
        }
    }

    bool isTextEnabled() const override { return _textEnabled; }

    bool isConnected() const override
    {
        return const_cast<SggwEndpointMux*>(this)->resolveActiveEndpoint() != nullptr;
    }

    bool shouldClaimOwnership() override
    {
        return kind() != SGGW_ENDPOINT_NONE;
    }

private:
    void addEndpoint(ISggwEndpoint& endpoint)
    {
        if (_count >= 2)
            return;

        endpoint.setTextEnabled(_textEnabled);
        _endpoints[_count++] = &endpoint;
    }

    ISggwEndpoint* resolveActiveEndpoint()
    {
        SggwEndpointKind ownerKind = refreshOwnerAndGetKind();
        return findByKind(ownerKind);
    }

    SggwEndpointKind refreshOwnerAndGetKind()
    {
        if (_sessionOwner.hasOwner())
        {
            const SggwEndpointKind currentOwner = _sessionOwner.owner();
            ISggwEndpoint* ownerEndpoint = findByKind(currentOwner);
            if (ownerEndpoint && ownerEndpoint->isConnected())
                return currentOwner;

            _sessionOwner.release(currentOwner);
        }

        for (uint8_t i = 0; i < _count; ++i)
        {
            ISggwEndpoint* endpoint = _endpoints[i];
            if (endpoint == nullptr)
                continue;

            if (!endpoint->shouldClaimOwnership())
                continue;

            if (_sessionOwner.tryClaim(endpoint->kind()))
                return endpoint->kind();
        }

        return SGGW_ENDPOINT_NONE;
    }

    ISggwEndpoint* findByKind(SggwEndpointKind kind) const
    {
        for (uint8_t i = 0; i < _count; ++i)
        {
            ISggwEndpoint* endpoint = _endpoints[i];
            if (endpoint != nullptr && endpoint->kind() == kind)
                return endpoint;
        }

        return nullptr;
    }

private:
    SggwSessionOwner& _sessionOwner;
    ISggwEndpoint* _endpoints[2];
    uint8_t _count;
    bool _textEnabled;
};
