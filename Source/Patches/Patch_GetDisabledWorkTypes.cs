using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace WorkStudio
{
    /// <summary>
    /// Makes a derived work type inherit the incapacities of its source types.
    /// <para>
    /// The <see cref="WorkTags"/> set by <c>WorkTypeRuntime.ComputeInheritance</c> already cover
    /// backstories and traits. What remains are the named bans - genes, ideoligion roles, quests,
    /// health, life stage - which point at one specific <see cref="WorkTypeDef"/> and so cannot
    /// know anything about a type created mid-game: that is what this postfix catches.
    /// </para>
    /// </summary>
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.GetDisabledWorkTypes))]
    public static class Patch_GetDisabledWorkTypes
    {
        /// <summary>
        /// The list is completed in place, not a copy: it is the pawn's cache, which the game
        /// rebuilds from scratch as soon as it invalidates it. The membership test makes the
        /// operation idempotent, so a second call on the same list adds nothing.
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
