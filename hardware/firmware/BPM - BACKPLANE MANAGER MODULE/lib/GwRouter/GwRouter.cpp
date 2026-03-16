#include "GwRouter.h"
#include "GwDeviceTable.h"
#include "GwTlv.h"
#include "Sggw.defs.h"

GwErr GwRouter::route(uint8_t cmd,
                      const uint8_t* reqTlv, uint8_t reqTlvLen,
                      uint8_t* respBuf, size_t respMax, size_t& respLen,
                      uint16_t timeoutMs)
{
    respLen = 0;

    const uint8_t addr = GW_CMD_ADDR(cmd);
    if (addr == GW_ADDR_BPM) return GWERR_ADDR_UNMAPPED; // nao roteia BPM local

    GwDeviceEntry e{};
    if (!GwDeviceTable::get(addr, e)) return GWERR_ADDR_UNMAPPED;

    // O host resolve o contrato interno da board; o gateway apenas roteia.
    if (!GwTlv::validatePacket(reqTlv, reqTlvLen))
        return GWERR_BAD_FRAME;

    // escolhe bus
    IGwBus* bus = nullptr;
    if (e.bus == GW_BUS_I2C) bus = &_i2c;
    else if (e.bus == GW_BUS_SPI) bus = &_spi;
    else return GWERR_BUS_DOWN;

    if (!bus->isOk()) {
        // ainda pode tentar, mas aqui sinaliza já
        // (ou você decide tentar mesmo assim)
    }

    size_t rxLen = 0;
    if (!bus->transact(addr, reqTlv, reqTlvLen, respBuf, respMax, rxLen, timeoutMs))
        return GWERR_TIMEOUT;

    if (!GwTlv::validatePacket(respBuf, rxLen))
        return GWERR_BAD_CRC;

    respLen = rxLen;
    return GWERR_OK;
}
