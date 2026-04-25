using System;
using System.Threading.Tasks;
using SimulDIESEL.BLL.Boards;
using SimulDIESEL.DAL.Protocols.SDGW;
using SimulDIESEL.DTL.Boards.UCE;
using SimulDIESEL.DTL.Protocols.SDGW;

namespace SimulDIESEL.BLL.Boards.UCE
{
    public sealed class UceClient : IDisposable
    {
        private delegate bool UceResponseParser<T>(SdgwFrame frame, out T response, out string error)
            where T : class;

        private readonly BoardTlvDispatcher _dispatcher;
        private bool _disposed;

        public UceClient(SdhClient sdh, SdgwSession sdgwSession)
        {
            _dispatcher = new BoardTlvDispatcher(
                sdh,
                sdgwSession,
                GwProtocol.UceAddress,
                GwProtocol.UceTlvTransactOp);
        }

        public async Task<UceCommandResult> SetBuiltinLedAsync(bool on)
        {
            UceOperationResult<UceLedResponse> result = await ExecuteOperationAsync<UceLedResponse>(
                CreateLedCommand(on),
                GwProtocol.UceSetLedType,
                GwProtocol.UceLedPayloadLength,
                "LED builtin da UCE",
                UceParsers.TryReadBuiltinLedResponse).ConfigureAwait(false);

            if (!result.Success || result.Response == null)
                return UceCommandResult.Fail(result.Message, result.SendOutcome);

            return UceCommandResult.Succeeded(
                result.Response.AcceptedState,
                result.SendOutcome ?? SdGwLinkEngine.SendOutcome.Acked,
                result.Message);
        }

        public Task<UceOperationResult<UceCanConfigResponse>> SetCanConfigAsync(string controller, int bitrateKbps, string mode)
        {
            return ExecuteOperationAsync<UceCanConfigResponse>(
                CreateCanConfigCommand(controller, bitrateKbps, mode),
                GwProtocol.UceCanConfigType,
                GwProtocol.UceCanConfigPayloadLength,
                "configuração CAN da UCE",
                UceParsers.TryReadCanConfigResponse);
        }

        public Task<UceOperationResult<UceCanEnableResponse>> SetCanEnabledAsync(string controller, bool enabled)
        {
            return ExecuteOperationAsync<UceCanEnableResponse>(
                CreateCanEnableCommand(controller, enabled),
                GwProtocol.UceCanEnableType,
                GwProtocol.UceCanEnablePayloadLength,
                "habilitação CAN da UCE",
                UceParsers.TryReadCanEnableResponse);
        }

        public Task<UceOperationResult<UceCanStatusResponse>> GetCanStatusAsync(string controller)
        {
            return ExecuteOperationAsync<UceCanStatusResponse>(
                CreateCanStatusCommand(controller),
                GwProtocol.UceCanStatusType,
                GwProtocol.UceCanStatusResponsePayloadLength,
                "status CAN da UCE",
                UceParsers.TryReadCanStatusResponse);
        }

        public Task<UceOperationResult<UceCanResetResponse>> ResetCanAsync(string controller)
        {
            return ExecuteOperationAsync<UceCanResetResponse>(
                CreateCanResetCommand(controller),
                GwProtocol.UceCanResetType,
                GwProtocol.UceCanResetResponsePayloadLength,
                "reset CAN da UCE",
                UceParsers.TryReadCanResetResponse);
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

            BoardTlvDispatchResult dispatchResult = await _dispatcher
                .TransactAsync(command, frame => MatchesExpectedResponse(frame, expectedType, expectedLen), operationName)
                .ConfigureAwait(false);

            if (!dispatchResult.Success)
                return UceOperationResult<T>.Fail(dispatchResult.Message, dispatchResult.SendOutcome);

            try
            {
                string gatewayErrorMessage;
                string gatewayParseError;
                if (UceParsers.TryReadGatewayError(dispatchResult.Frame, out gatewayErrorMessage, out gatewayParseError))
                    return UceOperationResult<T>.Fail(gatewayErrorMessage, dispatchResult.SendOutcome);

                string functionalErrorMessage;
                string functionalParseError;
                if (UceParsers.TryReadFunctionalError(dispatchResult.Frame, out functionalErrorMessage, out functionalParseError))
                    return UceOperationResult<T>.Fail(functionalErrorMessage, dispatchResult.SendOutcome);

                T response;
                string error;
                if (!parser(dispatchResult.Frame, out response, out error))
                    return UceOperationResult<T>.Fail(error, dispatchResult.SendOutcome);

                return UceOperationResult<T>.Succeeded(
                    response,
                    dispatchResult.SendOutcome ?? SdGwLinkEngine.SendOutcome.Acked,
                    "Resposta síncrona recebida da UCE para " + operationName + ".");
            }
            catch (Exception ex)
            {
                return UceOperationResult<T>.Fail("Falha ao processar a resposta da UCE para " + operationName + ": " + ex.Message);
            }
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

        private static SdhCommand CreateCanConfigCommand(string controller, int bitrateKbps, string mode)
        {
            var command = new SdhCommand
            {
                Target = "UCE.can.config",
                Op = "set"
            };

            command.Args["controller"] = controller;
            command.Args["bitrate"] = bitrateKbps.ToString(System.Globalization.CultureInfo.InvariantCulture);
            command.Args["mode"] = mode;
            return command;
        }

        private static SdhCommand CreateCanEnableCommand(string controller, bool enabled)
        {
            var command = new SdhCommand
            {
                Target = "UCE.can.enable",
                Op = "set"
            };

            command.Args["controller"] = controller;
            command.Args["state"] = enabled ? "on" : "off";
            return command;
        }

        private static SdhCommand CreateCanStatusCommand(string controller)
        {
            var command = new SdhCommand
            {
                Target = "UCE.can.status",
                Op = "get"
            };

            command.Args["controller"] = controller;
            return command;
        }

        private static SdhCommand CreateCanResetCommand(string controller)
        {
            var command = new SdhCommand
            {
                Target = "UCE.can",
                Op = "reset"
            };

            command.Args["controller"] = controller;
            return command;
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

            _dispatcher.Dispose();
            _disposed = true;
        }

    }
}
