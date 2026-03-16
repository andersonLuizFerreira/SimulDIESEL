#pragma once
#include <stdint.h>
#include <stddef.h>

class IGwBus {
public:

    virtual ~IGwBus() = default;

    // Encaminha o payload interno ja resolvido da baby board.
    // A BPM apenas faz o binding logico->fisico do barramento.
    virtual bool transact(uint8_t addr,
                          const uint8_t* tx, size_t txLen,
                          uint8_t* rx, size_t rxMax, size_t& rxLen,
                          uint16_t timeoutMs) = 0;

    virtual bool isOk() const = 0;
};
