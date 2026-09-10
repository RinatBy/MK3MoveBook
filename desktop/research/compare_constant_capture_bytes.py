#!/usr/bin/env python3
"""Compare bytes that stay constant throughout active fights in RAM captures."""

from __future__ import annotations

import argparse
from pathlib import Path

from analyze_capture import FRAME, SEGMENT, open_capture_stream, read_exact, read_header, resolve_stream


POWERBARS_OFFSET = 0x0C424


def constant_active_bytes(capture: Path, start: int, end: int) -> tuple[dict[int, int], int]:
    path = resolve_stream(capture.resolve())
    stream, _ = open_capture_stream(path)
    active_frames = 0
    baseline: bytes | None = None
    candidates: set[int] = set()

    with stream:
        header = read_header(stream)
        current = bytearray(header.ram_size)
        while True:
            frame_header = stream.read(FRAME.size)
            if not frame_header or len(frame_header) != FRAME.size:
                break
            _, _, _, _, segment_count = FRAME.unpack(frame_header)
            changed_ranges: list[tuple[int, int]] = []
            if segment_count == -1:
                current[:] = read_exact(stream, header.ram_size)
                changed_ranges.append((start, end))
            else:
                for _ in range(segment_count):
                    offset, length = SEGMENT.unpack(read_exact(stream, SEGMENT.size))
                    current[offset : offset + length] = read_exact(stream, length)
                    changed_ranges.append((offset, offset + length))

            if current[POWERBARS_OFFSET] != 1:
                continue
            active_frames += 1
            if baseline is None:
                baseline = bytes(current)
                candidates = set(range(start, end))
                continue
            for changed_start, changed_end in changed_ranges:
                overlap_start = max(start, changed_start)
                overlap_end = min(end, changed_end)
                for offset in range(overlap_start, overlap_end):
                    if offset in candidates and current[offset] != baseline[offset]:
                        candidates.remove(offset)

    if baseline is None:
        return {}, 0
    return {offset: baseline[offset] for offset in candidates}, active_frames


def compare(captures: list[Path], start: int, end: int, limit: int) -> None:
    results = [constant_active_bytes(path, start, end) for path in captures]
    names = [resolve_stream(path.resolve()).parent.name for path in captures]
    for name, (_, frames) in zip(names, results):
        print(f"{name}: active_frames={frames}")
    common = set.intersection(*(set(values) for values, _ in results))
    differing = [
        offset
        for offset in sorted(common)
        if len({values[offset] for values, _ in results}) > 1
    ]
    print("offset;" + ";".join(names))
    for offset in differing[:limit]:
        values = ";".join(f"0x{result[offset]:02X}" for result, _ in results)
        print(f"0x{offset:05X};{values}")
    print(f"differing_constant_offsets={len(differing)}")


def parse_int(value: str) -> int:
    return int(value, 0)


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("capture", type=Path, nargs="+")
    parser.add_argument("--start", type=parse_int, default=0x0B000)
    parser.add_argument("--end", type=parse_int, default=0x0C800)
    parser.add_argument("--limit", type=int, default=500)
    return parser.parse_args()


if __name__ == "__main__":
    args = parse_args()
    compare(args.capture, args.start, args.end, max(0, args.limit))
