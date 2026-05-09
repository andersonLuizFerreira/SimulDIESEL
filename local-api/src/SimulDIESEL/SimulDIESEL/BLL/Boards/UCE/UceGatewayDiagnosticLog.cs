using System;
using System.Globalization;
using System.IO;
using System.Text;
using SimulDIESEL.DTL.Boards.UCE;

namespace SimulDIESEL.BLL.Boards.UCE
{
    internal sealed class UceGatewayDiagnostic
    {
        public const byte ExtendedVersion = 0x01;
        public const byte LayerBpm = 0x01;
        public const byte LayerGwSpiBus = 0x02;
        public const byte LayerCrcValidation = 0x03;

        public const byte PhaseWrite = 0x01;
        public const byte PhaseWaitResponseReady = 0x02;
        public const byte PhaseReadHeader = 0x03;
        public const byte PhaseReadPayload = 0x04;
        public const byte PhaseFinalCrcValidation = 0x05;

        public const byte CauseFirstByteMisaligned = 0x01;
        public const byte CausePreloadFailure = 0x02;
        public const byte CauseWrongCrcPolynomial = 0x03;
        public const byte CauseEarlyReadBeforeResponseReady = 0x04;
        public const byte CauseLengthMismatch = 0x05;
        public const byte CauseTimeoutWaitingIrq = 0x06;
        public const byte CauseIncompleteFrame = 0x07;

        public static readonly string LogDirectory = @"C:\PROJETOS\SimulDIESEL\out\error_logs";
        public static readonly string LogFilePath = Path.Combine(LogDirectory, "uce_spi_crc_error_log.txt");

        public byte ErrorCode { get; set; }
        public bool HasExtendedData { get; set; }
        public byte Version { get; set; }
        public byte Layer { get; set; }
        public byte Phase { get; set; }
        public byte Cause { get; set; }
        public byte ExpectedLength { get; set; }
        public byte ReceivedLength { get; set; }
        public byte CrcCalculated { get; set; }
        public byte CrcReceived { get; set; }
        public byte[] TxBytes { get; set; } = Array.Empty<byte>();
        public byte[] RxBytes { get; set; } = Array.Empty<byte>();
        public byte[] RawDiagnosticValue { get; set; } = Array.Empty<byte>();
        public string ParseIssue { get; set; }
    }

    internal static class UceGatewayDiagnosticLog
    {
        public static UceGatewayDiagnostic Create(byte[] gatewayData)
        {
            var diagnostic = new UceGatewayDiagnostic();

            if (gatewayData == null || gatewayData.Length == 0)
            {
                diagnostic.ParseIssue = "Payload de diagnóstico da BPM ausente.";
                return diagnostic;
            }

            diagnostic.ErrorCode = gatewayData[0];
            diagnostic.RawDiagnosticValue = Copy(gatewayData);

            if (gatewayData.Length == 1)
                return diagnostic;

            diagnostic.HasExtendedData = gatewayData.Length >= 11;
            diagnostic.Version = gatewayData.Length > 1 ? gatewayData[1] : (byte)0x00;
            diagnostic.Layer = gatewayData.Length > 2 ? gatewayData[2] : (byte)0x00;
            diagnostic.Phase = gatewayData.Length > 3 ? gatewayData[3] : (byte)0x00;
            diagnostic.Cause = gatewayData.Length > 4 ? gatewayData[4] : (byte)0x00;
            byte txLen = gatewayData.Length > 5 ? gatewayData[5] : (byte)0x00;
            byte rxLen = gatewayData.Length > 6 ? gatewayData[6] : (byte)0x00;
            diagnostic.ExpectedLength = gatewayData.Length > 7 ? gatewayData[7] : (byte)0x00;
            diagnostic.ReceivedLength = gatewayData.Length > 8 ? gatewayData[8] : (byte)0x00;
            diagnostic.CrcCalculated = gatewayData.Length > 9 ? gatewayData[9] : (byte)0x00;
            diagnostic.CrcReceived = gatewayData.Length > 10 ? gatewayData[10] : (byte)0x00;

            if (gatewayData.Length < 11)
            {
                diagnostic.ParseIssue = "Payload estendido do gateway veio truncado.";
                return diagnostic;
            }

            int cursor = 11;
            int availableTx = Math.Max(0, gatewayData.Length - cursor);
            availableTx = Math.Min(availableTx, txLen);
            diagnostic.TxBytes = Slice(gatewayData, cursor, availableTx);
            cursor += availableTx;

            int availableRx = Math.Max(0, gatewayData.Length - cursor);
            availableRx = Math.Min(availableRx, rxLen);
            diagnostic.RxBytes = Slice(gatewayData, cursor, availableRx);

            if (availableTx != txLen || availableRx != rxLen)
            {
                diagnostic.ParseIssue = "Payload estendido do gateway não bate com os comprimentos declarados.";
            }

            return diagnostic;
        }

        public static void Append(UceGatewayDiagnostic diagnostic)
        {
            if (diagnostic == null)
                return;

            Directory.CreateDirectory(UceGatewayDiagnostic.LogDirectory);

            var builder = new StringBuilder();
            builder.AppendLine("============================================================");
            builder.Append("TIMESTAMP = ");
            builder.AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture));
            builder.Append("LAYER = ");
            builder.AppendLine(GetLayerText(diagnostic.Layer));
            builder.Append("STATUS = 0x");
            builder.AppendLine(diagnostic.ErrorCode.ToString("X2", CultureInfo.InvariantCulture));
            builder.AppendLine("WRITE PHASE:");
            builder.Append("TX: ");
            builder.AppendLine(FormatBytes(diagnostic.TxBytes));
            builder.AppendLine("READ HEADER PHASE:");
            builder.Append("TX: ");
            builder.AppendLine(GetHeaderPhaseTx(diagnostic));
            builder.Append("RX: ");
            builder.AppendLine(GetHeaderPhaseRx(diagnostic));
            builder.AppendLine("READ PAYLOAD PHASE:");
            builder.Append("TX: ");
            builder.AppendLine(GetPayloadPhaseTx(diagnostic));
            builder.Append("RX: ");
            builder.AppendLine(GetPayloadPhaseRx(diagnostic));
            builder.Append("RX RAW: ");
            builder.AppendLine(FormatBytes(diagnostic.RxBytes));

            AppendFrameInterpretation(builder, diagnostic);

            builder.Append("CRC CALCULATED = ");
            builder.AppendLine(FormatByteValue(diagnostic.CrcCalculated, diagnostic.HasExtendedData));
            builder.Append("CRC RECEIVED   = ");
            builder.AppendLine(FormatByteValue(diagnostic.CrcReceived, diagnostic.HasExtendedData));
            builder.Append("CRC MATCH      = ");
            builder.AppendLine(diagnostic.HasExtendedData
                ? (diagnostic.CrcCalculated == diagnostic.CrcReceived ? "TRUE" : "FALSE")
                : "UNKNOWN");
            builder.Append("EXPECTED LENGTH = ");
            builder.AppendLine(FormatLength(diagnostic.ExpectedLength, diagnostic.HasExtendedData));
            builder.Append("RECEIVED LENGTH = ");
            builder.AppendLine(FormatLength(diagnostic.ReceivedLength, diagnostic.HasExtendedData));
            builder.Append("ERROR PHASE = ");
            builder.AppendLine(GetPhaseText(diagnostic.Phase));
            builder.Append("POSSIBLE CAUSE = ");
            builder.AppendLine(GetCauseText(diagnostic.Cause));
            if (!string.IsNullOrWhiteSpace(diagnostic.ParseIssue))
            {
                builder.Append("PARSE NOTE = ");
                builder.AppendLine(diagnostic.ParseIssue);
            }
            builder.Append("RAW GATEWAY VALUE = ");
            builder.AppendLine(FormatBytes(diagnostic.RawDiagnosticValue));
            builder.AppendLine();

            File.AppendAllText(UceGatewayDiagnostic.LogFilePath, builder.ToString(), Encoding.ASCII);
        }

        public static void AppendDispatcherFifoOverflow(UceDispatcherOverflowDiagnostic diagnostic)
        {
            if (diagnostic == null)
                return;

            try
            {
                Directory.CreateDirectory(UceGatewayDiagnostic.LogDirectory);

                var builder = new StringBuilder();
                builder.AppendLine("============================================================");
                builder.Append("TIMESTAMP = ");
                builder.AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture));
                builder.AppendLine("LAYER = UCE / UceServiceDispatcher");
                builder.AppendLine("STATUS = DISPATCHER_FIFO_OVERFLOW");
                builder.AppendLine("WRITE PHASE:");
                builder.AppendLine("TX: <NOT APPLICABLE>");
                builder.AppendLine("READ HEADER PHASE:");
                builder.AppendLine("TX: <NOT APPLICABLE>");
                builder.AppendLine("RX: <NOT APPLICABLE>");
                builder.AppendLine("READ PAYLOAD PHASE:");
                builder.AppendLine("TX: <NOT APPLICABLE>");
                builder.AppendLine("RX: <NOT APPLICABLE>");
                builder.AppendLine("RX RAW: <NOT APPLICABLE>");
                builder.AppendLine("FRAME INTERPRETATION = DISPATCHER FIFO OVERFLOW");
                builder.AppendLine("CRC CALCULATED = NOT APPLICABLE");
                builder.AppendLine("CRC RECEIVED   = NOT APPLICABLE");
                builder.AppendLine("CRC MATCH      = NOT APPLICABLE");
                builder.AppendLine("EXPECTED LENGTH = NOT APPLICABLE");
                builder.AppendLine("RECEIVED LENGTH = NOT APPLICABLE");
                builder.AppendLine("ERROR PHASE = ASYNC EVENT FIFO");
                builder.AppendLine("POSSIBLE CAUSE = API/BPM/SPI slower than UCE event production");
                builder.Append("DISPATCHER FIFO COUNT = ");
                builder.AppendLine(diagnostic.OverflowCount.ToString(CultureInfo.InvariantCulture));
                builder.Append("DISPATCHER FIFO CAPACITY = ");
                builder.AppendLine(diagnostic.QueueSize.ToString(CultureInfo.InvariantCulture));
                builder.Append("MAX DISPATCH EVENT SIZE = ");
                builder.AppendLine(diagnostic.MaxEventSize.ToString(CultureInfo.InvariantCulture));
                builder.Append("RAW GATEWAY VALUE = ");
                builder.AppendLine(FormatBytes(BuildDispatcherFifoOverflowRawValue(diagnostic)));
                builder.AppendLine();

                File.AppendAllText(UceGatewayDiagnostic.LogFilePath, builder.ToString(), Encoding.ASCII);
            }
            catch (Exception)
            {
                // O diagnostico nao pode interromper o fluxo principal da aplicacao.
            }
        }

        public static void AppendCanMirrorOutOfSync(string reason, int index)
        {
            try
            {
                Directory.CreateDirectory(UceGatewayDiagnostic.LogDirectory);

                var builder = new StringBuilder();
                builder.AppendLine("============================================================");
                builder.Append("TIMESTAMP = ");
                builder.AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture));
                builder.AppendLine("LAYER = API / CAN MIRROR");
                builder.AppendLine("STATUS = MIRROR_OUT_OF_SYNC");
                builder.AppendLine("ERROR PHASE = CAN RX MIRROR");
                builder.AppendLine("POSSIBLE CAUSE = CAN table event referenced a row missing in the API mirror");
                builder.Append("INDEX = ");
                builder.AppendLine(index.ToString(CultureInfo.InvariantCulture));
                if (!string.IsNullOrWhiteSpace(reason))
                {
                    builder.Append("REASON = ");
                    builder.AppendLine(reason);
                }
                builder.AppendLine();

                File.AppendAllText(UceGatewayDiagnostic.LogFilePath, builder.ToString(), Encoding.ASCII);
            }
            catch (Exception)
            {
                // O diagnostico nao pode interromper a recuperacao automatica do espelho.
            }
        }

        public static void AppendCanProtocolDiagnostic(string status, string reason, int index)
        {
            try
            {
                Directory.CreateDirectory(UceGatewayDiagnostic.LogDirectory);

                var builder = new StringBuilder();
                builder.AppendLine("============================================================");
                builder.Append("TIMESTAMP = ");
                builder.AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture));
                builder.AppendLine("LAYER = API / CAN EVENT PROCESSOR");
                builder.Append("STATUS = ");
                builder.AppendLine(string.IsNullOrWhiteSpace(status) ? "CAN_PROTOCOL_DIAGNOSTIC" : status);
                builder.AppendLine("ERROR PHASE = CAN RX EVENT PARSE");
                builder.Append("INDEX = ");
                builder.AppendLine(index.ToString(CultureInfo.InvariantCulture));
                if (!string.IsNullOrWhiteSpace(reason))
                {
                    builder.Append("REASON = ");
                    builder.AppendLine(reason);
                }
                builder.AppendLine();

                File.AppendAllText(UceGatewayDiagnostic.LogFilePath, builder.ToString(), Encoding.ASCII);
            }
            catch (Exception)
            {
                // O diagnostico nao pode interromper o fluxo principal da aplicacao.
            }
        }

        public static string BuildCrcMessage(UceGatewayDiagnostic diagnostic)
        {
            if (diagnostic == null)
            {
                return "A BPM informou CRC inválido na resposta da UCE." +
                    Environment.NewLine +
                    "Consulte:" +
                    Environment.NewLine +
                    UceGatewayDiagnostic.LogFilePath;
            }

            if (!diagnostic.HasExtendedData)
            {
                return "A BPM informou CRC inválido na resposta da UCE." +
                    Environment.NewLine +
                    "Consulte:" +
                    Environment.NewLine +
                    UceGatewayDiagnostic.LogFilePath;
            }

            return "A BPM informou CRC inválido na resposta da UCE." +
                Environment.NewLine +
                "Esperado: " + FormatByteValue(diagnostic.CrcCalculated, true) +
                Environment.NewLine +
                "Recebido: " + FormatByteValue(diagnostic.CrcReceived, true) +
                Environment.NewLine +
                "Consulte:" +
                Environment.NewLine +
                UceGatewayDiagnostic.LogFilePath;
        }

        public static string BuildTimeoutMessage(UceGatewayDiagnostic diagnostic)
        {
            return "A BPM informou timeout ao falar com a UCE via SPI." +
                Environment.NewLine +
                "Consulte:" +
                Environment.NewLine +
                UceGatewayDiagnostic.LogFilePath;
        }

        public static string BuildDispatcherFifoOverflowMessage(UceDispatcherOverflowDiagnostic diagnostic)
        {
            return "UCE detectou overflow na fila FIFO do Dispatcher. Eventos assincronos podem ter sido perdidos." +
                Environment.NewLine +
                "Contador: " + (diagnostic == null ? "desconhecido" : diagnostic.OverflowCount.ToString(CultureInfo.InvariantCulture)) +
                Environment.NewLine +
                "Consulte:" +
                Environment.NewLine +
                UceGatewayDiagnostic.LogFilePath;
        }

        private static void AppendFrameInterpretation(StringBuilder builder, UceGatewayDiagnostic diagnostic)
        {
            if (diagnostic == null || diagnostic.RxBytes == null || diagnostic.RxBytes.Length < 2)
            {
                builder.AppendLine("FRAME INTERPRETATION = INSUFFICIENT DATA");
                return;
            }

            byte type = diagnostic.RxBytes[0];
            byte len = diagnostic.RxBytes[1];
            int availableValueLength = Math.Max(0, diagnostic.RxBytes.Length - 3);
            availableValueLength = Math.Min(availableValueLength, len);

            builder.Append("T = ");
            builder.AppendLine(FormatByteValue(type, true));
            builder.Append("L = ");
            builder.AppendLine(FormatByteValue(len, true));

            if (availableValueLength <= 0)
            {
                builder.AppendLine("V = <EMPTY>");
                return;
            }

            byte[] valueBytes = Slice(diagnostic.RxBytes, 2, availableValueLength);
            builder.Append("V = ");
            if (valueBytes.Length == 1)
                builder.AppendLine(FormatByteValue(valueBytes[0], true));
            else
                builder.AppendLine(FormatBytes(valueBytes));
        }

        private static string GetLayerText(byte layer)
        {
            switch (layer)
            {
                case UceGatewayDiagnostic.LayerBpm:
                    return "BPM";
                case UceGatewayDiagnostic.LayerGwSpiBus:
                    return "BPM / GwSpiBus";
                case UceGatewayDiagnostic.LayerCrcValidation:
                    return "BPM / GwSpiBus / CRC validation";
                default:
                    return "BPM / layer unknown";
            }
        }

        private static string GetPhaseText(byte phase)
        {
            switch (phase)
            {
                case UceGatewayDiagnostic.PhaseWrite:
                    return "WRITE PHASE";
                case UceGatewayDiagnostic.PhaseWaitResponseReady:
                    return "WAIT RESPONSE READY";
                case UceGatewayDiagnostic.PhaseReadHeader:
                    return "READ HEADER";
                case UceGatewayDiagnostic.PhaseReadPayload:
                    return "READ PAYLOAD";
                case UceGatewayDiagnostic.PhaseFinalCrcValidation:
                    return "FINAL CRC VALIDATION";
                default:
                    return "UNKNOWN";
            }
        }

        private static string GetCauseText(byte cause)
        {
            switch (cause)
            {
                case UceGatewayDiagnostic.CauseFirstByteMisaligned:
                    return "FIRST BYTE MISALIGNED";
                case UceGatewayDiagnostic.CausePreloadFailure:
                    return "PRELOAD FAILURE";
                case UceGatewayDiagnostic.CauseWrongCrcPolynomial:
                    return "WRONG CRC POLYNOMIAL";
                case UceGatewayDiagnostic.CauseEarlyReadBeforeResponseReady:
                    return "EARLY READ BEFORE RESPONSE READY";
                case UceGatewayDiagnostic.CauseLengthMismatch:
                    return "LENGTH MISMATCH";
                case UceGatewayDiagnostic.CauseTimeoutWaitingIrq:
                    return "TIMEOUT WAITING IRQ";
                case UceGatewayDiagnostic.CauseIncompleteFrame:
                    return "INCOMPLETE FRAME";
                default:
                    return "UNKNOWN";
            }
        }

        private static string GetHeaderPhaseTx(UceGatewayDiagnostic diagnostic)
        {
            if (diagnostic == null || diagnostic.RxBytes == null || diagnostic.RxBytes.Length == 0)
                return "<NOT CAPTURED>";

            return diagnostic.RxBytes.Length >= 2 ? "00 00" : "00";
        }

        private static string GetHeaderPhaseRx(UceGatewayDiagnostic diagnostic)
        {
            if (diagnostic == null || diagnostic.RxBytes == null || diagnostic.RxBytes.Length == 0)
                return "<NOT CAPTURED>";

            return FormatBytes(Slice(diagnostic.RxBytes, 0, Math.Min(2, diagnostic.RxBytes.Length)));
        }

        private static string GetPayloadPhaseTx(UceGatewayDiagnostic diagnostic)
        {
            if (diagnostic == null)
                return "<NOT CAPTURED>";

            int payloadPhaseLength = Math.Max(0, diagnostic.RxBytes.Length - 2);
            if (payloadPhaseLength == 0 && diagnostic.ExpectedLength > 2)
                payloadPhaseLength = diagnostic.ExpectedLength - 2;

            if (payloadPhaseLength <= 0)
                return "<NOT CAPTURED>";

            var bytes = new byte[payloadPhaseLength];
            return FormatBytes(bytes);
        }

        private static string GetPayloadPhaseRx(UceGatewayDiagnostic diagnostic)
        {
            if (diagnostic == null || diagnostic.RxBytes == null || diagnostic.RxBytes.Length <= 2)
                return "<NOT CAPTURED>";

            return FormatBytes(Slice(diagnostic.RxBytes, 2, diagnostic.RxBytes.Length - 2));
        }

        private static string FormatLength(byte value, bool available)
        {
            return available ? value.ToString(CultureInfo.InvariantCulture) : "UNKNOWN";
        }

        private static string FormatByteValue(byte value, bool available)
        {
            return available ? "0x" + value.ToString("X2", CultureInfo.InvariantCulture) : "UNKNOWN";
        }

        private static string FormatBytes(byte[] data)
        {
            if (data == null || data.Length == 0)
                return "<EMPTY>";

            var builder = new StringBuilder(data.Length * 3);
            for (int index = 0; index < data.Length; index++)
            {
                if (index > 0)
                    builder.Append(' ');

                builder.Append(data[index].ToString("X2", CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }

        private static byte[] Slice(byte[] source, int offset, int count)
        {
            if (source == null || count <= 0 || offset >= source.Length)
                return Array.Empty<byte>();

            int safeCount = Math.Min(count, source.Length - offset);
            var buffer = new byte[safeCount];
            Buffer.BlockCopy(source, offset, buffer, 0, safeCount);
            return buffer;
        }

        private static byte[] Copy(byte[] source)
        {
            return Slice(source, 0, source.Length);
        }

        private static byte[] BuildDispatcherFifoOverflowRawValue(UceDispatcherOverflowDiagnostic diagnostic)
        {
            var raw = new byte[7];
            raw[0] = 0x01;
            raw[1] = (byte)(diagnostic.OverflowCount & 0xFF);
            raw[2] = (byte)((diagnostic.OverflowCount >> 8) & 0xFF);
            raw[3] = (byte)((diagnostic.OverflowCount >> 16) & 0xFF);
            raw[4] = (byte)((diagnostic.OverflowCount >> 24) & 0xFF);
            raw[5] = diagnostic.QueueSize;
            raw[6] = diagnostic.MaxEventSize;
            return raw;
        }
    }
}
