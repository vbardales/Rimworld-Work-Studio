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
- Retake the three Workshop screenshots (below): the current ones in `Art/Workshop/` are from
  2026-09-20 23:48, one day before the two-line task labels and the measured grey-type width shipped
  in this version. They show the OLD single-line, fixed-width layout — not what 1.1.0 draws.
- Decide the two open design points STATUS.md lists (icons on by default; the long renamed header
  spilling into its neighbours in capture 07b) — not blocking, but worth a look before they are
  permanent on every subscriber's screen.

## Screenshot order

**Each upload stays under 2 MB.** Pickle's raw captures are ~2-3 MB PNGs at 1920x1080; cropping the
empty margin (never resampling — resizing with interpolation blurs flat pixel-art regions and can
make the file BIGGER despite fewer pixels, as it did on the first attempt for image 2 below) is
usually enough on its own. Check with `(Get-Item <file>).Length / 1MB` before calling one ready.

Steam shows the first one large: it should be the most demonstrative, not the prettiest. Order,
with what each proves, and where the file to upload is:

1. **The three-column editor** with a work type a player would actually make ("Hauling and
   tidying", fed from Hauling and Cleaning). This is what the mod IS.
   **`Art/Workshop/01-the-editor.png` — not ready.** It shows the bug 1.1.0 fixes: two "Carry to
   growth vat" rows in the right-hand column, both truncated and indistinguishable ("Carry..." /
   "Haul..."). Cropping does not fix that; it needs retaking against the current build (queue
   `14-publication-shots.feature`, English, then `Art/Update-WorkshopScreenshots.ps1`), and this
   slot stays empty until then.
2. **The new column, seen from the Work tab**: it exists immediately, no restart.
   **`Art/Workshop/ready/02-the-new-column.png`, 1600×890, 1.78 MB — ready.** Cropped 2026-09-22 from
   the original 1920×1080 (`x 0-1600, y 190-1080`) to trim the empty sky and the mostly-empty desert
   on the right (solar panels already half cut off there); the priorities panel and the built rooms
   fill more of the frame. Nothing in the dialog was cut.
3. **The settings window**, opened through Mod options, so the page shows there is a configuration
   surface beyond the editor.
   **`Art/Workshop/ready/03-the-settings.png`, 1620×1000, 1.66 MB — ready.** Cropped 2026-09-22 from
   1920×1080 (`x 150-1770, y 40-1040`) to trim the wide empty desert on both sides and a sliver top
   and bottom; the dialog itself (`x 510-1408, y 190-890` in the original) is untouched.

The originals stay in `Art/Workshop/`; the cropped, upload-ready files are in `Art/Workshop/ready/`.
Slot 1 has no ready file yet — upload 2 and 3 in the meantime only if the page cannot wait, and
replace this note once 1 is retaken and cropped the same way.

Produced by `Tests/Pickle/Mod/Pickle/Features/14-publication-shots.feature` (English only — the page
is English) and collected with `Art/Update-WorkshopScreenshots.ps1`, then cropped by hand.

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
