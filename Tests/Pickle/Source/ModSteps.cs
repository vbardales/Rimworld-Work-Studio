using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using RimWorks.Pickle;
using UnityEngine;
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
            // Two spellings on purpose. "[Work Studio]" is the prefix every deliberate Log call in
            // this mod carries, and matching only that would prove the mod chose to stay quiet -
            // not that nothing blew up. An exception thrown out of a patch is logged by the GAME,
            // with no prefix of ours, and the only thing of ours in it is the namespace in the
            // stack trace. A check that cannot see the failure it exists for is a permanent green;
            // Pickle had three of those found today, all showing up the same way.
            var lines = Log.Messages
                .Where(m => m.text != null &&
                            (m.text.Contains("[Work Studio]") || m.text.Contains("WorkStudio.")))
                .ToList();
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

        /// <summary>
        /// Says what is actually under the pointer before a click is sent, and names the mod it
        /// belongs to. Pickle's own failure is "tag not found", which reads like the button is
        /// missing when in truth something is sitting on top of it — Architect Studio spent a run
        /// on that, and it was a third-party window whose ASSEMBLY was the only thing that
        /// identified it; by type name alone they took it for a vanilla tutorial window.
        /// <para>
        /// Hovering first is not politeness: <c>Input.mousePosition</c> is sampled once per frame,
        /// so the pointer has to be where the click will land, a frame earlier, for the question to
        /// mean anything.
        /// </para>
        /// </summary>
        private static async Task WarnIfCovered(PickleContext ctx, string tag, Type expected = null)
        {
            try
            {
                await ctx.Hover(tag);
            }
            catch (Exception)
            {
                return; // no tag to hover: Pickle's own message says that better than this can
            }

            await ctx.WaitFrames(2);

            var pointer = UI.MousePositionOnUIInverted;
            var under = Find.WindowStack.GetWindowAt(pointer);
            if (under == null)
            {
                return;
            }

            // GetWindowAt only asks which rectangle holds the point. It cannot see a window that
            // does not hold it but absorbs input around itself - WindowStack.GetsInput walks down
            // from the top and answers false to everything below the first such window. That is the
            // invisible thing sitting on the button, and it was a blind spot here until 2026-09-21,
            // when a run with Work Tab loaded drew the button, clicked it, and opened nothing.
            if (!Find.WindowStack.GetsInput(under))
            {
                ctx.Assert(false,
                    $"'{tag}' is drawn in {under.GetType().Name}, but that window is not receiving input: " +
                    "a window above it absorbs everything around itself. The click would be swallowed. " +
                    "Window stack, top first:\n" + DescribeStack(pointer));
            }

            // With a type in hand, ask for that type. Without one - the keyed click reaches
            // whatever window drew the button - ask instead that the window belong to the game or
            // to this mod, which is what catches a third party sitting on top.
            string assembly = under.GetType().Assembly.GetName().Name;
            bool acceptable = expected != null
                ? expected.IsInstanceOfType(under)
                : assembly == "Assembly-CSharp" || assembly == typeof(WorkStudioMod).Assembly.GetName().Name;

            if (acceptable)
            {
                return;
            }

            ctx.Assert(false,
                $"'{tag}' is under another window: {under.GetType().FullName} from {assembly}"
                + (expected != null ? $", where {expected.Name} was expected" : string.Empty)
                + ". The click would go to that window, and Pickle would report the tag as missing.");
        }

        /// <summary>
        /// Every window on the stack, top first, with what decides whether a click reaches it.
        /// <para>
        /// An <see cref="ImmediateWindow"/> carries no identity of its own - its type name is all a
        /// report would say, and two of them look identical. What names it is the method that draws
        /// it, so the owner is read from <c>doWindowFunc</c>: the declaring type and its assembly say
        /// which mod put it there, which is the one thing a failed click needs to be attributed.
        /// </para>
        /// </summary>
        private static string DescribeStack(Vector2 pointer)
        {
            var stack = Find.WindowStack;
            var windows = stack.Windows;
            var lines = new List<string>();

            for (var i = windows.Count - 1; i >= 0; i--)
            {
                var window = windows[i];
                var drawnBy = string.Empty;

                if (window is ImmediateWindow immediate && immediate.doWindowFunc != null)
                {
                    var method = immediate.doWindowFunc.Method;
                    drawnBy = $"  drawn by {method.DeclaringType?.FullName}.{method.Name} " +
                              $"[{method.DeclaringType?.Assembly.GetName().Name}]";
                }

                lines.Add(
                    $"  #{i}{(i == windows.Count - 1 ? " top" : string.Empty)}  {window.GetType().Name} " +
                    $"[{window.GetType().Assembly.GetName().Name}]  layer={window.layer}  " +
                    $"rect={window.windowRect}  absorbsInput={window.absorbInputAroundWindow}  " +
                    $"getsInput={stack.GetsInput(window)}  holdsPointer={window.windowRect.Contains(pointer)}" +
                    drawnBy);
            }

            return lines.Count == 0 ? "  (no window at all)" : string.Join("\n", lines);
        }

        [When("I click the Work types button")]
        public async Task ClickOpenEditor(PickleContext ctx)
        {
            await WarnIfCovered(ctx, EditorButtonTag(), typeof(MainTabWindow));
            await ctx.Click(EditorButtonTag());

            // A click that lands and opens nothing is the failure this scenario exists for, and
            // the report used to say only "window should be open; open windows: ImmediateWindow,
            // ImmediateWindow, MainTabWindow_WorkTab" - two names that identify nothing. Read the
            // stack while it is still as the click left it.
            await ctx.WaitFrames(3);
            if (!Find.WindowStack.Windows.Any(window => window is Dialog_WorkTypes))
            {
                var pointer = UI.MousePositionOnUIInverted;
                var absorber = Find.WindowStack.Windows.Any(window => window.absorbInputAroundWindow);

                ctx.Assert(false,
                    "the click reached the button and the editor did not open. " +
                    (absorber
                        ? "A window on the stack absorbs input around itself - see below."
                        : "No window on the stack absorbs input, so nothing sits ABOVE the button. What is left " +
                          "is a control in the same window that took the click first: IMGUI hands an event to " +
                          "controls in the order they are drawn, and this button is drawn last, from a postfix. " +
                          "With Work Tab loaded that control is one of its three 30x30 toggles, which share the " +
                          "top-right corner of the very rectangle this button is placed against.") +
                    $"\nPointer at {pointer}. Window stack, top first:\n" + DescribeStack(pointer));
            }
        }

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
        [When("I open Work Studio's settings through the MainButtons shortcut")]
        public async Task OpenSettingsShortcut(PickleContext ctx)
        {
            var def = DefDatabase<MainButtonDef>.GetNamedSilentFail("WorkStudio_Settings");
            ctx.Require(def != null, "the MainButtonDef 'WorkStudio_Settings' is not loaded");
            def.Worker.Activate();
            await ctx.WaitFrames(2);
        }

        /// <summary>A frame-based pause, for a screenshot of something a step just opened.</summary>
        [When("I let the Work Studio interface draw")]
        public async Task LetInterfaceDraw(PickleContext ctx) => await ctx.WaitFrames(2);

        /// <summary>
        /// The other door: vanilla's own Mod options window, hosting the very same
        /// <c>DoSettingsWindowContents</c>. Opened the way the game opens it, with the mod instance,
        /// so closing it also runs vanilla's <c>PreClose</c> — which is what writes the settings to
        /// disk when a player leaves that window.
        /// </summary>
        [When("I open Work Studio's settings through Mod options")]
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

        [When("I close Work Studio's settings window")]
        public async Task CloseSettings(PickleContext ctx)
        {
            Find.WindowStack.WindowOfType<Dialog_ModSettings>()?.Close(doCloseSound: false);
            Find.WindowStack.WindowOfType<Dialog_WorkStudioSettings>()?.Close(doCloseSound: false);
            await ctx.WaitFrames(2);
        }

        [When("Work Studio's settings are re-read from disk, as a restart would")]
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
        [When("I click the Work Studio button keyed {string}")]
        public async Task ClickKeyed(PickleContext ctx, string key)
        {
            var tag = $"btn:{key.Translate()}";
            await WarnIfCovered(ctx, tag);
            await ctx.Click(tag);
        }

        /// <summary>
        /// A destructive confirmation is deliberately not clickable the instant it appears. Waiting
        /// on the dialog's own countdown beats guessing a number of frames, and says what it waits
        /// for.
        /// </summary>
        [When("I wait for Work Studio's confirmation to become clickable")]
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

        [Then("Work Studio asks to confirm first")]
        public void ConfirmationOpen(PickleContext ctx)
        {
            ctx.Assert(Find.WindowStack.WindowOfType<Dialog_MessageBox>() != null,
                "no Dialog_MessageBox is open: the action went through without asking");
        }

        [When("I close all windows but the main tabs, for Work Studio")]
        public async Task CloseTab(PickleContext ctx)
        {
            Find.MainTabsRoot.EscapeCurrentTab(playSound: false);
            await ctx.WaitFrames(2);
        }

        // ---------------------------------------------------------------- publication screenshots

        /// <summary>
        /// Windows whose <c>drawInScreenshotMode</c> this step turned off, so they can be turned
        /// back on. A scenario that dies between the two would otherwise leave the game with no
        /// interface at all, which is why <see cref="RestoreInterface"/> also runs after every
        /// scenario.
        /// </summary>
        private static readonly List<Window> hiddenWindows = new List<Window>();
        private static bool screenshotModeWasActive;

        /// <summary>
        /// Leaves only this mod's own windows on the map: the game's screenshot mode already draws
        /// nothing but windows that ask for it, so clearing the flag on everything else removes the
        /// tab bar, the alerts, the colonist bar, the dev tools - and Pickle's own runner panel,
        /// which sat in the corner of every @review capture until 2026-09-20 and had to be cropped
        /// out by hand. Pickle's windows are spotted by assembly rather than by type name, since it
        /// draws more than one.
        /// </summary>
        [When("I hide the interface around Work Studio's windows")]
        public async Task HideInterface(PickleContext ctx)
        {
            var root = Find.UIRoot;
            ctx.Require(root != null, "no UIRoot: the game is not drawing anything to photograph");

            screenshotModeWasActive = root.screenshotMode.Active;

            // Ours keep their default, which is to draw; only Pickle's own are taken out. The tab
            // bar, the alerts, the colonist bar and the dev tools are not windows at all - screenshot
            // mode drops those by itself.
            foreach (var window in Find.WindowStack.Windows)
            {
                bool isPickle = window.GetType().Assembly.GetName().Name
                    .StartsWith("RimWorks.Pickle", StringComparison.OrdinalIgnoreCase);

                if (isPickle && window.drawInScreenshotMode)
                {
                    window.drawInScreenshotMode = false;
                    hiddenWindows.Add(window);
                }
            }

            root.screenshotMode.Active = true;
            await ctx.WaitFrames(2);
        }

        [When("I bring the interface back around Work Studio's windows")]
        public async Task ShowInterface(PickleContext ctx)
        {
            RestoreInterface(ctx);
            await ctx.WaitFrames(2);
        }

        [AfterScenario]
        public void RestoreInterface(PickleContext ctx)
        {
            foreach (var window in hiddenWindows)
            {
                window.drawInScreenshotMode = true;
            }

            hiddenWindows.Clear();

            var root = Find.UIRoot;
            if (root != null && !screenshotModeWasActive)
            {
                root.screenshotMode.Active = false;
            }
        }

        // ---------------------------------------------------------------- export and import

        [When("I export the Work Studio setup as {string}")]
        public void Export(PickleContext ctx, string name)
        {
            ctx.Assert(ConfigFile.Export(SettingsSandbox.FilePrefix + name, out var error), $"export failed: {error}");
        }

        [When("I import the Work Studio setup {string}")]
        public void Import(PickleContext ctx, string name)
        {
            var path = ConfigFile.PathFor(SettingsSandbox.FilePrefix + name);
            ctx.Assert(ConfigFile.Import(path, out var error), $"import failed: {error}");
        }

        [When("I import a truncated copy of the Work Studio setup {string}")]
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

        [Then("the Work Studio import is refused")]
        public void Refused(PickleContext ctx)
        {
            ctx.Assert(!ctx.Get<ImportOutcome>().Succeeded, "a truncated file was reported as imported");
        }

        [Then("the exported Work Studio file {string} does not contain {string}")]
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

        [When("I remember Work Studio's whole setup")]
        public void Remember(PickleContext ctx) => ctx.Set(new ConfigSnapshot { Text = DescribeConfig() });

        [Then("Work Studio's whole setup is as remembered")]
        public void AsRemembered(PickleContext ctx)
        {
            var before = ctx.Get<ConfigSnapshot>().Text.Split('\n');
            var now = DescribeConfig().Split('\n');
            var diffs = before.Zip(now, (a, b) => a == b ? null : $"  was: {a}\n  now: {b}").Where(d => d != null).ToList();
            ctx.Assert(diffs.Count == 0, "the setup differs:\n" + string.Join("\n", diffs));
        }

        [Then("nothing is left to reset in Work Studio")]
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

        [Then("Work Studio opens {int} warning dialog(s)")]
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
