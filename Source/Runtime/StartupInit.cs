using Verse;

namespace WorkStudio
{
    /// <summary>
    /// Applies the configuration once all defs are loaded and resolved.
    /// <para>
    /// Timing matters: the configuration must be in place <b>before</b> a save is loaded. Work
    /// priorities are written there positionally, aligned on the <c>DefDatabase</c> order; if our
    /// types were added afterwards, every pawn would get its neighbour's priorities.
    /// </para>
    /// </summary>
    [StaticConstructorOnStartup]
    public static class StartupInit
    {
        static StartupInit()
        {
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                // Before Apply: the Work tab's tabWindowClass is only resolved once defs are
                // loaded, and the XML patches of replacement tabs have already been applied to
                // it.
                Patch_WorkTabButton.Apply(WorkStudioMod.HarmonyInstance);

                WorkTypeRuntime.Apply();

                // After Apply: the report must describe the landscape as it really is, once our
                // types are created and the tasks filed.
                ConfigDrift.ReportAtStartup();
            });
        }
    }
}
