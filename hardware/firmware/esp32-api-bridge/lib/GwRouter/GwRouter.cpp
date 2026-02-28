#include "GwRouter.h"
#include "GwDeviceTable.h"
#include "GwTlv.h"

GwErr GwRouter::route(uint8_t cmd,
                      const uint8_t* reqTlv, uint8_t reqTlvLen,
                      uint8_t* respBuf, size_t respMax, size_t& respLen,
                      uint16_t timeoutMs)
{
    respLen = 0;

    const uint8_t addr = (uint8_t)(cmd >> 4);
    if (addr == 0x00) return GWERR_ADDR_UNMAPPED; // não roteia gateway-local

    GwDeviceEntry e{};
    if (!GwDeviceTable::get(addr, e)) return GWERR_ADDR_UNMAPPED;

    // monta frame interno TLV
    uint8_t tx[300];
    size_t txLen = 0;
    if (!GwTlv::buildFrame(cmd, reqTlv, reqTlvLen, tx, sizeof(tx), txLen))
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
    if (!bus->transact(addr, tx, txLen, respBuf, respMax, rxLen, timeoutMs))
        return GWERR_TIMEOUT;

    // valida CRC do retorno
    uint8_t rcCmd = 0;
    const uint8_t* rcTlv = nullptr;
    uint8_t rcTlvLen = 0;
    if (!GwTlv::validateFrame(respBuf, rxLen, rcCmd, rcTlv, rcTlvLen))
        return GWERR_BAD_CRC;

    // opcional: garantir que cmd bate
    if (rcCmd != cmd) return GWERR_BAD_FRAME;

    respLen = rxLen;
    return GWERR_OK;
}