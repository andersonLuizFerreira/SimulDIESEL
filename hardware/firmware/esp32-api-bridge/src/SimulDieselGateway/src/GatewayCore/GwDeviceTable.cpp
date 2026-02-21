#include "GwDeviceTable.h"

// Ajuste aqui conforme seu hardware
static const GwDeviceEntry kTable[] = {
    // addr, bus,      i2cAddr, cs,  irq
    {0x01, GW_BUS_I2C, 0x21,    -1,  -1}, // Fonte
    {0x02, GW_BUS_SPI, 0x00,    5,   4 }, // Comunicação (exemplo CS=GPIO5, IRQ=GPIO4)
    {0x03, GW_BUS_I2C, 0x23,    -1,  -1}, // Gerador níveis
    {0x04, GW_BUS_I2C, 0x24,    -1,  -1}, // Relés
    {0x05, GW_BUS_SPI, 0x00,    15,  16}, // UI (exemplo)
};

bool GwDeviceTable::get(uint8_t addr, GwDeviceEntry& out) {
    for (auto& e : kTable) {
        if (e.addr == addr) { out = e; return true; }
    }
    return false;
}