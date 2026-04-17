#pragma once
#include <stdint.h>

#define TLV_MAX_LEN               32
#define UCE_CAN_MAX_MAILBOXES     8
#define UCE_CAN_MAX_FILTERS       8

#define UCE_PIN_NOT_CONNECTED     0xFF

#define CMD_LED_BUILTIN           0x12
#define CMD_CAN_CONFIG            0x20
#define CMD_CAN_ENABLE            0x21
#define CMD_CAN_STATUS            0x22
#define CMD_CAN_RESET             0x23
#define CMD_FUNCTIONAL_ERROR      0x7F

#define UCE_ERROR_INVALID_STATE          0x03
#define UCE_ERROR_COMMAND_NOT_SUPPORTED  0x07
#define UCE_ERROR_INVALID_PAYLOAD        0x08
#define UCE_ERROR_INVALID_TLV_CRC        0x09

#define LED_PIN                   LED_BUILTIN
