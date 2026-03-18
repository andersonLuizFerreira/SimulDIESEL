using System;
using System.Threading;
using System.Threading.Tasks;
using SimulDIESEL.DAL.Protocols.SDGW;
using SimulDIESEL.DTL.Boards.GSA;
using SimulDIESEL.DTL.Protocols.SDGW;

namespace SimulDIESEL.BLL.Boards.GSA
{
    /// <summary>
    /// Fachada funcional da board GSA.
    /// </summary>
    public sealed class GsaClient : IDisposable
    {
        private sealed class PendingLedRequest
        {
            public TaskCompletionSource<SggwFrame> ResponseSource { get; set; }
            public bool ExpectedState { get; set; }
        }

        private const int ResponseTimeoutMs = 2000;

        private readonly SdhClient _sdh;
        private readonly SdgwSession _sdgwSession;
        private readonly SemaphoreSlim _ledGate = new SemaphoreSlim(1, 1);

        private PendingLedRequest _pendingLedRequest;
        private bool _disposed;

        public GsaClient(SdhClient sdh, SdgwSession sdgwSession)
        {
            _sdh = sdh ?? throw new ArgumentNullException(nameof(sdh));
            _sdgwSession = sdgwSession ?? throw new ArgumentNullException(nameof(sdgwSession));

            _sdgwSession.FrameReceived += OnFrameReceived;
        }

        public Task<SdGwLinkEngine.SendOutcome> SetLedAsync(bool on)
        {
            var request = new GsaLedRequest { IsOn = on };
            var command = new SdhCommand
            {
                Target = "GSA.led",
                Op = "set"
            };

            command.Args["state"] = request.IsOn ? "on" : "off";
            return _sdh.SendAsync(command, SdGwTxPriority.High, "GSA builtin LED");
        }

        public async Task<GsaCommandResult> SetBuiltinLedAsync(bool on)
        {
            ThrowIfDisposed();

            await _ledGate.WaitAsync().ConfigureAwait(false);
            try
            {
                _pendingLedRequest = new PendingLedRequest
                {
                    ExpectedState = on,
                    ResponseSource = new TaskCompletionSource<SggwFrame>(TaskCreationOptions.RunContinuationsAsynchronously)
                };

                SdGwLinkEngine.SendOutcome outcome = await SetLedAsync(on).ConfigureAwait(false);
                if (outcome != SdGwLinkEngine.SendOutcome.Acked)
                    return GsaCommandResult.Fail(TranslateOutcome(outcome), outcome);

                SggwFrame response = await WaitForLedResponseAsync().ConfigureAwait(false);

                GsaLedResponse parsedResponse;
                string error;
                if (!GsaParsers.TryReadBuiltinLedResponse(response, out parsedResponse, out error))
                    return GsaCommandResult.Fail(error, outcome);

                if (parsedResponse.AppliedState != on)
                {
                    return GsaCommandResult.Fail(
                        "A GSA respondeu um estado diferente do solicitado para o LED builtin.",
                        outcome,
                        parsedResponse.AppliedState);
                }

                return GsaCommandResult.Succeeded(
                    parsedResponse.AppliedState,
                    outcome,
                    parsedResponse.AppliedState ? "LED builtin da GSA ligado." : "LED builtin da GSA desligado.");
            }
            catch (OperationCanceledException)
            {
                return GsaCommandResult.Fail("Timeout aguardando a resposta da GSA para o comando de LED builtin.");
            }
            catch (Exception ex)
            {
                return GsaCommandResult.Fail("Falha ao processar a resposta da GSA para o LED builtin: " + ex.Message);
            }
            finally
            {
                _pendingLedRequest = null;
                _ledGate.Release();
            }
        }

        private void OnFrameReceived(SggwFrame frame)
        {
            PendingLedRequest pending = _pendingLedRequest;
            if (pending == null || frame == null)
                return;

            if (frame.Cmd != GwProtocol.MakeCompactCommand(GwProtocol.GsaAddress, GwProtocol.GsaTlvTransactOp))
                return;

            GsaLedResponse parsedResponse;
            string error;
            if (!GsaParsers.TryReadBuiltinLedResponse(frame, out parsedResponse, out error))
                return;

            if (parsedResponse.AppliedState != pending.ExpectedState)
                return;

            pending.ResponseSource.TrySetResult(frame);
        }

        private Task<SggwFrame> WaitForLedResponseAsync()
        {
            return WaitForLedResponseAsync(_pendingLedRequest);
        }

        private static async Task<SggwFrame> WaitForLedResponseAsync(PendingLedRequest pendingRequest)
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
                    return "O link estava temporariamente ocupado. Tente novamente.";
                case SdGwLinkEngine.SendOutcome.Enqueued:
                    return "O comando foi enfileirado, mas não houve confirmação do gateway.";
                default:
                    return "Falha ao enviar comando para o LED builtin da GSA.";
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(GsaClient));
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _sdgwSession.FrameReceived -= OnFrameReceived;
            _ledGate.Dispose();
            _disposed = true;
        }
    }
}
