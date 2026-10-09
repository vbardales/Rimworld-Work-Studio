# TESTING.md history (moved 2026-10-09)

Section "What the passes said, 2026-09-21" of TESTING.md and what followed it, on old revisions. Current state: STATUS.md and the run lines of docs/runs/. Full former file: `git show 139fcf1:TESTING.md`.

### What the passes said, 2026-09-21

| Set | Result | What it does and does not establish |
| --- | --- | --- |
| `sans-facultatifs`, English | 47 of 47 | The mod stands up alone. The five `@review` scenarios asserted nothing; the captures were read by a person and found clean |
| `sans-facultatifs`, French | 47 of 47 | Same, in French, with the language named in the report; captures read, and the right-hand column was reworked because of what they showed |
| `avec-better-work-tab` | 47 of 47 | Coexistence with Better Work Tab, including scenario 2, the one written for tab replacers |
| `avec-enhanced-work-tab` | **45 of 47** on 2026-09-21; **49 passed, 0 failed, 6 skipped of 55 on 2026-09-23** (the skips are other mods' @requires scenarios) | Coexistence holds on the current build: the button scenario and both save scenarios of 05 pass. A first claim of 05 on 09-22 was withdrawn, below |
| `incompat-fluffy-worktab` | **45 of 47** | Historical exploration: the button race and save-load exception do not establish incompatibility. Feature 15 was added later from direct inspection and is written but not yet run |
| `incompat-compact-worktab` | 47 of 47 | Historical coexistence result, consistent with the later code inspection; the incorrect incompatibility declaration was removed on 2026-09-22 |

**With Enhanced Work Tab loaded, two things failed on 2026-09-21, and they were different in kind.**

*The save scenario was a finding about the mod's purpose, and it is fixed.* "A save written with one
type, loaded with two" asserts that a type absent from the save reads priority 0, and it read **2**,
while the raw `DefMap` value was **0** — Work Studio's own restore did its job, and something else
answered `GetPriority`. That matched what was seen on 2026-09-19, when this mod was found to
intercept `GetPriority`.

**The cause: a deleted type's defName was reused.** Enhanced Work Tab keeps each colonist's
priorities by `defName` in the save, and never prunes an entry whose type is gone. The editor's
`NewTypeId` took the first free `WorkStudio_Type<n>`, so deleting "Pickle first" and creating another
type gave it the SAME defName — and Enhanced Work Tab handed it the deleted type's old priority, 2,
while Work Studio's own list correctly held 0 for a name it had never written a value under. The fix
(`ed41491`) keeps a counter in the settings that only ever goes up, read from the source of
`EnhancedWorkTabGameComponent`/`PawnWorkSettings_GetPriority_Patch` decompiled for this diagnosis, not
guessed. **Confirmed in game, 2026-09-23**: `avec-enhanced-work-tab`, filtered to `05-priorities-across-save.feature`, 3 of 3 passed, `exitReason: passed`; the report's `setName` and its suite name were read before it was cited, and the launcher's own output for the run agreed. A first claim of the same result on 2026-09-22 is **withdrawn**: the `summary.json` copied then belonged to a different test (`PickleTools interface scale`, from a run queued around the same time and read from the shared one-report-for-the-whole-machine folder without checking its content). Reading another mod's decompiled source to explain a failure, rather than only measuring the outside, let the cause be named before the replay; the replay is what confirmed it.

*The button scenario is a measured overlap, and the cause is not yet known to be Enhanced Work Tab's.*
The tab window is vanilla `MainTabWindow_Work` here, so this mod **patches** the tab and does not
replace it. Pickle clicked at exactly the centre of the drawn button, (1389, 707) for a centre of
(1389.5, 706), so the rectangle was right. A second `Widgets.ButtonText`, 98x24 at (1362, 697),
drawn before ours (#64 against #103) also contains that point. Its label was not printed, so which
control it is is not established; a text button drawn earlier is offered the click first.

**The Work Tab button failure is explained, after three wrong explanations.** A Pickle build that
traces its own tag recording, run beside this suite's probe in the same game, gave identical raw and
converted rectangles on both sides for every frame, with an identity GUI matrix: Pickle's conversion
is correct. The button itself moved. With Work Tab the tab window is drawn narrow for about ten
frames after it opens and then widens to the full screen, and the button is anchored to its right
edge: raw x 975 for frames 216-225, raw x 1748 from frame 226. Pickle resolved the tag at 975, moved
the pointer there and pressed; the window widened before the release; IMGUI counts a click only when
press and release land on the same control. Nothing is wrong with the button for a person, who does
not click within a fifth of a second of opening the tab. The click step now waits until the button
has stood still for twelve frames.

The three earlier explanations - a Work Tab toggle took the click, something moved the pointer, and
Pickle stored a rectangle 773 px off - are withdrawn; each was stated more firmly than its evidence
carried. `BACKLOG.md` keeps them in order, because the sequence is the useful part.

**2026-09-23: the wait did not fix it, so the layout race was not the whole story.** Scenario 2 replayed
against `incompat-fluffy-worktab` (report read before citing: setName, suite name and the three scenario
names are this mod's; summary in `docs/runs/2026-09-23.md`, full report on disk in the ignored `Tests/Pickle/evidence/2026-09-23-fluffy-scenario2`): 2 of 3,
the button scenario failed again, now with the new diagnostic. Measured, not interpreted: the click step's
pointer was at (1058, 774) before and after the click, which is where the button was drawn while the tab
window was still narrow; the probe saw the button drawn at (1754, 759, 155x28), centre (1831.5, 773), and the
steps before the click (12 frames standing still, then the hover check) had passed. The window stack was
clean (nothing absorbing input, no window over the button). So Pickle aimed at a rectangle the probe says the
button had left, after the button had stood still for twelve frames. Not established: why. The earlier
explanation accounted for the first failure and does not account for this one; a stale or duplicated tag
inside Pickle, or the hover step itself, are candidates and neither is checked.

**The raw reports behind these results no longer exist.** The shared launcher keeps only the fifteen
newest report folders, and with this many sessions queued that is a few hours; the folders for the
runs above had already gone by the evening of 2026-09-21. What survives is what was copied into
this file and into `BACKLOG.md`: notably Pickle's own log line for the Work Tab click, `pointer at
(1058.50, 773.00): the OS reports x:1058 y:773 ... the game reads (1058.00, 774.00)`, and the
numbers of the Enhanced Work Tab failures. Anything a document cites from a report should be copied
into the repository at the time, or its folder given a `keep.txt`, which the launcher never removes.
