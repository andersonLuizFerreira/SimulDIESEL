from pathlib import Path
import shutil
import subprocess
import tempfile


ROOT = Path(__file__).resolve().parents[2]
ASSEMBLY = ROOT / "local-api/src/SimulDIESEL/SimulDIESEL/bin/x86/Debug/SimulDIESEL.exe"
REPORT = ROOT / "out/dumps/j1939_application_validation.md"
CSC = Path(r"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\Roslyn\csc.exe")


HARNESS = r'''
using System;
using SimulDIESEL.BLL.Protocols.J1939.Application;
using SimulDIESEL.DTL.Protocols.J1939.Application;
using SimulDIESEL.DTL.Protocols.J1939.DataLink;

public static class Harness
{
    private static int failures;

    public static int Main()
    {
        var app = new J1939ApplicationLayerService();
        ValidateEec1(app);
        ValidateEt1(app);
        ValidateEflp1(app);
        ValidateUnsupported(app);
        ValidateSpecialValues(app);
        ValidateReassembled(app);
        Console.WriteLine("failures=" + failures);
        return failures == 0 ? 0 : 1;
    }

    private static void ValidateEec1(J1939ApplicationLayerService app)
    {
        byte[] data = new byte[] { 0xFF, 0xFF, 0xFF, 0xC0, 0x12, 0xFF, 0xFF, 0xFF };
        var msg = app.Decode(Message(61444, "00F004", 0xF9, null, data));
        var spn = Find(msg, 190);
        Check("EEC1.Decoded", msg.IsDecoded);
        Check("EEC1.SPN190.Status", spn.Status == J1939SignalStatusDto.Valid);
        Check("EEC1.SPN190.Value", Nearly(spn.PhysicalValue, 600.0));
    }

    private static void ValidateEt1(J1939ApplicationLayerService app)
    {
        byte[] data = new byte[] { 80, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
        var msg = app.Decode(Message(65262, "00FEEE", 0xF9, null, data));
        var spn = Find(msg, 110);
        Check("ET1.SPN110.Status", spn.Status == J1939SignalStatusDto.Valid);
        Check("ET1.SPN110.Value", Nearly(spn.PhysicalValue, 40.0));
    }

    private static void ValidateEflp1(J1939ApplicationLayerService app)
    {
        byte[] data = new byte[] { 0xFF, 0xFF, 0xFF, 50, 0xFF, 0xFF, 0xFF, 0xFF };
        var msg = app.Decode(Message(65263, "00FEEF", 0xF9, null, data));
        var spn = Find(msg, 100);
        Check("EFLP1.SPN100.Status", spn.Status == J1939SignalStatusDto.Valid);
        Check("EFLP1.SPN100.Value", Nearly(spn.PhysicalValue, 200.0));
    }

    private static void ValidateUnsupported(J1939ApplicationLayerService app)
    {
        var msg = app.Decode(Message(12345, "003039", 0xF9, null, new byte[8]));
        Check("Unsupported.Status", msg.Status == "PgnNotSupportedYet");
        Check("Unsupported.NotDecoded", msg.IsDecoded == false);
    }

    private static void ValidateSpecialValues(J1939ApplicationLayerService app)
    {
        var na = app.Decode(Message(65262, "00FEEE", 0xF9, null, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }));
        Check("Special.0xFF", Find(na, 110).Status == J1939SignalStatusDto.NotAvailable);
        var err = app.Decode(Message(65262, "00FEEE", 0xF9, null, new byte[] { 0xFE, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }));
        Check("Special.0xFE", Find(err, 110).Status == J1939SignalStatusDto.ErrorIndicator);
    }

    private static void ValidateReassembled(J1939ApplicationLayerService app)
    {
        byte[] payload = new byte[] { 0xFF, 0xFF, 0xFF, 0xC0, 0x12, 0xFF, 0xFF, 0xFF, 0xAA };
        var msg = app.Decode(new J1939ReassembledMessageDto
        {
            TransportedPgn = 61444,
            FormattedTransportedPgn = "00F004",
            SourceAddress = 0x80,
            DestinationAddress = 0xFF,
            TotalSize = (ushort)payload.Length,
            Data = payload,
            TransportType = "BAM"
        });
        Check("Reassembled.PGN", msg.Pgn == 61444);
        Check("Reassembled.Source", msg.SourceAddress == 0x80);
        Check("Reassembled.SPN190", Nearly(Find(msg, 190).PhysicalValue, 600.0));
    }

    private static J1939DataLinkMessageDto Message(uint pgn, string hex, byte source, byte? destination, byte[] data)
    {
        return new J1939DataLinkMessageDto
        {
            Pgn = pgn,
            FormattedPgn = hex,
            Timestamp = DateTime.Now,
            Data = data,
            Dlc = (byte)Math.Min(8, data.Length),
            IdFields = new J1939IdFieldsDto { Pgn = pgn, FormattedPgn = hex, SourceAddress = source, DestinationAddress = destination }
        };
    }

    private static J1939DecodedSignalDto Find(J1939ApplicationMessageDto msg, int spn)
    {
        foreach (var signal in msg.Signals)
            if (signal.Spn == spn)
                return signal;
        throw new Exception("SPN not found: " + spn);
    }

    private static bool Nearly(double? actual, double expected)
    {
        return actual.HasValue && Math.Abs(actual.Value - expected) < 0.0001;
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
        harness = tmp_path / "J1939ApplicationHarness.cs"
        exe = tmp_path / "J1939ApplicationHarness.exe"
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
        "# Validacao J1939 Application Layer",
        "",
        f"- Resultado: `{'OK' if ok else 'FALHA'}`",
        "- Casos: EEC1/SPN190, ET1/SPN110, EFL/P1/SPN100, PGN nao cadastrado, valores 0xFF/0xFE e mensagem remontada.",
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
