# Attribution

## Where this mod comes from

It would not exist without two people.

**0。0**, who reported the technique publicly and explained where to find it:
https://steamcommunity.com/profiles/76561198380244407 — that is the starting point of everything
below. Without that message, hot-editing work types would have stayed buried in Achtung!'s code
and this mod would not have been written.

**Densevoid**, for *Personal Work Categories*, where that report was shared:
https://steamcommunity.com/sharedfiles/filedetails/?id=2722053051

## Achtung! - Andreas Pardeike (pardeike / Brrainz)

Workshop: https://steamcommunity.com/sharedfiles/filedetails/?id=730936602
Repository: https://github.com/pardeike/RimWorld-Achtung-Mod
Licence: MIT (see `LICENSE-achtung.txt`)

Achtung! is where this technique was first made to work. Everything else in this mod follows from
it.

The hot-reload sequence for `WorkTypeDef`s comes from Achtung!'s `Source/DynamicWorkTypes.cs`:
clear `workGiversByPriority`, call `DefDatabase<T>.ClearCachedData()` then
`ResolveAllReferences(false, true)` to reindex, regenerate the Work table's columns, and refresh
every pawn. The same file also circulates as a gist:
https://gist.github.com/pardeike/6ae015b86e5f909be93bdabd8316b078 (with no explicit licence, so
the Achtung! repository version, under MIT, is what served as the reference).

What was taken as is: the order of the reload calls, the idea of a postfix on
`Pawn.GetDisabledWorkTypes` so that a derived type inherits the incapabilities of its source type,
and the rebuilding of `PawnTableDefOf.Work`'s columns.

What deliberately differs:

- **Pawn priorities are remembered by `defName`**, not by index. `DefMap<D,V>` stores its values in
  a plain `List<V>` aligned on def order (`DefMap.ExposeData` writes `vals` positionally), so any
  insertion or removal shifts everyone's priorities. Achtung! handles that shift with an
  `Insert`/`Remove` at the computed index, which only holds for one change at a time; Work Studio
  applies a whole configuration at once and therefore has to capture and restore by name.
- **Achtung!'s `values.Remove(index)` is not reused**: on a `List<int>`, that call removes the first
  value *equal to* the index, not the element *at* that index.
- **Every pawn is handled** (`PawnsFinder.All_AliveOrDead`), not only spawned ones: a caravan or a
  pawn in a pod also has priorities to realign.
- **Derived incapabilities go through `workTags` first**, the union of the source types' tags, which
  makes filtering by backstory and by trait work without patching anything.

## The skill and work type icons — SkillIcons (this author)

The 35 monochrome drawings under `Mod/Textures/WorkStudio/`, and the two Harmony prefixes that
draw them, come from **SkillIcons**, another mod by the same author, which handed the feature over
on 2026-09-18 (its commit `80d3446`) and kept only its passion icons. Nothing here is third-party
work: no licence question arises, and the note exists so the drawings' origin — and the rule that
comes with them — is not lost.

The rule is theirs and it is not decoration: **this set is monochrome, shape alone**, because in
SkillIcons colour already means "which passion", and letting it also mean "which skill" would make
both unreadable. Their source is kept at `Art/icons/gen-icons.js`, along with the way to judge a
drawing — the silhouette sheet at 20 px, never at 64, where the four failures that mod records all
look perfectly fine.

## Preview image

`Mod/About/Preview.png` was generated with OpenAI's built-in image generation tool under human
direction and review, then cropped and downscaled to the required 896×504 Workshop format. The
2026-09-22 version uses a near-orthographic RimWorld-like colony scene to explain rearranging work
types, with the title and explanatory text added by the generator as part of the final raster.
