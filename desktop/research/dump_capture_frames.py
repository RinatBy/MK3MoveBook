#!/usr/bin/env python3
"""Dump selected RAM ranges from frames in a MoveBook research capture."""

from __future__ import annotations

import argparse
from pathlib import Path

from analyze_capture import FRAME, SEGMENT, open_capture_stream, read_exact, read_header, resolve_stream


def parse_int(value: str) -> int:
    return int(value, 0)


def hexdump(data: bytes, start: int) -> str:
    lines: list[str] = []
    for relative in range(0, len(data), 16):
        chunk = data[relative : relative + 16]
        hexadecimal = " ".join(f"{value:02X}" for value in chunk)
        text = "".join(chr(value) if 0x20 <= value <= 0x7E else "." for value in chunk)
        lines.append(f"0x{start + relative:05X}  {hexadecimal:<47}  {text}")
    return "\n".join(lines)


def dump(capture: Path, frames: list[int], start: int, end: int) -> None:
    path = resolve_stream(capture.resolve())
    requested = sorted(set(frames))
    remaining = set(requested)
    stream, truncated = open_capture_stream(path)

    print(f"capture={path.parent.name}")
    print(f"capture_truncated={'true' if truncated else 'false'}")
    with stream:
        header = read_header(stream)
        if start < 0 or end > header.ram_size or start >= end:
            raise ValueError(
                f"Range 0x{start:X}..0x{end:X} is outside RAM size 0x{header.ram_size:X}"
            )
        current = bytearray(header.ram_size)
        while remaining:
            frame_header = stream.read(FRAME.size)
            if not frame_header:
                break
            if len(frame_header) != FRAME.size:
                break
            _, frame, process_id, ram_address, segment_count = FRAME.unpack(frame_header)
            if segment_count == -1:
                current[:] = read_exact(stream, header.ram_size)
            else:
                for _ in range(segment_count):
                    offset, length = SEGMENT.unpack(read_exact(stream, SEGMENT.size))
                    current[offset : offset + length] = read_exact(stream, length)

            if frame not in remaining:
                continue
            remaining.remove(frame)
            print(
                f"\nframe={frame};process_id={process_id};ram_address=0x{ram_address:X}"
            )
            print(hexdump(bytes(current[start:end]), start))

    for frame in sorted(remaining):
        print(f"missing_frame={frame}")


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("capture", type=Path)
    parser.add_argument("--frame", type=parse_int, action="append", required=True)
    parser.add_argument("--start", type=parse_int, default=0x60000)
    parser.add_argument("--end", type=parse_int, default=0x60100)
    return parser.parse_args()


if __name__ == "__main__":
    args = parse_args()
    dump(args.capture, args.frame, args.start, args.end)
