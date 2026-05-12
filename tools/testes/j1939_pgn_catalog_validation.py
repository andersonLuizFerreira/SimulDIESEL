from pathlib import Path
import json
import shutil
import subprocess
import tempfile


ROOT = Path(__file__).resolve().parents[2]
ASSEMBLY = ROOT / "local-api/src/SimulDIESEL/SimulDIESEL/bin/x86/Debug/SimulDIESEL.exe"
CATALOG = ROOT / "Data/Protocols/J1939/j1939-pgn-standard-catalog.json"
REPORT = ROOT / "out/dumps/j1939_pgn_catalog_validation.md"
CSC = Path(r"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\Roslyn\csc.exe")


HARNESS = r'''
using System;
using System.Collections.Generic;
using SimulDIESEL.BLL.Protocols.J1939.Common;
using SimulDIESEL.BLL.Protocols.J1939.DataLink;
using SimulDIESEL.BLL.Protocols.J1939.Diagnostics;
using SimulDIESEL.BLL.Protocols.J1939.NetworkManagement;
using SimulDIESEL.DTL.Boards.UCE.Can;
using SimulDIESEL.DTL.Protocols.J1939.Common;

public static class Harness
{
    private static int failures;

    public static int Main()
    {
        var catalog = new J1939PgnStandardCatalog();
        ValidateRequiredPgns(catalog);
        ValidateUnknown(catalog);
        ValidateDuplicates(catalog);
        ValidateProprietaryBRange(catalog);
        ValidateDataLinkMetadata();
        ValidateDiagnosticsMetadata();
        ValidateNetworkManagementMetadata();
        Console.WriteLine("Catalog.Count=" + catalog.GetAll().Count);
        Console.WriteLine("failures=" + failures);
        return failures == 0 ? 0 : 1;
    }

    private static void ValidateRequiredPgns(J1939PgnStandardCatalog catalog)
    {
        J1939PgnDefinitionDto eec1 = catalog.FindByPgn(61444);
        Check("PGN61444.Exists", eec1 != null);
        Check("PGN61444.Acronym", eec1 != null && eec1.Acronym == "EEC1");
        Check("PGN61444.Label", eec1 != null && eec1.Label == "Electronic Engine Controller #1");
        Check("PGN61444.Document", eec1 != null && eec1.SaeDocument == "J1939-71");

        J1939PgnDefinitionDto dm1 = catalog.FindByPgn(65226);
        Check("PGN65226.Acronym", dm1 != null && dm1.Acronym == "DM1");
        Check("PGN65226.Document", dm1 != null && dm1.SaeDocument == "J1939-73");
        Check("PGN65226.MultiPacket", dm1 != null && dm1.MultiPacket);

        J1939PgnDefinitionDto ac = catalog.FindByPgn(60928);
        Check("PGN60928.Acronym", ac != null && ac.Acronym == "AC");
        Check("PGN60928.Document", ac != null && ac.SaeDocument == "J1939-81");

        Check("PGN59904.Request", catalog.GetAcronym(59904) == "REQ");
        Check("PGN60416.TPCM", catalog.GetAcronym(60416) == "TP.CM");
        Check("PGN60160.TPDT", catalog.GetAcronym(60160) == "TP.DT");
    }

    private static void ValidateUnknown(J1939PgnStandardCatalog catalog)
    {
        Check("Unknown.Null", catalog.FindByPgn(123456) == null);
        Check("Unknown.Contains", !catalog.Contains(123456));
        Check("Unknown.Display", catalog.GetDisplayName(123456) == "Unknown PGN");
    }

    private static void ValidateDuplicates(J1939PgnStandardCatalog catalog)
    {
        var seen = new HashSet<int>();
        foreach (J1939PgnDefinitionDto definition in catalog.GetAll())
        {
            if (definition.PgnEnd.HasValue)
                continue;

            if (seen.Contains(definition.Pgn))
            {
                Check("Catalog.NoDuplicate", false);
                return;
            }

            seen.Add(definition.Pgn);
        }

        Check("Catalog.NoDuplicate", true);
    }

    private static void ValidateProprietaryBRange(J1939PgnStandardCatalog catalog)
    {
        Check("PropB.Start", catalog.GetAcronym(65280) == "PropB");
        Check("PropB.Middle", catalog.GetAcronym(65300) == "PropB");
        Check("PropB.End", catalog.GetAcronym(65535) == "PropB");
    }

    private static void ValidateDataLinkMetadata()
    {
        var service = new J1939DataLinkService();
        var frame = new CanFrameDto
        {
            CanId = 0x18F004F9,
            IsExtended = true,
            Dlc = 8,
            Data = new byte[8],
            Timestamp = DateTime.Now
        };
        var result = service.ProcessCanFrame(frame);
        Check("DataLink.Acronym", result.PgnAcronym == "EEC1");
        Check("DataLink.Label", result.SingleFrameMessage != null && result.SingleFrameMessage.PgnLabel == "Electronic Engine Controller #1");
    }

    private static void ValidateDiagnosticsMetadata()
    {
        var dm1 = new J1939Dm1Decoder().Decode(0x80, 0xFF, DateTime.Now, new byte[] { 0, 0, 0, 0, 0, 0 }, false);
        Check("Diagnostics.Acronym", dm1.PgnAcronym == "DM1");
        Check("Diagnostics.Label", dm1.PgnLabel == "Active Diagnostic Trouble Codes");
    }

    private static void ValidateNetworkManagementMetadata()
    {
        byte[] name = new byte[8];
        var message = new SimulDIESEL.DTL.Protocols.J1939.DataLink.J1939DataLinkMessageDto
        {
            Pgn = 60928,
            FormattedPgn = "00EE00",
            Dlc = 8,
            Data = name,
            Timestamp = DateTime.Now,
            IdFields = new SimulDIESEL.DTL.Protocols.J1939.DataLink.J1939IdFieldsDto { SourceAddress = 0x80 }
        };
        var claim = new J1939AddressClaimDecoder().Decode(message);
        Check("Network.Acronym", claim.PgnAcronym == "AC");
        Check("Network.Label", claim.PgnLabel == "Address Claimed");
    }

    private static void Check(string name, bool ok)
    {
        Console.WriteLine(name + "=" + (ok ? "OK" : "FAIL"));
        if (!ok) failures++;
    }
}
'''


def validate_json_shape():
    entries = json.loads(CATALOG.read_text(encoding="utf-8"))
    return len(entries)


def main():
    if not ASSEMBLY.exists():
        raise SystemExit(f"Assembly not found: {ASSEMBLY}")
    if not CATALOG.exists():
        raise SystemExit(f"Catalog not found: {CATALOG}")

    json_count = validate_json_shape()
    with tempfile.TemporaryDirectory() as tmp:
        tmp_path = Path(tmp)
        harness = tmp_path / "J1939PgnCatalogHarness.cs"
        exe = tmp_path / "J1939PgnCatalogHarness.exe"
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
        completed = subprocess.run([str(exe)], cwd=str(ROOT), text=True, capture_output=True)

    ok = completed.returncode == 0
    REPORT.parent.mkdir(parents=True, exist_ok=True)
    lines = [
        "# Validacao Catalogo Padrao PGN J1939",
        "",
        f"- Resultado: `{'OK' if ok else 'FALHA'}`",
        f"- Entradas JSON: `{json_count}`",
        "- Casos: PGNs obrigatorios, desconhecido, duplicados, range Proprietary B e metadata nas camadas J1939.",
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
