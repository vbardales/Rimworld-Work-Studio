using System;
using System.Collections;
using HarmonyLib;
using Verse;

namespace WorkStudio
{
    /// <summary>
    /// Empties the caches Fluffy's Work Tab keeps by work type.
    /// <para>
    /// That mod answers priorities itself, per task, and finds the tasks of a type through
    /// <c>WorkType_Extensions.WorkGivers</c>, which fills a static dictionary the first time a type
    /// is asked about and never clears it. Work Studio changes <c>WorkGiverDef.workType</c> and
    /// creates types, so the cached lists go stale: a task moved into a type is not in the type's
    /// list, and a type created and filled after it was first asked about reads priority 0 whatever
    /// is written to it.
    /// </para>
    /// <para>
    /// Soft binding, through reflection: no dependency, and the mod's absence costs only one type
    /// lookup on the first call.
    /// </para>
    /// </summary>
    public static class FluffyWorkTabCompat
    {
        private const string ExtensionsTypeName = "WorkTab.WorkType_Extensions";
        private static readonly string[] CacheFields = { "_workgiversByType", "workListCache" };

        private static bool looked;
        private static IDictionary[] caches;

        public static void Invalidate()
        {
            if (!looked)
            {
                looked = true;

                // Every mod assembly is loaded before our first Apply, so a failed lookup really
                // does mean the mod is absent.
                var extensions = AccessTools.TypeByName(ExtensionsTypeName);
                caches = extensions == null ? null : Find(extensions);
            }

            if (caches == null)
            {
                return;
            }

            foreach (var cache in caches)
            {
                cache.Clear();
            }
        }

        private static IDictionary[] Find(Type extensions)
        {
            try
            {
                var found = new IDictionary[CacheFields.Length];
                for (var i = 0; i < CacheFields.Length; i++)
                {
                    found[i] = (IDictionary)AccessTools.Field(extensions, CacheFields[i]).GetValue(null);
                }

                return found;
            }
            catch (Exception exception)
            {
                // A field was renamed: stop trying rather than shout on every change.
                Log.Warning("[Work Studio] Could not reach Fluffy's Work Tab caches: " + exception.Message);
                return null;
            }
        }
    }
}
