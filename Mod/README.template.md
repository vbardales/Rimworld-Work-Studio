Reorganize the Work tab from inside the game, without restarting.

- Create your own work types and fill them with tasks taken from other types — pull “tend animals” out of Handling, or separate cooking from serving.
- Rename, reorder and hide columns, including vanilla columns.
- Drag and drop at two levels: the order of work types and the order of tasks within a type.
- Use the up/down arrows as an alternative to dragging, including on Steam Deck.
- Import and export setups between games.

Everything applies immediately. No def file to write and no restart required.

# YOUR PRIORITIES ARE SAFE

RimWorld stores colonist priorities in a list indexed by position, without recording which work type each value belongs to. Adding or removing a work type — yours, or one supplied by another mod — can therefore shift the whole list.

Work Studio stores priorities by defName in the save and restores them against the correct work type when loading. This protects vanilla and modded work types alike.

# GOOD TO KNOW

- Work Studio does not create new tasks. It redistributes existing tasks.
- A type fed from several sources inherits the disabling rules of each source. Work combining medicine and hauling is disabled for a colonist incapable of either.
- Incompatible with [Work Tab](https://steamcommunity.com/sharedfiles/filedetails/?id=3552152340), which preserves its own startup-time column list and restores it when its tab opens.
- Removing Work Studio returns moved tasks to their original work types.
- A hidden MainButtons shortcut is available for [RIMMSQOL](https://steamcommunity.com/sharedfiles/filedetails/?id=1084452457) and similar customization mods. Revealing it opens the same settings as Mod options → Work Studio.

# WITH BETTER WORK TAB

Work Studio and [Better Work Tab](https://steamcommunity.com/sharedfiles/filedetails/?id=3626737803) agree as long as you leave the latter’s columns alone. It only records a column order after you drag a column there. Until then, the order configured in Work Studio controls both the columns and the order in which colonists move between work types.

After columns have been reordered there, its saved order wins for the work types it recorded. Reordering those types in Work Studio will no longer affect their execution order. A work type created afterwards is absent from that saved list, so it appears last until positioned there.

Everything else remains available: creating work types, moving and ordering tasks, renaming, hiding columns and protecting priorities.

Interface available in English and French.

# IF I GO QUIET

If I do not answer within a reasonable time after being contacted, anyone may freely update this or any other of my mods, including publishing a continuation of it. All credit must be preserved.

AI-GENERATED

This mod's code was written with Claude Code (Anthropic) and Codex (OpenAI). Its images were generated with DALL-E (OpenAI) and OpenAI ImageGen, under human direction, review and testing. Stated openly: designing with these tools is my job.

THANKS

[Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077), by Andreas Pardeike (pardeike / Brrainz), without which none of this would exist.

Andreas Pardeike (pardeike / Brrainz), and [Achtung!](https://steamcommunity.com/sharedfiles/filedetails/?id=730936602), whose MIT-licensed DynamicWorkTypes.cs is the reason work types can be changed without a restart.

Above all, 0。0, who pointed at that technique publicly and explained where to find it — without that pointer this mod would not exist at all.

Densevoid and [Personal Work Categories](https://steamcommunity.com/sharedfiles/filedetails/?id=2722053051), where the pointer was shared.

Thanks to [Work Tab](https://steamcommunity.com/sharedfiles/filedetails/?id=3552152340), [Better Work Tab](https://steamcommunity.com/sharedfiles/filedetails/?id=3626737803), [Compact Work Tab (Continued)](https://steamcommunity.com/sharedfiles/filedetails/?id=3250322299) and [Enhanced Work Tab](https://steamcommunity.com/sharedfiles/filedetails/?id=3715873875), whose distinct approaches made the compatibility boundaries worth checking directly.

Thanks to [RIMMSQOL](https://steamcommunity.com/sharedfiles/filedetails/?id=1084452457) and [Work Type Tag](https://steamcommunity.com/sharedfiles/filedetails/?id=3779138895) for the optional integrations tested here.

[Pickle](https://steamcommunity.com/sharedfiles/filedetails/?id=3791648678) and [RimLogging](https://steamcommunity.com/sharedfiles/filedetails/?id=3733484696) made the automated in-game evidence possible. [Nelim's Pickle Tools](https://steamcommunity.com/sharedfiles/filedetails/?id=3806142401) supplied shared test steps. These are development tools only, never dependencies of the distributed mod.

What is reused and how it differs is detailed in ATTRIBUTION.md. This mod is MIT licensed.

[Source code on GitHub](https://github.com/vbardales/Rimworld-Work-Studio)
