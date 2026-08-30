# Journal des modifications

Format inspiré de [Keep a Changelog](https://keepachangelog.com/fr/1.1.0/).
Ce fichier sert au dépôt et à rédiger les notes de version Steam ; RimWorld ne l'affiche pas en jeu.

## [1.0.0] — non publié

À la publication : créer le tag `v1.0.0` et la release GitHub correspondante.

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
- Incompatible avec Work Tab, qui reconstruit lui aussi les colonnes de l'onglet Travail.
- Retirer le mod rend leurs types d'origine aux tâches déplacées.
