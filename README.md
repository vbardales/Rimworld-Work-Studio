# Work Studio

A work type editor for RimWorld 1.6, applied **live**: create your own work types, move tasks
into them from other types, rename them, hide a column. No restart, no def file to write.

Drag and drop acts on the game's two orders, which have nothing to do with each other:

- **the left column** reorders the work types by reassigning their `naturalPriority` — that is
  both the order of the Work tab columns and the order in which a colonist moves from one job to
  the next;
- **the task list of a type** reorders `WorkGiverDef.priorityInType` — the order in which a
  colonist picks up tasks once already busy with that job.

In both cases the values already in place are redistributed rather than a new scale invented, so
that a mod loaded later can still slot into the middle of the list.

Every row also carries up/down arrows that move it one step. Drag and drop is awkward on a Steam
Deck trackpad, and the arrows stay visible — greyed out — at the ends of a list: a button that
disappeared would shift every other one under your finger.

Open it from the **Work types…** button at the top right of the Work tab, or from the mod
settings.

## How it works

Defs are not replayed: they are mutated in memory and the game is then forced to reindex.
`WorkTypeRuntime.Apply()` is a full, idempotent reconciliation — the result depends only on the
configuration, never on the order in which changes were made.

The delicate part is not creating a `WorkTypeDef`, it is not scrambling colonist priorities.
`DefMap<D,V>` — the structure behind `Pawn_WorkSettings.priorities` — stores no keys: it is a
`List<V>` indexed by `def.index`, serialised as such. Adding, removing or reordering a single
work type would therefore shift the priorities of every type after it, for every pawn in the
game, and nobody would notice until they found a doctor down a mine shaft. Hence two safeguards:

- `PriorityMemory` captures priorities **by defName** before each reconfiguration and puts them
  back against the right type afterwards. A new type inherits the priority of the type its tasks
  were taken from: splitting a job in two changes nothing about what the colonist does.
- `Patch_WorkSettingsExposeData` writes those same priorities, named, into the save. The
  configuration can therefore change between two sessions without anything being scrambled.

## When the mod list changes

An assignment points at a `defName`, never at an object: removing a mod makes the task disappear,
not the setting. The assignment stays in reserve and resumes on its own if the mod comes back. A
work type target that cannot be found falls back to the original type, with a warning in the log.

The genuinely dangerous case is elsewhere, and it does not come from this mod: adding or removing
any mod that declares a `WorkTypeDef` shifts the priority `DefMap` of the whole colony.
`Patch_WorkSettingsExposeData` writes those priorities by name into the save and puts them back
against the right job on load, which protects vanilla and other mods' work types too. A type that
appeared since the save starts switched off, as it would without this mod.

`ConfigDrift` compares the def landscape with the one from the previous startup and reports it
once — types gone, tasks not found, types added. The warning does not come back while nothing
moves, and does not show at all without a configuration.

## Import and export

From the mod settings. The files live in `WorkStudio/`, next to `Saves` and `Config` in the
game's save data.

An export carries only the portable part of the settings: custom types, task assignments, orders,
renames, hidden columns. It leaves out `knownWorkTypes`, the snapshot of the mod list used to
detect changes — sharing that would raise a false alarm on somebody else's first import.

An import reads the file into a fresh settings object first and adopts it only once it has been
read in full. A truncated file therefore does not leave the configuration half replaced, which
would be worse than importing nothing.

## What this mod does not do

- **It does not create new tasks.** A custom work type is a container: it has to be given
  existing `WorkGiverDef`s, taken from other types.
- **A mixed type is more restrictive than its parts.** A type fed from both Doctor and Handling
  is disabled for a colonist incapable of one *or* the other. That is the only choice that never
  lets a colonist do work the game had ruled out for them.
- **Other Work tab replacements are not supported.** `Fluffy.WorkTab` and `Mlie.CompactWorkTab`
  rebuild the Work tab columns as well, so the two would fight over them. Both are declared in
  `incompatibleWith`, so the game warns on its own. Achtung! works around the problem through
  reflection on `WorkTab.Controller`; that was not reused here for lack of a way to test it.
- **Hidden types stay active.** Hiding removes the column, not the job. To stop a job, set its
  priority to zero as usual.

## With Better Work Tab

Better Work Tab replaces the Work tab window and puts a prefix returning `false` on
`Pawn_WorkSettings.CacheWorkGiversInOrder`, rebuilding the execution order from its own saved
column order — a list of defNames held in a `GameComponent`, so per save. Its comparator falls
back to `naturalPriority` only when two types share a colonist priority *and* the same position
in that list:

```csharp
indexA = indexMap[a.defName] ?? int.MaxValue;
if (indexA != indexB) return indexA.CompareTo(indexB);
return b.naturalPriority.CompareTo(a.naturalPriority);
```

Three consequences. A type Better Work Tab already lists is ordered by it, and reordering here
does nothing. A save where it has recorded nothing yet falls through to `naturalPriority`, so
this mod drives the order normally. And a type just created here is absent from that list, hence
`int.MaxValue`: it sorts **last** until it is positioned in Better Work Tab.

Everything else composes: the two mods both read `PawnTableDefOf.Work`, so created types, moved
tasks, `priorityInType` ordering, renames and hidden columns all come through.

## Uninstalling

Removing the mod gives moved tasks their original types back. Custom types go with it: the
priorities colonists had on them are lost, every other job's priorities are kept.

## Repository layout

```
Mod/       published to the Workshop; target of the junction into RimWorld/Mods
Source/    never published
.build/    build intermediates, ignored by git
```

The Workshop uploader sends the mod folder as it stands, with no filtering — `SteamUGC.SetItemContent`
takes the root directory and nothing else. Keeping the sources out of `Mod/` is the only way not
to publish them, and `Source/Directory.Build.props` keeps `obj/` out too: without it, the
publicised `Assembly-CSharp.dll` it contains — about 6 MB — would ship to every subscriber.

## Build

    dotnet build Source/WorkStudio.csproj -c Release

The assembly lands in `Mod/Assemblies/`. Reference assemblies come from NuGet
(`Krafs.Rimworld.Ref`), so no RimWorld installation is needed to compile.

See `ATTRIBUTION.md` for what comes from Achtung! (MIT), for what differs from it, and for who
pointed at the technique in the first place.
