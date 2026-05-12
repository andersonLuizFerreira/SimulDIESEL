using System;
using System.Collections.Generic;
using SimulDIESEL.BLL.Protocols.J1939.Common;
using SimulDIESEL.DTL.Protocols.J1939.DataLink;

namespace SimulDIESEL.BLL.Protocols.J1939.DataLink
{
    public sealed class J1939TransportSessionManager
    {
        private sealed class SessionState
        {
            public J1939TransportSessionDto Dto;
            public byte[] Data;
            public bool[] ReceivedPackets;
            public int BytesWritten;
        }

        private readonly Dictionary<string, SessionState> _sessions = new Dictionary<string, SessionState>();

        public J1939TransportSessionDto StartSession(J1939TransportControlMessageDto control, DateTime now)
        {
            if (control == null)
                throw new ArgumentNullException(nameof(control));

            string transportType = control.IsBam ? "BAM" : "RTS_CTS";
            string key = BuildKey(control.SourceAddress, control.DestinationAddress, control.TransportedPgn, transportType);
            SessionState state = new SessionState
            {
                Dto = new J1939TransportSessionDto
                {
                    SessionKey = key,
                    TransportType = transportType,
                    TransportedPgn = control.TransportedPgn,
                    FormattedTransportedPgn = control.FormattedTransportedPgn,
                    SourceAddress = control.SourceAddress,
                    DestinationAddress = control.DestinationAddress,
                    TotalMessageSize = control.TotalMessageSize,
                    TotalPackets = control.TotalPackets,
                    NextExpectedSequenceNumber = 1,
                    CreatedTimestamp = now,
                    LastActivityTimestamp = now,
                    DiagnosticText = control.IsBam ? J1939Constants.StatusTransportBamStarted : J1939Constants.StatusTransportRtsStarted
                },
                Data = new byte[control.TotalMessageSize],
                ReceivedPackets = new bool[control.TotalPackets + 1]
            };

            _sessions[key] = state;
            return CloneSession(state.Dto);
        }

        public J1939TransportSessionDto RegisterControl(J1939TransportControlMessageDto control, DateTime now)
        {
            if (control == null)
                return null;

            string transportType = control.IsBam ? "BAM" : "RTS_CTS";
            string key = BuildKey(control.SourceAddress, control.DestinationAddress, control.TransportedPgn, transportType);
            SessionState state;
            if (!_sessions.TryGetValue(key, out state))
                return null;

            state.Dto.LastActivityTimestamp = now;
            if (control.IsAbort)
            {
                state.Dto.DiagnosticText = J1939Constants.StatusTransportAbortReceived;
                _sessions.Remove(key);
            }

            return CloneSession(state.Dto);
        }

        public J1939TransportSessionDto AddDataPacket(J1939TransportDataPacketDto packet, uint transportedPgn, string transportType, DateTime now, out J1939ReassembledMessageDto reassembled)
        {
            reassembled = null;
            if (packet == null)
                return null;

            string key = BuildKey(packet.SourceAddress, packet.DestinationAddress, transportedPgn, transportType);
            SessionState state;
            if (!_sessions.TryGetValue(key, out state))
                return null;

            state.Dto.LastActivityTimestamp = now;
            if (packet.SequenceNumber == 0 ||
                packet.SequenceNumber > state.Dto.TotalPackets ||
                packet.SequenceNumber != state.Dto.NextExpectedSequenceNumber)
            {
                state.Dto.HasSequenceError = true;
                state.Dto.DiagnosticText = J1939Constants.StatusTransportSequenceError;
                return CloneSession(state.Dto);
            }

            int offset = (packet.SequenceNumber - 1) * J1939Constants.TransportPacketPayloadSize;
            for (int i = 0; i < J1939Constants.TransportPacketPayloadSize && offset + i < state.Data.Length; ++i)
            {
                state.Data[offset + i] = packet.Payload[i];
                ++state.BytesWritten;
            }

            state.ReceivedPackets[packet.SequenceNumber] = true;
            state.Dto.ReceivedPacketCount++;
            state.Dto.NextExpectedSequenceNumber = (byte)(packet.SequenceNumber + 1);
            state.Dto.DiagnosticText = J1939Constants.StatusTransportInProgress;

            if (state.Dto.ReceivedPacketCount >= state.Dto.TotalPackets)
            {
                state.Dto.IsComplete = true;
                state.Dto.DiagnosticText = J1939Constants.StatusTransportCompleted;
                reassembled = new J1939ReassembledMessageDto
                {
                    TransportedPgn = state.Dto.TransportedPgn,
                    FormattedTransportedPgn = state.Dto.FormattedTransportedPgn,
                    SourceAddress = state.Dto.SourceAddress,
                    DestinationAddress = state.Dto.DestinationAddress,
                    TotalSize = state.Dto.TotalMessageSize,
                    Data = (byte[])state.Data.Clone(),
                    TransportType = state.Dto.TransportType
                };
                _sessions.Remove(key);
            }

            return CloneSession(state.Dto);
        }

        public IReadOnlyList<J1939TransportSessionDto> CheckTimeouts(DateTime now)
        {
            List<J1939TransportSessionDto> timedOut = new List<J1939TransportSessionDto>();
            List<string> removeKeys = new List<string>();
            foreach (KeyValuePair<string, SessionState> item in _sessions)
            {
                if ((now - item.Value.Dto.LastActivityTimestamp).TotalMilliseconds < J1939Constants.TimeoutT3Milliseconds)
                    continue;

                item.Value.Dto.IsTimedOut = true;
                item.Value.Dto.DiagnosticText = J1939Constants.StatusTransportTimeout;
                timedOut.Add(CloneSession(item.Value.Dto));
                removeKeys.Add(item.Key);
            }

            foreach (string key in removeKeys)
                _sessions.Remove(key);

            return timedOut;
        }

        public J1939TransportSessionDto FindSessionForPacket(J1939TransportDataPacketDto packet, out uint transportedPgn, out string transportType)
        {
            transportedPgn = 0;
            transportType = null;
            if (packet == null)
                return null;

            foreach (SessionState state in _sessions.Values)
            {
                if (state.Dto.SourceAddress != packet.SourceAddress)
                    continue;

                if (state.Dto.DestinationAddress != packet.DestinationAddress)
                    continue;

                transportedPgn = state.Dto.TransportedPgn;
                transportType = state.Dto.TransportType;
                return CloneSession(state.Dto);
            }

            return null;
        }

        private static string BuildKey(byte sourceAddress, byte? destinationAddress, uint transportedPgn, string transportType)
        {
            return sourceAddress.ToString("X2") +
                ":" +
                (destinationAddress.HasValue ? destinationAddress.Value.ToString("X2") : "--") +
                ":" +
                J1939IdParser.FormatPgn(transportedPgn) +
                ":" +
                transportType;
        }

        private static J1939TransportSessionDto CloneSession(J1939TransportSessionDto source)
        {
            if (source == null)
                return null;

            return new J1939TransportSessionDto
            {
                SessionKey = source.SessionKey,
                TransportType = source.TransportType,
                TransportedPgn = source.TransportedPgn,
                FormattedTransportedPgn = source.FormattedTransportedPgn,
                SourceAddress = source.SourceAddress,
                DestinationAddress = source.DestinationAddress,
                TotalMessageSize = source.TotalMessageSize,
                TotalPackets = source.TotalPackets,
                NextExpectedSequenceNumber = source.NextExpectedSequenceNumber,
                ReceivedPacketCount = source.ReceivedPacketCount,
                IsComplete = source.IsComplete,
                HasSequenceError = source.HasSequenceError,
                IsTimedOut = source.IsTimedOut,
                DiagnosticText = source.DiagnosticText,
                CreatedTimestamp = source.CreatedTimestamp,
                LastActivityTimestamp = source.LastActivityTimestamp
            };
        }
    }
}
