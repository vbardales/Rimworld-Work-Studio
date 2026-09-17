using RimWorld;
using Verse;

namespace WorkStudio
{
    /// <summary>
    /// Worker for the hidden <c>WorkStudio_Settings</c> MainButtonDef. Not a tab: activating it
    /// opens <see cref="Dialog_WorkStudioSettings"/> directly, the same controls as Mod options ->
    /// Work Studio, for a MainButtons customization mod (RIMMSQOL and the like) to reveal.
    /// </summary>
    public class MainButtonWorker_OpenSettings : MainButtonWorker
    {
        public override void Activate()
        {
            Find.WindowStack.Add(new Dialog_WorkStudioSettings());
        }
    }
}
