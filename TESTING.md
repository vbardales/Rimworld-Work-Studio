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

## 8 — Hide a column

**Proves** that hiding removes the column and nothing else.

Untick a type in the left column. Its column leaves the Work tab. The colonists **keep doing that
work** at the priority they had — hiding is not disabling. Re-tick it and the column returns with
the priorities intact.

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

## Automated off game

`Tests/OffGame/` instances the shipped `Mod/Assemblies/WorkStudio.dll` against the installed
game's own `Assembly-CSharp.dll`, no RimWorld process involved. It found and fixed a real defect
before it ever shipped: the Publicizer/`GenerateAssemblyInfo=false` combination that silently drops
the private-member access waiver, which would have thrown `FieldAccessException` on
`PriorityMemory.Restore`'s first call — every startup, every edit. See `Tests/OffGame/RESULTS-*.md`
for what is covered and what still needs the game (`ConfigFile`'s own file paths, chiefly).

```powershell
dotnet build Source/WorkStudio.csproj -c Release
dotnet build Tests/OffGame/WorkStudio.Tests.csproj -c Release
.build/offgame/bin/Release/net48/WorkStudio.Tests.exe
```

## Automated in game: Pickle

Scenarios 1, 2, 4 to 10, and half of 12 (the raw-save half, not an actual restart) are also written
in Gherkin under `Tests/Pickle/`, played inside RimWorld by the Pickle test mod. Its README says
how to run them and which parts stay manual. Not run yet.

## What a pass means

**Scenarios 1, 2, 4 and 5 gate publishing.** They cover loading, the button that failed twice, the
feature itself, and the priority safety that is the mod's reason to exist.

3, 6, 7, 8, 9 and 10 are behaviour worth getting right, but a defect there is a patch note rather
than a blocker. 11 and 12 confirm documented behaviour; if they disagree with the documentation,
the documentation is what needs fixing.
