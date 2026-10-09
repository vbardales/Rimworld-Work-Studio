# Publication history (moved out of PUBLICATION.md on 2026-10-09)

Update sections and Steam notes of versions already published. The notes below are the texts as published; the full former file is in git (`git show 139fcf1:PUBLICATION.md`).

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


## Steam notes as published

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

