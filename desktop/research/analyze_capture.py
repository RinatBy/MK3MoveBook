#!/usr/bin/env python3
"""Inspect a MK3 MoveBook read-only research capture."""

from __future__ import annotations

import argparse
import csv
import gzip
import struct
from array import array
from dataclasses import dataclass
from datetime import datetime, timedelta, timezone
from pathlib import Path


HEADER = struct.Struct("<8siiiq")
FRAME = struct.Struct("<qiiqi")
SEGMENT = struct.Struct("<ii")

KNOWN_OFFSETS = {
    0x0B732: "p1_input_current",
    0x0B733: "p2_input_current",
    0x0B734: "p1_p2_aux_input_current",
    0x0B736: "p1_input_previous",
    0x0B737: "p2_input_previous",
    0x0B738: "p1_p2_aux_input_previous",
    0x0C146: "p1_character",
    0x0C14C: "p1_health",
    0x0C14E: "p1_run",
    0x0C2BC: "p2_character",
    0x0C2C2: "p2_health",
    0x0C2C4: "p2_run",
    0x0C44C: "kombat_zone",
    0x0C424: "powerbars_enabled",
    0x0C426: "finisher_success_candidate_a",
    0x0C442: "finisher_success_candidate_b",
    0x0C55C: "no_throwing",
    0x0C560: "no_blocking",
    0x0C57C: "fast_uppercut",
    0x0C57E: "unlimited_run",
    0x0C4E6: "finisher_success_candidate_c",
}

P1_MAIN_INPUT_OFFSET = 0x0B732
P2_MAIN_INPUT_OFFSET = 0x0B733
AUX_INPUT_OFFSET = 0x0B734

MAIN_INPUT_BITS = (
    (0x01, "UP"),
    (0x02, "DOWN"),
    (0x04, "LEFT"),
    (0x08, "RIGHT"),
    (0x10, "HP"),
    (0x20, "BL"),
    (0x40, "HK"),
)

P1_AUX_INPUT_BITS = (
    (0x01, "LP"),
    (0x02, "LK"),
    (0x04, "RUN"),
)

P2_AUX_INPUT_BITS = (
    (0x10, "LP"),
    (0x20, "LK"),
    (0x40, "RUN"),
)


@dataclass
class CaptureHeader:
    ram_size: int
    interval_ms: int
    started_ticks: int


def read_exact(stream, size: int) -> bytes:
    data = stream.read(size)
    if len(data) != size:
        raise EOFError(f"Expected {size} bytes, got {len(data)}")
    return data


def dotnet_ticks_to_iso(ticks: int) -> str:
    origin = datetime(1, 1, 1, tzinfo=timezone.utc)
    value = origin + timedelta(microseconds=ticks // 10)
    return value.isoformat(timespec="milliseconds").replace("+00:00", "Z")


def resolve_stream(path: Path) -> Path:
    if path.is_dir():
        path = path / "ram-xor-stream.bin.gz"
    if not path.is_file():
        raise FileNotFoundError(path)
    return path


def read_header(stream) -> CaptureHeader:
    magic, version, ram_size, interval_ms, started_ticks = HEADER.unpack(
        read_exact(stream, HEADER.size)
    )
    if magic != b"MK3RAM02" or version != 2:
        raise ValueError(f"Unsupported capture format: {magic!r}, version {version}")
    if ram_size <= 0 or ram_size > 16 * 1024 * 1024:
        raise ValueError(f"Implausible RAM size: {ram_size}")
    return CaptureHeader(ram_size, interval_ms, started_ticks)


def held_inputs(main_value: int, aux_value: int, player_two: bool) -> tuple[str, ...]:
    names = [name for bit, name in MAIN_INPUT_BITS if not main_value & bit]
    aux_bits = P2_AUX_INPUT_BITS if player_two else P1_AUX_INPUT_BITS
    names.extend(name for bit, name in aux_bits if not aux_value & bit)
    return tuple(names)


def input_delta(previous: tuple[str, ...], current: tuple[str, ...]) -> tuple[str, str]:
    pressed = ",".join(name for name in current if name not in previous)
    released = ",".join(name for name in previous if name not in current)
    return pressed, released


def analyze(capture_path: Path, output_dir: Path, top: int) -> None:
    output_dir.mkdir(parents=True, exist_ok=True)
    activity_path = output_dir / "frame-activity.csv"
    offsets_path = output_dir / "offset-activity.csv"
    inputs_path = output_dir / "input-events.csv"
    summary_path = output_dir / "summary.txt"

    with gzip.open(capture_path, "rb") as stream:
        header = read_header(stream)
        changes = array("I", [0]) * header.ram_size
        first_frame = array("i", [-1]) * header.ram_size
        last_frame = array("i", [-1]) * header.ram_size
        minimum = bytearray([255]) * header.ram_size
        maximum = bytearray(header.ram_size)
        current = bytearray(header.ram_size)
        initial = None
        frame_count = 0
        first_ticks = None
        last_ticks = None
        process_ids: set[int] = set()
        ram_addresses: set[int] = set()

        with (
            activity_path.open("w", newline="", encoding="utf-8-sig") as activity_file,
            inputs_path.open("w", newline="", encoding="utf-8-sig") as inputs_file,
        ):
            writer = csv.writer(activity_file, delimiter=";")
            writer.writerow(
                [
                    "timestamp_utc",
                    "frame",
                    "process_id",
                    "ram_address",
                    "segments",
                    "changed_bytes",
                ]
            )
            input_writer = csv.writer(inputs_file, delimiter=";")
            input_writer.writerow(
                [
                    "timestamp_utc",
                    "frame",
                    "p1_main_hex",
                    "p2_main_hex",
                    "aux_hex",
                    "p1_held",
                    "p1_pressed",
                    "p1_released",
                    "p2_held",
                    "p2_pressed",
                    "p2_released",
                ]
            )
            previous_p1_inputs: tuple[str, ...] | None = None
            previous_p2_inputs: tuple[str, ...] | None = None

            while True:
                frame_header = stream.read(FRAME.size)
                if not frame_header:
                    break
                if len(frame_header) != FRAME.size:
                    raise EOFError("Truncated frame header")
                ticks, frame, process_id, ram_address, segment_count = FRAME.unpack(
                    frame_header
                )
                changed_bytes = 0
                if segment_count == -1:
                    current[:] = read_exact(stream, header.ram_size)
                    initial = bytes(current)
                    for offset, value in enumerate(current):
                        minimum[offset] = value
                        maximum[offset] = value
                elif segment_count < 0:
                    raise ValueError(f"Invalid segment count {segment_count} at frame {frame}")
                else:
                    for _ in range(segment_count):
                        offset, length = SEGMENT.unpack(read_exact(stream, SEGMENT.size))
                        if offset < 0 or length <= 0 or offset + length > header.ram_size:
                            raise ValueError(
                                f"Invalid segment offset={offset} length={length} at frame {frame}"
                            )
                        values = read_exact(stream, length)
                        for relative, value in enumerate(values):
                            absolute = offset + relative
                            if current[absolute] == value:
                                continue
                            current[absolute] = value
                            changes[absolute] += 1
                            if first_frame[absolute] < 0:
                                first_frame[absolute] = frame
                            last_frame[absolute] = frame
                            if value < minimum[absolute]:
                                minimum[absolute] = value
                            if value > maximum[absolute]:
                                maximum[absolute] = value
                            changed_bytes += 1

                writer.writerow(
                    [
                        dotnet_ticks_to_iso(ticks),
                        frame,
                        process_id,
                        f"0x{ram_address:X}",
                        segment_count,
                        changed_bytes,
                    ]
                )

                p1_inputs = held_inputs(
                    current[P1_MAIN_INPUT_OFFSET],
                    current[AUX_INPUT_OFFSET],
                    False,
                )
                p2_inputs = held_inputs(
                    current[P2_MAIN_INPUT_OFFSET],
                    current[AUX_INPUT_OFFSET],
                    True,
                )
                if previous_p1_inputs is None or previous_p2_inputs is None:
                    previous_p1_inputs = p1_inputs
                    previous_p2_inputs = p2_inputs
                elif p1_inputs != previous_p1_inputs or p2_inputs != previous_p2_inputs:
                    p1_pressed, p1_released = input_delta(previous_p1_inputs, p1_inputs)
                    p2_pressed, p2_released = input_delta(previous_p2_inputs, p2_inputs)
                    input_writer.writerow(
                        [
                            dotnet_ticks_to_iso(ticks),
                            frame,
                            f"0x{current[P1_MAIN_INPUT_OFFSET]:02X}",
                            f"0x{current[P2_MAIN_INPUT_OFFSET]:02X}",
                            f"0x{current[AUX_INPUT_OFFSET]:02X}",
                            ",".join(p1_inputs),
                            p1_pressed,
                            p1_released,
                            ",".join(p2_inputs),
                            p2_pressed,
                            p2_released,
                        ]
                    )
                    previous_p1_inputs = p1_inputs
                    previous_p2_inputs = p2_inputs
                frame_count += 1
                first_ticks = ticks if first_ticks is None else first_ticks
                last_ticks = ticks
                process_ids.add(process_id)
                ram_addresses.add(ram_address)

    if initial is None:
        raise ValueError("Capture has no complete RAM frame")

    active_offsets = [index for index, count in enumerate(changes) if count]
    active_offsets.sort(key=lambda index: (-changes[index], index))
    if top > 0:
        active_offsets = active_offsets[:top]

    with offsets_path.open("w", newline="", encoding="utf-8-sig") as offsets_file:
        writer = csv.writer(offsets_file, delimiter=";")
        writer.writerow(
            [
                "offset_hex",
                "known_name",
                "change_count",
                "first_frame",
                "last_frame",
                "initial_hex",
                "final_hex",
                "minimum_hex",
                "maximum_hex",
            ]
        )
        for offset in active_offsets:
            writer.writerow(
                [
                    f"0x{offset:05X}",
                    KNOWN_OFFSETS.get(offset, ""),
                    changes[offset],
                    first_frame[offset],
                    last_frame[offset],
                    f"0x{initial[offset]:02X}",
                    f"0x{current[offset]:02X}",
                    f"0x{minimum[offset]:02X}",
                    f"0x{maximum[offset]:02X}",
                ]
            )

    duration_seconds = 0.0
    if first_ticks is not None and last_ticks is not None:
        duration_seconds = (last_ticks - first_ticks) / 10_000_000
    summary_lines = [
        f"capture={capture_path}",
        f"format=MK3RAM02/v2",
        f"ram_size={header.ram_size}",
        f"capture_interval_ms={header.interval_ms}",
        f"frames={frame_count}",
        f"duration_seconds={duration_seconds:.3f}",
        f"active_offsets_total={sum(1 for count in changes if count)}",
        f"offset_rows_written={len(active_offsets)}",
        f"process_ids={','.join(str(value) for value in sorted(process_ids))}",
        "ram_addresses=" + ",".join(f"0x{value:X}" for value in sorted(ram_addresses)),
        f"frame_activity={activity_path}",
        f"offset_activity={offsets_path}",
        f"input_events={inputs_path}",
    ]
    summary_path.write_text("\n".join(summary_lines) + "\n", encoding="utf-8-sig")
    print("\n".join(summary_lines))


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Decode a MK3 MoveBook research memory capture."
    )
    parser.add_argument("capture", type=Path, help="Capture directory or .bin.gz stream")
    parser.add_argument(
        "--output",
        type=Path,
        help="Output directory (default: <capture>/analysis)",
    )
    parser.add_argument(
        "--top",
        type=int,
        default=10000,
        help="Maximum number of most active offsets to write; 0 writes all",
    )
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    capture_path = resolve_stream(args.capture.resolve())
    output_dir = args.output
    if output_dir is None:
        output_dir = capture_path.parent / "analysis"
    analyze(capture_path, output_dir.resolve(), max(0, args.top))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
