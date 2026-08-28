using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace WorkStudio
{
    /// <summary>
    /// Compare le paysage de defs a celui du dernier demarrage, et previent une fois si la liste de
    /// mods a bouge sous la configuration.
    /// <para>
    /// Rien de tout cela n'est une panne : une tache disparue est simplement ignoree, et son
    /// affectation reste en reserve au cas ou le mod reviendrait. Mais un type personnalise qui se
    /// vide sans un mot, ou une nouvelle colonne surgie au milieu d'un ordre patiemment regle, se
    /// remarquent tard et s'expliquent mal. D'ou cet avertissement, qui ne reparait pas tant que
    /// rien ne bouge.
    /// </para>
    /// </summary>
    public static class ConfigDrift
    {
        /// <summary>Au-dela, on resume plutot que d'aligner cinquante noms dans une boite de dialogue.</summary>
        private const int MaxNamesListed = 8;

        public static void ReportAtStartup()
        {
            var settings = WorkStudioMod.Settings;

            var ourTypes = new HashSet<string>(settings.customTypes
                .Where(e => e != null && !e.id.NullOrEmpty())
                .Select(e => e.id));

            // Nos propres types sont recrees a chaque demarrage : les compter comme des nouveautes
            // ferait sonner l'alarme a chaque lancement.
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

            // Premier demarrage : on ne fait qu'enregistrer le paysage, il n'y a rien a comparer.
            // Et sans configuration, un changement de liste de mods ne concerne personne.
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
