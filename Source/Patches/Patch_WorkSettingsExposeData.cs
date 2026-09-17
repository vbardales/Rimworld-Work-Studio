using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;
using RimWorld;
using Verse;

namespace WorkStudio
{
    /// <summary>
    /// Writes work priorities a second time into the save, <b>named</b> this time, and reads them
    /// back in that format.
    /// <para>
    /// Vanilla serializes <c>priorities</c> as a <see cref="DefMap{D,V}"/>, that is, a list of bare
    /// values aligned on the <c>DefDatabase</c> order. Nothing in the save says which value belongs
    /// to which work type: on load, <c>DefMap.ExposeData</c> merely pads or truncates the list.
    /// Adding, removing or reordering a work type between two sessions would therefore be enough to
    /// reshuffle every priority in the colony.
    /// </para>
    /// <para>
    /// The node added here is ignored by the game if the mod is removed, and costs only a few lines
    /// per pawn.
    /// </para>
    /// </summary>
    [HarmonyPatch(typeof(Pawn_WorkSettings), nameof(Pawn_WorkSettings.ExposeData))]
    public static class Patch_WorkSettingsExposeData
    {
        private const string Node = "workStudioPriorities";

        /// <summary>
        /// What was read during <c>LoadingVars</c>, waiting for the <c>PostLoadInit</c> where defs
        /// are resolved. Weak table: a pawn dropped mid-load holds on to nothing.
        /// </summary>
        private static readonly ConditionalWeakTable<Pawn_WorkSettings, Dictionary<string, int>> pending =
            new ConditionalWeakTable<Pawn_WorkSettings, Dictionary<string, int>>();

        public static void Postfix(Pawn_WorkSettings __instance)
        {
            switch (Scribe.mode)
            {
                case LoadSaveMode.Saving:
                    Save(__instance);
                    break;
                case LoadSaveMode.LoadingVars:
                    Load(__instance);
                    break;
                case LoadSaveMode.PostLoadInit:
                    Restore(__instance);
                    break;
            }
        }

        private static void Save(Pawn_WorkSettings settings)
        {
            var values = settings.priorities?.values;
            if (values == null)
            {
                return;
            }

            var types = DefDatabase<WorkTypeDef>.AllDefsListForReading;
            var named = new Dictionary<string, int>(values.Count);

            for (var i = 0; i < types.Count && i < values.Count; i++)
            {
                named[types[i].defName] = values[i];
            }

            Scribe_Collections.Look(ref named, Node, LookMode.Value, LookMode.Value);
        }

        private static void Load(Pawn_WorkSettings settings)
        {
            Dictionary<string, int> named = null;
            Scribe_Collections.Look(ref named, Node, LookMode.Value, LookMode.Value);

            if (named != null && named.Count > 0)
            {
                pending.Remove(settings);
                pending.Add(settings, named);
            }
        }

        private static void Restore(Pawn_WorkSettings settings)
        {
            if (!pending.TryGetValue(settings, out var named))
            {
                return;
            }

            pending.Remove(settings);

            var values = settings.priorities?.values;
            if (values == null)
            {
                return;
            }

            var types = DefDatabase<WorkTypeDef>.AllDefsListForReading;

            while (values.Count < types.Count)
            {
                values.Add(0);
            }
            while (values.Count > types.Count)
            {
                values.RemoveAt(values.Count - 1);
            }

            for (var i = 0; i < types.Count; i++)
            {
                // A type missing from the save is a type that appeared since: typically a mod
                // added between two sessions. It is set to zero - inactive - rather than keeping
                // the positional value vanilla loading just put there, which belongs to its
                // neighbour. It is also what the game does without us: a work type the save does
                // not know starts switched off.
                values[i] = named.TryGetValue(types[i].defName, out var priority) ? priority : 0;
            }

            // Vanilla just disabled what the pawn is not allowed to do; restoring overwrote that
            // work, so it has to be done again.
            settings.pawn?.Notify_DisabledWorkTypesChanged();
        }
    }
}
