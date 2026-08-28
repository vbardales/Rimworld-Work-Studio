using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace WorkStudio
{
    /// <summary>
    /// Fait heriter un type de travail derive des incapacites de ses types sources.
    /// <para>
    /// Les <see cref="WorkTags"/> poses par <c>WorkTypeRuntime.ComputeInheritance</c> couvrent deja
    /// les histoires et les traits. Restent les interdits nommes - genes, roles ideologiques,
    /// quetes, sante, stade de vie - qui designent un <see cref="WorkTypeDef"/> precis et ne peuvent
    /// donc rien savoir d'un type cree en cours de partie : c'est ce que ce postfix rattrape.
    /// </para>
    /// </summary>
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.GetDisabledWorkTypes))]
    public static class Patch_GetDisabledWorkTypes
    {
        /// <summary>
        /// On complete la liste sur place, et non une copie : c'est le cache du pion, que le jeu
        /// reconstruit de zero des qu'il l'invalide. Le test d'appartenance rend l'operation
        /// idempotente, donc un second appel sur la meme liste n'ajoute rien.
        /// </summary>
        [HarmonyPriority(Priority.Last)]
        public static void Postfix(List<WorkTypeDef> __result)
        {
            var inherited = WorkTypeRuntime.InheritedDisabling;
            if (__result == null || inherited.Count == 0)
            {
                return;
            }

            foreach (var pair in inherited)
            {
                if (__result.Contains(pair.Key))
                {
                    continue;
                }

                var origins = pair.Value;
                for (var i = 0; i < origins.Count; i++)
                {
                    if (__result.Contains(origins[i]))
                    {
                        __result.Add(pair.Key);
                        break;
                    }
                }
            }
        }
    }
}
