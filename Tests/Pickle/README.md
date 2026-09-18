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
- **Unattended**: `RimWorldWin64.exe "-pickle-run=Work Studio - Pickle tests"`. The filter is the
  companion mod's name, exactly. Reports land in `PickleReports` beside the saves.

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
(`git add -f` one when it is). **They are not publication-ready as they come out**: Pickle's own
runner panel sits in the corner of every frame and has to be cropped out.

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
