---
localization: complete
translation_en: complete
translation_fr: complete
mod:          Work Studio
packageId:    nelim.workstudio
repo:         Rimworld-Work-Studio
visibility:   public
detached:     yes
workflow_stage: mountPreview[1.2.2]
licence:      open
licence_at:   this mod's own code is MIT and nothing of Achtung! is redistributed, but the technique came from it and it is a named debt, so the source's licence decides: MIT, LICENSE-achtung.txt
settings_audit: complete
dependencies: declared
showcase:     complete
tested_on:    2026-09-21
publication_changelog_review_sha: b2e254a0ba32006211a278eb93eca18448c625e3
code_review_sha: c8e02c647a9190ac4f58365376033f5a6ec48496
workshop:     3792836684
session:      01a0c844-1b24-7b61-8615-7c2ee4ae1b69
remaining:
  - "2026-10-10: feature - two points the owner has not decided: the Skill icons are on by default; a long renamed Work tab header spills into its neighbours (BACKLOG.md, 'An overlong header of a type without an icon shows two letters'). Not blocking."
  - "2026-10-10: defect - RIMMSQOL's DefMapSaveStateFix.Postfix throws loading a save written with one more custom work type than the game has (BACKLOG.md, 'Whether Work Studio can shield RIMMSQOL's save-load crash'). Either Work Studio pre-empts it or it is RIMMSQOL's own bug."
  - "2026-10-10: unverified - Mech Work Tab's own storage across a save (Work Studio does not disturb the vanilla priorities, evidence/mechworktab-2, BACKLOG.md) and the category UI with a custom work type."
  - "2026-10-10: unverified - code review. code_review_sha is c8e02c6 (the 2026-10-05 /code-review, low effort); commits after it are not reviewed. The 2026-10-06 hand read covered Dialog_WorkTypes.cs and Dialog_EditWorkType.cs only."
  - "2026-10-10: unverified - echo_review_sha absent (AUDIT.md 10.a, the field is a commit sha since 2026-10-10), social_preview_sha256 absent (10.f; the repository is public and og:image points to an uploaded image, but its hash was never recorded; Mod/About/Preview.png is ca6a4100...), tested_on still 2026-09-21 although runs went on to 2026-10-09, FRENCH_REVIEW.md predates the Repository type line of Make-FrenchReview.ps1 (text unchanged, regenerate), TEST_SCENARIOS.md absent (TESTING.md carries the scenarios), French agreements not re-read sentence by sentence in this audit."
  - "2026-10-10: unverified - scenarios 18, 19 and 20 had their step names changed (the word Work Studio added, audit 7.b fix) and have not been replayed in game; the PickleSteps DLL was rebuilt. The Pickle sets rimmsqol, work-type-tag, mechworktab, incompat-compact-worktab and incompat-fluffy-worktab were not replayed on 1.2.2 (queued then cancelled)."
  - "2026-10-10: defect - Mod/About/About.xml holds em dashes (description), flagged by the new Check-Status rule; the description comes from Mod/README.template.md, so both change together and ship with the next version. Not corrected by the audit."
  - "2026-10-10: unverified - protocols_read_sha stays at 06263cb0: Mark-ProtocolsRead.ps1 refuses while PUBLISHING.md of the protocols repository has uncommitted changes (another session); mark again once it is committed."
updated: 2026-10-10
protocols_read_sha: 06263cb0d19e6e3cf21e0145cad5ce348d9a4a69
---

# Work Studio — status

Published item 3792836684, state `mountPreview[1.2.2]` (audit 2026-10-10, see below). Everything open is in `remaining` above; run lines are in `docs/runs/`; the dated sections of this file up to 2026-10-09 (preview regeneration, audits, settings and translation audits, in-game passes of September, detachment, icons) are summarised in `docs/runs/status-history.md` and kept in full in git (this file at 139fcf1).

## Current notes (2026-10-10)

- verified 2026-10-09, French minimal pass of the 1.2.2 build: min-fr-122 (fd10) 62 scenarios, 50 passed, 1 failed, 11 skipped, the one red being a test expectation (English name where the French game translates the label), not a defect of the mod. The check was made language independent and replayed alone: naming-fr-122 (run 2a96, filter 20-work-type-without-label::vanilla types, setName sans-facultatifs, log says language French (Français) requested French) 1 of 1 passed, exitReason passed, no mod error in Player.log (two load notices of PickleTools companion mods with no Defs). The whole French suite was not replayed after the fix; the other 50 scenarios are those of fd10. Minimal English 2d84 green, naming English e3c6 green.
- state 2026-10-09: 1.2.1 and 1.2.2 are published (tags v1.2.1, v1.2.2; 1.2.2 = run 37798803232 on cf88dfe5b417f886e8d38c2c643c4faea48fed3d, update_preview). Publication texts reviewed (publication_changelog_review_sha) and fixed; closing pass done (Art/ clean, root clean, gallery 4 files 3.3 MB); published-cleanup of the docs and evidence done 2026-10-09. Branch ci/steam-login-and-description-guard deleted 2026-10-09 (superseded by main, confirmed by the CI/CD session).
- WSL cleaned 2026-10-09 (read-only check, nothing removed): the WSL steamcmd cache holds two items of this mod's passes, Compact Work Tab 3250322299 and Fluffy Work Tab 3552152340 (3 MB), and both are still named by SkillIcons wsl-deps maps (avec-compactworktab, avec-krypt-worktab), so they stay. Better Work Tab, Enhanced Work Tab, Mech Work Tab, RIMMSQOL and Work Type Tag come from the owner's Windows subscriptions, not from the WSL cache. Branch ci/steam-login-and-description-guard: no CI/CD session reachable (TicketManager relayed to Virginie 2026-10-09), deleted 2026-10-09 on the owner's word after the CI/CD session confirmed main supersedes it (local; origin already gone).
- evidence kept (evidence/ and Tests/Pickle/evidence/, ignored by git): min-en-122, min-fr-122, naming-en-122, naming-fr-122 (1.2.2 build); gallery-3, gallery-8, gallery-13 (the sources of gallery images 3, 1, 2); 0925-ewt-full, 0926-better-work-tab, 0926-incompat-compact-worktab, 0926-rimmsqol, 0926-work-type-tag, 0927-fluffy-full (latest full pass of each optional set, 1.2.0 to 1.2.1 builds); mechworktab-2. Deleted 2026-10-09 as superseded: the ten 2026-09-23-* folders, 0925-fluffy-button-5, 0926-min-en, 0926-min-en-replay, 0926-min-fr, 0926-13-settings-fix, nolabel-2, bug2-bills-3. Earlier status text: docs/runs/status-history.md and git history of this file.

- 2026-10-10 audit (AUDIT.md section 16) on 0bd5640, working tree clean: followUp[1.2.2] -> writeTests[1.2.2]. Established: build of Source is byte-identical to Mod/Assemblies/WorkStudio.dll (sha256 eb180f45...), 0 warnings; Check-DefInjected 2 keys 0 errors; About.xml, dependencies (Harmony hard, Fluffy incompatible, guarded ModsConfig.IsActive for Enhanced and Fluffy), no LoadFolders needed (single version 1.6); description in About.xml follows Mod/README.template.md and ends with the Source code line; ATTRIBUTION copies equal; Art/ minimal set, gallery 0-3 contiguous (3.3 MB, each under 2 MB), 0-preview.png byte copy of Preview.png; root clean. Not established: the off-game suite is red (two defects above) so criterion 7.b fails; the code review is stale (12.a; the publication review is per x.y line since 2026-10-10, now checked by the linter); items in the unverified entry above. The live Pickle non-regression tickets (seven optional-set passes) are valid on the unchanged Mod/ and continue; the stage goes back to followUp once the defects are fixed and the suite is green.
- 2026-10-10 correction after the audit: the off-game defect and the LICENSE defect are fixed (commit of the same day: off-game 156 PASS, 0 FAIL; Mod/LICENSE equals the root LICENSE, a change to Mod/ that ships with the next version). 7.b now holds, so the first unmet criterion is 10.a / 10.f: echo_review_sha is not recorded and social_preview_sha256 was never recorded; stage mountPreview[1.2.2]. Non-regression 2026-10-10: Better Work Tab 8879 and Enhanced Work Tab 502c green (docs/runs/2026-10-10.md).
