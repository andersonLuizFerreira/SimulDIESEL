using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using SimulDIESEL.DTL.Protocols.J1939.Capture;

namespace SimulDIESEL.BLL.Protocols.J1939.Capture
{
    public sealed class J1939TemporalCaptureService
    {
        public const string EventNewFrame = "NEW_FRAME";
        public const string EventTransition = "TRANSITION";
        public const string EventPeriodicTick = "PERIODIC_TICK";
        public const string EventAddressClaim = "ADDRESS_CLAIM";
        private const uint PgnAddressClaimed = 0x00EE00;

        private readonly object _syncRoot = new object();
        private J1939CaptureSessionDto _session;
        private FrameSignature _lastSignature;
        private DateTime _lastSignatureFirstTimestamp;
        private DateTime _lastSignatureLastTimestamp;
        private int _lastSignatureRepeatCount;
        private long _lastSignatureIntervalSumMs;
        private int _lastSignatureIntervalSamples;
        private DateTime? _lastEmittedTimestamp;

        public bool IsCapturing
        {
            get
            {
                lock (_syncRoot)
                    return _session != null && _session.IsActive;
            }
        }

        public J1939CaptureSessionDto Start()
        {
            lock (_syncRoot)
            {
                ClearLocked();
                _session = new J1939CaptureSessionDto
                {
                    StartedAt = DateTime.Now,
                    IsActive = true
                };

                return CloneSession(_session);
            }
        }

        public J1939CaptureSessionDto Stop()
        {
            lock (_syncRoot)
            {
                if (_session == null)
                    return null;

                FlushPeriodicTickLocked();
                _session.IsActive = false;
                _session.StoppedAt = DateTime.Now;
                return CloneSession(_session);
            }
        }

        public void Clear()
        {
            lock (_syncRoot)
                ClearLocked();
        }

        public J1939CaptureSessionDto GetSnapshot()
        {
            lock (_syncRoot)
            {
                if (_session == null)
                    return null;

                return CloneSession(_session);
            }
        }

        public void RegisterFrame(
            DateTime timestamp,
            byte sourceAddress,
            byte? destinationAddress,
            bool isGlobalDestination,
            uint? rawCanId,
            uint pgn,
            string formattedPgn,
            byte[] data,
            string notes)
        {
            lock (_syncRoot)
            {
                if (_session == null || !_session.IsActive)
                    return;

                if (timestamp == default(DateTime))
                    timestamp = DateTime.Now;

                string dataHex = FormatPayload(data);
                FrameSignature signature = new FrameSignature(
                    sourceAddress,
                    isGlobalDestination ? (byte)0xFF : destinationAddress.GetValueOrDefault(0xFF),
                    isGlobalDestination,
                    rawCanId,
                    pgn,
                    string.IsNullOrWhiteSpace(formattedPgn) ? pgn.ToString("X6", CultureInfo.InvariantCulture) : formattedPgn,
                    dataHex,
                    data);

                _session.TotalFrameCount++;

                if (_lastSignature != null && _lastSignature.Equals(signature))
                {
                    long intervalMs = SafeDeltaMs(_lastSignatureLastTimestamp, timestamp);
                    _lastSignatureIntervalSumMs += intervalMs;
                    _lastSignatureIntervalSamples++;
                    _lastSignatureLastTimestamp = timestamp;
                    _lastSignatureRepeatCount++;
                    return;
                }

                string transitionNotes = _lastSignature == null
                    ? notes
                    : BuildTransitionNotes(_lastSignature, signature);
                string eventType = ResolveEventType(signature, _lastSignature == null ? EventNewFrame : EventTransition);

                FlushPeriodicTickLocked();
                AddEventLocked(signature, timestamp, eventType, 1, null, transitionNotes);
                _session.UniqueFrameCount++;
                _lastSignature = signature;
                _lastSignatureFirstTimestamp = timestamp;
                _lastSignatureLastTimestamp = timestamp;
                _lastSignatureRepeatCount = 1;
                _lastSignatureIntervalSumMs = 0;
                _lastSignatureIntervalSamples = 0;
            }
        }

        private void FlushPeriodicTickLocked()
        {
            if (_session == null || _lastSignature == null || _lastSignatureRepeatCount <= 1)
                return;

            long intervalMs = _lastSignatureIntervalSamples > 0
                ? _lastSignatureIntervalSumMs / _lastSignatureIntervalSamples
                : SafeDeltaMs(_lastSignatureFirstTimestamp, _lastSignatureLastTimestamp);

            AddEventLocked(
                _lastSignature,
                _lastSignatureLastTimestamp,
                EventPeriodicTick,
                _lastSignatureRepeatCount,
                intervalMs,
                "Frames identicos consecutivos agrupados para reduzir spam temporal.");
        }

        private void AddEventLocked(
            FrameSignature signature,
            DateTime timestamp,
            string eventType,
            int repeatCount,
            long? intervalMs,
            string notes)
        {
            long deltaMs = _lastEmittedTimestamp.HasValue
                ? SafeDeltaMs(_lastEmittedTimestamp.Value, timestamp)
                : 0;

            _session.Events.Add(new J1939CapturedEventDto
            {
                Timestamp = timestamp,
                DeltaMs = deltaMs,
                SourceAddress = signature.SourceAddress,
                DestinationAddress = signature.IsGlobalDestination ? (byte?)null : signature.DestinationAddress,
                IsGlobalDestination = signature.IsGlobalDestination,
                RawCanId = signature.RawCanId,
                Pgn = signature.Pgn,
                FormattedPgn = signature.FormattedPgn,
                DataHex = signature.DataHex,
                NameHex = signature.NameHex,
                ClaimedSourceAddress = signature.ClaimedSourceAddress,
                RepeatCount = repeatCount,
                EventType = eventType,
                IntervalMs = intervalMs,
                Notes = string.IsNullOrWhiteSpace(notes) ? string.Empty : notes
            });

            _lastEmittedTimestamp = timestamp;
        }

        private void ClearLocked()
        {
            _session = null;
            _lastSignature = null;
            _lastSignatureFirstTimestamp = default(DateTime);
            _lastSignatureLastTimestamp = default(DateTime);
            _lastSignatureRepeatCount = 0;
            _lastSignatureIntervalSumMs = 0;
            _lastSignatureIntervalSamples = 0;
            _lastEmittedTimestamp = null;
        }

        private static string BuildTransitionNotes(FrameSignature previous, FrameSignature current)
        {
            List<string> changes = new List<string>();
            if (previous.SourceAddress != current.SourceAddress)
                changes.Add("origem");
            if (previous.DestinationAddress != current.DestinationAddress ||
                previous.IsGlobalDestination != current.IsGlobalDestination)
                changes.Add("destino");
            if (previous.Pgn != current.Pgn)
                changes.Add("PGN");
            if (!string.Equals(previous.DataHex, current.DataHex, StringComparison.Ordinal))
                changes.Add("payload");

            if (changes.Count == 0)
                return string.Empty;

            return "Transicao detectada por mudanca de " + string.Join(", ", changes.ToArray()) + ".";
        }

        private static long SafeDeltaMs(DateTime start, DateTime end)
        {
            if (end < start)
                return 0;

            return (long)(end - start).TotalMilliseconds;
        }

        private static string FormatPayload(byte[] data)
        {
            if (data == null || data.Length == 0)
                return string.Empty;

            StringBuilder builder = new StringBuilder(data.Length * 3);
            for (int index = 0; index < data.Length; ++index)
            {
                if (index > 0)
                    builder.Append(' ');

                builder.Append(data[index].ToString("X2", CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }

        private static string ResolveEventType(FrameSignature signature, string defaultEventType)
        {
            if (signature != null &&
                signature.Pgn == PgnAddressClaimed &&
                !string.IsNullOrWhiteSpace(signature.NameHex))
                return EventAddressClaim;

            return defaultEventType;
        }

        private static string TryFormatNameHex(byte[] data)
        {
            if (data == null || data.Length < 8)
                return string.Empty;

            StringBuilder builder = new StringBuilder(16);
            for (int index = 7; index >= 0; --index)
                builder.Append(data[index].ToString("X2", CultureInfo.InvariantCulture));

            return builder.ToString();
        }

        private static J1939CaptureSessionDto CloneSession(J1939CaptureSessionDto source)
        {
            if (source == null)
                return null;

            J1939CaptureSessionDto clone = new J1939CaptureSessionDto
            {
                Id = source.Id,
                StartedAt = source.StartedAt,
                StoppedAt = source.StoppedAt,
                IsActive = source.IsActive,
                TotalFrameCount = source.TotalFrameCount,
                UniqueFrameCount = source.UniqueFrameCount
            };

            foreach (J1939CapturedEventDto item in source.Events)
            {
                clone.Events.Add(new J1939CapturedEventDto
                {
                    Timestamp = item.Timestamp,
                    DeltaMs = item.DeltaMs,
                    SourceAddress = item.SourceAddress,
                    DestinationAddress = item.DestinationAddress,
                    IsGlobalDestination = item.IsGlobalDestination,
                    RawCanId = item.RawCanId,
                    Pgn = item.Pgn,
                    FormattedPgn = item.FormattedPgn,
                    DataHex = item.DataHex,
                    NameHex = item.NameHex,
                    ClaimedSourceAddress = item.ClaimedSourceAddress,
                    RepeatCount = item.RepeatCount,
                    EventType = item.EventType,
                    IntervalMs = item.IntervalMs,
                    Notes = item.Notes
                });
            }

            return clone;
        }

        private sealed class FrameSignature : IEquatable<FrameSignature>
        {
            public FrameSignature(
                byte sourceAddress,
                byte destinationAddress,
                bool isGlobalDestination,
                uint? rawCanId,
                uint pgn,
                string formattedPgn,
                string dataHex,
                byte[] data)
            {
                SourceAddress = sourceAddress;
                DestinationAddress = destinationAddress;
                IsGlobalDestination = isGlobalDestination;
                RawCanId = rawCanId;
                Pgn = pgn;
                FormattedPgn = string.IsNullOrWhiteSpace(formattedPgn) ? pgn.ToString("X6", CultureInfo.InvariantCulture) : formattedPgn;
                DataHex = string.IsNullOrWhiteSpace(dataHex) ? string.Empty : dataHex;
                NameHex = pgn == PgnAddressClaimed ? TryFormatNameHex(data) : string.Empty;
                ClaimedSourceAddress = pgn == PgnAddressClaimed && !string.IsNullOrWhiteSpace(NameHex) ? (byte?)sourceAddress : null;
            }

            public byte SourceAddress { get; private set; }
            public byte DestinationAddress { get; private set; }
            public bool IsGlobalDestination { get; private set; }
            public uint? RawCanId { get; private set; }
            public uint Pgn { get; private set; }
            public string FormattedPgn { get; private set; }
            public string DataHex { get; private set; }
            public string NameHex { get; private set; }
            public byte? ClaimedSourceAddress { get; private set; }

            public bool Equals(FrameSignature other)
            {
                if (other == null)
                    return false;

                return SourceAddress == other.SourceAddress &&
                    DestinationAddress == other.DestinationAddress &&
                    IsGlobalDestination == other.IsGlobalDestination &&
                    Pgn == other.Pgn &&
                    string.Equals(DataHex, other.DataHex, StringComparison.Ordinal);
            }
        }
    }
}
