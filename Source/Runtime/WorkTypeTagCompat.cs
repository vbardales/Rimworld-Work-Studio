using System;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace WorkStudio
{
    /// <summary>
    /// Tells [baku] Work Type Tag that a work type has changed.
    /// <para>
    /// That mod shows the work type's name in front of a colonist's current action, and colours
    /// the matching column header. It already gets along very well with Work Studio: it indexes by
    /// <c>defName</c>, and an unknown colour is derived from the name, so a type created here
    /// automatically gets its tint and its tag.
    /// </para>
    /// <para>
    /// Its only blind spot is ours: it keeps its labels in a cache that it only clears from its
    /// own settings window. Renaming a work type from Work Studio would therefore leave the old
    /// name in front of colonists' actions until the next restart. A call to its
    /// <c>InvalidateAll</c> after each apply is enough.
    /// </para>
    /// <para>
    /// Soft binding, through reflection: no dependency, and the mod's absence costs only one type
    /// lookup on the first call.
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

                // Every mod assembly is loaded before our first Apply, so a failed lookup really
                // does mean the mod is absent.
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
                // Its signature has changed: stop trying rather than shout on every change. A
                // stale work type name is a nuisance, not a failure.
                invalidateAll = null;
                Log.Warning("[Work Studio] Could not refresh Work Type Tag's label cache: " +
                            exception.Message);
            }
        }
    }
}
