using System.Collections.Generic;
using RimWorld;
using Verse;

namespace WorkStudio
{
    /// <summary>
    /// Remembers every pawn's work priorities <b>by defName</b> for the duration of a
    /// reconfiguration, then puts them back.
    /// <para>
    /// This is the piece the whole structure cannot do without. <see cref="DefMap{D,V}"/> stores no
    /// keys: it is a plain <c>List</c> of values aligned on the order of the
    /// <see cref="DefDatabase{T}"/>, indexed by <c>def.index</c>. Adding, removing or reordering a
    /// single <see cref="WorkTypeDef"/> therefore silently shifts the priorities of every following
    /// type, for every pawn in the game.
    /// </para>
    /// </summary>
    public static class PriorityMemory
    {
        public sealed class Snapshot
        {
            public readonly Dictionary<Pawn, Dictionary<string, int>> byPawn =
                new Dictionary<Pawn, Dictionary<string, int>>();
        }

        /// <summary>
        /// Every pawn carrying work settings, not only those standing on a map: a caravan, a
        /// transport pod in flight or a pawn in cryptosleep also have a <c>DefMap</c> to
        /// realign.
        /// </summary>
        private static List<Pawn> AllPawns()
        {
            if (Current.Game == null)
            {
                return new List<Pawn>();
            }

            return PawnsFinder.All_AliveOrDead;
        }

        public static Snapshot Capture()
        {
            var snapshot = new Snapshot();
            var types = DefDatabase<WorkTypeDef>.AllDefsListForReading;

            foreach (var pawn in AllPawns())
            {
                var values = pawn?.workSettings?.priorities?.values;
                if (values == null)
                {
                    continue;
                }

                var map = new Dictionary<string, int>(values.Count);
                for (var i = 0; i < types.Count && i < values.Count; i++)
                {
                    map[types[i].defName] = values[i];
                }

                snapshot.byPawn[pawn] = map;
            }

            return snapshot;
        }

        /// <summary>
        /// Puts each priority back against its type, resizes the <c>DefMap</c>s to the new number of
        /// defs, and refreshes every pawn's caches.
        /// </summary>
        /// <param name="seeds">
        /// defName of a derived type to the defName of the type it was split from. A type the pawn
        /// did not know inherits its source's priority: splitting a job in two must not change
        /// anything about the colonist's behaviour.
        /// </param>
        public static void Restore(Snapshot snapshot, Dictionary<string, string> seeds)
        {
            var types = DefDatabase<WorkTypeDef>.AllDefsListForReading;
            var count = types.Count;

            foreach (var pawn in AllPawns())
            {
                var settings = pawn?.workSettings;
                var values = settings?.priorities?.values;
                if (values == null)
                {
                    continue;
                }

                while (values.Count < count)
                {
                    values.Add(0);
                }
                while (values.Count > count)
                {
                    values.RemoveAt(values.Count - 1);
                }

                snapshot.byPawn.TryGetValue(pawn, out var saved);

                for (var i = 0; i < count; i++)
                {
                    values[i] = PriorityFor(saved, seeds, types[i]);
                }

                settings.workGiversDirty = true;

                // Clears the incapacity caches, zeroes what the pawn is not allowed to do, and
                // notifies the Skills tab.
                pawn.Notify_DisabledWorkTypesChanged();

                settings.CacheWorkGiversInOrder();
            }
        }

        private static int PriorityFor(Dictionary<string, int> saved, Dictionary<string, string> seeds,
            WorkTypeDef type)
        {
            if (saved == null)
            {
                return Pawn_WorkSettings.DefaultPriority;
            }

            if (saved.TryGetValue(type.defName, out var priority))
            {
                return priority;
            }

            if (seeds.TryGetValue(type.defName, out var seed) && saved.TryGetValue(seed, out var seedPriority))
            {
                return seedPriority;
            }

            return Pawn_WorkSettings.DefaultPriority;
        }
    }
}
