using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace WorkStudio
{
    /// <summary>
    /// Applique la configuration voulue par-dessus les defs de travail, a chaud.
    /// <para>
    /// Rien n'est ecrit sur disque cote defs : on mute <see cref="WorkGiverDef.workType"/>, on cree
    /// les <see cref="WorkTypeDef"/> personnalises en memoire, puis on force le jeu a se reindexer.
    /// <see cref="Apply"/> est une reconciliation complete et idempotente : on peut l'appeler autant
    /// de fois qu'on veut, le resultat ne depend que de la configuration, jamais de l'historique.
    /// </para>
    /// </summary>
    public static class WorkTypeRuntime
    {
        /// <summary>Priorite donnee a un type personnalise dont l'ordre n'a pas encore ete choisi.</summary>
        public const int DefaultCustomPriority = 500;

        // ------------------------------------------------------- etat d'origine

        /// <summary>defName d'un WorkGiverDef vers le defName de son type de travail d'origine.</summary>
        private static readonly Dictionary<string, string> originalGiverTypes = new Dictionary<string, string>();

        /// <summary>defName d'un WorkGiverDef vers sa priorite d'origine au sein de son type.</summary>
        private static readonly Dictionary<string, int> originalGiverOrders = new Dictionary<string, int>();

        private static readonly Dictionary<string, int> originalPriorities = new Dictionary<string, int>();
        private static readonly Dictionary<string, string> originalLabels = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> originalShortLabels = new Dictionary<string, string>();
        private static readonly Dictionary<string, bool> originalVisible = new Dictionary<string, bool>();

        private static bool captured;

        // ------------------------------------------------------- etat courant

        /// <summary>defName des types que nous avons nous-memes ajoutes a la DefDatabase.</summary>
        private static readonly HashSet<string> ourTypes = new HashSet<string>();

        /// <summary>
        /// Type derive vers les types dont ses taches ont ete extraites. Lu par le postfix sur
        /// <c>Pawn.GetDisabledWorkTypes</c> : un colon incapable de faire le travail source doit
        /// rester incapable de faire le travail derive.
        /// </summary>
        public static readonly Dictionary<WorkTypeDef, List<WorkTypeDef>> InheritedDisabling =
            new Dictionary<WorkTypeDef, List<WorkTypeDef>>();

        /// <summary>Cibles introuvables deja signalees une fois.</summary>
        private static readonly HashSet<string> warnedMissing = new HashSet<string>();

        private static WorkStudioSettings Settings => WorkStudioMod.Settings;

        public static bool IsCustom(WorkTypeDef type) => type != null && ourTypes.Contains(type.defName);

        public static CustomWorkTypeEntry EntryOf(string defName)
        {
            return Settings.customTypes.FirstOrDefault(e => e.id == defName);
        }

        /// <summary>Type de travail d'origine d'une tache, avant toute reaffectation.</summary>
        public static WorkTypeDef OriginalTypeOf(WorkGiverDef giver)
        {
            Capture();
            return originalGiverTypes.TryGetValue(giver.defName, out var name) && !name.NullOrEmpty()
                ? DefDatabase<WorkTypeDef>.GetNamedSilentFail(name)
                : null;
        }

        public static bool HasOverrides()
        {
            return Settings.customTypes.Count > 0
                   || Settings.giverAssignments.Count > 0
                   || Settings.priorityOverrides.Count > 0
                   || Settings.giverOrderOverrides.Count > 0
                   || Settings.labelOverrides.Count > 0
                   || Settings.hiddenTypes.Count > 0;
        }

        // ------------------------------------------------------- capture

        /// <summary>
        /// Photographie l'etat livre par le jeu et les autres mods. Doit tourner avant la moindre
        /// mutation, donc au tout premier <see cref="Apply"/>, au demarrage.
        /// </summary>
        private static void Capture()
        {
            if (captured)
            {
                return;
            }

            foreach (var giver in DefDatabase<WorkGiverDef>.AllDefsListForReading)
            {
                originalGiverTypes[giver.defName] = giver.workType?.defName;
                originalGiverOrders[giver.defName] = giver.priorityInType;
            }

            foreach (var type in DefDatabase<WorkTypeDef>.AllDefsListForReading)
            {
                originalPriorities[type.defName] = type.naturalPriority;
                originalLabels[type.defName] = type.label;
                originalShortLabels[type.defName] = type.labelShort;
                originalVisible[type.defName] = type.visible;
            }

            captured = true;
        }

        public static int OriginalPriorityOf(WorkTypeDef type)
        {
            Capture();
            return originalPriorities.TryGetValue(type.defName, out var priority)
                ? priority
                : DefaultCustomPriority;
        }

        // ------------------------------------------------------- application

        public static void Apply()
        {
            Capture();

            // Avant toute chose : les priorites des pions sont indexees par position, et tout ce qui
            // suit deplace ces positions.
            var snapshot = PriorityMemory.Capture();

            SyncCustomTypes();
            ApplyGiverAssignments();
            ApplyTypeOverrides();
            var seeds = ComputeInheritance();

            RebuildDefs();
            RebuildWorkColumns();
            ClearBackstoryCaches();

            PriorityMemory.Restore(snapshot, seeds);
            RefreshWorkTab();
        }

        /// <summary>Efface toute la configuration et remet les defs dans leur etat d'origine.</summary>
        public static void ResetAll()
        {
            Settings.customTypes.Clear();
            Settings.giverAssignments.Clear();
            Settings.priorityOverrides.Clear();
            Settings.giverOrderOverrides.Clear();
            Settings.labelOverrides.Clear();
            Settings.hiddenTypes.Clear();
            WorkStudioMod.Instance.WriteSettings();
            Apply();
        }

        // ------------------------------------------------------- etapes

        /// <summary>Cree, met a jour et supprime les <see cref="WorkTypeDef"/> personnalises.</summary>
        private static void SyncCustomTypes()
        {
            var wanted = Settings.customTypes
                .Where(e => e != null && !e.id.NullOrEmpty())
                .ToDictionary(e => e.id, e => e);

            foreach (var defName in ourTypes.ToList())
            {
                if (wanted.ContainsKey(defName))
                {
                    continue;
                }

                var stale = DefDatabase<WorkTypeDef>.GetNamedSilentFail(defName);
                if (stale != null)
                {
                    DefDatabase<WorkTypeDef>.Remove(stale);
                }

                var staleColumn = DefDatabase<PawnColumnDef>.GetNamedSilentFail("WorkPriority_" + defName);
                if (staleColumn != null)
                {
                    DefDatabase<PawnColumnDef>.Remove(staleColumn);
                }

                ourTypes.Remove(defName);
            }

            foreach (var entry in wanted.Values)
            {
                var def = DefDatabase<WorkTypeDef>.GetNamedSilentFail(entry.id);
                if (def == null)
                {
                    def = new WorkTypeDef { defName = entry.id };
                    def.modContentPack = WorkStudioMod.Instance?.Content;
                    DefDatabase<WorkTypeDef>.Add(def);

                    // Add() renomme en cas de collision : on se realigne sur le defName retenu.
                    entry.id = def.defName;
                }

                ourTypes.Add(def.defName);

                def.label = entry.label.NullOrEmpty() ? def.defName : entry.label;
                def.description = entry.description;

                // labelShort est le seul libelle que l'en-tete de colonne affiche. A defaut, le nom
                // complet : une colonne trop large se voit, un en-tete vide ne se comprend pas.
                def.labelShort = entry.labelShort.NullOrEmpty() ? def.label : entry.labelShort;
                def.pawnLabel = entry.pawnLabel.NullOrEmpty() ? def.label : entry.pawnLabel;
                def.gerundLabel = entry.gerundLabel.NullOrEmpty() ? def.label : entry.gerundLabel;
                def.verb = entry.verb.NullOrEmpty() ? def.label : entry.verb;
                def.alwaysStartActive = entry.alwaysStartActive;
                def.requireCapableColonist = entry.requireCapableColonist;
                def.relevantSkills = entry.relevantSkills
                    .Select(s => DefDatabase<SkillDef>.GetNamedSilentFail(s))
                    .Where(s => s != null)
                    .ToList();
            }
        }

        /// <summary>Range chaque tache dans le type de travail voulu.</summary>
        private static void ApplyGiverAssignments()
        {
            foreach (var giver in DefDatabase<WorkGiverDef>.AllDefsListForReading)
            {
                WorkTypeDef desired = null;

                if (Settings.giverAssignments.TryGetValue(giver.defName, out var target) && !target.NullOrEmpty())
                {
                    desired = DefDatabase<WorkTypeDef>.GetNamedSilentFail(target);

                    // La cible a disparu (mod retire, type supprime) : on revient a l'origine sans
                    // effacer l'affectation, au cas ou la cible reviendrait.
                    if (desired == null && warnedMissing.Add(target))
                    {
                        Log.Warning("[Work Studio] Type de travail introuvable : '" + target +
                                    "'. Les taches concernees reprennent leur type d'origine.");
                    }
                }

                if (desired == null)
                {
                    desired = OriginalTypeOf(giver);
                }

                giver.workType = desired;

                // Doit etre pose avant RebuildDefs : c'est sur priorityInType que
                // WorkTypeDef.ResolveReferences trie workGiversByPriority.
                giver.priorityInType = Settings.giverOrderOverrides.TryGetValue(giver.defName, out var order)
                    ? order
                    : OriginalOrderOf(giver);
            }
        }

        /// <summary>Priorite d'origine d'une tache au sein de son type, avant toute reorganisation.</summary>
        public static int OriginalOrderOf(WorkGiverDef giver)
        {
            Capture();
            return originalGiverOrders.TryGetValue(giver.defName, out var order) ? order : 0;
        }

        /// <summary>Applique ordre, libelle et visibilite a tous les types, personnalises ou non.</summary>
        private static void ApplyTypeOverrides()
        {
            foreach (var type in DefDatabase<WorkTypeDef>.AllDefsListForReading)
            {
                type.naturalPriority = Settings.priorityOverrides.TryGetValue(type.defName, out var priority)
                    ? priority
                    : BasePriorityOf(type);

                if (!IsCustom(type))
                {
                    // Le libelle d'un type personnalise vit dans sa fiche, pas dans les surcharges :
                    // une seule source de verite par type.
                    var renamed = Settings.labelOverrides.TryGetValue(type.defName, out var label)
                                  && !label.NullOrEmpty();

                    type.label = renamed ? label : BaseLabelOf(type);

                    // Renommer sans toucher a labelShort ne renommerait rien de visible : l'en-tete
                    // de colonne n'affiche que le libelle court, et garderait donc l'ancien nom.
                    type.labelShort = renamed ? label : BaseShortLabelOf(type);
                }

                type.visible = BaseVisibleOf(type) && !Settings.hiddenTypes.Contains(type.defName);
            }
        }

        private static int BasePriorityOf(WorkTypeDef type)
        {
            return originalPriorities.TryGetValue(type.defName, out var priority)
                ? priority
                : DefaultCustomPriority;
        }

        private static string BaseLabelOf(WorkTypeDef type)
        {
            return originalLabels.TryGetValue(type.defName, out var label) ? label : type.defName;
        }

        private static string BaseShortLabelOf(WorkTypeDef type)
        {
            return originalShortLabels.TryGetValue(type.defName, out var label) ? label : type.labelShort;
        }

        private static bool BaseVisibleOf(WorkTypeDef type)
        {
            return !originalVisible.TryGetValue(type.defName, out var visible) || visible;
        }

        /// <summary>
        /// Fait heriter chaque type derive des incapacites de ses types sources, et renvoie, pour
        /// chacun, la source dont il doit reprendre la priorite chez les pions qui ne le connaissent
        /// pas encore.
        /// <para>
        /// Deux mecanismes se completent. Les <see cref="WorkTags"/> couvrent tout ce que le jeu
        /// filtre par tag - histoires et traits - sans le moindre patch, puisque
        /// <c>BackstoryDef.AllowsWorkType</c> compare les tags du type a ceux que l'histoire
        /// interdit. Le reste - genes, roles ideologiques, quetes, sante, stade de vie - designe des
        /// <see cref="WorkTypeDef"/> nommement : c'est la que le postfix sur
        /// <c>Pawn.GetDisabledWorkTypes</c> prend le relais, via <see cref="InheritedDisabling"/>.
        /// </para>
        /// <para>
        /// Un type derive de plusieurs sources herite des incapacites de <b>chacune</b> d'elles. Un
        /// type mixte est donc plus restrictif que ses parties : c'est le seul choix qui ne laisse
        /// jamais un colon faire un travail dont le jeu l'avait ecarte.
        /// </para>
        /// </summary>
        private static Dictionary<string, string> ComputeInheritance()
        {
            InheritedDisabling.Clear();
            var seeds = new Dictionary<string, string>();

            foreach (var defName in ourTypes)
            {
                var type = DefDatabase<WorkTypeDef>.GetNamedSilentFail(defName);
                if (type == null)
                {
                    continue;
                }

                // Combien de taches ce type derive tient-il de chaque source ?
                var weights = new Dictionary<WorkTypeDef, int>();

                foreach (var pair in Settings.giverAssignments)
                {
                    if (pair.Value != defName)
                    {
                        continue;
                    }

                    if (!originalGiverTypes.TryGetValue(pair.Key, out var originName) || originName.NullOrEmpty())
                    {
                        continue;
                    }

                    var origin = DefDatabase<WorkTypeDef>.GetNamedSilentFail(originName);
                    if (origin == null || origin == type)
                    {
                        continue;
                    }

                    weights.TryGetValue(origin, out var count);
                    weights[origin] = count + 1;
                }

                if (weights.Count == 0)
                {
                    type.workTags = WorkTags.None;
                    continue;
                }

                var origins = weights.Keys.ToList();

                var tags = WorkTags.None;
                foreach (var origin in origins)
                {
                    tags |= origin.workTags;
                }
                type.workTags = tags;

                InheritedDisabling[type] = origins;

                // La source majoritaire donne le ton : c'est d'elle que le type derive reprend la
                // priorite deja reglee par le joueur. A egalite, on tranche par defName pour que
                // deux applications successives donnent le meme resultat.
                seeds[defName] = origins
                    .OrderByDescending(o => weights[o])
                    .ThenBy(o => o.defName)
                    .First()
                    .defName;
            }

            return seeds;
        }

        /// <summary>
        /// Reindexe les defs et reconstruit les listes de taches par type.
        /// <c>WorkTypeDef.ResolveReferences</c> repeuple <c>workGiversByPriority</c> en balayant les
        /// <see cref="WorkGiverDef"/>, donc il faut vider ces listes d'abord sous peine de doublons.
        /// </summary>
        private static void RebuildDefs()
        {
            foreach (var type in DefDatabase<WorkTypeDef>.AllDefsListForReading)
            {
                type.workGiversByPriority = new List<WorkGiverDef>();
            }

            DefDatabase<WorkTypeDef>.ClearCachedData();
            DefDatabase<WorkTypeDef>.ResolveAllReferences(false, true);

            DefDatabase<WorkGiverDef>.ClearCachedData();
            DefDatabase<WorkGiverDef>.ResolveAllReferences(false, true);
        }

        /// <summary>
        /// Refait les colonnes de l'onglet Travail exactement comme
        /// <see cref="PawnColumnDefGenerator"/> les fabrique au demarrage : chaque colonne inseree
        /// juste apres le bouton copier/coller, en parcourant les types du moins au plus prioritaire
        /// pour que le resultat sorte dans l'ordre inverse.
        /// </summary>
        private static void RebuildWorkColumns()
        {
            var workTable = PawnTableDefOf.Work;
            workTable.columns.RemoveAll(c => c.workType != null);

            var moveLabelDown = false;

            foreach (var type in WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder
                         .Where(t => t.visible)
                         .Reverse())
            {
                moveLabelDown = !moveLabelDown;

                var columnName = "WorkPriority_" + type.defName;
                var column = DefDatabase<PawnColumnDef>.GetNamedSilentFail(columnName);

                if (column == null)
                {
                    column = new PawnColumnDef
                    {
                        defName = columnName,
                        workerClass = typeof(PawnColumnWorker_WorkPriority),
                        sortable = true,
                        generated = true,
                        modContentPack = type.modContentPack,
                    };
                    column.PostLoad();
                    DefDatabase<PawnColumnDef>.Add(column);
                }

                column.workType = type;
                column.moveWorkTypeLabelDown = moveLabelDown;

                // PawnColumnWorker_WorkPriority mesure le libelle court une seule fois et garde le
                // resultat. Renommer un travail laisserait donc une colonne dimensionnee pour
                // l'ancien nom : on jette le worker, le prochain affichage en fabrique un neuf.
                column.workerInt = null;

                workTable.columns.Insert(AnchorIndex(workTable), column);
            }

            DefDatabase<PawnColumnDef>.ClearCachedData();
            DefDatabase<PawnColumnDef>.ResolveAllReferences(false, true);
        }

        /// <summary>Position ou inserer une colonne de travail : juste apres le copier/coller.</summary>
        private static int AnchorIndex(PawnTableDef workTable)
        {
            var index = workTable.columns.FindIndex(c => c.Worker is PawnColumnWorker_CopyPasteWorkPriorities);
            if (index >= 0)
            {
                return index + 1;
            }

            // Pas de colonne copier/coller (mod qui l'a retiree) : on se rabat sur l'espace restant,
            // qui doit imperativement rester en queue de table.
            index = workTable.columns.FindIndex(c => c.Worker is PawnColumnWorker_RemainingSpace);
            return index >= 0 ? index : workTable.columns.Count;
        }

        /// <summary>
        /// Les histoires gardent en cache la liste des types qu'elles interdisent, et ce cache
        /// n'est pas vide par <c>ClearCachedData</c>. Sans ce coup de balai, un type cree apres coup
        /// n'y figure jamais, quels que soient ses <see cref="WorkTags"/>.
        /// </summary>
        private static void ClearBackstoryCaches()
        {
            foreach (var backstory in DefDatabase<BackstoryDef>.AllDefsListForReading)
            {
                backstory.cachedDisabledWorkTypes = null;
            }
        }

        private static void RefreshWorkTab()
        {
            // Notify_PawnsChanged marque la table sale ; PawnTable.Columns relit def.columns a
            // chaque affichage, donc les nouvelles colonnes apparaissent des la frame suivante.
            Find.WindowStack?.WindowOfType<MainTabWindow_Work>()?.Notify_PawnsChanged();
        }
    }
}
