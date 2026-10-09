# Publication

This document records what the Workshop page needs but the repository records nowhere else. Written once, for
this update and for whoever ships the next one. This is not the first publication — the item
(3792836684) already exists, published as 1.0.0 and subsequently updated through 1.2.1 — so the first-upload steps of
AUDIT.md's `tested -> prepublished` do not all apply; what follows is scoped to an update.

## The 1.2.0 update (published 2026-09-25, kept as history)

Prepared 2026-09-22 as "1.1.0", on the owner's word that the mod is stable enough to push, **with two
`done -> tested` items still open** (see `STATUS.md`): the button-click fix under Work Tab has not
been replayed since it was written, and the full 47-scenario Enhanced Work Tab set has not been
replayed since the defName fix — only that one scenario has, and it passed. Publishing over these
gaps is the owner's call, recorded here rather than hidden.

**The Workshop history, as the owner pasted it on 2026-09-25** (the change notes of the page; all uploaded from the game
except the last):

| When | Note | What was actually in the mod |
| --- | --- | --- |
| 2026-09-22 09:09 | `1.1.0 — 2026-09-22`, Added / Changed / Fixed | The commit of the tag `v1.1.0` (`bf89753`, pushed 09:07). |
| 2026-09-22 12:00 | `1.1.1 Update of mod's preview picture` | The same DLL and, from git (`c27b8af`, 11:47): the new Preview, the Compact Work Tab entry removed from `incompatibleWith` in `About.xml`, the ATTRIBUTION copy. The note says only the preview. |
| 2026-09-25 11:27 | The 1.2.0 note (Added / Changed / Fixed, no version number) | Commit `7a9ffec`, by the CI (`publish-tag.yml`, run 36118066093). |

**Why 1.2.0.** 1.1.0 and 1.1.1 were taken, and what follows them is a new DLL (the settings and Work tab overlap fixes,
the `defName` of a duplicate on its own line), which the owner named 1.2.0. `CHANGELOG.md` has one section per version,
split from what the notes say; the 1.2.0 note, as published, repeats the items of 1.1.0 and 1.1.1 along with its own.

**How it is published now.** By `.github/workflows/publish-tag.yml`, generated from `Rimworld-Release-Admin`. The
semantic-release path (`release.yml`, `release.config.mjs`, `release-steam-plugin.mjs`, `package.json`) was removed on
2026-09-25: it computed the version from commits and tags, so with the stale `v1.1.0` it would have proposed 1.1.1, and
it could still be dispatched in publish mode next to the manual one. Now: the version is typed in, the
Steam change note is the fenced block under `### 1.2.0` below, the release notes are the `## [1.2.0]` section of
`CHANGELOG.md`, and the description (only when `update_description` is on) is `Mod/README.template.md`, converted by the
workflow ("Steam description" below). See `Rimworld-Release-Admin/docs/OPERATIONS.md`: a dry-run of the exact commit first,
`publish` with its full 40-character SHA, and only the owner approves `steam-production`.

**Published 2026-09-25** by `publish-tag.yml` (tag `v1.2.0`, runs in STATUS.md): the description and the change note went with it, and
the two fail-fast runtime checks, which had been moved to after the upload, ran afterward (STATUS.md has their verdicts). What is still open:

- The Workshop images of `Art/Gallery/` were uploaded by hand by the owner on 2026-10-08, in the order of their names (`0-` is
  the Preview). All three were validated first (table under "Workshop screenshots").
- Decide the two open design points STATUS.md lists (icons on by default; the long renamed header
  spilling into its neighbours in capture 07b) — not blocking, but worth a look before they are
  permanent on every subscriber's screen.
- The next change note starts with its version on a line of its own (`[b]1.3.0[/b]`): the CI refuses it otherwise.

## The 1.2.1 update (published 2026-10-08, kept as history)

Published by the CI (run `37789139646`, tag `v1.2.1`) on `a1029b9`, **without** `--preview`: the Workshop page kept the older Preview. The dry-run and the publish are
recorded in STATUS.md.

## The 1.2.2 update (published 2026-10-08, kept as history)

Published 2026-10-08 on `cf88dfe5b417f886e8d38c2c643c4faea48fed3d` by `publish-tag.yml` (run `37798803232`, `update_preview=true`, approved by the owner).
Only the Preview changed (the ModIcon in the corner, `Art/Preview.config.json` without its inward `translate` override; `Mod/About/Preview.png`).
Tag `v1.2.2` and the GitHub release were created by the CI. `CHANGELOG.md` has `## [1.2.2]` and the Steam note is the block under `### 1.2.2`.
A Preview-only update needs `--preview` (`update_preview=true`) on the dry-run and the `publish`, otherwise the picture is not sent.

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

1. **The scene is Nelim's sanctuary**, the fixture `Nelims-tribe` shared by every mod's gallery, never the test fixture. Since
   2026-10-08 it lives in its own repository, SanctuaryBacklot (`docs/GALERIE.md`, `docs/SANCTUAIRE-LIEUX.md`); the pass that stages it,
   with the mods that dress the pawn, is `wsl-deps.gallery.map`. Run it in English, the page is English. Two prefixes in the feature
   tell the tools apart: `Nelim's Sanctuary:` (the places) and `Nelim's Pickle Tools:` (everything else). Choose the place by
   reading every named one: a full-screen interface window goes on `window-backdrop-for-height`, cropped on the sides (PUBLISHING.md,
   2026-10-06), a main tab on a place that sells the game (`water-garden`). Before 2026-10-06 the scene was
   `nelim-zen-meadow-studio` (`wsl-deps.studio.map`, now removed).
2. **Two kinds of image, two treatments:**
   - **An option window** (this mod's own windows: the editor, the settings): the game's screenshot mode is on
     (HUD and Pickle panels hidden), the window sits on `window-backdrop-for-height`, and the image is **cropped on the sides**
     around the window, with a margin of backdrop, **the height of the frame kept whole** (1080 px; PUBLISHING.md, 2026-10-06).
     PNG.
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
7. **Where files live: `Art/Gallery/` holds only the images to upload, `0-preview.png` (a byte copy of `Mod/About/Preview.png`) first,
   then `1-`, `2-`, `3-`... in the order they go on the page, nothing else** (no older versions, no raw captures, no
   subfolder). Raw captures stay in the evidence folder of the run (ignored by git) and the chosen ones are copied by hand.
   Produced by `Tests/Pickle/Mod/Pickle/Features/14-publication-shots.feature`, one scenario per image.
   A capture proposed for the owner to judge goes there too while it is judged, as `<index>-candidate-<name>.png` (for example
   `2-candidate-morning-the-new-column.png`, with a run id suffix when there are several), not committed; the rejected ones are removed,
   the chosen one becomes `<index>-<name>.png`.
8. **The gallery tells one story** (PUBLISHING.md, 2026-10-06): one morning at the sanctuary, in ONE place at three close
   hours, in the order of the scenarios of the feature. `1-early-morning-the-editor` (8: the editor, a full-screen window,
   so on `window-backdrop-for-height`, cropped on the sides), `2-morning-the-new-column` (9: the Work tab, in the story place `water-garden`),
   `3-late-morning-the-settings` (10: the settings window, same place). Each name carries its number and its moment. Played on 2026-10-06 (ticket `afa7`, `f3d594f`), read, and re-run for the new
   backdrop rule, read and cropped on the sides, the height of the frame kept whole.

**Where each image stands (2026-10-08).** The gallery tells one story in the order of its names (rule 8). Files of `Art/Gallery/`,
all in English, type `Tidying`, scene = the sanctuary:

| File | Kind | Size | State |
| --- | --- | --- | --- |
| `0-preview.png` | the Preview | 590 KB | byte copy of `Mod/About/Preview.png` |
| `1-early-morning-the-editor.png` | option window | 1320x1080, 1.2 MB | validated by the owner 2026-10-07 (run `cbb5`, `evidence/gallery-8`, on `window-backdrop-for-height`) |
| `2-morning-the-new-column.jpg` | interface window | 1920x1080, JPEG 95, 593 KB (the PNG was 2.8 MB) | validated by the owner 2026-10-08 (run `e3ef`, `evidence/gallery-13`: Nelim smiling, carrying a log) |
| `3-late-morning-the-settings.png` | option window | 1120x1000, 1.0 MB | approved, cropped (run `87ce`, `evidence/gallery-3`) |

The raw captures stay in the evidence folders of those runs (ignored by git). The older images (2026-09-18, 2026-09-20 and
2026-09-24) were deleted, so that the folder can be uploaded as it is.

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

### 1.2.2

```
[b]1.2.2[/b]

[b]Changed[/b]
[list]
[*] The Workshop Preview now shows the mod icon in its bottom-left corner. No change to the mod itself.
[/list]
```

### 1.2.1

```
[b]1.2.1[/b]

[b]Changed[/b]
[list]
[*] A work type that has only a short label now reads that way in the editor (Cook, Hunt, Construct...), as it does in the Work tab columns, instead of by its internal name.
[*] Wording pass on the English and French texts: settings, reset confirmation, saved setups, drift report, icon descriptions and the hidden MainButtons shortcut.
[*] New preview picture and mod icon.
[/list]

[b]Fixed[/b]
[list]
[*] With Fluffy's Work Tab, the Work types button no longer sits on top of that tab's "Expand all priorities" toggle, which took the click and widened the window instead of opening the editor. Fluffy's Work Tab is still declared incompatible.
[*] A work type added by another mod with no label no longer shows as a blank or an internal name: it takes its short label, or its internal name when it has neither.
[/list]
```

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
- Do not create the tag or the release by hand: `publish-tag.yml` creates `v<version>` and the matching GitHub release after a successful upload, and refuses to start if the tag exists.
- Then check the public page: title, new update time, the change note, the description if it was sent, and upload the
  images of `Art/Gallery/` by hand (no tool of the chain can send a gallery).

**The tag `v1.1.0`** (`bf89753`, pushed by hand at 09:07 on 2026-09-22) is the commit that was uploaded from the game at
09:09 as 1.1.0: it is right, and it stays (the owner's word, 2026-09-25). `v1.1.1` has no tag: the 12:00 upload was of
about `c27b8af`, and a tag is not created after the fact. `publish-tag.yml` only looks at the tag of the version it
publishes.
