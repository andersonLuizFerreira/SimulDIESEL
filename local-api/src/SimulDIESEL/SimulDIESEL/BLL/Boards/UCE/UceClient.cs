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

        private readonly SdgwSession _sdgwSession;
        private readonly BoardTlvDispatcher _dispatcher;
        private bool _disposed;

        public UceClient(SdhClient sdh, SdgwSession sdgwSession)
        {
            _sdgwSession = sdgwSession ?? throw new ArgumentNullException(nameof(sdgwSession));
            _dispatcher = new BoardTlvDispatcher(
                sdh,
                sdgwSession,
                GwProtocol.UceAddress,
                GwProtocol.UceTlvTransactOp);

            _sdgwSession.EventReceived += OnEventReceived;
        }

        public event Action<UceLedEvent> LedEventReceived;
        public event Action<UceCanRxEvent> CanRxEventReceived;
        public event Action<byte, byte[]> CanCrudEventReceived;
        public event Action<UceDispatcherOverflowDiagnostic> DispatcherOverflowDiagnosticReceived;

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
                CreateCanConfigCommand(controller, bitrateKbps, mode, null),
                GwProtocol.UceCanConfigType,
                GwProtocol.UceCanConfigPayloadLength,
                "configuração CAN da UCE",
                UceParsers.TryReadCanConfigResponse);
        }

        public Task<UceOperationResult<UceCanConfigResponse>> SetCanConfigAsync(string controller, int bitrateKbps, string mode, UceCanRxMode rxMode)
        {
            return ExecuteOperationAsync<UceCanConfigResponse>(
                CreateCanConfigCommand(controller, bitrateKbps, mode, rxMode),
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

        public Task<UceOperationResult<UceCanRxPollResponse>> PollCanRxAsync(string controller)
        {
            return ExecuteOperationAsync<UceCanRxPollResponse>(
                CreateCanRxPollCommand(controller),
                GwProtocol.UceCanRxPollType,
                IsExpectedCanRxPollResponse,
                "poll CAN_RX da UCE",
                UceParsers.TryReadCanRxPollResponse);
        }

        public Task<UceOperationResult<UceCanReadAllResponse>> RequestCanReadAllAsync(string controller)
        {
            return ExecuteOperationAsync<UceCanReadAllResponse>(
                CreateCanReadAllCommand(controller),
                GwProtocol.UceCanReadAllType,
                GwProtocol.UceCanReadAllPayloadLength,
                "snapshot CAN_READ_ALL da UCE",
                UceParsers.TryReadCanReadAllResponse);
        }

        public Task<UceOperationResult<UceCanDriverLogPollResponse>> PollCanDriverLogAsync(string controller)
        {
            return ExecuteOperationAsync<UceCanDriverLogPollResponse>(
                CreateCanDriverLogPollCommand(controller),
                GwProtocol.UceCanDriverLogPollType,
                IsExpectedCanDriverLogPollResponse,
                "poll de diagnóstico do driver CAN da UCE",
                UceParsers.TryReadCanDriverLogPollResponse);
        }

        [Obsolete("Use SdctpApiService.SendDirectAsync / CAN_TX_DIRECT 0x50, or SDCTP TX table methods.")]
        public Task<UceOperationResult<UceCanTxResponse>> SendCanAsync(string controller, bool extended, uint id, byte dlc, byte[] data, ushort periodMs)
        {
            return ExecuteOperationAsync<UceCanTxResponse>(
                CreateCanTxCommand(controller, extended, id, dlc, data, periodMs),
                GwProtocol.UceCanTxType,
                GwProtocol.UceCanTxResponsePayloadLength,
                "envio CAN_TX da UCE",
                UceParsers.TryReadCanTxResponse);
        }

        public Task<UceOperationResult<UceCanTxResponse>> SendCanDirectAsync(string controller, bool extended, bool rtr, uint id, byte dlc, byte[] data)
        {
            return ExecuteOperationAsync<UceCanTxResponse>(
                CreateCanTxDirectCommand(controller, extended, rtr, id, dlc, data),
                GwProtocol.UceCanTxDirectType,
                GwProtocol.UceCanTxResponsePayloadLength,
                "CAN_TX_DIRECT da UCE",
                UceParsers.TryReadCanTxDirectResponse);
        }

        public Task<UceOperationResult<UceCanTxResponse>> CreateCanTxRowAsync(string controller, byte index, bool extended, bool rtr, uint id, byte dlc, byte[] data, ushort periodMs, bool enabled)
        {
            return ExecuteOperationAsync<UceCanTxResponse>(
                CreateCanTxRowCommand(controller, index, extended, rtr, id, dlc, data, periodMs, enabled),
                GwProtocol.UceCanTxCreateType,
                GwProtocol.UceCanTxResponsePayloadLength,
                "CAN_TX_CREATE da UCE",
                UceParsers.TryReadCanTxCreateResponse);
        }

        public Task<UceOperationResult<UceCanTxResponse>> EditCanTxRowAsync(string controller, byte index, byte mask, byte flags, uint id, byte dlc, byte dataMask, byte[] data, ushort periodMs, bool enabled)
        {
            return ExecuteOperationAsync<UceCanTxResponse>(
                CreateCanTxEditCommand(controller, index, mask, flags, id, dlc, dataMask, data, periodMs, enabled),
                GwProtocol.UceCanTxEditType,
                GwProtocol.UceCanTxResponsePayloadLength,
                "CAN_TX_EDIT da UCE",
                UceParsers.TryReadCanTxEditResponse);
        }

        public Task<UceOperationResult<UceCanTxResponse>> DeleteCanTxRowAsync(string controller, byte index, byte reason)
        {
            return ExecuteOperationAsync<UceCanTxResponse>(
                CreateCanTxDeleteCommand(controller, index, reason),
                GwProtocol.UceCanTxDeleteType,
                GwProtocol.UceCanTxResponsePayloadLength,
                "CAN_TX_DELETE da UCE",
                UceParsers.TryReadCanTxDeleteResponse);
        }

        public Task<UceOperationResult<UceCanTxStopResponse>> StopCanTxAsync(string controller)
        {
            return ExecuteOperationAsync<UceCanTxStopResponse>(
                CreateCanTxStopCommand(controller),
                GwProtocol.UceCanTxStopType,
                GwProtocol.UceCanTxStopResponsePayloadLength,
                "parada CAN_TX periódico da UCE",
                UceParsers.TryReadCanTxStopResponse);
        }

        private async Task<UceOperationResult<T>> ExecuteOperationAsync<T>(
            SdhCommand command,
            byte expectedType,
            byte expectedLen,
            string operationName,
                UceResponseParser<T> parser)
            where T : class
        {
            return await ExecuteOperationAsync(
                command,
                expectedType,
                frame => MatchesExpectedResponse(frame, expectedType, expectedLen),
                operationName,
                parser).ConfigureAwait(false);
        }

        private async Task<UceOperationResult<T>> ExecuteOperationAsync<T>(
            SdhCommand command,
            byte expectedType,
            Func<SdgwFrame, bool> matchFrame,
            string operationName,
            UceResponseParser<T> parser)
            where T : class
        {
            ThrowIfDisposed();

            BoardTlvDispatchResult dispatchResult = await _dispatcher
                .TransactAsync(command, matchFrame, operationName)
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

        private void OnEventReceived(SdgwFrame frame)
        {
            if (frame == null)
                return;

            if (frame.Cmd != GwProtocol.MakeCompactCommand(GwProtocol.UceAddress, GwProtocol.UceTlvTransactOp))
                return;

            UceLedEvent ledEvent;
            string ledError;
            if (UceParsers.TryReadLedEvent(frame, out ledEvent, out ledError))
            {
                LedEventReceived?.Invoke(ledEvent);
                return;
            }

            UceCanRxEvent canRxEvent;
            string canRxError;
            if (UceParsers.TryReadCanRxEvent(frame, out canRxEvent, out canRxError))
            {
                CanRxEventReceived?.Invoke(canRxEvent);
                return;
            }

            byte eventType;
            byte[] payload;
            string canCrudError;
            if (UceParsers.TryReadCanCrudEvent(frame, out eventType, out payload, out canCrudError))
            {
                CanCrudEventReceived?.Invoke(eventType, payload);
                return;
            }

            UceDispatcherOverflowDiagnostic dispatcherOverflowDiagnostic;
            string transportDiagError;
            if (UceParsers.TryReadTransportDiagnosticEvent(frame, out dispatcherOverflowDiagnostic, out transportDiagError))
            {
                UceGatewayDiagnosticLog.AppendDispatcherFifoOverflow(dispatcherOverflowDiagnostic);
                DispatcherOverflowDiagnosticReceived?.Invoke(dispatcherOverflowDiagnostic);
            }
        }

        private static bool IsExpectedCanRxPollResponse(SdgwFrame frame)
        {
            if (frame?.Payload == null)
                return false;

            if (frame.Payload.Length >= 4 &&
                frame.Payload[0] == GwProtocol.UceCanRxPollType)
            {
                byte len = frame.Payload[1];
                if (len >= 2 &&
                    (len - 2) % GwProtocol.UceCanRxFrameLength == 0 &&
                    (len - 2) / GwProtocol.UceCanRxFrameLength <= GwProtocol.UceCanRxMaxFramesPerResponse)
                {
                    return true;
                }
            }

            if (frame.Payload.Length >= 5 &&
                frame.Payload[0] == GwProtocol.UceErrorType &&
                frame.Payload[1] == 0x03 &&
                frame.Payload[2] == GwProtocol.UceCanRxPollType)
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

        private static bool IsExpectedCanDriverLogPollResponse(SdgwFrame frame)
        {
            if (frame?.Payload == null)
                return false;

            if (frame.Payload.Length >= 4 &&
                frame.Payload[0] == GwProtocol.UceCanDriverLogPollType)
            {
                byte len = frame.Payload[1];
                if (len >= 2 &&
                    (len - 2) % GwProtocol.UceCanDriverLogEntryLength == 0 &&
                    (len - 2) / GwProtocol.UceCanDriverLogEntryLength <= GwProtocol.UceCanDriverLogMaxEntriesPerResponse)
                {
                    return true;
                }
            }

            if (frame.Payload.Length >= 5 &&
                frame.Payload[0] == GwProtocol.UceErrorType &&
                frame.Payload[1] == 0x03 &&
                frame.Payload[2] == GwProtocol.UceCanDriverLogPollType)
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

        private static SdhCommand CreateCanConfigCommand(string controller, int bitrateKbps, string mode, UceCanRxMode? rxMode)
        {
            var command = new SdhCommand
            {
                Target = "UCE.can.config",
                Op = "set"
            };

            command.Args["controller"] = controller;
            command.Args["bitrate"] = bitrateKbps.ToString(System.Globalization.CultureInfo.InvariantCulture);
            command.Args["mode"] = mode;
            if (rxMode.HasValue)
                command.Args["rxMode"] = rxMode.Value == UceCanRxMode.DirectOnly ? "directOnly" : "auto";
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

        private static SdhCommand CreateCanRxPollCommand(string controller)
        {
            var command = new SdhCommand
            {
                Target = "UCE.can.rx",
                Op = "poll"
            };

            command.Args["controller"] = controller;
            return command;
        }

        private static SdhCommand CreateCanReadAllCommand(string controller)
        {
            var command = new SdhCommand
            {
                Target = "UCE.can.rx",
                Op = "readAll"
            };

            command.Args["controller"] = controller;
            return command;
        }

        private static SdhCommand CreateCanDriverLogPollCommand(string controller)
        {
            var command = new SdhCommand
            {
                Target = "UCE.can.driverLog",
                Op = "poll"
            };

            command.Args["controller"] = controller;
            return command;
        }

        private static SdhCommand CreateCanTxCommand(string controller, bool extended, uint id, byte dlc, byte[] data, ushort periodMs)
        {
            if (data == null || data.Length < 8)
                throw new ArgumentException("Payload CAN_TX deve conter 8 bytes.", nameof(data));

            var command = new SdhCommand
            {
                Target = "UCE.can.tx",
                Op = "send"
            };

            command.Args["controller"] = controller;
            command.Args["extended"] = extended ? "1" : "0";
            command.Args["id"] = id.ToString(System.Globalization.CultureInfo.InvariantCulture);
            command.Args["dlc"] = dlc.ToString(System.Globalization.CultureInfo.InvariantCulture);
            command.Args["period"] = periodMs.ToString(System.Globalization.CultureInfo.InvariantCulture);
            for (int i = 0; i < 8; ++i)
                command.Args["d" + i.ToString(System.Globalization.CultureInfo.InvariantCulture)] = data[i].ToString(System.Globalization.CultureInfo.InvariantCulture);

            return command;
        }

        private static SdhCommand CreateCanTxDirectCommand(string controller, bool extended, bool rtr, uint id, byte dlc, byte[] data)
        {
            var command = CreateCanTxFrameCommand(controller, "direct", extended, rtr, id, dlc, data);
            return command;
        }

        private static SdhCommand CreateCanTxRowCommand(string controller, byte index, bool extended, bool rtr, uint id, byte dlc, byte[] data, ushort periodMs, bool enabled)
        {
            var command = CreateCanTxFrameCommand(controller, "create", extended, rtr, id, dlc, data);
            command.Args["index"] = index.ToString(System.Globalization.CultureInfo.InvariantCulture);
            command.Args["period"] = periodMs.ToString(System.Globalization.CultureInfo.InvariantCulture);
            command.Args["enabled"] = enabled ? "1" : "0";
            return command;
        }

        private static SdhCommand CreateCanTxEditCommand(string controller, byte index, byte mask, byte flags, uint id, byte dlc, byte dataMask, byte[] data, ushort periodMs, bool enabled)
        {
            if (data == null || data.Length < 8)
                throw new ArgumentException("Payload CAN_TX_EDIT deve conter 8 bytes.", nameof(data));

            var command = new SdhCommand
            {
                Target = "UCE.can.tx",
                Op = "edit"
            };

            command.Args["controller"] = controller;
            command.Args["index"] = index.ToString(System.Globalization.CultureInfo.InvariantCulture);
            command.Args["mask"] = mask.ToString(System.Globalization.CultureInfo.InvariantCulture);
            if ((mask & GwProtocol.UceCanTxEditMaskFlags) != 0)
                command.Args["flags"] = flags.ToString(System.Globalization.CultureInfo.InvariantCulture);
            if ((mask & GwProtocol.UceCanTxEditMaskCanId) != 0)
                command.Args["id"] = id.ToString(System.Globalization.CultureInfo.InvariantCulture);
            if ((mask & GwProtocol.UceCanTxEditMaskDlc) != 0)
                command.Args["dlc"] = dlc.ToString(System.Globalization.CultureInfo.InvariantCulture);
            if ((mask & GwProtocol.UceCanTxEditMaskData) != 0)
            {
                command.Args["dataMask"] = dataMask.ToString(System.Globalization.CultureInfo.InvariantCulture);
                for (int i = 0; i < 8; ++i)
                {
                    if ((dataMask & (1 << i)) != 0)
                        command.Args["d" + i.ToString(System.Globalization.CultureInfo.InvariantCulture)] = data[i].ToString(System.Globalization.CultureInfo.InvariantCulture);
                }
            }
            if ((mask & GwProtocol.UceCanTxEditMaskPeriodMs) != 0)
                command.Args["period"] = periodMs.ToString(System.Globalization.CultureInfo.InvariantCulture);
            if ((mask & GwProtocol.UceCanTxEditMaskEnabled) != 0)
                command.Args["enabled"] = enabled ? "1" : "0";

            return command;
        }

        private static SdhCommand CreateCanTxDeleteCommand(string controller, byte index, byte reason)
        {
            var command = new SdhCommand
            {
                Target = "UCE.can.tx",
                Op = "delete"
            };

            command.Args["controller"] = controller;
            command.Args["index"] = index.ToString(System.Globalization.CultureInfo.InvariantCulture);
            command.Args["reason"] = reason.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return command;
        }

        private static SdhCommand CreateCanTxFrameCommand(string controller, string op, bool extended, bool rtr, uint id, byte dlc, byte[] data)
        {
            if (data == null || data.Length < 8)
                throw new ArgumentException("Payload CAN_TX deve conter 8 bytes.", nameof(data));

            var command = new SdhCommand
            {
                Target = "UCE.can.tx",
                Op = op
            };

            command.Args["controller"] = controller;
            command.Args["extended"] = extended ? "1" : "0";
            command.Args["rtr"] = rtr ? "1" : "0";
            command.Args["id"] = id.ToString(System.Globalization.CultureInfo.InvariantCulture);
            command.Args["dlc"] = dlc.ToString(System.Globalization.CultureInfo.InvariantCulture);
            for (int i = 0; i < 8; ++i)
                command.Args["d" + i.ToString(System.Globalization.CultureInfo.InvariantCulture)] = data[i].ToString(System.Globalization.CultureInfo.InvariantCulture);

            return command;
        }

        private static SdhCommand CreateCanTxStopCommand(string controller)
        {
            var command = new SdhCommand
            {
                Target = "UCE.can.tx",
                Op = "stop"
            };

            command.Args["controller"] = controller;
            command.Args["slot"] = GwProtocol.UceCanTxStopAllSlots.ToString(System.Globalization.CultureInfo.InvariantCulture);
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

            _sdgwSession.EventReceived -= OnEventReceived;
            _dispatcher.Dispose();
            _disposed = true;
        }

    }
}
