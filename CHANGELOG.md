# Changelog

Format inspired by [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
This file serves the repository and the writing of Steam patch notes; RimWorld does not display it in game.

## [1.0.1] — 2026-09-02

### Fixed

- The **Work types…** button did not appear when another mod replaces the Work tab. Those mods declare their own window in the `MainButtonDef`'s `tabWindowClass`: they inherit from the vanilla class but override `DoWindowContents` without calling `base`, so the patched method never ran. The patch now targets whichever class is actually in use. Checked against Better Work Tab.
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
- Incompatible with Work Tab (`Fluffy.WorkTab`) and Compact Work Tab (`Mlie.CompactWorkTab`), which also rebuild the Work tab's columns. The incompatibility is declared in `About.xml`, so the game warns you itself.
- Removing the mod returns moved tasks to their original types.
