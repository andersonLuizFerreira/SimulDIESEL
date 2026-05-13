using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using SimulDIESEL.DTL.Protocols.J1939.Capture;

namespace SimulDIESEL.BLL.Protocols.J1939.Capture
{
    public sealed class J1939CaptureExportService
    {
        public void ExportToFile(J1939CaptureSessionDto session, string path)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Caminho de exportacao da captura J1939 e obrigatorio.", nameof(path));

            string text = ExportToString(session, IsMarkdown(path));
            File.WriteAllText(path, text, new UTF8Encoding(false));
        }

        public string ExportToString(J1939CaptureSessionDto session, bool markdown)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));

            StringBuilder builder = new StringBuilder();
            WriteHeader(builder, markdown, "Sessao");
            WriteLine(builder, "Id", session.Id);
            WriteLine(builder, "Inicio", FormatTimestamp(session.StartedAt));
            WriteLine(builder, "Fim", session.StoppedAt.HasValue ? FormatTimestamp(session.StoppedAt.Value) : "captura ativa");
            WriteLine(builder, "DuracaoMs", session.DurationMs.ToString(CultureInfo.InvariantCulture));
            WriteLine(builder, "FramesRecebidos", session.TotalFrameCount.ToString(CultureInfo.InvariantCulture));
            WriteLine(builder, "FramesUnicos", session.UniqueFrameCount.ToString(CultureInfo.InvariantCulture));
            WriteLine(builder, "EventosExportados", session.Events.Count.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine();

            WriteSummary(builder, markdown, session);
            WriteEvents(builder, markdown, session);
            return builder.ToString();
        }

        private static void WriteSummary(StringBuilder builder, bool markdown, J1939CaptureSessionDto session)
        {
            WriteHeader(builder, markdown, "Resumo por PGN");
            foreach (IGrouping<uint, J1939CapturedEventDto> group in session.Events.GroupBy(item => item.Pgn).OrderByDescending(CountEffectiveFrames))
            {
                int frames = CountEffectiveFrames(group);
                builder.Append("- PGN=0x");
                builder.Append(group.Key.ToString("X6", CultureInfo.InvariantCulture));
                builder.Append(" eventos=");
                builder.Append(group.Count().ToString(CultureInfo.InvariantCulture));
                builder.Append(" frames=");
                builder.Append(frames.ToString(CultureInfo.InvariantCulture));
                builder.AppendLine();
            }

            builder.AppendLine();
            WriteHeader(builder, markdown, "Resumo por Source Address");
            foreach (IGrouping<byte, J1939CapturedEventDto> group in session.Events.GroupBy(item => item.SourceAddress).OrderByDescending(CountEffectiveFrames))
            {
                builder.Append("- SA=0x");
                builder.Append(group.Key.ToString("X2", CultureInfo.InvariantCulture));
                builder.Append(" eventos=");
                builder.Append(group.Count().ToString(CultureInfo.InvariantCulture));
                builder.Append(" frames=");
                builder.Append(CountEffectiveFrames(group).ToString(CultureInfo.InvariantCulture));
                builder.AppendLine();
            }

            builder.AppendLine();
            WriteHeader(builder, markdown, "Address Claims detectados");
            foreach (J1939CapturedEventDto item in session.Events.Where(IsAddressClaim).OrderBy(item => item.Timestamp))
            {
                builder.Append("- ");
                builder.Append(FormatTimestamp(item.Timestamp));
                builder.Append(" SA=0x");
                builder.Append(item.SourceAddress.ToString("X2", CultureInfo.InvariantCulture));
                if (!string.IsNullOrWhiteSpace(item.NameHex))
                {
                    builder.Append(" NAME=");
                    builder.Append(item.NameHex);
                }

                if (item.RawCanId.HasValue)
                {
                    builder.Append(" RawCanId=0x");
                    builder.Append(item.RawCanId.Value.ToString("X8", CultureInfo.InvariantCulture));
                }

                builder.AppendLine();
            }

            builder.AppendLine();
            WriteHeader(builder, markdown, "Periodicidade detectada");
            foreach (J1939CapturedEventDto item in session.Events.Where(IsPeriodic).OrderByDescending(item => item.RepeatCount))
            {
                builder.Append("- ");
                builder.Append(FormatTimestamp(item.Timestamp));
                builder.Append(" SA=0x");
                builder.Append(item.SourceAddress.ToString("X2", CultureInfo.InvariantCulture));
                builder.Append(" DST=");
                builder.Append(FormatDestination(item));
                builder.Append(" PGN=0x");
                builder.Append(item.Pgn.ToString("X6", CultureInfo.InvariantCulture));
                builder.Append(" intervalo_ms=");
                builder.Append(item.IntervalMs.GetValueOrDefault().ToString(CultureInfo.InvariantCulture));
                builder.Append(" repeticoes=");
                builder.Append(item.RepeatCount.ToString(CultureInfo.InvariantCulture));
                builder.AppendLine();
            }

            builder.AppendLine();
            WriteHeader(builder, markdown, "Top talkers");
            foreach (IGrouping<byte, J1939CapturedEventDto> group in session.Events.GroupBy(item => item.SourceAddress).OrderByDescending(CountEffectiveFrames).Take(10))
            {
                builder.Append("- SA=0x");
                builder.Append(group.Key.ToString("X2", CultureInfo.InvariantCulture));
                builder.Append(" frames=");
                builder.Append(CountEffectiveFrames(group).ToString(CultureInfo.InvariantCulture));
                builder.AppendLine();
            }

            builder.AppendLine();
        }

        private static void WriteEvents(StringBuilder builder, bool markdown, J1939CaptureSessionDto session)
        {
            WriteHeader(builder, markdown, "Eventos");
            foreach (J1939CapturedEventDto item in session.Events)
            {
                builder.Append("[");
                builder.Append(FormatTimestamp(item.Timestamp));
                builder.AppendLine("]");
                WriteLine(builder, "DeltaMs", item.DeltaMs.ToString(CultureInfo.InvariantCulture));
                WriteLine(builder, "Origem", "0x" + item.SourceAddress.ToString("X2", CultureInfo.InvariantCulture));
                WriteLine(builder, "Destino", FormatDestination(item));
                if (item.RawCanId.HasValue)
                    WriteLine(builder, "RawCanId", "0x" + item.RawCanId.Value.ToString("X8", CultureInfo.InvariantCulture));
                WriteLine(builder, "PGN", "0x" + item.Pgn.ToString("X6", CultureInfo.InvariantCulture));
                WriteLine(builder, "Dados", item.DataHex);
                WriteLine(builder, "Evento", item.EventType);
                if (!string.IsNullOrWhiteSpace(item.NameHex))
                    WriteLine(builder, "NameHex", item.NameHex);
                if (item.ClaimedSourceAddress.HasValue)
                    WriteLine(builder, "ClaimedSA", "0x" + item.ClaimedSourceAddress.Value.ToString("X2", CultureInfo.InvariantCulture));
                WriteLine(builder, "Repeticoes", item.RepeatCount.ToString(CultureInfo.InvariantCulture));
                if (item.IntervalMs.HasValue)
                    WriteLine(builder, "IntervaloMs", item.IntervalMs.Value.ToString(CultureInfo.InvariantCulture));
                if (!string.IsNullOrWhiteSpace(item.Notes))
                    WriteLine(builder, "Notas", item.Notes);
                builder.AppendLine();
            }
        }

        private static void WriteHeader(StringBuilder builder, bool markdown, string title)
        {
            builder.Append(markdown ? "# " : "## ");
            builder.AppendLine(title);
        }

        private static void WriteLine(StringBuilder builder, string key, string value)
        {
            builder.Append(key);
            builder.Append(": ");
            builder.AppendLine(string.IsNullOrWhiteSpace(value) ? "-" : value);
        }

        private static int CountEffectiveFrames(IEnumerable<J1939CapturedEventDto> events)
        {
            return events.Sum(item => item.RepeatCount <= 0 ? 1 : item.RepeatCount);
        }

        private static bool IsPeriodic(J1939CapturedEventDto item)
        {
            return item != null &&
                string.Equals(item.EventType, J1939TemporalCaptureService.EventPeriodicTick, StringComparison.Ordinal);
        }

        private static bool IsAddressClaim(J1939CapturedEventDto item)
        {
            return item != null &&
                string.Equals(item.EventType, J1939TemporalCaptureService.EventAddressClaim, StringComparison.Ordinal);
        }

        private static string FormatDestination(J1939CapturedEventDto item)
        {
            if (item == null || item.IsGlobalDestination || !item.DestinationAddress.HasValue)
                return "GLOBAL";

            return "0x" + item.DestinationAddress.Value.ToString("X2", CultureInfo.InvariantCulture);
        }

        private static string FormatTimestamp(DateTime timestamp)
        {
            return timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
        }

        private static bool IsMarkdown(string path)
        {
            string extension = Path.GetExtension(path);
            return string.Equals(extension, ".md", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(extension, ".markdown", StringComparison.OrdinalIgnoreCase);
        }
    }
}
