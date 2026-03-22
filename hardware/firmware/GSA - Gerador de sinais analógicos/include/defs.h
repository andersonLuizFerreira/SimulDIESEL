#pragma once
#include <stdint.h>

#define I2C_GSA_ADDR  0x23
#define TLV_MAX_LEN   32   // inclui CRC no final

#define CMD_SETPOINT           0x10
#define CMD_ENABLE_CHANNEL     0x11
#define CMD_LED_BUILTIN        0x12
#define CMD_ENABLE_GLOBAL      0x14
#define CMD_FAULT_RESET        0x15
#define CMD_OFFSET_SET         0x16
#define CMD_OFFSET_SAVE        0x18
#define CMD_OFFSET_RESET_ALL   0x1A
#define CMD_STATUS_CHANNEL     0x1B
#define CMD_FAULT_EVENT        0x30
#define CMD_FUNCTIONAL_ERROR   0x7F

#define GSA_EVENT_BUSY         0x01
#define GSA_EVENT_IDLE         0x02

#define GSA_EVENT_STATE_IDLE   0x00
#define GSA_EVENT_STATE_BUSY   0x01

#define GSA_OFFSET_KIND_VOUT   0x01
#define GSA_OFFSET_KIND_VREAD  0x02
#define GSA_OFFSET_KIND_IREAD  0x03

#define GSA_ERROR_INVALID_CHANNEL        0x01
#define GSA_ERROR_INVALID_VALUE          0x02
#define GSA_ERROR_INVALID_STATE          0x03
#define GSA_ERROR_INVALID_KIND           0x04
#define GSA_ERROR_FAULT_LATCHED         0x05
#define GSA_ERROR_EEPROM_WRITE_FAILED   0x06
#define GSA_ERROR_COMMAND_NOT_SUPPORTED 0x07
#define GSA_ERROR_INVALID_PAYLOAD       0x08
#define GSA_ERROR_INVALID_TLV_CRC       0x09
#define GSA_ERROR_PHYSICAL_FAULT        0x0A
#define GSA_ERROR_OPERATION_NOT_ALLOWED 0x0B

#define LED_PIN  LED_BUILTIN

#define LINK_ERR_NONE         0x00
#define LINK_ERR_BAD_LEN      0x01
#define LINK_ERR_BAD_TLV      0x02
#define LINK_ERR_BAD_CRC      0x03

struct GsaChannelOffsets {
  int16_t vout;
  int16_t vread;
  int16_t iread;
};
