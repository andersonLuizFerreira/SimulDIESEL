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
#define CMD_PHYSICAL_EVENT     0x31
#define CMD_FUNCTIONAL_ERROR   0x7F

#define GSA_PHYSICAL_STATUS_OK            0x01
#define GSA_PHYSICAL_STATUS_TCA_NO_ACK    0x02
#define GSA_PHYSICAL_STATUS_MCP_NO_ACK    0x03

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

struct GsaChannelOffsets {
  int16_t vout;
  int16_t vread;
  int16_t iread;
};
