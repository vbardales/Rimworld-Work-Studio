using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using RimWorks.Pickle;
using Verse;

namespace WorkStudio.PickleSteps
{
    /// <summary>Loading, the Work tab button, export and import, the drift warning.</summary>
    [PickleSteps]
    public class ModSteps
    {
        // ---------------------------------------------------------------- loading

        [Then("Work Studio patched {string}")]
        public void Patched(PickleContext ctx, string target)
        {
            var parts = target.Split(new[] { "::" }, StringSplitOptions.None);
            ctx.Require(parts.Length == 2, $"write the target as Type::Member, not '{target}'");
            var type = AccessTools.TypeByName(parts[0]);
            ctx.Require(type != null, $"type '{parts[0]}' not found");
            AssertPatched(ctx, AccessTools.DeclaredMethod(type, parts[1]), target);
        }

        /// <summary>
        /// The window class is whatever the Work button names after every XML patch - Fluffy's Work
        /// Tab, Better Work Tab or vanilla - and the patch must sit on the class that declares the
        /// draw method, which twice was not the case.
        /// </summary>
        [Then("Work Studio patched the draw method of the window the Work tab really uses")]
        public void PatchedWorkTab(PickleContext ctx)
        {
            var windowClass = DefDatabase<MainButtonDef>.GetNamed("Work").tabWindowClass;
            MethodInfo declared = null;
            for (var t = windowClass; t != null && declared == null; t = t.BaseType)
            {
                declared = AccessTools.DeclaredMethod(t, nameof(Window.DoWindowContents));
            }

            ctx.Require(declared != null, $"{windowClass} declares no DoWindowContents anywhere up its hierarchy");
            AssertPatched(ctx, declared, $"{declared.DeclaringType.FullName}::DoWindowContents (tab window {windowClass.FullName})");
        }

        private static void AssertPatched(PickleContext ctx, MethodBase method, string name)
        {
            ctx.Require(method != null, $"'{name}' not found");
            var owners = Harmony.GetPatchInfo(method)?.Owners ?? (IEnumerable<string>)new string[0];
            ctx.Assert(owners.Contains(WorkStudioMod.HarmonyId), $"'{name}' carries no Work Studio patch; owners: {string.Join(", ", owners)}");
        }

        [Then("the game log holds nothing from Work Studio since startup")]
        public void LogSilent(PickleContext ctx)
        {
            var lines = Log.Messages.Where(m => m.text != null && m.text.Contains("[Work Studio]")).ToList();
            ctx.Assert(lines.Count == 0,
                "Work Studio logged at startup:\n" + string.Join("\n", lines.Select(m => $"{m.type}: {m.text}")));
        }

        [Then("the Work Studio button {string} is not drawn")]
        public async Task NotDrawn(PickleContext ctx, string label)
        {
            try
            {
                await ctx.Hover($"btn:{label}");
            }
            catch (Exception)
            {
                return;
            }

            ctx.Assert(false, $"a button labelled '{label}' is drawn here and should not be");
        }

        [When("I close all windows but the main tabs")]
        public async Task CloseTab(PickleContext ctx)
        {
            Find.MainTabsRoot.EscapeCurrentTab(playSound: false);
            await ctx.WaitFrames(2);
        }

        // ---------------------------------------------------------------- export and import

        [When("I export the setup as {string}")]
        public void Export(PickleContext ctx, string name)
        {
            ctx.Assert(ConfigFile.Export(SettingsSandbox.FilePrefix + name, out var error), $"export failed: {error}");
        }

        [When("I import the setup {string}")]
        public void Import(PickleContext ctx, string name)
        {
            var path = ConfigFile.PathFor(SettingsSandbox.FilePrefix + name);
            ctx.Assert(ConfigFile.Import(path, out var error), $"import failed: {error}");
        }

        [When("I import a truncated copy of the setup {string}")]
        public void ImportBroken(PickleContext ctx, string name)
        {
            var path = ConfigFile.PathFor(SettingsSandbox.FilePrefix + name);
            var broken = ConfigFile.PathFor(SettingsSandbox.FilePrefix + name + "-truncated");
            var text = File.ReadAllText(path);
            File.WriteAllText(broken, text.Substring(0, text.Length / 2));
            ctx.Set(new ImportOutcome { Succeeded = ConfigFile.Import(broken, out _) });
        }

        private sealed class ImportOutcome
        {
            public bool Succeeded;
        }

        [Then("the import is refused")]
        public void Refused(PickleContext ctx)
        {
            ctx.Assert(!ctx.Get<ImportOutcome>().Succeeded, "a truncated file was reported as imported");
        }

        [Then("the exported file {string} does not contain {string}")]
        public void FileLacks(PickleContext ctx, string name, string text)
        {
            var content = File.ReadAllText(ConfigFile.PathFor(SettingsSandbox.FilePrefix + name));
            ctx.Assert(!content.Contains(text), $"the export contains '{text}':\n{content}");
        }

        private sealed class ConfigSnapshot
        {
            public string Text;
        }

        /// <summary>The configuration as the export writes it, as text: two snapshots compare whole.</summary>
        private static string DescribeConfig()
        {
            var s = WorkStudioMod.Settings;
            return string.Join("\n", new[]
            {
                "types: " + string.Join("; ", s.customTypes.Select(e => $"{e.id}={e.label}/{e.labelShort}")),
                "tasks: " + string.Join("; ", s.giverAssignments.OrderBy(p => p.Key).Select(p => $"{p.Key}>{p.Value}")),
                "type order: " + string.Join("; ", s.priorityOverrides.OrderBy(p => p.Key).Select(p => $"{p.Key}={p.Value}")),
                "task order: " + string.Join("; ", s.giverOrderOverrides.OrderBy(p => p.Key).Select(p => $"{p.Key}={p.Value}")),
                "labels: " + string.Join("; ", s.labelOverrides.OrderBy(p => p.Key).Select(p => $"{p.Key}={p.Value}")),
                "hidden: " + string.Join("; ", s.hiddenTypes.OrderBy(x => x)),
                "defs: " + string.Join("; ", Driver.TypesInOrder().Select(t => $"{t.defName}:{t.label}:{t.naturalPriority}:{t.visible}")),
                "columns: " + string.Join("; ", Driver.WorkColumns().Select(c => c.workType.defName)),
                "givers: " + string.Join("; ", DefDatabase<WorkGiverDef>.AllDefsListForReading.Select(g => $"{g.defName}>{g.workType?.defName}:{g.priorityInType}"))
            });
        }

        [When("I remember the whole setup")]
        public void Remember(PickleContext ctx) => ctx.Set(new ConfigSnapshot { Text = DescribeConfig() });

        [Then("the whole setup is as remembered")]
        public void AsRemembered(PickleContext ctx)
        {
            var before = ctx.Get<ConfigSnapshot>().Text.Split('\n');
            var now = DescribeConfig().Split('\n');
            var diffs = before.Zip(now, (a, b) => a == b ? null : $"  was: {a}\n  now: {b}").Where(d => d != null).ToList();
            ctx.Assert(diffs.Count == 0, "the setup differs:\n" + string.Join("\n", diffs));
        }

        [Then("nothing is left to reset")]
        public void NothingLeft(PickleContext ctx)
        {
            ctx.Assert(!WorkTypeRuntime.HasOverrides(), "the reset button is still offered:\n" + DescribeConfig());
        }

        // ---------------------------------------------------------------- drift warning

        /// <summary>
        /// Stands in for a changed mod list: the startup record gets a type no mod provides any more,
        /// which is exactly what a disabled mod leaves behind.
        /// </summary>
        [Given("the last startup saw a work type {string} that is gone now")]
        public void GoneType(PickleContext ctx, string defName)
        {
            var settings = WorkStudioMod.Settings;
            settings.knownWorkTypes = DefDatabase<WorkTypeDef>.AllDefsListForReading
                .Where(t => !WorkTypeRuntime.IsCustom(t)).Select(t => t.defName).Append(defName).ToList();
            WorkStudioMod.Instance.WriteSettings();
        }

        [When("Work Studio runs its startup check")]
        public void StartupCheck(PickleContext ctx)
        {
            ctx.Set(new DialogCount { Before = Find.WindowStack.Windows.OfType<Dialog_MessageBox>().Count() });
            ConfigDrift.ReportAtStartup();
        }

        private sealed class DialogCount
        {
            public int Before;
        }

        [Then("it opens {int} warning dialog(s)")]
        public void Dialogs(PickleContext ctx, int expected)
        {
            var now = Find.WindowStack.Windows.OfType<Dialog_MessageBox>().ToList();
            var opened = now.Count - ctx.Get<DialogCount>().Before;
            ctx.Assert(opened == expected,
                $"expected {expected} new warning dialog(s); {opened} opened. Texts: " +
                string.Join(" | ", now.Select(d => d.text.ToString())));
        }
    }
}
