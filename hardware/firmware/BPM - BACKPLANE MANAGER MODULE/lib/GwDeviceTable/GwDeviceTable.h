#pragma once
#include <stdint.h>

enum GwBusType : uint8_t { GW_BUS_I2C = 1, GW_BUS_SPI = 2 };

struct GwDeviceEntry {
    uint8_t addr;       // 0x1..0xE
    GwBusType bus;
    // I2C
    uint8_t i2cAddr;    // 7-bit
    // SPI
    int8_t  spiCsPin;   // GPIO CS (se bus=SPI)
    int8_t  spiIrqPin;  // opcional
};

class GwDeviceTable {
public:
    // A tabela atual representa apenas os defaults de bootstrap das
    // baby boards externas. A BPM local (ADDR 0) nao participa daqui.
    static bool get(uint8_t addr, GwDeviceEntry& out);
};
