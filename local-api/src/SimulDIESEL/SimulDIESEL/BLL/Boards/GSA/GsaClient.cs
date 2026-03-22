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
        private delegate bool GsaResponseParser<T>(SggwFrame frame, out T response, out string error)
            where T : class;

        private sealed class PendingGsaRequest
        {
            public TaskCompletionSource<SggwFrame> ResponseSource { get; set; }
            public Func<SggwFrame, bool> MatchFrame { get; set; }
        }

        private const int ResponseTimeoutMs = 2000;

        private readonly SdhClient _sdh;
        private readonly SdgwSession _sdgwSession;
        private readonly SemaphoreSlim _requestGate = new SemaphoreSlim(1, 1);

        private PendingGsaRequest _pendingRequest;
        private bool _disposed;

        public GsaClient(SdhClient sdh, SdgwSession sdgwSession)
        {
            _sdh = sdh ?? throw new ArgumentNullException(nameof(sdh));
            _sdgwSession = sdgwSession ?? throw new ArgumentNullException(nameof(sdgwSession));

            _sdgwSession.FrameReceived += OnFrameReceived;
            _sdgwSession.EventReceived += OnEventReceived;
        }

        public event Action<GsaChannelFaultEvent> ChannelFaultEventReceived;

        public Task<SdGwLinkEngine.SendOutcome> SetLedAsync(bool on)
        {
            var request = new GsaLedRequest { IsOn = on };
            var command = CreateCommand("GSA.led", "set");
            command.Args["state"] = request.IsOn ? "on" : "off";
            return _sdh.SendAsync(command, SdGwTxPriority.High, "GSA builtin LED");
        }

        public async Task<GsaCommandResult> SetBuiltinLedAsync(bool on)
        {
            GsaOperationResult<GsaLedResponse> result = await ExecuteOperationAsync<GsaLedResponse>(
                CreateLedCommand(on),
                GwProtocol.GsaSetLedType,
                0x01,
                null,
                "LED builtin da GSA",
                GsaParsers.TryReadBuiltinLedResponse).ConfigureAwait(false);

            if (!result.Success)
                return GsaCommandResult.Fail(result.Message, result.SendOutcome, null);

            return GsaCommandResult.Succeeded(
                result.Response.AppliedState,
                result.SendOutcome ?? SdGwLinkEngine.SendOutcome.Acked,
                result.Message);
        }

        public Task<GsaOperationResult<GsaChannelSetpointResponse>> SetChannelSetpointAsync(GsaChannelSetpointRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var command = CreateCommand("GSA.channel.setpoint", "set");
            command.Args["channel"] = request.Channel.ToString();
            command.Args["value"] = request.Value.ToString();

            return ExecuteOperationAsync<GsaChannelSetpointResponse>(
                command,
                GwProtocol.GsaChannelSetpointType,
                0x02,
                request.Channel,
                "setpoint do canal " + request.Channel.ToString(),
                GsaParsers.TryReadChannelSetpointResponse);
        }

        public Task<GsaOperationResult<GsaChannelEnableResponse>> SetChannelEnableAsync(GsaChannelEnableRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var command = CreateCommand("GSA.channel.enable", "set");
            command.Args["channel"] = request.Channel.ToString();
            command.Args["state"] = request.State ? "on" : "off";

            return ExecuteOperationAsync<GsaChannelEnableResponse>(
                command,
                GwProtocol.GsaChannelEnableType,
                0x02,
                request.Channel,
                "enable do canal " + request.Channel.ToString(),
                GsaParsers.TryReadChannelEnableResponse);
        }

        public Task<GsaOperationResult<GsaChannelsEnableResponse>> SetChannelsEnableAsync(GsaChannelsEnableRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var command = CreateCommand("GSA.channels.enable", "set");
            command.Args["state"] = request.State ? "on" : "off";

            return ExecuteOperationAsync<GsaChannelsEnableResponse>(
                command,
                GwProtocol.GsaChannelsEnableType,
                0x02,
                null,
                "enable global dos canais da GSA",
                GsaParsers.TryReadChannelsEnableResponse);
        }

        public Task<GsaOperationResult<GsaChannelStatusResponse>> GetChannelStatusAsync(GsaChannelStatusRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var command = CreateCommand("GSA.channel.status", "get");
            command.Args["channel"] = request.Channel.ToString();

            return ExecuteOperationAsync<GsaChannelStatusResponse>(
                command,
                GwProtocol.GsaChannelStatusType,
                0x06,
                request.Channel,
                "status do canal " + request.Channel.ToString(),
                GsaParsers.TryReadChannelStatusResponse);
        }

        public Task<GsaOperationResult<GsaChannelsStatusResponse>> GetChannelsStatusAsync()
        {
            return ExecuteOperationAsync<GsaChannelsStatusResponse>(
                CreateCommand("GSA.channels.status", "get"),
                GwProtocol.GsaChannelsStatusType,
                0x60,
                null,
                "status global da GSA",
                GsaParsers.TryReadChannelsStatusResponse);
        }

        public Task<GsaOperationResult<GsaChannelFaultResetResponse>> ResetChannelFaultAsync(GsaChannelFaultResetRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var command = CreateCommand("GSA.channel.fault", "reset");
            command.Args["channel"] = request.Channel.ToString();

            return ExecuteOperationAsync<GsaChannelFaultResetResponse>(
                command,
                GwProtocol.GsaChannelFaultResetType,
                0x02,
                request.Channel,
                "reset de fault do canal " + request.Channel.ToString(),
                GsaParsers.TryReadChannelFaultResetResponse);
        }

        public Task<GsaOperationResult<GsaChannelOffsetResponse>> SetChannelOffsetAsync(GsaChannelOffsetSetRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var command = CreateCommand("GSA.channel.offset", "set");
            command.Args["channel"] = request.Channel.ToString();
            command.Args["kind"] = ToKindText(request.Kind);
            command.Args["value"] = request.Value.ToString();

            return ExecuteOperationAsync<GsaChannelOffsetResponse>(
                command,
                GwProtocol.GsaChannelOffsetSetType,
                0x04,
                request.Channel,
                "set de offset do canal " + request.Channel.ToString(),
                ParseOffsetSetResponse);
        }

        public Task<GsaOperationResult<GsaChannelOffsetResponse>> GetChannelOffsetAsync(GsaChannelOffsetGetRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var command = CreateCommand("GSA.channel.offset", "get");
            command.Args["channel"] = request.Channel.ToString();
            command.Args["kind"] = ToKindText(request.Kind);

            return ExecuteOperationAsync<GsaChannelOffsetResponse>(
                command,
                GwProtocol.GsaChannelOffsetGetType,
                0x04,
                request.Channel,
                "get de offset do canal " + request.Channel.ToString(),
                ParseOffsetGetResponse);
        }

        public Task<GsaOperationResult<GsaChannelOffsetSaveResponse>> SaveChannelOffsetAsync(GsaChannelOffsetSaveRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var command = CreateCommand("GSA.channel.offset", "save");
            command.Args["channel"] = request.Channel.ToString();

            return ExecuteOperationAsync<GsaChannelOffsetSaveResponse>(
                command,
                GwProtocol.GsaChannelOffsetSaveType,
                0x01,
                request.Channel,
                "save de offset do canal " + request.Channel.ToString(),
                GsaParsers.TryReadChannelOffsetSaveResponse);
        }

        public Task<GsaOperationResult<GsaChannelOffsetResetResponse>> ResetChannelOffsetAsync(GsaChannelOffsetResetRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var command = CreateCommand("GSA.channel.offset", "reset");
            command.Args["channel"] = request.Channel.ToString();

            return ExecuteOperationAsync<GsaChannelOffsetResetResponse>(
                command,
                GwProtocol.GsaChannelOffsetResetType,
                0x01,
                request.Channel,
                "reset de offset do canal " + request.Channel.ToString(),
                GsaParsers.TryReadChannelOffsetResetResponse);
        }

        public Task<GsaOperationResult<GsaOffsetResetResponse>> ResetOffsetsAsync()
        {
            return ExecuteOperationAsync<GsaOffsetResetResponse>(
                CreateCommand("GSA.offset", "reset"),
                GwProtocol.GsaOffsetResetType,
                0x01,
                null,
                "reset global de offsets da GSA",
                GsaParsers.TryReadOffsetResetResponse);
        }

        private async Task<GsaOperationResult<T>> ExecuteOperationAsync<T>(
            SdhCommand command,
            byte expectedType,
            byte expectedLen,
            int? expectedChannel,
            string operationName,
            GsaResponseParser<T> parser)
            where T : class
        {
            ThrowIfDisposed();

            await _requestGate.WaitAsync().ConfigureAwait(false);
            try
            {
                _pendingRequest = new PendingGsaRequest
                {
                    ResponseSource = new TaskCompletionSource<SggwFrame>(TaskCreationOptions.RunContinuationsAsynchronously),
                    MatchFrame = frame => MatchesExpectedResponse(frame, expectedType, expectedLen, expectedChannel)
                };

                SdGwLinkEngine.SendOutcome outcome = await _sdh.SendAsync(
                    command,
                    SdGwTxPriority.High,
                    operationName).ConfigureAwait(false);

                if (outcome != SdGwLinkEngine.SendOutcome.Acked)
                    return GsaOperationResult<T>.Fail(TranslateOutcome(outcome, operationName), outcome);

                SggwFrame responseFrame = await WaitForResponseAsync(_pendingRequest).ConfigureAwait(false);

                GsaFunctionalErrorResponse functionalError;
                string functionalErrorParseMessage;
                if (GsaParsers.TryReadFunctionalError(responseFrame, out functionalError, out functionalErrorParseMessage))
                    return GsaOperationResult<T>.FunctionalFail(functionalError, outcome);

                T response;
                string error;
                if (!parser(responseFrame, out response, out error))
                    return GsaOperationResult<T>.Fail(error, outcome);

                return GsaOperationResult<T>.Succeeded(
                    response,
                    outcome,
                    "Operação concluída com sucesso: " + operationName + ".");
            }
            catch (OperationCanceledException)
            {
                return GsaOperationResult<T>.Fail("Timeout aguardando a resposta da GSA para " + operationName + ".");
            }
            catch (Exception ex)
            {
                return GsaOperationResult<T>.Fail("Falha ao processar a resposta da GSA para " + operationName + ": " + ex.Message);
            }
            finally
            {
                _pendingRequest = null;
                _requestGate.Release();
            }
        }

        private void OnFrameReceived(SggwFrame frame)
        {
            PendingGsaRequest pending = _pendingRequest;
            if (pending == null || frame == null)
                return;

            if ((frame.Flags & 0x02) != 0)
                return;

            if (frame.Cmd != GwProtocol.MakeCompactCommand(GwProtocol.GsaAddress, GwProtocol.GsaTlvTransactOp))
                return;

            Func<SggwFrame, bool> matcher = pending.MatchFrame;
            if (matcher == null || !matcher(frame))
                return;

            pending.ResponseSource.TrySetResult(frame);
        }

        private void OnEventReceived(SggwFrame frame)
        {
            if (frame == null)
                return;

            if (frame.Cmd != GwProtocol.MakeCompactCommand(GwProtocol.GsaAddress, GwProtocol.GsaTlvTransactOp))
                return;

            GsaChannelFaultEvent faultEvent;
            string error;
            if (!GsaParsers.TryReadChannelFaultEvent(frame, out faultEvent, out error))
                return;

            ChannelFaultEventReceived?.Invoke(faultEvent);
        }

        private static Task<SggwFrame> WaitForResponseAsync(PendingGsaRequest pendingRequest)
        {
            return WaitForResponseCoreAsync(pendingRequest);
        }

        private static async Task<SggwFrame> WaitForResponseCoreAsync(PendingGsaRequest pendingRequest)
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

        private static bool MatchesExpectedResponse(SggwFrame frame, byte expectedType, byte expectedLen, int? expectedChannel)
        {
            if (frame?.Payload == null)
                return false;

            if (frame.Payload.Length >= 2 &&
                frame.Payload[0] == expectedType &&
                frame.Payload[1] == expectedLen)
            {
                if (!expectedChannel.HasValue)
                    return true;

                return frame.Payload.Length >= 3 && frame.Payload[2] == expectedChannel.Value;
            }

            if (frame.Payload.Length >= 5 &&
                frame.Payload[0] == GwProtocol.GsaErrorType &&
                frame.Payload[1] == 0x03 &&
                frame.Payload[2] == expectedType)
            {
                if (!expectedChannel.HasValue)
                    return true;

                return frame.Payload[3] == expectedChannel.Value;
            }

            return false;
        }

        private static bool ParseOffsetSetResponse(SggwFrame frame, out GsaChannelOffsetResponse response, out string error)
        {
            return GsaParsers.TryReadChannelOffsetResponse(frame, GwProtocol.GsaChannelOffsetSetType, out response, out error);
        }

        private static bool ParseOffsetGetResponse(SggwFrame frame, out GsaChannelOffsetResponse response, out string error)
        {
            return GsaParsers.TryReadChannelOffsetResponse(frame, GwProtocol.GsaChannelOffsetGetType, out response, out error);
        }

        private static SdhCommand CreateLedCommand(bool on)
        {
            var command = CreateCommand("GSA.led", "set");
            command.Args["state"] = on ? "on" : "off";
            return command;
        }

        private static SdhCommand CreateCommand(string target, string op)
        {
            return new SdhCommand
            {
                Target = target,
                Op = op
            };
        }

        private static string ToKindText(GsaOffsetKind kind)
        {
            switch (kind)
            {
                case GsaOffsetKind.Vout:
                    return "vout";
                case GsaOffsetKind.Vread:
                    return "vread";
                case GsaOffsetKind.Iread:
                    return "iread";
                default:
                    throw new InvalidOperationException("Kind de offset não suportado: " + kind.ToString() + ".");
            }
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
                    return "O transporte serial está indisponível no momento.";
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
                throw new ObjectDisposedException(nameof(GsaClient));
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _sdgwSession.FrameReceived -= OnFrameReceived;
            _sdgwSession.EventReceived -= OnEventReceived;
            _requestGate.Dispose();
            _disposed = true;
        }
    }
}
