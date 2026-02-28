#pragma once
#include <stdint.h>
#include <stddef.h>
#include "Sggw.defs.h"

class SggwLink; // forward

class SggwDevice {
public:
    explicit SggwDevice(SggwLink& link) : _link(link) {}

    void onCommand(uint8_t cmd, uint8_t flags, uint8_t seq, const uint8_t* data, uint8_t dataLen);

private:
    SggwLink& _link;
};
