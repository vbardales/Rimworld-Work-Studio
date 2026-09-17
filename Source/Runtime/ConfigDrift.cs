using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace WorkStudio
{
    /// <summary>
    /// Compares the def landscape with the one from the last startup, and warns once if the mod
    /// list has shifted under the configuration.
    /// <para>
    /// None of this is a failure: a vanished task is simply ignored, and its assignment is kept in
    /// reserve in case the mod comes back. But a custom type that empties without a word, or a new
    /// column popping up in the middle of a patiently tuned order, gets noticed late and is hard to
    /// explain. Hence this warning, which does not show again as long as nothing moves.
    /// </para>
    /// </summary>
    public static class ConfigDrift
    {
        /// <summary>Beyond this, summarize rather than line up fifty names in a dialog box.</summary>
        private const int MaxNamesListed = 8;

        public static void ReportAtStartup()
        {
            var settings = WorkStudioMod.Settings;

            var ourTypes = new HashSet<string>(settings.customTypes
                .Where(e => e != null && !e.id.NullOrEmpty())
                .Select(e => e.id));

            // Our own types are recreated at every startup: counting them as new arrivals would
            // sound the alarm at every launch.
            var currentTypes = DefDatabase<WorkTypeDef>.AllDefsListForReading
                .Select(t => t.defName)
                .Where(name => !ourTypes.Contains(name))
                .OrderBy(name => name)
                .ToList();

            var missingTasks = settings.giverAssignments.Keys
                .Where(defName => DefDatabase<WorkGiverDef>.GetNamedSilentFail(defName) == null)
                .OrderBy(defName => defName)
                .ToList();

            var firstRun = settings.knownWorkTypes.Count == 0;
            var newTypes = currentTypes.Except(settings.knownWorkTypes).ToList();
            var goneTypes = settings.knownWorkTypes.Except(currentTypes).ToList();
            var missingChanged = !missingTasks.SequenceEqual(settings.reportedMissingTasks.OrderBy(n => n));

            settings.knownWorkTypes = currentTypes;
            settings.reportedMissingTasks = missingTasks;
            WorkStudioMod.Instance?.WriteSettings();

            // First startup: only record the landscape, there is nothing to compare against.
            // And without a configuration, a mod list change concerns no one.
            if (firstRun || !WorkTypeRuntime.HasOverrides())
            {
                return;
            }

            if (newTypes.Count == 0 && goneTypes.Count == 0 && !missingChanged)
            {
                return;
            }

            var text = BuildReport(goneTypes, newTypes, missingTasks);
            if (text.NullOrEmpty())
            {
                return;
            }

            Find.WindowStack?.Add(new Dialog_MessageBox(
                text,
                "OK".Translate(),
                null,
                "WorkStudio.Settings.OpenEditor".Translate(),
                () => Find.WindowStack.Add(new Dialog_WorkTypes()),
                "WorkStudio.Drift.Title".Translate()));
        }

        private static string BuildReport(List<string> goneTypes, List<string> newTypes,
            List<string> missingTasks)
        {
            var report = new StringBuilder();
            report.AppendLine("WorkStudio.Drift.Intro".Translate());

            if (goneTypes.Count > 0)
            {
                report.AppendLine();
                report.AppendLine("WorkStudio.Drift.GoneTypes".Translate(Summarize(goneTypes, Plain)));
            }

            if (missingTasks.Count > 0)
            {
                report.AppendLine();
                report.AppendLine("WorkStudio.Drift.MissingTasks".Translate(Summarize(missingTasks, Plain)));
            }

            if (newTypes.Count > 0)
            {
                report.AppendLine();
                report.AppendLine("WorkStudio.Drift.NewTypes".Translate(Summarize(newTypes, TypeLabel)));
            }

            return report.ToString().TrimEndNewlines();
        }

        private static string Plain(string defName) => defName;

        private static string TypeLabel(string defName)
        {
            var def = DefDatabase<WorkTypeDef>.GetNamedSilentFail(defName);
            return def == null || def.label.NullOrEmpty() ? defName : def.LabelCap.ToString();
        }

        private static string Summarize(List<string> names, System.Func<string, string> display)
        {
            var shown = names.Take(MaxNamesListed).Select(display).ToList();
            var text = shown.ToCommaList(useAnd: true);

            var rest = names.Count - shown.Count;
            return rest > 0 ? text + " " + "WorkStudio.Drift.AndMore".Translate(rest) : text;
        }
    }
}
