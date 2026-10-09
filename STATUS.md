---
localization: complete
translation_en: complete
translation_fr: complete
mod:          Work Studio
packageId:    nelim.workstudio
repo:         Rimworld-Work-Studio
visibility:   public
detached:     yes
stage:        published[1.2.2]
workflow_stage: published[1.2.2]
licence:      open
licence_at:   this mod's own code is MIT and nothing of Achtung! is redistributed, but the technique came from it and it is a named debt, so the source's licence decides: MIT, LICENSE-achtung.txt
settings_audit: complete
dependencies: declared
showcase:     complete
tested_on:    2026-09-21
publication_changelog_review_sha: b2e254a0ba32006211a278eb93eca18448c625e3
code_review_sha: c8e02c647a9190ac4f58365376033f5a6ec48496
workshop:     3792836684
  - "unverified: minimal French pass naming-fr-122 (run 2a96) of the 1.2.2 build, evidence/naming-fr-122, still to read. Minimal English 2d84 (51 passed, 0 failed, 11 skipped) and naming English e3c6 are green; French fd10 was red on a test expectation (English name where the French game translates the label), the check was made language independent. A red on 2a96 is a defect of the published version."
  - "open, design: two points the owner has not decided: the Skill icons are on by default; a long renamed Work tab header spills into its neighbours (BACKLOG.md, 'An overlong header of a type without an icon shows two letters'). Not blocking."
  - "open, not a regression: RIMMSQOL's DefMapSaveStateFix.Postfix throws loading a save written with one more custom work type than the game has (BACKLOG.md, 'Whether Work Studio can shield RIMMSQOL's save-load crash'). Either Work Studio pre-empts it or it is RIMMSQOL's own bug."
  - "open, untested: Mech Work Tab's own storage across a save (Work Studio does not disturb the vanilla priorities, evidence/mechworktab-2, BACKLOG.md) and the category UI with a custom work type."
  - "unverified: code review. code_review_sha is c8e02c6 (the 2026-10-05 /code-review, low effort); commits after it are not reviewed. The 2026-10-06 hand read covered Dialog_WorkTypes.cs and Dialog_EditWorkType.cs only."
  - "state 2026-10-09: 1.2.1 and 1.2.2 are published (tags v1.2.1, v1.2.2; 1.2.2 = run 37798803232 on cf88dfe5b417f886e8d38c2c643c4faea48fed3d, update_preview). Publication texts reviewed (publication_changelog_review_sha) and fixed; closing pass done (Art/ clean, root clean, gallery 4 files 3.3 MB); published-cleanup of the docs and evidence done 2026-10-09. Branch ci/steam-login-and-description-guard is NOT merged (it holds the removed semantic-release path and Mod/README.template.md of an earlier state): kept for the owner to decide."
  - "evidence kept (evidence/ and Tests/Pickle/evidence/, ignored by git): min-en-122, min-fr-122, naming-en-122, naming-fr-122 (1.2.2 build); gallery-3, gallery-8, gallery-13 (the sources of gallery images 3, 1, 2); 0925-ewt-full, 0926-better-work-tab, 0926-incompat-compact-worktab, 0926-rimmsqol, 0926-work-type-tag, 0927-fluffy-full (latest full pass of each optional set, 1.2.0 to 1.2.1 builds); mechworktab-2. Deleted 2026-10-09 as superseded: the ten 2026-09-23-* folders, 0925-fluffy-button-5, 0926-min-en, 0926-min-en-replay, 0926-min-fr, 0926-13-settings-fix, nolabel-2, bug2-bills-3. Earlier status text: docs/runs/status-history.md and git history of this file."
session:      01a0c844-1b24-7b61-8615-7c2ee4ae1b69
updated: 2026-10-09
---

# Work Studio — status

Published item 3792836684, stage `published[1.2.2]`. Everything open is in `remaining` above; run lines are in `docs/runs/`; the dated sections of this file up to 2026-10-09 (preview regeneration, audits, settings and translation audits, in-game passes of September, detachment, icons) are summarised in `docs/runs/status-history.md` and kept in full in git (this file at 139fcf1).
