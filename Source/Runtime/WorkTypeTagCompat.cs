using System;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace WorkStudio
{
    /// <summary>
    /// Previent [baku] Work Type Tag qu'un type de travail a change.
    /// <para>
    /// Ce mod affiche le nom du travail devant l'action courante d'un colon, et colore l'en-tete
    /// de colonne assorti. Il s'entend deja tres bien avec Work Studio : il indexe par
    /// <c>defName</c>, et une couleur inconnue est derivee du nom, donc un type cree ici recoit
    /// automatiquement sa teinte et son etiquette.
    /// </para>
    /// <para>
    /// Son seul angle mort est le notre : il garde ses libelles dans un cache qu'il ne vide que
    /// depuis sa propre fenetre de reglages. Renommer un travail depuis Work Studio laisserait
    /// donc l'ancien nom devant l'action des colons jusqu'au redemarrage suivant. Un appel a son
    /// <c>InvalidateAll</c> apres chaque application suffit.
    /// </para>
    /// <para>
    /// Liaison molle, par reflexion : aucune dependance, et l'absence du mod ne coute qu'une
    /// recherche de type au premier appel.
    /// </para>
    /// </summary>
    public static class WorkTypeTagCompat
    {
        private const string ResolverTypeName = "baku.WorkTypeTag.WorkTypeColorResolver";

        private static bool looked;
        private static MethodInfo invalidateAll;

        public static void Notify()
        {
            if (!looked)
            {
                looked = true;

                // Toutes les assemblies de mods sont chargees avant notre premier Apply, donc une
                // recherche infructueuse signifie bien que le mod est absent.
                var resolver = AccessTools.TypeByName(ResolverTypeName);
                invalidateAll = resolver == null ? null : AccessTools.Method(resolver, "InvalidateAll");
            }

            if (invalidateAll == null)
            {
                return;
            }

            try
            {
                invalidateAll.Invoke(null, null);
            }
            catch (Exception exception)
            {
                // Sa signature a change : on cesse d'essayer plutot que de crier a chaque
                // modification. Un nom de travail perime est un desagrement, pas une panne.
                invalidateAll = null;
                Log.Warning("[Work Studio] Could not refresh Work Type Tag's label cache: " +
                            exception.Message);
            }
        }
    }
}
