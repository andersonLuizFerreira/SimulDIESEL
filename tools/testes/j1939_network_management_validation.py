from pathlib import Path
import shutil
import subprocess
import tempfile


ROOT = Path(__file__).resolve().parents[2]
ASSEMBLY = ROOT / "local-api/src/SimulDIESEL/SimulDIESEL/bin/x86/Debug/SimulDIESEL.exe"
REPORT = ROOT / "out/dumps/j1939_network_management_validation.md"
CSC = Path(r"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\Roslyn\csc.exe")


HARNESS = r'''
using System;
using SimulDIESEL.BLL.Protocols.J1939.Common;
using SimulDIESEL.BLL.Protocols.J1939.Diagnostics;
using SimulDIESEL.BLL.Protocols.J1939.NetworkManagement;
using SimulDIESEL.DTL.Boards.UCE.Can;
using SimulDIESEL.DTL.Protocols.J1939.DataLink;
using SimulDIESEL.DTL.Protocols.J1939.NetworkManagement;

public static class Harness
{
    private static int failures;

    public static int Main()
    {
        byte[] name = BuildName(0x12345, 0x456, 2, 3, 0x9A, 0, 0x22, 4, 5, true);
        ValidateNameParser(name);
        ValidateAddressClaim(name);
        ValidateCannotClaim(name);
        ValidateRequestAddressClaim();
        ValidateConflict(name);
        ValidateSameNameRefresh(name);
        ValidateWorkingSet(name);
        ValidateCommandedAddress(name);
        ValidateToolAddressConstant();
        Console.WriteLine("failures=" + failures);
        return failures == 0 ? 0 : 1;
    }

    private static void ValidateNameParser(byte[] name)
    {
        var parsed = new J1939NameParser().Parse(name);
        Check("NAME.IdentityNumber", parsed.IdentityNumber == 0x12345);
        Check("NAME.ManufacturerCode", parsed.ManufacturerCode == 0x456);
        Check("NAME.EcuInstance", parsed.EcuInstance == 2);
        Check("NAME.FunctionInstance", parsed.FunctionInstance == 3);
        Check("NAME.Function", parsed.Function == 0x9A);
        Check("NAME.Reserved", parsed.Reserved == 0);
        Check("NAME.VehicleSystem", parsed.VehicleSystem == 0x22);
        Check("NAME.VehicleSystemInstance", parsed.VehicleSystemInstance == 4);
        Check("NAME.IndustryGroup", parsed.IndustryGroup == 5);
        Check("NAME.Arbitrary", parsed.IsArbitraryAddressCapable);
    }

    private static void ValidateAddressClaim(byte[] name)
    {
        var service = new J1939NetworkManagementService();
        J1939NetworkEventDto evt;
        bool ok = service.TryProcess(AddressClaimMessage(0x00, name), out evt);
        Check("AddressClaim.Decoded", ok);
        Check("AddressClaim.Event", evt.EventType == "AddressClaimed");
        Check("AddressClaim.RegistrySA", service.AddressRegistry.GetSnapshot()[0].SourceAddress == 0x00);
    }

    private static void ValidateCannotClaim(byte[] name)
    {
        var service = new J1939NetworkManagementService();
        J1939NetworkEventDto evt;
        bool ok = service.TryProcess(AddressClaimMessage(0xFE, name), out evt);
        Check("CannotClaim.Decoded", ok);
        Check("CannotClaim.Event", evt.EventType == "CannotClaimAddress");
    }

    private static void ValidateRequestAddressClaim()
    {
        CanFrameDto request = new J1939AddressClaimRequestService().BuildGlobalRequest();
        Check("RequestAddressClaim.Payload", request.Dlc == 3 && request.Data[0] == 0x00 && request.Data[1] == 0xEE && request.Data[2] == 0x00);
        Check("RequestAddressClaim.CanId", request.CanId == 0x18EAFFF9U);
    }

    private static void ValidateConflict(byte[] name)
    {
        var service = new J1939NetworkManagementService();
        J1939NetworkEventDto first;
        service.TryProcess(AddressClaimMessage(0x80, name), out first);
        byte[] lowerName = BuildName(0x12344, 0x456, 2, 3, 0x9A, 0, 0x22, 4, 5, true);
        J1939NetworkEventDto conflict;
        service.TryProcess(AddressClaimMessage(0x80, lowerName), out conflict);
        Check("Conflict.Event", conflict.EventType == "AddressConflictDetected");
        Check("Conflict.Flag", conflict.RegistryEntry.ConflictDetected);
        Check("Conflict.Winner", conflict.RegistryEntry.NameHex == new J1939NameParser().Parse(lowerName).NameHex);
    }

    private static void ValidateSameNameRefresh(byte[] name)
    {
        var service = new J1939NetworkManagementService();
        J1939NetworkEventDto first;
        service.TryProcess(AddressClaimMessage(0x81, name), out first);
        J1939NetworkEventDto refresh;
        service.TryProcess(AddressClaimMessage(0x81, name), out refresh);
        Check("Refresh.Event", refresh.EventType == "AddressClaimRefreshed");
        Check("Refresh.NoConflict", !refresh.RegistryEntry.ConflictDetected);
    }

    private static void ValidateWorkingSet(byte[] name)
    {
        var service = new J1939NetworkManagementService();
        J1939NetworkEventDto masterEvent;
        bool master = service.TryProcess(Message(65037, 0x90, new byte[] { 3, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, 8), out masterEvent);
        J1939NetworkEventDto memberEvent;
        bool member = service.TryProcess(Message(65036, 0x90, name, 8), out memberEvent);
        Check("WorkingSet.Master", master && masterEvent.WorkingSet.MemberCount == 3);
        Check("WorkingSet.Member", member && memberEvent.WorkingSetMember.Name.IdentityNumber == 0x12345);
    }

    private static void ValidateCommandedAddress(byte[] name)
    {
        byte[] payload = new J1939CommandedAddressService().BuildCommandedAddressPayload(name, 0xA5);
        Check("CommandedAddress.Length", payload.Length == 9);
        Check("CommandedAddress.Name", payload[0] == name[0] && payload[7] == name[7]);
        Check("CommandedAddress.NewSA", payload[8] == 0xA5);
    }

    private static void ValidateToolAddressConstant()
    {
        CanFrameDto dm1 = new J1939DiagnosticRequestService().BuildDm1Request();
        CanFrameDto addressClaim = new J1939AddressClaimRequestService().BuildGlobalRequest();
        Check("ToolSA.Constant", J1939ToolAddressConfig.DefaultToolSourceAddress == 0xF9);
        Check("ToolSA.DM1", (dm1.CanId & 0xFF) == J1939ToolAddressConfig.DefaultToolSourceAddress);
        Check("ToolSA.AddressClaim", (addressClaim.CanId & 0xFF) == J1939ToolAddressConfig.DefaultToolSourceAddress);
    }

    private static J1939DataLinkMessageDto AddressClaimMessage(byte sourceAddress, byte[] name)
    {
        return Message(60928, sourceAddress, name, 8);
    }

    private static J1939DataLinkMessageDto Message(uint pgn, byte sourceAddress, byte[] data, byte dlc)
    {
        return new J1939DataLinkMessageDto
        {
            Pgn = pgn,
            FormattedPgn = pgn.ToString("X6"),
            Dlc = dlc,
            Data = data,
            Timestamp = DateTime.Now,
            IdFields = new J1939IdFieldsDto
            {
                Pgn = pgn,
                FormattedPgn = pgn.ToString("X6"),
                SourceAddress = sourceAddress,
                DestinationAddress = 0xFF
            }
        };
    }

    private static byte[] BuildName(uint identity, ushort manufacturer, byte ecuInstance, byte functionInstance, byte function, byte reserved, byte vehicleSystem, byte vehicleSystemInstance, byte industryGroup, bool arbitrary)
    {
        ulong value = 0;
        value |= identity & 0x1FFFFFUL;
        value |= ((ulong)manufacturer & 0x7FFUL) << 21;
        value |= ((ulong)ecuInstance & 0x07UL) << 32;
        value |= ((ulong)functionInstance & 0x1FUL) << 35;
        value |= ((ulong)function & 0xFFUL) << 40;
        value |= ((ulong)reserved & 0x01UL) << 48;
        value |= ((ulong)vehicleSystem & 0x7FUL) << 49;
        value |= ((ulong)vehicleSystemInstance & 0x0FUL) << 56;
        value |= ((ulong)industryGroup & 0x07UL) << 60;
        if (arbitrary)
            value |= 1UL << 63;

        byte[] bytes = new byte[8];
        for (int i = 0; i < 8; ++i)
            bytes[i] = (byte)((value >> (8 * i)) & 0xFF);
        return bytes;
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
        harness = tmp_path / "J1939NetworkManagementHarness.cs"
        exe = tmp_path / "J1939NetworkManagementHarness.exe"
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
        "# Validacao J1939 Network Management J1939-81",
        "",
        f"- Resultado: `{'OK' if ok else 'FALHA'}`",
        "- Casos: NAME, Address Claimed, Cannot Claim, Request Address Claimed, conflito, refresh, Working Set, Commanded Address e Tool SA.",
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
