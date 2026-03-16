#include <Arduino.h>
#include "SggwDevice.h"
#include "SggwLink.h"

// Ajuste caso o LED da sua placa seja ativo em LOW
#define LED_ACTIVE_LOW 0

static inline void ledWrite(bool on)
{
#if LED_ACTIVE_LOW
    digitalWrite(LED_BUILTIN, on ? LOW : HIGH);
#else
    digitalWrite(LED_BUILTIN, on ? HIGH : LOW);
#endif
}

static inline uint8_t ledReadState01()
{
#if LED_ACTIVE_LOW
    return (digitalRead(LED_BUILTIN) == LOW) ? 1 : 0;
#else
    return (digitalRead(LED_BUILTIN) == HIGH) ? 1 : 0;
#endif
}

void SggwDevice::onCommand(uint8_t cmd,
                           uint8_t /*flags*/,
                           uint8_t /*seq*/,
                           const uint8_t *data,
                           uint8_t dataLen)
{
    switch (cmd)
    {
    case (uint8_t)SGGW_CMD_PING:
        // ACK já é tratado automaticamente pelo Link
        break;

    case (uint8_t)SGGW_CMD_ECHO:
        _link.sendEvent((uint8_t)SGGW_CMD_ECHO, data, dataLen);
        break;

    case (uint8_t)SGGW_CMD_LED_SET:
    {
        if (dataLen >= 1 && data != nullptr)
        {
            const uint8_t value = data[0];
            ledWrite(value != 0);
        }

        uint8_t realState = ledReadState01();
        _link.sendEvent((uint8_t)SGGW_CMD_LED_SET, &realState, 1);
        break; // <<< FIX (não cair no LOGOUT)
    }

    case (uint8_t)SGGW_CMD_LOGOUT:
    {
        const uint8_t ok = 1;
        _link.sendEvent((uint8_t)SGGW_CMD_LOGOUT, &ok, 1);

        _link.logout(); // volta pro modo texto (banner)
        break;
    }

    default:
        // opcional: poderia mandar ERR unknown cmd se quiser
        break;
    }
}
