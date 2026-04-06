#pragma once
#include "ISdgwEndpoint.h"

// Politica minima de ownership para a sessao SDGW:
// apenas um endpoint alimenta o link por vez, mas Serial e Bluetooth
// podem disputar a posse de forma exclusiva.
class SdgwSessionOwner {
public:
    explicit SdgwSessionOwner(SdgwEndpointKind defaultOwner)
    : _hasOwner(true), _owner(defaultOwner) {}

    bool tryClaim(SdgwEndpointKind kind) {
        if (!_hasOwner) {
            _hasOwner = true;
            _owner = kind;
            return true;
        }

        if (_owner == kind) {
            return true;
        }

        return false;
    }

    void release(SdgwEndpointKind kind) {
        if (_hasOwner && _owner == kind) {
            _hasOwner = false;
        }
    }

    bool isOwner(SdgwEndpointKind kind) const {
        return _hasOwner && _owner == kind;
    }

    bool hasOwner() const { return _hasOwner; }
    SdgwEndpointKind owner() const { return _owner; }

private:
    bool _hasOwner;
    SdgwEndpointKind _owner;
};
