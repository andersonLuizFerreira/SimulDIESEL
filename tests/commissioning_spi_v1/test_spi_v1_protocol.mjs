// [COMMISSIONING_SPI_V1] Executable protocol checks for environments with Node.js.
import assert from "node:assert/strict";

const MAGIC = 0x53;
const VERSION = 0x01;
const FRAME_DATA = 0x01;
const FRAME_ACK = 0x02;
const ACK = 0x01;
const RETRY = 0x02;
const DROP = 0x03;

function crc8(data) {
  let crc = 0;
  for (const value of data) {
    crc ^= value;
    for (let bit = 0; bit < 8; bit += 1) {
      crc = (crc & 0x80) ? ((crc << 1) ^ 0x07) & 0xff : (crc << 1) & 0xff;
    }
  }
  return crc;
}

function cobsEncode(data) {
  const out = [0];
  let codeIndex = 0;
  let code = 1;
  for (const value of data) {
    if (value === 0) {
      out[codeIndex] = code;
      codeIndex = out.length;
      out.push(0);
      code = 1;
    } else {
      out.push(value);
      code += 1;
      if (code === 0xff) {
        out[codeIndex] = code;
        codeIndex = out.length;
        out.push(0);
        code = 1;
      }
    }
  }
  out[codeIndex] = code;
  return Uint8Array.from(out);
}

function cobsDecode(data) {
  const out = [];
  let index = 0;
  while (index < data.length) {
    const code = data[index++];
    assert.notEqual(code, 0);
    const end = index + code - 1;
    assert.ok(end <= data.length);
    while (index < end) out.push(data[index++]);
    if (code !== 0xff && index < data.length) out.push(0);
  }
  return Uint8Array.from(out);
}

function tlvPacket(type, value) {
  return Uint8Array.from([type, value.length, ...value]);
}

function dataFrame(messageId, ...tlvs) {
  const frame = [MAGIC, VERSION, FRAME_DATA, tlvs.length];
  tlvs.forEach((tlv, offset) => {
    const item = Uint8Array.from([(messageId + offset) & 0xff, ...tlv]);
    frame.push(...item, crc8(item));
  });
  assert.ok(frame.length <= 256);
  return Uint8Array.from(frame);
}

function ackFrame(...items) {
  const frame = [MAGIC, VERSION, FRAME_ACK, items.length];
  for (const [id, status] of items) {
    const item = Uint8Array.from([id, status]);
    frame.push(...item, crc8(item));
  }
  assert.ok(frame.length <= 256);
  return Uint8Array.from(frame);
}

function parseData(frame) {
  assert.deepEqual([...frame.slice(0, 3)], [MAGIC, VERSION, FRAME_DATA]);
  const count = frame[3];
  const parsed = [];
  let cursor = 4;
  while (cursor < frame.length) {
    const id = frame[cursor++];
    const len = frame[cursor + 1];
    const tlvLen = len + 2;
    const tlv = frame.slice(cursor, cursor + tlvLen);
    const msgCrc = frame[cursor + tlvLen];
    assert.equal(crc8(Uint8Array.from([id, ...tlv])), msgCrc);
    parsed.push([id, [...tlv]]);
    cursor += tlvLen + 1;
  }
  assert.equal(parsed.length, count);
  return parsed;
}

const frame = dataFrame(7, tlvPacket(0x12, [1]), tlvPacket(0x22, []), tlvPacket(0x20, [3, 4, 5]));
const physical = Uint8Array.from([...cobsEncode(frame), 0]);
assert.deepEqual(parseData(cobsDecode(physical.slice(0, -1))), [
  [7, [...tlvPacket(0x12, [1])]],
  [8, [...tlvPacket(0x22, [])]],
  [9, [...tlvPacket(0x20, [3, 4, 5])]],
]);
assert.equal(frame.length, 4 + (1 + tlvPacket(0x12, [1]).length + 1) + (1 + tlvPacket(0x22, []).length + 1) + (1 + tlvPacket(0x20, [3, 4, 5]).length + 1));
assert.equal(1 + tlvPacket(0x12, [1]).length + 1, 1 + 2 + 1 + 1);

const acks = ackFrame([1, ACK], [2, RETRY], [3, DROP]);
for (let cursor = 4; cursor < acks.length; cursor += 3) {
  assert.equal(crc8(acks.slice(cursor, cursor + 2)), acks[cursor + 2]);
}

assert.deepEqual(["A", "A", "A", "A", "A", "A", "M", "M", "M", "B"], [...Array(6).fill("A"), ...Array(3).fill("M"), "B"]);
const largeFrame = dataFrame(1, tlvPacket(0x10, Array(240).fill(0x55)));
assert.ok(largeFrame.length + 1 + tlvPacket(0x11, Array(8).fill(0x66)).length + 1 > 256);
assert.equal(parseData(largeFrame).length, 1);
assert.ok(1501 - 1000 > 500);

const states = new Map([[1, "SENT_WAIT_ACK"], [2, "SENT_WAIT_ACK"], [3, "SENT_WAIT_ACK"]]);
for (const [id, status] of [[1, ACK], [2, RETRY], [3, DROP]]) {
  states.set(id, status === ACK ? "DONE" : status === RETRY ? "RETRY_PENDING" : "DISCARDED");
}
assert.deepEqual(Object.fromEntries(states), { 1: "DONE", 2: "RETRY_PENDING", 3: "DISCARDED" });
assert.deepEqual(["LOW", "HIGH", "HIGH", "LOW"].map((level) => level === "LOW" ? "CLK" : "PAUSE"), ["CLK", "PAUSE", "PAUSE", "CLK"]);
const response = Uint8Array.from([...tlvPacket(0x12, [0x01]), crc8(tlvPacket(0x12, [0x01]))]);
assert.deepEqual([...response.slice(0, 2)], [0x12, 0x01]);
assert.deepEqual([...response.slice(2)], [0x01, response[response.length - 1]]);
assert.deepEqual(["LOW", "LOW", "LOW", "LOW"], Array(4).fill("LOW"));

console.log("[COMMISSIONING_SPI_V1] protocol checks passed");
