# Testing Work Studio in game

Version 1.0.0 was played and worked. Everything added since — the up/down arrows, import and
export, the startup drift warning, the owning type shown in the right-hand column, the task order
within a type, and three successive attempts at the Work tab button — has only ever been compiled.

Two of those button attempts were already observed failing, both in a log rather than on screen,
which is why this file exists in the order it does: the cheap checks that invalidate everything
else come first.

This file says what each scenario proves, because a test whose failure you cannot interpret is not
worth running.

## Before anything

1. **Harmony must be active, and this mod after it.** Declared in `About.xml`; the mod list will
   say so if it is not.
2. **The packageId is `nelim.workstudio`.** It changed after the first release, so RimWorld may
   have dropped the mod from a previously saved list. Re-tick it.
3. **Keep `Player.log`.** Everything this mod complains about is prefixed `[Work Studio]`. The
   file is overwritten at the next launch and moved to `Player-prev.log`, so copy it out before
   relaunching.
4. Your Work tab is **Better Work Tab**, not the vanilla one. That is not a side note: it replaces
   the tab window entirely, which is what broke the button twice. Scenario 11 covers what it owns.

The editor opens from **Work types…** at the top right of the Work tab, or from the mod settings.
The settings route always works; the button is the thing under test.

## 1 — The mod loads and its patches apply

**Proves** the three Harmony patches and the startup pass that creates the custom types. Every
other scenario depends on this one.

Start the game, reach the main menu, quit. Search `Player.log` for `[Work Studio]`.
**Silence is the pass**, with one exception noted in scenario 2.

Two messages mean real trouble:

- *"Work type not found: '…'"* — a saved assignment points at a work type no longer present. Not a
  failure in itself: the tasks fall back to their original type, and the assignment is kept in case
  the mod returns. Only worrying if you did not change your mod list.
- Any unhandled exception naming `WorkStudio` — that is a failure, and the stack trace names the
  step: `SyncCustomTypes`, `ApplyGiverAssignments`, `RebuildDefs`, `RebuildWorkColumns` or
  `PriorityMemory`.

## 2 — The button appears in the Work tab

**Proves** the patch resolves the window class actually in use. This has failed twice and is the
single most likely thing to still be wrong.

Open the Work tab. **A `Work types…` button sits at the top right of the band, above the table.**

The two past failures both announced themselves in the log, and a third is possible:

- *"Could not add the button to the Work tab: Parameter "rect" not found…"* — the fixed bug.
  Harmony injects patch arguments **by name**, and the parameter is `rect` in vanilla but `inRect`
  in Better Work Tab. The current code asks for it by position (`__0`) instead. If this line comes
  back, the positional form is not doing what it should.
- *"Could not find the Work tab window"* — the `MainButtonDef` named `Work` or its `tabWindowClass`
  could not be read. The editor is still reachable from the settings.
- **Silence and no button** is the worst outcome, and it means the patch applied to a method that
  is never called — the first bug, in a new disguise. The fix targets whichever class declares
  `DoWindowContents`, walking up from `tabWindowClass`; a tab mod that declares none, and whose
  parent is skipped, would land here.

Check the other tabs while you are there: **Animals and Restrictions must not have the button.**
They share the same base method, and a guard on the def name is what keeps them clean.

## 3 — The three columns say what they are

**Proves** the 1.0.1 clarification. You reported not understanding the middle and right columns,
and this is the change that answers it.

Open the editor and select a work type.

- **Left**: every work type, in the real order, with the number of tasks it holds.
- **Middle**: titled *Tasks of "X"* — the tasks **in** the selected type.
- **Right**: titled *Tasks from other types* — everything else, searchable, each row showing **the
  type it currently belongs to in grey** on the right of its name, and a help line underneath
  naming where a click will send it.

If the right column still reads *Add a task* and shows no grey type, the running assembly predates
the fix.

## 4 — Create a type, and fill it

**Proves** the def creation, the task reassignment and the column rebuild, all at runtime.

New type → name it → select it → click a task in the right column, for instance *Tame* or *Milk*.

- A new column appears in the Work tab **immediately**, without reopening the tab.
- The task leaves the middle column of its old type and joins the new one.
- The old type's task count drops by one.

Then check a colonist can actually do it: their Work tab cell for the new type accepts a priority,
and a colonist assigned to it walks off and does the task.

## 5 — Priorities survive a reconfiguration across a save

**This is the scenario the mod exists for.** Everything else is convenience; this is the one that
can quietly ruin a colony, and nobody has ever run it.

**Proves** `PriorityMemory` and the postfix on `Pawn_WorkSettings.ExposeData`. RimWorld stores
priorities in a list indexed by position with no record of which work each value belongs to, so
adding or removing a single work type shifts every value after it, for every pawn.

1. On a running colony, **screenshot one colonist's whole Work row** — the numbers, not an
   impression of them.
2. **Save.**
3. In the editor, **create a type and move a task into it.** The number of work types has now
   changed, which is the whole point.
4. **Reload the save from step 2.**
5. Compare against the screenshot.

**The pass**: every vanilla priority identical, and the new type carrying the priority of the type
its task came from.

**The failure is unmistakable** — the values are shifted by one column, so a doctor reads as a
miner. If you see that, stop and keep the save; it means the named priorities are not being written
or not being read back.

Then the reverse: delete the custom type, reload again, compare again.

## 6 — Reordering, both levels

**Proves** the reorderable group fix and the insertion convention, neither of which has run.

There are two independent orders and they are not the same thing:

- **Left column** — the order of the types. Drives the columns and the order a colonist moves from
  one job to the next.
- **Middle column** — the order of tasks **inside** one type. Drives which task they pick up first
  once already on that job.

On each, try **both** the drag and the arrows, and separately:

- Dragging a row **downward** must land it where you dropped it, not one row further. That
  off-by-one is the insertion convention, and it only shows on downward moves.
- Dragging must actually start. If the row highlights and nothing moves, the group identifier is
  being lost between event passes again.
- **At the ends of a list the arrow is greyed and does nothing** — no click sound, no movement.

With Better Work Tab installed, the left-column order may not reach the game. See scenario 11
before calling that a bug.

## 7 — Rename a work type

**Proves** three things at once, and two of them were wrong until recently.

Rename a vanilla type — *Hauling* to *Portage*, say.

- The **column header** changes. The header draws `labelShort`, not `label`, so a rename that only
  touched `label` would show the old name here.
- A name too long for the narrow header is represented by its icon and full-name tooltip. It must
  stay identifiable without spilling into a neighbouring column. The renamed value remains in the
  work type; the column worker must have been rebuilt for its other cached state.
- If **[baku] Work Type Tag** is installed, the name in front of a colonist's current job changes
  too, without a restart. That one is a cache this mod clears by reflection.

  The cache-clearing itself is proven off-game, against the real third-party assembly, by
  `Tests/OffGame`'s `TheWorkTypeTagCompat` check (skips cleanly if that Workshop item is not
  installed on the machine running the suite). The dedicated Pickle pass `avec-work-type-tag`
  creates a current job attributed to `HaulGeneral`, asserts that its real job report contains the
  renamed label, selects the colonist and captures the result. A person only judges the screenshot.

## 8 — Hide a column

**Proves** that hiding removes the column and nothing else.

Untick a type in the left column. Its column leaves the Work tab. The colonists **keep doing that
work** at the priority they had — hiding is not disabling. Re-tick it and the column returns with
the priorities intact.

The "hiding is not disabling" half is also proven off-game, against the real
`WorkTypeRuntime.Apply()`, by `Tests/OffGame`'s `TheHideColumnRegression` (mutation-confirmed
sensitive). What still needs the game: the live run on 2026-09-17 found a colonist's priority
zeroed after hide/show anyway — see `STATUS.md`'s "In-game pass" section for why the off-game proof
rules out this mod's own reconciliation logic as the cause, and what is still open.

## 9 — Export, reset, import

**Proves** the settings round trip and, more importantly, that a failed import does not leave half
a configuration behind.

Mod settings → export under a name → **Reset the whole setup** → import it back. Everything
returns: custom types, moved tasks, orders, renames, hidden columns.

The files live in `WorkStudio/`, beside `Saves` and `Config`. Open the exported file: it must
**not** contain `knownWorkTypes`, which is a snapshot of your mod list and would raise a false
alarm on someone else's machine.

## 10 — The mod list drift warning

**Proves** the once-only warning, whose whole value is that it does not nag.

With a configuration in place, disable a mod that adds a work type and start the game. A dialog
lists what changed — types gone, tasks not found, types added.

**Start again without changing anything: the dialog must not come back.** It reappearing on every
launch is the failure.

## 11 — What Better Work Tab owns

**Proves a documented limitation, not a defect.** Read this before reporting scenario 6 as broken.

Better Work Tab replaces the execution order with its own saved column order — but that list starts
empty and is only written the first time you drag one of its columns.

- **If you have never dragged a column there**: its list is empty, everything falls through to
  `naturalPriority`, and Work Studio drives both the columns and the execution order. Scenario 6
  should pass in full.
- **If you have**: its order wins for every type it recorded, and reordering those in Work Studio
  stops having an effect. A type created afterwards is absent from its list and sorts **last**
  until you position it there.

The rule itself — recorded types keep the recorded order, a type missing from it goes last — is
proven off-game against Better Work Tab's real assembly by `Tests/OffGame`'s
`TheBetterWorkTabColumnOrder` (skips cleanly if that Workshop item is not installed). What still
needs the game: actually dragging a column in Better Work Tab and watching Work Studio's own
reordering stop affecting it, live.

Both are correct behaviour. The way to tell them apart is whether you have ever dragged a column
in Better Work Tab on that particular save.

## 12 — Removing the mod

**Proves** that nothing is left behind in a save.

Untick Work Studio, load a colony that used it. Moved tasks are back in their original types, and
every remaining priority is intact. The custom types are gone and the priorities held on them with
them; that is expected and is the only loss.

The priority half is also proven from the raw save, without an actual restart, by
`Tests/Pickle/Mod/Pickle/Features/12-removing-the-mod.feature`: see
`Tests/Pickle/README.md`'s "What stays manual" section for what that does and does not cover.

## 13 — The settings window, both doors

**Proves** that the two ways into the settings are one room, not two. Added 2026-09-18: the twelve
scenarios above are all about the editor and what it does to the Work tab and to colonists, and
none of them ever opened the settings.

There are two doors, and they must agree because they share a single `WorkStudioSettings` instance:

- **Mod options → Work Studio**, which always works and is how the editor is reached when the Work
  tab button fails.
- **The hidden `WorkStudio_Settings` MainButtons shortcut**, which draws nothing by itself: it
  exists so that RIMMSQOL, or another MainButtons customisation mod, can reveal it as a button.

Open each in turn. Both show the same intro line, the same *Open the work type editor* and
*Import / export a setup* buttons, the same save note, and *Reset the whole setup* only when there
is something to reset. Change something through one and reopen the other: the values follow.

Then the parts a machine can check on its own:

- **Both doors lead to one room.** The hidden def's worker is built and activated the way RimWorld
  would; Mod options is opened the way the game opens it, and the window that hosts our settings is
  checked to be holding the running `WorkStudioMod` itself, by reference — two instances would mean
  two configurations that merely look alike.
- **A configuration survives a reload.** Leave the Mod options window (vanilla writes the file in
  its `PreClose`), throw away the settings held in memory, read the file again: the custom type, the
  moved task and the hidden column are all still there.
- **The reset asks first.** With something to reset, the button is offered; clicking it opens a
  confirmation and **destroys nothing yet**. Confirming clears the configuration; going back leaves
  it untouched.

`Tests/Pickle`'s `13-settings.feature` holds those, and `13b-settings-visual.feature` photographs
the window through each door for a person to read.

The dedicated Pickle pass `avec-rimmsqol` uses PickleTools' RIMMSQOL driver to assert that the mod's
own list offers this def, reveal it, verify the persisted choice and real main-bar button, activate
that button, then hide and forget the choice. It repeats activation at 150% interface scale. The
attached screenshots leave only the visual result for a person to judge.

## Automated off game

`Tests/OffGame/` instances the shipped `Mod/Assemblies/WorkStudio.dll` against the installed
game's own `Assembly-CSharp.dll`, no RimWorld process involved. It found and fixed a real defect
before it ever shipped: the Publicizer/`GenerateAssemblyInfo=false` combination that silently drops
the private-member access waiver, which would have thrown `FieldAccessException` on
`PriorityMemory.Restore`'s first call — every startup, every edit. It also loads real third-party
assemblies from the local Workshop subscription when available (skipping cleanly when not) to
prove a compatibility claim against the actual mod rather than a description of it — see
`Tests/OffGame/RESULTS-*.md` for what is covered and what still needs the game (`ConfigFile`'s own
file paths, chiefly).

```powershell
dotnet build Source/WorkStudio.csproj -c Release
dotnet build Tests/OffGame/WorkStudio.Tests.csproj -c Release
.build/offgame/bin/Release/net48/WorkStudio.Tests.exe
```

## Automated in game: Pickle

Scenarios 1, 2, 4 to 10, 13, and half of 12 (the raw-save half, not an actual restart) are also
written in Gherkin under `Tests/Pickle/`, played inside RimWorld by the Pickle test mod. Its README
says how to run them and which parts stay manual.

Run for the first time 2026-09-17, against the full live mod list (150+ mods): scenarios 1, 4, 7,
9 and 10 passed in full; 2, 5, 6, 8 and 12 each had at least one failure, some traced to the
Concord conflict above or to a Pickle-framework issue, others not yet explained. See STATUS.md's
"In-game pass, 2026-09-17" section for the breakdown and what stays open.

### Screenshots, for what only a person can judge

Added 2026-09-18, following the same pattern as Architect Studio's own suite: the features tagged
Every `@review` feature asserts the state it intends to photograph, then attaches a screenshot to
the report. A person only judges the pixels: layout, clipping, readability and composition. The
trip and its functional preconditions are automated and reproducible; no manual gesture remains.

That covers what no assertion can reach: whether three columns read as three columns, whether a
renamed column is wide enough for its new name, whether a translated dialog clips or shows a raw
key, whether a completed drag looks right, and whether the optional RIMMSQOL and Work Type Tag
interfaces show the asserted state. Each file's header lists what to look for.

**Run the suite once per language** and the same screenshots double as the English and French
display pass that `TRANSLATIONS.md` records separately from the static localization gate.

## What a pass means

**Scenarios 1, 2, 4 and 5 gate publishing.** They cover loading, the button that failed twice, the
feature itself, and the priority safety that is the mod's reason to exist.

3, 6, 7, 8, 9, 10 and 13 are behaviour worth getting right, but a defect there is a patch note
rather than a blocker. 11 and 12 confirm documented behaviour; if they disagree with the
documentation, the documentation is what needs fixing.

## How many passes, and which

**One pass validates nothing here, whatever colour it comes back.** A run proves the mod behaves
that way *in the mod set it ran in*, and this mod's whole subject — the Work tab and its columns —
is territory other mods take over. Three sets are the minimum, and a report must say which one it
is; `Run-PickleWsl.ps1 -DepsMap wsl-deps.<set>.map` writes the name into the report, and a run
without `-DepsMap` is recorded as `sans-facultatifs`. Every map of this suite, the minimal one included (`wsl-deps.sans-facultatifs.map`), stages two development-only PickleTools companions: `ClickDiagnostics` holds the scenario 2 click diagnostics with no Defs, and `ScreenshotMode` owns the shared HUD/runner-free Workshop capture steps for feature 14. With no map at all, scenario 2 and the publication captures cannot be played.

| Set | What only this set can show |
| --- | --- |
| `sans-facultatifs` | That the mod stands up alone: Core, the DLCs, Harmony, RimLogging, Pickle and this mod. It is also the only set whose screenshots are clean enough to publish |
| `avec-better-work-tab` | Scenario 2, and with it 3, 7, 7b and 8. The **Work types…** button failed twice in this mod's history, and 1.0.1's fix was about a mod declaring its own window in the `MainButtonDef` and overriding `DoWindowContents` without calling `base`. A set with no tab replacer cannot see that class of defect at all |
| `avec-enhanced-work-tab` | The third tab mod, and the open question. It intercepted `GetPriority` during an investigation on 2026-09-19, which suggests it patches rather than replaces and would therefore cohabit — but that is an inference, and this set is how it stops being one |
| `incompat-fluffy-worktab` | Whether `Fluffy.WorkTab`, which `About.xml` declares **incompatible**, still is |
| `incompat-compact-worktab` | Coexistence with Compact Work Tab. The filename is historical: direct inspection of its 1.6 assembly showed that it patches the current vanilla columns in place and does not own a competing list, so the incorrect `incompatibleWith` entry was removed on 2026-09-22 |

**The incompatibility set is read green-is-good.** Feature 15 asserts the stable mechanism rather
than expecting an ordinary scenario to fail: after Work Studio creates a runtime type, that type
exists in the live Work table but is absent from `WorkTab.Controller.allColumns`, the startup-time
snapshot Work Tab restores when its window rebuilds. It is tagged `@requires:Fluffy.WorkTab`, so it
is skipped outside that pass. Green means the declaration still describes the installed Work Tab;
red means its implementation changed and `About.xml` must be reviewed.

An `incompatibleWith` ages. This review caught exactly that with Compact Work Tab: its current
1.6 assembly patches `PawnColumnWorker_WorkPriority` in place and recalculates layout from the
current table, while its own documentation explicitly supports mods that add work types. The
declaration was removed instead of inventing a failing symptom. Keep its historical map as a
coexistence pass and replay both sets when the respective other mod moves.

An earlier version of this section listed Fluffy's Work Tab as a compatibility set, which was
wrong: this mod declares it incompatible, and a set named `avec-` asserts the opposite of what
`About.xml` says. Better Work Tab is the genuine coexistence case — scenario 11 documents what it
owns and what this mod keeps.

Scenario 6 remains relevant beside tab modifiers, but it is not the evidence for the Work Tab
declaration. Feature 15 names the column-snapshot conflict directly.

Enhanced Work Tab was left out of an earlier version of this plan on the grounds that a dependency
map asserts compatibility and nobody had verified that one. That was backwards: a map names what
to stage, and the run is what answers whether they cohabit. Refusing to measure for want of
knowing the result is how an inference survives — the set is listed above and the question gets
settled by running it.

**The originals are not sets, because they do not load in 1.6.** Compact Work Tab's original
(`CaptainArbitrary.CompactWorkTab`) lists `supportedVersions` 1.4 only and ships a `1.4/` folder;
the original Work Tab predates 1.6 as well. `About.xml`'s `incompatibleWith` names the maintained
continuations, which are the ones a 1.6 player can actually have active, so the fact that it does
not name the originals is not a gap. An earlier note here and in one map file said the opposite on
the strength of a different `packageId` alone, before anyone had opened the original's own
`About.xml`.

**The language axis is separate, and does not multiply this one.** Tab replacers decide scenarios
2, 3, 7, 7b and 8; the language decides what the `@review` captures read. The useful runs are the
minimal set in each language, plus each of the other three sets once, in whichever language —
crossing the two axes everywhere would buy nothing but machine time, and this machine is shared.

### What the passes said, 2026-09-21

| Set | Result | What it does and does not establish |
| --- | --- | --- |
| `sans-facultatifs`, English | 47 of 47 | The mod stands up alone. The five `@review` scenarios asserted nothing; the captures were read by a person and found clean |
| `sans-facultatifs`, French | 47 of 47 | Same, in French, with the language named in the report; captures read, and the right-hand column was reworked because of what they showed |
| `avec-better-work-tab` | 47 of 47 | Coexistence with Better Work Tab, including scenario 2, the one written for tab replacers |
| `avec-enhanced-work-tab` | 45 of 47 on 2026-09-21; scenario 5 alone **3 of 3 on 2026-09-22** after the fix | The save-load failure is fixed (below); the full 47-scenario set has not been replayed since, and the button overlap, below, is untouched by this fix |
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
guessed. **Confirmed in game, 2026-09-22**: `avec-enhanced-work-tab`, filtered to
`05-priorities-across-save.feature` alone, 3 of 3 scenarios passed, `exitReason: passed`
(`Tests/Pickle/evidence/2026-09-22-scenario5-enhanced/`). The full 47-scenario set has not been
replayed since the fix; that is still owed before this pass counts for `done -> tested`. Reading
another mod's decompiled source to explain a failure, rather than only measuring the outside, is new
for this mod's testing; it is the reason the cause could be named before the scenario was replayed
rather than only after.

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

**The raw reports behind these results no longer exist.** The shared launcher keeps only the fifteen
newest report folders, and with this many sessions queued that is a few hours; the folders for the
runs above had already gone by the evening of 2026-09-21. What survives is what was copied into
this file and into `BACKLOG.md`: notably Pickle's own log line for the Work Tab click, `pointer at
(1058.50, 773.00): the OS reports x:1058 y:773 ... the game reads (1058.00, 774.00)`, and the
numbers of the Enhanced Work Tab failures. Anything a document cites from a report should be copied
into the repository at the time, or its folder given a `keep.txt`, which the launcher never removes.
