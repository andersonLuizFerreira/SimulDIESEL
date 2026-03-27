#pragma once
#include "ISggwEndpoint.h"

// Politica minima de ownership para a primeira versao multi-endpoint:
// apenas um tipo de endpoint pode alimentar a sessao SGGW por vez.
// Nesta fase a Serial e o owner default e unico em uso.
class SggwSessionOwner {
public:
    explicit SggwSessionOwner(SggwEndpointKind defaultOwner)
    : _hasOwner(true), _owner(defaultOwner) {}

    bool tryClaim(SggwEndpointKind kind) {
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

    void release(SggwEndpointKind kind) {
        if (_hasOwner && _owner == kind) {
            _hasOwner = false;
        }
    }

    bool isOwner(SggwEndpointKind kind) const {
        return _hasOwner && _owner == kind;
    }

    bool hasOwner() const { return _hasOwner; }
    SggwEndpointKind owner() const { return _owner; }

private:
    bool _hasOwner;
    SggwEndpointKind _owner;
};
