using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;
using RimWorld;
using Verse;

namespace WorkStudio
{
    /// <summary>
    /// Ecrit les priorites de travail une seconde fois dans la sauvegarde, cette fois <b>nommees</b>,
    /// et les relit a ce format.
    /// <para>
    /// Vanilla serialise <c>priorities</c> en <see cref="DefMap{D,V}"/>, c'est-a-dire une liste de
    /// valeurs nues alignee sur l'ordre de la <c>DefDatabase</c>. Rien dans la sauvegarde ne dit
    /// quelle valeur appartient a quel travail : au chargement, <c>DefMap.ExposeData</c> se contente
    /// de completer ou de tronquer la liste. Ajouter, retirer ou reordonner un type de travail entre
    /// deux sessions suffirait donc a redistribuer toutes les priorites de la colonie.
    /// </para>
    /// <para>
    /// Le noeud ajoute ici est ignore par le jeu si le mod est retire, et ne coute que quelques
    /// lignes par pion.
    /// </para>
    /// </summary>
    [HarmonyPatch(typeof(Pawn_WorkSettings), nameof(Pawn_WorkSettings.ExposeData))]
    public static class Patch_WorkSettingsExposeData
    {
        private const string Node = "workStudioPriorities";

        /// <summary>
        /// Ce qui a ete lu en phase <c>LoadingVars</c>, en attendant le <c>PostLoadInit</c> ou les
        /// defs sont resolus. Table faible : un pion abandonne en cours de chargement ne retient
        /// rien.
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
                // Un type absent de la sauvegarde est un type apparu depuis : un mod ajoute entre
                // deux sessions, typiquement. On le met a zero - inactif - plutot que de laisser la
                // valeur positionnelle que le chargement vanilla vient d'y deposer, qui appartient
                // au voisin. C'est aussi ce que fait le jeu sans nous : un travail inconnu de la
                // sauvegarde demarre eteint.
                values[i] = named.TryGetValue(types[i].defName, out var priority) ? priority : 0;
            }

            // Vanilla vient de desactiver ce que le pion n'a pas le droit de faire ; on a ecrase ce
            // travail en restaurant, il faut donc le refaire.
            settings.pawn?.Notify_DisabledWorkTypesChanged();
        }
    }
}
