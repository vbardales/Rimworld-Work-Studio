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

        /// <summary>
        /// Pickle tags a button by whatever text is actually drawn, so a hardcoded English literal
        /// in the feature file only ever matches on an English client - on hers (French), the drawn
        /// label is "Types de travail…" and every step built from the English string would silently
        /// miss the button entirely. Resolving the same key the button itself draws
        /// (Patch_WorkTabButton.cs) keeps this suite language-agnostic instead.
        /// </summary>
        private static string EditorButtonTag() => $"btn:{"WorkStudio.OpenEditorShort".Translate()}";

        [When("I click the Work types button")]
        public async Task ClickOpenEditor(PickleContext ctx) => await ctx.Click(EditorButtonTag());

        [Then("the Work Studio button is not drawn")]
        public async Task NotDrawn(PickleContext ctx)
        {
            try
            {
                await ctx.Hover(EditorButtonTag());
            }
            catch (Exception)
            {
                return;
            }

            ctx.Assert(false, $"the Work types button is drawn here and should not be");
        }

        /// <summary>
        /// Opens the settings window the way the hidden <c>WorkStudio_Settings</c> MainButtonDef
        /// does — through the def's own <see cref="MainButtonDef.Worker"/>, so what runs is the
        /// worker RimWorld would build and call, not a window this step made itself. That is the
        /// half of the shortcut a machine can check; whether RIMMSQOL lists and reveals the button
        /// stays manual.
        /// </summary>
        [When("I open the settings window through the MainButtons shortcut")]
        public async Task OpenSettingsShortcut(PickleContext ctx)
        {
            var def = DefDatabase<MainButtonDef>.GetNamedSilentFail("WorkStudio_Settings");
            ctx.Require(def != null, "the MainButtonDef 'WorkStudio_Settings' is not loaded");
            def.Worker.Activate();
            await ctx.WaitFrames(2);
        }

        /// <summary>A frame-based pause, for a screenshot of something a step just opened.</summary>
        [When("I let the interface draw")]
        public async Task LetInterfaceDraw(PickleContext ctx) => await ctx.WaitFrames(2);

        /// <summary>
        /// The other door: vanilla's own Mod options window, hosting the very same
        /// <c>DoSettingsWindowContents</c>. Opened the way the game opens it, with the mod instance,
        /// so closing it also runs vanilla's <c>PreClose</c> — which is what writes the settings to
        /// disk when a player leaves that window.
        /// </summary>
        [When("I open the settings window through Mod options")]
        public async Task OpenSettingsModOptions(PickleContext ctx)
        {
            Find.WindowStack.Add(new Dialog_ModSettings(WorkStudioMod.Instance));
            await ctx.WaitFrames(2);
        }

        [Then("the Mod options window is drawing Work Studio's own settings")]
        public void ModOptionsDrawsOurs(PickleContext ctx)
        {
            var window = Find.WindowStack.WindowOfType<Dialog_ModSettings>();
            ctx.Require(window != null, "no Dialog_ModSettings is open");

            var field = typeof(Dialog_ModSettings).GetField("mod", BindingFlags.Instance | BindingFlags.NonPublic);
            ctx.Require(field != null, "Dialog_ModSettings.mod no longer exists: update the steps");

            // Reference equality, not a name match: it is the single WorkStudioMod instance that
            // owns the single WorkStudioSettings both doors draw. Two instances would mean two
            // configurations that only look alike.
            ctx.Assert(ReferenceEquals(field.GetValue(window), WorkStudioMod.Instance),
                $"Mod options is hosting '{field.GetValue(window)}', not the running WorkStudioMod");
        }

        [When("I close the settings window")]
        public async Task CloseSettings(PickleContext ctx)
        {
            Find.WindowStack.WindowOfType<Dialog_ModSettings>()?.Close(doCloseSound: false);
            Find.WindowStack.WindowOfType<Dialog_WorkStudioSettings>()?.Close(doCloseSound: false);
            await ctx.WaitFrames(2);
        }

        [When("the settings are re-read from disk, as a restart would")]
        public async Task ReloadSettings(PickleContext ctx)
        {
            SettingsSandbox.ReloadFromDisk();
            await ctx.WaitFrames(2);
        }

        /// <summary>
        /// Clicks a button by the translation key it draws, not by the text. Pickle tags a button by
        /// what is on screen, so a literal in a feature file only ever matches one language; a key
        /// matches whichever is running. Works for vanilla keys too - "Confirm" and "GoBack" are
        /// what <c>Dialog_MessageBox.CreateConfirmation</c> labels its two buttons with.
        /// </summary>
        [When("I click the button keyed {string}")]
        public async Task ClickKeyed(PickleContext ctx, string key) => await ctx.Click($"btn:{key.Translate()}");

        /// <summary>
        /// A destructive confirmation is deliberately not clickable the instant it appears. Waiting
        /// on the dialog's own countdown beats guessing a number of frames, and says what it waits
        /// for.
        /// </summary>
        [When("I wait for the confirmation to become clickable")]
        public async Task WaitForConfirmation(PickleContext ctx)
        {
            var dialog = Find.WindowStack.WindowOfType<Dialog_MessageBox>();
            ctx.Require(dialog != null, "no confirmation dialog is open to wait for");

            var property = typeof(Dialog_MessageBox).GetProperty("TimeUntilInteractive",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (property == null)
            {
                await ctx.WaitFrames(2);
                return;
            }

            await ctx.WaitUntil(() => (float)property.GetValue(dialog) <= 0f, 10f);
        }

        [Then("a confirmation dialog is open")]
        public void ConfirmationOpen(PickleContext ctx)
        {
            ctx.Assert(Find.WindowStack.WindowOfType<Dialog_MessageBox>() != null,
                "no Dialog_MessageBox is open: the action went through without asking");
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
