#pragma once
#include <stdint.h>


// ============================================================
// I2C - Baby boards
// ============================================================
#define I2C_GSA_ADDR  0x23   // endereço do GSA (Gerador de Sinais Analógicos)




// ============================================================
// GSA - TLV commands (I2C payload)
// ============================================================
#define GSA_CMD_GET_ERR   0x01
#define GSA_CMD_CLR_ERR   0x02
#define GSA_CMD_GET_LED   0x11
#define GSA_CMD_SET_LED   0x12



#define LED_BUILTIN  2
// ============================================================
// UART
// ============================================================
#define SGGW_UART_BAUDRATE              115200
#define SGGW_UART_CONFIG                SERIAL_8N1


// Ajuste caso o LED da sua placa seja ativo em LOW
#define LED_ACTIVE_LOW 0


// ============================================================
// Protocol limits (logical = antes do COBS)
// ============================================================
#define SGGW_MAX_LOGICAL_FRAME          250   // CMD+FLAGS+SEQ+DATA(0..247)+CRC
#define SGGW_MAX_PAYLOAD                247

// COBS worst-case: len + len/254 + 1  (250 => 252) + delimiter => 253
// Mantemos folga para robustez
#define SGGW_MAX_ENCODED_FRAME          384

// Cache de resposta para retransmissão (ACK/ERR + delimiter)
// 64 é suficiente para ACK/ERR + 1 byte de erro, mas deixamos folga
#define SGGW_MAX_LAST_RESPONSE          64

// Handshake
#define SGGW_HANDSHAKE_BUFFER           64

// ============================================================
// Flags
// ============================================================
#define SGGW_FLAG_ACK_REQUIRED          0x01
#define SGGW_FLAG_IS_EVENT              0x02

// ============================================================
// Reserved commands
// ============================================================
#define SGGW_CMD_ACK                    0xF1
#define SGGW_CMD_ERR                    0xF2

// ============================================================
// Implemented commands
// ============================================================
#define SGGW_CMD_PING                   0x55
#define SGGW_CMD_ECHO                   0x02
#define SGGW_CMD_LED_SET                0x03
#define SGGW_CMD_LOGOUT                 0x04

// ============================================================
// Error codes (para payload do ERR)
// ============================================================
#define SGGW_ERR_UNKNOWN_CMD            0x01
#define SGGW_ERR_BAD_CRC                0x02
#define SGGW_ERR_BAD_COBS               0x03
#define SGGW_ERR_TOO_LARGE              0x04
#define SGGW_ERR_TOO_SMALL              0x05

// ============================================================
// CRC-8/ATM
// ============================================================
#define SGGW_CRC_POLY                   0x07
#define SGGW_CRC_INIT                   0x00

// ============================================================
// COBS framing
// ============================================================
#define SGGW_COBS_DELIMITER             0x00

// ============================================================
// Handshake strings
// ============================================================
#define SGGW_PC_BANNER                  "\nSIMULDIESELAPI\n"
#define SGGW_DEVICE_BANNER              "SimulDIESEL ver 0.0.1\n"

// ============================================================
// Sequence
// ============================================================
#define SGGW_SEQ_START                  1


// ============================================================
// Addressing (CMD byte = [ADDR:4][OP:4])
// ============================================================
#define GW_ADDR_GATEWAY   0x0
#define GW_ADDR_GSA       0x1
#define GW_MAKE_CMD(addr, op4)   (uint8_t)((((addr)&0x0F)<<4) | ((op4)&0x0F))

// GSA ops (0..15)
#define GSA_OP_TLV_TRANSACT  0x0

// SGGW CMD byte para “enviar TLV ao GSA”
#define SGGW_CMD_GSA_TLV     GW_MAKE_CMD(GW_ADDR_GSA, GSA_OP_TLV_TRANSACT)