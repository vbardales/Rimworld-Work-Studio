# Journal des modifications

Format inspiré de [Keep a Changelog](https://keepachangelog.com/fr/1.1.0/).
Ce fichier sert au dépôt et à rédiger les notes de version Steam ; RimWorld ne l'affiche pas en jeu.

## [1.0.1] — non publié

### Ajouté

- Compatibilité avec [baku] Work Type Tag, qui affiche le nom du travail devant l'action d'un colon et colore l'en-tête de colonne assorti. Il gérait déjà les types créés ici — il indexe par `defName` et dérive une couleur du nom — mais gardait ses libellés dans un cache que rien ne vidait : renommer un travail laissait l'ancien nom devant l'action des colons jusqu'au redémarrage. Liaison molle par réflexion, aucune dépendance ajoutée.

### Modifié

- La colonne de droite de l'éditeur affiche désormais, en gris, le type auquel chaque tâche appartient déjà, et s'intitule « Tâches des autres types ». Les deux colonnes listaient des tâches sans que rien ne les distingue : l'appartenance n'apparaissait qu'en infobulle.
- Une ligne d'aide sous la recherche indique ce que produit un clic.

## [1.0.0] — 2026-08-30

Première version. RimWorld 1.6.

### Ajouté

- Création de types de travail depuis le jeu, sans redémarrer.
- Déplacement des tâches d'un type à l'autre, y compris depuis les types du jeu de base.
- Renommage, réordonnancement et masquage des colonnes de l'onglet Travail.
- Glisser-déposer sur deux niveaux : l'ordre des types entre eux, l'ordre des tâches dans un type.
- Flèches haut/bas doublant le glisser-déposer, pour l'usage sans souris.
- Import et export des configurations.
- Priorités des colons enregistrées par nom dans la sauvegarde, au lieu de la liste indexée par position du jeu de base : changer de configuration ou de liste de mods ne les décale plus. Protège aussi les travaux du jeu de base et ceux des autres mods.

### Notes

- Ce mod ne crée pas de nouvelles tâches, il redistribue celles qui existent.
- Un type nourri par plusieurs sources hérite des incapacités de chacune.
- Incompatible avec Work Tab (`Fluffy.WorkTab`) et Compact Work Tab (`Mlie.CompactWorkTab`), qui reconstruisent eux aussi les colonnes de l'onglet Travail. L'incompatibilité est déclarée dans `About.xml`, donc le jeu prévient de lui-même.
- Retirer le mod rend leurs types d'origine aux tâches déplacées.
