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
tested_on:
workshop:     3792836684
remaining:
  - unverified: the MainButtonDef shortcut (WorkStudio_Settings) is code- and mutation-verified off-game (Tests/OffGame) but has never been exercised at runtime - no RIMMSQOL or other MainButtons customization mod test revealing it, activating it, and confirming it opens the same settings with the same values as Mod options -> Work Studio
  - unverified: settings otherwise have no recorded functional pass at all - no documented run of Mod options -> Work Studio: open/close/reopen, each control's effect, persistence across reload, or the "Reset the whole setup" confirmation
  - unverified: ConfigFile.PathFor/Folder/Export/Import stay untested even off-game - they all reach GenFilePaths.SaveDataFolderPath, and merely JIT-compiling that property throws outside a running Unity player; Tests/OffGame exercises the WorkStudioSettings/CustomWorkTypeEntry Scribe contract they wrap instead, at a path it computes itself
  - unverified: Tests/Pickle (Gherkin, played in game by the Pickle mod) covers scenarios 1, 2 and 4 to 10 of TESTING.md and has never been run - TESTING.md's own note says so explicitly ("Not run yet")
  - unverified: never seen running for anything beyond v1.0.0 - TESTING.md states the up/down arrows, import/export, the startup drift warning, the right-hand column and task ordering, and three successive attempts at the Work tab button have only ever been compiled
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
an in-game/RIMMSQOL pass runs, which this session cannot do (see "Next: an in-game pass" below).

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
  errors against the reflected 1.6 def graph.
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
and others). Confirmed by mutation: removing that file and rebuilding turns `Tests/OffGame`'s
first two checks red with a real `FieldAccessException` thrown from inside
`PriorityMemory.Restore`; restoring it and rebuilding turns them green again. Whether the game's
own Mono runtime enforces this check at all was never established either way in this codebase
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

Not reached, but no longer for lack of an off-game harness. `TESTING.md` names four gating
scenarios (1, 2, 4, 5) and states plainly that everything past v1.0.0 "has only ever been
compiled" — never played, and that half of the gate (execution in game) is still missing.

**`Tests/OffGame/`** (added 2026-09-17, `WorkStudio.Tests.csproj` + `ModTests.cs`, following the
established `rimworld-tests-hors-jeu` pattern) instances the shipped `WorkStudio.dll` against the
installed `Assembly-CSharp.dll` and actually executes real mod and game code, no RimWorld process
involved: the publicizer waiver (see above), `PriorityMemory.Restore`'s two private-member touches
performed for real via a Harmony-faked pawn list, the three Harmony patch targets' continued
existence and signatures in 1.6, the `WorkStudio_Settings` MainButtonDef's declared content and
its worker's override slot, `PriorityMemory.PriorityFor`'s full fallback chain, a real
`WorkStudioSettings`/`CustomWorkTypeEntry` Scribe export-then-load round trip, and Keyed key/
placeholder parity between English and French. Result: **25 PASS, 0 FAIL, 1 SKIP** (documented:
`MainButtonWorker.Visible`'s real getter is unreachable off-game the same way `ConfigFile` is —
see `remaining`). Full output in `Tests/OffGame/RESULTS-2026-09-17.md`.

`Tests/Pickle/`, an in-game Gherkin harness for the Pickle mod, was written 2026-09-17 and covers
most of the twelve TESTING.md scenarios, but its own note in `TESTING.md` says "Not run yet" — this
is the half `Tests/OffGame` cannot reach (real map/pawn/save state).

A rebuild (`dotnet build Source/WorkStudio.csproj -c Release`) was run as part of this audit to
confirm the distributed assembly matches current source, since every `.cs` file's mtime post-dates
`Mod/Assemblies/WorkStudio.dll`. The build succeeded and produced a byte-identical DLL: the stale
mtime came from comment-only and documentation commits, not from unbuilt functional changes. No
defect there.

Net: closer to `preTest -> done` than before, but not there — "written, executed and green"
now holds for the off-game half only; the in-game half (Pickle, and the four scenarios it does
not cover) is written but not executed.

## Next: an in-game pass

Every remaining item, in every section above, needs the game running, and this session does not
launch it. Prepared and ready to hand off:

1. **The MainButtons shortcut.** With RIMMSQOL (or another MainButtons customization mod)
   installed: reveal `WorkStudio_Settings`, click it, confirm it opens the same window as Mod
   options -> Work Studio, edit something through it, and check the edit shows up through the
   other route too.
2. **The settings window itself**, through Mod options -> Work Studio: open/close/reopen; click
   "Open the work type editor", "Import / export a setup", and "Reset the whole setup" (with its
   confirmation); change something and confirm it survives a reload.
3. **`Tests/Pickle/`**: run the suite per `Tests/Pickle/README.md` (dev mode -> debug actions ->
   Pickle -> the Work Studio suite -> Run selected, or the `-pickle-run=` command line). It covers
   TESTING.md scenarios 1, 2 and 4 to 10 unattended.
4. **The four scenarios Pickle does not cover** (3, 7, 11, 12 — see the README's "What stays
   manual" table) and the general FR/EN display pass from the localization audit above.

Paste back `Player.log` (prefixed `[Work Studio]`) and whatever Pickle's report says; that is
enough to fill in `tested_on` and clear the `remaining` list above without guessing at a result.

## Note, outside this workflow's ladder

`Mod/About/PublishedFileId.txt` and `workshop: 3792836684` show the mod was uploaded to the
Workshop at some point, ahead of where this audit finds it today. That is a pre-existing fact
about the account's Workshop item, not something this audit can undo, and it does not raise the
stage recorded here — the workflow being audited stops at `tested`, well short of `published`.

## Prior automatic sweep, 2026-09-12

The fields below were the previous automatic read-off, kept for history: `dependencies: declared`,
`showcase: complete`, `remaining: unverified: never seen running`. Confirmed still accurate by
this audit rather than replaced.
