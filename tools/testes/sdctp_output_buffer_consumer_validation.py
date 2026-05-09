#!/usr/bin/env python3
from pathlib import Path
import shutil
import subprocess
import tempfile
import textwrap


ROOT = Path(__file__).resolve().parents[2]
API_ASSEMBLY = ROOT / "local-api/src/SimulDIESEL/SimulDIESEL/bin/x86/Debug/SimulDIESEL.exe"
REPORT = ROOT / "out/dumps/sdctp_output_buffer_consumer_validation.md"


def find_csc():
    candidates = [
        Path(r"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\Roslyn\csc.exe"),
        Path(r"C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\Roslyn\csc.exe"),
        Path(r"C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\Roslyn\csc.exe"),
        Path(r"C:\Program Files\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\Roslyn\csc.exe"),
    ]
    for candidate in candidates:
        if candidate.exists():
            return candidate

    found = shutil.which("csc")
    if found:
        return Path(found)

    raise RuntimeError("csc.exe nao encontrado. Rode o build API VS 2022 antes da validacao.")


def write_harness(source_path):
    source_path.write_text(textwrap.dedent(
        r'''
        using System;
        using System.Collections.Generic;
        using System.Threading.Tasks;
        using SimulDIESEL.BLL.Boards.UCE;
        using SimulDIESEL.BLL.Services.CAN.SDCTP;
        using SimulDIESEL.DTL.Boards.UCE;
        using SimulDIESEL.DTL.Boards.UCE.Can;

        internal sealed class FakeUceDispatcher : IUceDispatcher
        {
            public event Action<UceLedEvent> LedEventReceived;
            public event Action<UceCanRxEvent> CanRxEventReceived;
            public event Action<byte, byte[]> CanCrudEventReceived;
            public event Action<UceDispatcherOverflowDiagnostic> DispatcherOverflowDiagnosticReceived;

            public void EmitRx(uint id, byte tag)
            {
                CanRxEventReceived?.Invoke(new UceCanRxEvent
                {
                    Controller = UceCanController.Can0,
                    Frames = new[]
                    {
                        new UceCanFrame
                        {
                            Id = id,
                            Extended = true,
                            RemoteRequest = false,
                            Dlc = 8,
                            Data = new byte[] { tag, 1, 2, 3, 4, 5, 6, 7 }
                        }
                    }
                });
            }

            public Task<UceCommandResult> SetBuiltinLedAsync(bool on) { return Task.FromResult(UceCommandResult.Fail("not used")); }
            public Task<UceOperationResult<UceCanConfigResponse>> SetCanConfigAsync(string controller, int bitrateKbps, string mode) { return Fail<UceCanConfigResponse>(); }
            public Task<UceOperationResult<UceCanConfigResponse>> SetCanConfigAsync(string controller, int bitrateKbps, string mode, UceCanRxMode rxMode) { return Fail<UceCanConfigResponse>(); }
            public Task<UceOperationResult<UceCanEnableResponse>> SetCanEnabledAsync(string controller, bool enabled) { return Fail<UceCanEnableResponse>(); }
            public Task<UceOperationResult<UceCanStatusResponse>> GetCanStatusAsync(string controller) { return Fail<UceCanStatusResponse>(); }
            public Task<UceOperationResult<UceCanResetResponse>> ResetCanAsync(string controller) { return Fail<UceCanResetResponse>(); }
            public Task<UceOperationResult<UceCanRxPollResponse>> PollCanRxAsync(string controller) { return Fail<UceCanRxPollResponse>(); }
            public Task<UceOperationResult<UceCanReadAllResponse>> RequestCanReadAllAsync(string controller) { return Fail<UceCanReadAllResponse>(); }
            public Task<UceOperationResult<UceCanDriverLogPollResponse>> PollCanDriverLogAsync(string controller) { return Fail<UceCanDriverLogPollResponse>(); }
            public Task<UceOperationResult<UceCanTxResponse>> SendCanAsync(string controller, bool extended, uint id, byte dlc, byte[] data, ushort periodMs) { return Fail<UceCanTxResponse>(); }
            public Task<UceOperationResult<UceCanTxResponse>> SendCanDirectAsync(string controller, bool extended, bool rtr, uint id, byte dlc, byte[] data) { return Fail<UceCanTxResponse>(); }
            public Task<UceOperationResult<UceCanTxResponse>> CreateCanTxRowAsync(string controller, byte index, bool extended, bool rtr, uint id, byte dlc, byte[] data, ushort periodMs, bool enabled) { return Fail<UceCanTxResponse>(); }
            public Task<UceOperationResult<UceCanTxResponse>> EditCanTxRowAsync(string controller, byte index, byte mask, byte flags, uint id, byte dlc, byte dataMask, byte[] data, ushort periodMs, bool enabled) { return Fail<UceCanTxResponse>(); }
            public Task<UceOperationResult<UceCanTxResponse>> DeleteCanTxRowAsync(string controller, byte index, byte reason) { return Fail<UceCanTxResponse>(); }
            public Task<UceOperationResult<UceCanTxStopResponse>> StopCanTxAsync(string controller) { return Fail<UceCanTxStopResponse>(); }

            private static Task<UceOperationResult<T>> Fail<T>() where T : class
            {
                return Task.FromResult(UceOperationResult<T>.Fail("not used"));
            }
        }

        internal static class Program
        {
            private const int TotalFrames = 16;

            private static int Main()
            {
                var dispatcher = new FakeUceDispatcher();
                using (var sdctp = new SdctpApiService(dispatcher))
                {
                    int availableEvents = 0;
                    sdctp.CanRxFrameAvailable += (sender, args) => availableEvents++;

                    for (int index = 0; index < TotalFrames; ++index)
                        dispatcher.EmitRx((uint)(0x18FF0000 + index), (byte)index);

                    int read = 0;
                    int matches = 0;
                    int mismatches = 0;
                    CanFrameDto frame;
                    while (sdctp.TryReadRxFrame(out frame))
                    {
                        if (frame != null &&
                            frame.IsExtended &&
                            !frame.IsRemoteRequest &&
                            frame.Dlc == 8 &&
                            frame.CanId == 0x18FF0000 + read &&
                            frame.Data != null &&
                            frame.Data.Length == 8 &&
                            frame.Data[0] == read &&
                            frame.Source == CanFrameSource.Direct)
                        {
                            matches++;
                        }
                        else
                        {
                            mismatches++;
                        }
                        read++;
                    }

                    Console.WriteLine("frames_enfileirados=" + TotalFrames.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    Console.WriteLine("eventos_disponiveis=" + availableEvents.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    Console.WriteLine("frames_lidos=" + read.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    Console.WriteLine("matches=" + matches.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    Console.WriteLine("mismatches=" + mismatches.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    Console.WriteLine("output_buffer_restante=" + sdctp.OutputBufferCount.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    Console.WriteLine("mirror_acessado=false");
                    Console.WriteLine("tlv_interpretado=false");
                    Console.WriteLine("result=" + (read == TotalFrames && matches == TotalFrames && mismatches == 0 ? "OK" : "FAIL"));
                    return read == TotalFrames && matches == TotalFrames && mismatches == 0 ? 0 : 2;
                }
            }
        }
        '''), encoding="utf-8")


def parse_output(output):
    values = {}
    for line in output.splitlines():
        if "=" not in line:
            continue
        key, value = line.split("=", 1)
        values[key.strip()] = value.strip()
    return values


def write_report(values, stdout):
    REPORT.parent.mkdir(parents=True, exist_ok=True)
    ok = values.get("result") == "OK"
    lines = [
        "# SDCTP OutputBuffer Consumer Validation",
        "",
        "## Metodo usado",
        "",
        "- Compilado harness C# temporario contra `SimulDIESEL.exe` x86 Debug.",
        "- Instanciado `SdctpApiService` com `IUceDispatcher` fake.",
        "- Emitidos eventos RX ja parseados pelo dispatcher fake.",
        "- Consumidor leu exclusivamente por `SdctpApiService.TryReadRxFrame(out frame)`.",
        "- O harness nao acessa `CanRxMirrorManager` e nao interpreta TLV.",
        "",
        "## Resultado",
        "",
        f"- Total frames enfileirados: `{values.get('frames_enfileirados', '-')}`",
        f"- Eventos CanRxFrameAvailable: `{values.get('eventos_disponiveis', '-')}`",
        f"- Total frames lidos por TryReadRxFrame: `{values.get('frames_lidos', '-')}`",
        f"- Matches: `{values.get('matches', '-')}`",
        f"- Mismatches: `{values.get('mismatches', '-')}`",
        f"- OutputBuffer restante: `{values.get('output_buffer_restante', '-')}`",
        f"- Acesso direto ao mirror: `{values.get('mirror_acessado', '-')}`",
        f"- Interpretacao de TLV pelo consumidor: `{values.get('tlv_interpretado', '-')}`",
        "",
        "CONSUMIDOR SDCTP VALIDADO: TryReadRxFrame e a saida oficial de RX" if ok else "CONSUMIDOR SDCTP FALHOU",
        "",
        "## Saida bruta",
        "",
        "```text",
        stdout.strip(),
        "```",
        "",
    ]
    REPORT.write_text("\n".join(lines), encoding="utf-8")


def main():
    if not API_ASSEMBLY.exists():
        raise SystemExit(f"API assembly nao encontrado: {API_ASSEMBLY}")

    csc = find_csc()
    with tempfile.TemporaryDirectory(prefix="sdctp-consumer-") as tmp:
        tmp_path = Path(tmp)
        source = tmp_path / "SdctpOutputBufferConsumerHarness.cs"
        exe = tmp_path / "SdctpOutputBufferConsumerHarness.exe"
        write_harness(source)
        subprocess.run([
            str(csc),
            "/nologo",
            "/langversion:7.3",
            "/platform:x86",
            "/warn:4",
            "/nowarn:0067",
            "/out:" + str(exe),
            "/reference:" + str(API_ASSEMBLY),
            str(source),
        ], check=True, cwd=str(ROOT))
        shutil.copy2(API_ASSEMBLY, tmp_path / API_ASSEMBLY.name)
        completed = subprocess.run([str(exe)], check=False, capture_output=True, text=True, cwd=str(tmp_path))

    stdout = completed.stdout + completed.stderr
    values = parse_output(stdout)
    write_report(values, stdout)
    print(f"report={REPORT.relative_to(ROOT)}")
    print(stdout.strip())
    raise SystemExit(completed.returncode)


if __name__ == "__main__":
    main()
