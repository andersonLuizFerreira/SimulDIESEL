#pragma once
#include <stdint.h>

#define TLV_MAX_LEN               32
#define SPI_V1_MAX_FRAME_LEN      256              // [COMMISSIONING_SPI_V1]
#define SPI_V1_MAX_ENCODED_LEN    260              // [COMMISSIONING_SPI_V1]
#define SPI_V1_MAX_MSG_VALUE_LEN  (TLV_MAX_LEN - 4) // [COMMISSIONING_SPI_V1]
#define UCE_CAN_MAX_MAILBOXES     8
#define UCE_CAN_MAX_FILTERS       8

#define UCE_PIN_NOT_CONNECTED     0xFF

// ============================================================
// SPI - BPM backplane
// ============================================================
#define UCE_SPI_CS_PIN            10
#define UCE_SPI_IRQ_PIN           2
#define UCE_SPI_IRQ_ACTIVE_LEVEL  LOW
#define UCE_SPI_IRQ_IDLE_LEVEL    HIGH

#define CMD_LED_BUILTIN           0x12
#define CMD_CAN_CONFIG            0x20
#define CMD_CAN_ENABLE            0x21
#define CMD_CAN_STATUS            0x22
#define CMD_CAN_RESET             0x23
#define CMD_TRANSPORT_DIAG        0x7E
#define CMD_FUNCTIONAL_ERROR      0x7F

#define UCE_ERROR_INVALID_STATE          0x03
#define UCE_ERROR_COMMAND_NOT_SUPPORTED  0x07
#define UCE_ERROR_INVALID_PAYLOAD        0x08
#define UCE_ERROR_INVALID_TLV_CRC        0x09

#define UCE_TRANSPORT_ERR_NONE          0x00
#define UCE_TRANSPORT_ERR_PRELOAD_FAIL  0x01
#define UCE_TRANSPORT_ERR_TX_UNDERRUN   0x02
#define UCE_TRANSPORT_ERR_RX_OVERFLOW   0x03
#define UCE_TRANSPORT_ERR_CLOCK_FAST    0x04
#define UCE_TRANSPORT_ERR_SESSION_ABORT 0x05

#define LED_PIN                   LED_BUILTIN
