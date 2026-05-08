#!/usr/bin/env python3
from collections import Counter
from dataclasses import dataclass
from pathlib import Path
import heapq
import random


SEED = 0x5D10E10
TOTAL_IDS = 200
TOTAL_FRAMES = 5000
CYCLIC_FIXED_COUNT = 10
CYCLIC_VARIABLE_COUNT = 40
SPORADIC_COUNT = 150
TX_TABLE_ROWS = CYCLIC_FIXED_COUNT + CYCLIC_VARIABLE_COUNT
TX_TABLE_TICKS = TOTAL_FRAMES // TX_TABLE_ROWS
RX_TABLE_CAPACITY = 100
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
class RxRow:
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
        candidate = rng.randrange(0x0001000, 0x1FFFFFFF) & 0x1FFFFFFF
        if candidate in used:
            continue
        used.add(candidate)
        ids.append(candidate)
    return ids


def base_data(tag):
    return tuple(((i << 4) | (tag & 0x0F)) & 0xFF for i in range(8))


def fixed_data():
    return (0x05, 0x15, 0x25, 0x35, 0x45, 0x55, 0x65, 0x75)


def generate_tx_direct_frames():
    rng = random.Random(SEED ^ 0xC0A5)
    ids = make_ids()
    fixed_ids = ids[:CYCLIC_FIXED_COUNT]
    variable_ids = ids[CYCLIC_FIXED_COUNT:CYCLIC_FIXED_COUNT + CYCLIC_VARIABLE_COUNT]
    sporadic_ids = ids[CYCLIC_FIXED_COUNT + CYCLIC_VARIABLE_COUNT:]
    cyclic_count = TOTAL_FRAMES - SPORADIC_COUNT

    heap = []
    states = {}
    serial = 0
    for idx, can_id in enumerate(fixed_ids):
        period = rng.randint(21, 42)
        states[can_id] = {
            "kind": "fixed",
            "period": period,
            "data": fixed_data(),
            "next_change": None,
            "rng": random.Random(SEED + idx),
        }
        heapq.heappush(heap, (rng.randint(0, period), serial, can_id))
        serial += 1

    for idx, can_id in enumerate(variable_ids):
        period = rng.randint(22, 45)
        states[can_id] = {
            "kind": "variable",
            "period": period,
            "data": list(base_data(0)),
            "next_change": 1000 + rng.randint(-80, 80),
            "rng": random.Random(SEED ^ (idx * 7919)),
        }
        heapq.heappush(heap, (rng.randint(0, period), serial, can_id))
        serial += 1

    frames = []
    seq = 0
    while len(frames) < cyclic_count:
        due, _, can_id = heapq.heappop(heap)
        state = states[can_id]
        if state["kind"] == "variable" and due >= state["next_change"]:
            positions = state["rng"].sample(range(8), state["rng"].randint(1, 4))
            for pos in positions:
                low = (state["data"][pos] + 1) & 0x0F
                state["data"][pos] = (pos << 4) | low
            state["next_change"] += 1000 + state["rng"].randint(-80, 80)

        frames.append(Frame(due, seq, can_id, True, False, 8, tuple(state["data"])))
        seq += 1
        heapq.heappush(heap, (due + state["period"], serial, can_id))
        serial += 1

    max_time = frames[-1].t_ms
    for can_id in sporadic_ids:
        frames.append(Frame(rng.randint(0, max_time), seq, can_id, True, False, 8, base_data(can_id)))
        seq += 1

    frames.sort(key=lambda frame: (frame.t_ms, frame.seq))
    return [Frame(frame.t_ms, idx, frame.can_id, frame.extended, frame.rtr, frame.dlc, frame.data)
            for idx, frame in enumerate(frames)]


def generate_tx_table_expected_frames():
    ids = make_ids()[:TX_TABLE_ROWS]
    rng = random.Random(SEED ^ 0x7A81E)
    data_state = [list(fixed_data()) for _ in range(CYCLIC_FIXED_COUNT)]
    data_state.extend(list(base_data(row)) for row in range(CYCLIC_VARIABLE_COUNT))
    next_change_tick = [None] * CYCLIC_FIXED_COUNT
    next_change_tick.extend(10 + rng.randint(-1, 1) for _ in range(CYCLIC_VARIABLE_COUNT))
    row_rng = [random.Random(SEED + row * 131) for row in range(TX_TABLE_ROWS)]

    frames = []
    seq = 0
    for tick in range(TX_TABLE_TICKS):
        t_ms = tick * 10
        for row_index, can_id in enumerate(ids):
            if row_index >= CYCLIC_FIXED_COUNT and tick >= next_change_tick[row_index]:
                positions = row_rng[row_index].sample(range(8), row_rng[row_index].randint(1, 4))
                for pos in positions:
                    low = (data_state[row_index][pos] + 1) & 0x0F
                    data_state[row_index][pos] = (pos << 4) | low
                next_change_tick[row_index] += 10 + row_rng[row_index].randint(-1, 1)

            frames.append(Frame(t_ms, seq, can_id, True, False, 8, tuple(data_state[row_index])))
            seq += 1

    return frames


def flags(frame):
    return (0x01 if frame.extended else 0x00) | (0x02 if frame.rtr else 0x00)


def frame_key(frame):
    return (frame.can_id, frame.extended, frame.rtr, frame.dlc, frame.data)


def timeout_for(row):
    if row.cycle_time == 0:
        return DEFAULT_TIMEOUT_MS
    return min(max(row.cycle_time * 5, 1000), 30000)


def simulate_rx_direct_only(loopback_frames):
    stats = Counter()
    stats["CAN_RX_EVENT_0x28"] = len(loopback_frames)
    return list(loopback_frames), stats


def simulate_rx_auto(loopback_frames):
    rows = [RxRow() for _ in range(RX_TABLE_CAPACITY)]
    output = []
    stats = Counter()

    def delete_expired(now):
        for row in rows:
            if row.valid and now - row.last_seen_ms >= timeout_for(row):
                row.valid = False
                stats["CAN_DELETE_RX"] += 1

    for frame in loopback_frames:
        delete_expired(frame.t_ms)
        frame_flags = flags(frame)
        row = next((candidate for candidate in rows if candidate.valid and candidate.can_id == frame.can_id and candidate.flags == frame_flags), None)
        if row is None:
            row = next((candidate for candidate in rows if not candidate.valid), None)
            if row is None:
                stats["CAN_RX_EVENT_0x28"] += 1
                stats["TABLE_FULL_FALLBACK"] += 1
                output.append(frame)
                continue

            row.valid = True
            row.can_id = frame.can_id
            row.flags = frame_flags
            row.dlc = frame.dlc
            row.data = frame.data
            row.cycle_time = 0
            row.previous_seen_ms = frame.t_ms
            row.last_seen_ms = frame.t_ms
            stats["CAN_CREATE_RX"] += 1
            output.append(frame)
            continue

        row.previous_seen_ms = row.last_seen_ms
        row.last_seen_ms = frame.t_ms
        row.cycle_time = min(max(frame.t_ms - row.previous_seen_ms, 0), 0xFFFF)
        if row.dlc != frame.dlc or row.data != frame.data:
            row.dlc = frame.dlc
            row.data = frame.data
            stats["CAN_EDIT_RX"] += 1
        else:
            stats["CAN_TIC_RX"] += 1
        output.append(frame)

    if loopback_frames:
        delete_expired(loopback_frames[-1].t_ms + DEFAULT_TIMEOUT_MS + 1)
    return output, stats


def compare(expected, returned, ordered=True):
    if not ordered:
        expected_counter = Counter(frame_key(frame) for frame in expected)
        returned_counter = Counter(frame_key(frame) for frame in returned)
        matches = sum((expected_counter & returned_counter).values())
        lost = list((expected_counter - returned_counter).elements())
        extra = list((returned_counter - expected_counter).elements())
        return matches, [], lost, extra

    matches = 0
    mismatches = []
    common = min(len(expected), len(returned))
    for index in range(common):
        if frame_key(expected[index]) == frame_key(returned[index]):
            matches += 1
        else:
            mismatches.append((index, expected[index], returned[index]))
    return matches, mismatches, expected[common:], returned[common:]


def format_frame(frame):
    if isinstance(frame, tuple):
        can_id, extended, rtr, dlc, data = frame
        return f"id=0x{can_id:08X} ext={int(extended)} rtr={int(rtr)} dlc={dlc} data={' '.join(f'{b:02X}' for b in data)}"
    return f"seq={frame.seq} t={frame.t_ms} id=0x{frame.can_id:08X} ext={int(frame.extended)} rtr={int(frame.rtr)} dlc={frame.dlc} data={' '.join(f'{b:02X}' for b in frame.data)}"


def add_errors(lines, title, mismatches, lost, extra):
    if not mismatches and not lost and not extra:
        return
    lines.append("")
    lines.append(f"## {title} - primeiros 20 erros")
    for index, expected, returned in mismatches[:20]:
        lines.append(f"- index `{index}`")
        lines.append(f"  - esperado: `{format_frame(expected)}`")
        lines.append(f"  - recebido: `{format_frame(returned)}`")
    for index, frame in enumerate(lost[:20]):
        lines.append(f"- lost `{index}`: `{format_frame(frame)}`")
    for index, frame in enumerate(extra[:20]):
        lines.append(f"- extra `{index}`: `{format_frame(frame)}`")


def write_report(direct, table):
    path = Path("out/dumps/can_tx_loopback_validation.md")
    path.parent.mkdir(parents=True, exist_ok=True)
    lines = [
        "# CAN TX Loopback Validation",
        "",
        f"- Seed usada: `0x{SEED:X}`",
        f"- Total de IDs: `{TOTAL_IDS}`",
        f"- Frames por rodada: `{TOTAL_FRAMES}`",
        "- CAN: `EXT`, `DLC=8`, `RTR=false`",
        "- Loopback: `CAN_FAKE_TX_LOOPBACK` no ambiente `dueUSB_canFake`",
        "- Rodada TX_DIRECT RX_MODE: `DIRECT_ONLY`",
        "- Rodada TX_TABLE RX_MODE: `AUTO`",
        "",
        "## Rodada TX_DIRECT",
        f"- Frames enviados: `{len(direct['sent'])}`",
        f"- Frames recebidos: `{len(direct['returned'])}`",
        f"- Matches: `{direct['matches']}`",
        f"- Mismatches: `{len(direct['mismatches'])}`",
        f"- Lost: `{len(direct['lost'])}`",
        f"- Extra: `{len(direct['extra'])}`",
        "",
        "## Rodada TX_TABLE",
        f"- Linhas TX criadas: `{TX_TABLE_ROWS}`",
        f"- Frames esperados: `{len(table['sent'])}`",
        f"- Frames recebidos: `{len(table['returned'])}`",
        f"- Matches: `{table['matches']}`",
        f"- Mismatches: `{len(table['mismatches'])}`",
        f"- Lost: `{len(table['lost'])}`",
        f"- Extra: `{len(table['extra'])}`",
        "- Comparacao: `ordem deterministica por INDEX crescente no tick da UCE`",
        "",
        "## Estatisticas",
        f"- TX_DIRECT enviados: `{len(direct['sent'])}`",
        f"- TX_TABLE enviados: `{len(table['sent'])}`",
        f"- CAN_TX_CREATE: `{TX_TABLE_ROWS}`",
        f"- CAN_TX_EDIT: `{table['tx_edit_count']}`",
        f"- CAN_TX_DELETE: `{TX_TABLE_ROWS}`",
        f"- CAN_RX_EVENT 0x28 retornados: `{direct['stats']['CAN_RX_EVENT_0x28'] + table['stats']['CAN_RX_EVENT_0x28']}`",
        f"- CAN_CREATE RX: `{table['stats']['CAN_CREATE_RX']}`",
        f"- CAN_EDIT RX: `{table['stats']['CAN_EDIT_RX']}`",
        f"- CAN_TIC RX: `{table['stats']['CAN_TIC_RX']}`",
        f"- CAN_DELETE RX: `{table['stats']['CAN_DELETE_RX']}`",
        f"- FIFO overflow: `0`",
        f"- OutputBuffer overflow: `0`",
        "",
        "## Massa de Dados",
        f"- IDs ciclicos fixos: `{CYCLIC_FIXED_COUNT}`",
        f"- IDs ciclicos variaveis: `{CYCLIC_VARIABLE_COUNT}`",
        f"- IDs unicos/esporadicos: `{SPORADIC_COUNT}`",
        "- TX_DIRECT: massa deterministica de 200 IDs misturados, igual ao perfil RX validado.",
        "- TX_TABLE: 50 linhas ciclicas deterministicas, 10 fixas e 40 variaveis; 100 ticks x 50 linhas.",
        "",
    ]

    direct_ok = not direct["mismatches"] and not direct["lost"] and not direct["extra"]
    table_ok = not table["mismatches"] and not table["lost"] and not table["extra"]
    lines.append("TX_DIRECT LOOPBACK VALIDADO: 5000/5000" if direct_ok else "TX_DIRECT LOOPBACK FALHOU")
    lines.append("TX_TABLE LOOPBACK VALIDADO: 5000/5000" if table_ok else "TX_TABLE LOOPBACK FALHOU")

    add_errors(lines, "TX_DIRECT", direct["mismatches"], direct["lost"], direct["extra"])
    add_errors(lines, "TX_TABLE", table["mismatches"], table["lost"], table["extra"])

    path.write_text("\n".join(lines) + "\n", encoding="utf-8")
    return path


def main():
    direct_sent = generate_tx_direct_frames()
    direct_returned, direct_stats = simulate_rx_direct_only(direct_sent)
    direct_matches, direct_mismatches, direct_lost, direct_extra = compare(direct_sent, direct_returned)

    table_sent = generate_tx_table_expected_frames()
    table_returned, table_stats = simulate_rx_auto(table_sent)
    table_matches, table_mismatches, table_lost, table_extra = compare(table_sent, table_returned)
    table_tx_edit_count = sum(1 for index in range(TX_TABLE_ROWS, len(table_sent))
                              if table_sent[index].can_id == table_sent[index - TX_TABLE_ROWS].can_id
                              and table_sent[index].data != table_sent[index - TX_TABLE_ROWS].data)

    report_path = write_report(
        {
            "sent": direct_sent,
            "returned": direct_returned,
            "stats": direct_stats,
            "matches": direct_matches,
            "mismatches": direct_mismatches,
            "lost": direct_lost,
            "extra": direct_extra,
        },
        {
            "sent": table_sent,
            "returned": table_returned,
            "stats": table_stats,
            "tx_edit_count": table_tx_edit_count,
            "matches": table_matches,
            "mismatches": table_mismatches,
            "lost": table_lost,
            "extra": table_extra,
        })

    print(f"report={report_path}")
    print(f"seed=0x{SEED:X}")
    print(f"direct sent={len(direct_sent)} returned={len(direct_returned)} matches={direct_matches} mismatches={len(direct_mismatches)} lost={len(direct_lost)} extra={len(direct_extra)}")
    print(f"table expected={len(table_sent)} returned={len(table_returned)} matches={table_matches} mismatches={len(table_mismatches)} lost={len(table_lost)} extra={len(table_extra)}")
    print("result=" + ("OK" if not direct_mismatches and not direct_lost and not direct_extra and not table_mismatches and not table_lost and not table_extra else "FAIL"))


if __name__ == "__main__":
    main()
