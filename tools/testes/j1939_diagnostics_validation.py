from pathlib import Path
import shutil
import subprocess
import tempfile


ROOT = Path(__file__).resolve().parents[2]
ASSEMBLY = ROOT / "local-api/src/SimulDIESEL/SimulDIESEL/bin/x86/Debug/SimulDIESEL.exe"
REPORT = ROOT / "out/dumps/j1939_diagnostics_validation.md"
CSC = Path(r"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\Roslyn\csc.exe")


HARNESS = r'''
using System;
using SimulDIESEL.BLL.Protocols.J1939.Diagnostics;
using SimulDIESEL.DTL.Boards.UCE.Can;
using SimulDIESEL.DTL.Protocols.J1939.DataLink;
using SimulDIESEL.DTL.Protocols.J1939.Diagnostics;

public static class Harness
{
    private static int failures;

    public static int Main()
    {
        ValidateDm1NoFault();
        ValidateDm1GrandfatheredNoFault();
        ValidateDm1OneDtc();
        ValidateDm2OneDtc();
        ValidateReassembledDm1();
        ValidateFmiCatalog();
        ValidateKnownAndUnknownSpn();
        ValidateRequests();
        Console.WriteLine("failures=" + failures);
        return failures == 0 ? 0 : 1;
    }

    private static void ValidateDm1NoFault()
    {
        var decoder = new J1939Dm1Decoder();
        var message = decoder.Decode(0x80, 0xFF, DateTime.Now, new byte[] { 0, 0, 0, 0, 0, 0 }, false);
        Check("DM1.NoFault.Status", message.Status == "Sem DTC");
        Check("DM1.NoFault.Count", message.Dtcs.Count == 0);
    }

    private static void ValidateDm1GrandfatheredNoFault()
    {
        var decoder = new J1939Dm1Decoder();
        var message = decoder.Decode(0x80, 0xFF, DateTime.Now, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, false);
        Check("DM1.GrandfatheredNoFault.Status", message.Status == "Sem DTC");
        Check("DM1.GrandfatheredNoFault.Count", message.Dtcs.Count == 0);
    }

    private static void ValidateDm1OneDtc()
    {
        var decoder = new J1939Dm1Decoder();
        byte[] dtc = EncodeDtc(91, 3, 5, 0);
        var message = decoder.Decode(0x80, 0xFF, DateTime.Now, Payload(dtc), false);
        Check("DM1.OneDtc.Count", message.Dtcs.Count == 1);
        Check("DM1.OneDtc.SPN", message.Dtcs[0].Spn == 91);
        Check("DM1.OneDtc.FMI", message.Dtcs[0].Fmi == 3);
        Check("DM1.OneDtc.OC", message.Dtcs[0].OccurrenceCount == 5);
        Check("DM1.OneDtc.CM", message.Dtcs[0].ConversionMethod == 0);
    }

    private static void ValidateDm2OneDtc()
    {
        var decoder = new J1939Dm2Decoder();
        byte[] dtc = EncodeDtc(190, 2, 1, 0);
        var message = decoder.Decode(0x81, 0xFF, DateTime.Now, Payload(dtc), false);
        Check("DM2.OneDtc.Type", message.Type == "DM2");
        Check("DM2.OneDtc.SPN", message.Dtcs[0].Spn == 190);
        Check("DM2.OneDtc.FMI", message.Dtcs[0].Fmi == 2);
        Check("DM2.OneDtc.OC", message.Dtcs[0].OccurrenceCount == 1);
    }

    private static void ValidateReassembledDm1()
    {
        var service = new J1939DiagnosticsService();
        byte[] dtc1 = EncodeDtc(91, 3, 5, 0);
        byte[] dtc2 = EncodeDtc(190, 2, 1, 0);
        byte[] data = new byte[] { 0, 0, dtc1[0], dtc1[1], dtc1[2], dtc1[3], dtc2[0], dtc2[1], dtc2[2], dtc2[3] };
        J1939DiagnosticMessageDto message;
        bool decoded = service.TryDecode(new J1939ReassembledMessageDto
        {
            TransportedPgn = J1939DiagnosticRequestService.Dm1Pgn,
            FormattedTransportedPgn = "00FECA",
            SourceAddress = 0x80,
            DestinationAddress = 0xFF,
            TotalSize = (ushort)data.Length,
            Data = data,
            TransportType = "BAM"
        }, out message);

        Check("DM1.Reassembled.Decoded", decoded);
        Check("DM1.Reassembled.Count", message.Dtcs.Count == 2);
        Check("DM1.Reassembled.Flag", message.IsReassembled);
    }

    private static void ValidateFmiCatalog()
    {
        var catalog = new J1939FmiCatalog();
        Check("FMI.3.Description", catalog.Get(3).Description.IndexOf("Voltage above normal", StringComparison.OrdinalIgnoreCase) >= 0);
    }

    private static void ValidateKnownAndUnknownSpn()
    {
        var parser = new J1939DtcParser();
        Check("SPN.Known.190", parser.Parse(EncodeDtc(190, 2, 1, 0)[0], EncodeDtc(190, 2, 1, 0)[1], EncodeDtc(190, 2, 1, 0)[2], EncodeDtc(190, 2, 1, 0)[3]).SpnName == "Engine Speed");
        var unknown = EncodeDtc(123456, 3, 1, 0);
        Check("SPN.Unknown", parser.Parse(unknown[0], unknown[1], unknown[2], unknown[3]).SpnName == "SPN nao cadastrado");
    }

    private static void ValidateRequests()
    {
        var service = new J1939DiagnosticRequestService();
        CanFrameDto dm1 = service.BuildDm1Request();
        CanFrameDto dm2 = service.BuildDm2Request();
        Check("Request.DM1.Payload", dm1.Dlc == 3 && dm1.Data[0] == 0xCA && dm1.Data[1] == 0xFE && dm1.Data[2] == 0x00);
        Check("Request.DM2.Payload", dm2.Dlc == 3 && dm2.Data[0] == 0xCB && dm2.Data[1] == 0xFE && dm2.Data[2] == 0x00);
        Check("Request.PGN59904", dm1.IsExtended && dm1.CanId == 0x18EAFFF9U);
    }

    private static byte[] Payload(byte[] dtc)
    {
        return new byte[] { 0, 0, dtc[0], dtc[1], dtc[2], dtc[3] };
    }

    private static byte[] EncodeDtc(int spn, int fmi, int oc, int cm)
    {
        return new byte[]
        {
            (byte)(spn & 0xFF),
            (byte)((spn >> 8) & 0xFF),
            (byte)(((spn >> 16) & 0x07) << 5 | (fmi & 0x1F)),
            (byte)(((cm & 0x01) << 7) | (oc & 0x7F))
        };
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
        harness = tmp_path / "J1939DiagnosticsHarness.cs"
        exe = tmp_path / "J1939DiagnosticsHarness.exe"
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
        "# Validacao J1939 Diagnostics J1939-73",
        "",
        f"- Resultado: `{'OK' if ok else 'FALHA'}`",
        "- Casos: DM1 sem DTC, DM1/DM2 com DTC, DM1 remontado, FMI, SPN conhecido/desconhecido e Request PGN 59904.",
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
