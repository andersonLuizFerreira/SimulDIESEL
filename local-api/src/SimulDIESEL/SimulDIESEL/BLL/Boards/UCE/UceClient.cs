using System;
using System.Threading;
using System.Threading.Tasks;
using SimulDIESEL.DAL.Protocols.SDGW;
using SimulDIESEL.DTL.Boards.UCE;
using SimulDIESEL.DTL.Protocols.SDGW;

namespace SimulDIESEL.BLL.Boards.UCE
{
    public sealed class UceClient : IDisposable
    {
        private delegate bool UceResponseParser<T>(SdgwFrame frame, out T response, out string error)
            where T : class;

        private sealed class PendingUceRequest
        {
            public TaskCompletionSource<SdgwFrame> ResponseSource { get; set; }
            public Func<SdgwFrame, bool> MatchFrame { get; set; }
        }

        private const int ResponseTimeoutMs = 2000;

        private readonly SdhClient _sdh;
        private readonly SdgwSession _sdgwSession;
        private readonly SemaphoreSlim _requestGate = new SemaphoreSlim(1, 1);

        private PendingUceRequest _pendingRequest;
        private bool _disposed;

        public UceClient(SdhClient sdh, SdgwSession sdgwSession)
        {
            _sdh = sdh ?? throw new ArgumentNullException(nameof(sdh));
            _sdgwSession = sdgwSession ?? throw new ArgumentNullException(nameof(sdgwSession));

            _sdgwSession.FrameReceived += OnFrameReceived;
        }

        public async Task<UceCommandResult> SetBuiltinLedAsync(bool on)
        {
            UceOperationResult<UceLedResponse> result = await ExecuteOperationAsync<UceLedResponse>(
                CreateLedCommand(on),
                GwProtocol.UceSetLedType,
                0x01,
                "LED builtin da UCE",
                UceParsers.TryReadBuiltinLedResponse).ConfigureAwait(false);

            if (!result.Success || result.Response == null)
                return UceCommandResult.Fail(result.Message, result.SendOutcome);

            return UceCommandResult.Succeeded(
                result.Response.AcceptedState,
                result.SendOutcome ?? SdGwLinkEngine.SendOutcome.Acked,
                result.Message);
        }

        private async Task<UceOperationResult<T>> ExecuteOperationAsync<T>(
            SdhCommand command,
            byte expectedType,
            byte expectedLen,
            string operationName,
            UceResponseParser<T> parser)
            where T : class
        {
            ThrowIfDisposed();

            await _requestGate.WaitAsync().ConfigureAwait(false);
            try
            {
                _pendingRequest = new PendingUceRequest
                {
                    ResponseSource = new TaskCompletionSource<SdgwFrame>(TaskCreationOptions.RunContinuationsAsynchronously),
                    MatchFrame = frame => MatchesExpectedResponse(frame, expectedType, expectedLen)
                };

                SdGwLinkEngine.SendOutcome outcome = await _sdh.SendAsync(
                    command,
                    SdGwTxPriority.High,
                    operationName).ConfigureAwait(false);

                if (outcome != SdGwLinkEngine.SendOutcome.Acked)
                    return UceOperationResult<T>.Fail(TranslateOutcome(outcome, operationName), outcome);

                SdgwFrame responseFrame = await WaitForResponseAsync(_pendingRequest).ConfigureAwait(false);

                string gatewayErrorMessage;
                string gatewayParseError;
                if (UceParsers.TryReadGatewayError(responseFrame, out gatewayErrorMessage, out gatewayParseError))
                    return UceOperationResult<T>.Fail(gatewayErrorMessage, outcome);

                string functionalErrorMessage;
                string functionalParseError;
                if (UceParsers.TryReadFunctionalError(responseFrame, out functionalErrorMessage, out functionalParseError))
                    return UceOperationResult<T>.Fail(functionalErrorMessage, outcome);

                T response;
                string error;
                if (!parser(responseFrame, out response, out error))
                    return UceOperationResult<T>.Fail(error, outcome);

                return UceOperationResult<T>.Succeeded(
                    response,
                    outcome,
                    "Resposta síncrona recebida da UCE para " + operationName + ".");
            }
            catch (OperationCanceledException)
            {
                return UceOperationResult<T>.Fail("Timeout aguardando a resposta da UCE para " + operationName + ".");
            }
            catch (Exception ex)
            {
                return UceOperationResult<T>.Fail("Falha ao processar a resposta da UCE para " + operationName + ": " + ex.Message);
            }
            finally
            {
                _pendingRequest = null;
                _requestGate.Release();
            }
        }

        private void OnFrameReceived(SdgwFrame frame)
        {
            PendingUceRequest pending = _pendingRequest;
            if (pending == null || frame == null)
                return;

            if ((frame.Flags & 0x02) != 0)
                return;

            if (frame.Cmd != GwProtocol.MakeCompactCommand(GwProtocol.UceAddress, GwProtocol.UceTlvTransactOp))
                return;

            Func<SdgwFrame, bool> matcher = pending.MatchFrame;
            if (matcher == null || !matcher(frame))
                return;

            pending.ResponseSource.TrySetResult(frame);
        }

        private static async Task<SdgwFrame> WaitForResponseAsync(PendingUceRequest pendingRequest)
        {
            if (pendingRequest == null)
                throw new OperationCanceledException();

            Task finished = await Task.WhenAny(
                pendingRequest.ResponseSource.Task,
                Task.Delay(ResponseTimeoutMs)).ConfigureAwait(false);

            if (finished != pendingRequest.ResponseSource.Task)
                throw new OperationCanceledException();

            return await pendingRequest.ResponseSource.Task.ConfigureAwait(false);
        }

        private static bool MatchesExpectedResponse(SdgwFrame frame, byte expectedType, byte expectedLen)
        {
            if (frame?.Payload == null)
                return false;

            if (frame.Payload.Length >= 2 &&
                frame.Payload[0] == expectedType &&
                frame.Payload[1] == expectedLen)
            {
                return true;
            }

            if (frame.Payload.Length >= 5 &&
                frame.Payload[0] == GwProtocol.UceErrorType &&
                frame.Payload[1] == 0x03 &&
                frame.Payload[2] == expectedType)
            {
                return true;
            }

            if (frame.Payload.Length >= 3 &&
                frame.Payload[0] == GwProtocol.GatewayErrorType &&
                frame.Payload[1] >= 0x01)
            {
                return true;
            }

            return false;
        }

        private static SdhCommand CreateLedCommand(bool on)
        {
            var command = new SdhCommand
            {
                Target = "UCE.led",
                Op = "set"
            };

            command.Args["state"] = on ? "on" : "off";
            return command;
        }

        private static string TranslateOutcome(SdGwLinkEngine.SendOutcome outcome, string operationName)
        {
            switch (outcome)
            {
                case SdGwLinkEngine.SendOutcome.Nacked:
                    return "A BPM rejeitou o comando para " + operationName + ".";
                case SdGwLinkEngine.SendOutcome.Timeout:
                    return "Timeout aguardando ACK do gateway para " + operationName + ".";
                case SdGwLinkEngine.SendOutcome.TransportDown:
                    return "O transporte ativo da BPM está indisponível no momento.";
                case SdGwLinkEngine.SendOutcome.Busy:
                    return "O link estava temporariamente ocupado. Tente novamente.";
                case SdGwLinkEngine.SendOutcome.Enqueued:
                    return "O comando foi enfileirado, mas não houve confirmação do gateway.";
                default:
                    return "Falha ao enviar comando para " + operationName + ".";
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(UceClient));
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _sdgwSession.FrameReceived -= OnFrameReceived;
            _requestGate.Dispose();
            _disposed = true;
        }

        private sealed class UceOperationResult<T>
            where T : class
        {
            private UceOperationResult(bool success, T response, string message, SdGwLinkEngine.SendOutcome? sendOutcome)
            {
                Success = success;
                Response = response;
                Message = message ?? string.Empty;
                SendOutcome = sendOutcome;
            }

            public bool Success { get; }
            public T Response { get; }
            public string Message { get; }
            public SdGwLinkEngine.SendOutcome? SendOutcome { get; }

            public static UceOperationResult<T> Succeeded(T response, SdGwLinkEngine.SendOutcome sendOutcome, string message)
            {
                return new UceOperationResult<T>(true, response, message, sendOutcome);
            }

            public static UceOperationResult<T> Fail(string message, SdGwLinkEngine.SendOutcome? sendOutcome = null)
            {
                return new UceOperationResult<T>(false, null, message, sendOutcome);
            }
        }
    }
}
