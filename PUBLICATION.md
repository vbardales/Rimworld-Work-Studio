# Publication

What the Workshop page needs and this repository does not record anywhere else. Written once, for
this update and for whoever ships the next one. This is not the first publication — the item
(3792836684) already exists, published as 1.0.0 and updated to 1.0.1 — so the first-envoi steps of
AUDIT.md's `tested -> prepublished` do not all apply; what follows is scoped to an update.

## This update: 1.1.0

Prepared 2026-09-22, on the owner's word that the mod is stable enough to push, **with two
`done -> tested` items still open** (see `STATUS.md`): the button-click fix under Work Tab has not
been replayed since it was written, and the full 47-scenario Enhanced Work Tab set has not been
replayed since the defName fix — only that one scenario has, and it passed. Publishing over these
gaps is the owner's call, recorded here rather than hidden.

**Before the actual Steam upload, still to do:**

- Edit the existing Workshop description manually to remove the old Compact Work Tab
  incompatibility sentence and retain only the documented Fluffy Work Tab conflict. RimWorld does
  not resend `About.xml`'s description on an update.
- Replay `02-work-tab-button.feature` against `wsl-deps.incompat-fluffy-worktab.map`, and the full
  `wsl-deps.avec-enhanced-work-tab.map` set. Both are one command away
  (`scripts/Run-PickleWsl.ps1 -Mod WorkStudio -DepMap <map> ...`); neither has run since the fixes
  they are meant to confirm.
- Upload the three Workshop screenshots of `Art/Workshop/ready/` in the order of their names. They are done and
  follow the rules below; the owner has seen the Work tab one and said it is beautiful, the editor and the settings
  were sent to her too. The older ones in `Art/Workshop/` must not be uploaded.
- Decide the two open design points STATUS.md lists (icons on by default; the long renamed header
  spilling into its neighbours in capture 07b) — not blocking, but worth a look before they are
  permanent on every subscriber's screen.

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
7. **Where files live:** raw captures in `Art/Workshop/studio-raw/` (ignored by git, delete once cropped), finished
   files in `Art/Workshop/ready/`. Produced by `Tests/Pickle/Mod/Pickle/Features/14-publication-shots.feature`,
   collected with `Art/Update-WorkshopScreenshots.ps1`.

**Where each image stands (2026-09-25).** Steam shows the first one large; the order is the Work tab first for its
impact, then the editor, then the settings. Shots in `Art/Workshop/ready/`, all on the showcase colony, all in English,
type `Tidying`:

| File | Kind | Size | State |
| --- | --- | --- | --- |
| `01-the-new-column.jpg` | interface window | 1920x1080, 0.75 MB | seen by the owner 2026-09-25 ("c'est beau") |
| `02-the-editor.png` | option window | 1210x790, 0.24 MB | captured 2026-09-24, cropped to the window + 16 px |
| `03-the-settings.png` | option window | 930x730, 0.16 MB | same |

The Work tab shot comes from `docs/runs/2026-09-24.md`; the editor and settings from the same feature, same map. The
older images of `Art/Workshop/` (`01-the-editor.png`, `02-the-new-column.png`, the 2026-09-18 ones) predate the two-line
labels and the duplicate-row defNames, or sit on the fixture desert: do not upload any of them.

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

## Steam patch notes, 1.1.0

Written here so the update tab is not blank at the moment of the envoi (AUDIT.md's own warning:
that tab is easy to forget because nothing prompts for it until the form is open). BBCode, plain
text otherwise — matches the style of the `[Unreleased]` -> `[1.1.0]` section of `CHANGELOG.md`.

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
[*] Two tasks sharing a label now each show their defName in grey beside it, so they can be told apart.
[/list]

[b]Fixed[/b]
[list]
[*] A work type you create could take the name of one you had just deleted, and with it, its old priority for colonists — confirmed with Enhanced Work Tab installed, which is what exposed it. Names now only ever go up.
[*] The editor's help line was clipped as soon as it wrapped onto two lines.
[*] A packaging mistake could throw a FieldAccessException on some runtimes; not observed in the shipped game, fixed regardless.
[/list]
```

## Thank-you messages

Not part of this update's publish; tracked separately, still open from an earlier session:
Workshop links for **Work Type Tag** and **RIMMSQOL** are needed before drafting their messages, and
it is not established which thank-you comments (if any) already exist on the mods this one is
inspired by or coexists with. Draft, personalize, and post **after** this version goes public — a
link to a still-updating item is fine to post, but check the linked item is the one you mean.

## After the Steam upload

- Commit `Mod/About/PublishedFileId.txt` immediately if it ever changes (it should not, for an
  update to an existing item) — losing it before a commit makes the next envoi create a second item.
- `git tag v1.1.0` and push it, so the Workshop version and the repository agree on what shipped.

**The tag is not at the release commit yet.** `v1.1.0` was pushed on 2026-09-22 at `bf89753`, and the
`[1.1.0]` section of `CHANGELOG.md` has been edited since (the duplicate task's defName on its own line,
the confirmation of scenario 5), so the notes describe code the tag does not contain. Nothing has been
uploaded to Steam, so nothing is wrong yet, but before the upload: fix the last commit, then re-point
the tag at it (`git tag -f -a v1.1.0 <commit>` and a forced push of the tag, which needs the owner's word),
or cut the tag again under a new number. Anything changed after that goes under `[Unreleased]`.
