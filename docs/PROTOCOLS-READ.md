# Protocols read

Which rule documents this mod's session read, at which version, and which ones turned out not to help. Written so that
after the next context compaction the session re-reads only what has moved, as `Rimworld-Ticket-Dispatcher/docs/WELCOME.md`
(point 5) asks: the version of a file is the last commit that touched it, and `modified, not committed` when
`git status --short -- <file>` shows `M`.

To see what moved since this reading:

```bash
git -C <repo> log --oneline <hash>..HEAD -- <file>
```

**Read 2026-09-25, in full, after a compaction.** Repository heads at the time: monorepo root `9afdc758`,
PickleTools `6c1d976`, Rimworld-Release-Admin `d403592`, Rimworld-Ticket-Dispatcher `79668cc`, this repository `7ee198b`.
Every file below was clean (no `M`) when read.

## Shared rules

| File | Version read | Read | Useful to this mod? |
| --- | --- | --- | --- |
| `AGENTS.md` | `90d51374`, 2026-09-25 | full | **Yes.** The evidence rule (reports are gitignored, one text line per run in `docs/runs/`, delete a superseded archive), and "Publishing by CI": dry-run first, publish by full SHA, only the owner approves, the CI creates the tag and the release. |
| `AUDIT.md` | `90d51374`, 2026-09-25 | full | **Yes, the most.** The Pickle section (submit a request, never launch; a request carries no SHA; small tickets), `done -> tested` (no `@wip`, every `@requires` played, nothing manual left), and the fail-fast policy of `prepublished -> published`, worded by the owner more strictly than the first version of it I wrote. Not used here: the sound-recording rules, the ModIcon ownership rule (this session never generates one). |
| `PUBLISHING.md` | `90d51374`, 2026-09-25 | full | **Partly.** Used: "Images" (the upload folder holds only the images, numbered), "À chaque mise à jour" (fail fast), "Publier par la CI". Not useful to an update of an existing item: description written once, packageId, licence suffixes, GitHub topics and social preview, the whole "Pièges d'outillage" (git hygiene of the monorepo). |
| `TRANSLATIONS.md` | `90d51374`, 2026-09-25 | full | **Not this time.** The mod's text has not changed since the 2026-09-21 audit (`localization`, `translation_en`, `translation_fr` are `complete`). Re-read if a player-facing string changes. |
| `STYLE_RIMWORLD.md` | `90d51374`, 2026-09-25 | full | **No.** It is about the Preview and the ModIcon; the Preview was regenerated on 2026-09-22 and the ModIcon belongs to the owner. |
| `scripts/SEARCHING.md` | `90d51374`, 2026-09-25 | full | **No.** Nothing to search in the Workshop corpus for this work. |

## Pickle and the machine

| File | Version read | Read | Useful to this mod? |
| --- | --- | --- | --- |
| `PickleTools/README.md` | `2b7b6d0`, 2026-09-25 | full | **Yes**: the table of tools (ScreenshotMode, ClickDiagnostics, ScreenshotStudio) and how a pass map stages them. |
| `PickleTools/Headless/README.md` | `b2712fc`, 2026-09-25 | full | **Partly.** Used: the launcher's exit codes, the filter terms (`::scenario` targets a scenario, a bare phrase is read as the mod's name), "one mod, several passes", what a report is. Not needed: the sleep-freeze diagnosis, the reservation, the orphan-game handling, the Pickle build notes. |
| `PickleTools/docs/steps.md` | `7268217`, 2026-09-25 | full | **Only as a lookup.** Generated; it now lists the two `developer mode` steps added to ScreenshotMode on 2026-09-25. |
| `Rimworld-Ticket-Dispatcher/docs/WELCOME.md` | `79668cc`, 2026-09-25 | full | **Yes.** Filter terms, `-DepMap` rules, "a request carries no SHA", "note the version you read". |
| `Rimworld-Ticket-Dispatcher/docs/SUBMIT.md` | `79668cc`, 2026-09-25 | full | **Yes.** Every option of `Submit-PickleRun.ps1`, the launcher's exit codes (a code 1 with `no-report.txt` is an infrastructure stop, not a red), `-Label` with the SHA. |

## Publishing

| File | Version read | Read | Useful to this mod? |
| --- | --- | --- | --- |
| `Rimworld-Release-Admin/docs/OPERATIONS.md` | `d403592`, 2026-09-25 | full | **Partly.** Used: the dry-run rule, "Publish workflow template", what a publish sends, what the tooling cannot send (the gallery). Not needed: credentials setup, Skill Icons 1.0.2, the 0.1.0 first publications, the semantic-release path (removed from this repository). **New since my first reading: `--description-markdown FILE`**, which converts a Markdown file to BBCode inside the workflow and would remove the duplicated description block in `PUBLICATION.md` (see below). |

## This repository

| File | Version read | Read | Notes |
| --- | --- | --- | --- |
| `STATUS.md` | `5c9772b`, 2026-09-25 (edited afterwards, front matter only) | full | Front matter and the recent sections matter; the July-September history is archaeology. The front matter was stale (`updated`, two `remaining` entries) and was brought up to date on 2026-09-25. |
| `README.md` | `3330aa1`, 2026-09-22 | full | Accurate. It does not mention the icons (1.2.0); not a defect, a gap. |
| `CHANGELOG.md` | `761888f`, 2026-09-25 | headings and the 1.2.0 section (written this session) | Current. |
| `ATTRIBUTION.md` | `c27b8af`, 2026-09-22 | full | Accurate; the distributed copy `Mod/ATTRIBUTION.md` was synchronised on 2026-09-22 (not compared again today). |
| `LICENSE` | `65f89fa`, 2026-09-20 | first lines | MIT, unchanged. |
| `PUBLICATION.md` | `04dcc84`, 2026-09-25 | headings and the parts written this session | Current: policy, image rules, patch notes, description block. |
| `TESTING.md` | `7786c77`, 2026-09-23 | full | Explains what each scenario proves and which passes exist. Not useful for the publication itself; useful to interpret a red. |
| `BACKLOG.md` | `d03dae9`, 2026-09-25 | the icons entry's head (written this session), lines 660-809 in full; lines 75-660 (licences of the linked mods, the SkillIcons handover, what was verified for Busywork, the colonist bar, Work Type Tag, GrimWorks, the action menu) **by headings only** | Re-read those lines before starting the icons work. |
| `docs/runs/README.md` | `c780f95`, 2026-09-23 | full | **Useful**: what to keep and delete from an evidence folder (keep `summary.json`, `junit.xml`, `summary.md` and this mod's `@review` captures; delete `Player.log`, `messages.ndjson`, `report.html`). Applied to this session's four evidence folders on 2026-09-25. |
| `docs/runs/2026-09-22.md` … `2026-09-25.md` | `f0e5c86`, `c780f95`, `5c9772b`, `7ee198b` | the 24th and 25th written this session; the 22nd and 23rd not re-read | Day records. |
| `Tests/Pickle/README.md` | `5c9772b`, 2026-09-25 | full | Accurate after the Art/Workshop change. |
| `Mod/About/About.xml` | `3330aa1`, 2026-09-22 | full | **Stale against the live page**: its description still says "Handler", has the shorter AI-generated and thanks lines, and lacks the icons bullet. RimWorld does not resend it on an update, so it harms nothing on Steam, but it is not what the page says. |
| `NOTES.md`, `BUGS.md` | do not exist in this repository | | Nothing to read. |

## What this reading changed in what I do

1. **A request carries no SHA.** The mod is staged when the ticket is played, from the working tree of that moment. My
   requests of 2026-09-24 and 25 did not put the SHA in `-Label`. From now on: the SHA in the label, and the tree left
   alone until `RUN_DONE`.
2. **Evidence.** `Player.log` and `messages.ndjson` do not stay in an evidence folder: deleted from the four of this session.
3. **The fail-fast gates.** AUDIT.md now says what stays required before a `publish`: no red scenario without a green replay,
   the gallery, the owner's manual validations, the rollback target. 1.2.0 was published with one exception, the Work
   types button under Fluffy Work Tab (red on 2026-09-23, fix written, replay not yet run), which the owner approved knowing
   it. It stays a red to close, or a rollback: recorded in `STATUS.md`.
4. **Commits in the monorepo root** use the pathspec on the commit (`git commit -- <paths>`), because the index is shared with
   other sessions. Mine of 2026-09-25 (`24c0b266`) held only my two files, checked afterwards with `git show --stat`.
5. **To decide with the owner** (not done): use `--description-markdown Mod/README.template.md` in the publish workflow instead
   of the BBCode block copied into `PUBLICATION.md`, so the two cannot drift; bring `About.xml`'s description in line with
   the page.
