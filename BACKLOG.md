# Backlog

Ideas for this mod that are not started. Each entry says what the feature would be, what already
covers part of it, and what has to be settled before the first line of code.

An idea earns a place here if it **serves what the mod already does**: Work Studio edits work
types, so anything that makes a work type easier to create, recognise or steer belongs here. The
monorepo backlog keeps the stricter rule — a new mod has to change what pawns do — because a new
mod has no existing purpose to serve. Anything already shipped is in the changelog instead, and
anything that needs checking in play is in `TESTING.md`.

---

## An icon and a colour for each work type

Proposed 2026-09-11 in the Work Studio thread, as the 1.1.0; written down 2026-09-17, when
[baku] Work Type Tag came up again and the plan turned out to exist only in that conversation.

**What the feature would be.** In the sheet of a work type, a player picks **one icon and one
colour**. Work Studio does not draw them itself wherever another mod already does: it hands them
to each mod that is installed, through a soft link — reflection, no dependency, silent when the
mod is absent. The same pattern as `WorkTypeTagCompat`, which already ships.

The icon and colour then show up in five places, each owned by whichever mod draws there.

| Where | Owner | Entry point, as read in its assembly |
|---|---|---|
| In front of the current job | **[baku] Work Type Tag** (3779138895) | Already linked in 1.0.1. It derives a colour from the `defName`; its per-type RGB setting is what we would write. |
| Marker above the pawn, on the map | **Busywork** (3775253009), on **Useful Marks** (3506573327), both by Andromeda | `MarkerProvider.WorkMarkers`, a public static `Dictionary<WorkTypeDef, MarkerSettings>`. 104 icons in `Textures/Marks/`: 81 from Useful Marks, 23 from Busywork. |
| Colonist bar portrait | **Busywork** | A postfix on `ColonistBarColonistDrawer.DrawColonist` draws `GetMarkerFor(colonist)` in the portrait's top-left corner — the same work marker, read 2026-09-17. Only with its default "upper" alignment, see below. |
| Work tab column header | **GrimWorks: Work Manager** (3761759348) | `SetCategoryOverride(WorkTypeDef, GW_WorkManager_WorkTypeCategory)`, public static. It colours by **category** and named palette, not by type: we would pick a category, not a colour. |
| Right-click action menu | nobody | Our own patch, see below. |

### What was verified for Busywork

- **Short cache.** `PawnMarkerCache` keeps a result 100 ticks, so a marker written mid-game shows
  up within two seconds of play. No invalidation call to find, unlike Work Type Tag.
- **It does not overwrite us.** `ApplyXmlMarkers` skips any `WorkTypeDef` already in the
  dictionary and honours `ClearedWorkDefs`, what the player cleared by hand. The player's own
  choice stays on top.
- **It runs before us.** Its `Bootstrap` is `[StaticConstructorOnStartup]`, ahead of our
  `LongEventHandler.ExecuteWhenFinished`.
- **The trap.** `WorkTypeDef.GetHashCode` combines `defName` and `gerundLabel`, which is mutable.
  An entry inserted before `gerundLabel` is set becomes unreachable. Write at the end of
  `Apply()`, after `SyncCustomTypes` — the same hazard as `InheritedDisabling`.
- `MarkerSettings` comes from Useful Marks, constructor `(string icon, Color color, int,
  MarkerConditionNode)`, callable by reflection.

### What was verified for the colonist bar, and what it costs

Read 2026-09-17 in `Busywork.dll` and `UsefulColonistBar.dll`.

- **Busywork draws the work marker on the portrait itself.** `Patch_ColonistBarColonistDrawer_DrawColonist`
  is a postfix that calls `MarkerProvider.GetMarkerFor(colonist)` and draws it at the top-left
  of the portrait. On the map, a prefix on Useful Marks' `DrawMarks` draws the same marker above
  the head, and skips when Useful Marks flags that it is drawing at the colonist bar.
- **Only in "upper" alignment.** Both draws require `MarkerProvider.GlobalAlignment == 2`, the
  default. In a side alignment the marker is handed to Useful Marks' own marker list instead, and
  whether Useful Marks then shows it on the bar was not read.
- **The work type is the weakest match.** `ComputeMarker` starts from
  `CurJob.workGiverDef?.workType`, then lets a `JobDef` marker, the joy marker and, for a bill, a
  skill marker override it in turn. A custom type made of bills can show its skill's marker
  instead of ours. Nothing to fix on our side; say it in the sheet.
- **Only jobs that came from a work giver.** No `workGiverDef`, no work type, no marker — the
  same limit as the action menu. Hidden while drafted, by Busywork's own setting.
- **Two calls after writing.** Busywork tints player-forced jobs yellow and applies its default
  colour only to markers in its `OwnMarkers` set, which `RefreshOwners()` rebuilds from
  `WorkMarkers`. An entry we add is not in it until that runs: call `RefreshOwners()`, and set
  `atSide` to `GlobalAlignment` — `ApplyGlobalAlignmentToAll()` does it for everyone, and runs
  again on each game load.

### The action menu, which no mod covers

Read in the 1.6 assembly, not assumed. `FloatMenuOption` already draws a tinted icon: a private
`iconTex` and a public `iconColor`, applied to `GUI.color` when drawn. Orders that come from work
("Prioritize repairing…") are built by
`FloatMenuOptionProvider_WorkGivers.GetWorkGiverOption(pawn, workGiver, target, context)`, which
reads `workGiver.workType` itself. A postfix there knows the work type and can set both fields on
the option it returns; `iconTex` needs `AccessTools`.

Limit: only those options carry a work type. Equip, eat, rescue, shoot come from no work giver and
stay bare.

### Before starting

1. **Where the icons come from without Busywork.** The picker lists the 104 marks when Useful
   Marks is present. Without it, the action menu still needs a texture: ship a small set, or
   offer only a colour.
2. **GrimWorks' categories are not colours.** Decide whether the header link maps our colour to
   the nearest category, or asks the player to pick a category as a separate field.
3. **Vanilla types too, or only custom ones.** The 1.1.0 plan was the custom-type sheet. Letting a
   player restyle Construction is the same code and a larger save footprint.
4. **Side alignment on the colonist bar.** Read how Useful Marks draws its own marker list, to
   know whether the bar keeps our icon when a player moves Busywork's markers to the side.
5. **Equivalence groups in the action menu.** `GetOptions` merges equivalent work-giver options
   into one; check in play that the surviving option keeps its icon.
6. **One mod or several.** Considered 2026-09-17: a style core with one display mod per place.
   The mods above already split it that way, so Work Studio stays the one place the player
   chooses, and the action menu patch lives in Work Studio unless it proves to conflict with a
   float-menu mod (Useful Marks ships `FloatSubMenu.dll`).
