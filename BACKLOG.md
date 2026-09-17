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

### Licences of the linked mods

Checked 2026-09-17 in each mod's `About.xml`, in every file it ships, and in its full Workshop
description through the Steam API. **None of the four states a licence or a permission.**

| Mod | Author | What the page says |
|---|---|---|
| Busywork | Andromeda | Nothing on rights. |
| Useful Marks | Andromeda | "Art : Andromeda" — the pixel-art marks are the author's own; paid support on Boosty. Players may drop their own icons in `Textures/Marks`, which is not a grant to redistribute. |
| GrimWorks: Work Manager | Grim & Chat | Nothing on rights. |
| [baku] Work Type Tag | baku | Nothing on rights; the Workshop copy even says its DLL must be built from source it does not ship. |

**Silence is not a refusal.** The repository's rule, set 2026-09-04 and restated 2026-09-17: a mod
that states no licence can be reused, with the author credited by name and a takedown clause —
if the author objects, the borrowed part comes out. Only an explicit prohibition (an ND licence,
a "do not redistribute" clause) closes the door. A first version of this section read the silence
as "all rights reserved" and forbade copying a mark; that was wrong.

- **The soft links** ship none of their code or art: Work Studio finds their types at runtime, on
  the player's install, and silences itself when they are absent.
- **Reusing a mark** — in the action menu without Useful Marks, or in a marker mod of our own —
  is open under the same rule: credit Andromeda, keep the takedown clause.
- **The other half of the rule still applies.** A mod derived from a source that is **alive in
  1.6** stays private; Busywork and Useful Marks are both maintained (updated
  August–September 2026). Reusing their marks in something published is a decision to take
  knowingly, not a default.
- **Fallback, said 2026-09-17: "au pire, on fera les nôtres"** — our own marks, possibly
  **animated**. The chain already exists in SkillIcons: parametric drawings in `_tools/gen.js`,
  rasterised frame by frame by headless Chrome, and a Harmony postfix that swaps the frame at
  draw time (`PassionIconAnimations.cs`). Its design rules carry over: the silhouette alone must
  be recognisable, and a tint is a hue remap, never a desaturation.

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

**But a custom type's Busywork marker does not survive a restart on its own.** Read 2026-09-17:
- Busywork saves **every** entry of `WorkMarkers` into its own settings whenever they are written
  (`BuildSavedList`), ours included, and rebuilds the dictionary at startup from that list
  (`ApplyLoadedMarkers`, which clears it first), looking each type up with `GetNamedSilentFail`.
- That happens in its `[StaticConstructorOnStartup]` bootstrap, **before** Work Studio's
  `ExecuteWhenFinished` recreates the custom `WorkTypeDef`s. The lookup finds nothing, the entry is
  dropped, and the next time Busywork writes its settings it is gone from its file too.
- Stock types are not affected: their defs exist when Busywork looks, and Work Studio never
  changes their `gerundLabel`, so their key stays valid.

So the rule needs one addition for the Busywork link: **at startup, after `Apply()`, write the
marker of every styled type that is missing from `WorkMarkers` and not in Busywork's
`ClearedWorkDefs`**. It restores what Busywork could not, and never overwrites anything Busywork
holds. On an explicit edit, overwrite as before. And when the player edits a custom type's
`gerundLabel`, remove its entry before the change and put it back after: the key's hash moves with
it.

**What it means for the icon.** ~~Without Useful Marks, only our action menu would show an icon. So
without it the sheet offers a colour alone, and the action menu shows a dot in that colour.~~
Superseded 2026-09-17: the marks ship in Work Studio (question 7), so the sheet offers them and the
action menu shows them with or without Useful Marks. The dot remains for a type styled with a
colour and no mark.

**Settled 2026-09-17: our own marks, made last.** The picker offers a set drawn for Work Studio,
not Useful Marks' marks; the drawing comes at the end of the work, once the study and the code are
done. Read in Useful Marks the same day, and it keeps the Busywork link whole:
`Assets.LoadAllMarkTextures` loads `ContentFinder<Texture2D>.GetAllInFolder("Marks")`, the
`Textures/Marks` folder of **every** active mod, keyed by texture name, first one wins, point
filtering, with a `_plus` suffix marking an accent layer. Marks shipped in Work Studio's
`Textures/Marks` would therefore be known to Useful Marks by name and usable in a Busywork marker
— provided their names are prefixed so they collide with nobody's. The animation stays ours: Useful
Marks draws a static texture.

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
- `MarkerSettings` comes from Useful Marks, with **three** constructors besides the empty one:
  `(string iconName, Color col, string desc, int side, MarkerConditionNode condition = null)`,
  `(string iconName, Color col, int side, MarkerConditionNode condition = null)` — the one Busywork
  itself calls — and `(string iconName, Color col, int side, string descr, MarkerConditionNode
  condition = null)`. Both the 2026-09-11 reading and a first 2026-09-17 "correction" saw one
  each. By reflection, ask for the exact parameter types: the name alone is ambiguous.
- **Busywork already marks the stock types.** Its settings bootstrap creates markers for 45 work
  types when they exist: every vanilla and DLC type, and Complex Jobs' split types — all with
  `AdaptiveColor`. Through `CreateMarker`, which, like the XML path, skips a type already in the
  dictionary or cleared by the player.

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

**The write, read in full 2026-09-17** (`WorkTypeTagMod`, `WorkTypeTagSettings`, and `Verse.ModSettings`):
1. Read the internal static field `baku.WorkTypeTag.WorkTypeTagMod.Settings` by reflection. That is
   the only non-public step.
2. Resolve the **group**, not the `defName`: call the internal
   `WorkTypeColorResolver.ResolveGroupDefName(WorkTypeDef)` by reflection. For every type but
   Complex Jobs' split ones the group is the `defName`; for those it is the vanilla parent
   (`FSFNurse` → `Doctor`), and a record written under `FSFNurse` would never be read. The
   consequence cannot be avoided: Work Type Tag has **one colour per group**, so styling Complex
   Jobs' Nurse recolours Doctor and Surgeon too. **Settled 2026-09-17: write anyway, and say so
   in the style dialog** when the type's group differs from its `defName`, naming the other types
   of the group that the colour will reach in Work Type Tag.
3. `GetOrCreate(group)` — public — returns the record, creating it with the default colour.
4. Set its public `red`, `green`, `blue` (0–255).
5. `NotifyGroupChanged(group)` — public — clamps them and drops that group from the presentation
   cache, so the next job report shows the new colour.
6. `Write()`. Not a Work Type Tag method: `WorkTypeTagSettings` is a `ModSettings`, and
   `Verse.ModSettings.Write()` is public, so a plain cast does it. Nothing in Work Type Tag calls it;
   its settings are otherwise saved only when the player closes its window, through its
   `WriteSettings` override.

**It survives a restart on its own**, unlike Busywork: records are keyed by a string and loaded in
the mod's constructor, with no def lookup. No restore at startup is needed.

**One cosmetic leftover.** Its window keeps the RGB text fields of the last group shown in private
buffers, refreshed only when the selected group changes (`bufferGroupDefName`). After a write from
Work Studio to that same group, the sliders show the new colour and the text fields the old
numbers, until the player selects another group. Harmless — the record is right, and typing in a
field sets it from what is typed — or cured by setting that private field to `null` on the mod
instance.

**The fallback reads the same way.** `GetEffectiveColor(group)` — resolved group again, not the
`defName`.

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

**Settled 2026-09-17: the mark goes on the right, in the option's extra part** — not in its icon,
which already shows the target thing (see below). Orders that come from work ("Prioritize
repairing…") are built by
`FloatMenuOptionProvider_WorkGivers.GetWorkGiverOption(pawn, workGiver, target, context)`, which
reads `workGiver.workType` itself. A postfix there knows the work type and can fill the extra part
of the option it returns: `extraPartWidth`, `extraPartOnGUI` and `extraPartRightJustified`, all
public fields.

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
- **The slot pair, verified 2026-09-17.**
  - *The array.* `private static FloatMenuOption[] equivalenceGroupTempStorage`, one slot per
    `WorkGiverEquivalenceGroupDef` (an empty `Def`; the slot is `group.index`). `GetOptions`
    (re)creates it when its length does not match the def count, and `FloatMenuMakerMap` always
    calls a provider's `GetOptions` before its `GetOptionsFor`, so the array exists when a
    thing is clicked. Guard for `null` anyway. `AccessTools.StaticFieldRefAccess` reads it.
  - *Only thing targets.* The slot is written only when `target.HasThing` and the giver has a
    group; every other path returns before it or returns the option.
  - *Who wins.* An option takes the slot when the slot is empty, or when the slot holds a disabled
    option and the new one is not (`Disabled` is `action == null`). An enabled option is never
    displaced. Each call builds a new `FloatMenuOption`, so a reference comparison between the
    prefix's copy and the slot afterwards is exact.
  - *What is at stake in vanilla.* Three groups. `AssistInConstruction` holds five givers across
    **two work types**: Construction (finish frames, deliver to frames, deliver to blueprints) and
    Hauling (deliver to frames, deliver to blueprints). The winner decides whether the option shows
    Construction's icon or Hauling's — and Work Studio can move any of the five elsewhere.
    `FeedPatientAnimals` (Doctor) and `ReleasePrisoner` (Warden) have one giver each in vanilla.
  - *Harmony will run our prefix.* Read in `HarmonyLib.MethodCreator.AddPrefixes` (Harmony
    2009463077): once a prefix returns `false`, later prefixes are skipped **only if they can
    affect the original** — a `bool` return, or an `out`/`ref` parameter other than `__state`,
    the instance or the original method. A `void` prefix taking the giver and target by value plus
    `out object __state` always runs, whatever LarvaParasite, VEF or Hauler's Dream do before it.
    When they skip the method, the slot does not change and the postfix does nothing.
  - *Both, not either.* A postfix that replaces `__result` (Children, School and Learning) can
    hand back an option while the slot also changed. Decorate `__result` when it is non-null,
    and the slot when it changed; the two are independent.
- **Our icon would hide the target's icon.** `FloatMenuMakerMap.GetProviderOptions` sets
  `iconThing` to the clicked thing on every option from `GetOptionsFor` that has none — so every
  work order on a thing already shows that thing's icon. `DoGUI` draws one icon only, in this
  order: `shownItem`, then `iconTex`, then `iconThing`. Setting `iconTex` would replace the wall or
  the plant with our mark. Options from `GetOptions` (a clicked cell) have no icon to lose. Hence
  the extra part, which loses nothing.
- **The extra part, read in `DoGUI`.**
  - *Free on these options.* Vanilla's work-giver options use the plain constructor and leave all
    three fields empty; Better Work Tab's replacements too. Leave any option whose
    `extraPartOnGUI` is already set alone.
  - *Where it lands.* With `extraPartRightJustified`, the part's rect is
    `(rect.xMax - extraPartWidth, labelRect.yMin, extraPartWidth, 30)`, and the label's rect loses
    `extraPartWidth` on the right, so a long label wraps instead of running under the mark. The
    height is a fixed 30 whatever the size mode: draw a square of `extraPartWidth`, not of the
    rect's height.
  - *What the delegate returns.* `DoGUI` returns `true` at once when the delegate does, before the
    option's own button: `true` would swallow the click. Return `false`.
  - *Hover.* While the mouse is over the part, `DoGUI` draws the option unhighlighted, but the
    click still goes to the option. A small mark keeps that area small.
  - *Colour is ours to set.* Unlike the icon, the part gets no `iconColor`: the delegate sets
    `GUI.color` to the type's colour for the dot, white for a Useful Marks icon, dimmed when the
    option is `Disabled`, and restores it.
- **Duplicates are dropped by label.** `tmpUsedLabels` discards an option whose label another giver
  already produced. Whichever giver wins, the icon follows its work type; nothing to handle.
- **Disabled options too.** Everything that reaches the end of the method gets through, including
  "Cannot …: not assigned to …". **Settled 2026-09-17: they carry the icon too** — it names the
  work type the player has to enable. So the postfix decorates every non-null option, with no
  test on `Disabled`.
- **The size is measured after us.** `FloatMenu` calls `SetSizeMode` on every option when it is
  built, and it counts `extraPartWidth` then. Setting the field in a postfix reserves the mark's
  width; no need to go through a constructor.
- **`DecoratePrioritizedTask` keeps the same object.** It edits the label and returns the option it
  was given, so the reference we decorate is the one shown.
- **Drawing.** A white texture takes the tint cleanly, which suits the colour dot;
  `Widgets.DrawTextureFitted` keeps its proportions, as vanilla does for icons.
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
   action menu section. Its size is now ours to pick, as `extraPartWidth`: vanilla draws it at
   16 px in `Command_ColorIcon`. Check in play that it reads well beside the label before shipping
   a texture of our own.
2. **GrimWorks: settled, no link** (rule 2 above). The question of where a custom type's column
   lands in GrimWorks' order was read on 2026-09-17 and turned out to be a conflict over ordering
   as a whole, not a colour matter: see the entry "Coexisting with GrimWorks' ordering" below.
3. **Vanilla types too, or only custom ones.** The 1.1.0 plan was the custom-type sheet. Read
   2026-09-17 in Work Studio's own source and the three mods:
   - *No save footprint either way.* Work Studio keeps everything in its `ModSettings`, per
     install, and `ExposeConfig` is what an export file holds. The earlier "larger save
     footprint" was wrong.
   - *Stock types are already overridable.* `labelOverrides`, `priorityOverrides` and
     `hiddenTypes` are keyed by `defName` and apply to any type, and the Rename button is offered
     on every type. A `styleOverrides` keyed the same way would cover both kinds; deleting a
     custom type already clears its keys in the other dictionaries. What is custom-only is the
     sheet, `Dialog_EditWorkType`: a stock type would need its own way in, next to Rename.
   - *The other mods already style stock types, and only them.* Busywork ships icons for 45 stock
     types; Work Type Tag has hand-picked colours for 23; GrimWorks has a category for each. A
     custom type gets no Busywork marker, a colour hashed from its `defName`, and GrimWorks'
     grey. So for the linked mods, the value sits almost entirely on custom types — and by the
     "last word" rule a stock type is only touched if the player edits it.
   - *The action menu is the exception.* It is our own code, and nobody marks stock types there.
     Custom types only would leave every vanilla order bare, and the mark would come to mean
     "custom" rather than naming a work type.
   **Settled 2026-09-17: every type, with a fallback on the linked mods.** Any work type can be
   styled, stock or custom, through a `styleOverrides` keyed by `defName`. In the action menu, a
   type the player has not styled borrows what the linked mods already hold — read, never
   written, so nothing changes in them. What each piece comes from, first match wins:
   - *Icon:* the player's choice; else Busywork's marker for the type, if any
     (`MarkerProvider.WorkMarkers[type]`, whose `MarkerSettings` carries the loaded `icon` and
     `iconAccent` textures — a mark is two layers, drawn one over the other); else the dot.
   - *Colour:* the player's choice; else Work Type Tag's effective colour
     (`WorkTypeTagSettings.GetEffectiveColor(group)`, with the group resolved as in the Work Type Tag
     section, which already falls back on its own
     hand-picked or hashed colour, so it always answers when the mod is present); else Busywork's
     marker colour when it is a real one; else white.
   - *Busywork's stock markers are all `AdaptiveColor`*, an alpha below 1 that Useful Marks
     resolves to the pawn's name colour (`MarkerSettings.IsAdaptiveColor`). Not a colour to
     borrow: skip it and go on down the chain.
   - *No linked mod, no player choice:* no mark. The menu then looks exactly like vanilla.
   The fallback is computed when the menu is built, not stored: a colour changed in Work Type
   Tag's own window shows in the next menu.
4. **Whether to expose `showMode`.** Read 2026-09-17 in Busywork and Useful Marks; it is not free:
   - *Only the side path reads it.* Useful Marks' `DrawMarks` filters on `showMode`. Busywork's own
     two draws — the colonist bar postfix and the prefix above the head — call `DrawMark`
     directly, with no test on it. In Busywork's default "upper" alignment the setting would do
     nothing.
   - *Busywork never touches it.* No reference to `showMode` anywhere in its assembly, no control
     in its settings window.
   - *Busywork does not keep it.* Its saved format, `SavedMarker`, holds `defName`, `iconName`
     and `color` only, and `BuildDefDictionary` rebuilds every entry with a fresh
     `MarkerSettings`: after a restart `showMode` is back to `ColonistBarAndWorld`, and `atSide`
     to the global alignment.
   So a per-type choice would be ignored in the default alignment and forgotten at the next
   start. **Settled 2026-09-17: not exposed.** The marker keeps the constructor's default,
   `ColonistBarAndWorld`.
5. **Equivalence groups in the action menu.** Handled by the prefix/postfix pair above, not by a
   plain postfix. Still check in play on a frame, where Construction and Hauling givers compete,
   that the option shown carries the mark of the giver that won.
6. **Where the mark sits in the action menu: settled 2026-09-17, on the right, in the extra
   part.** `iconTex` on the left would have hidden the target thing's icon on every order given on
   a thing.
7. **One mod or several.** Considered 2026-09-17: a style core with one display mod per place.
   The mods above already split it that way, so Work Studio stays the one place the player
   chooses, and the action menu patch lives in Work Studio unless it proves to conflict with a
   float-menu mod (Useful Marks ships `FloatSubMenu.dll`).
   **Re-read 2026-09-17, after the decisions on our own animated marks.** The conflict check is
   done: `FloatSubMenu.dll` patches `FloatMenu.UpdateBaseColor` and `GenUI.DistFromRect` only.
   Three pieces are now on the table, and they do not all belong together:
   - **Work Studio** (public, MIT, Workshop 3792836684): the choice in the sheet, the storage, the
     links to Busywork and Work Type Tag, the action menu mark — which it draws and animates
     itself, frames included, with no need for anything else.
   - **Useful Marks Plus**: animates any Useful Marks mark that has frames, by a prefix on
     `NamePlatePatches.DrawMark`. It needs Useful Marks, not Work Studio, and is useful to anyone
     who draws animated marks: a mod of its own, filed in the monorepo backlog.
   - **The marks themselves**: a still in `Textures/Marks/`, frames in `Textures/MarksAnimated/`.
     Useful Marks loads the stills from every active mod and Plus would find the frames the same
     way (`ContentFinder` searches all mods), so the marks can ship in whichever mod — nobody
     needs to depend on the one that carries them.
   **Settled 2026-09-17: the marks ship in Work Studio.** It is the one piece that uses them without
   anything else installed (the sheet's picker, the action menu), and with Useful Marks present
   they appear in its picker for free. A separate marks pack would be a third repository and a
   third Workshop page for textures only. Plus stays a separate, generic mod; the frame layout
   (`MarksAnimated/<name>/<name>_NN`) is the only contract the two share, and names carry a
   `WorkStudio_` prefix, since Useful Marks keeps the first texture of a given name.
   **This reopens an earlier line.** "Without Useful Marks the sheet offers a colour alone" was
   written when the icons were Useful Marks' own. With our marks inside Work Studio, the sheet can
   offer them — and the action menu show them — with or without Useful Marks.
8. **How a player reaches a type's style.** Read 2026-09-17 in `Dialog_WorkTypes` and
   `Dialog_EditWorkType`:
   - *Today the two kinds are reached differently.* Under the task list of the selected type,
     `DrawManageRow` shows **Rename · Edit · Delete** for a custom type and **Rename · Reset
     tasks** for a stock one. The sheet, `Dialog_EditWorkType` (620 × 640), is custom-only, and
     its skill list already takes all the room left above its Apply button.
   - *Three ways in.*
     - **A swatch in the type list**, before the label, on every row: it shows the type's
       effective style — the player's choice, else the linked mods' fallback, else nothing — and
       clicking it opens the style. One access for both kinds, and the list shows at a glance
       which types are styled. Clicking the label still selects the row; the swatch needs its own
       rect, carved from the label's.
     - **A Style button in the manage row**: a third button for stock types, a fourth for custom
       ones, which narrows all four.
     - **A style section in the sheet** for custom types, plus a button for stock ones: two
       paths to one setting, and the sheet has no room.
   - *In every case, one shared dialog.* A small `Dialog_WorkTypeStyle`: the mark picker, a colour
     button, and "use the linked mods' style" to clear the choice. The colour itself can use the
     game's own picker: `RimWorld.Dialog_ColorPickerBase` is a public abstract `Window` with a hue
     wheel, a palette and editable text fields, whose subclasses only supply the palette, the
     default colour and `SaveColor(Color)` — `Dialog_AllowedAreaColorPicker` is a 97-line example.
   - *Storage follows the existing overrides.* A `styleOverrides` keyed by `defName` for both
     kinds, like `labelOverrides`, rather than fields in `CustomWorkTypeEntry`: one place for one
     setting. It must join the four places the other dictionaries already go — `ExposeConfig`
     (settings and export), `AdoptConfig` (import), the full reset in `WorkTypeRuntime`, and
     `DeleteType`, which removes a deleted type's keys.
   **Settled 2026-09-17: the swatch in the list, opening the shared dialog.**

---

## Coexisting with GrimWorks' ordering

Found 2026-09-17 while checking where GrimWorks puts a custom type's column, in `GW_WorkManager.dll`
as updated on the Workshop on 2026-09-16. **It contradicts the 2026-09-11 reading**, which found
neither `naturalPriority` nor `CacheWorkGiversInOrder` in GrimWorks: the current version touches
both. Not about icons; parked here because the icon study found it.

**GrimWorks keeps its own order and writes it into `naturalPriority`.**
- The order is a list of `defName`s per save (`GW_WorkManager_GameComponent.customWorkTypeOrder`),
  seeded from a global template in its settings.
- `ApplySavedOrder` rewrites the `naturalPriority` of **every** type in that list — `count × 10`
  down to 10 — then reorders the Work tab's columns. `naturalPriority` is Work Studio's only
  ordering lever (`priorityOverrides`): column order and the order in which pawns pick up work.
- It runs on `LoadedGame` and `StartedNewGame`, when a column is dragged in its header, when a
  category is collapsed or moved, and when its settings change. Work Studio applies its own values
  at startup and on every edit. **Whoever ran last wins**, and loading a save always hands it to
  GrimWorks.
- It also postfixes `Pawn_WorkSettings.CacheWorkGiversInOrder` — not read yet; it may compete
  with Work Studio's per-type giver order as well.

**Where a custom type lands.** `EnsureOrderList` inserts a type missing from the list after the
last type of the **same category**. A custom type is `Unsupported`, and no stock type is, so it
goes to the **end of the list**: last column, and lowest `naturalPriority` once the order is
applied — its work is picked up after everything else. That position is then saved in the list.

**A column created mid-game can vanish.** `ReorderWorkTableColumns` captures the Work table's work
columns **once per session** (`completeWorkColumns`, static), then on every reorder removes all work
columns and puts back only the captured ones. Work Studio builds a new `PawnColumnDef` when a type is
created while playing. Types that existed at startup are in the capture; one created afterwards
loses its column at GrimWorks' next reorder — loading a save, dragging a header, collapsing a
category — until the game restarts. A deleted type's captured column is filtered by `visible`
only.

**Before starting.**
1. Read the `CacheWorkGiversInOrder` postfix, to know whether the giver order conflicts too.
2. Decide who owns the order when both are installed: Work Studio stands down (and says so in its
   window), writes GrimWorks' list through its public `MoveWorkTypeToIndex`, or re-applies its
   own values after GrimWorks' hooks.
3. The column cache is the one to fix whatever the choice: clearing `completeWorkColumns` by
   reflection after Work Studio rebuilds the columns would let GrimWorks capture them afresh.
4. Check the lot in play: create a type mid-game with GrimWorks installed, then drag a header.
