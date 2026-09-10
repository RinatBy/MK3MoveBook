#!/usr/bin/env python3
"""Replay a research capture and print confirmed achievement memory signals."""

from __future__ import annotations

import argparse
from pathlib import Path

from analyze_capture import (
    FRAME,
    SEGMENT,
    open_capture_stream,
    read_exact,
    read_header,
    resolve_stream,
)


PROCESS_TABLE_OFFSET = 0x0D442
PROCESS_COUNT = 94
PROCESS_STRIDE = 0xDE
PROCESS_ID_FIELD_OFFSET = 0xC2
ACTION_FROM_PROCESS_ID = 0x3A
HIT_BY_FROM_PROCESS_ID = 0x68
COMBO_DAMAGE_FROM_PROCESS_ID = 0x6A
COMBO_HITS_FROM_PROCESS_ID = 0x6E
FORDEN_ARGUMENT_FROM_PROCESS_ID = 0x10
PID_P1 = 0x0001
PID_P2 = 0x0002
PID_FORDEN = 0x010F
ARG_TOASTY = 0x001B
ARG_FROSTY = 0x001C
ACT_UPPERCUT = 0x010E
SHEEVA_ID = 0x0B
P1_CHARACTER_OFFSET = 0x0C146
P2_CHARACTER_OFFSET = 0x0C2BC
KOMBAT_ZONE_OFFSET = 0x0C44C


def u16(ram: bytearray, offset: int) -> int:
    return ram[offset] | ram[offset + 1] << 8


def process_state(ram: bytearray) -> dict[str, int | bool]:
    state: dict[str, int | bool] = {
        "p1_action": -1,
        "p2_action": -1,
        "p1_hit_by": -1,
        "p2_hit_by": -1,
        "p1_damage": -1,
        "p2_damage": -1,
        "p1_hits": -1,
        "p2_hits": -1,
        "toasty": False,
        "frosty": False,
    }
    for index in range(PROCESS_COUNT):
        process_id_offset = (
            PROCESS_TABLE_OFFSET
            + index * PROCESS_STRIDE
            + PROCESS_ID_FIELD_OFFSET
        )
        process_id = u16(ram, process_id_offset)
        if process_id in (PID_P1, PID_P2):
            prefix = "p1" if process_id == PID_P1 else "p2"
            state[f"{prefix}_action"] = u16(
                ram, process_id_offset + ACTION_FROM_PROCESS_ID
            )
            state[f"{prefix}_hit_by"] = u16(
                ram, process_id_offset + HIT_BY_FROM_PROCESS_ID
            )
            state[f"{prefix}_damage"] = u16(
                ram, process_id_offset + COMBO_DAMAGE_FROM_PROCESS_ID
            )
            state[f"{prefix}_hits"] = u16(
                ram, process_id_offset + COMBO_HITS_FROM_PROCESS_ID
            )
        elif process_id == PID_FORDEN:
            argument = u16(
                ram, process_id_offset + FORDEN_ARGUMENT_FROM_PROCESS_ID
            )
            state["toasty"] = state["toasty"] or argument == ARG_TOASTY
            state["frosty"] = state["frosty"] or argument == ARG_FROSTY
    return state


def detect(path: Path) -> None:
    stream, truncated = open_capture_stream(resolve_stream(path))
    with stream:
        header = read_header(stream)
        ram = bytearray(header.ram_size)
        previous_toasty = False
        previous_frosty = False
        previous_p1_combo = False
        previous_p2_combo = False
        previous_zone = -1
        while True:
            frame_header = stream.read(FRAME.size)
            if not frame_header:
                break
            if len(frame_header) != FRAME.size:
                truncated = True
                break
            _, frame, _, _, segment_count = FRAME.unpack(frame_header)
            if segment_count == -1:
                ram[:] = read_exact(stream, header.ram_size)
            else:
                for _ in range(segment_count):
                    offset, length = SEGMENT.unpack(
                        read_exact(stream, SEGMENT.size)
                    )
                    ram[offset : offset + length] = read_exact(stream, length)

            state = process_state(ram)
            toasty = bool(state["toasty"])
            frosty = bool(state["frosty"])
            if toasty and not previous_toasty:
                print(f"frame={frame} signal=toasty argument=0x001B")
            if frosty and not previous_frosty:
                print(f"frame={frame} signal=frosty argument=0x001C")

            p1_combo = (
                ram[P1_CHARACTER_OFFSET] == SHEEVA_ID
                and state["p2_damage"] == 70
                and state["p2_hits"] == 7
            )
            p2_combo = (
                ram[P2_CHARACTER_OFFSET] == SHEEVA_ID
                and state["p1_damage"] == 70
                and state["p1_hits"] == 7
            )
            if p1_combo and not previous_p1_combo:
                print(f"frame={frame} signal=sheeva-42-combo actor=p1")
            if p2_combo and not previous_p2_combo:
                print(f"frame={frame} signal=sheeva-42-combo actor=p2")

            zone = ram[KOMBAT_ZONE_OFFSET]
            if previous_zone >= 0 and zone != previous_zone:
                actor = ""
                if state["p2_hit_by"] == ACT_UPPERCUT:
                    actor = "p1"
                elif state["p1_hit_by"] == ACT_UPPERCUT:
                    actor = "p2"
                if actor:
                    print(
                        f"frame={frame} signal=stage-transition-uppercut "
                        f"actor={actor} zone=0x{previous_zone:02X}->0x{zone:02X}"
                    )

            previous_toasty = toasty
            previous_frosty = frosty
            previous_p1_combo = p1_combo
            previous_p2_combo = p2_combo
            previous_zone = zone

    if truncated:
        print("warning=capture-truncated")


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("capture", type=Path)
    args = parser.parse_args()
    detect(args.capture)


if __name__ == "__main__":
    main()
