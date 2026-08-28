# Attribution

## Achtung! - Andreas Pardeike (pardeike / Brrainz)

Depot : https://github.com/pardeike/RimWorld-Achtung-Mod
Licence : MIT (voir `LICENSE-achtung.txt`)

La sequence de rechargement a chaud des `WorkTypeDef` vient de `Source/DynamicWorkTypes.cs`
d'Achtung! : purger `workGiversByPriority`, appeler `DefDatabase<T>.ClearCachedData()` puis
`ResolveAllReferences(false, true)` pour reindexer, regenerer les colonnes de la table Travail,
et rafraichir chaque pion. Le meme fichier circule aussi sous forme de gist :
https://gist.github.com/pardeike/6ae015b86e5f909be93bdabd8316b078 (sans licence explicite,
c'est donc la version du depot Achtung!, sous MIT, qui a servi de reference).

Ce qui a ete repris tel quel : l'ordre des appels de rechargement, l'idee du postfix sur
`Pawn.GetDisabledWorkTypes` pour qu'un type derive herite des incapacites de son type source,
et la reconstruction des colonnes de `PawnTableDefOf.Work`.

Ce qui differe volontairement :

- **Les priorites des pions sont rememorees par `defName`**, pas par index. `DefMap<D,V>` stocke
  ses valeurs dans une simple `List<V>` alignee sur l'ordre des defs (`DefMap.ExposeData` ecrit
  `vals` positionnellement), donc toute insertion ou suppression decale les priorites de tout le
  monde. Achtung! gere ce decalage par un `Insert`/`Remove` a l'index calcule, ce qui ne tient que
  pour une modification unitaire a la fois ; Work Studio applique une configuration entiere d'un
  coup et doit donc capturer puis restaurer par nom.
- **`values.Remove(index)` d'Achtung! n'est pas repris** : sur une `List<int>`, cet appel supprime
  la premiere valeur *egale* a l'index, pas l'element *situe* a cet index.
- **Tous les pions sont traites** (`PawnsFinder.All_AliveOrDead`), pas seulement les pions
  spawnes : une caravane ou un pion en nacelle a lui aussi des priorites a recaler.
- **Les incapacites derivees passent d'abord par `workTags`**, l'union des tags des types sources,
  ce qui fait fonctionner sans rien patcher le filtrage par histoire et par trait.
