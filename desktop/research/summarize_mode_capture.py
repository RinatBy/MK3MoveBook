#!/usr/bin/env python3
"""Summarize confirmed match-mode bytes in a MK3 RAM capture."""

from __future__ import annotations

import argparse
from collections import Counter
from pathlib import Path

from analyze_capture import FRAME, SEGMENT, open_capture_stream, read_exact, read_header, resolve_stream


OFFSETS = (
    (0x0C424, "powerbars"),
    (0x0C5D7, "tower_state_a"),
    (0x0C5D9, "tower_state_b"),
    (0x0C5F6, "tower_id"),
    (0x0B77A, "tower_fight"),
    (0x0C2AA, "p2_control"),
    (0x0C144, "p1_participant"),
    (0x0C2BA, "p2_participant"),
    (0x0C5FE, "team_size_code"),
    (0x0C146, "p1_character"),
    (0x0C2BC, "p2_character"),
)


def summarize(capture: Path) -> None:
    path = resolve_stream(capture.resolve())
    stream, truncated = open_capture_stream(path)
    counts: Counter[tuple[int, tuple[int, ...]]] = Counter()
    process_frames: Counter[int] = Counter()
    active_frames = 0

    with stream:
        header = read_header(stream)
        current = bytearray(header.ram_size)
        while True:
            frame_header = stream.read(FRAME.size)
            if not frame_header:
                break
            if len(frame_header) != FRAME.size:
                truncated = True
                break
            _, _, process_id, _, segment_count = FRAME.unpack(frame_header)
            if segment_count == -1:
                try:
                    current[:] = read_exact(stream, header.ram_size)
                except EOFError:
                    truncated = True
                    break
            else:
                try:
                    for _ in range(segment_count):
                        offset, length = SEGMENT.unpack(read_exact(stream, SEGMENT.size))
                        current[offset : offset + length] = read_exact(stream, length)
                except EOFError:
                    truncated = True
                    break

            process_frames[process_id] += 1
            if current[0x0C424] != 1:
                continue
            active_frames += 1
            values = tuple(current[offset] for offset, _ in OFFSETS)
            counts[(process_id, values)] += 1

    print(f"capture={path.parent.name}")
    print(f"capture_truncated={'true' if truncated else 'false'}")
    print(f"process_frames={dict(sorted(process_frames.items()))}")
    print(f"active_frames={active_frames}")
    print("count;process_id;" + ";".join(name for _, name in OFFSETS))
    for (process_id, values), count in counts.most_common():
        rendered = ";".join(f"0x{value:02X}" for value in values)
        print(f"{count};{process_id};{rendered}")


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("capture", type=Path)
    return parser.parse_args()


if __name__ == "__main__":
    summarize(parse_args().capture)
