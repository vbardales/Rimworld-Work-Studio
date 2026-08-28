using System.Collections.Generic;
using RimWorld;
using Verse;

namespace WorkStudio
{
    /// <summary>
    /// Retient les priorites de travail de chaque pion <b>par defName</b> le temps d'une
    /// reconfiguration, puis les remet en place.
    /// <para>
    /// C'est la piece indispensable de tout l'edifice. <see cref="DefMap{D,V}"/> ne stocke pas de
    /// cles : c'est une simple <c>List</c> de valeurs alignee sur l'ordre de la
    /// <see cref="DefDatabase{T}"/>, indexee par <c>def.index</c>. Ajouter, retirer ou reordonner un
    /// seul <see cref="WorkTypeDef"/> decale donc silencieusement les priorites de tous les types
    /// suivants, chez tous les pions de la partie.
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
        /// Tous les pions porteurs de reglages de travail, pas seulement ceux poses sur une carte :
        /// une caravane, une nacelle en vol ou un pion en cryptosommeil ont eux aussi une
        /// <c>DefMap</c> a recaler.
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
        /// Remet chaque priorite en face de son type, redimensionne les <c>DefMap</c> au nouveau
        /// nombre de defs, et rafraichit les caches de chaque pion.
        /// </summary>
        /// <param name="seeds">
        /// defName d'un type derive vers le defName du type dont il a ete extrait. Un type que le
        /// pion ne connaissait pas herite de la priorite de sa source : scinder un travail en deux
        /// ne doit rien changer au comportement du colon.
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

                // Vide les caches d'incapacites, remet a zero ce que le pion n'a pas le droit de
                // faire, et previent l'onglet Competences.
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
