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
| In front of the current job | **[baku] Work Type Tag** (3779138895) | Linked in 1.0.1 for its label cache only. Its per-type RGB record is reachable, through one internal field, see below. Colour only — it has no icon. |
| Marker above the pawn, on the map | **Busywork** (3775253009), on **Useful Marks** (3506573327), both by Andromeda | `MarkerProvider.WorkMarkers`, a public static `Dictionary<WorkTypeDef, MarkerSettings>`. 104 icons in `Textures/Marks/`: 81 from Useful Marks, 23 from Busywork. |
| Colonist bar portrait | **Busywork** | A postfix on `ColonistBarColonistDrawer.DrawColonist` draws `GetMarkerFor(colonist)` in the portrait's top-left corner — the same work marker, read 2026-09-17. In side alignments Useful Marks draws it instead, see below. |
| Work tab column header | **GrimWorks: Work Manager** (3761759348) | `WorkTypeCategoryUtility.SetCategoryOverride(WorkTypeDef, GW_WorkManager_WorkTypeCategory)`, public static. It colours by **category** and named palette, not by type: a category also moves the column. **Not linked**, see the rule below. |
| Right-click action menu | nobody | Our own patch, see below. |

### When to link another mod, when to write our own code

Agreed 2026-09-17, from the four assemblies read below. One question per place: who already draws
there, and does it store what we store?

1. **A mod draws there and stores one icon or colour per work type, like us.** Hand it our choice
   through a soft link, and write no drawing code. Without the mod, nothing shows in that place:
   redrawing it would mean maintaining a worse copy of someone else's mod.
   - Map marker and colonist bar: **Busywork**, through `WorkMarkers`.
   - Job tag and header label colour: **Work Type Tag**, through its RGB record.
2. **A mod draws there but stores something else.** No link, and no code of our own either.
   Translating our choice into its model would distort both, and drawing beside it would double
   the place.
   - Header plate: **GrimWorks**. Its "colour" is a category, which also moves the column, and its
     own header menu already sets it for our types.
3. **Nobody draws there.** Write our own code.
   - Action menu, the only such place today.

Two rules on top:

- **Never a dependency.** Every link is silent when the mod is absent, and switches itself off
  when a signature changes, as `WorkTypeTagCompat` already does.
- **Never two drawings in one place.** If we ever draw a place a mod also covers, our code stands
  down as soon as that mod is detected.

**Who has the last word.** Work Studio writes to another mod only when the player edits the type
sheet, never on every `Apply()`. A choice made in Busywork's or Work Type Tag's own screen is
overwritten only by an explicit choice made in Work Studio.

**What it means for the icon.** Without Useful Marks, only our action menu would show an icon. So
without it the sheet offers a colour alone, and the action menu shows a dot in that colour.

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
- `MarkerSettings` comes from Useful Marks. Its constructor is
  `(string iconName, Color col, string desc, int side, MarkerConditionNode condition = null)` —
  read 2026-09-17; the 2026-09-11 reading had dropped the `desc` string. Callable by reflection.

### What was verified for the colonist bar, and what it costs

Read 2026-09-17 in `Busywork.dll` and `UsefulColonistBar.dll`.

- **Busywork draws the work marker on the portrait itself.** `Patch_ColonistBarColonistDrawer_DrawColonist`
  is a postfix that calls `MarkerProvider.GetMarkerFor(colonist)` and draws it at the top-left
  of the portrait. On the map, a prefix on Useful Marks' `DrawMarks` draws the same marker above
  the head, and skips when Useful Marks flags that it is drawing at the colonist bar.
- **In every alignment, by two different paths.** Busywork's own two draws require
  `MarkerProvider.GlobalAlignment == 2` ("upper", the default). In any other alignment,
  `MarkerProvider.Process` adds the marker to Useful Marks' list instead, and Useful Marks draws
  that list on the bar too. The chain, each link read:
  - vanilla `ColonistBarColonistDrawer.DrawColonist` calls the `Vector2` overload of
    `GenMapUI.DrawPawnLabel`, which forwards to the `Rect` overload;
  - Useful Marks raises `DrawingAtColonistBar` around `DrawColonist` and postfixes that `Rect`
    overload; with the flag up and `Settings.DrawMarksOnColonistBar` (default true) it calls
    `DrawMarks`;
  - `DrawMarks` calls `ProcessMarkersFor(pawn)`, which runs every `IMarkerProvider.Process`
    after the player's own markers — so ours is always in the list, never cut by Useful Marks'
    per-side priority, which only filters its automatic markers;
  - it places `atSide >= 1` on the right of the label, `<= -1` on the left, `0` underneath,
    two columns wide.
- **`showMode` filters the bar.** `ColonistBarAndWorld`, `ColonistBarOnly`, `WorldOnly`. The
  constructor does not set it, so it keeps the enum's first value, `ColonistBarAndWorld`: our
  marker shows on both unless we set it. A per-place choice would live here.
- **The player can turn the bar off.** `DrawMarksOnColonistBar` only governs the side path; the
  upper path is Busywork's own postfix and ignores it.
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

### What was verified for Work Type Tag

Read 2026-09-17 in `baku.WorkTypeTag.dll` (Workshop copy; its `BUILD_REQUIRED.txt` names source
version alpha1g1).

- **One internal door, then a public API.** `WorkTypeTagMod` is public but its `Settings` field is
  `internal static`, so reflection is needed to reach it. Behind it, `WorkTypeTagSettings` and
  `WorkTypeColorRecord` are public: `GetOrCreate(groupDefName)` returns the record, whose `red`,
  `green` and `blue` are public ints, and `NotifyGroupChanged(groupDefName)` clamps them and drops
  that group from the presentation cache. The resolver itself, and `InvalidateAll` that 1.0.1
  already calls, are internal.
- **Keyed by `defName`, as a string.** Rename-safe. The key is a *group*: Complex Jobs' split types
  fold into their vanilla parent through a fixed table, and anything else, ours included, is its
  own group.
- **An unknown type already gets a colour.** Not configured, it takes a hue hashed from the
  `defName` (FNV-1a, fixed saturation and value) — stable, but nobody chose it.
- **It is a setting, not save data.** Records live in its `ModSettings`, per install. Nothing in
  the assembly calls `Write()`: a record we set is only saved if we call it, or when the player
  next closes its settings window. Its "reset all" clears ours, and records for types deleted in
  Work Studio are never pruned.
- **RGB, no alpha, no icon.** The tag is the text `[Label]`, from `labelShort` or else `label`,
  wrapped in a `<color>` rich-text tag. Our custom types need a sensible `labelShort`.
- **Same job limit as the others.** The tag reads `job.workGiverDef?.workType`, plus two hardcoded
  continuations (childcare, cleaning). Jobs from no work giver get no tag.
- **Where it draws.** Postfixes on `Pawn.GetJobReport` and `Job.GetReport(Pawn)` for the current
  and queued jobs, and a transpiler on `PawnColumnWorker_WorkPriority.DoHeader` that tints the
  header **label text**. GrimWorks patches the same `DoHeader` to draw its coloured **plate**
  behind it: the two may stack, one colouring the text, the other the background. Not seen in
  play.
- **The player sets it in its settings window**, one group at a time. Writing from Work Studio
  overwrites that choice, the same question as GrimWorks.

### What was verified for GrimWorks

Read 2026-09-17 in `GW_WorkManager.dll`.

- **The entry point exists as described.** `GW_WorkManager.WorkTypeCategoryUtility` is a public
  static class; `CategoryFor` returns the player's override if there is one, else
  `AutomaticCategoryFor`. `SetCategoryOverride`, `ClearCategoryOverride` and
  `HasCategoryOverride` are public. The category is an enum, so reflection has to build the value
  with `Enum.Parse` on GrimWorks' own type.
- **Keyed by `defName`, as a string.** No hash trap here, unlike Busywork, and a rename in Work
  Studio keeps the override.
- **A custom type starts grey.** The automatic category is a fixed table of vanilla and GrimWorks
  `defName`s; anything else is `Unsupported`, the eleventh colour of the palette.
- **The colour is not ours to give.** `ColorFor(category)` indexes one of eight palettes of eleven
  colours, chosen by the player. We can pick one of eleven categories, never an RGB.
- **A category is more than a colour.** It is also the group a column collapses into
  (`GW_WorkManager_WorkCategoryCollapseUtility`), and where GrimWorks inserts a new work type in
  its own order: after the last column of the same category (`InsertNewWorkTypeIntoCategory`).
  Choosing a category for its colour moves the column, which is Work Studio's job too.
- **It is a setting, not save data.** Overrides live in GrimWorks' `ModSettings`, per install, and
  every `SetCategoryOverride` call writes the settings file to disk. Its header reset clears them
  all, ours included.
- **The player can already do it.** GrimWorks' own header menu (`GW_WorkManager_WorkTypeColorMenu`)
  lists the eleven categories for any work type, ours included, since it only needs a `defName`.
  Writing from Work Studio would overwrite that choice.

### The action menu, which no mod covers

Read in the 1.6 assembly, not assumed. `FloatMenuOption` already draws a tinted icon: a private
`iconTex` and a public `iconColor`, applied to `GUI.color` when drawn. Orders that come from work
("Prioritize repairing…") are built by
`FloatMenuOptionProvider_WorkGivers.GetWorkGiverOption(pawn, workGiver, target, context)`, which
reads `workGiver.workType` itself. A postfix there knows the work type and can set both fields on
the option it returns; `iconTex` needs `AccessTools`.

Limit: only those options carry a work type. Equip, eat, rescue, shoot come from no work giver and
stay bare.

Read in full 2026-09-17, and the simple postfix is **not enough**:

- **Grouped options come back `null`.** When the target is a thing and the work giver has an
  `equivalenceGroup`, `GetWorkGiverOption` stores the option in the private static array
  `equivalenceGroupTempStorage[group.index]` and returns `null`; `GetOptionsFor` yields the array
  afterwards. A postfix reading `__result` never sees those options. The fix stays small: a prefix
  keeps the slot's current value, and the postfix, when `__result` is null, checks whether the
  slot now holds a different option — the one just built for this work giver — and decorates it.
  A later giver of the same group can replace it (a disabled option yields to an enabled one), and
  the replacement is decorated with its own work type the same way.
- **Duplicates are dropped by label.** `tmpUsedLabels` discards an option whose label another giver
  already produced. Whichever giver wins, the icon follows its work type; nothing to handle.
- **Disabled options too.** Everything that reaches the end of the method gets through, including
  "Cannot …: not assigned to …". **Settled 2026-09-17: they carry the icon too** — it names the
  work type the player has to enable. So the postfix decorates every non-null option, with no
  test on `Disabled`.
- **The size is measured after us.** `FloatMenu` calls `SetSizeMode` on every option when it is
  built, and `IconOffset` counts `iconTex` then. Setting the field in a postfix reserves the icon's
  width; no need to go through a constructor.
- **`DecoratePrioritizedTask` keeps the same object.** It edits the label and returns the option it
  was given, so the reference we decorate is the one shown.
- **Drawing.** `DoGUI` multiplies `iconColor` into `GUI.color` and draws `iconTex` with
  `Widgets.DrawTextureFitted`, left-justified by default. A white texture takes the tint cleanly —
  which suits the colour dot, and means Useful Marks' coloured icons should be drawn with
  `iconColor` white.
- **No other vanilla path gives orders from work.** The 53 `FloatMenuOptionProvider_*` classes
  include specialised ones (rescue, arrest, clean room, extinguish fires, childcare, drafted
  repair and tend); their options carry no `WorkGiverDef`. Only `_WorkGivers` is in scope, with
  one borderline case: `_ExtinguishFires` ("Extinguish fires nearby") builds its option from
  `WorkGiverDefOf.FightFires` and `WorkTypeDefOf.Firefighter` by name. Firefighter's icon could go
  there with a second, one-line postfix — a vanilla type, so only if question 3 opens them.
- **Useful Marks' `FloatSubMenu.dll`** patches `FloatMenu.UpdateBaseColor` and `GenUI.DistFromRect`
  only: it does not rebuild options, so it keeps our icon.

**Other mods on the same method.** Every installed DLL was searched on 2026-09-17, in binary mode,
for the method name in ASCII and UTF-16 (the first search, through `scripts/Search-Workshop.sh`,
reported zero because ripgrep silently skips binaries when it walks a folder). Twelve shipped mods
patch `GetWorkGiverOption`; the rest of the hits were copies of the game's own assembly.

- **Prefixes that return `null` and skip the method** — LarvaParasite, VEF (animal behaviours),
  Mosquito Animal Work, Toddlers (with DBH), Hauler's Dream. Postfixes still run and see `null`:
  nothing to decorate, nothing to break.
- **Transpilers that change text or priority checks** — Prioritize Research, Assignment Overrides,
  More Than Capable (two). They leave the returned option alone.
- **A prefix with a finalizer** — Stop Farming It's Enough, state only.
- **Postfixes that replace the option.** Children, School and Learning (both the same code: a new
  "Cannot study…" option on a school table) and **Better Work Tab**. So our postfix must carry
  `[HarmonyPriority(Priority.Last)]`, to decorate the option that is actually shown.
- **Better Work Tab goes further**, and Work Studio already supports it. When the pawn is not
  assigned to the work type, its postfix returns a new "do once" option and pushes two more into
  its public static list `Patch_FloatMenuOptionProvider_WorkGivers_GetWorkGiverOptionFor.AdditionalOptions`:
  the original disabled option and an "assign work" option. A postfix on
  `GetWorkGiversOptionsFor` yields that list and clears it. Running last, ours sees the "do once"
  option only. The other two carry the same work type and can be decorated from that list — read
  by reflection right after BWT fills it, in the same postfix.

  **The list, read in full 2026-09-17:**
  - `public static readonly List<FloatMenuOption>` on a `public static class`, full name
    `Better_Work_Tab.Patches.Patch_FloatMenuOptionProvider_WorkGivers_GetWorkGiverOptionFor`.
    `AccessTools.TypeByName` and `AccessTools.Field` reach it without any access trick.
  - **Only one writer.** BWT's `GetWorkGiverOption` postfix adds exactly two entries per
    unassigned giver, "assign work" first, then the original option. Nothing else in the assembly
    writes to it.
  - **Filled and emptied inside one enumeration.** Vanilla's `GetWorkGiversOptionsFor` is an
    iterator that calls `GetWorkGiverOption` for every giver before its first `yield`. BWT's
    postfix wraps it: it passes the vanilla options through, which runs all those calls, then
    yields the list, then clears it. So while any `GetWorkGiverOption` postfix runs, the list holds
    only the current menu's entries, and the entries for this giver are the ones added during
    this call.
  - **How to find them without guessing.** A prefix stores the list's `Count` in `__state`; the
    postfix decorates every entry from that index to the end. No matching by label, no
    assumption on the pair's order.
  - **BWT's postfix has no `HarmonyPriority`**, so it runs at the default 400; ours at
    `Priority.Last` runs after it and finds the entries already added.
  - **Grouped options never reach it.** For an equivalence group the method returns `null`, and
    BWT's postfix returns at once on `null`: those options stay in vanilla's storage and are
    handled by the slot comparison above.
  - **One leak, not ours.** The list is cleared only when the enumeration reaches its end. If a
    caller stops early, entries survive into the next menu. Harmless to the decoration: our index
    comes from the count at our prefix.

**The dot's texture: `UI/Icons/ColorIndicatorBulb`.** Found by extracting every texture path
string from the 1.6 assembly (656 of them, UTF-16 at both byte parities) and filtering for round
shapes. It is the one vanilla uses for exactly this: `Command_ColorIcon` loads it in a
`[StaticConstructorOnStartup]` class of the base assembly — so it ships with Core, not a DLC — and
draws it 16 px wide, tinted with the gizmo's colour, as its "colour circle". The other candidates
are map overlays (`UI/Overlays/Circle75Solid`, a material for the multi-pawn goto marker;
`UI/Overlays/DotHighlight`) or widgets (`UI/Widgets/RadioButOn`). Not seen: the texture lives in
the game's asset bundle, so its look is inferred from that use, and checked in play.

### Before starting

1. **The dot's texture.** `UI/Icons/ColorIndicatorBulb`, vanilla's own colour circle, see the
   action menu section. Check in play that it reads well at the menu's icon size (27 px, 16 px in
   tiny mode) before shipping a texture of our own.
2. **GrimWorks: settled, no link** (rule 2 above). What stays open is only whether a new custom
   type's column lands somewhere sensible in GrimWorks' order when it falls into `Unsupported`,
   which is an ordering question for Work Studio, not a colour one.
3. **Vanilla types too, or only custom ones.** The 1.1.0 plan was the custom-type sheet. Letting a
   player restyle Construction is the same code and a larger save footprint.
4. **Whether to expose `showMode`.** The colonist bar and the map can be told apart per marker
   for free; decide whether the type sheet offers it or leaves Busywork's default.
5. **Equivalence groups in the action menu.** Handled by the prefix/postfix pair above, not by a
   plain postfix. Still check in play, on a thing with two equivalent givers, that the option
   shown carries the icon of the giver that won.
6. **One mod or several.** Considered 2026-09-17: a style core with one display mod per place.
   The mods above already split it that way, so Work Studio stays the one place the player
   chooses, and the action menu patch lives in Work Studio unless it proves to conflict with a
   float-menu mod (Useful Marks ships `FloatSubMenu.dll`).
