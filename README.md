# Work Studio

Editeur des types de travail de RimWorld 1.6, applicable **a chaud** : creer ses propres types,
y deplacer les taches prises dans d'autres types, renommer, masquer une colonne.
Aucun redemarrage, aucun fichier de def a ecrire.

Le glisser-deposer joue sur les deux niveaux d'ordre du jeu, qui n'ont rien a voir entre eux :

- **la colonne de gauche** reordonne les types de travail, en reaffectant leurs
  `naturalPriority` - c'est l'ordre des colonnes de l'onglet Travail, et l'ordre dans lequel un
  colon passe d'un travail au suivant ;
- **la liste des taches d'un type** reordonne les `WorkGiverDef.priorityInType` - c'est l'ordre
  dans lequel il attrape les taches une fois qu'il s'est mis a ce travail.

Dans les deux cas on redistribue les valeurs deja en place au lieu d'inventer une echelle, pour
qu'un mod charge plus tard puisse encore se glisser au milieu de la liste.

Chaque ligne porte aussi deux fleches haut/bas, qui deplacent d'un cran. Le glisser-deposer est
peu praticable au pave tactile d'une Steam Deck, et les fleches restent visibles - grisees - aux
extremites de liste : un bouton qui disparait deplacerait tous les autres sous le doigt.

Le mod s'ouvre depuis le bouton **Types de travail...** en haut a droite de l'onglet Travail,
ou depuis ses reglages.

## Comment ca marche

Les defs ne sont pas rejoues : ils sont mutes en memoire puis le jeu est force a se reindexer.
`WorkTypeRuntime.Apply()` est une reconciliation complete et idempotente - le resultat ne depend
que de la configuration, jamais de l'ordre des modifications.

Le point delicat n'est pas de creer un `WorkTypeDef`, c'est de ne pas melanger les priorites des
colons. `DefMap<D,V>` - la structure qui porte `Pawn_WorkSettings.priorities` - ne stocke aucune
cle : c'est une `List<V>` indexee par `def.index`, serialisee telle quelle. Ajouter, retirer ou
reordonner un seul type de travail decalerait donc les priorites de tous les types suivants,
chez tous les pions de la partie, et personne ne s'en apercevrait avant de retrouver un medecin
en train de miner. D'ou deux garde-fous :

- `PriorityMemory` capture les priorites **par defName** avant chaque reconfiguration et les
  remet en face du bon type apres. Un type nouveau herite de la priorite du type dont ses taches
  ont ete extraites : scinder un travail en deux ne change rien au comportement du colon.
- `Patch_WorkSettingsExposeData` ecrit les memes priorites, nommees, dans la sauvegarde. On peut
  donc modifier la configuration entre deux sessions sans rien melanger.

## Quand la liste de mods change

Une affectation pointe sur un `defName`, jamais sur un objet : un mod retire fait donc disparaitre
la tache, pas le reglage. L'affectation reste en reserve et reprend seule si le mod revient. Une
cible de type de travail introuvable retombe sur le type d'origine, avec un avertissement au
journal.

Le cas vraiment dangereux est ailleurs, et il ne vient pas de ce mod : ajouter ou retirer
n'importe quel mod qui declare un `WorkTypeDef` decale la `DefMap` des priorites de toute la
colonie. `Patch_WorkSettingsExposeData` ecrit ces priorites nommees dans la sauvegarde et les
remet en face du bon travail au chargement, ce qui protege donc aussi les travaux vanilla et ceux
des autres mods. Un type apparu depuis la sauvegarde demarre eteint, comme le fait le jeu seul.

`ConfigDrift` compare le paysage de defs a celui du dernier demarrage et l'annonce une fois -
types disparus, taches introuvables, types nouveaux. L'avertissement ne reparait pas tant que
rien ne bouge, et ne s'affiche pas du tout sans configuration.

## Importer et exporter

Depuis les reglages du mod. Les fichiers vivent dans `WorkStudio/` a cote de `Saves` et de
`Config`, dans les donnees de sauvegarde du jeu.

Un export ne contient que la part transportable des reglages : types personnalises, affectations
des taches, ordres, renommages, masquages. Il laisse de cote `knownWorkTypes`, la photographie de
la liste de mods qui sert a reperer les changements - la partager ferait crier au changement des
le premier import chez quelqu'un d'autre.

Un import lit d'abord le fichier dans un jeu de reglages neuf et ne l'adopte que s'il a ete lu en
entier. Un fichier tronque ne laisse donc pas la configuration a moitie remplacee, ce qui serait
pire que de ne rien importer.

## Ce que le mod ne fait pas

- **Il ne cree pas de nouvelles taches.** Un type de travail personnalise est un contenant : il
  faut lui donner des `WorkGiverDef` existants, pris a d'autres types.
- **Un type mixte est plus restrictif que ses parties.** Un type nourri par Docteur et Manieur
  est desactive chez un colon incapable de l'un *ou* de l'autre. C'est le seul choix qui ne
  laisse jamais un colon faire un travail dont le jeu l'avait ecarte.
- **Work Tab n'est pas gere.** Ce mod reconstruit lui aussi les colonnes de l'onglet Travail ;
  les deux se marcheraient dessus. Achtung! contourne le probleme par reflexion sur
  `WorkTab.Controller`, ce qui n'a pas ete repris ici faute de pouvoir le tester.
- **Les types masques restent actifs.** Masquer retire la colonne, pas le travail. Pour arreter
  un travail, il faut mettre sa priorite a zero comme d'habitude.

## Desinstallation

Retirer le mod rend leurs types d'origine aux taches deplacees. Les types personnalises
disparaissent avec lui : les priorites que les colons y avaient sont perdues, celles de tous les
autres travaux sont conservees.

## Build

    dotnet build WorkStudio/Source/WorkStudio.csproj -c Release

La DLL sort directement dans `WorkStudio/Assemblies/`, et une jonction NTFS relie
`RimWorld\Mods\WorkStudio` a ce dossier : il n'y a rien a copier.

Voir `ATTRIBUTION.md` pour ce qui vient d'Achtung! (MIT) et pour ce qui en differe.
