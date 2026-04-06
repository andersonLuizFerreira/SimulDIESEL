#pragma once
#include "ISdgwEndpoint.h"
#include "SdgwSessionOwner.h"

class SdgwEndpointMux : public ISdgwEndpoint {
public:
    SdgwEndpointMux(SdgwSessionOwner& sessionOwner, ISdgwEndpoint& first, ISdgwEndpoint& second)
    : _sessionOwner(sessionOwner), _count(0), _textEnabled(true)
    {
        _endpoints[0] = nullptr;
        _endpoints[1] = nullptr;
        addEndpoint(first);
        addEndpoint(second);
    }

    SdgwEndpointKind kind() const override
    {
        return const_cast<SdgwEndpointMux*>(this)->refreshOwnerAndGetKind();
    }

    const char* name() const override
    {
        ISdgwEndpoint* endpoint = const_cast<SdgwEndpointMux*>(this)->resolveActiveEndpoint();
        return endpoint ? endpoint->name() : "Nenhum";
    }

    int available() override
    {
        ISdgwEndpoint* endpoint = resolveActiveEndpoint();
        return endpoint ? endpoint->available() : 0;
    }

    int readByte() override
    {
        ISdgwEndpoint* endpoint = resolveActiveEndpoint();
        return endpoint ? endpoint->readByte() : -1;
    }

    void writeBytes(const uint8_t* data, size_t len) override
    {
        ISdgwEndpoint* endpoint = resolveActiveEndpoint();
        if (endpoint)
            endpoint->writeBytes(data, len);
    }

    void flushTx() override
    {
        ISdgwEndpoint* endpoint = resolveActiveEndpoint();
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
        return const_cast<SdgwEndpointMux*>(this)->resolveActiveEndpoint() != nullptr;
    }

    bool shouldClaimOwnership() override
    {
        return kind() != SDGW_ENDPOINT_NONE;
    }

private:
    void addEndpoint(ISdgwEndpoint& endpoint)
    {
        if (_count >= 2)
            return;

        endpoint.setTextEnabled(_textEnabled);
        _endpoints[_count++] = &endpoint;
    }

    ISdgwEndpoint* resolveActiveEndpoint()
    {
        SdgwEndpointKind ownerKind = refreshOwnerAndGetKind();
        return findByKind(ownerKind);
    }

    SdgwEndpointKind refreshOwnerAndGetKind()
    {
        if (_sessionOwner.hasOwner())
        {
            const SdgwEndpointKind currentOwner = _sessionOwner.owner();
            ISdgwEndpoint* ownerEndpoint = findByKind(currentOwner);
            if (ownerEndpoint && ownerEndpoint->isConnected())
                return currentOwner;

            _sessionOwner.release(currentOwner);
        }

        for (uint8_t i = 0; i < _count; ++i)
        {
            ISdgwEndpoint* endpoint = _endpoints[i];
            if (endpoint == nullptr)
                continue;

            if (!endpoint->shouldClaimOwnership())
                continue;

            if (_sessionOwner.tryClaim(endpoint->kind()))
                return endpoint->kind();
        }

        return SDGW_ENDPOINT_NONE;
    }

    ISdgwEndpoint* findByKind(SdgwEndpointKind kind) const
    {
        for (uint8_t i = 0; i < _count; ++i)
        {
            ISdgwEndpoint* endpoint = _endpoints[i];
            if (endpoint != nullptr && endpoint->kind() == kind)
                return endpoint;
        }

        return nullptr;
    }

private:
    SdgwSessionOwner& _sessionOwner;
    ISdgwEndpoint* _endpoints[2];
    uint8_t _count;
    bool _textEnabled;
};
