using System;
using System.Diagnostics;
using SimulDIESEL.BLL.Boards.UCE;
using SimulDIESEL.DTL.Boards.UCE;
using SimulDIESEL.DTL.Boards.UCE.Can;
using SimulDIESEL.DTL.Protocols.SDGW;

namespace SimulDIESEL.BLL.Services.CAN
{
    /// <summary>
    /// Validated SDCTP event processor implementation.
    /// </summary>
    public sealed class CanEventProcessor
    {
        private readonly CanRxMirrorManager _mirrorManager;

        public CanEventProcessor(CanRxMirrorManager mirrorManager)
        {
            _mirrorManager = mirrorManager;
        }

        public bool ProcessEvent(byte type, byte[] payload)
        {
            switch (type)
            {
                case GwProtocol.UceCanCreateType:
                    CanCreateDto create;
                    if (!TryParseCreate(payload, out create))
                    {
                        Debug.WriteLine("CanEventProcessor: payload CAN_CREATE inválido.");
                        return false;
                    }
                    return ProcessCreate(create);
                case GwProtocol.UceCanEditType:
                    CanEditDto edit;
                    if (!TryParseEdit(payload, out edit))
                    {
                        Debug.WriteLine("CanEventProcessor: payload CAN_EDIT inválido.");
                        return false;
                    }
                    return ProcessEdit(edit);
                case GwProtocol.UceCanDeleteType:
                    CanDeleteDto delete;
                    if (!TryParseDelete(payload, out delete))
                    {
                        Debug.WriteLine("CanEventProcessor: payload CAN_DELETE inválido.");
                        return false;
                    }
                    return ProcessDelete(delete);
                case GwProtocol.UceCanTicType:
                    CanTicDto tic;
                    if (!TryParseTic(payload, out tic))
                    {
                        Debug.WriteLine("CanEventProcessor: payload CAN_TIC inválido.");
                        UceGatewayDiagnosticLog.AppendCanProtocolDiagnostic("CAN_TIC_INVALID_PAYLOAD", "CAN_TIC deve ter payload de 1 byte.", -1);
                        return false;
                    }
                    return ProcessTic(tic);
                case GwProtocol.UceCanRowType:
                    CanRowDto row;
                    if (!TryParseRow(payload, out row))
                    {
                        Debug.WriteLine("CanEventProcessor: payload CAN_ROW inválido.");
                        return false;
                    }
                    Debug.WriteLine("CanEventProcessor: roteando CAN_ROW para ApplyRow no index " + row.Index + ".");
                    ProcessRow(row);
                    return false;
                case GwProtocol.UceCanReadAllDoneType:
                    int count;
                    uint messageOrder;
                    if (!TryParseReadAllDone(payload, out count, out messageOrder))
                    {
                        Debug.WriteLine("CanEventProcessor: payload CAN_READ_ALL_DONE inválido.");
                        return false;
                    }
                    Debug.WriteLine(
                        "CanEventProcessor: roteando CAN_READ_ALL_DONE para ApplyReadAllDone count=" +
                        count.ToString() +
                        " messageOrder=" +
                        messageOrder.ToString() +
                        ".");
                    return ProcessReadAllDone(count, messageOrder);
                default:
                    return false;
            }
        }

        public void ProcessCanRxEvent(UceCanRxEvent canRxEvent)
        {
        }

        public bool ProcessCreate(CanCreateDto create)
        {
            return _mirrorManager.ApplyCreate(create);
        }

        public bool ProcessEdit(CanEditDto edit)
        {
            return _mirrorManager.ApplyEdit(edit);
        }

        public bool ProcessDelete(CanDeleteDto delete)
        {
            return _mirrorManager.ApplyDelete(delete);
        }

        public bool ProcessTic(CanTicDto tic)
        {
            return _mirrorManager.ApplyTic(tic);
        }

        public bool ProcessRow(CanRowDto row)
        {
            return _mirrorManager.ApplyRow(row);
        }

        public bool ProcessReadAllDone(int count, uint messageOrder)
        {
            return _mirrorManager.ApplyReadAllDone(count, messageOrder);
        }

        private static bool TryParseCreate(byte[] payload, out CanCreateDto create)
        {
            create = null;
            if (payload == null || payload.Length != GwProtocol.UceCanCreatePayloadLength)
                return false;

            create = new CanCreateDto
            {
                Index = payload[0],
                Valid = true,
                Flags = payload[1],
                CanId = ReadUInt32Le(payload, 2),
                Dlc = payload[6],
                Data = new byte[8],
                CycleTime = ReadUInt16Le(payload, 15),
                MessageOrder = ReadUInt32Le(payload, 17)
            };
            Buffer.BlockCopy(payload, 7, create.Data, 0, 8);
            return true;
        }

        private static bool TryParseEdit(byte[] payload, out CanEditDto edit)
        {
            edit = null;
            if (payload == null || payload.Length < GwProtocol.UceCanEditPayloadMinLength)
            {
                UceGatewayDiagnosticLog.AppendCanProtocolDiagnostic("CAN_EDIT_TRUNCATED", "Payload menor que o cabeçalho CAN_EDIT.", -1);
                return false;
            }

            int offset = 0;
            var candidate = new CanEditDto
            {
                Index = payload[offset++],
                Mask = payload[offset++],
                MessageOrder = ReadUInt32Le(payload, offset),
                Data = new byte[8]
            };
            offset += 4;

            if ((candidate.Mask & GwProtocol.UceCanCrudEditMaskFlags) != 0)
            {
                if (!CanRead(payload, offset, 1))
                {
                    UceGatewayDiagnosticLog.AppendCanProtocolDiagnostic("CAN_EDIT_TRUNCATED", "Payload terminou antes de FLAGS.", candidate.Index);
                    return false;
                }
                candidate.Flags = payload[offset++];
            }

            if ((candidate.Mask & GwProtocol.UceCanCrudEditMaskCanId) != 0)
            {
                if (!CanRead(payload, offset, 4))
                {
                    UceGatewayDiagnosticLog.AppendCanProtocolDiagnostic("CAN_EDIT_TRUNCATED", "Payload terminou antes de CAN_ID.", candidate.Index);
                    return false;
                }
                candidate.CanId = ReadUInt32Le(payload, offset);
                offset += 4;
            }

            if ((candidate.Mask & GwProtocol.UceCanCrudEditMaskDlc) != 0)
            {
                if (!CanRead(payload, offset, 1))
                {
                    UceGatewayDiagnosticLog.AppendCanProtocolDiagnostic("CAN_EDIT_TRUNCATED", "Payload terminou antes de DLC.", candidate.Index);
                    return false;
                }
                candidate.Dlc = payload[offset++];
            }

            if ((candidate.Mask & GwProtocol.UceCanCrudEditMaskData) != 0)
            {
                int bytesReservedAfterData = (candidate.Mask & GwProtocol.UceCanCrudEditMaskCycleTime) != 0 ? 2 : 0;
                int availableForData = payload.Length - offset - bytesReservedAfterData;
                if (availableForData <= 0)
                {
                    UceGatewayDiagnosticLog.AppendCanProtocolDiagnostic("CAN_EDIT_INVALID_DATA_MASK", "DATA ativo sem DATA_MASK ou DATA[8].", candidate.Index);
                    return false;
                }

                byte dataMask = payload[offset];
                int maskedByteCount = CountBits(dataMask);
                bool canReadMasked = dataMask != 0 && availableForData == 1 + maskedByteCount;
                if (canReadMasked)
                {
                    candidate.DataMask = dataMask;
                    candidate.UsesDataMask = true;
                    offset++;
                    for (int dataIndex = 0; dataIndex < 8; ++dataIndex)
                    {
                        if ((dataMask & (1 << dataIndex)) != 0)
                            candidate.Data[dataIndex] = payload[offset++];
                    }
                }
                else if (availableForData == 8)
                {
                    Buffer.BlockCopy(payload, offset, candidate.Data, 0, 8);
                    candidate.DataMask = 0xFF;
                    candidate.UsesDataMask = false;
                    offset += 8;
                }
                else
                {
                    UceGatewayDiagnosticLog.AppendCanProtocolDiagnostic("CAN_EDIT_INVALID_DATA_MASK", "DATA_MASK incompatível com o tamanho do payload.", candidate.Index);
                    return false;
                }
            }

            if ((candidate.Mask & GwProtocol.UceCanCrudEditMaskCycleTime) != 0)
            {
                if (!CanRead(payload, offset, 2))
                {
                    UceGatewayDiagnosticLog.AppendCanProtocolDiagnostic("CAN_EDIT_TRUNCATED", "Payload terminou antes de CYCLE_TIME.", candidate.Index);
                    return false;
                }
                candidate.CycleTime = ReadUInt16Le(payload, offset);
                offset += 2;
            }

            if (offset != payload.Length)
            {
                UceGatewayDiagnosticLog.AppendCanProtocolDiagnostic("CAN_EDIT_INVALID_PAYLOAD", "Payload CAN_EDIT possui bytes excedentes.", candidate.Index);
                return false;
            }

            edit = candidate;
            return true;
        }

        private static bool TryParseTic(byte[] payload, out CanTicDto tic)
        {
            tic = null;
            if (payload == null || payload.Length != GwProtocol.UceCanTicPayloadLength)
                return false;

            tic = new CanTicDto
            {
                Index = payload[0]
            };
            return true;
        }

        private static bool TryParseRow(byte[] payload, out CanRowDto row)
        {
            row = null;

            CanCreateDto create;
            if (!TryParseCreate(payload, out create))
                return false;

            row = new CanRowDto
            {
                Index = create.Index,
                Valid = true,
                Flags = create.Flags,
                CanId = create.CanId,
                Dlc = create.Dlc,
                Data = create.Data != null ? (byte[])create.Data.Clone() : new byte[8],
                CycleTime = create.CycleTime,
                MessageOrder = create.MessageOrder
            };
            return true;
        }

        private static bool TryParseDelete(byte[] payload, out CanDeleteDto delete)
        {
            delete = null;
            if (payload == null || payload.Length != GwProtocol.UceCanDeletePayloadLength)
                return false;

            delete = new CanDeleteDto
            {
                Index = payload[0],
                Reason = payload[1],
                MessageOrder = ReadUInt32Le(payload, 2)
            };
            return true;
        }

        private static bool TryParseReadAllDone(byte[] payload, out int count, out uint messageOrder)
        {
            count = 0;
            messageOrder = 0;

            if (payload == null || payload.Length != GwProtocol.UceCanReadAllDonePayloadLength)
                return false;

            count = payload[0];
            messageOrder = ReadUInt32Le(payload, 1);
            return true;
        }

        private static bool CanRead(byte[] payload, int offset, int count)
        {
            return payload != null && offset >= 0 && count >= 0 && offset + count <= payload.Length;
        }

        private static int CountBits(byte value)
        {
            int count = 0;
            for (int bit = 0; bit < 8; ++bit)
            {
                if ((value & (1 << bit)) != 0)
                    ++count;
            }
            return count;
        }

        private static ushort ReadUInt16Le(byte[] payload, int offset)
        {
            return (ushort)(payload[offset] | (payload[offset + 1] << 8));
        }

        private static uint ReadUInt32Le(byte[] payload, int offset)
        {
            return (uint)(payload[offset] |
                (payload[offset + 1] << 8) |
                (payload[offset + 2] << 16) |
                (payload[offset + 3] << 24));
        }
    }
}
