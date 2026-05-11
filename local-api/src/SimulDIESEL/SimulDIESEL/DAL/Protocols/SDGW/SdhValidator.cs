using System;
using System.Collections.Generic;
using System.Globalization;
using SimulDIESEL.DTL.Protocols.SDGW;

namespace SimulDIESEL.DAL.Protocols.SDGW
{
    public sealed class SdhCommandValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorCode { get; set; }
        public string Message { get; set; }
        public string Target { get; set; }
        public string Op { get; set; }
        public List<string> InvalidArgs { get; set; } = new List<string>();
    }

    public sealed class SdhValidator
    {
        private const string SupportedVersion = "sdh/1";
        private const string GsaBoard = "GSA";
        private const string UceBoard = "UCE";
        private const string BpmBoard = "BPM";
        private const string BpmResource = "gateway";
        private const string BpmPingOp = "ping";

        public void Validate(SdhCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (string.IsNullOrWhiteSpace(command.Version))
                throw new InvalidOperationException("Version SDH é obrigatória.");

            if (!string.Equals(command.Version, SupportedVersion, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Version SDH inválida. Versão suportada nesta fase: " + SupportedVersion + ".");

            if (string.IsNullOrWhiteSpace(command.Target))
                throw new InvalidOperationException("Target SDH é obrigatório.");

            if (string.IsNullOrWhiteSpace(command.Op))
                throw new InvalidOperationException("Op SDH é obrigatória.");

            SdhTarget target = SdhTarget.Parse(command.Target);

            if (string.Equals(target.Board, GsaBoard, StringComparison.OrdinalIgnoreCase))
            {
                ValidateGsaCommand(target, command);
                return;
            }

            if (string.Equals(target.Board, UceBoard, StringComparison.OrdinalIgnoreCase))
            {
                ValidateUceCommand(target, command);
                return;
            }

            if (string.Equals(target.Board, BpmBoard, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(target.Resource, BpmResource, StringComparison.OrdinalIgnoreCase) &&
                string.IsNullOrWhiteSpace(target.Subresource))
            {
                ValidateBpmGateway(command);
                return;
            }

            throw new NotSupportedException("Target SDH não suportado nesta fase: " + command.Target + ".");
        }

        public SdhCommandValidationResult ValidateOnly(SdhCommand command)
        {
            var result = new SdhCommandValidationResult
            {
                Target = command != null ? command.Target : null,
                Op = command != null ? command.Op : null
            };

            try
            {
                Validate(command);
                result.IsValid = true;
                result.ErrorCode = "OK";
                result.Message = "Comando SDH válido.";
                return result;
            }
            catch (ArgumentNullException ex)
            {
                result.IsValid = false;
                result.ErrorCode = "MISSING_COMMAND";
                result.Message = ex.Message;
                return result;
            }
            catch (NotSupportedException ex)
            {
                result.IsValid = false;
                result.ErrorCode = "UNSUPPORTED";
                result.Message = ex.Message;
                return result;
            }
            catch (InvalidOperationException ex)
            {
                result.IsValid = false;
                result.ErrorCode = "INVALID_ARG";
                result.Message = ex.Message;
                AddInvalidArgIfMentioned(result.InvalidArgs, ex.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                result.IsValid = false;
                result.ErrorCode = "INVALID_TARGET";
                result.Message = ex.Message;
                return result;
            }
        }

        private static void ValidateUceCommand(SdhTarget target, SdhCommand command)
        {
            if (string.Equals(target.Resource, "led", StringComparison.OrdinalIgnoreCase) &&
                string.IsNullOrWhiteSpace(target.Subresource))
            {
                ValidateUceLed(command);
                return;
            }

            if (string.Equals(target.Resource, "can", StringComparison.OrdinalIgnoreCase))
            {
                ValidateUceCan(target, command);
                return;
            }

            throw new NotSupportedException("Target SDH não suportado nesta fase: " + command.Target + ".");
        }

        private static void ValidateGsaCommand(SdhTarget target, SdhCommand command)
        {
            if (string.Equals(target.Resource, "led", StringComparison.OrdinalIgnoreCase) &&
                string.IsNullOrWhiteSpace(target.Subresource))
            {
                ValidateGsaLed(command);
                return;
            }

            if (string.Equals(target.Resource, "channel", StringComparison.OrdinalIgnoreCase))
            {
                ValidateGsaChannel(target, command);
                return;
            }

            if (string.Equals(target.Resource, "channels", StringComparison.OrdinalIgnoreCase))
            {
                ValidateGsaChannels(target, command);
                return;
            }

            if (string.Equals(target.Resource, "offset", StringComparison.OrdinalIgnoreCase) &&
                string.IsNullOrWhiteSpace(target.Subresource))
            {
                RequireOp(command, "reset");
                RequireArgCount(command, 0);
                return;
            }

            throw new NotSupportedException("Target SDH não suportado nesta fase: " + command.Target + ".");
        }

        private static void ValidateGsaLed(SdhCommand command)
        {
            RequireOp(command, "set");
            RequireArgCount(command, 1);
            RequireStateArg(command);
        }

        private static void ValidateUceLed(SdhCommand command)
        {
            RequireOp(command, "set");
            RequireArgCount(command, 1);
            RequireStateArg(command);
        }

        private static void ValidateUceCan(SdhTarget target, SdhCommand command)
        {
            if (string.Equals(target.Subresource, "config", StringComparison.OrdinalIgnoreCase))
            {
                RequireOp(command, "set");
                RequireArgCount(command, command.Args.ContainsKey("rxMode") ? 4 : 3);
                RequireControllerArg(command);
                RequireCanBitrateArg(command);
                RequireCanModeArg(command);
                if (command.Args.ContainsKey("rxMode"))
                    RequireCanRxModeArg(command);
                return;
            }

            if (string.Equals(target.Subresource, "enable", StringComparison.OrdinalIgnoreCase))
            {
                RequireOp(command, "set");
                RequireArgCount(command, 2);
                RequireControllerArg(command);
                RequireStateArg(command);
                return;
            }

            if (string.Equals(target.Subresource, "status", StringComparison.OrdinalIgnoreCase))
            {
                RequireOp(command, "get");
                RequireArgCount(command, 1);
                RequireControllerArg(command);
                return;
            }

            if (string.Equals(target.Subresource, "rx", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(command.Op, "poll", StringComparison.OrdinalIgnoreCase))
                {
                    RequireArgCount(command, 1);
                    RequireControllerArg(command);
                    return;
                }

                if (string.Equals(command.Op, "readAll", StringComparison.OrdinalIgnoreCase))
                {
                    RequireArgCount(command, 1);
                    RequireControllerArg(command);
                    return;
                }

                throw new NotSupportedException("Op SDH não suportada para " + command.Target + ": " + command.Op + ".");
            }

            if (string.Equals(target.Subresource, "driverLog", StringComparison.OrdinalIgnoreCase))
            {
                RequireOp(command, "poll");
                RequireArgCount(command, 1);
                RequireControllerArg(command);
                return;
            }

            if (string.Equals(target.Subresource, "tx", StringComparison.OrdinalIgnoreCase))
            {
                ValidateUceCanTx(command);
                return;
            }

            if (string.IsNullOrWhiteSpace(target.Subresource))
            {
                RequireOp(command, "reset");
                RequireArgCount(command, 1);
                RequireControllerArg(command);
                return;
            }

            throw new NotSupportedException("Target SDH não suportado nesta fase: " + command.Target + ".");
        }

        private static void ValidateGsaChannel(SdhTarget target, SdhCommand command)
        {
            string subresource = target.Subresource ?? string.Empty;

            if (string.Equals(subresource, "setpoint", StringComparison.OrdinalIgnoreCase))
            {
                RequireOp(command, "set");
                RequireArgCount(command, 2);
                RequireChannelArg(command);
                RequireByteArg(command, "value");
                return;
            }

            if (string.Equals(subresource, "enable", StringComparison.OrdinalIgnoreCase))
            {
                RequireOp(command, "set");
                RequireArgCount(command, 2);
                RequireChannelArg(command);
                RequireStateArg(command);
                return;
            }

            if (string.Equals(subresource, "status", StringComparison.OrdinalIgnoreCase))
            {
                RequireOp(command, "get");
                RequireArgCount(command, 1);
                RequireChannelArg(command);
                return;
            }

            if (string.Equals(subresource, "fault", StringComparison.OrdinalIgnoreCase))
            {
                RequireOp(command, "reset");
                RequireArgCount(command, 1);
                RequireChannelArg(command);
                return;
            }

            if (string.Equals(subresource, "offset", StringComparison.OrdinalIgnoreCase))
            {
                ValidateGsaChannelOffset(command);
                return;
            }

            throw new NotSupportedException("Target SDH não suportado nesta fase: " + command.Target + ".");
        }

        private static void ValidateUceCanTx(SdhCommand command)
        {
            if (string.Equals(command.Op, "send", StringComparison.OrdinalIgnoreCase))
            {
                RequireArgCount(command, 13);
                RequireControllerArg(command);
                bool extended = RequireBool01Arg(command, "extended");
                uint id = RequireUInt32Arg(command, "id");
                if (!extended && id > 0x7FFU)
                    throw new InvalidOperationException("ID inválido para CAN STD. Valor máximo: 0x7FF.");
                if (extended && id > 0x1FFFFFFFU)
                    throw new InvalidOperationException("ID inválido para CAN EXT. Valor máximo: 0x1FFFFFFF.");

                byte dlc = RequireByteArg(command, "dlc");
                if (dlc > 8)
                    throw new InvalidOperationException("DLC inválido para CAN_TX. Faixa aceita: 0..8.");

                int period = RequireIntArg(command, "period");
                if (period < 0 || period > ushort.MaxValue)
                    throw new InvalidOperationException("Periodo inválido para CAN_TX. Faixa aceita: 0..65535 ms.");

                for (int i = 0; i < 8; ++i)
                    RequireByteArg(command, "d" + i.ToString(CultureInfo.InvariantCulture));

                return;
            }

            if (string.Equals(command.Op, "direct", StringComparison.OrdinalIgnoreCase))
            {
                RequireArgCount(command, 13);
                RequireControllerArg(command);
                ValidateCanFrameArgs(command);
                return;
            }

            if (string.Equals(command.Op, "create", StringComparison.OrdinalIgnoreCase))
            {
                RequireArgCount(command, 16);
                RequireControllerArg(command);
                RequireTxIndexArg(command);
                ValidateCanFrameArgs(command);
                int period = RequireIntArg(command, "period");
                if (period < 0 || period > ushort.MaxValue)
                    throw new InvalidOperationException("Periodo inválido para CAN_TX_CREATE. Faixa aceita: 0..65535 ms.");
                byte enabled = RequireByteArg(command, "enabled");
                if (enabled > 1)
                    throw new InvalidOperationException("Enabled inválido para CAN_TX_CREATE. Valores aceitos: 0 ou 1.");
                return;
            }

            if (string.Equals(command.Op, "edit", StringComparison.OrdinalIgnoreCase))
            {
                ValidateUceCanTxEdit(command);
                return;
            }

            if (string.Equals(command.Op, "delete", StringComparison.OrdinalIgnoreCase))
            {
                RequireArgCount(command, 3);
                RequireControllerArg(command);
                RequireTxIndexArg(command);
                byte reason = RequireByteArg(command, "reason");
                if (reason < 1 || reason > 4)
                    throw new InvalidOperationException("Reason inválido para CAN_TX_DELETE. Faixa aceita: 1..4.");
                return;
            }

            if (string.Equals(command.Op, "stop", StringComparison.OrdinalIgnoreCase))
            {
                RequireArgCount(command, 2);
                RequireControllerArg(command);
                byte slot = RequireByteArg(command, "slot");
                if (slot != 0x00 && slot != 0xFF)
                    throw new InvalidOperationException("Slot inválido para CAN_TX stop. Valores aceitos: 0 ou 255.");
                return;
            }

            throw new NotSupportedException("Op SDH não suportada para " + command.Target + ": " + command.Op + ".");
        }

        private static void ValidateCanFrameArgs(SdhCommand command)
        {
            bool extended = RequireBool01Arg(command, "extended");
            RequireBool01Arg(command, "rtr");
            uint id = RequireUInt32Arg(command, "id");
            if (!extended && id > 0x7FFU)
                throw new InvalidOperationException("ID inválido para CAN STD. Valor máximo: 0x7FF.");
            if (extended && id > 0x1FFFFFFFU)
                throw new InvalidOperationException("ID inválido para CAN EXT. Valor máximo: 0x1FFFFFFF.");

            byte dlc = RequireByteArg(command, "dlc");
            if (dlc > 8)
                throw new InvalidOperationException("DLC inválido para CAN_TX. Faixa aceita: 0..8.");

            for (int i = 0; i < 8; ++i)
                RequireByteArg(command, "d" + i.ToString(CultureInfo.InvariantCulture));
        }

        private static void ValidateUceCanTxEdit(SdhCommand command)
        {
            RequireControllerArg(command);
            RequireTxIndexArg(command);
            byte mask = RequireByteArg(command, "mask");
            if ((mask & 0xC0) != 0)
                throw new InvalidOperationException("Mask inválida para CAN_TX_EDIT.");

            int expectedCount = 3;
            if ((mask & GwProtocol.UceCanTxEditMaskFlags) != 0)
            {
                byte flags = RequireByteArg(command, "flags");
                if ((flags & 0xFC) != 0)
                    throw new InvalidOperationException("Flags inválidas para CAN_TX_EDIT.");
                ++expectedCount;
            }
            if ((mask & GwProtocol.UceCanTxEditMaskCanId) != 0)
            {
                RequireUInt32Arg(command, "id");
                ++expectedCount;
            }
            if ((mask & GwProtocol.UceCanTxEditMaskDlc) != 0)
            {
                byte dlc = RequireByteArg(command, "dlc");
                if (dlc > 8)
                    throw new InvalidOperationException("DLC inválido para CAN_TX_EDIT. Faixa aceita: 0..8.");
                ++expectedCount;
            }
            if ((mask & GwProtocol.UceCanTxEditMaskData) != 0)
            {
                byte dataMask = RequireByteArg(command, "dataMask");
                if (dataMask == 0)
                    throw new InvalidOperationException("DATA_MASK inválida para CAN_TX_EDIT.");
                ++expectedCount;
                for (int i = 0; i < 8; ++i)
                {
                    if ((dataMask & (1 << i)) != 0)
                    {
                        RequireByteArg(command, "d" + i.ToString(CultureInfo.InvariantCulture));
                        ++expectedCount;
                    }
                }
            }
            if ((mask & GwProtocol.UceCanTxEditMaskPeriodMs) != 0)
            {
                int period = RequireIntArg(command, "period");
                if (period < 0 || period > ushort.MaxValue)
                    throw new InvalidOperationException("Periodo inválido para CAN_TX_EDIT. Faixa aceita: 0..65535 ms.");
                ++expectedCount;
            }
            if ((mask & GwProtocol.UceCanTxEditMaskEnabled) != 0)
            {
                byte enabled = RequireByteArg(command, "enabled");
                if (enabled > 1)
                    throw new InvalidOperationException("Enabled inválido para CAN_TX_EDIT. Valores aceitos: 0 ou 1.");
                ++expectedCount;
            }

            RequireArgCount(command, expectedCount);
        }

        private static void ValidateGsaChannels(SdhTarget target, SdhCommand command)
        {
            if (string.Equals(target.Subresource, "enable", StringComparison.OrdinalIgnoreCase))
            {
                RequireOp(command, "set");
                RequireArgCount(command, 1);
                RequireStateArg(command);
                return;
            }

            if (string.Equals(target.Subresource, "status", StringComparison.OrdinalIgnoreCase))
            {
                RequireOp(command, "get");
                RequireArgCount(command, 0);
                return;
            }

            throw new NotSupportedException("Target SDH não suportado nesta fase: " + command.Target + ".");
        }

        private static void ValidateGsaChannelOffset(SdhCommand command)
        {
            if (string.Equals(command.Op, "set", StringComparison.OrdinalIgnoreCase))
            {
                RequireArgCount(command, 3);
                RequireChannelArg(command);
                RequireKindArg(command);
                RequireInt16Arg(command, "value");
                return;
            }

            if (string.Equals(command.Op, "get", StringComparison.OrdinalIgnoreCase))
            {
                RequireArgCount(command, 2);
                RequireChannelArg(command);
                RequireKindArg(command);
                return;
            }

            if (string.Equals(command.Op, "save", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(command.Op, "reset", StringComparison.OrdinalIgnoreCase))
            {
                RequireArgCount(command, 1);
                RequireChannelArg(command);
                return;
            }

            throw new NotSupportedException("Op SDH não suportada para " + command.Target + ": " + command.Op + ".");
        }

        private static void ValidateBpmGateway(SdhCommand command)
        {
            RequireOp(command, BpmPingOp);
            RequireArgCount(command, 0);
        }

        private static void RequireOp(SdhCommand command, string expectedOp)
        {
            if (!string.Equals(command.Op, expectedOp, StringComparison.OrdinalIgnoreCase))
                throw new NotSupportedException("Op SDH não suportada para " + command.Target + ": " + command.Op + ".");
        }

        private static void RequireArgCount(SdhCommand command, int expectedCount)
        {
            if (command.Args.Count != expectedCount)
                throw new InvalidOperationException("Quantidade de argumentos inválida para " + command.Target + " " + command.Op + ".");
        }

        private static int RequireChannelArg(SdhCommand command)
        {
            int channel = RequireIntArg(command, "channel");
            if (channel < 1 || channel > 16)
                throw new InvalidOperationException("Canal inválido para " + command.Target + ". Faixa aceita: 1..16.");

            return channel;
        }

        private static byte RequireTxIndexArg(SdhCommand command)
        {
            byte index = RequireByteArg(command, "index");
            if (index >= GwProtocol.UceCanRxMirrorCapacity)
                throw new InvalidOperationException("Index inválido para CAN_TX. Faixa aceita: 0..99.");

            return index;
        }

        private static byte RequireByteArg(SdhCommand command, string argName)
        {
            int value = RequireIntArg(command, argName);
            if (value < byte.MinValue || value > byte.MaxValue)
                throw new InvalidOperationException("Valor inválido para " + command.Target + ": " + argName + " deve estar em 0..255.");

            return (byte)value;
        }

        private static short RequireInt16Arg(SdhCommand command, string argName)
        {
            int value = RequireIntArg(command, argName);
            if (value < short.MinValue || value > short.MaxValue)
                throw new InvalidOperationException("Valor inválido para " + command.Target + ": " + argName + " deve estar em -32768..32767.");

            return (short)value;
        }

        private static int RequireIntArg(SdhCommand command, string argName)
        {
            string rawValue;
            if (!command.Args.TryGetValue(argName, out rawValue) || string.IsNullOrWhiteSpace(rawValue))
                throw new InvalidOperationException("Argumento obrigatório ausente para " + command.Target + ": " + argName + ".");

            int parsedValue;
            if (!int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsedValue))
                throw new InvalidOperationException("Argumento inválido para " + command.Target + ": " + argName + " deve ser inteiro.");

            return parsedValue;
        }

        private static uint RequireUInt32Arg(SdhCommand command, string argName)
        {
            string rawValue;
            if (!command.Args.TryGetValue(argName, out rawValue) || string.IsNullOrWhiteSpace(rawValue))
                throw new InvalidOperationException("Argumento obrigatório ausente para " + command.Target + ": " + argName + ".");

            uint parsedValue;
            if (!uint.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsedValue))
                throw new InvalidOperationException("Argumento inválido para " + command.Target + ": " + argName + " deve ser uint32.");

            return parsedValue;
        }

        private static bool RequireBool01Arg(SdhCommand command, string argName)
        {
            string rawValue;
            if (!command.Args.TryGetValue(argName, out rawValue) || string.IsNullOrWhiteSpace(rawValue))
                throw new InvalidOperationException("Argumento obrigatório ausente para " + command.Target + ": " + argName + ".");

            if (string.Equals(rawValue, "1", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(rawValue, "true", StringComparison.OrdinalIgnoreCase))
                return true;

            if (string.Equals(rawValue, "0", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(rawValue, "false", StringComparison.OrdinalIgnoreCase))
                return false;

            throw new InvalidOperationException("Argumento inválido para " + command.Target + ": " + argName + " deve ser 0/1.");
        }

        private static string RequireStateArg(SdhCommand command)
        {
            string state;
            if (!command.Args.TryGetValue("state", out state) || string.IsNullOrWhiteSpace(state))
                throw new InvalidOperationException("Argumento obrigatório ausente para " + command.Target + ": state.");

            if (!string.Equals(state, "on", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(state, "off", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("State inválido para " + command.Target + ". Valores aceitos: on, off.");
            }

            return state;
        }

        private static string RequireKindArg(SdhCommand command)
        {
            string kind;
            if (!command.Args.TryGetValue("kind", out kind) || string.IsNullOrWhiteSpace(kind))
                throw new InvalidOperationException("Argumento obrigatório ausente para " + command.Target + ": kind.");

            if (!string.Equals(kind, "vout", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(kind, "vread", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(kind, "iread", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Kind inválido para " + command.Target + ". Valores aceitos: vout, vread, iread.");
            }

            return kind;
        }

        private static string RequireControllerArg(SdhCommand command)
        {
            string controller;
            if (!command.Args.TryGetValue("controller", out controller) || string.IsNullOrWhiteSpace(controller))
                throw new InvalidOperationException("Argumento obrigatório ausente para " + command.Target + ": controller.");

            if (!string.Equals(controller, "can0", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(controller, "can1", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Controller inválido para " + command.Target + ". Valores aceitos: can0, can1.");
            }

            return controller;
        }

        private static int RequireCanBitrateArg(SdhCommand command)
        {
            int bitrate = RequireIntArg(command, "bitrate");
            if (bitrate != 5 &&
                bitrate != 10 &&
                bitrate != 25 &&
                bitrate != 50 &&
                bitrate != 125 &&
                bitrate != 250 &&
                bitrate != 500 &&
                bitrate != 800 &&
                bitrate != 1000)
            {
                throw new InvalidOperationException("Bitrate inválido para " + command.Target + ". Valores aceitos: 5, 10, 25, 50, 125, 250, 500, 800, 1000.");
            }

            return bitrate;
        }

        private static string RequireCanModeArg(SdhCommand command)
        {
            string mode;
            if (!command.Args.TryGetValue("mode", out mode) || string.IsNullOrWhiteSpace(mode))
                throw new InvalidOperationException("Argumento obrigatório ausente para " + command.Target + ": mode.");

            if (!string.Equals(mode, "normal", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(mode, "listen", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(mode, "loopback", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Mode inválido para " + command.Target + ". Valores aceitos: normal, listen, loopback.");
            }

            return mode;
        }

        private static string RequireCanRxModeArg(SdhCommand command)
        {
            string rxMode;
            if (!command.Args.TryGetValue("rxMode", out rxMode) || string.IsNullOrWhiteSpace(rxMode))
                throw new InvalidOperationException("Argumento obrigatório ausente para " + command.Target + ": rxMode.");

            if (!string.Equals(rxMode, "auto", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(rxMode, "directOnly", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("RxMode inválido para " + command.Target + ". Valores aceitos: auto, directOnly.");
            }

            return rxMode;
        }

        private static void AddInvalidArgIfMentioned(List<string> invalidArgs, string message)
        {
            if (invalidArgs == null || string.IsNullOrWhiteSpace(message))
                return;

            string[] knownArgs =
            {
                "controller", "bitrate", "mode", "rxMode", "state", "channel", "value", "kind",
                "extended", "rtr", "id", "dlc", "period", "index", "mask", "flags", "dataMask",
                "enabled", "reason", "slot", "d0", "d1", "d2", "d3", "d4", "d5", "d6", "d7"
            };

            foreach (string arg in knownArgs)
            {
                if (message.IndexOf(arg, StringComparison.OrdinalIgnoreCase) >= 0 &&
                    !invalidArgs.Contains(arg))
                {
                    invalidArgs.Add(arg);
                }
            }
        }
    }
}
