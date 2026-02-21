#pragma once
#include <stdint.h>
#include <stddef.h>

class IGwBus {
public:
    virtual ~IGwBus() = default;

    // Envia frame TLV para um "addr" (lógico), e obtém resposta TLV.
    // Implementação pode mapear addr->i2cAddr ou addr->CS.
    virtual bool transact(uint8_t addr,
                          const uint8_t* tx, size_t txLen,
                          uint8_t* rx, size_t rxMax, size_t& rxLen,
                          uint16_t timeoutMs) = 0;

    virtual bool isOk() const = 0;
};