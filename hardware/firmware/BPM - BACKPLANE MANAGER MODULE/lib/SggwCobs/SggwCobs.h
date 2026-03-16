#pragma once
#include <stdint.h>
#include <stddef.h>
#include "Sggw.defs.h"

class SggwCobs {
public:
    // Retorna tamanho do payload codificado (sem incluir delimitador)
    static bool encode(const uint8_t* in, size_t len, uint8_t* out, size_t outMax, size_t& outLen);

    // Retorna tamanho do payload decodificado
    static bool decode(const uint8_t* in, size_t len, uint8_t* out, size_t outMax, size_t& outLen);
};
