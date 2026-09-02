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
