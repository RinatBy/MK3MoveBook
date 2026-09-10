# Frosty, Toasty and Sheeva 42% combo signals

Date: 2026-08-03

## Captures

- `capture-20260803-172931`: Unmasked Sub-Zero versus Scorpion.
- `capture-20260803-173848`: Sheeva versus Scorpion with repeated combo attempts.
- `capture-20260803-173131`: contains a confirmed uppercut arena transition.

The captures are stored under
`work/mode-diagnostic-test-12/research-captures` outside the application source
tree.

## Process table

The supported arcade ROM exposes 94 process slots:

- table offset: `0x0D442`;
- slot stride: `0xDE` bytes;
- process id: slot `+ 0xC2`;
- player action: process-id address `+ 0x3A`;
- last hit action (`p_hitby`): process-id address `+ 0x68`;
- combo damage (`p_damage`): process-id address `+ 0x6A`;
- combo hit count (`p_hit`): process-id address `+ 0x6E`;
- process argument used by Forden: process-id address `+ 0x10`.

Player processes have ids `0x0001` and `0x0002`. Dan Forden uses process id
`0x010F` (`PID_FORDEN`).

## Frosty

The `PID_FORDEN` process has argument `0x001C` while Frosty is visible.
In capture `172931` this signal is present at frames 2433–2571 and again from
frame 2736. The opponent action is `0x0610` (`ACT_FROZEN`) and the opponent is
at critical health.

The reference source creates `FX_FORDEN` with argument `0x1C` in `r_freeze`
when the frozen fighter has 8 HP or less. The reaction does not check which
Sub-Zero variant caused the freeze, so the achievement is tied to the actual
freezing player rather than hard-coded to Unmasked Sub-Zero.

## Toasty

The same `PID_FORDEN` process has argument `0x001B` while Toasty is visible.
In capture `173848` the process is active from frame 2202 through frame 2545.
The source confirms that `fx_comment` randomly calls `forden_peek(0x1B)`.
Therefore Toasty is a distinct random event and is not an intrinsic part of
Sheeva's combo.

## Sheeva 42% combo

The successful attempt immediately before Toasty reaches:

- 7 hits;
- 70 raw damage;
- `floor(70 * 100 / 166) = 42%`.

The target process shows damage progression
`16, 26, 36, 43, 53, 60, 70` and hit progression `1..7` at frames
2106–2183. Detection uses these two counters, not input timing, so duplicated
or dropped input samples cannot change the result.

The combo reward is emitted after the counters reset. This intentionally gives
the rarer Toasty event priority when both happen during the same combo; the
server still grants at most one new achievement for the match.

## Other confirmed/inferred actions

- Uppercut is action `0x010E` (`ACT_UPPERCUT`). In capture `173131`, P2's
  `p_hitby` becomes `0x010E`, then the zone changes from `0x0C` to `0x0D`.
- Nightwolf's arrow is action `0x0005` (`ACT_IND_ZAP`) in the reference source.
  Its round-winning detector is implemented, but it still needs one direct
  capture of an arrow as the last hit to validate the arcade memory lifetime.
- The game creates the visible Danger warning only at 1–6 HP. The low-health
  round achievements therefore remember whether the player reached that range
  at any time in the round instead of checking only the final HP value.

## Reference source

The incomplete PlayStation source release was used only to name and interpret
signals already observed in the arcade RAM captures:

- <https://github.com/RetailGameSourceCode/UltimateMortalKombat3>
- `src/MKFX.C`, `src/MKREACT.C`, `src/MKUTIL.C`, `src/MKZAP.C`,
  `src/MKGAME.H`, and `src/MKOS.H`.
