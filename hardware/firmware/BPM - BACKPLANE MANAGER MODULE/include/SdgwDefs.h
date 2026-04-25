#pragma once
#include <stdint.h>


// ============================================================
// I2C - Baby boards
// ============================================================
#define I2C_GSA_ADDR  0x23   // endereco fisico padrao do GSA no barramento I2C
#define BPM_GSA_I2C_SDA_PIN   21
#define BPM_GSA_I2C_SCL_PIN   22
#define BPM_GSA_IRQ_PIN       19
#define BPM_GLOBAL_RESET_PIN  23
#define BPM_GLOBAL_RESET_ACTIVE_LEVEL LOW
#define BPM_GLOBAL_RESET_INACTIVE_LEVEL HIGH

// ============================================================
// SPI - UCE backplane
// ============================================================
#define BPM_SPI_SCK_PIN       18
#define BPM_SPI_MISO_PIN      26
#define BPM_SPI_MOSI_PIN      25
#define BPM_UCE_SPI_CS_PIN    33
#define BPM_UCE_IRQ_PIN       27
#define BPM_UCE_RESET_PIN     BPM_GLOBAL_RESET_PIN

// ============================================================
// GSA - TLV commands (I2C payload)
// ============================================================
#define GSA_CMD_SETPOINT       0x10
#define GSA_CMD_ENABLE_CH      0x11
#define GSA_CMD_SET_LED        0x12
#define GSA_CMD_ENABLE_GLOBAL  0x14
#define GSA_CMD_FAULT_RESET    0x15
#define GSA_CMD_OFFSET_SET     0x16
#define GSA_CMD_OFFSET_SAVE    0x18
#define GSA_CMD_OFFSET_RESET   0x1A
#define GSA_CMD_STATUS_CH      0x1B
#define GSA_CMD_FAULT_EVENT    0x30
#define GSA_CMD_PHYSICAL_EVENT 0x31
#define GSA_CMD_FUNC_ERROR     0x7F
#define UCE_CMD_SET_LED        0x12
#define UCE_CMD_FUNC_ERROR     0x7F

#define LED_BUILTIN  2
// ============================================================
// UART
// ============================================================
#define SDGW_UART_BAUDRATE              115200
#define SDGW_UART_CONFIG                SERIAL_8N1


// Ajuste caso o LED da sua placa seja ativo em LOW
#define LED_ACTIVE_LOW 0


// ============================================================
// Protocol limits (logical = antes do COBS)
// ============================================================
#define SDGW_MAX_LOGICAL_FRAME          250   // CMD+FLAGS+SEQ+DATA(0..247)+CRC
#define SDGW_MAX_PAYLOAD                247

// COBS worst-case: len + len/254 + 1  (250 => 252) + delimiter => 253
// Mantemos folga para robustez
#define SDGW_MAX_ENCODED_FRAME          384
#define GW_SPI_PACKET_LIMIT             64

// Cache de resposta para retransmissão (ACK/ERR + delimiter)
// 64 é suficiente para ACK/ERR + 1 byte de erro, mas deixamos folga
#define SDGW_MAX_LAST_RESPONSE          64

// Handshake
#define SDGW_HANDSHAKE_BUFFER           64
#define SDGW_HANDSHAKE_TIMEOUT_MS       2000

// Sessao/atividade do link
#define SDGW_LINK_ACTIVITY_TIMEOUT_MS   4000

// Timeout interno do gateway ao rotear para a baby board
#define SDGW_GATEWAY_ROUTE_TIMEOUT_MS   100

// ============================================================
// Flags
// ============================================================
#define SDGW_FLAG_ACK_REQUIRED          0x01
#define SDGW_FLAG_IS_EVENT              0x02

// ============================================================
// Reserved commands
// ============================================================
#define SDGW_CMD_ACK                    0xF1
#define SDGW_CMD_ERR                    0xF2
#define SDGW_TLV_GATEWAY_ERR            0xFE

// ============================================================
// Reserved operational commands
// ============================================================
#define SDGW_CMD_PING                   0x55

// ============================================================
// Error codes (para payload do ERR)
// ============================================================
#define SDGW_ERR_UNKNOWN_CMD            0x01
#define SDGW_ERR_BAD_CRC                0x02
#define SDGW_ERR_BAD_COBS               0x03
#define SDGW_ERR_TOO_LARGE              0x04
#define SDGW_ERR_TOO_SMALL              0x05

// ============================================================
// CRC-8/ATM
// ============================================================
#define SDGW_CRC_POLY                   0x07
#define SDGW_CRC_INIT                   0x00

// ============================================================
// COBS framing
// ============================================================
#define SDGW_COBS_DELIMITER             0x00

// ============================================================
// Handshake strings
// ============================================================
#define SDGW_PC_BANNER                  "\nSIMULDIESELAPI\n"
#define SDGW_DEVICE_BANNER              "SimulDIESEL ver 0.0.1\n"

// ============================================================
// Sequence
// ============================================================
#define SDGW_SEQ_START                  1


// ============================================================
// Gateway compact commands (CMD byte = [ADDR:4][OP:4])
// A BPM sempre ocupa o endereco logico local 0x0.
// Os demais enderecos abaixo sao defaults de bootstrap do firmware
// e podem evoluir depois para configuracao persistida pela propria BPM.
// ============================================================
#define GW_ADDR_BPM         0x0
#define GW_ADDR_GSA         0x1
#define GW_ADDR_UCE         0x2
#define GW_ADDR_BROADCAST   0xF

#define GW_MAKE_CMD(addr, op4)   (uint8_t)((((addr) & 0x0F) << 4) | ((op4) & 0x0F))
#define GW_CMD_ADDR(cmd)         (uint8_t)(((cmd) >> 4) & 0x0F)
#define GW_CMD_OP(cmd)           (uint8_t)((cmd) & 0x0F)

// BPM local ops (0..15)
#define GW_OP_BPM_PING           0x0

// GSA ops (0..15)
#define GW_OP_GSA_TLV_TRANSACT   0x0
// UCE ops (0..15)
#define GW_OP_UCE_TLV_TRANSACT   0x0

// Comandos compactos resolvidos pelo host.
#define SDGW_CMD_BPM_PING        GW_MAKE_CMD(GW_ADDR_BPM, GW_OP_BPM_PING)
#define SDGW_CMD_GSA_TLV         GW_MAKE_CMD(GW_ADDR_GSA, GW_OP_GSA_TLV_TRANSACT)
#define SDGW_CMD_UCE_TLV         GW_MAKE_CMD(GW_ADDR_UCE, GW_OP_UCE_TLV_TRANSACT)
