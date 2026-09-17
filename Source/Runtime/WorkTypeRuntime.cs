using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace WorkStudio
{
    /// <summary>
    /// Applies the desired configuration on top of the work defs, live.
    /// <para>
    /// Nothing is written to disk on the defs side: <see cref="WorkGiverDef.workType"/> is mutated,
    /// the custom <see cref="WorkTypeDef"/>s are created in memory, then the game is forced to
    /// reindex. <see cref="Apply"/> is a full, idempotent reconciliation: it can be called as many
    /// times as needed, and the result depends only on the configuration, never on history.
    /// </para>
    /// </summary>
    public static class WorkTypeRuntime
    {
        /// <summary>Priority given to a custom type whose order has not been chosen yet.</summary>
        public const int DefaultCustomPriority = 500;

        // ------------------------------------------------------- original state

        /// <summary>defName of a WorkGiverDef to the defName of its original work type.</summary>
        private static readonly Dictionary<string, string> originalGiverTypes = new Dictionary<string, string>();

        /// <summary>defName of a WorkGiverDef to its original priority within its type.</summary>
        private static readonly Dictionary<string, int> originalGiverOrders = new Dictionary<string, int>();

        private static readonly Dictionary<string, int> originalPriorities = new Dictionary<string, int>();
        private static readonly Dictionary<string, string> originalLabels = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> originalShortLabels = new Dictionary<string, string>();
        private static readonly Dictionary<string, bool> originalVisible = new Dictionary<string, bool>();

        private static bool captured;

        // ------------------------------------------------------- current state

        /// <summary>defNames of the types we added to the DefDatabase ourselves.</summary>
        private static readonly HashSet<string> ourTypes = new HashSet<string>();

        /// <summary>
        /// Derived type to the types its tasks were taken from. Read by the postfix on
        /// <c>Pawn.GetDisabledWorkTypes</c>: a colonist unable to do the source work must remain
        /// unable to do the derived work.
        /// </summary>
        public static readonly Dictionary<WorkTypeDef, List<WorkTypeDef>> InheritedDisabling =
            new Dictionary<WorkTypeDef, List<WorkTypeDef>>();

        /// <summary>Missing targets already reported once.</summary>
        private static readonly HashSet<string> warnedMissing = new HashSet<string>();

        private static WorkStudioSettings Settings => WorkStudioMod.Settings;

        public static bool IsCustom(WorkTypeDef type) => type != null && ourTypes.Contains(type.defName);

        public static CustomWorkTypeEntry EntryOf(string defName)
        {
            return Settings.customTypes.FirstOrDefault(e => e.id == defName);
        }

        /// <summary>Original work type of a task, before any reassignment.</summary>
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
        /// Takes a snapshot of the state delivered by the game and other mods. Must run before any
        /// mutation, hence on the very first <see cref="Apply"/>, at startup.
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

            // First of all: pawn priorities are indexed by position, and everything that follows
            // moves those positions.
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
            WorkTypeTagCompat.Notify();
        }

        /// <summary>Erases the whole configuration and puts the defs back in their original state.</summary>
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

        // ------------------------------------------------------- steps

        /// <summary>Creates, updates and removes the custom <see cref="WorkTypeDef"/>s.</summary>
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

                    // Add() renames on collision: realign on the defName it kept.
                    entry.id = def.defName;
                }

                ourTypes.Add(def.defName);

                def.label = entry.label.NullOrEmpty() ? def.defName : entry.label;
                def.description = entry.description;

                // labelShort is the only label the column header shows. Failing that, the full
                // name: a column too wide is visible, an empty header makes no sense.
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

        /// <summary>Files each task under the desired work type.</summary>
        private static void ApplyGiverAssignments()
        {
            foreach (var giver in DefDatabase<WorkGiverDef>.AllDefsListForReading)
            {
                WorkTypeDef desired = null;

                if (Settings.giverAssignments.TryGetValue(giver.defName, out var target) && !target.NullOrEmpty())
                {
                    desired = DefDatabase<WorkTypeDef>.GetNamedSilentFail(target);

                    // The target is gone (mod removed, type deleted): fall back to the original
                    // without erasing the assignment, in case the target comes back.
                    if (desired == null && warnedMissing.Add(target))
                    {
                        // In English, like everything sent to the log: it is other modders who
                        // read it in bug reports.
                        Log.Warning("[Work Studio] Work type not found: '" + target +
                                    "'. The tasks assigned to it fall back to their original type.");
                    }
                }

                if (desired == null)
                {
                    desired = OriginalTypeOf(giver);
                }

                giver.workType = desired;

                // Must be set before RebuildDefs: priorityInType is what
                // WorkTypeDef.ResolveReferences sorts workGiversByPriority on.
                giver.priorityInType = Settings.giverOrderOverrides.TryGetValue(giver.defName, out var order)
                    ? order
                    : OriginalOrderOf(giver);
            }
        }

        /// <summary>Original priority of a task within its type, before any reordering.</summary>
        public static int OriginalOrderOf(WorkGiverDef giver)
        {
            Capture();
            return originalGiverOrders.TryGetValue(giver.defName, out var order) ? order : 0;
        }

        /// <summary>Applies order, label and visibility to every type, custom or not.</summary>
        private static void ApplyTypeOverrides()
        {
            foreach (var type in DefDatabase<WorkTypeDef>.AllDefsListForReading)
            {
                type.naturalPriority = Settings.priorityOverrides.TryGetValue(type.defName, out var priority)
                    ? priority
                    : BasePriorityOf(type);

                if (!IsCustom(type))
                {
                    // A custom type's label lives in its sheet, not in the overrides: a single
                    // source of truth per type.
                    var renamed = Settings.labelOverrides.TryGetValue(type.defName, out var label)
                                  && !label.NullOrEmpty();

                    type.label = renamed ? label : BaseLabelOf(type);

                    // Renaming without touching labelShort would rename nothing visible: the column
                    // header only shows the short label, and would therefore keep the old name.
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
        /// Makes each derived type inherit the incapacities of its source types, and returns, for
        /// each one, the source whose priority it should take over for pawns that do not know it
        /// yet.
        /// <para>
        /// Two mechanisms complement each other. <see cref="WorkTags"/> cover everything the game
        /// filters by tag - backstories and traits - without any patch, since
        /// <c>BackstoryDef.AllowsWorkType</c> compares the type's tags with those the backstory
        /// forbids. The rest - genes, ideoligion roles, quests, health, life stage - names
        /// <see cref="WorkTypeDef"/>s explicitly: that is where the postfix on
        /// <c>Pawn.GetDisabledWorkTypes</c> takes over, through <see cref="InheritedDisabling"/>.
        /// </para>
        /// <para>
        /// A type derived from several sources inherits the incapacities of <b>each</b> of them. A
        /// mixed type is therefore more restrictive than its parts: it is the only choice that never
        /// lets a colonist do work the game had ruled out for them.
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

                // How many tasks does this derived type hold from each source?
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

                // The majority source sets the tone: it is the one whose priority, already tuned by
                // the player, the derived type takes over. On a tie, defName decides so that two
                // successive applies give the same result.
                seeds[defName] = origins
                    .OrderByDescending(o => weights[o])
                    .ThenBy(o => o.defName)
                    .First()
                    .defName;
            }

            return seeds;
        }

        /// <summary>
        /// Reindexes the defs and rebuilds the per-type task lists.
        /// <c>WorkTypeDef.ResolveReferences</c> repopulates <c>workGiversByPriority</c> by sweeping the
        /// <see cref="WorkGiverDef"/>s, so those lists must be emptied first or they get duplicates.
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
        /// Rebuilds the Work tab columns exactly the way <see cref="PawnColumnDefGenerator"/> makes
        /// them at startup: each column inserted right after the copy/paste button, walking the
        /// types from lowest to highest priority so that the result comes out in reverse order.
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

                // PawnColumnWorker_WorkPriority measures the short label only once and keeps the
                // result. Renaming a work type would therefore leave a column sized for the old
                // name: the worker is thrown away, and the next draw makes a fresh one.
                column.workerInt = null;

                workTable.columns.Insert(AnchorIndex(workTable), column);
            }

            DefDatabase<PawnColumnDef>.ClearCachedData();
            DefDatabase<PawnColumnDef>.ResolveAllReferences(false, true);
        }

        /// <summary>Where to insert a work column: right after the copy/paste one.</summary>
        private static int AnchorIndex(PawnTableDef workTable)
        {
            var index = workTable.columns.FindIndex(c => c.Worker is PawnColumnWorker_CopyPasteWorkPriorities);
            if (index >= 0)
            {
                return index + 1;
            }

            // No copy/paste column (a mod removed it): fall back to the remaining space column,
            // which must stay at the end of the table.
            index = workTable.columns.FindIndex(c => c.Worker is PawnColumnWorker_RemainingSpace);
            return index >= 0 ? index : workTable.columns.Count;
        }

        /// <summary>
        /// Backstories cache the list of types they forbid, and that cache is not cleared by
        /// <c>ClearCachedData</c>. Without this sweep, a type created afterwards never shows up in
        /// it, whatever its <see cref="WorkTags"/>.
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
            // Notify_PawnsChanged marks the table dirty; PawnTable.Columns rereads def.columns on
            // every draw, so the new columns show up from the next frame.
            Find.WindowStack?.WindowOfType<MainTabWindow_Work>()?.Notify_PawnsChanged();
        }
    }
}
