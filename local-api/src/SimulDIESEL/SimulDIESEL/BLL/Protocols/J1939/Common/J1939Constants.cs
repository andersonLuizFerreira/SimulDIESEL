namespace SimulDIESEL.BLL.Protocols.J1939.Common
{
    public static class J1939Constants
    {
        public const uint ExtendedCanIdMask = 0x1FFFFFFF;
        public const byte Pdu1Threshold = 240;
        public const byte GlobalDestinationAddress = 0xFF;

        public const uint PgnRequest = 0x00EA00;
        public const uint PgnAcknowledgment = 0x00E800;
        public const uint PgnProprietaryA = 0x00EF00;
        public const uint PgnProprietaryA2 = 0x01EF00;
        public const uint PgnRequest2 = 0x00C900;
        public const uint PgnTransfer = 0x00CA00;
        public const uint PgnTransportConnectionManagement = 0x00EC00;
        public const uint PgnTransportDataTransfer = 0x00EB00;

        public const byte TpCmRts = 0x10;
        public const byte TpCmCts = 0x11;
        public const byte TpCmEndOfMsgAck = 0x13;
        public const byte TpCmBam = 0x20;
        public const byte TpCmConnectionAbort = 0xFF;

        public const ushort TransportMinMessageSize = 9;
        public const ushort TransportMaxMessageSize = 1785;
        public const byte TransportMinPackets = 2;
        public const byte TransportMaxPackets = 255;
        public const int TransportPacketPayloadSize = 7;

        public const int TimeoutTrMilliseconds = 200;
        public const int TimeoutThMilliseconds = 500;
        public const int TimeoutT1Milliseconds = 750;
        public const int TimeoutT2Milliseconds = 1250;
        public const int TimeoutT3Milliseconds = 1250;
        public const int TimeoutT4Milliseconds = 1050;

        public const string StatusOk = "Ok";
        public const string StatusNotExtendedFrame = "NotExtendedFrame";
        public const string StatusIso15765Frame = "Iso15765Frame";
        public const string StatusReservedEdpDpCombination = "ReservedEdpDpCombination";
        public const string StatusUnsupportedStandardFrame = "UnsupportedStandardFrame";
        public const string StatusSingleFrameDecoded = "SingleFrameDecoded";
        public const string StatusTransportBamStarted = "TransportBamStarted";
        public const string StatusTransportRtsStarted = "TransportRtsStarted";
        public const string StatusTransportInProgress = "TransportInProgress";
        public const string StatusTransportCompleted = "TransportCompleted";
        public const string StatusTransportOrphanPacket = "TransportOrphanPacket";
        public const string StatusTransportSequenceError = "TransportSequenceError";
        public const string StatusTransportTimeout = "TransportTimeout";
        public const string StatusTransportAbortReceived = "TransportAbortReceived";
        public const string StatusInvalidDlc = "InvalidDlc";
        public const string StatusInvalidTransportSize = "InvalidTransportSize";
    }
}
