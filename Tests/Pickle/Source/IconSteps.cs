using System;
using System.Collections.Generic;
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
    /// <summary>
    /// The skill icons, the Work tab column icons and the header mode: what the three display
    /// settings really do on screen. A drawing cannot be asserted from its pixels, so a probe counts
    /// what Work Studio hands to <c>GUI.DrawTexture</c> and what its header prefix answers to the
    /// game, and the scenarios read the counters while the real tab is open.
    /// <para>
    /// The probe patches every <c>GUI.DrawTexture</c> overload that takes an <c>image</c>, not one:
    /// the short overloads only forward to the long one and a JIT may inline them into a caller that
    /// was compiled before the patch. A draw is therefore counted once per overload it passes
    /// through, so the scenarios only ever ask "some" or "none", never "how many".
    /// </para>
    /// </summary>
    [PickleSteps]
    public class IconSteps
    {
        private const string ProbeId = "workstudio.pickle.icons";

        private static Harmony probe;
        private static HashSet<Texture> skillTextures = new HashSet<Texture>();
        private static HashSet<Texture> workTypeTextures = new HashSet<Texture>();
        private static int skillDraws;
        private static int workTypeDraws;
        private static readonly Dictionary<string, bool> headerLeftToTheGame = new Dictionary<string, bool>();

        // ------------------------------------------------------------------------ the settings

        private static void Save() => WorkStudioMod.Instance.WriteSettings();

        private static bool OnOff(PickleContext ctx, string word)
        {
            ctx.Require(word == "on" || word == "off", $"'{word}' is neither 'on' nor 'off'");
            return word == "on";
        }

        private static int HeaderMode(PickleContext ctx, string name)
        {
            switch (name)
            {
                case "icon and label": return WorkTypeIcons.HeaderIconAndLabel;
                case "icon only": return WorkTypeIcons.HeaderIconOnly;
                case "label only": return WorkTypeIcons.HeaderLabelOnly;
            }

            ctx.Require(false, $"'{name}' is not a header mode: 'icon and label', 'icon only' or 'label only'");
            return -1;
        }

        [When("I turn the Work Studio skill icons {word}")]
        public void SkillIcons(PickleContext ctx, string word)
        {
            WorkStudioMod.Settings.showSkillIcons = OnOff(ctx, word);
            Save();
        }

        [When("I turn the Work Studio column icons {word}")]
        public void ColumnIcons(PickleContext ctx, string word)
        {
            WorkStudioMod.Settings.showWorkTypeIcons = OnOff(ctx, word);
            Save();
        }

        [When("I set the Work Studio column header to {string}")]
        public void ColumnHeader(PickleContext ctx, string name)
        {
            WorkStudioMod.Settings.workTabHeaderMode = HeaderMode(ctx, name);
            Save();
        }

        [Then("a new Work Studio settings object starts with the skill icons on, the column icons on and the header showing icon and label")]
        public void IconDefaults(PickleContext ctx)
        {
            var fresh = new WorkStudioSettings();
            ctx.Assert(fresh.showSkillIcons, "a new settings object starts with the skill icons off");
            ctx.Assert(fresh.showWorkTypeIcons, "a new settings object starts with the column icons off");
            ctx.Assert(fresh.workTabHeaderMode == WorkTypeIcons.HeaderIconAndLabel,
                $"a new settings object starts with the header mode {fresh.workTabHeaderMode}");
        }

        [Then("the Work Studio skill icons are {word}, the column icons are {word} and the header shows {string}")]
        public void IconSettingsAre(PickleContext ctx, string skills, string columns, string header)
        {
            var settings = WorkStudioMod.Settings;
            ctx.Assert(settings.showSkillIcons == OnOff(ctx, skills),
                $"the skill icons are {(settings.showSkillIcons ? "on" : "off")}, expected {skills}");
            ctx.Assert(settings.showWorkTypeIcons == OnOff(ctx, columns),
                $"the column icons are {(settings.showWorkTypeIcons ? "on" : "off")}, expected {columns}");
            ctx.Assert(settings.workTabHeaderMode == HeaderMode(ctx, header),
                $"the header mode is {settings.workTabHeaderMode}, expected '{header}'");
        }

        // ------------------------------------------------------------------------ the probe

        [When("I start counting the icons Work Studio draws")]
        public void StartCounting(PickleContext ctx)
        {
            StopProbe();

            skillTextures = new HashSet<Texture>();
            foreach (var def in DefDatabase<SkillDef>.AllDefsListForReading)
            {
                var icon = WorkTypeIcons.For(def);
                if (icon != null) skillTextures.Add(icon);
            }

            workTypeTextures = new HashSet<Texture>();
            foreach (var def in DefDatabase<WorkTypeDef>.AllDefsListForReading)
            {
                var icon = WorkTypeIcons.For(def);
                if (icon != null) workTypeTextures.Add(icon);
            }

            ctx.Require(skillTextures.Count > 0, "Work Studio found no skill icon texture to look for: Textures/WorkStudio/Skills is missing");
            ctx.Require(workTypeTextures.Count > 0, "Work Studio found no work type icon texture to look for: Textures/WorkStudio/WorkTypes is missing");

            skillDraws = 0;
            workTypeDraws = 0;
            headerLeftToTheGame.Clear();

            probe = new Harmony(ProbeId);

            var draws = typeof(GUI).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name == "DrawTexture" && m.GetParameters().Any(p => p.Name == "image"))
                .ToList();
            ctx.Require(draws.Count > 0, "GUI.DrawTexture has no overload with a parameter named 'image' any more: update the probe");
            var count = new HarmonyMethod(typeof(IconSteps), nameof(CountDraw));
            foreach (var draw in draws)
            {
                probe.Patch(draw, prefix: count);
            }

            var header = AccessTools.Method(typeof(WorkTypeIcons), nameof(WorkTypeIcons.HeaderPrefix));
            ctx.Require(header != null, "WorkTypeIcons.HeaderPrefix no longer exists: update the probe");
            probe.Patch(header, postfix: new HarmonyMethod(typeof(IconSteps), nameof(AfterHeader)));
        }

        public static void CountDraw(Texture image)
        {
            if (image == null) return;
            if (skillTextures.Contains(image)) skillDraws++;
            else if (workTypeTextures.Contains(image)) workTypeDraws++;
        }

        /// <summary><c>__args</c> rather than <c>__instance</c>: the patched method is static.</summary>
        public static void AfterHeader(object[] __args, bool __result)
        {
            var worker = __args != null && __args.Length > 1 ? __args[1] as PawnColumnWorker : null;
            var type = worker?.def?.workType;
            if (type != null)
            {
                headerLeftToTheGame[type.defName] = __result;
            }
        }

        private static void StopProbe()
        {
            probe?.UnpatchAll(ProbeId);
            probe = null;
        }

        [AfterScenario]
        public void CleanUpIcons(PickleContext ctx)
        {
            StopProbe();

            // A scenario of 01-loading runs at the main menu, with no map and no selector: asking
            // Find.Selector there throws, and a throwing hook fails the scenario it follows.
            if (Current.ProgramState == ProgramState.Playing)
            {
                Find.Selector.ClearSelection();
            }
        }

        // ------------------------------------------------------------------------ the tabs

        /// <summary>
        /// The Character tab is where the skill list is drawn: the colonist is selected and the
        /// inspect pane opened on that tab, the way a click on the colonist and on the tab does.
        /// </summary>
        [When("I open the Work Studio Character tab of {string}")]
        public async Task OpenCharacterTab(PickleContext ctx, string nickname)
        {
            var pawn = PawnsFinder.AllMaps_FreeColonists.FirstOrDefault(p =>
                string.Equals(p.Name?.ToStringShort, nickname, StringComparison.OrdinalIgnoreCase));
            ctx.Require(pawn != null, $"no free colonist is called '{nickname}'");

            Find.Selector.ClearSelection();
            Find.Selector.Select(pawn, playSound: false, forceDesignatorDeselect: false);
            Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Inspect, playSound: false);
            InspectPaneUtility.OpenTab(typeof(ITab_Pawn_Character));
            await ctx.WaitFrames(5);
        }

        [When("I let the Work Studio icons draw for a moment")]
        public async Task LetIconsDraw(PickleContext ctx) => await ctx.WaitFrames(8);

        [When("I close the Work Studio Character tab")]
        public async Task CloseCharacterTab(PickleContext ctx)
        {
            Find.Selector.ClearSelection();
            Find.MainTabsRoot.EscapeCurrentTab(playSound: false);
            await ctx.WaitFrames(2);
        }

        // ------------------------------------------------------------------------ what was drawn

        [Then("Work Studio drew a skill icon")]
        public void DrewSkillIcon(PickleContext ctx) =>
            ctx.Assert(skillDraws > 0, "no skill icon was drawn while the Character tab was open");

        [Then("Work Studio drew no skill icon")]
        public void DrewNoSkillIcon(PickleContext ctx) =>
            ctx.Assert(skillDraws == 0, $"a skill icon was drawn ({skillDraws} draw calls) with the skill icons off");

        [Then("Work Studio drew a work type icon")]
        public void DrewWorkTypeIcon(PickleContext ctx) =>
            ctx.Assert(workTypeDraws > 0, "no work type icon was drawn while the Work tab was open");

        [Then("Work Studio drew no work type icon")]
        public void DrewNoWorkTypeIcon(PickleContext ctx) =>
            ctx.Assert(workTypeDraws == 0, $"a work type icon was drawn ({workTypeDraws} draw calls) with the column icons off or the header label only");

        [Then("Work Studio left the Work tab header of {string} to the game")]
        public void HeaderLeftToTheGame(PickleContext ctx, string defName) => AssertHeader(ctx, defName, true);

        [Then("Work Studio took over the Work tab header of {string}")]
        public void HeaderTakenOver(PickleContext ctx, string defName) => AssertHeader(ctx, defName, false);

        private static void AssertHeader(PickleContext ctx, string defName, bool leftToTheGame)
        {
            ctx.Require(headerLeftToTheGame.TryGetValue(defName, out var actual),
                $"the Work tab never drew a header for '{defName}' while the probe ran (it drew: "
                + string.Join(", ", headerLeftToTheGame.Keys.OrderBy(k => k)) + ")");
            ctx.Assert(actual == leftToTheGame,
                $"the header of '{defName}' was {(actual ? "left to the game" : "taken over by Work Studio")}, expected the opposite");
        }
    }
}
