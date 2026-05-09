from pathlib import Path
import shutil
import subprocess
import tempfile


ROOT = Path(__file__).resolve().parents[2]
ASSEMBLY = ROOT / "local-api/src/SimulDIESEL/SimulDIESEL/bin/x86/Debug/SimulDIESEL.exe"
REPORT = ROOT / "out/dumps/j1939_datalink_validation.md"
CSC = Path(r"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\Roslyn\csc.exe")


HARNESS = r'''
using System;
using SimulDIESEL.BLL.Protocols.J1939.DataLink;
using SimulDIESEL.DTL.Boards.UCE.Can;
using SimulDIESEL.DTL.Protocols.J1939.DataLink;

public static class Harness
{
    private static int failures;

    public static int Main()
    {
        var service = new J1939DataLinkService();
        ValidateIdParser(service);
        ValidateRequest(service);
        ValidateAcknowledgment(service);
        ValidateTpCmBam(service);
        ValidateTpDt(service);
        ValidateBamReassembly();
        ValidateIso15765(service);
        ValidateStandardFrame(service);
        Console.WriteLine("failures=" + failures);
        return failures == 0 ? 0 : 1;
    }

    private static void ValidateIdParser(J1939DataLinkService service)
    {
        var result = service.ProcessCanFrame(Frame(0x18DAFFF9, true, 8, new byte[8]));
        Check("A.Priority", result.IdFields.Priority == 6);
        Check("A.EDP", result.IdFields.ExtendedDataPage == false);
        Check("A.DP", result.IdFields.DataPage == false);
        Check("A.PF", result.IdFields.PduFormat == 0xDA);
        Check("A.PS", result.IdFields.PduSpecific == 0xFF);
        Check("A.SA", result.IdFields.SourceAddress == 0xF9);
        Check("A.PDU1", result.IdFields.IsPdu1);
        Check("A.DA", result.IdFields.DestinationAddress == 0xFF);
        Check("A.Global", result.IdFields.IsGlobalDestination);
        Check("A.PGN", result.Pgn == 0x00DA00);
    }

    private static void ValidateRequest(J1939DataLinkService service)
    {
        var result = service.ProcessCanFrame(Frame(0x18EAFFF9, true, 3, new byte[] { 0x00, 0xF0, 0x00 }));
        Check("B.Type", result.MessageType == J1939MessageTypeDto.Request);
        Check("B.PGN", result.Pgn == 0x00EA00);
        uint requested = result.SingleFrameMessage.Data[0] | ((uint)result.SingleFrameMessage.Data[1] << 8) | ((uint)result.SingleFrameMessage.Data[2] << 16);
        Check("B.RequestedPgn", requested == 0x00F000);
    }

    private static void ValidateAcknowledgment(J1939DataLinkService service)
    {
        var result = service.ProcessCanFrame(Frame(0x18E8FFF9, true, 8, new byte[] { 0x00, 0xFF, 0xFF, 0xFF, 0x80, 0x00, 0xEA, 0x00 }));
        Check("C.Type", result.MessageType == J1939MessageTypeDto.Acknowledgment);
        uint ackPgn = result.SingleFrameMessage.Data[5] | ((uint)result.SingleFrameMessage.Data[6] << 8) | ((uint)result.SingleFrameMessage.Data[7] << 16);
        Check("C.AckPgn", ackPgn == 0x00EA00);
    }

    private static void ValidateTpCmBam(J1939DataLinkService service)
    {
        var result = service.ProcessCanFrame(Frame(0x18ECFFF9, true, 8, new byte[] { 0x20, 0x11, 0x00, 0x03, 0xFF, 0x00, 0xF0, 0x00 }));
        Check("D.Type", result.MessageType == J1939MessageTypeDto.TransportConnectionManagement);
        Check("D.Control", result.TransportControlMessage != null && result.TransportControlMessage.IsBam);
        Check("D.Size", result.TransportControlMessage.TotalMessageSize == 17);
        Check("D.Packets", result.TransportControlMessage.TotalPackets == 3);
        Check("D.TransportedPgn", result.TransportControlMessage.TransportedPgn == 0x00F000);
    }

    private static void ValidateTpDt(J1939DataLinkService service)
    {
        var isolatedService = new J1939DataLinkService();
        var result = isolatedService.ProcessCanFrame(Frame(0x18EBFFF9, true, 8, new byte[] { 0x01, 1, 2, 3, 4, 5, 6, 7 }));
        Check("E.Type", result.MessageType == J1939MessageTypeDto.TransportDataTransfer);
        Check("E.Orphan", result.Status.Code == "TransportOrphanPacket");
        Check("E.Seq", result.TransportDataPacket.SequenceNumber == 1);
    }

    private static void ValidateBamReassembly()
    {
        var service = new J1939DataLinkService();
        service.ProcessCanFrame(Frame(0x18ECFFF9, true, 8, new byte[] { 0x20, 0x11, 0x00, 0x03, 0xFF, 0x00, 0xF0, 0x00 }));
        service.ProcessCanFrame(Frame(0x18EBFFF9, true, 8, new byte[] { 0x01, 1, 2, 3, 4, 5, 6, 7 }));
        service.ProcessCanFrame(Frame(0x18EBFFF9, true, 8, new byte[] { 0x02, 8, 9, 10, 11, 12, 13, 14 }));
        var done = service.ProcessCanFrame(Frame(0x18EBFFF9, true, 8, new byte[] { 0x03, 15, 16, 17, 0xFF, 0xFF, 0xFF, 0xFF }));
        Check("F.Complete", done.IsTransportSessionComplete);
        Check("F.Size", done.ReassembledMessage != null && done.ReassembledMessage.Data.Length == 17);
        Check("F.Last", done.ReassembledMessage != null && done.ReassembledMessage.Data[16] == 17);
    }

    private static void ValidateIso15765(J1939DataLinkService service)
    {
        var result = service.ProcessCanFrame(Frame(0x03000001, true, 8, new byte[8]));
        Check("G.Iso", result.MessageType == J1939MessageTypeDto.Iso15765);
        Check("G.NotJ1939", result.IsJ1939 == false);
    }

    private static void ValidateStandardFrame(J1939DataLinkService service)
    {
        var result = service.ProcessCanFrame(Frame(0x7E8, false, 8, new byte[8]));
        Check("H.Std", result.MessageType == J1939MessageTypeDto.NotJ1939);
        Check("H.NotJ1939", result.IsJ1939 == false);
    }

    private static CanFrameDto Frame(uint id, bool ext, byte dlc, byte[] data)
    {
        return new CanFrameDto { CanId = id, IsExtended = ext, Dlc = dlc, Data = data, Timestamp = DateTime.Now };
    }

    private static void Check(string name, bool ok)
    {
        Console.WriteLine(name + "=" + (ok ? "OK" : "FAIL"));
        if (!ok) failures++;
    }
}
'''


def main():
    if not ASSEMBLY.exists():
        raise SystemExit(f"Assembly not found: {ASSEMBLY}")

    with tempfile.TemporaryDirectory() as tmp:
        tmp_path = Path(tmp)
        harness = tmp_path / "J1939DataLinkHarness.cs"
        exe = tmp_path / "J1939DataLinkHarness.exe"
        local_assembly = tmp_path / ASSEMBLY.name
        harness.write_text(HARNESS, encoding="utf-8")
        shutil.copy2(ASSEMBLY, local_assembly)
        compiler = str(CSC) if CSC.exists() else "csc"
        subprocess.check_call([
            compiler,
            "/nologo",
            "/platform:x86",
            "/reference:" + str(local_assembly),
            "/out:" + str(exe),
            str(harness),
        ])
        completed = subprocess.run([str(exe)], cwd=str(tmp_path), text=True, capture_output=True)

    REPORT.parent.mkdir(parents=True, exist_ok=True)
    ok = completed.returncode == 0
    lines = [
        "# Validacao J1939 Data Link",
        "",
        f"- Resultado: `{'OK' if ok else 'FALHA'}`",
        "- Casos: ID parser, Request, Acknowledgment, TP.CM_BAM, TP.DT, remontagem BAM, ISO15765 e frame STD.",
        "",
        "```text",
        completed.stdout.strip(),
        "```",
    ]
    if completed.stderr.strip():
        lines.extend(["", "```text", completed.stderr.strip(), "```"])
    REPORT.write_text("\n".join(lines) + "\n", encoding="utf-8")
    print(f"report={REPORT.relative_to(ROOT)}")
    print(completed.stdout.strip())
    raise SystemExit(completed.returncode)


if __name__ == "__main__":
    main()
