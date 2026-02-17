# SerialTransport (DAL)

## Propósito
Implementar transporte serial de baixo nível (leitura/gravação de bytes) sobre `System.IO.Ports.SerialPort`. Fornece eventos para consumidores reagirem a bytes recebidos, mudanças de conexão e erros.

## Escopo
Classe: `SimulDIESEL.DAL.SerialTransport`

## Responsabilidades
- Abrir/fechar uma porta serial com parâmetros (baudRate, parity, dataBits, stopBits, handshake, DTR/RTS).
- Enviar arrays de bytes para a porta através de `Write(byte[])`.
- Receber bytes do `SerialPort` e propagar via evento `BytesReceived`.
- Reportar mudanças lógicas de conexão via `ConnectionChanged`.
- Reportar erros via `Error` (string[] contendo mensagem e fonte).
- Gerenciar estado consistente após falhas (fechar porta, limpar handlers).

## API pública (resumida)
- `bool Connect(string portName, int baudRate, ...)`
- `void Disconnect()`
- `bool Write(byte[] data)`
- `bool IsOpen { get; }`
- `string PortName { get; }`
- `int BaudRate { get; }`
- `event Action<byte[]> BytesReceived`
- `event Action<bool> ConnectionChanged`
- `event Action<string[]> Error`
- `static string[] ListPorts()`

## Modelo de conexão
- `Connect` cria e configura `SerialPort`, adiciona handlers, chama `_port.Open()`. Em caso de exceção, garante fechar e limpar, dispara `Error` e retorna `false`. Em caso de sucesso, define `_connected = true` e dispara `ConnectionChanged(true)` fora do lock.
- `Disconnect` fecha a porta com `SafeClose_NoThrow`, atualiza `_connected = false` e, se aplicável, dispara `ConnectionChanged(false)` uma vez.
- Mantém um flag lógico `_connected` como fonte de verdade para detecção de cabo removido (não confiar apenas em `SerialPort.IsOpen`).

## Detecção de falha física
- Em falhas de escrita (`_port.Write` lança) ou erros serial detectados por `ErrorReceived`, a implementação chama `HandleTransportFault`:
  - Fecha a porta de forma segura.
  - Atualiza `_connected = false`.
  - Dispara `Error` com mensagem e `ConnectionChanged(false)` fora do lock.
- `OnDataReceived` também captura exceções e trata como falha de transporte.

## Thread safety / concorrência
- Acesso ao `_port`, `_connected` e operações sensíveis é protegido pelo lock `_sync`.
- Eventos são disparados fora do lock para evitar reentrância e deadlocks.

## Tratamento de erros
- `RaiseError` envia um array com mensagem e identificação (`"DAL.SerialTransport"`).
- `SafeClose_NoThrow` tenta encerrar/dispôr o `SerialPort` de forma resiliente (captura exceções).
- Em operações inválidas (ex.: `Write` com porta fechada), a função sinaliza erro via evento e retorna `false` (não dispara UI diretamente).

## Eventos e contratos
- `BytesReceived(byte[] buffer)` — entrega dos bytes lidos do `SerialPort`.
- `ConnectionChanged(bool connected)` — conectado/desconectado.
- `Error(string[] msg)` — mensagens de erro.

## Exemplo de uso (pseudocódigo)
ASCII:
PC Application
   ↓
SerialTransport
   ↓
SerialPort (IO)

- Conectar: `transport.Connect("COM3", 115200)`
- Enviar: `transport.Write(new byte[] { ... })`
- Receber: subscrever `transport.BytesReceived += buffer => ...`