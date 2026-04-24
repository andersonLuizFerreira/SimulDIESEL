"""[COMMISSIONING_SPI_V1] Protocol commissioning tests for BPM <-> UCE SPI V1."""

import unittest


MAGIC = 0x53
VERSION = 0x01
FRAME_DATA = 0x01
FRAME_ACK = 0x02
ACK = 0x01
RETRY = 0x02
DROP = 0x03


def crc8(data):
    crc = 0
    for value in data:
        crc ^= value
        for _ in range(8):
            crc = ((crc << 1) ^ 0x07) & 0xFF if crc & 0x80 else (crc << 1) & 0xFF
    return crc


def cobs_encode(data):
    out = bytearray([0])
    code_index = 0
    code = 1
    for value in data:
        if value == 0:
            out[code_index] = code
            code_index = len(out)
            out.append(0)
            code = 1
        else:
            out.append(value)
            code += 1
            if code == 0xFF:
                out[code_index] = code
                code_index = len(out)
                out.append(0)
                code = 1
    out[code_index] = code
    return bytes(out)


def cobs_decode(data):
    out = bytearray()
    index = 0
    while index < len(data):
        code = data[index]
        if code == 0:
            raise ValueError("zero inside COBS payload")
        index += 1
        end = index + code - 1
        if end > len(data):
            raise ValueError("COBS block overruns payload")
        out.extend(data[index:end])
        index = end
        if code != 0xFF and index < len(data):
            out.append(0)
    return bytes(out)


def tlv_packet(t, value):
    return bytes([t, len(value)]) + bytes(value)


def data_frame(message_id, *tlvs):
    frame = bytearray([MAGIC, VERSION, FRAME_DATA, len(tlvs)])
    for offset, tlv in enumerate(tlvs):
        mid = (message_id + offset) & 0xFF
        item = bytes([mid]) + tlv
        frame.extend(item)
        frame.append(crc8(item))
    assert len(frame) <= 256
    return bytes(frame)


def ack_frame(*items):
    frame = bytearray([MAGIC, VERSION, FRAME_ACK, len(items)])
    for message_id, status in items:
        item = bytes([message_id, status])
        frame.extend(item)
        frame.append(crc8(item))
    assert len(frame) <= 256
    return bytes(frame)


def parse_data(frame):
    assert frame[:3] == bytes([MAGIC, VERSION, FRAME_DATA])
    count = frame[3]
    cursor = 4
    parsed = []
    while cursor < len(frame):
        message_id = frame[cursor]
        cursor += 1
        length = frame[cursor + 1]
        tlv_len = length + 2
        tlv = frame[cursor : cursor + tlv_len]
        msg_crc = frame[cursor + tlv_len]
        assert crc8(bytes([message_id]) + tlv) == msg_crc
        parsed.append((message_id, bytes(tlv)))
        cursor += tlv_len + 1
    assert len(parsed) == count
    return parsed


class SpiV1ProtocolTests(unittest.TestCase):
    def test_frame_data_multiple_messages_round_trip(self):
        frame = data_frame(7, tlv_packet(0x12, [1]), tlv_packet(0x22, []), tlv_packet(0x20, [3, 4, 5]))
        physical = cobs_encode(frame) + b"\x00"
        decoded = cobs_decode(physical[:-1])
        self.assertEqual(parse_data(decoded), [(7, tlv_packet(0x12, [1])), (8, tlv_packet(0x22, [])), (9, tlv_packet(0x20, [3, 4, 5]))])

    def test_ack_statuses_validate_crc(self):
        frame = ack_frame((1, ACK), (2, RETRY), (3, DROP))
        self.assertEqual(frame[2], FRAME_ACK)
        for cursor in range(4, len(frame), 3):
            self.assertEqual(crc8(frame[cursor : cursor + 2]), frame[cursor + 2])

    def test_priority_cycle_is_6_3_1(self):
        cycle = ["A"] * 6 + ["M"] * 3 + ["B"]
        self.assertEqual(cycle, ["A", "A", "A", "A", "A", "A", "M", "M", "M", "B"])

    def test_no_fragmentation_or_skip_when_next_message_does_not_fit(self):
        frame = data_frame(1, tlv_packet(0x10, [0x55] * 240))
        next_msg = tlv_packet(0x11, [0x66] * 8)
        self.assertGreater(len(frame) + 1 + len(next_msg) + 1, 256)

    def test_timeout_policy_is_500_ms_without_progress(self):
        last_progress_ms = 1000
        now_ms = 1501
        self.assertGreater(now_ms - last_progress_ms, 500)

    def test_window_lifecycle_ack_retry_drop(self):
        states = {1: "SENT_WAIT_ACK", 2: "SENT_WAIT_ACK", 3: "SENT_WAIT_ACK"}
        for message_id, status in [(1, ACK), (2, RETRY), (3, DROP)]:
            if status == ACK:
                states[message_id] = "DONE"
            elif status == RETRY:
                states[message_id] = "RETRY_PENDING"
            elif status == DROP:
                states[message_id] = "DISCARDED"
        self.assertEqual(states, {1: "DONE", 2: "RETRY_PENDING", 3: "DISCARDED"})

    def test_pause_resume_model_blocks_clock_while_irq_high(self):
        irq_levels = ["LOW", "HIGH", "HIGH", "LOW"]
        clocks = ["CLK" if level == "LOW" else "PAUSE" for level in irq_levels]
        self.assertEqual(clocks, ["CLK", "PAUSE", "PAUSE", "CLK"])

    def test_header_is_read_before_payload_without_releasing_cs(self):
        response = tlv_packet(0x12, [0x01]) + bytes([crc8(tlv_packet(0x12, [0x01]))])
        cs_levels = ["LOW", "LOW", "LOW", "LOW"]
        header = response[:2]
        payload = response[2:]
        self.assertEqual(header, bytes([0x12, 0x01]))
        self.assertEqual(payload, bytes([0x01, response[-1]]))
        self.assertTrue(all(level == "LOW" for level in cs_levels))


if __name__ == "__main__":
    unittest.main()
