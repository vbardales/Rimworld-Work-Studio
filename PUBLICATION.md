# Publication

What the Workshop page needs and this repository does not record anywhere else. Written once, for
this update and for whoever ships the next one. This is not the first publication — the item
(3792836684) already exists, published as 1.0.0 and updated to 1.0.1 — so the first-envoi steps of
AUDIT.md's `tested -> prepublished` do not all apply; what follows is scoped to an update.

## This update: 1.2.0

Prepared 2026-09-22 as "1.1.0", on the owner's word that the mod is stable enough to push, **with two
`done -> tested` items still open** (see `STATUS.md`): the button-click fix under Work Tab has not
been replayed since it was written, and the full 47-scenario Enhanced Work Tab set has not been
replayed since the defName fix — only that one scenario has, and it passed. Publishing over these
gaps is the owner's call, recorded here rather than hidden.

**Why 1.2.0.** The Workshop page shows a change note "1.1.1 — Update of mod's preview picture", posted by nelim17 on
2026-09-22 at 12:00, uploaded from the game (the owner's word, 2026-09-25). It is not the CI's: no tag, no GitHub
release and no publishing run of this repository match it. Its content is not established: it came about 13 minutes
after `c27b8af` ("Rebuild preview from preserved source", 11:47), so it most likely uploaded the mod folder as it stood
then (the work prepared as 1.1.0) and **not** what came after (`bb48d04`, the settings and Work tab overlap fix of
22:47, and everything later). The owner chose **1.2.0** on 2026-09-25 and `CHANGELOG.md` lists everything since 1.0.1
under it, so no subscriber misses a line whichever of the two uploads they got.

**How it is published now.** By `.github/workflows/publish-tag.yml`, generated from `Rimworld-Release-Admin`. The
semantic-release path (`release.yml`, `release.config.mjs`, `release-steam-plugin.mjs`, `package.json`) was removed on
2026-09-25: it computed the version from commits and tags, so with the stale `v1.1.0` it would have proposed 1.1.1, and
it could still be dispatched in publish mode next to the manual one. Now: the version is typed in, the
Steam change note is the fenced block under `### 1.2.0` below, the release notes are the `## [1.2.0]` section of
`CHANGELOG.md`, and the description (only when `update_description` is on) is `Mod/README.template.md`, converted by the
workflow ("Steam description" below). See `Rimworld-Release-Admin/docs/OPERATIONS.md`: a dry-run of the exact commit first,
`publish` with its full 40-character SHA, and only the owner approves `steam-production`.

**Before the actual Steam upload, still to do:**

- Run the publish with `update_description` on. The live description already says only what the template says about
  Compact Work Tab (the dry-run of 2026-09-25 shows no such sentence on the page), so the description would change
  by one line: the new bullet about icons. The rest of its diff is list formatting only (the renderer indents each
  `[*]` and drops the space after it). RimWorld does not resend `About.xml`'s description on an update, so without
  the option the page keeps the old text.
- Nothing blocks the upload any more on the two open runtime checks: under the fail-fast policy below they run
  **after** the publication. They are `02-work-tab-button.feature` against `wsl-deps.incompat-fluffy-worktab.map`, and the
  full `wsl-deps.avec-enhanced-work-tab.map` set, filed as small tickets through the TicketDispatcher; neither has run
  since the fixes they are meant to confirm.
- Upload the three Workshop screenshots of `Art/Workshop/` in the order of their names. They are done and
  follow the rules below; the owner has seen the Work tab one and said it is beautiful, the editor and the settings
  were sent to her too.
- Decide the two open design points STATUS.md lists (icons on by default; the long renamed header
  spilling into its neighbours in capture 07b) — not blocking, but worth a look before they are
  permanent on every subscriber's screen.

## Publication policy: fail fast

The owner's rule (2026-09-25): **publish, then let the tests speak; if they come back red, publish a rollback and a
fix.** A mod that has passed its dry-run and whose known gaps are written down is not held back for the runs still
queued behind thirty other sessions' tickets.

- The dry-run of the exact commit still comes first (`Rimworld-Release-Admin/docs/OPERATIONS.md`), and the owner still
  approves `steam-production`. Fail fast removes the wait for the remaining game tests, not the gates of the pipeline.
- The tests still open at publication time run right after it, one small ticket each. Their verdict goes in
  `STATUS.md` and `docs/runs/`. A red result is a defect of the published version, said as such.
- **On a red result, roll back first, then fix.** The rollback is a new publication, since version numbers only go up
  and `publish-tag.yml` refuses a tag that exists: dispatch it with `ref` = the full SHA of the last good commit and
  the next patch number, and give the change note "Rolls back to <what>, because <what failed>". The fix is a
  second, later publication of its own.
- **Choose the rollback target before publishing, not after the red run.** For 1.2.0 the only tagged good state is
  `v1.0.0` (`53215a5`); 1.0.1 has no tag and 1.1.1 (a preview-only upload from the game, content unknown) has no
  commit. Write down the commit to fall back to with the owner if it is needed, and tag the next good version, so the
  next rollback has a target.

## Workshop screenshots: the rules, and where each image stands

**The rules**, decided with the owner on 2026-09-24. They apply to every Workshop image of this mod and are the
only place they are written; `14-publication-shots.feature` points here.

1. **The scene is the owner's showcase colony**, `Nelim-Zen-Meadow-Studio`, never the test fixture. It is the fixture
   `nelim-zen-meadow-studio` of `PickleTools/ScreenshotStudio`, only present in a pass that stages that companion:
   `wsl-deps.studio.map`. Run it in English, the page is English.
2. **Two kinds of image, two treatments:**
   - **An option window** (this mod's own windows: the editor, the settings): the game's screenshot mode is on
     (HUD and Pickle panels hidden) and the image is **cropped tight around the window**, its rectangle plus 16 px
     of margin on every side. PNG.
   - **An interface window** (a game window this mod changes, such as the Work tab, a main tab): the **full
     interface**, screenshot mode off, **no crop**, the whole 1920x1080 frame, so the tab bar under it (Architect,
     Work, Schedule...) says where the panel comes from. The game never highlights the open tab, so do not count on
     that. Saved as JPEG at quality 95 when the PNG is over 2 MB (on the meadow: about 4 MB as PNG, 0.75 MB as JPEG).
3. **Named as a player would name it**: no defName, no test name, and short enough that a column header reads in
   full (`Tidying`, not `Hauling and tidying`, which drew as `HA` for want of an icon; see `BACKLOG.md`).
4. **Each upload stays under 2 MB.** Crop, never resample: resizing with interpolation blurs flat pixel-art regions
   and can make a PNG bigger despite fewer pixels. Check with `(Get-Item <file>).Length / 1MB`.
5. **Every image is opened and looked at before it is called ready**, and its composition is the owner's call: a
   passed scenario proves the state, not that the picture sells the mod.
6. **A capture on the map** (an object or a pawn shown zoomed and cropped; this mod has none today, the rule is
   for the day it has): aim at an **orange zone** of the studio, and never at the black tiles, where the subject
   drowns; or at the grass **just above the smiley** emblem. On the grass, **circle in red what is worth seeing**
   so the eye finds it, since the meadow is busy.
7. **Where files live: `Art/Workshop/` holds only the images to upload, numbered `01-`, `02-`, `03-`... in the order they
   go on the page, nothing else** (no older versions, no raw captures, no subfolder: an image that is not to be uploaded
   does not stay there). Raw captures go to `Art/Workshop/studio-raw/` (ignored by git, delete once cropped). Produced by `Tests/Pickle/Mod/Pickle/Features/14-publication-shots.feature`,
   collected with `Art/Update-WorkshopScreenshots.ps1`.

**Where each image stands (2026-09-25).** Steam shows the first one large; the order is the Work tab first for its
impact, then the editor, then the settings. Shots in `Art/Workshop/`, all on the showcase colony, all in English,
type `Tidying`:

| File | Kind | Size | State |
| --- | --- | --- | --- |
| `01-the-new-column.jpg` | interface window | 1920x1080, 0.75 MB | seen by the owner 2026-09-25 ("c'est beau") |
| `02-the-editor.png` | option window | 1210x790, 0.24 MB | captured 2026-09-24, cropped to the window + 16 px |
| `03-the-settings.png` | option window | 930x730, 0.16 MB | same |

The Work tab shot comes from `docs/runs/2026-09-24.md`; the editor and settings from the same feature, same map. The
older images (2026-09-18 and 2026-09-20: they predated the two-line labels and the duplicate-row defNames, or sat on the
fixture desert) were deleted on 2026-09-25, so that the folder can be uploaded as it is.

`Preview.png` and `ModIcon.png` (in `Mod/About/`) ship with the mod. The Preview was regenerated and
visually reviewed on 2026-09-22 at 896×504 and 545,873 bytes. Its text-free source, deterministic
HTML/CSS composition and palette remain under `Art/`; STATUS.md records the handoff and QA.

## Dependencies and DLC

Unchanged since 1.0.0, verified against the source, not the intention:

- **Hard dependency**: `brrainz.harmony` (Harmony) — the mod does not run without it, correctly a
  `modDependencies` entry.
- **DLC**: none required. `loadAfter` lists every DLC so this mod's patches apply after them when
  present, but nothing in `modDependencies` names one, and no code path assumes a DLC is active.
- **Incompatibility**: `Fluffy.WorkTab`, declared in `<incompatibleWith>` and in the description.
  `Mlie.CompactWorkTab`, `natee.EnhancedWorkTab` and `Coolnether123.BetterWorkTab` are neither
  incompatible nor a dependency — coexistence, tested (see STATUS.md) — and are not listed here.

## Adult content

**No.** Nothing in this mod's assets, descriptions or screenshots is adult content. The Workshop
checkboxes for mature/adult content are answered No.

## Steam patch notes

The publish workflow reads the note of the version it publishes from the fenced block under `### <version>` of
this file. BBCode. It matches the `## [<version>]` section of `CHANGELOG.md`; keep the two in step.

**Every note starts with its version number, on a line of its own** (`[b]1.3.0[/b]`), then the sections. The owner noticed
on 2026-09-25 that the 1.2.0 note, as shown on the Workshop page, does not say which version it is: the block below is
kept as it was published, and the rule holds from the next version. A published note can only be changed by hand on the
Steam page, so it is not edited here after the fact.

### 1.2.0

```
[b]Added[/b]
[list]
[*] An icon for each skill and each work type, in the character tab and at the foot of each Work tab column. Three settings pick what shows.
[*] A hidden MainButtons shortcut, for RIMMSQOL and similar mods to reveal, opening the same settings as Mod options.
[/list]

[b]Changed[/b]
[list]
[*] A task name that does not fit its column now wraps to two lines instead of being cut.
[*] The grey type shown beside a task in the right-hand column is measured from what is actually listed, instead of a fixed width.
[*] Two tasks sharing a label now each show their defName in grey, in full, on a line of their own under the label, so they can be told apart.
[*] New preview picture.
[/list]

[b]Fixed[/b]
[list]
[*] A work type you create could take the name of one you had just deleted, and with it, its old priority for colonists. Confirmed with Enhanced Work Tab installed, which is what exposed it. Names now only ever go up.
[*] A column header too long for its column no longer spills over the next one: it shows its icon, and the full name in the tooltip.
[*] The settings window's footer no longer covers the note about how priorities are saved.
[*] With Enhanced Work Tab, the Work types button sits clear of its "Done editing" button.
[*] The editor's help line was clipped as soon as it wrapped onto two lines.
[*] Work Studio is no longer declared incompatible with Compact Work Tab: they coexist. Fluffy's Work Tab remains incompatible.
[*] A packaging mistake could throw a FieldAccessException on some runtimes; not observed in the shipped game, fixed regardless.
[/list]
```

## Steam description

Sent only when `update_description` is on for a publish. The source is `Mod/README.template.md`, in Markdown, converted to
Steam BBCode by the workflow itself (`.github/publish.config.json`: `"format": "markdown"`, the `--description-markdown`
option of `generate-publish-workflow.sh`). The dry-run prints the converted text, its size, its SHA-256 and a line diff
against the page, so the text sent is read before the approval. Nothing is copied by hand any more: until 2026-09-25 the
BBCode was pasted here from a render of the template, and the two could drift. The template sits in `Mod/` but is listed
in `Mod/.steamignore`, so it does not reach players. Steam's limit is 8000 bytes; the description is about 5.3 KB.

## Thank-you messages

Not part of this update's publish; tracked separately, still open from an earlier session:
Workshop links for **Work Type Tag** and **RIMMSQOL** are needed before drafting their messages, and
it is not established which thank-you comments (if any) already exist on the mods this one is
inspired by or coexists with. Draft, personalize, and post **after** this version goes public — a
link to a still-updating item is fine to post, but check the linked item is the one you mean.

## After the Steam upload

- Commit `Mod/About/PublishedFileId.txt` immediately if it ever changes (it should not, for an
  update to an existing item) — losing it before a commit makes the next envoi create a second item.
- Do not create the tag or the release by hand: `publish-tag.yml` creates `v1.2.0` and the GitHub release (with the
  `## [1.2.0]` section of `CHANGELOG.md`) once the upload has succeeded, and refuses to start if the tag exists.
- Then check the public page: title, new update time, the change note, the description if it was sent, and upload the
  three images of `Art/Workshop/` by hand (no tool of the chain can send a gallery).

**The tag `v1.1.0`** was pushed by hand on 2026-09-22 at `bf89753` (09:07), before the owner's upload from the game at
12:00 the same day, which she considers the published 1.1.x and whose Workshop change note reads "1.1.1". The tag matches
neither that label nor, most likely, that upload's commit (about `c27b8af`, 11:47), and no GitHub release goes with it.
**It stays, as history: it is not deleted** (the owner's word, 2026-09-25). `publish-tag.yml` only looks at the tag of the
version it publishes, so it does no harm. The commit that was actually uploaded on 2026-09-22 carries no tag.
