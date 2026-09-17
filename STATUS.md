---
localization: complete
translation_en: complete
translation_fr: complete
mod:          Work Studio
packageId:    nelim.workstudio
repo:         Rimworld-Work-Studio
visibility:   public
detached:     yes
stage:        showcase
licence:      open
licence_at:   this mod's own code is MIT and nothing of Achtung! is redistributed, but the technique came from it and it is a named debt, so the source's licence decides: MIT, LICENSE-achtung.txt
settings_audit: partial
dependencies: declared
showcase:     complete
tested_on:    2026-09-17
workshop:     3792836684
remaining:
  - defect: TESTING.md scenario 5 ("deleting a type in a running game keeps every other priority in place") failed in the 2026-09-17 Pickle run - Pickle second's priority dropped from 3 to 0 after deleting Pickle first, even though PriorityMemory.Restore looks up saved priorities by name and tracing Apply()/SyncCustomTypes step by step found no fault on paper. A first hypothesis (1trickPwnyta's Defaults) was raised and refuted by reading her real config. Strong second hypothesis, mechanism confirmed by decompiling the real 1.6 assembly but not yet confirmed live: Enhanced Work Tab (natee.EnhancedWorkTab) prefixes Pawn_WorkSettings.GetPriority to answer from its own per-pawn store instead of the vanilla DefMap whenever "time-aware priorities" is on (default true, no override in her config), and only learns new values through SetPriority - which PriorityMemory.Restore never calls, writing the DefMap's backing list directly instead. ColonySteps.cs's Describe() now also reports the raw DefMap value by reflection, which settles this outright on the next run: if HasPriority fails but the raw value is correct, this is confirmed
  - defect: TESTING.md scenario 8 ("hide a column"), both scenarios, failed the same way in the same run - Cleaning's priority dropped from 2 to 0 after hide/show, reproducibly. Tracing Apply()'s pipeline by hand found no fault, and Tests/OffGame's TheHideColumnRegression (added 2026-09-17) now proves that by driving the real WorkTypeRuntime.Apply() against a fake WorkTypeDef and pawn: hiding and re-showing the type leaves its priority untouched, and the check is mutation-confirmed sensitive (breaking PriorityMemory.Capture() turns it red). This rules out Apply()'s own reconciliation as the cause. Same Enhanced Work Tab hypothesis and same raw-value diagnostic as scenario 5 above apply here too
  - defect: TESTING.md scenario 6 ("the arrows move a type one place") failed - Research landed right after Patient instead of where the test expected. Not yet distinguished from interference by another loaded mod touching Research or the Work tab's ordering
  - unverified: several other 2026-09-17 Pickle failures trace to environment, not Work Studio - "Work types… opens the editor" hit the Concord/OS-click conflict TESTING.md's own note documents ("no tags recorded this frame"), and two of the three scenario-5 sub-scenarios failed on a ReflectionOnly UnityEngine.InputLegacyModule error that also broke the unrelated generic "save and reload steps" testsuite in the same run - a Pickle-framework issue
  - fixed: Tests/Pickle's own scenario 12 step had an ambiguous raw-save pawn lookup (a large save can carry more than one <nick>Keeper</nick>) and failed on that basis in the same run, not on a real Work Studio defect - Patch_WorkSettingsExposeData.Save() writes the positional list and the named dictionary in the same lockstep loop, so they cannot disagree if the lookup is correct. Narrowed 2026-09-17 to require a <workStudioPriorities> sibling and, if still ambiguous, a matching def count; not yet re-run
  - unverified: the MainButtonDef shortcut (WorkStudio_Settings) is code- and mutation-verified off-game (Tests/OffGame) but has never been exercised at runtime - no RIMMSQOL or other MainButtons customization mod test revealing it, activating it, and confirming it opens the same settings with the same values as Mod options -> Work Studio
  - unverified: settings otherwise have no recorded functional pass at all - no documented run of Mod options -> Work Studio: open/close/reopen, each control's effect, persistence across reload, or the "Reset the whole setup" confirmation
  - unverified: ConfigFile.PathFor/Folder/Export/Import stay untested even off-game - they all reach GenFilePaths.SaveDataFolderPath, and merely JIT-compiling that property throws outside a running Unity player; Tests/OffGame exercises the WorkStudioSettings/CustomWorkTypeEntry Scribe contract they wrap instead, at a path it computes itself
  - unverified: no in-game pass of English or French display yet (raw keys, clipping, fallback text) - the static localization gate is certified complete, but TRANSLATIONS.md tracks this runtime check separately and it must pass before claiming the translations tested in game
session:      local_df8ae659-1a8e-4bf8-a74a-ff90c6c7ada7
updated:      2026-09-17
---

# Work Studio — status

## Stage correspondence

`stage` here uses the coarse vocabulary of the automatic sweep (`port`, `showcase`, `preTest`,
`done`, `tested`, `published`). This audit instead worked the finer chain
`dansMonoRepo -> horsMonoRepo -> ModIcon générée -> Preview générée -> preOptions -> options ->
l10n -> preTest -> done -> tested`. Correspondence: `port` covers `dansMonoRepo` and
`horsMonoRepo`; `showcase` covers `ModIcon générée`, `Preview générée` and `preOptions`; `preTest`
covers `options` and `l10n` on top of the workflow's own `preTest`; `done` and `tested` are
unchanged. Work Studio is set to `showcase`: `horsMonoRepo`, `ModIcon générée`, `Preview générée`
and `preOptions` are all now satisfied (see below), and the `l10n` gate is now certified
`complete` too, but `preOptions -> options` still is not — `settings_audit` stays `partial` until
an in-game/RIMMSQOL pass runs, which this session cannot do (see "In-game pass, 2026-09-17" below).

## Detachment, 2026-09-17 — `dansMonoRepo -> horsMonoRepo`, now done

Found blocked at audit time, fixed the same day. The GitHub repository already existed
(`vbardales/Rimworld-Work-Studio`, pushed via `git subtree push` at some past point) but had
drifted from the monorepo's current content, and the folder itself was still tracked inside the
monorepo — `detached: no` was accurate.

Resolution: found the monorepo commit (`595179b9`) whose `WorkStudio` subtree matched the stale
remote tip exactly (proof: equal `rev-parse` on both sides), then replayed the 18 monorepo commits
since that point onto the remote tip with `git commit-tree`, one per commit, each using that
commit's own `WorkStudio` subtree as its tree and its original author/committer identity and
dates. The replayed tip's tree (`57a9d5d0`) matched `HEAD:WorkStudio` exactly before push. Pushed
as a fast-forward (`549af77c..4941be7e`, no `--force` needed) rather than the "redo the import"
recipe, since the existing history had real content worth keeping rather than being a single stale
import commit.

The folder then became its own repository: `git init -b main` inside `WorkStudio/`,
`origin` pointed at the same GitHub URL, `git fetch`, `git reset --mixed FETCH_HEAD` — HEAD and
index moved to the fetched commit without touching a single file on disk, `git status` came back
empty. `main` now tracks `origin/main`.

On the monorepo side: the path was retired from the shared index and `/WorkStudio/` added to the
monorepo's `.gitignore`, built through a private index so as not to disturb the several other
detachments other sessions had staged concurrently in the same file (`PoultryVarietyPackRenew`,
`AlphaMythologyRenew`, `ImperialFurnishings`, …were left exactly as found, still pending their own
commits). Landed as monorepo commit `3dd1b149` by compare-and-swap (`update-ref` with the old tip
as a guard), then the shared/common index was realigned to match. The `workstudio` remote was
removed from the monorepo, per the "one remote per mod, and it is not the monorepo" rule.

`detached` is now `yes`, `repo` still names `Rimworld-Work-Studio`, unchanged.

## Showcase and preOptions

- `Mod/About/ModIcon.png`: 128×128 RGBA, 22.4 KB. Matches the mascot brief in
  `STYLE_RIMWORLD.md` (orange mascot, drafting tools beside it, thick outline, near-black
  background, one sparkle). No defect found.
- `Mod/About/Preview.png`: 896×504 RGB, 544 KB (< 1 MB). Format and weight pass. **Visual
  reservation, not a blocker**: the image is a flat, thick-outlined, cel-shaded cartoon scene —
  the same rendering language as the ModIcon mascot — rather than the high-oblique,
  matte-painted, no-outline RimWorld camera the STYLE_RIMWORLD.md STYLE block asks for. This is a
  concrete style mismatch against a documented block, not a taste call; it is left as a
  reservation rather than a defect because the workflow only makes the camera comparison a review
  method, and the accent/secondary colours (cream title, warm gold underline and rule against the
  sand-and-brown gradient) are otherwise clearly distinct, satisfying that specific criterion.
- `<description>` in `Mod/About/About.xml`: English, matches the mod, and now ends on
  `[url=https://github.com/vbardales/Rimworld-Work-Studio]Source code on GitHub[/url]` after the
  AI-generation and thanks sections. Fixed 2026-09-17, was the one hard defect on this transition.
- Naming: `Work Studio` is an original mod, carries no port suffix and needs none; packageId,
  folder name and repo name already agree (`nelim.workstudio`, `WorkStudio`,
  `Rimworld-Work-Studio`). No defect.

## Settings audit (MOD_SETTINGS.md)

Useful settings exist and are not cosmetic: opening the work-type editor, exporting/importing/
resetting a setup, all backed by real state (`WorkStudioSettings`, `ExposeData`/`ExposeConfig`
already separate the portable part of the config from the machine-local `knownWorkTypes`, which is
correct and deliberate per its own doc comment). `settings_audit` was entirely absent from this
file before this audit — absent means `unchecked` per MOD_SETTINGS.md, not a pass.

- Primary access (`DoSettingsWindowContents` in `WorkStudioMod.cs`) works by inspection: intro
  label, "Open the work type editor", "Import / export a setup", a conditional "Reset the whole
  setup" behind a confirmation dialog, and the save note. All four labels resolve through
  `.Translate()` to keys present in both languages.
- **Mandatory MainButtons shortcut: added 2026-09-17.** `Mod/Defs/MainButtonDefs/MainButtonDefs.xml`
  defines `WorkStudio_Settings` with `buttonVisible=false` and `workerClass=
  WorkStudio.MainButtonWorker_OpenSettings`. Checked directly in `RimWorld.MainButtonsRoot`/
  `MainButtonWorker` (decompiled 1.6 `Assembly-CSharp.dll`): `MainButtonsRoot.DoButtons()` only
  iterates buttons whose `Worker.Visible` is true, and the base `MainButtonWorker.Visible` getter
  returns `def.buttonVisible` directly — a false def takes no layout slot and draws nothing, which
  is "neither visible nor greyed out" rather than a button disabled at runtime. The worker's
  `Activate()` opens `Dialog_WorkStudioSettings`, a plain `Window` whose `DoWindowContents` calls
  `WorkStudioMod.Instance.DoSettingsWindowContents` directly — the same method, the same
  `WorkStudioMod.Settings` instance, as Mod options -> Work Studio, so both routes necessarily
  share values and persistence rather than needing to be kept in sync by hand. `label`/
  `description` are English in the def (the game's native fallback) with a French DefInjected
  counterpart at `Mod/Languages/French/DefInjected/MainButtonDef/MainButtonDefs.xml`.
  No dependency was added: nothing requires RIMMSQOL to be installed, and nothing in the def or
  worker references it beyond the description's mention of it as an example.
- Verified without launching the game: the project builds clean with the new files
  (`dotnet build Source/WorkStudio.csproj -c Release`); both new XML files parse as well-formed
  XML; `scripts/Check-DefInjected.ps1 -TransMod Mod` reports the two new keys checked with 0
  errors against the reflected 1.6 def graph. Also run against the same `MainButtonDefs.xml`,
  all clean: `Check-XmlFields.ps1` (every element maps to a real 1.6 field), `Check-DefRefs.ps1`
  (no dangling def reference, no wrong-type reference), `Check-TypeRefs.ps1` (the `workerClass`
  reference resolves to the mod's own assembly, not an unguarded third-party type), and
  `Check-ConfigErrors.ps1` (26 load-time rules applied, none triggered).
- **Not verified**: no in-game or RIMMSQOL pass. Nothing confirms yet that RIMMSQOL (or another
  MainButtons customization mod) actually lists this def, can reveal it, that the button then
  activates and opens the window, or that edits made through it persist identically to edits made
  through Mod options. This remains open in `remaining`.
- No recorded functional pass on the settings window itself either: no log of opening/closing/
  reopening through Mod options, changing an option and observing the effect, persistence across
  a reload, or exercising the confirmation dialogs. `TESTING.md`'s twelve scenarios are about the
  editor and its effects on the Work tab and colonists, not about the settings window.

Net: `partial`, not `not_applicable` (the settings are real and useful) and not `complete` (the
shortcut now exists and is code- and DefInjected-verified, but no functional or RIMMSQOL pass has
been run).

### A real defect this settings work surfaced, found and fixed off-game

Building the shortcut led to writing `Tests/OffGame/` (see `preTest -> done` below), which caught
a genuine, pre-existing bug unrelated to the shortcut itself: `Source/WorkStudio.csproj` combines
`<Publicize Include="Assembly-CSharp" />` with `<GenerateAssemblyInfo>false</GenerateAssemblyInfo>`,
the exact pairing that silently drops the `IgnoresAccessChecksTo` waiver
(`rimworld-tests-hors-jeu`, "the mod publicises" chapter). `WorkStudio.dll` had the type embedded
but the waiver never applied. `PriorityMemory.Restore` reads the private
`Pawn_WorkSettings.priorities` and writes its private `workGiversDirty` on every `Apply()` — every
startup, every edit — so this was a live `FieldAccessException` risk on the mod's most central
path, not a cosmetic gap.

Fixed with `Source/AccessChecks.cs` (one line, the established pattern from `ContentedLivestock`
and others). Confirmed by mutation: removing that file and rebuilding turns two of
`Tests/OffGame`'s checks red with a real `FieldAccessException` thrown from inside
`PriorityMemory.Restore`; restoring it and rebuilding turns them green again. A full IL scan added
the same day (`TheNonPublicMemberScan`, the "balayage inverse" from `rimworld-tests-hors-jeu`)
found the fix's true scope is wider than the manual reading above: three more private members
(`GenFilePaths.FolderUnderSaveData`, `DefDatabase<T>.Remove`, `Pawn_WorkSettings.pawn`), all
already covered by the same one-line waiver. Whether the game's own Mono runtime enforces this
check at all was never established either way in this codebase
(same memory chapter, "severity not established" note) — a reason to fix it for free, not a reason
it would necessarily have been visible in play.

## Translation audit (TRANSLATIONS.md)

At audit time the mod shipped no `Defs` and no `DefInjected` folder — every player-facing string
was code-owned, so this was a pure Keyed audit. That changed 2026-09-17 with the MainButtonDef
shortcut (see the settings audit above): one Def, two DefInjected-covered fields, checked with
`scripts/Check-DefInjected.ps1` (0 errors). The Keyed audit below is unaffected.

- `Mod/Languages/English/Keyed/WorkStudio.xml` and the French counterpart both hold exactly 63
  keys, and a full diff of the key lists is empty: no key exists in one language and not the
  other.
- Traced every `.Translate()` call site under `Source/`, including the two places that pass a key
  through a variable instead of a literal (`Dialog_WorkTypes.cs`'s `DrawArrow(..., tooltipKey)`,
  called with `"WorkStudio.MoveUp"` and `"WorkStudio.MoveDown"`, and `Dialog_EditWorkType.cs`'s
  `Field(...)` helper, called with `"WorkStudio.Field.Label".Translate()` etc. at each call site).
  No hardcoded UI string found; the few `Widgets.Label` calls that take a bare variable
  (`Dialog_WorkTypes.cs`'s `TypeLabel`/`GiverLabel`, `Dialog_ConfigFiles.cs`'s file name) draw
  vanilla `WorkTypeDef`/`WorkGiverDef` labels or user-entered file names, not mod-owned interface
  text, so they are outside this gate rather than a gap in it.
  `"OK"`/`"Cancel"` are reused vanilla keys (`Dialog_TextEntry.cs`, `ConfigDrift.cs`), acceptable
  under TRANSLATIONS.md's rule on reusing existing keys.
  Every one of the 18 keys carrying a `{0}` placeholder places it identically in both languages;
  no orphaned or missing parameter found.
- **Not done, and not required for this certification**: an in-game pass in each language (raw
  keys, clipping, fallback text). TRANSLATIONS.md is explicit that `complete` "certifies readiness
  for `preTest`, not in-game validation" and that runtime checks are "recorded separately" in
  `remaining` — see the `unverified` bullet in the front matter above.

Net: `complete` for `localization`, `translation_en` and `translation_fr`. TRANSLATIONS.md's
three static requirements (inventory every player-facing text, make it localizable, verify EN/FR
coverage) are all met with evidence above, for both the Keyed surface and the MainButtonDef added
2026-09-17. Nothing here was inferred from an old stage, an existing language folder, or a bare
syntax check — each line above is a specific trace or a specific comparison.

## Dependencies (l10n -> preTest)

`brrainz.harmony` is the only real dependency (the mod calls `HarmonyLib` directly) and is
declared as a `modDependencies` entry with Workshop and GitHub download links; `loadAfter` also
lists it plus every vanilla DLC actually referenced (`WorkTypeDef`s from Royalty/Ideology/
Biotech/Anomaly/Odyssey are read, not required — this mod does not hard-depend on any DLC).
`incompatibleWith` correctly names the two other mods that rebuild the Work tab's columns
(`Fluffy.WorkTab`, `Mlie.CompactWorkTab`), with a comment explaining why, translated to English
2026-09-17 (was French). Single supported version (1.6 only), so no `LoadFolders.xml` is needed
and none exists. No defect found on this transition itself.

## preTest -> done, and beyond

Not reached. Two of the four gating scenarios TESTING.md names (1, 4) are now confirmed passing in
game; the other two (2, 5) each have at least one real failure — see "In-game pass, 2026-09-17"
below for what is environmental noise and what is not yet explained. "Written, executed" now holds
for the whole suite; "green" does not yet, for either half.

**`Tests/OffGame/`** (added 2026-09-17, `WorkStudio.Tests.csproj` + `ModTests.cs`, following the
established `rimworld-tests-hors-jeu` pattern) instances the shipped `WorkStudio.dll` against the
installed `Assembly-CSharp.dll` and actually executes real mod and game code, no RimWorld process
involved: the publicizer waiver, a full IL scan of every non-public Assembly-CSharp member the DLL
touches (9 found, all legal only because of that waiver), `PriorityMemory.Restore`'s private-member
touches performed for real via a Harmony-faked pawn list, the three Harmony patch targets'
continued existence and signatures in 1.6, the `WorkStudio_Settings` MainButtonDef's declared
content and its worker's override slot, `PriorityMemory.PriorityFor`'s full fallback chain, a real
`WorkStudioSettings`/`CustomWorkTypeEntry` Scribe export-then-load round trip, Keyed key/
placeholder parity between English and French, TESTING.md scenario 7's [baku] Work Type Tag
label-cache claim, scenario 11's Better Work Tab column-order rule — the latter two each proven
against that third-party mod's real installed assembly, skipping cleanly if not subscribed — and,
added the same day after the live Pickle run below surfaced a real-looking failure, scenario 8's
"hiding is not disabling" claim driven end to end through the real `WorkTypeRuntime.Apply()`.
Result: **35 PASS, 0 FAIL, 1 SKIP** (documented: `MainButtonWorker.Visible`'s real getter is
unreachable off-game the same way `ConfigFile` is — see `remaining`). Full output in
`Tests/OffGame/RESULTS-2026-09-17.md`.

`Tests/Pickle/`, an in-game Gherkin harness for the Pickle mod, was written 2026-09-17 and covers
most of the twelve TESTING.md scenarios — the half `Tests/OffGame` cannot reach (real map/pawn/save
state) — and was run for the first time the same day, with mixed results: see "In-game pass,
2026-09-17" below.

A rebuild (`dotnet build Source/WorkStudio.csproj -c Release`) was run as part of this audit to
confirm the distributed assembly matches current source, since every `.cs` file's mtime post-dates
`Mod/Assemblies/WorkStudio.dll`. The build succeeded and produced a byte-identical DLL: the stale
mtime came from comment-only and documentation commits, not from unbuilt functional changes. No
defect there.

Net: closer to `preTest -> done` than before, but not there — "written and executed" now holds for
both halves; "green" holds for the off-game half (32/0/1) and for five of Pickle's ten features,
but not yet for scenarios 5, 6 and 8, whose failures are not yet explained (see below).

## In-game pass, 2026-09-17: Tests/Pickle run for real, mixed results

Run across the full live mod list (150+ mods), not an isolated one. 162 scenarios total across
every Pickle-instrumented mod on the list; the ones belonging to Work Studio, read from
`PickleReports/junit.xml`'s per-feature `testsuite` blocks rather than guessed from scenario titles
alone (several titles, like "the mod loads after Harmony", repeat across mods):

| Feature | Result |
| --- | --- |
| Work Studio loads and its patches apply (1) | 3/3 pass |
| the button in the Work tab (2) | 2/3 pass |
| create a work type, and fill it (4) | 5/5 pass |
| priorities survive a reconfiguration across a save (5) | 0/3 pass |
| reordering types and tasks (6) | 6/7 pass |
| rename a work type (7) | 2/2 pass |
| hide a column (8) | 0/2 pass |
| export, reset, import (9) | 3/3 pass |
| the mod list drift warning (10) | 2/2 pass |
| what removing the mod would leave behind (12, raw-save half) | 0/1 pass |

**Traced to environment, not this mod:**
- "Work types… opens the editor" (2): `tag 'btn:Work types…' not found; known tags: no tags
  recorded this frame` — the Concord/Harmony conflict `Tests/Pickle/README.md` already documents.
- Two of the three scenario-5 sub-scenarios: `FileNotFoundException: ... UnityEngine.
  InputLegacyModule ... ReflectionOnly APIs must be pre-loaded` — this also broke the unrelated,
  generic "save and reload steps" testsuite in the same run. A Pickle-framework limitation on this
  machine, not a Work Studio fault.

**Traced to this suite's own scenario 12 step, not a mod defect:** `position 0 (Firefighter) holds
3 positionally but 0 by name`. `Patch_WorkSettingsExposeData.Save()` writes the positional value
and the named entry for a given type in the *same loop iteration*, from the *same* live value —
they cannot disagree if the step read the right pawn. The likely cause is an ambiguous
`<nick>Keeper</nick>` match in a save this large (a world pawn, a faction record, anything else
sharing the name), which `FirstOrDefault` picked over the real colonist. Narrowed the same day to
require a `<workStudioPriorities>` sibling and, on remaining ambiguity, a matching def count. Not
yet re-run.

**Real, reproducible, and not yet explained — the ones that matter:**
- **Scenario 5** ("deleting a type... keeps every other priority in place"): `'Keeper' should have
  3 for Pickle second; it has 0.` `PriorityMemory.Restore` looks up saved priorities by defName,
  which should be immune to the reindexing `DefDatabase<T>.Remove()` causes (it calls
  `SetIndices()` after removing) — traced the whole `Apply()` pipeline by hand
  (`Capture -> SyncCustomTypes -> ... -> Restore`) and found nothing wrong on paper.
- **Scenario 8** ("hide a column"), both of its scenarios: `'Keeper' should have 2 for Cleaning;
  it has 0.` Hiding only ever touches `type.visible` in this mod's own code — no def is added or
  removed, so no reindexing even happens. Same result twice removes coincidence as an explanation.
  Traced `ApplyTypeOverrides`, `RebuildWorkColumns` and the `Notify_DisabledWorkTypesChanged` /
  `Patch_GetDisabledWorkTypes` chain by hand and found nothing that should zero a type the colonist
  can do and never asked to disable. **Followed up off-game the same day**: `Tests/OffGame`'s new
  `TheHideColumnRegression` drives the real `WorkTypeRuntime.Apply()` against a fake `WorkTypeDef`
  and a fake pawn holding priority 2, hides it, shows it again, and reads the priority back from the
  live `DefMap` values list at each step — both times the priority survives, and the check is
  mutation-confirmed sensitive (forcing `PriorityMemory.Capture()` to return an empty snapshot turns
  it red, as expected). That rules out `Apply()`'s own reconciliation as the cause of this failure:
  whatever zeroed Cleaning's priority in the live run happened somewhere this synthetic exercise
  does not reach - most plausibly something specific to a live `Game`/save (real
  `GetDisabledWorkTypes()`, another mod's patch, a UI-thread timing issue) rather than a logic fault
  in the code this mod's own hide/show path runs.
- **Scenario 6** ("the arrows move a type one place"): `Research` lands right after `Patient`
  instead of at the position the test expects. Plausibly another loaded mod reordering Research
  independently (several in this list touch the Work tab or the Research menu), but not confirmed.

**Hypothesis raised and refuted the same evening (1trickPwnyta's Defaults):** a concurrent session
decompiling `Defaults.dll` found it patches `Pawn_WorkSettings.EnableAndInitialize` and, more to the
point, prefixes `Pawn.Notify_DisabledWorkTypesChanged` - which `PriorityMemory.Restore` calls on
every pawn right after writing named priorities back - to overwrite a type's priority with a
configured default whenever that type is in `WorkPrioritiesBasic` (basic mode) or matched by a rule
(advanced mode). The failure shape (values landing on 3 or 0) matched well enough to be worth
checking against her real config. It does not survive that check: her
`Config/Mod_3285178686_DefaultsMod.xml` has `WorkPrioritiesAdvancedMode` absent (false, so the rules
branch never runs) and `WorkPrioritiesBasic` holding 27 vanilla work types, Cleaning among them, but
every one set to `-1` (`WorkPriorityValue.TynansChoice`) - the basic branch only acts on `0` or a
positive value, so every entry is a no-op on her machine. Defaults' patches fire but do nothing here;
they are not the cause of either failure. Read of one already-identified file, not a search.

**Strong candidate, mechanism confirmed by decompile, not yet confirmed live (2026-09-17 night):**
Enhanced Work Tab (`natee.EnhancedWorkTab`, Workshop 3715873875, active in this list) keeps its own
per-pawn priority store and puts itself in front of both vanilla accessors:
`PawnWorkSettings_GetPriority_Patch` prefixes `Pawn_WorkSettings.GetPriority` and, whenever the game
is `Playing` and `EnhancedWorkTabMod.Settings.enableTimeAwarePriorities` is on (default `true`,
confirmed independently against the shipped 1.6 `EnhancedWorkTab.dll`; her
`Config/Mod_3715873875_EnhancedWorkTabMod.xml` carries no override, so it stays on), asks its own
`EnhancedWorkTabGameComponent` for the priority and answers from that instead of ever reading the
vanilla `DefMap` — `__result` is set and the real getter's body never runs.
`PawnWorkSettings_SetPriority_Patch` postfixes `SetPriority` to keep that store in sync, but only for
writes that go through the vanilla method. `PriorityMemory.Restore` writes straight into
`pawn.workSettings.priorities.values` - the private `DefMap`'s own backing list - which never calls
`SetPriority`, so Enhanced Work Tab's store never learns the restored value; every step here reads
back through `GetPriority()`, so it reads Enhanced Work Tab's stale or missing entry instead of what
this mod just repaired. Fits both scenarios: Cleaning already had an entry (set earlier in the same
scenario through the real `SetPriority()`, which Enhanced Work Tab did absorb) that the hide/show
`Apply()` afterwards never updates in that store; a type created at runtime (scenario 5's custom
type) has no entry there at all.

**Settling it needs one live run, cheaply — added the same night:** `ColonySteps.cs`'s `Describe()`
now also reports `RawPriority(pawn, def)`, the value read straight from the private `DefMap`'s
backing list by reflection (this test assembly has no Publicizer, so reflection stands in for it),
bypassing `GetPriority()` and whatever prefixes it entirely. On the next run, if `HasPriority` fails
but the raw value in the failure message is the expected one, this is confirmed outright — no need
for the live A/B (disabling "time-aware priorities" in Enhanced Work Tab's own settings) at all,
though that remains the fallback if the diagnostic itself is inconclusive. If confirmed, the fix on
this mod's side would be to replay the restored values back through `pawn.workSettings.SetPriority`
after rebuilding the DefMap, at least for pawns whose values changed, so any other mod's own
priority store stays in step — not implemented yet, since it has a real cost for a large colony and
is only worth paying if a foreign patch on `SetPriority` is actually present.

Ruled out or downgraded by the checks above: 1trickPwnyta's Defaults (its own patches on this exact
machine fire but do nothing, see below); `riketta.recolorworkpriorities` and the scenario's own
`Given` setup remain untraced but are now lower priority than Enhanced Work Tab given how precisely
this mechanism fits the failure shape.

The same-day addition of a read-back assertion and a `disabled/visible/useWorkPriorities/hidden`
diagnostic to `SetPriority`/`HasPriority` (`Tests/Pickle/Source/ColonySteps.cs`) exists to separate
"the game never took the value" from "something later dropped it" on the next run — exactly the
question scenarios 5 and 8 raise. A re-run with that diagnostic, ideally against a smaller mod
list, is the fastest way to tell a real defect from environmental noise here.

**Still not exercised at all:** the MainButtons shortcut with RIMMSQOL; the settings window through
Mod options directly; scenario 3 and the visual halves of 7 and 11 (`Tests/OffGame` already proves
each underlying mechanism against the real third-party assembly, not that either renders correctly
on screen); the other half of scenario 12 (an actual restart without the mod); FR/EN display.

## Note, outside this workflow's ladder

`Mod/About/PublishedFileId.txt` and `workshop: 3792836684` show the mod was uploaded to the
Workshop at some point, ahead of where this audit finds it today. That is a pre-existing fact
about the account's Workshop item, not something this audit can undo, and it does not raise the
stage recorded here — the workflow being audited stops at `tested`, well short of `published`.

## Prior automatic sweep, 2026-09-12

The fields below were the previous automatic read-off, kept for history: `dependencies: declared`,
`showcase: complete`, `remaining: unverified: never seen running`. Confirmed still accurate by
this audit rather than replaced.
