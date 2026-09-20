# In-game scenarios, run by Pickle

The scenarios of [TESTING.md](../../TESTING.md), written in Gherkin and played inside a running
RimWorld by [Pickle](https://github.com/RimWorks/Rimworld-Pickle) (`rimworks.pickle`,
Workshop 3791648678).

`Mod/` is a companion mod, **Work Studio - Pickle tests**, never published. It holds the feature
files and the step assembly, so nothing test-related ships in the Workshop folder.

## Setup, once

1. Subscribe to Pickle and RimLogging, and enable both.
2. Link both this repository's `Mod/` and the companion mod into RimWorld's `Mods` folder. The local
   link matters: with the Workshop copy subscribed too, the game names that one
   `nelim.workstudio_steam`, and the steps are built against the local assembly.

   ```powershell
   $mods = "C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods"
   New-Item -ItemType Junction -Path "$mods\WorkStudio" -Target "<repo>\Mod"
   New-Item -ItemType Junction -Path "$mods\WorkStudioPickleTests" -Target "<repo>\Tests\Pickle\Mod"
   ```

3. Enable Work Studio, then the companion mod below it and Pickle.

Pickle patches the game through Concord when Concord is loaded, and through Harmony otherwise. If
Concord fails to start ("Failed to initialize Concord" in the log), none of Pickle's hooks land:
every `I click button` fails with "no tags recorded this frame". Disable Concord for the run.

Pickle tags a button by the text it actually draws. On a non-English client, `I click button
"Work types…"` would silently never find anything, since the drawn label is whatever
`WorkStudio.OpenEditorShort` resolves to in that language (found the same night in Architect
Studio's own suite, on a French client). Scenario 02 uses this mod's own `I click the Work types
button` / `the Work Studio button is not drawn` steps instead (`ModSteps.cs`), which resolve the
same key the button itself draws before building the tag.

Pickle itself later gained a generic equivalent, `I click button keyed {string}` (upstream
[RimWorks/Rimworld-Pickle#19](https://github.com/RimWorks/Rimworld-Pickle/pull/19), 2026-09-17
night, not yet on a released Pickle version at the time this suite was written). Kept the
mod-owned steps above rather than switching: they depend only on `PickleContext.Click`/`.Hover`,
which every Pickle version this suite has ever run against already has, not on a specific build.

## Build

```powershell
dotnet build Tests/Pickle/Source/WorkStudio.PickleSteps.csproj -c Release
```

The output goes to `Mod/Pickle/Assemblies/`. It binds to `Mod/Assemblies/WorkStudio.dll`, so build
the mod first. Feature files need no build.

## Run

- **In game**: dev mode on, debug actions menu, *Pickle*. Tick the Work Studio suite, *Run selected*.
- **Scripted**: `.\Tests\Pickle\Run-Pickle.ps1` drives the game that is already open. It cannot
  start one, on purpose — see below.
- **Unattended**: `RimWorldWin64.exe "-pickle-run=Work Studio - Pickle tests"`. The filter is the
  companion mod's name, exactly. Reports land in `PickleReports` beside the saves. This is also
  what a build made since the running game started needs, since the assemblies are read at startup.

**Starting the game is a person's job, never a script's.** `Run-Pickle.ps1` had a `-Launch` switch
guarded by a `Get-Process` check, and on 2026-09-20 that guard let a launch through against a game
that was still coming up: a game starting is not yet a process, so no check of this kind can see
it. The second instance cut off what the first was doing — the same evening, one killed SkillIcons'
run, which died without writing a report and left nothing in the next one to explain the gap. The
switch was removed rather than hardened. The script now has no `Start-Process` at all: with no game
open it prints the unattended command above and stops.

The machine has one RimWorld and that RimWorld has one Pickle runner, so two sessions starting a
run at the same time destroy each other silently. The script takes
`%LOCALAPPDATA%\rimworld-pickle-run.lock` first, with `CreateNew` so the file system arbitrates,
and gives it back in a `finally`. Start a run any other way and nothing holds that lock.

`PickleReports` likewise holds one report for the whole machine: `summary.md` and `junit.xml` are
rewritten by whichever suite ran last, whatever mod it belonged to. Read the outcome back from
`http://localhost:27750/state`, which names the mod each scenario belongs to — a scenario the
running game has never played reads `Pending` there, and no report can tell you that. The script
also moves the previous report into `PickleReports-archive` before starting, under the hour it was
written, keeping the last `-KeepReports` runs; that is a reprieve, not storage, so copy out what
you need to keep.

The script refuses a run whose assemblies are newer than the running game — they are read at
startup, so an older game tests the previous build while you read the new build's source. It also
reports any `.pickle-backup` left in `Config/` by a scenario that died before restoring the
settings it saved: those are the real settings, waiting to be put back, and they belong to whatever
mod is audited next as much as to this one.

Scenario 02 clicks real buttons through OS input: the pointer moves on its own while it runs.

## What the suite does to your files

- **Settings.** Before each scenario the suite copies `Mod_WorkStudio_WorkStudioMod.xml` to a
  `.pickle-backup` beside it and resets the mod; afterwards it restores the file and reapplies it.
  If the game dies mid-scenario the next scenario restores the backup first. **A `.pickle-backup`
  left in `Config/` with no further run planned must be copied back by hand.**
- **Saves and exports.** Scenario 05 writes saves and scenario 09 writes exports, all prefixed
  `pickle-workstudio-`, deleted after each scenario.
- **The fixture.** Scenarios needing a colony load Pickle's own `test-colony` save. Nothing is
  written back to it.

## How the scenarios reach the mod

The editor keeps its mutations in private methods of `Dialog_WorkTypes` (`CreateType`, `MoveTask`,
`DeleteType`, `Rename`, `SetVisible`, `ShiftType`, `ShiftTask`). The steps call those by name, so
they run the code the buttons run. A renamed method fails the step with its name.

Drags are replayed through the method each column registers with `ReorderableWidget` on its last
repaint, captured by a Harmony postfix, with rows named rather than numbered.

## Why scenario 05 is built the way it is

A created work type is appended at the end of the def database, so creating one shifts no index,
and a test that only creates a type passes with or without the mod's protection. The scenario
creates two, saves, deletes the first and loads the save: without the named priorities, the second
type would read the first one's value.

## Why each of these is in Gherkin and not off-game

Pickle does not replace the off-game suite, it completes it, and it costs incomparably more: a run
takes the machine over — real clicks, real pointer — for minutes, where `Tests/OffGame` answers in
seconds. So a scenario that repeats what a unit test already proves confiscates the machine every
run for nothing, and belongs deleted rather than kept just in case. Every feature here was put
against that rule on 2026-09-20, with the timings of the 20:48 run as the measure of what it costs.

| Feature | Cost | What only a running game can show |
| --- | --- | --- |
| `02` the Work tab button | 55.0 s | Real OS clicks, which go to whichever window owns the point — the failure other mods cause and no unit test reproduces |
| `13` the settings window, both doors | 52.9 s | Vanilla's own `Dialog_ModSettings`, its `PreClose` writing the file, and a configuration re-read from disk |
| `05` priorities across a save | 40.3 s | What survives a save and a reload, with the def database rebuilt in between |
| `03`, `07b`, `10b`, `13b`, `14` | 89.4 s | Screenshots a person reads. They assert nothing by design |
| `04` create and fill | 18.5 s | A def created at runtime, the column rebuilt, and a colonist actually picking the task up |
| `08` hide a column | 17.1 s | The column leaving the drawn table, and the colonist still picking `CleanFilth` up |
| `10` the drift warning | 16.2 s | A dialog opening on a real window stack, once and not twice |
| `12` removing the mod | 8.6 s | Writing a real save, whose raw XML is then the evidence |
| `06` reordering | 1.7 s | Drops replayed through the callback the column registered on its **last repaint**, and arrows acting on a row list rebuilt that frame — the frame-cache defect of 2026-09-18 lived exactly there |
| `09` export, reset, import | 1.5 s | `ConfigFile` reaches `GenFilePaths.SaveDataFolderPath`, which throws outside a Unity player: off-game cannot run this at all |
| `01` loading and patches | 1.2 s | Harmony having actually applied, in a real load order, beside the other mods |
| `07` rename | 0.3 s | The column worker measuring the header width once and keeping it |

One overlap is real and stays: `Tests/OffGame` reproduces TESTING.md scenario 8 against the real
`WorkTypeRuntime.Apply()` and already proves that hiding and showing leave the priority alone, which
`08` asserts too. It is kept because `08` also shows the column leaving the table and the colonist
still picking the work up, which off-game cannot; the two extra `Then` lines ride along in a
scenario that has to run anyway and cost no machine time of their own.

The four cheapest features together come to 4.7 seconds. Deleting them would buy nothing and lose
the frame-cache and load-order checks, so nothing was deleted.

## What stays manual

| TESTING.md | Why |
| --- | --- |
| 3, the column titles | `03` walks the editor there and screenshots it, tagged `@review`; a person reads |
| 6, the pointer starting a drag | Only the drop is replayed |
| 7, the header width after a rename | `07b` screenshots the Work tab, tagged `@review`; a person looks |
| 7, Work Type Tag's current-job label | Not written |
| 10, a real mod list change and restart | The startup check is run on a recorded list instead |
| 10, the dialog's wording | `10b` screenshots it, tagged `@review`; a person reads |
| 11, what Better Work Tab owns | Documented behaviour of another mod |
| 13, RIMMSQOL revealing the button | Another mod's own UI; `13` covers the worker and the window it opens |

## The `@review` features: automated trip, human verdict

`03-three-columns`, `07b-rename-visual`, `10b-drift-warning-wording` and the second scenario of
`13-settings-window` **assert nothing**. They drive the game to the state a manual scenario
describes and call Pickle's own `I take a screenshot` step; the image goes into the report and a
person decides. The pattern is Architect Studio's (`04b-arrows-at-150-percent`), and it buys the
half of a manual test that a machine can do reliably — the trip — without pretending to judge what
only eyes can.

Each of those files opens with the list of what to look for in its screenshots. **Run the suite
once per language** and the same images serve as the English and French display pass.

The same captures are also what the Steam page wants to show, in a clean fixture colony at a
consistent size. After a run:

```powershell
.\Art\Update-WorkshopScreenshots.ps1
```

copies that set into `Art/Workshop/` under names meant for the page rather than for the test that
took them. `Art/` is not shipped, so none of it reaches subscribers, and the files are gitignored
by default — they change every run, and only a set worth publishing belongs in the history
(`git add -f` one when it is).

Pickle's own runner panel used to sit in the corner of every frame, and cropping it by hand was
weighed and accepted on 2026-09-18. It is not needed any more: `I hide the interface around Work
Studio's windows` turns on the game's own screenshot mode, which draws only windows that ask for
it, and clears that flag on Pickle's own — so the tab bar, the alerts, the colonist bar, the dev
tools and the runner panel all go, leaving the mod's window on the map. The step restores what it
changed, and an `[AfterScenario]` restores it again in case a scenario dies in between, which would
otherwise leave the game with no interface at all. Learned from Architect Studio's session,
2026-09-20.

`14-publication-shots.feature` uses it, and builds its own scene rather than photographing whatever
the fixture holds: a screenshot of a test configuration puts names like "Pickle herding" on a store
page. **Run that one with the game in English**; the @review features are the ones to run in each
language.

A screenshot scenario waits *frames*, never ticks: the settings window sets `forcePause`, and a
tick-based wait behind it would sit until its timeout.

**12, removing the mod, is covered a different way.** A companion mod bound to Work Studio's own
assembly cannot script "and now the mod is gone" from inside itself, so `12-removing-the-mod.feature`
does not restart RimWorld without it. It saves a colony with a custom type in place, then reads
the raw `.rws` XML directly and proves the fact TESTING.md's claim rests on: custom types are
always appended at the end of `DefDatabase<WorkTypeDef>` (see "Why scenario 05 is built the way it
is" above), so a mod-less database is this same list with its tail cut off, and vanilla's own
positional `DefMap` loading would read the same leading values into the same leading types either
way — only the trailing, custom-type values would have nowhere to go. What still needs an actual
restart without the mod: confirming the game tolerates the now-unread `<workStudioPriorities>`
node without complaint, and that moved tasks really do read from their XML-declared `workType`
once nothing overrides it at runtime.
