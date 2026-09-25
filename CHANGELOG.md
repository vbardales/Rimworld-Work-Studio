# Changelog

Format inspired by [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
This file serves the repository and the writing of Steam patch notes; RimWorld does not display it in game.

## [Unreleased]

## [1.2.0] — 2026-09-25

Published by the CI (`publish-tag.yml`, tag `v1.2.0`). Its Workshop note lists the changes of 1.1.0, 1.1.1 and this version together, without a version number; the sections here are split by version.

### Changed

- **The `defName` of two tasks that share a label is now drawn in full on a line of its own under the label**, instead of beside it.

### Fixed

- **A work type whose name is too long for its column header no longer spills over the next column.** The header shows its icon, and the full name in the tooltip, instead.
- **The settings window's footer no longer covers the note about how priorities are saved** (it hid the French one altogether). The window is larger and the redundant footer button is gone.
- **With Enhanced Work Tab, the Work types button sits further left**, clear of that mod's "Done editing" button, which was on top of it.

## [1.1.1] — 2026-09-22

Published from the game at 12:00, with a Workshop change note that says only "Update of mod's preview picture". The compiled mod is the one of 1.1.0; `About.xml` and the preview changed.

### Changed

- The Workshop preview picture was regenerated in a near-orthographic RimWorld-like presentation, keeping the Work Studio title and explanation while making the reordering action clearer at thumbnail size.

### Fixed

- Removed the incorrect incompatibility declaration for Compact Work Tab. Its 1.6 code patches the current vanilla work-priority columns in place and explicitly supports mods that add work types; it does not keep a competing column list. Fluffy's Work Tab remains incompatible because it restores a startup-time column snapshot that cannot contain types created later by Work Studio.

## [1.1.0] — 2026-09-22

Published from the game at 09:09 (tag `v1.1.0`, commit `bf89753`).

### Added

- **An icon for each skill and each work type** — in the character tab's skill list, and at the foot of each Work tab column header. Three settings decide what shows: icons in the skill list, icons on the columns, and whether a header carries its icon, its label, or both. "Icon only" gives noticeably narrower columns and keeps the tooltip that says what the column is.
- The drawings come from the **SkillIcons** mod, which handed the feature over on 2026-09-18 and keeps only its passion icons. They are monochrome on purpose: in that mod colour means "which passion", and letting it also mean "which skill" would make both unreadable. 12 skills and 23 work types, nine of which deliberately share their skill's drawing — *Cook* and *Cooking* name one domain.
- A hidden MainButtons shortcut (`WorkStudio_Settings`), for RIMMSQOL and similar MainButtons customization mods to reveal. It opens the exact same settings window as Mod options → Work Studio, against the same settings instance, so both routes share values and persistence.

### Changed

- **A task whose name does not fit its column now takes two lines** instead of being cut. A name that fits is left on one line, so the list keeps its rhythm; only past two lines is anything dropped, and then from the middle. This is what a French client showed first — the same names are longer there — but it applies to any language and any mod whose task names run long.
- The grey type shown beside each task in the right-hand column took a fixed 42% of the row, whatever it held. It is now measured from the types actually listed, and the width it no longer needs goes to the task name.
- When two tasks in the same column carry the same label, each now shows its `defName` in grey beside it. Vanilla alone has several — *construct placed frames* belongs to both Construction and Art, *carry to growth vat* appears twice — and a mod list adds more; the rows were indistinguishable in a column whose whole purpose is clicking one of them rather than the other. A label shown only once is left alone.

### Fixed

- **A work type you create no longer takes the name of one you deleted.** The editor gave a new type the first free name, so deleting a type and creating another reused its defName — and anything that keeps data by defName then handed the new type the old one's values. Enhanced Work Tab does this: it keeps each colonist's priorities by defName in the save and never prunes them, so the new type could read the priority the deleted one had. Names now only go up, counted per installation; a setup made before this update continues above its highest existing type. Found by the in-game test *a save written with one type, loaded with two*, which read 2 where the game's own list held 0. The cause was read from Enhanced Work Tab's code. Confirmed in game on 2026-09-23: with Enhanced Work Tab loaded, the three scenarios of that feature pass. (A first "confirmation" on 2026-09-22 was withdrawn: its report belonged to another test.)
- The help line under the editor's search box was drawn in a fixed-height box, so it was clipped as soon as it wrapped — which a long work type name is enough to cause, and a player can rename a type to anything. It is measured now.
- `Source/WorkStudio.csproj` combined the Publicizer with `GenerateAssemblyInfo=false`, which silently dropped the waiver that lets this mod read and write a few private fields of `Pawn_WorkSettings` — the same fields `PriorityMemory.Restore` touches on every startup and every edit. Left alone, this was a `FieldAccessException` waiting to happen on at least one runtime (confirmed on the desktop CLR; never observed in the shipped game itself, whose Mono may not enforce the same check). One line restores the waiver.

## [1.0.1] — 2026-09-02

### Fixed

- The **Work types…** button did not appear when another mod replaces the Work tab. Those mods declare their own window in the `MainButtonDef`'s `tabWindowClass`: they inherit from the vanilla class but override `DoWindowContents` without calling `base`, so the patched method never ran. The patch now targets whichever class is actually in use, and takes the rectangle by position rather than by name — Harmony injects parameters by name, and the parameter is called `rect` in vanilla but `inRect` in Better Work Tab, which was enough for the patch to be refused outright. Checked against Better Work Tab, in game.
- The up/down arrows at the ends of a list still placed an invisible clickable area, which swallowed the click and played its sound. A disabled arrow is now only a drawing.

### Added

- Compatibility with [baku] Work Type Tag, which shows the work type's name in front of a colonist's current job and colours the matching column header. It already handled types created here — it indexes by `defName` and derives a colour from the name — but kept its labels in a cache nothing cleared: renaming a work type left the old name in front of colonists' jobs until a restart. Soft-linked by reflection, with no dependency added.

### Changed

- The editor's right-hand column now shows, in grey, the type each task already belongs to, and is titled "Tasks from other types". Both columns listed tasks with nothing to tell them apart: membership only showed in a tooltip.
- A help line under the search box says what a click will do.

## [1.0.0] — 2026-08-30

First version. RimWorld 1.6.

### Added

- Creating work types from inside the game, without restarting.
- Moving tasks from one type to another, including out of base game types.
- Renaming, reordering and hiding the Work tab's columns.
- Drag and drop on two levels: the order of types among themselves, the order of tasks within a type.
- Up/down arrows doubling for drag and drop, for mouseless use.
- Importing and exporting configurations.
- Colonist priorities saved by name in the save file, instead of the base game's position-indexed list: changing configuration or mod list no longer shifts them. This also protects base game work types and those from other mods.

### Notes

- This mod does not create new tasks, it redistributes the ones that exist.
- A type fed from several sources inherits the incapabilities of each.
- At release time, Work Tab (`Fluffy.WorkTab`) and Compact Work Tab (`Mlie.CompactWorkTab`) were both declared incompatible. The Compact Work Tab declaration was removed later after its 1.6 implementation was inspected directly; see Unreleased.
- Removing the mod returns moved tasks to their original types.
