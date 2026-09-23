---
localization: complete
translation_en: complete
translation_fr: complete
mod:          Work Studio
packageId:    nelim.workstudio
repo:         Rimworld-Work-Studio
visibility:   public
detached:     yes
stage:        done
licence:      open
licence_at:   this mod's own code is MIT and nothing of Achtung! is redistributed, but the technique came from it and it is a named debt, so the source's licence decides: MIT, LICENSE-achtung.txt
settings_audit: complete
dependencies: declared
showcase:     complete
tested_on:    2026-09-21
workshop:     3792836684
remaining:
  - "verified: 2026-09-22 minimal English, minimal French and Better Work Tab full Pickle passes each report 49 passed, 0 failed, 6 skipped; Work Type Tag reports 50 passed, 0 failed, 5 skipped. Each complete report was copied to Tests/Pickle/evidence before the shared lock was released; relevant captures were opened."
  - "defect found in visual review: the MainButtons settings dialog footer covered the English save note and hid the French note; long renamed or custom Work tab headers overlapped neighbouring headers in both languages. A larger dialog without the redundant footer button and an icon/tooltip fallback for overlong headers are built; in-game captures must confirm both before tested."
  - "defect found in the Enhanced Work Tab full pass: its Done editing button intercepted the Work types shortcut at the top right. A mod-specific leftward placement is built; a targeted review and full rerun are queued to confirm the click and all scenarios. The first pass is preserved as a failed regression, not counted as tested."
  - "defect (measured 2026-09-23, cause unknown): scenario 2 under Work Tab still fails after the wait for the button to stand still. Pickle aimed at (1058, 774), the old narrow-window position; the probe saw the button at (1754, 759) and the pre-click checks passed; no window covers it. The layout-race explanation does not cover this. Evidence: Tests/Pickle/evidence/2026-09-23-fluffy-scenario2 (summary in docs/runs/2026-09-23.md; the full report and the screenshot stay on disk). Not yet diagnosed."
  - "verified: 2026-09-23 Enhanced Work Tab full pass on the current build: 55 scenarios, 49 passed, 0 failed, 6 skipped, exitReason passed, setName avec-enhanced-work-tab (read before citing; launcher output agrees; summary in `docs/runs/2026-09-23.md`, full report on disk in the ignored `Tests/Pickle/evidence/2026-09-23-ewt-full`). Under that mod the Work types button scenario passes (the leftward placement past its Done editing button works) and both save scenarios of feature 05 pass. The 6 skips are the @requires scenarios of mods that pass does not stage (Fluffy Work Tab 15, RIMMSQOL 16 x4, Work Type Tag 17): they belong to their own passes, still queued. Screenshots of this pass not yet opened."
  - "verified: 2026-09-23 minimal English pass on the current build: 55 scenarios, 49 passed, 0 failed, 6 skipped, exitReason passed, setName sans-facultatifs, language English (read before citing; launcher output agrees; archive pickle-reports-archive/0923-1843 protected by keep.txt; summary in docs/runs/2026-09-23.md, full report on disk in the ignored Tests/Pickle/evidence/2026-09-23-minimal-english). The same 6 @requires skips as the Enhanced Work Tab pass, belonging to the Fluffy, RIMMSQOL and Work Type Tag passes. Captures not yet reviewed with the owner."
  - "verified: 2026-09-23 minimal French pass: 55 scenarios, 49 passed, 0 failed, 6 skipped, exitReason passed, setName sans-facultatifs, language French (read before citing; launcher output agrees; archive pickle-reports-archive/0923-1851 protected by keep.txt). NOTE: this pass and the English and Enhanced Work Tab ones above ran on the build BEFORE the right-hand column put a duplicate task's defName on its own line (2026-09-23); that layout change postdates them and has no pass yet."
  - "measured 2026-09-23, Fluffy Work Tab full pass (docs/runs/2026-09-23.md): 55 scenarios, 45 passed, 5 failed, 5 skipped. Feature 15 (the real Fluffy conflict) PASSED for the first time. Still failing under Work Tab, cause not established for either: the Work types button (pointer aimed at the old narrow-window position) and the save-load exception (an apparel read as a pawn, scenario 5 with two types loaded with one). The three publication-shot scenarios failed on a wrong fixture name of mine (Pickle does not know Nelim-Zen-Meadow-Studio; the fixture is nelim-zen-meadow-studio from PickleTools/ScreenshotStudio), fixed in 07ca835 and now @requires-gated with its own map wsl-deps.studio.map; any full pass staged between about 18:55 and 19:40 carries those three failures."
  - "unverified: Enhanced Work Tab, Fluffy Work Tab, Compact Work Tab and RIMMSQOL full passes are queued for LF-map reruns after CRLF in the dependency maps caused staging failures. A matching report and review capture are required for each."
  - "verified: distributed attribution synchronized on 2026-09-22 after owner approval; first-gate documentation defect closed. See the follow-up below."
  - "fixed (documentation): the cited 2026-09-22-scenario5-enhanced evidence contained one PickleTools interface-scale test, not the Work Studio scenario it was labelled as - a report copied from the shared, one-report-for-the-whole-machine folder without checking its content first. Folder deleted and the false 'confirmed in game' claim removed from CHANGELOG.md and TESTING.md on 2026-09-23. The underlying fix (ed41491, a created type's defName only ever goes up) is unaffected and still stands on its own reasoning; only the in-game confirmation was marked as not done. CONFIRMED for real on 2026-09-23: Enhanced Work Tab set, 05-priorities-across-save.feature, 3 passed, 0 failed, exitReason passed, setName avec-enhanced-work-tab, suite name and scenario names read before citing (summary in `docs/runs/2026-09-23.md`, full report on disk in the ignored `Tests/Pickle/evidence/2026-09-23-ewt-scenario5`)."
  - "verified: preTest -> done scenario gap closed on 2026-09-22. Feature 15 asserts Fluffy Work Tab's stale startup column snapshot; Compact Work Tab's false incompatibility declaration was removed after direct 1.6 assembly inspection. Pickle coverage now also automates the RIMMSQOL and Work Type Tag paths and asserts every review capture's subject state. The duplicate ScreenshotMode capture helper was moved to PickleTools and every map stages it. Written, compiled and statically checked; runtime execution remains done -> tested."
  - "unverified: final-build runtime regressions, EN/FR layout and logs, applicable optional sets, new-game coverage, icon-setting effects and shortcut integration. Historical passes are retained, not certified against this build."
  - "unverified after setting tested aside: prepublished remains blocked by stale Workshop editor screenshots, the v1.1.0 tag pointing behind the current code, release publication not verified, the existing Steam description needing a manual compatibility edit, and thank-you messages lacking final URLs/status. No Steam update was performed."
session:      01a0c844-1b24-7b61-8615-7c2ee4ae1b69
updated:      2026-09-22
---

# Work Studio — status
## Preview regeneration and capture handoff — 2026-09-22

The first replacement committed earlier today did not follow `STYLE_RIMWORLD.md`: it let the image
model rasterize the text, kept no text-free source, palette or deterministic composition, and
omitted the 1.6 badge. It is superseded here.

The corrected text-free source is `Art/Preview.png` (1672×941). `Art/preview.html` renders it with
Segoe UI at the required final size and loads its only overlay colours from
`Art/preview-palette.json`; it adds the 46 px title, 58×3 px accent rule, 21 px summary and 1.6
corner badge. The delivered `Mod/About/Preview.png` is opaque RGB, 896×504 and 545,873 bytes.
Full-size and 268 px inspections found the high-oblique near-orthographic camera, tiled ground,
one warm lamp pool, small faceless settlers, clear three-board reordering action, readable title,
visible rule and badge, and no crop or overlap. The summary is present at 268 px but intentionally
too small for grid browsing, as the style guide documents. Palette measurement reports two vivid
hue families (61.79% vivid in the final, dominant family 94%). The worst sampled title/summary
contrast is 6.00:1 and badge contrast is 6.97:1, both above 4.5:1. Root and distributed attribution
record the generated source and deterministic overlay. The previous cartoon-style reservation is
closed.

The locally prepared review evidence was also inspected and committed: the updated French
three-column editor capture, two French diagnostic crops showing the truncation and two-line
layout, and the Fluffy Work Tab button-overlap crop. These are audit evidence, not claims that the
new Pickle scenarios have run. Stage remains `done`; no runtime or Steam publication claim changes.

## Pickle completeness and later-stage audit — 2026-09-22

**Stage stays `done`; `tested` was deliberately set aside.** The scenario suite now leaves no
purely manual Work Studio check. Feature 06b captures the asserted result of both type and task
dragging. Feature 16 uses PickleTools' RIMMSQOL and InterfaceScale companions to assert the real
list/reveal/persistence/main-bar/open/hide/forget path, including a 150% activation. Feature 17
starts an attributed current job, asserts [baku] Work Type Tag's patched report contains the newly
renamed work type, selects the colonist and captures it. Dedicated dependency maps stage each
optional integration. Every existing `@review` scenario now asserts its subject state before the
capture; a person only has to judge the screenshot. This mod has no audio behavior.

The Pickle step DLL builds with 0 warnings/errors. The off-game suite reports **148 PASS, 0 FAIL,
1 SKIP** after loading 74 local steps plus ClickDiagnostics, RimmsqolSteps and InterfaceScale;
every feature step resolves. No RimWorld process was launched or driven, so the new scenarios are
written and statically validated, not claimed as runtime passes.

Looking beyond `tested` without promoting through it, `tested -> prepublished` is **not ready**:
the first Workshop editor screenshot still shows the old layout; tag `v1.1.0` points to `bf89753`,
behind the current code; a matching release is not verified; the existing Steam description must
be edited manually to remove the obsolete Compact Work Tab conflict; and thank-you-message URLs
and posting status remain open. The dependency/DLC decision, adult-content answer, patch notes,
existing `PublishedFileId.txt` (`3792836684`) and the other two screenshot slots are recorded.
`prepublished -> published` is likewise not satisfied for this update: no Steam upload, subscribed
self-test, public visibility check or thank-you posting was performed. The historical public item
does not prove that this update was published.

## Incompatibility audit and scenario completion — 2026-09-22

**Stage: `preTest` -> `done`.** The final written-scenario blocker is closed. This does not claim
new runtime execution: under AUDIT.md, Pickle execution and capture review belong to
`done -> tested`.

Direct inspection of the installed 1.6 assemblies established two different outcomes:

- Fluffy's Work Tab captures `WorkTab.Controller.allColumns` during implied-def generation and
  later restores that snapshot from `MainTabWindow_WorkTab.RebuildTable`. A work type Work Studio
  creates at runtime is present in the live `PawnTableDefOf.Work.columns` but absent from the
  snapshot. New feature `15-work-tab-incompatibility.feature`, gated by
  `@requires:Fluffy.WorkTab`, asserts exactly that stable conflict. It does not treat an unrelated
  click failure or log error as evidence.
- Compact Work Tab 1.6 patches the existing `PawnColumnWorker_WorkPriority` header and cell
  methods and recalculates its cache from the current `table.Columns`. It owns no competing
  column list, and its documentation explicitly supports mods adding work types. The
  `Mlie.CompactWorkTab` incompatibility entry and matching description claim were therefore
  removed; its historical dependency map now documents an optional coexistence pass.

Updated `About.xml`, README, CHANGELOG, TESTING.md, PUBLICATION.md and both pass-map notes.
Rebuilt the Pickle step assembly: 0 warnings/errors. Rebuilt and ran the off-game suite:
**146 PASS, 0 FAIL, 1 SKIP**; 72 declared suite steps and every feature step resolved. The suite
now contains 17 feature files and 48 scenarios. Check-DefRefs remains clean: one mod def, all XML
well formed, no missing/wrong references or unresolved parents. No RimWorld process was launched,
stopped or driven.

The next transition remains `done -> tested`: execute the final-build passes and inspect their
captures/logs. The misattributed Enhanced Work Tab evidence remains unverified. Existing local art
changes were preserved. No commit, push or publication was performed.
Because this is an existing Workshop item, PUBLICATION.md now also records the required manual
description edit: an update does not resend About.xml's description.

## Attribution repair — 2026-09-22

Following the owner's go-ahead, copied root `ATTRIBUTION.md` to `Mod/ATTRIBUTION.md` and
verified identical SHA256 hashes. The missing SkillIcons provenance is now distributed.
Only that documentation copy and this status were changed by the follow-up; existing local
art changes and the earlier audit record are preserved. No commit or push was made.

**Stage: `dansMonoRepo` -> `preTest`.** The first documentation gate now passes. The independent
build/artifact, showcase, settings, localization and dependency validations from today's audit
remain applicable: this documentation-only repair invalidates none of them. The shipped DLL
still has SHA256 `3530BA389FF05A1CF616FC1E68CC4FBF8CE5C2650E70FE117BBCE7E549C389C5`.
The revision remains `6f2ff47f2ddcee87b3a8d3cc8f4b8bd548dfa173` plus the recorded local edits.
The exact `preTest` workflow state is used, not the older coarse code covering options/l10n.

**Next gate at the time: `preTest -> done`.** This paragraph is superseded by the
incompatibility follow-up above: the real Work Tab conflict now has a written scenario and the
false Compact Work Tab declaration was removed. Runtime execution still belongs to
`done -> tested`; no game result was invented. The misattributed Enhanced Work Tab report remains
an evidence defect, independently of the completed packaging repair.

Validation: byte-identical attribution hashes and `git diff --check`. No build/test rerun is
needed for this documentation-only change; earlier technical results remain recorded below.
This follow-up supersedes the earlier audit's first-gate blocker and stage conclusion only.

## Current audit — 2026-09-22

This section and the front matter supersede contradictory current-tense statements in the
historical sections below. Read the parent AUDIT.md, AGENTS.md, PUBLISHING.md,
STYLE_RIMWORLD.md, MOD_SETTINGS.md and TRANSLATIONS.md. No production fix, art edit,
publication, commit or push was performed.

**Previous stage: `done`. Retained cumulative stage: `dansMonoRepo`.** The exact AUDIT.md
vocabulary now replaces the historical coarse `port` code (which covered both `dansMonoRepo`
and `horsMonoRepo`). This does NOT undo the actual Git detachment: `detached: yes` remains
verified. Required distributed documentation blocks the first gate; independent later checks
are retained below.

### Scope and first gate

- Repository: `C:/Users/nelim/Documents/rimworld/WorkStudio`; distribution: `Mod/`.
  Audited revision: `6f2ff47f2ddcee87b3a8d3cc8f4b8bd548dfa173`.
- Initial local changes preserved: modified `Art/Review-FR/03a-editeur-trois-colonnes.png`;
  untracked `Art/Review-FR/zoom-2-apres.png`, `Art/Review-FR/zoom-3-deux-lignes.png`,
  `Art/Review/zoom-fluffy-coin-haut-droit.png`. No initial production source/DLL changes.
- Live read-only `git ls-remote origin HEAD refs/heads/main refs/tags/v1.1.0` confirmed remote
  HEAD/main equal the audited revision; tag v1.1.0 resolves to
  `10250f02f1425a7297c7d72ee88b22baf48edac7`. `gh repo view --json visibility,url`
  confirmed PUBLIC and the exact About.xml repository URL.
- README, CHANGELOG, attribution and MIT notices exist. Naming is coherent. The distributed
  Achtung notice matches its root copy. Own MIT notices differ only by Nelim/nelim casing.
- **Defect:** `git diff --no-index -- Mod/ATTRIBUTION.md ATTRIBUTION.md` shows the entire
  SkillIcons section (35 drawings and two icon prefixes transferred on 2026-09-18) absent
  from the distributed copy. PUBLISHING.md requires synchronizing this copy after changes;
  AUDIT.md puts required distributed documentation in `dansMonoRepo -> horsMonoRepo`.
  This is a documentation/packaging defect, not a finding of missing third-party permission.
- **Next transition, strictly:** synchronize that copy and verify the contents agree.
  The audit instruction does not ask to repair the product to obtain a better status.

### Independent checks performed now

| Area | Observed result |
| --- | --- |
| Build | `dotnet build Source/WorkStudio.csproj -c Release --no-restore --nologo -p:OutputPath=../.build/audit-2026-09-22/mod/`: 0 warnings/errors, shipped DLL untouched |
| DLL identity | Shipped and rebuilt SHA256 both `3530BA389FF05A1CF616FC1E68CC4FBF8CE5C2650E70FE117BBCE7E549C389C5` |
| Off-game | Build Tests/OffGame/WorkStudio.Tests.csproj, Release, no-restore: 0 warnings/errors. Run `.build/offgame/bin/Release/net48/WorkStudio.Tests.exe`: **146 PASS, 0 FAIL, 1 SKIP**. Output: `.build/audit-2026-09-22/offgame.txt` |
| Skip | Actual Worker.Visible invocation requires the live Unity environment. Hidden def and worker contract checked separately; no runtime success inferred |
| XML/refs | `powershell.exe -NoProfile -ExecutionPolicy Bypass -File ../scripts/Check-DefRefs.ps1 -ModPath ./Mod`: one mod def; well formed; no missing def, wrong reference type or unresolved parent |
| DefInjected | Same shell, `../scripts/Check-DefInjected.ps1 -TransMod ./Mod`: 29 patch operations, 11587 indexed defs, 2 keys, 0 errors |
| XML class | Check-XmlClasses with ModPath ./Mod, SourceDirs ./Source and temporary TypeLists `.build/audit-2026-09-22/types.txt`: one referenced type resolved from source. Off-game suite independently verifies the worker in the shipped DLL |
| Settings | Useful editor/import/export/reset and icon preferences; both settings routes delegate to the same renderer/state; buttonVisible=false; defaults and header-mode clamp inspected. Complete retained for the off-game gate only |
| Localization | 76 EN / 76 FR Keyed entries, no duplicates, empty values or placeholder-parity failures; source WorkStudio key literals covered, dynamic arrow key call sites reviewed; two French DefInjected fields resolve. Static complete retained, not a runtime layout certificate |
| Dependencies | Harmony used and sole hard dependency; no required DLC or LoadFolders branch; optional reflection integration remains optional; Fluffy.WorkTab is the single declared incompatibility after direct inspection removed the false Compact Work Tab entry |
| Art | Opened Preview (896x504, 557028 bytes) and ModIcon (128x128, 22983 bytes). Title and orange rule visibly separated; naming passes. Historical outlined/cartoon-style reservation remains advisory |
| Scenarios | Initially 16 feature files and 47 scenarios; the follow-up above adds feature 15, making 17 files and 48 scenarios, with one incompatibility set and Compact Work Tab retained as a coexistence pass |

Initial sandbox builds could not read Microsoft SDKs; authorized retries succeeded. Windows
PowerShell checkers initially hit execution policy; explicit per-process Bypass runs above
completed. The first XML type-list attempt could not load Assembly-CSharp in PowerShell;
source-aware checking and the shipped-DLL tests resolved that tooling failure. These are not
production mod defects.

### Runtime evidence and later gates

**The cited Enhanced Work Tab confirmation is not established by its files.** Opened both
`Tests/Pickle/evidence/2026-09-22-scenario5-enhanced/summary.json` and junit.xml. JSON says
`total: 1`, `passed: 1`, `exitReason: passed`, `setName: interfacescale`; XML names
`PickleTools interface scale` and `a button is clickable at another interface scale`.
This is not the claimed three-scenario Work Studio run. Historical statements below remain
historical claims, not verified results. No other session's transient reports were used.
The off-game custom-ID regression checks pass; this evidence mismatch is not a mod failure.

This finding was resolved by the incompatibility follow-up above: the real Work Tab conflict now
has a targeted written scenario, and Compact Work Tab's unsupported declaration was removed.
Feature 01's generic loaded/load-order scenario still tests a game responsibility; that remains a
scope-cleanup recommendation rather than a blocker or demonstrated Work Studio defect.

Historical full passes and visual reviews are preserved, including their acknowledged need
for regressions after the latest changes. Opened existing French captures
`Art/Review-FR/13b-reglages-raccourci.png` and `03a-editeur-trois-colonnes.png`: they show French
settings/editor labels but do not establish a complete final-build EN/FR review. The editor
image was already locally modified; no build identity follows from its filename. The shortcut
capture does not visibly show the final save-note paragraph; current layout remains to verify.
English-looking inherited work-type names alone do not prove a Work Studio translation defect.
Other runtime captures were not reviewed in this audit.

No RimWorld process was launched, stopped or driven; no game configuration was changed, so no
teardown is needed. `tested_on: 2026-09-21` remains a historical date, not certification of this
revision. Final-build runtime regressions/logs and EN/FR visual evidence, relevant optional
sets, new-game coverage and applicable settings/shortcut interactions remain unverified.

PUBLICATION.md exists but acknowledges a missing current editor screenshot. Clean tree and
release were not certified. The historical owner publication decision is preserved; an
existing Workshop item does not upgrade the cumulative audit stage.

### Previous front-matter findings (historical; superseded above)

```yaml
remaining:
  - fixed: TESTING.md scenarios 5, 8 and 12 were failing because the SUITE tested work its colonist may not do - settled and repaired 2026-09-20, never a defect in the mod. The fixture generates "Keeper" with random backstories; that run drew Rancher43, a rancher, whose workDisables is ManualDumb - which vanilla's own Cleaning, Hauling, HaulingUrgent and KAU_UrgentHaul all carry, checked in Core's own XML, with Cleaning's workTags untouched by this mod. So vanilla zeroing those priorities was correct, and PriorityMemory.Restore calling Notify_DisabledWorkTypesChanged is right. The guard meant to catch it passed because Pawn.GetDisabledWorkTypes answers from a cache nothing had invalidated until WorkTypeRuntime.Apply cleared the backstory caches. Two repairs: that guard now drops the pawn's two caches and every backstory's before asking, and names the disabling backstory when it refuses; and the three Backgrounds now give Keeper backstories and traits that forbid no work at all, so the scenarios can go on naming Cleaning and mean it. Confirmed by the 2026-09-20 20:48 run: 8 ("hide a column") passed, and 5's two remaining failures are the known ReflectionOnly UnityEngine.InputLegacyModule framework error caught by Pickle's Log.Error guard, not this mod's assertions. The fixture premise was the whole of that defect, and the 23:00 run in the WSL game settles it: 5, 8 and 12 all green, 46 of 47 scenarios passing. The one failure left was the same premise in the one place it had not been repaired - scenario 4's custom type inherits CleanFilth's ManualDumb tag, and the fixture's Keeper could not do it. Both scenarios of 04 now give their colonist harmless backstories; the Porter one passed on luck, not on anything the test guaranteed. Re-run at 23:49 in the WSL game: 47 of 47 green, 248.5s, no failure anywhere in the suite. Note what that green does NOT cover: the five @review scenarios assert nothing, so their screenshots still need a person to read them
  - fixed: TESTING.md scenario 12's raw-save check failed for the same reason as 5 and 8 - the positional and named lists disagreed because the game correctly zeroed work the colonist could not do, between vanilla writing one node and this mod writing the other. Repaired by the same change, and the 2026-09-20 20:48 run showed that was not the whole story: 12 failed again, this time on a defect of the step itself. Scribe_Collections writes a Dictionary with LookMode.Value on both sides as two parallel <keys>/<values> lists; the step read the node for <li> pairs, found none, and built an EMPTY dictionary - not null, so the guard meant to catch a patch that never ran let it through - after which every lookup answered "absent", TryGetValue handed back 0, and the first colonist with a non-zero priority read as a disagreement. Checked against a save left by an earlier run: 9 colonists, 27 entries each, positional and named agreeing on all 243 values. The mod and the save were right throughout. Fixed 2026-09-20 in 46982a0, and CONFIRMED the same evening: the 23:00 run in the WSL game plays scenario 12 green
  - fixed: TESTING.md scenario 6 ("the arrows move a type one place") was a test artifact, not a defect - settled 2026-09-18. The editor's row list is a cache DoWindowContents drops at the top of every draw pass, so a real arrow click always acts on a list rebuilt that frame; the step called ShiftType directly with no repaint behind it and acted on a list from before the between-scenario reset, then ApplyTypeOrder rewrote every priority from it. The drag scenarios never had the problem because ReorderableWidget only hands out its callback during a repaint. The four arrow steps now let the window draw first
  - fixed: TESTING.md scenario 5's three failures in the 2026-09-18 run were Pickle's Log.Error guard firing on other mods' errors (a Yet another Optimizer / VEF WorkGiver NullReferenceException, and the known UnityEngine.InputLegacyModule framework error), not on this mod's assertions - the 2026-09-17 reading of scenario 5 did not recur
  - fixed: TESTING.md scenario 7 ("the header and its width follow the new name") turned red in the 2026-09-18 run because the screenshot feature added that morning left the Work tab open, and a drawn table rebuilds the column worker the assertion expects to find thrown away. 07b now closes the tab behind it; the mod was never involved
  - unverified: several other 2026-09-17 Pickle failures trace to environment, not Work Studio - "Work types… opens the editor" hit the Concord/OS-click conflict TESTING.md's own note documents ("no tags recorded this frame"), and two of the three scenario-5 sub-scenarios failed on a ReflectionOnly UnityEngine.InputLegacyModule error that also broke the unrelated generic "save and reload steps" testsuite in the same run - a Pickle-framework issue
  - fixed: Tests/Pickle's own scenario 12 step had an ambiguous raw-save pawn lookup (a large save can carry more than one <nick>Keeper</nick>) and failed on that basis in the same run, not on a real Work Studio defect - Patch_WorkSettingsExposeData.Save() writes the positional list and the named dictionary in the same lockstep loop, so they cannot disagree if the lookup is correct. Narrowed 2026-09-17 to require a <workStudioPriorities> sibling and, if still ambiguous, a matching def count; not yet re-run
  - unverified: the MainButtonDef shortcut (WorkStudio_Settings) is code- and mutation-verified off-game (Tests/OffGame) and, since 2026-09-18, has a written in-game check too - Tests/Pickle's 13-settings.feature builds the def's own Worker, activates it the way RimWorld would and asserts the window opens, plus a screenshot of it. Written, not yet run. What no test will ever cover is RIMMSQOL itself listing the def and revealing a real button for it
  - unverified: settings otherwise have no recorded functional pass at all - no documented run of Mod options -> Work Studio: open/close/reopen, each control's effect, persistence across reload, or the "Reset the whole setup" confirmation. TESTING.md gained a scenario 13 for this on 2026-09-18, and Tests/Pickle 13-settings.feature now covers three of the four: both doors lead to one settings instance (checked by reference), a configuration survives being written and re-read from disk, and the reset asks before destroying anything - confirming clears, going back does not. Written, not yet run. Only RIMMSQOL stays out of reach
  - unverified: ConfigFile.PathFor/Folder/Export/Import stay untested even off-game - they all reach GenFilePaths.SaveDataFolderPath, and merely JIT-compiling that property throws outside a running Unity player; Tests/OffGame exercises the WorkStudioSettings/CustomWorkTypeEntry Scribe contract they wrap instead, at a path it computes itself
  - unverified: no in-game pass of English or French display yet (raw keys, clipping, fallback text) - the static localization gate is certified complete, but TRANSLATIONS.md tracks this runtime check separately and it must pass before claiming the translations tested in game. Since 2026-09-18 there is a scripted path for it: the four @review features attach screenshots of the editor, a renamed column, the drift dialog and the settings window, so running the suite once per language produces the evidence a person then reads. The English half is now done: the 2026-09-20 23:44 WSL run produced the six screenshots, collected into Art/Review, and she read them on 2026-09-21 and found nothing wrong - the three editor columns, the renamed column's width, the drift dialog's wording and the settings window all read correctly in English. The French half is still to run, with -Language 'French (Français)'; the staging now forces devMode, so a key missing from French will show as accented gibberish there instead of hiding behind clean English
  - unverified: the run believed on the evening of 2026-09-20 to have confirmed the fixture repair was not this suite's. PickleReports holds one report for the whole machine, and the summary.md written at 20:00:16 that evening belongs to Architect Studio - 58 scenarios, all of them categories, groups and the Architect window. Established from the runner's own state at http://localhost:27750/state, which names the mod each scenario belongs to: every "Work Studio - Pickle tests" scenario reads Pending, meaning the running game has never played them. The game started at 19:15 and the step assembly was built at 14:40, so that game does carry the repair; nothing has asked it to run. Tests/Pickle/Run-Pickle.ps1 was added the same evening, taking the machine-wide lock AUDIT.md prescribes and reading the outcome back per mod rather than from the shared report folder. A run of this suite did follow at 20:32-20:48, started in error by this session against the standing rule that it never launches RimWorld; its report is this suite's and is the one read above. 16 features, 4 failures: one Log.Error from VEF.Plants.WorkGiver_RemoveWeeds via YaOpt, two UnityEngine.InputLegacyModule, and scenario 12's own step defect, all three causes outside this mod
  - verified: the Pickle suite was put against the rule that Gherkin keeps only what a running game alone can show, 2026-09-20, with the 20:48 run's timings as the measure - 302.7s of machine time for 47 scenarios. Nothing was deleted, and the reasoning per feature is in Tests/Pickle/README.md so the next audit does not have to redo it. The expensive features are the ones that load a save and click, which is exactly what no unit test reproduces; the four cheapest come to 4.7s together. One real overlap stays: Tests/OffGame already reproduces scenario 8 against the real WorkTypeRuntime.Apply(), but 08-hide also shows the column leaving the drawn table and the colonist still picking CleanFilth up, and its two extra assertions ride along in a scenario that has to run anyway
```

## Historical audit record

The following sections retain their original dates and conclusions. Use the current audit
above for the present checkout, including where older paragraphs still say stage is done.

## Stage correspondence

`stage` here uses the coarse vocabulary of the automatic sweep (`port`, `showcase`, `preTest`,
`done`, `tested`, `published`). This audit instead worked the finer chain
`dansMonoRepo -> horsMonoRepo -> ModIcon générée -> Preview générée -> preOptions -> options ->
l10n -> preTest -> done -> tested`. Correspondence: `port` covers `dansMonoRepo` and
`horsMonoRepo`; `showcase` covers `ModIcon générée`, `Preview générée` and `preOptions`; `preTest`
covers `options` and `l10n` on top of the workflow's own `preTest`; `done` and `tested` are
unchanged. Work Studio is set to `done` since 2026-09-21: `horsMonoRepo`, `ModIcon générée`, `Preview générée`
and `preOptions` are all satisfied (see below), and so, since 2026-09-21, are `options`, `l10n`,
`preTest` and `preTest -> done`.

**Why this was `showcase` until 2026-09-21, and why that was wrong.** The stage was held on
`settings_audit: partial`, waiting for "an in-game/RIMMSQOL pass". AUDIT.md's interpretation rules
say the opposite in as many words: the `preOptions -> options` step rests on the code and the defs
plus the applicable automated tests, *n'exige pas de vérification en jeu*, and the interactive
in-game checks belong to `done -> tested` and do not block `options`. The blocker was recorded
before that rule existed and nobody re-read it afterwards. RIMMSQOL listing the def and drawing a
real button is another mod's own UI; Tests/Pickle's 13-settings builds this mod's MainButtonWorker,
activates it the way RimWorld would and asserts the window opens, which is the part that is ours.

**The gates, re-run on 2026-09-21, with what was actually executed:**

| Gate | Evidence |
| --- | --- |
| `preOptions -> options` | Settings reachable through Mod options with no XML editing, and the hidden MainButtons shortcut opens the same instance - both asserted in game by 13-settings, beyond what this gate requires |
| `options -> l10n` | `localization: complete`; Check-DefInjected over the shipped mod: 29 patch operations, 11587 defs indexed, 2 keys checked, **0 errors** |
| `l10n -> preTest` | About.xml declares one hard dependency, Harmony, with both a Workshop URL and a download URL, and `loadAfter` naming Harmony and the six Ludeon packages. Nothing else is used |
| `preTest -> done`, scenarios | TESTING.md, with preconditions, actions and expected results |
| `preTest -> done`, automated | Tests/OffGame, 2026-09-21: 0 failed, 1 skipped (a check that needs a live Unity player) |
| `preTest -> done`, Pickle | 47 of 47 green, 2026-09-21 08:51, in the WSL game |
| `preTest -> done`, XML | Check-DefRefs: well-formed, no unresolved reference, every reference on the right def type, every ParentName resolved. Check-XmlClasses: the one referenced type resolves. Check-DefInjected: 0 errors |
| `preTest -> done`, shipped version | `Mod/Assemblies/WorkStudio.dll` built 2026-09-19 11:40, newest source 11:39: the artefact is the sources |

`done` means ready for the final functional validation in game, not already validated there. What
`done -> tested` still wants is listed under `remaining`: the French display pass, and a real
RIMMSQOL install revealing the button.

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
- **Half of it is now verified in game (2026-09-18).** `Tests/Pickle`'s `13-settings.feature`
  builds the def's own `Worker` and activates it exactly as RimWorld would, and the run has it
  green: the worker is constructed, `Activate()` runs, and `Dialog_WorkStudioSettings` opens. A
  screenshot of that window is attached to the same report. **Not verified**: RIMMSQOL's own side —
  that it lists this def, that revealing it puts a real button on the bar, and that clicking that
  button is what opens the window. No test can close that one; it needs RIMMSQOL installed and a
  person looking. This remains open in `remaining`.
- The settings window itself now has a scenario (TESTING.md 13, added 2026-09-18) and a screenshot
  from a real run, so "what it draws" is recorded. Still missing: opening it through **Mod options**
  rather than the shortcut, changing an option and observing the effect, persistence across a
  reload, and the "Reset the whole setup" confirmation dialog.

Net: still `partial`. It is no longer "never exercised at runtime" — the shortcut door was opened
for real on 2026-09-18 and the window drawn and photographed. What keeps it from `complete` is
narrower than it was: the Mod options door, a value surviving a reload, the reset confirmation, and
RIMMSQOL. Those four are what `preOptions -> options` now waits on.

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

- `Mod/Languages/English/Keyed/WorkStudio.xml` and the French counterpart both hold exactly 76
  keys (63 at the audit; 13 added 2026-09-18 with the icon feature), and a full diff of the key
  lists is empty: no key exists in one language and not the other. `Tests/OffGame` re-checks that
  parity, and the placeholder parity, on every run rather than on trust.
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
  **A second, independent cause found and fixed the same night (2026-09-17), once Concord was
  disabled and tags were confirmed captured again**: Pickle tags a button by the text it actually
  draws, and this scenario's `I click button "Work types…"` step hardcoded the English label - on
  her French client the button draws `WorkStudio.OpenEditorShort`'s French translation ("Types de
  travail…"), so the tag would never have matched even with Concord fixed. Same class of bug found
  the same night in Architect Studio's own suite (a different hardcoded string). Fixed here by
  replacing the generic step with this mod's own `I click the Work types button` / `the Work Studio
  button is not drawn` (`ModSteps.cs`, `02-work-tab-button.feature`), which resolve the same key the
  button itself draws before building the tag. Not yet re-run live.
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

## In-game pass, 2026-09-18: the Work Studio suite alone, 29/37

Second run, this time filtered to Work Studio's own suite (37 scenarios, not the 162 of the whole
machine), Concord disabled as `Tests/Pickle/README.md` says. **29 passed, 8 failed.** Read from
`PickleReports/junit.xml` and `summary.md`.

**Three things the 2026-09-17 pass left open are now closed, green:**

- **Scenario 2 is 3/3.** "Work types… opens the editor" passes. The English-label bug fixed the
  night before was real and is gone: on a French client the step now resolves
  `WorkStudio.OpenEditorShort` and finds the button.
- **The MainButtons shortcut works at runtime.** "the hidden shortcut opens the settings window"
  passes — the def's own `Worker` is built and activated the way RimWorld would, and the window
  opens. That was `unverified` since the shortcut was written. Only RIMMSQOL's own side is left.
- **All five screenshot scenarios pass and their images are in the report**, so the `@review`
  pattern works end to end here.

**Scenario 5 is no longer evidence of anything in this mod.** All three failures are Pickle's
`Log.Error during scenario` guard firing on *other mods'* errors, not on a failed assertion:
`[Yet another Optimizer] Keeper threw exception in WorkGiver VEF.Plants.WorkGiver_ExtractFlower:
NullReferenceException` for the first, and the known `UnityEngine.InputLegacyModule` ReflectionOnly
framework error for the other two. The priority assertions themselves never failed. The 2026-09-17
reading of scenario 5 ("'Keeper' should have 3 for Pickle second; it has 0") did not recur.

**Scenario 8 still fails, and the diagnostic added for it did its job — by refuting the hypothesis
it was built to test.** The failure now reads:

    'Keeper' should have 2 for Cleaning; it has 0. disabled=False, visible=False,
    useWorkPriorities=True, hidden by the mod=True, raw DefMap value=0.

**The raw `DefMap` value is 0 too**, so Enhanced Work Tab answering `GetPriority` from its own store
is *not* what is happening: vanilla's own list holds 0. That hypothesis is closed. What the same
message shows instead is the whole map — `Firefighter=3, Patient=3, Doctor=0, PatientBedRest=3,
HaulingUrgent=0, …` — every value either 3 or 0, and nothing else. That is not a shifted list or a
corrupted one; **it has the shape of a pawn whose work settings were initialized from scratch**
(vanilla's `EnableAndInitialize` gives about six types a 3, plus the `alwaysStartActive` ones, and
leaves the rest at 0). A reconfiguration that merely hides a column cannot produce that shape, so
something is re-initializing or replacing the settings between the `When` and the `Then`.

Two diagnostics were added the same day to split what remains, both in `ColonySteps.cs`:

- `SetPriority` now asserts the **raw** `DefMap` value right after the call, not only `GetPriority`.
  That separates "the write never reached vanilla's list" from "it reached it and was overwritten
  afterwards" — the one question the `Then`-side reading cannot answer on its own.
- `Describe` now reports the pawn's `thingIDNumber` and whether **this suite** had to initialize its
  work settings. If a `Then` reads a pawn the suite itself initialized, the default-looking spread
  above is explained outright, and by the suite rather than by the mod.

**Scenario 6 is explained, and it was the test's fault, not the mod's.** The failure message shows
the arrow did not move `Cooking` at all; what moved is `Research`, from last in the "before" list
back to its vanilla third place, with every `naturalPriority` rewritten to vanilla values. The
editor's row list is a cache that `DoWindowContents` drops at the top of **every** draw pass — the
comment there says so, and names this exact hazard. A real arrow click therefore always acts on a
list rebuilt that same frame. The step, calling `ShiftType` directly with no repaint behind it, acted
on whatever the last pass had built — a list from before the sandbox reset between scenarios — and
`ApplyTypeOrder` then rewrote every priority from it. A drag never had the problem, because
`ReorderableWidget` only hands out its callback during a repaint, so those steps cannot run without
one; and indeed both drag scenarios pass. The four arrow steps now let the window draw first.

**Scenario 7 broke because of the screenshot feature added that morning, and that is fixed.**
"the header and its width follow the new name" asserts that a rename threw the column's worker away
(`workerInt == null`). The new `07b-rename-visual` opens the Work tab for its screenshot and left it
open; a drawn table rebuilds the worker on the very next repaint, so 07 found one and failed. `07b`
now closes the tab behind it. The mod was never involved.

**Scenario 12 still fails, differently, and is now linked to scenario 8.** The raw-save check reads
`position 0 (Firefighter) holds 3 positionally but 0 by name`. `Patch_WorkSettingsExposeData.Save`
builds the named dictionary from `settings.priorities.values` — the *same* list vanilla had written
as `<vals>` moments earlier in the same `ExposeData` pass — so the two cannot disagree unless that
list changed in between. Enhanced Work Tab does not patch `ExposeData` (checked: its patches are
`GetPriority`, `SetPriority`, `WorkGiversInOrder`, `CacheWorkGiversInOrder` and UI), so the writer is
something else. The value in question, Firefighter, is one of the 3s in scenario 8's default-looking
spread — the two failures are most likely the same unknown seen from two sides.

Net: of the three defects carried since 2026-09-17, **scenario 6 is closed as a test artifact,
scenario 5 is closed as other mods' log noise**, and scenario 8 remains — narrowed from "something
zeroes one priority" to "something hands this pawn a freshly initialized set of work settings", with
scenario 12 as its likely second face. Nothing here points at `WorkTypeRuntime.Apply()`, which
`Tests/OffGame` exercises end to end and which the run's own passing scenarios (4, 9, 10) depend on.

## Screenshot scenarios, 2026-09-18: the trip automated, the verdict left to a person

Several of the items just listed were stuck for the same reason — they are judged by eye, so no
assertion can close them — while the *trip* to the state they judge is exactly what this suite
already does well. Following the pattern SkillIcons and Architect Studio use (`@review` features
that assert nothing and attach a screenshot), four features were written:

| Feature | Covers | What a person then judges |
| --- | --- | --- |
| `03-three-columns` | TESTING.md 3, which had no automated coverage at all | the three columns read as described, in either language |
| `07b-rename-visual` | the visual half of 7 | the column is as wide as its new name needs |
| `10b-drift-warning-wording` | the wording half of 10 | four keys, four filled placeholders, nothing clipped |
| `13b-settings-visual` | the settings window, a TESTING.md scenario 13 added the same day | that it reads correctly through either door |

The asserting half went into `13-settings.feature` instead, and grew the same day into the three
checks that close most of the settings gate: both doors lead to one settings instance, a
configuration survives being written and re-read from disk, and the reset asks before destroying
anything. Building the `WorkStudio_Settings` def's own `Worker` and activating it the way RimWorld
would is what turned the "never exercised at runtime" half of the MainButtons item above into a
written test. Only RIMMSQOL's own side of it stays manual.

Three new steps carry the trips (`I open the work type editor`, `I select the work type`,
`I search the other tasks for`, plus `I open Work Studio's settings through the MainButtons shortcut`
and a frame-based `I let the Work Studio interface draw`). They wait frames rather than ticks on purpose: the
settings window sets `forcePause`, so a tick-based wait behind it would sit until its timeout.

**All four are written and build clean; none has been run.** Like everything else opened since the
2026-09-17 pass, they wait on a live run.

## The scenario 8 and 5 failures, named at last: 2026-09-20

Four runs of elimination ended here. `PriorityProbe`, added the day before, put a stack trace on
every `SetPriority(0)` for the watched pawn and type, and the run returned the same chain for both
scenarios:

    before Apply=2 | SetPriority(0) FROM Pawn_WorkSettings.SetPriority
      <- Pawn_WorkSettings.Disable
      <- Pawn_WorkSettings.Notify_DisabledWorkTypesChanged
      <- Pawn.Notify_DisabledWorkTypesChanged
      <- WorkStudio.PriorityMemory.Restore
      <- WorkStudio.WorkTypeRuntime.Apply
    | after PriorityMemory.Restore=0 | after Apply=0

**It is this mod's own code, and no other mod is involved.** `PriorityMemory.Restore` writes the
remembered priorities into the `DefMap`, then calls `pawn.Notify_DisabledWorkTypesChanged()` — which
is deliberate, and the reason is sound: restoring by name overwrites the zeroes vanilla puts on work
a pawn is not allowed to do, so they have to be reapplied. Vanilla's implementation walks
`pawn.GetDisabledWorkTypes()` and calls `Disable` on each, and `Disable` is `SetPriority(w, 0)`.

What makes it a defect rather than the intended behaviour is that **the game considers the type
disabled at that instant and not a moment later**: the same failure message reports
`disabled=False, visible=False` when the assertion reads it. So `GetDisabledWorkTypes()` answers
differently during `Apply()` than after it, and `Restore` calls it at exactly the wrong moment —
after `RebuildDefs` has reindexed the database and `ClearBackstoryCaches` has emptied every
backstory's cached list, so the first thing to ask for that list rebuilds it against a def landscape
that is still half-rewritten.

Vanilla's own `Pawn.GetDisabledWorkTypes` was read in full: nothing in it consults `visible`, so
hiding a column is not what makes the game call it disabled. The remaining candidates are what the
backstory cache is rebuilt *against* — a work type's `workTags`, which this mod rewrites on its own
types in `ComputeInheritance` — or a stale index. The probe was extended the same day to report, at
the instant of the zeroing, the disabled list it just built, the type's `workTags`, its index and
the pawn's backstories. One more run separates the two.

**Not fixed yet, and deliberately not guessed at.** The obvious move — stop calling
`Notify_DisabledWorkTypesChanged` from `Restore` — would reintroduce the bug that call exists to
prevent: a pawn keeping a priority on work it cannot do. The fix has to keep that guarantee while
not consulting a list built mid-rebuild, and which of the two mechanisms is at work decides how.

### Settled 2026-09-20, second run: the mod is not at fault, the fixture is

The extended probe answered in one run, and it reverses the verdict above:

    workTags=ManualDumb, Commoner, Cleaning, AllWork, visible=False, index=18,
    disabled list=[Hauling Cleaning HaulingUrgent KAU_UrgentHaul],
    backstories=[MusicalKid86:None Rancher43:ManualDumb]

Both halves were then checked against vanilla's own XML rather than taken on trust:
`Core/Defs/WorkTypeDefs/WorkTypes.xml` gives Cleaning exactly `ManualDumb, Cleaning, Commoner,
AllWork` — so **this mod never touched those tags**, and the stale-index theory dies with the
`workTags` one. `Core/Defs/BackstoryDefs/Shuffled/Offworld_Nonspecific_Adult.xml` gives `Rancher43`
(a *rancher*) a `workDisables` of `ManualDumb`.

**So Keeper genuinely cannot clean, and vanilla zeroing that priority is correct.** The four types
in the disabled list are exactly the `ManualDumb` ones. `PriorityMemory.Restore` calling
`Notify_DisabledWorkTypesChanged` is right, and so is what it does.

What went wrong is the scenario: it set a priority on work its colonist is not allowed to do. The
`Given "Keeper" can do the work types …` guard exists to prevent precisely that, and it passed —
because `Pawn.GetDisabledWorkTypes` answers from a cache nothing had invalidated yet, so the game
said "allowed" until `WorkTypeRuntime.Apply` cleared the backstory caches and the question was asked
honestly for the first time. The mod made the game tell the truth, and the truth was that the
scenario's premise was false.

The guard now drops `Pawn.cachedDisabledWorkTypes`, its permanent twin and every
`BackstoryDef.cachedDisabledWorkTypes` before asking, so it asks the real question. Its failure
message names the disabling backstory, what it disables and the type's own tags, and says explicitly
when the cached answer differed from the real one — which is how this hid for four runs.

**The fixture's colonist is generated with random backstories.** That is the deeper problem and it
is not fixed: any scenario naming a work type is a coin flip on whether that run's Keeper may do it.
Scenarios 5, 8 and 12 all name one. The next step is to stop hardcoding the type — pick one the
colonist can actually do, or give the fixture a colonist with no `workDisables` — and that is a
change to four feature files, left for a decision rather than made unilaterally.

**Repaired the same day, by her choice between the two options.** The fixture now gets a colonist
that forbids nothing: `Given "Keeper" is given backstories that disable no work type` swaps any
childhood or adulthood backstory whose `workDisables` is not `None` for one that forbids nothing,
removes any trait that disables work, drops the caches and checks that the pawn really has an empty
disabled list — naming what still forbids the work if something else does (a gene, a quest, an
ideoligion role, a life stage), since those would need a different answer.

Replacing the backstory rather than picking a work type at run time was the choice made: the
scenarios go on naming Cleaning, and now mean it. Skills and passions are untouched — only what the
colonist is permitted to do changes. The three Backgrounds that name a work type (05, 08, 12) call
it right after the colonist is created, and the honest guard that follows double-checks the result.

**Not re-run yet.** What this predicts: scenarios 5, 8 and 12 go green, since the only thing wrong
with them was a premise the game was right to refuse.

## Icons taken over from SkillIcons, 2026-09-18

Decided by Virginie, relayed by the SkillIcons session and confirmed by her here: the skill and
work-type icon feature moves into Work Studio, drawings included, and SkillIcons goes back to being
a passions-only mod. GrimWorks is dropped from the "Work tab column header" plan `BACKLOG.md`
carried — consistent with what that file already argued, since a GrimWorks "colour" is a category
and a category also moves the column. No fallback: without Work Studio there are no icons, which is
this mod's own "never two drawings in one place" rule applied.

What landed:

- **35 drawings**, 127 KB total, under `Mod/Textures/WorkStudio/Skills` (12) and
  `.../WorkTypes` (23). Namespaced under `WorkStudio/` rather than shipped at the top level, so a
  player who still has SkillIcons installed during the handover does not get two mods claiming the
  same `ContentFinder` paths.
- **`Source/UI/WorkTypeIcons.cs`**, the two Harmony prefixes, ported with identifiers in English
  per this repository's rule and hooked into the existing Harmony instance from `StartupInit`
  rather than a static constructor of its own. The skill prefix **shrinks the rect** and lets the
  game draw in what is left, which is what keeps it independent of `DrawSkill`'s internal layout.
  The header prefix returns `false` for "icon only" and re-registers the tooltip the suppressed
  label carried.
- **Three settings** (`showSkillIcons`, `showWorkTypeIcons`, `workTabHeaderMode`), in `ExposeData`
  only and deliberately **not** in `ExposeConfig`: importing someone else's setup should not change
  what your own screen draws.
- **13 Keyed entries**, English and French.
- **`Art/icons/gen-icons.js`**, the drawings' own source, so they stay changeable instead of
  becoming 35 PNGs nobody can redraw. Not wired to a build here — Work Studio has no `_tools`
  chain — and it carries the rule that comes with it: judge on the silhouette sheet **at 20 px**,
  never at 64, where all four failures SkillIcons records look fine.

Four defects in that settings UI were fixed while porting rather than carried over: the three
header-mode radios had no tooltip while the ones above them did, nothing introduced the group, they
were dead controls when work-type icons were off with nothing saying why (`MOD_SETTINGS.md` §3), and
the French read "colonnes du Work Tab" — untranslated, where RimWorld FR says *Travail*.

Two things `Tests/OffGame` now protects, since both patch targets are resolved by reflection and a
signature change would show as icons quietly not drawing: that `SkillUI.DrawSkill`'s four-argument
overload still exists with the rect in second place, and that a `DoHeader` is still found. **Worth
recording: it is declared on `PawnColumnWorker_WorkPriority` in 1.6**, so the fall-back that would
have patched every column, Name and Sex included, does not trigger. A third check reads the shipped
icon names against `SkillDefOf`/`WorkTypeDefOf`, so a drawing named after nothing cannot ship
silently.

**Not verified in game.** Nothing here has been seen drawing: the feature builds, its patch targets
exist, its names match, and that is all this session can say.

## Note, outside this workflow's ladder

`Mod/About/PublishedFileId.txt` and `workshop: 3792836684` show the mod was uploaded to the
Workshop at some point, ahead of where this audit finds it today. That is a pre-existing fact
about the account's Workshop item, not something this audit can undo, and it does not raise the
stage recorded here — the workflow being audited stops at `tested`, well short of `published`.

## Prior automatic sweep, 2026-09-12

The fields below were the previous automatic read-off, kept for history: `dependencies: declared`,
`showcase: complete`, `remaining: unverified: never seen running`. Confirmed still accurate by
this audit rather than replaced.

## done -> tested, criterion by criterion (2026-09-21, late evening)

`stage` stays `done`. This is what each line of AUDIT.md's `done -> tested` has behind it, and what does not.

| Criterion | State |
| --- | --- |
| Scenarios run in game and passed | Minimal set: 47 of 47 in English and in French, but **before the last changes to the mod** (the two-line rows of 09-21 10:19 for the English pass, the click steps, the type-name counter of `ed41491`). Has to be replayed on the final build. Coexistence sets: Better Work Tab 47/47, Compact Work Tab 47/47 (says nothing about the conflict), Work Tab 45/47 (unreplayed since), **Enhanced Work Tab's scenario 5 alone replayed 2026-09-22 and now 3/3** (below), the rest of that set not replayed since the fix |
| Pickle suites green, `@review` captures opened | Captures of both languages opened and read (2026-09-21); to redo on the final build |
| Logs checked | Scenario 1 asserts the log holds nothing from the mod since startup; the passes above were read for errors. To redo with the final build |
| FR and EN interface | Both passes done, captures read; the right-hand column was reworked because of what the French one showed |
| Options, persistence, MainButtons shortcut | Scenarios 13 and 13b (settings through the shortcut and through Mod options, write to disk and read back); RIMMSQOL listing the button stays another mod's UI and is not claimed |
| New game and existing save | **Existing save only** (`test-colony`). No scenario starts a new game; not covered, not claimed |
| Fixes followed by their regression tests | Scenario 5's fix is now confirmed (below). Scenario 2's wait is **not yet replayed** |

**Open, in the order they gate `tested`:**

1. **Scenario 5 under Enhanced Work Tab — fixed and confirmed, 2026-09-22.** `GetPriority` had read 2 for a type
   absent from the save while the raw list held 0. Cause, read from Enhanced Work Tab's decompiled source: it
   keeps each colonist's priority by `defName` in the save and never prunes it, and the editor reused a deleted
   type's `defName` for the next one created. Fix `ed41491` (names only go up). Replayed alone
   (`-Filter '05-priorities-across-save.feature'`, `avec-enhanced-work-tab`): **3 of 3 passed**, `exitReason: passed`
   (`Tests/Pickle/evidence/2026-09-22-scenario5-enhanced/`). **The rest of that 47-scenario set has not been
   replayed since**, and neither has this fix's effect on the two coexistence sets' own custom-type scenarios.
2. **Scenario 2 under Work Tab**: the button moves for about ten frames after the tab opens; the steps now wait
   until it has stood still (`PickleTools/ClickDiagnostics`). One replay attempt timed out in an 8-hour queue
   (2026-09-21 night), a second crashed the game itself before Pickle wrote a report (a native Mono GC segfault
   during mod startup, `pickle-reports-archive/0922-0822-nosummary/Player.log` — no sign it is caused by this
   mod's code or by ClickDiagnostics; queued again).
3. **The save-load exception under Work Tab** (an apparel read as a pawn): not explained, and not written up as
   anything but exploration.
4. The two `incompat-` sets have no scenario asserting the documented symptom yet; their runs are exploration.
5. Decisions that are Virginie's, not settled here: icons ON by default; the long renamed header spilling into its
   neighbours in capture 07b; whether the rule that one pass validates nothing applies to a stage already reached.

**2026-09-22, owner decision in chat: publish 1.1.0 now** — "ça me semble stable suffisamment pour
pousser une version" — without waiting for items 2 and the full Enhanced Work Tab replay. `stage`
stays `done`: this is a decision to publish over an incomplete `done -> tested`, not a claim that
`tested` is reached. Version prepared: `CHANGELOG.md` dated to `[1.1.0] — 2026-09-22`, `PUBLICATION.md`
written (screenshot order, dependencies checked, adult-content answer, draft Steam patch notes),
tag `v1.1.0` pushed. **Not done, and PUBLICATION.md says so:** the Workshop screenshots in
`Art/Workshop/` predate the two-line task labels this version ships and need retaking; the actual
Steam upload is the owner's own action, not this session's, per AUDIT.md.

Then, when 2 through 4 have come back and the full Enhanced Work Tab set has been replayed: one full English and
one full French pass of the minimal set on the final build.
