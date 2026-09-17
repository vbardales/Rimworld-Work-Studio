---
localization:   partial
translation_en: partial
translation_fr: partial
settings_audit: partial
mod:          Work Studio
packageId:    nelim.workstudio
repo:         Rimworld-Work-Studio
visibility:   public
detached:     no
stage:        port
licence:      open
licence_at:   this mod's own code is MIT and nothing of Achtung! is redistributed, but the technique came from it and it is a named debt, so the source's licence decides: MIT, LICENSE-achtung.txt
dependencies: declared
showcase:     complete
tested_on:
workshop:     3792836684
remaining:
  - defect: no MainButtonDef exists for this mod, so there is nothing for RIMMSQOL or another
      MainButtons customization mod to reveal (MOD_SETTINGS.md requires one, hidden by default).
      The mod only offers a button drawn inside the vanilla Work tab's own header and the normal
      Mod options entry; neither is a MainButtons shortcut.
  - unverified: settings have no recorded functional pass at all - no RIMMSQOL integration test
      (impossible today, see the settings_audit defect above), no documented run of Mod options ->
      Work Studio: open/close/reopen, each control's effect, persistence across reload, or the
      "Reset the whole setup" confirmation.
  - unverified: Tests/Pickle (Gherkin, played in game by the Pickle mod) now covers scenarios 1, 2
      and 4 to 10 of TESTING.md, written today (2026-09-17) and never run - TESTING.md's own new
      note says so explicitly ("Not run yet").
  - unverified: never seen running for anything beyond v1.0.0 - TESTING.md states the up/down
      arrows, import/export, the startup drift warning, the right-hand column and task ordering,
      and three successive attempts at the Work tab button have only ever been compiled.
  - unverified: localization and settings text were audited by source inspection only (inventory,
      EN/FR key parity, placeholder parity, a trace of the two dynamically-passed tooltip keys);
      no in-game pass in either language, no run of a formal coverage script (not applicable here,
      the mod ships no Defs and no DefInjected paths).
session:      audited 2026-09-17, full workflow audit against PUBLISHING.md / STYLE_RIMWORLD.md /
              MOD_SETTINGS.md / TRANSLATIONS.md, against monorepo revision 30f2c3ec
              (WorkStudio tree 0877d10e before this session's own STATUS.md edit)
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
unchanged. Work Studio is set to `port` because it has not reached `horsMonoRepo` — see below —
even though its images, description text and settings code are already largely in place.

## Detachment audit, 2026-09-17 — blocks `dansMonoRepo -> horsMonoRepo`

Checked directly, not from `detached: no` alone:

- `git rev-parse --show-toplevel` from inside `WorkStudio/` returns the **monorepo root**
  (`C:/Users/nelim/Documents/rimworld`), not a repo of its own. `git ls-files -- WorkStudio` still
  lists 41 tracked paths in the monorepo's index, and the monorepo's own recent history
  (`a3bb4727`, `9d7fce71`, …) is commits made *to this path*, not to an autonomous repo. The
  monorepo's `.gitignore` has no `WorkStudio/` line.
- A `workstudio` remote does exist, but only on the **monorepo**, pointing at
  `https://github.com/vbardales/Rimworld-Work-Studio.git`. That GitHub repo exists and has a
  `main` branch, so the first half of detachment (publish an import commit) happened at some
  point. The second half — `git init` inside `WorkStudio/`, point it at that remote, retire the
  path from the monorepo's index and `.gitignore` it — never did.
- The two halves have also drifted apart: `git rev-parse HEAD:WorkStudio` and
  `git rev-parse workstudio/main^{tree}` are different tree objects. Whatever was pushed is stale
  against the monorepo's current content, so even a plain `reset --mixed FETCH_HEAD` is not
  available as a shortcut here — the mod needs the divergence resolved, not just a local `git
  init`, before its own repo can be considered authoritative.

Everything downstream keeps whatever independent validation it already has; it just cannot lift
the overall stage past `port` until this is resolved.

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
- **Mandatory MainButtons shortcut: missing**, not merely unverified. `grep`-ing the whole mod for
  `MainButtonDef` finds none, in code or XML. `Patch_WorkTabButton.cs` adds a button to the
  vanilla Work tab's own header strip by Harmony postfix — a different, real, working feature —
  but it is not a `MainButtonDef`, so no customization mod has anything to reveal. This is the
  one item MOD_SETTINGS.md treats as required regardless of how useful the rest of the settings
  are.
  - Nothing in the repository suggests one was attempted and hidden: no def, no reference to
    `RIMMSQOL` anywhere outside `TESTING.md`'s one incidental mention of the log message from the
    Work-tab-button patch.
- No recorded functional pass: no log of opening/closing/reopening the settings window, changing
  an option and observing the effect, persistence across a reload, or exercising the confirmation
  dialogs. `TESTING.md`'s twelve scenarios are about the editor and its effects on the Work tab
  and colonists, not about the settings window itself.

Net: `partial`, not `not_applicable` (the settings are real and useful) and not `complete` (the
mandatory shortcut does not exist, and no functional pass is recorded).

## Translation audit (TRANSLATIONS.md)

The mod ships no `Defs` and no `DefInjected` folder — every player-facing string is
code-owned, so this is a pure Keyed audit.

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
- **Not done**: an in-game pass in each language (raw keys, clipping, fallback text) —
  TRANSLATIONS.md explicitly allows this to remain pending until `preTest`/in-game testing, so it
  does not by itself block this gate.

Net: `partial`. The inventory and coverage checks found no defect, but they were a manual trace
rather than a scripted, exhaustive one (no coverage script applies here — there are no Defs to
check), so it is recorded as audited-with-no-defect-found rather than certified `complete`.

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

Not reached. `TESTING.md` names four gating scenarios (1, 2, 4, 5) and states plainly that
everything past v1.0.0 "has only ever been compiled" — never played. `Tests/Pickle/`, an in-game
Gherkin harness for the Pickle mod, was written today and now covers most of the twelve scenarios,
but its own new note in `TESTING.md` says "Not run yet". No automated off-game test project exists
for this mod (its logic is UI- and Harmony-patch-heavy rather than data-driven, unlike mods that
keep a `Check-*.ps1`/xUnit suite); the Pickle harness is the intended substitute and has not been
exercised even once.

A rebuild (`dotnet build Source/WorkStudio.csproj -c Release`) was run as part of this audit to
confirm the distributed assembly matches current source, since every `.cs` file's mtime post-dates
`Mod/Assemblies/WorkStudio.dll`. The build succeeded and produced a byte-identical DLL: the stale
mtime came from comment-only and documentation commits, not from unbuilt functional changes. No
defect there.

## Note, outside this workflow's ladder

`Mod/About/PublishedFileId.txt` and `workshop: 3792836684` show the mod was uploaded to the
Workshop at some point, ahead of where this audit finds it today. That is a pre-existing fact
about the account's Workshop item, not something this audit can undo, and it does not raise the
stage recorded here — the workflow being audited stops at `tested`, and this mod has not reached
even `horsMonoRepo` on it.

## Prior automatic sweep, 2026-09-12

The fields below were the previous automatic read-off, kept for history: `dependencies: declared`,
`showcase: complete`, `remaining: unverified: never seen running`. Confirmed still accurate by
this audit rather than replaced.
