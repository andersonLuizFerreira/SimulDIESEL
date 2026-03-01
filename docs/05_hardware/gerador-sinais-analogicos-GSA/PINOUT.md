# GSA — Pinout

## Pinos identificados no firmware

| Nome lógico | Definição | Função | Observações |
|---|---:|---|---|
| `LED_PIN` | `LED_BUILTIN` | Saída para LED de status | Pino físico depende da placa (**não identificado no resumo**) |

## Barramento

### I2C
- `Wire.h`
- Endereço slave: `0x23` (`I2C_GSA_ADDR`)

> Pinos físicos SDA/SCL variam conforme a placa alvo. **Não identificado no resumo**.