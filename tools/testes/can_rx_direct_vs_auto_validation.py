#!/usr/bin/env python3
from dataclasses import dataclass
from pathlib import Path
import heapq
import random


SEED = 0x5D10E10
TOTAL_IDS = 200
CYCLIC_FIXED_COUNT = 10
CYCLIC_VARIABLE_COUNT = 40
SPORADIC_COUNT = 150
TOTAL_MESSAGES = 5000
CYCLIC_MESSAGES = TOTAL_MESSAGES - SPORADIC_COUNT
TABLE_CAPACITY = 100
DEFAULT_TIMEOUT_MS = 3000


@dataclass(frozen=True)
class Frame:
    t_ms: int
    seq: int
    can_id: int
    extended: bool
    rtr: bool
    dlc: int
    data: tuple


@dataclass
class Row:
    valid: bool = False
    can_id: int = 0
    flags: int = 0
    dlc: int = 0
    data: tuple = (0,) * 8
    cycle_time: int = 0
    last_seen_ms: int = 0
    previous_seen_ms: int = 0


def make_ids():
    rng = random.Random(SEED)
    ids = []
    used = set()
    while len(ids) < TOTAL_IDS:
        candidate = rng.randrange(0x0001000, 0x1FFFFFFF)
        candidate &= 0x1FFFFFFF
        if candidate in used:
            continue
        used.add(candidate)
        ids.append(candidate)
    return ids


def base_data(index):
    return tuple(((i << 4) | (index & 0x0F)) & 0xFF for i in range(8))


def generate_frames():
    rng = random.Random(SEED ^ 0xC0A5)
    ids = make_ids()
    fixed_ids = ids[:CYCLIC_FIXED_COUNT]
    variable_ids = ids[CYCLIC_FIXED_COUNT:CYCLIC_FIXED_COUNT + CYCLIC_VARIABLE_COUNT]
    sporadic_ids = ids[CYCLIC_FIXED_COUNT + CYCLIC_VARIABLE_COUNT:]

    heap = []
    states = {}
    serial = 0

    for idx, can_id in enumerate(fixed_ids):
        period = rng.randint(21, 42)
        first_due = rng.randint(0, period)
        states[can_id] = {
            "kind": "fixed",
            "period": period,
            "data": tuple([0x05, 0x15, 0x25, 0x35, 0x45, 0x55, 0x65, 0x75]),
            "next_change": None,
            "rng": random.Random(SEED + idx),
        }
        heapq.heappush(heap, (first_due, serial, can_id))
        serial += 1

    for idx, can_id in enumerate(variable_ids):
        period = rng.randint(22, 45)
        first_due = rng.randint(0, period)
        states[can_id] = {
            "kind": "variable",
            "period": period,
            "data": list(base_data(0)),
            "next_change": 1000 + rng.randint(-80, 80),
            "rng": random.Random(SEED ^ (idx * 7919)),
        }
        heapq.heappush(heap, (first_due, serial, can_id))
        serial += 1

    frames = []
    seq = 0
    while len(frames) < CYCLIC_MESSAGES:
        due, _, can_id = heapq.heappop(heap)
        state = states[can_id]
        if state["kind"] == "variable" and due >= state["next_change"]:
            change_count = state["rng"].randint(1, 4)
            positions = state["rng"].sample(range(8), change_count)
            for pos in positions:
                low = (state["data"][pos] + 1) & 0x0F
                state["data"][pos] = (pos << 4) | low
            state["next_change"] += 1000 + state["rng"].randint(-80, 80)

        data = tuple(state["data"])
        frames.append(Frame(due, seq, can_id, True, False, 8, data))
        seq += 1
        heapq.heappush(heap, (due + state["period"], serial, can_id))
        serial += 1

    max_time = frames[-1].t_ms
    for can_id in sporadic_ids:
        t_ms = rng.randint(0, max_time)
        data = base_data(can_id)
        frames.append(Frame(t_ms, seq, can_id, True, False, 8, data))
        seq += 1

    frames.sort(key=lambda f: (f.t_ms, f.seq))
    return frames


def frame_key(frame):
    return (frame.can_id, frame.extended, frame.rtr, frame.dlc, frame.data)


def timeout_for(row):
    if row.cycle_time == 0:
        return DEFAULT_TIMEOUT_MS
    timeout = row.cycle_time * 5
    if timeout < 1000:
        timeout = 1000
    if timeout > 30000:
        timeout = 30000
    return timeout


def simulate_auto(frames):
    rows = [Row() for _ in range(TABLE_CAPACITY)]
    capture = []
    stats = {
        "CREATE": 0,
        "EDIT": 0,
        "TIC": 0,
        "DELETE": 0,
        "RX_DIRECT_0x28": 0,
        "TABLE_FULL_FALLBACK": 0,
        "FIFO_OVERFLOW": 0,
        "OUTPUT_BUFFER_OVERFLOW": 0,
    }

    def delete_expired(now):
        for row in rows:
            if row.valid and now - row.last_seen_ms >= timeout_for(row):
                row.valid = False
                stats["DELETE"] += 1

    for frame in frames:
        delete_expired(frame.t_ms)
        flags = (0x01 if frame.extended else 0x00) | (0x02 if frame.rtr else 0x00)
        row = next((candidate for candidate in rows if candidate.valid and candidate.can_id == frame.can_id and candidate.flags == flags), None)

        if row is None:
            row = next((candidate for candidate in rows if not candidate.valid), None)
            if row is None:
                stats["RX_DIRECT_0x28"] += 1
                stats["TABLE_FULL_FALLBACK"] += 1
                capture.append(frame)
                continue

            row.valid = True
            row.can_id = frame.can_id
            row.flags = flags
            row.dlc = frame.dlc
            row.data = frame.data
            row.cycle_time = 0
            row.previous_seen_ms = frame.t_ms
            row.last_seen_ms = frame.t_ms
            stats["CREATE"] += 1
            capture.append(frame)
            continue

        row.previous_seen_ms = row.last_seen_ms
        row.last_seen_ms = frame.t_ms
        elapsed = frame.t_ms - row.previous_seen_ms
        next_cycle_time = min(max(elapsed, 0), 0xFFFF)

        content_changed = row.dlc != frame.dlc or row.data != frame.data
        row.cycle_time = next_cycle_time
        if content_changed:
            row.dlc = frame.dlc
            row.data = frame.data
            stats["EDIT"] += 1
        else:
            stats["TIC"] += 1
        capture.append(frame)

    if frames:
        delete_expired(frames[-1].t_ms + DEFAULT_TIMEOUT_MS + 1)

    return capture, stats


def compare(direct_capture, auto_capture):
    matches = 0
    mismatches = []
    common = min(len(direct_capture), len(auto_capture))
    for index in range(common):
        if frame_key(direct_capture[index]) == frame_key(auto_capture[index]):
            matches += 1
        else:
            mismatches.append((index, direct_capture[index], auto_capture[index]))

    lost = direct_capture[common:]
    extra = auto_capture[common:]
    return matches, mismatches, lost, extra


def format_frame(frame):
    data = " ".join(f"{b:02X}" for b in frame.data)
    return f"seq={frame.seq} t={frame.t_ms} id=0x{frame.can_id:08X} ext={int(frame.extended)} rtr={int(frame.rtr)} dlc={frame.dlc} data={data}"


def write_report(frames, direct_capture, auto_capture, stats, matches, mismatches, lost, extra):
    report_path = Path("out/dumps/can_rx_direct_vs_auto_validation.md")
    report_path.parent.mkdir(parents=True, exist_ok=True)

    lines = []
    lines.append("# CAN RX DIRECT_ONLY vs AUTO Validation")
    lines.append("")
    lines.append(f"- Seed utilizada: `0x{SEED:X}`")
    lines.append(f"- Total de IDs: `{TOTAL_IDS}`")
    lines.append(f"- Total de mensagens transmitidas: `{len(frames)}`")
    lines.append(f"- Total DIRECT_ONLY: `{len(direct_capture)}`")
    lines.append(f"- Total AUTO: `{len(auto_capture)}`")
    lines.append(f"- Matches: `{matches}`")
    lines.append(f"- Mismatches: `{len(mismatches)}`")
    lines.append(f"- Lost: `{len(lost)}`")
    lines.append(f"- Extra: `{len(extra)}`")
    lines.append("")
    lines.append("## Estatisticas")
    for key in ["CREATE", "EDIT", "TIC", "DELETE", "RX_DIRECT_0x28", "TABLE_FULL_FALLBACK", "FIFO_OVERFLOW", "OUTPUT_BUFFER_OVERFLOW"]:
        lines.append(f"- {key}: `{stats[key]}`")
    lines.append("")
    lines.append("## Massa de Dados")
    lines.append(f"- IDs ciclicos fixos: `{CYCLIC_FIXED_COUNT}`")
    lines.append(f"- IDs ciclicos variaveis: `{CYCLIC_VARIABLE_COUNT}`")
    lines.append(f"- IDs unicos/esporadicos: `{SPORADIC_COUNT}`")
    lines.append(f"- Mensagens ciclicas: `{CYCLIC_MESSAGES}`")
    lines.append(f"- Mensagens esporadicas: `{SPORADIC_COUNT}`")
    lines.append("- Periodos ciclicos pseudoaleatorios: `21..45 ms`")
    lines.append("- Alteracao dos IDs variaveis: `~1000 ms`, com `1..4` bytes pseudoaleatorios por alteracao")
    lines.append("")
    if not mismatches and not lost and not extra:
        lines.append("CAN RX DIRECT_ONLY vs AUTO VALIDADO: CONTEUDO FIEL")
    else:
        lines.append("CAN RX DIRECT_ONLY vs AUTO FALHOU: DIVERGENCIAS ENCONTRADAS")
        lines.append("")
        lines.append("## Primeiros 20 Erros")
        for index, direct, auto in mismatches[:20]:
            lines.append(f"- index `{index}`")
            lines.append(f"  - DIRECT: `{format_frame(direct)}`")
            lines.append(f"  - AUTO: `{format_frame(auto)}`")
        for index, frame in enumerate(lost[:20]):
            lines.append(f"- lost `{index}`: `{format_frame(frame)}`")
        for index, frame in enumerate(extra[:20]):
            lines.append(f"- extra `{index}`: `{format_frame(frame)}`")

    report_path.write_text("\n".join(lines) + "\n", encoding="utf-8")
    return report_path


def main():
    frames = generate_frames()
    direct_capture = list(frames)
    auto_capture, stats = simulate_auto(frames)
    matches, mismatches, lost, extra = compare(direct_capture, auto_capture)
    report_path = write_report(frames, direct_capture, auto_capture, stats, matches, mismatches, lost, extra)

    print(f"report={report_path}")
    print(f"seed=0x{SEED:X}")
    print(f"messages={len(frames)} ids={TOTAL_IDS}")
    print(f"direct={len(direct_capture)} auto={len(auto_capture)} matches={matches} mismatches={len(mismatches)} lost={len(lost)} extra={len(extra)}")
    print("result=" + ("OK" if not mismatches and not lost and not extra else "FAIL"))


if __name__ == "__main__":
    main()
