using SimulDIESEL.BLL.SDH;
using SimulDIESEL.DTL;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SimulDIESEL.BLL.Boards
{
    /// <summary>
    /// Cliente especializado da GSA.
    /// Nesta fase, ele encapsula a resposta do LED builtin porque o pipeline SDH
    /// ainda não expõe request/response semântico via SdhResponse.
    /// </summary>
    public sealed class GsaClient : IDisposable
    {
        private const int ResponseTimeoutMs = 2000;

        private readonly SdhClient _sdh;
        private readonly SdGgwClient _sggw;
        private readonly SemaphoreSlim _ledGate = new SemaphoreSlim(1, 1);

        private TaskCompletionSource<SggwFrame> _pendingLedResponse;
        private bool _disposed;

        public GsaClient(SdhClient sdh, SdGgwClient sggw)
        {
            _sdh = sdh ?? throw new ArgumentNullException(nameof(sdh));
            _sggw = sggw ?? throw new ArgumentNullException(nameof(sggw));

            _sggw.FrameReceived += OnFrameReceived;
        }

        public Task<SdGwLinkEngine.SendOutcome> SetLedAsync(bool on)
        {
            var command = new SdhCommand
            {
                Target = "GSA.led",
                Op = "set"
            };

            command.Args["state"] = on ? "on" : "off";

            return _sdh.SendAsync(command);
        }

        public async Task<GsaLedResult> SetBuiltinLedAsync(bool on)
        {
            ThrowIfDisposed();

            await _ledGate.WaitAsync().ConfigureAwait(false);
            try
            {
                _pendingLedResponse = new TaskCompletionSource<SggwFrame>(TaskCreationOptions.RunContinuationsAsynchronously);

                SdGwLinkEngine.SendOutcome outcome = await SetLedAsync(on).ConfigureAwait(false);
                if (outcome != SdGwLinkEngine.SendOutcome.Acked)
                {
                    return GsaLedResult.Fail(TranslateOutcome(outcome), outcome);
                }

                SggwFrame response = await WaitForLedResponseAsync().ConfigureAwait(false);

                bool appliedState;
                string error;
                if (!TryReadLedResponse(response, out appliedState, out error))
                {
                    return GsaLedResult.Fail(error, outcome);
                }

                if (appliedState != on)
                {
                    return GsaLedResult.Fail(
                        "A GSA respondeu um estado diferente do solicitado para o LED builtin.",
                        outcome,
                        appliedState);
                }

                return GsaLedResult.Succeeded(
                    appliedState,
                    outcome,
                    appliedState ? "LED builtin da GSA ligado." : "LED builtin da GSA desligado.");
            }
            catch (OperationCanceledException)
            {
                return GsaLedResult.Fail("Timeout aguardando a resposta da GSA para o comando de LED builtin.");
            }
            catch (Exception ex)
            {
                return GsaLedResult.Fail("Falha ao processar a resposta da GSA para o LED builtin: " + ex.Message);
            }
            finally
            {
                _pendingLedResponse = null;
                _ledGate.Release();
            }
        }

        private void OnFrameReceived(SggwFrame frame)
        {
            TaskCompletionSource<SggwFrame> pending = _pendingLedResponse;
            if (pending == null || frame == null)
            {
                return;
            }

            if (frame.Cmd != GwProtocol.MakeCompactCommand(GwProtocol.GsaAddress, GwProtocol.GsaTlvTransactOp))
            {
                return;
            }

            pending.TrySetResult(frame);
        }

        private static async Task<SggwFrame> WaitForLedResponseAsync(TaskCompletionSource<SggwFrame> pendingResponse)
        {
            Task finished = await Task.WhenAny(
                pendingResponse.Task,
                Task.Delay(ResponseTimeoutMs)).ConfigureAwait(false);

            if (finished != pendingResponse.Task)
            {
                throw new OperationCanceledException();
            }

            return await pendingResponse.Task.ConfigureAwait(false);
        }

        private Task<SggwFrame> WaitForLedResponseAsync()
        {
            return WaitForLedResponseAsync(_pendingLedResponse);
        }

        private static bool TryReadLedResponse(SggwFrame frame, out bool appliedState, out string error)
        {
            appliedState = false;
            error = null;

            if (frame.Payload == null)
            {
                error = "Resposta da GSA sem payload para o comando de LED builtin.";
                return false;
            }

            if (frame.Payload.Length != 3 && frame.Payload.Length != 4)
            {
                error = "Resposta da GSA com tamanho inválido para o comando de LED builtin.";
                return false;
            }

            if (frame.Payload[0] != GwProtocol.GsaSetLedType || frame.Payload[1] != 0x01)
            {
                error = "Resposta da GSA com TLV inesperado para o comando de LED builtin.";
                return false;
            }

            if (frame.Payload.Length == 4)
            {
                byte expectedCrc = SdGwLinkEngine.Crc8Atm(frame.Payload, 0, 3);
                if (frame.Payload[3] != expectedCrc)
                {
                    error = "Resposta da GSA com CRC inválido para o comando de LED builtin.";
                    return false;
                }
            }

            appliedState = frame.Payload[2] != 0;
            return true;
        }

        private static string TranslateOutcome(SdGwLinkEngine.SendOutcome outcome)
        {
            switch (outcome)
            {
                case SdGwLinkEngine.SendOutcome.Nacked:
                    return "A BPM rejeitou o comando para o LED builtin da GSA.";
                case SdGwLinkEngine.SendOutcome.Timeout:
                    return "Timeout aguardando ACK do gateway para o comando de LED builtin da GSA.";
                case SdGwLinkEngine.SendOutcome.TransportDown:
                    return "O transporte serial está indisponível no momento.";
                case SdGwLinkEngine.SendOutcome.Busy:
                    return "O gateway está ocupado processando outro comando.";
                case SdGwLinkEngine.SendOutcome.Enqueued:
                    return "O comando foi enfileirado, mas não houve confirmação do gateway.";
                default:
                    return "Falha ao enviar comando para o LED builtin da GSA.";
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(GsaClient));
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _sggw.FrameReceived -= OnFrameReceived;
            _ledGate.Dispose();
            _disposed = true;
        }
    }

    public sealed class GsaLedResult
    {
        private GsaLedResult(bool success, bool? appliedState, string message, SdGwLinkEngine.SendOutcome? sendOutcome)
        {
            Success = success;
            AppliedState = appliedState;
            Message = message ?? string.Empty;
            SendOutcome = sendOutcome;
        }

        public bool Success { get; }
        public bool? AppliedState { get; }
        public string Message { get; }
        public SdGwLinkEngine.SendOutcome? SendOutcome { get; }

        public static GsaLedResult Succeeded(bool appliedState, SdGwLinkEngine.SendOutcome sendOutcome, string message)
        {
            return new GsaLedResult(true, appliedState, message, sendOutcome);
        }

        public static GsaLedResult Fail(string message, SdGwLinkEngine.SendOutcome? sendOutcome = null, bool? appliedState = null)
        {
            return new GsaLedResult(false, appliedState, message, sendOutcome);
        }
    }
}
