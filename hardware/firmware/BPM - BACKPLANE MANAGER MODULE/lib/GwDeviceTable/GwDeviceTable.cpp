#include "GwDeviceTable.h"
#include "Sggw.defs.h"

// Defaults de bootstrap enquanto a BPM ainda nao persiste configuracao
// dinamica de enderecos. O host continua tratando o binding logico;
// a BPM consome apenas enderecos compactos ja resolvidos.
static const GwDeviceEntry kBootstrapDefaults[] = {
    // addr,        bus,       i2cAddr,      cs, irq
    {GW_ADDR_GSA,  GW_BUS_I2C, I2C_GSA_ADDR, -1, -1},
};

bool GwDeviceTable::get(uint8_t addr, GwDeviceEntry& out) {
    for (auto& e : kBootstrapDefaults) {
        if (e.addr == addr) { out = e; return true; }
    }
    return false;
}
