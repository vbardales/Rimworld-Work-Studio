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
- The column **width** follows the new name. The column worker measures the label once and caches
  it, so a stale width means the worker was not rebuilt.
- If **[baku] Work Type Tag** is installed, the name in front of a colonist's current job changes
  too, without a restart. That one is a cache this mod clears by reflection.

  The cache-clearing itself is proven off-game, against the real third-party assembly, by
  `Tests/OffGame`'s `TheWorkTypeTagCompat` check (skips cleanly if that Workshop item is not
  installed on the machine running the suite). What still needs the game: seeing the label actually
  redraw in front of a moving colonist.

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

**What stays manual is RIMMSQOL itself**: that it lists this def, that revealing it puts a real
button on the bar, and that the button then opens the same window. No test can reach that.

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
`@review` (`03-three-columns`, `07b-rename-visual`, `10b-drift-warning-wording`, `13-settings-window`)
**assert nothing**. They walk the game to the state a manual scenario describes and attach a
screenshot to the report; a person looks at it and decides. The trip is automated and reproducible,
the judgement stays human.

That covers what no assertion can reach: whether three columns read as three columns, whether a
renamed column is wide enough for its new name, whether a translated dialog clips or shows a raw
key. Each file's header lists what to look for in its screenshots.

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
without `-DepsMap` is recorded as `sans-facultatifs`.

| Set | What only this set can show |
| --- | --- |
| `sans-facultatifs` | That the mod stands up alone: Core, the DLCs, Harmony, RimLogging, Pickle and this mod. It is also the only set whose screenshots are clean enough to publish |
| `avec-better-work-tab` | Scenario 2, and with it 3, 7, 7b and 8. The **Work types…** button failed twice in this mod's history, and 1.0.1's fix was about a mod declaring its own window in the `MainButtonDef` and overriding `DoWindowContents` without calling `base`. A set with no tab replacer cannot see that class of defect at all |
| `avec-enhanced-work-tab` | The third tab mod, and the open question. It intercepted `GetPriority` during an investigation on 2026-09-19, which suggests it patches rather than replaces and would therefore cohabit — but that is an inference, and this set is how it stops being one |
| `incompat-fluffy-worktab` | Whether `Fluffy.WorkTab`, which `About.xml` declares **incompatible**, still is |
| `incompat-compact-worktab` | The same for `Mlie.CompactWorkTab`, the other declared incompatibility |

**An incompatibility set is still read green-is-good.** The tempting design is to let the suite go
red and call that the measurement, and it was written that way here for an hour. It does not work:
an expected red and an accidental red are the same colour, so nobody can tell which one they are
looking at, and a suite that needs explaining is a suite nobody reads. What these sets assert is
the **documented symptom** — the window that opens is not this mod's, or the error the conflict
produces is logged — with `@allow-errors` on the scenario so the expected error does not fail it
by itself. Green then means the incompatibility is still exactly what `About.xml` claims; red means
something moved, and that is when someone looks.

That is not hypothetical here. 1.0.1's fix made the button patch target whichever window class is
actually in use and take the rectangle by position rather than by name. If that turned out general
enough, one of these two declared conflicts may have quietly stopped being one — and only a
scenario written to go green can say so with a red that means something.

An `incompatibleWith` ages. The other mod can be fixed, rewritten, or simply stop patching what it
patched, and an incompatibility never replayed ends up forbidding a coexistence that would work,
depriving players of both mods for nothing. These two sets are replayed when **the other mod**
moves, not when this one is published: it is their update that stales the verdict, not ours.

The scenarios that assert those symptoms are **not written yet**. The first runs of these two sets,
on 2026-09-21, play the ordinary suite instead — which is exploration, not validation: it says
which scenarios break and how, and that is the material the symptom assertions are written from.
Until they exist, a red in one of these reports means "something happened here", nothing more.

An earlier version of this section listed Fluffy's Work Tab as a compatibility set, which was
wrong: this mod declares it incompatible, and a set named `avec-` asserts the opposite of what
`About.xml` says. Better Work Tab is the genuine coexistence case — scenario 11 documents what it
owns and what this mod keeps.

Scenario 6 is the one to watch when those reports are put side by side. Its own header already
says that with Fluffy's Work Tab or Better Work Tab the type order may not reach execution once a
column has been dragged there. That sentence has never been measured — it is a caveat someone
wrote, not a result. Two of these sets turn it into one.

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
| `avec-enhanced-work-tab` | **45 of 47** | Two failures, below |
| `incompat-fluffy-worktab` | **45 of 47** | Exploration only: the symptom scenarios do not exist yet. The button scenario failed because Pickle aimed 773 px left of the drawn button (its own log says so), which says nothing against the button; the other failure is a save-load exception whose cause is not established |
| `incompat-compact-worktab` | 47 of 47 | Says nothing about the declared conflict: these 47 scenarios never meet it |

**With Enhanced Work Tab loaded, two things fail, and they are different in kind.**

*The save scenario is a finding about the mod's purpose.* "A save written with one type, loaded with
two" asserts that a type absent from the save reads priority 0, and it read **2**, while the raw
`DefMap` value was **0** — Work Studio's own restore did its job, and something else answered
`GetPriority`. That matches what was seen on 2026-09-19, when this mod was found to intercept
`GetPriority`. What is not yet known is where its answer comes from, and therefore whether Work
Studio's protection can reach it; scenario 5 gates publishing, so this is the one to settle first.

*The button scenario is a measured overlap, and the cause is not yet known to be Enhanced Work Tab's.*
The tab window is vanilla `MainTabWindow_Work` here, so this mod **patches** the tab and does not
replace it. Pickle clicked at exactly the centre of the drawn button, (1389, 707) for a centre of
(1389.5, 706), so the rectangle was right. A second `Widgets.ButtonText`, 98x24 at (1362, 697),
drawn before ours (#64 against #103) also contains that point. Its label was not printed, so which
control it is is not established; a text button drawn earlier is offered the click first.

**An earlier explanation of the Work Tab failure was wrong and is withdrawn, twice.** First it said
the click was taken by one of that mod's image toggles, worked out from the layout; the measured
pointer was at (1058, 774), nowhere near the button. Then it said something moved the pointer after
the click; Pickle's own log shows it AIMED at (1058.5, 773) and the OS and game agree it got there.
What is established: with Work Tab loaded, the rectangle Pickle stored for the tag is 773 px to the
left of where the button is drawn (centre 1831.5, confirmed by the screenshot), so the click went to
an empty spot. The button is not shown to be broken; the scenario failed on where it was aimed. Why
the stored rectangle is off is not known.

**The raw reports behind these results no longer exist.** The shared launcher keeps only the fifteen
newest report folders, and with this many sessions queued that is a few hours; the folders for the
runs above had already gone by the evening of 2026-09-21. What survives is what was copied into
this file and into `BACKLOG.md`: notably Pickle's own log line for the Work Tab click, `pointer at
(1058.50, 773.00): the OS reports x:1058 y:773 ... the game reads (1058.00, 774.00)`, and the
numbers of the Enhanced Work Tab failures. Anything a document cites from a report should be copied
into the repository at the time, or its folder given a `keep.txt`, which the launcher never removes.
