#pragma once
#include <stdint.h>

#define I2C_GSA_ADDR  0x23
#define TLV_MAX_LEN   32   // inclui CRC no final

// Comandos (TLV T)
#define CMD_GET_ERR     0x01
#define CMD_CLR_ERR     0x02

#define CMD_GET_LED     0x11
#define CMD_SET_LED     0x12

#define LED_PIN  LED_BUILTIN   // ou número fixo, ex: 13

#ifndef DEVICE_ID
#define DEVICE_ID 0x01
#endif

// Erros do Link (armazenados e lidos por GET_ERR)
#define LINK_ERR_NONE         0x00
#define LINK_ERR_BAD_LEN      0x01
#define LINK_ERR_BAD_TLV      0x02
#define LINK_ERR_BAD_CRC      0x03