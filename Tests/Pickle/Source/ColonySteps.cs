using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using RimWorld;
using RimWorks.Pickle;
using Verse;

namespace WorkStudio.PickleSteps
{
    /// <summary>Colonists' priorities, the work they actually pick up, and saving and loading around a reconfiguration.</summary>
    [PickleSteps]
    public class ColonySteps
    {
        private static Pawn Colonist(PickleContext ctx, string nickname)
        {
            var pawn = PawnsFinder.AllMaps_FreeColonists.FirstOrDefault(p =>
                string.Equals(p.Name?.ToStringShort, nickname, StringComparison.OrdinalIgnoreCase));
            ctx.Require(pawn != null, $"no free colonist is called '{nickname}'");
            ctx.Require(pawn.workSettings != null, $"'{nickname}' has no work settings");
            pawn.workSettings.EnableAndInitializeIfNotAlreadyInitialized();
            return pawn;
        }

        [When("I set {string} to priority {int} for {string}")]
        public void SetPriority(PickleContext ctx, string nickname, int priority, string type)
        {
            var pawn = Colonist(ctx, nickname);
            var def = Driver.WorkType(ctx, type);
            ctx.Require(!pawn.WorkTypeIsDisabled(def),
                $"'{nickname}' cannot do {Driver.Describe(def)}, so its priority would stay 0; pick a type this colonist can do");
            // Manual priorities keep only 0 or 3 when off, which would erase every distinct value below.
            Current.Game.playSettings.useWorkPriorities = true;
            pawn.workSettings.SetPriority(def, priority);

            // Read straight back: it separates "the game never took the value" from "something
            // later dropped it", which the Then step alone cannot tell apart.
            ctx.Assert(pawn.workSettings.GetPriority(def) == priority,
                $"the game did not keep priority {priority} for {Driver.Describe(def)} on '{nickname}': it reads " +
                $"{pawn.workSettings.GetPriority(def)} right after the call. {Describe(pawn, def)}");
        }

        private static string Describe(Pawn pawn, WorkTypeDef def)
        {
            return $"disabled={pawn.WorkTypeIsDisabled(def)}, visible={def.visible}, " +
                $"useWorkPriorities={Current.Game.playSettings.useWorkPriorities}, " +
                $"hidden by the mod={WorkStudioMod.Settings.hiddenTypes.Contains(def.defName)}, " +
                $"raw DefMap value={RawPriority(pawn, def)}";
        }

        /// <summary>
        /// Pawn_WorkSettings.GetPriority(), straight from the private DefMap this mod's own
        /// PriorityMemory.Restore writes into - bypassing GetPriority() itself, since a mod that
        /// prefixes it (2026-09-17: Enhanced Work Tab keeps its own per-pawn priority store and
        /// answers GetPriority from that instead of the DefMap whenever time-aware priorities are
        /// on, its default) would otherwise make this diagnostic read the same wrong value the
        /// failing assertion already saw. A mismatch between this and GetPriority() proves the
        /// value the mod restored is intact and something else is what the player - or this step -
        /// actually reads back.
        /// </summary>
        private static int RawPriority(Pawn pawn, WorkTypeDef def)
        {
            var priorities = typeof(Pawn_WorkSettings)
                .GetField("priorities", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(pawn.workSettings);
            var values = priorities?.GetType()
                .GetField("values", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(priorities) as System.Collections.IList;
            return values != null && def.index < values.Count ? (int)values[def.index] : -1;
        }

        [Then("{string} has priority {int} for {string}")]
        public void HasPriority(PickleContext ctx, string nickname, int expected, string type)
        {
            var pawn = Colonist(ctx, nickname);
            var def = Driver.WorkType(ctx, type);
            var actual = pawn.workSettings.GetPriority(def);
            ctx.Assert(actual == expected,
                $"'{nickname}' should have {expected} for {Driver.Describe(def)}; it has {actual}. {Describe(pawn, def)}. Every priority: " +
                string.Join(", ", Driver.TypesInOrder().Select(t => $"{t.defName}={pawn.workSettings.GetPriority(t)}")));
        }

        [Given("{string} can do {string} and {string}")]
        public void CanDo(PickleContext ctx, string nickname, string a, string b)
        {
            var pawn = Colonist(ctx, nickname);
            foreach (var type in new[] { a, b })
            {
                var def = Driver.WorkType(ctx, type);
                ctx.Require(!pawn.WorkTypeIsDisabled(def),
                    $"the generated colonist '{nickname}' cannot do '{type}'; give them another backstory in the scenario");
            }
        }

        [When("{string} does nothing but {string}")]
        public void OnlyThis(PickleContext ctx, string nickname, string type)
        {
            var pawn = Colonist(ctx, nickname);
            var def = Driver.WorkType(ctx, type);
            Current.Game.playSettings.useWorkPriorities = true;
            foreach (var other in DefDatabase<WorkTypeDef>.AllDefsListForReading)
            {
                if (!pawn.WorkTypeIsDisabled(other))
                {
                    pawn.workSettings.SetPriority(other, other == def ? 1 : 0);
                }
            }

            ctx.Require(pawn.workSettings.GetPriority(def) == 1, $"'{nickname}' cannot do {Driver.Describe(def)}");
        }

        [Then("{string} would pick up the task {string} while working")]
        public void WouldPickUp(PickleContext ctx, string nickname, string giver)
        {
            var pawn = Colonist(ctx, nickname);
            var task = Driver.Task(ctx, giver);
            var list = pawn.workSettings.WorkGiversInOrderNormal.Select(w => w.def).ToList();
            ctx.Assert(list.Contains(task),
                $"'{giver}' is not among the tasks '{nickname}' goes through: {Driver.Names(list)}. " +
                "A type hidden or moved should not stop the colonist doing the work");
        }

        // ---------------------------------------------------------------- save and load

        [When("I save the game as {string}")]
        public void Save(PickleContext ctx, string name)
        {
            var file = SettingsSandbox.FilePrefix + name;
            GameDataSaveLoader.SaveGame(file);
            ctx.Require(File.Exists(GenFilePaths.FilePathForSavedGame(file)),
                $"saving '{file}' wrote no file; the error is in the log");
        }

        /// <summary>
        /// Loads a save written earlier in the scenario, under whatever work types exist now. That is
        /// the case the named priorities exist for: a save written with one set of types, read with
        /// another. Waits the way Pickle's own fixture step does.
        /// </summary>
        [When("I load the game {string}", TimeoutSeconds = 130f)]
        public async Task Load(PickleContext ctx, string name)
        {
            var file = SettingsSandbox.FilePrefix + name;
            ctx.Require(File.Exists(GenFilePaths.FilePathForSavedGame(file)), $"no save '{file}' to load");

            await ctx.WaitUntil(() => !LongEventHandler.AnyEventNowOrWaiting, 60f);
            var before = Current.Game;
            var suppress = typeof(PickleContext).Assembly.GetType("RimWorks.Pickle.Autorun.AutorunState")
                ?.GetProperty("SuppressingFixtureLoad", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            suppress?.SetValue(null, true);
            try
            {
                GameDataSaveLoader.LoadGame(file);
                await ctx.WaitUntil(() => Current.Game != null && !ReferenceEquals(Current.Game, before) &&
                                          Current.ProgramState == ProgramState.Playing &&
                                          !LongEventHandler.AnyEventNowOrWaiting && Find.CurrentMap != null, 120f);
                await ctx.WaitTicks(2);
            }
            finally
            {
                suppress?.SetValue(null, false);
            }
        }

        // ---------------------------------------------------------------- raw save inspection

        /// <summary>
        /// Scenario 12, without an actual restart: a companion mod bound to Work Studio's own
        /// assembly cannot script "and now the mod is gone" from inside itself. What it can do is
        /// prove, from the save Work Studio just wrote, that a mod-less load would read every
        /// non-custom priority correctly - because custom types are appended at the END of
        /// <see cref="DefDatabase{WorkTypeDef}"/> (never inserted among the earlier ones, see the
        /// README's note on scenario 05), a mod-less database is simply this same list with its
        /// tail cut off, and vanilla's own positional <c>DefMap</c> loading reads the same leading
        /// values into the same leading types either way. Only the trailing, custom-type values
        /// would have nowhere to go - the one loss TESTING.md documents.
        /// </summary>
        [Then("in the raw save {string}, the custom types sit at the end of {string}'s vanilla priority list, and nothing else would shift if the mod were gone")]
        public void SurvivesRemoval(PickleContext ctx, string saveName, string nickname)
        {
            var path = GenFilePaths.FilePathForSavedGame(SettingsSandbox.FilePrefix + saveName);
            ctx.Require(File.Exists(path), $"no save '{saveName}' to inspect");

            var doc = XDocument.Load(path);

            // A save with a large mod list can carry more than one <nick>Keeper</nick> - a world
            // pawn, a faction relation, a history entry - none of which are the live colonist this
            // step means. Every <nick> match is walked up to its nearest <workSettings>-bearing
            // ancestor, then filtered to the ones that actually carry Work Studio's own
            // <workStudioPriorities> node; a real colonist's block is the only kind that has one.
            var candidates = doc.Descendants("nick")
                .Where(e => e.Value == nickname)
                .Select(e => e.Ancestors().FirstOrDefault(a => a.Element("workSettings") != null)?.Element("workSettings"))
                .Where(ws => ws?.Element("workStudioPriorities") != null)
                .Distinct()
                .ToList();
            ctx.Require(candidates.Count > 0,
                $"no pawn named '{nickname}' with a <workStudioPriorities> node found in the raw save '{saveName}'");

            var currentTypeCount = DefDatabase<WorkTypeDef>.AllDefsListForReading.Count;
            var workSettings = candidates.Count == 1 ? candidates[0]
                : candidates.FirstOrDefault(ws => ws.Element("priorities")?.Element("vals")?.Elements("li").Count() == currentTypeCount);
            ctx.Require(workSettings != null,
                $"{candidates.Count} pawns named '{nickname}' found in the raw save '{saveName}', none with a " +
                $"<priorities><vals> matching the current {currentTypeCount} work types - ambiguous, cannot pick one");

            var vals = workSettings.Element("priorities")?.Element("vals")?.Elements("li")
                .Select(e => int.Parse(e.Value)).ToList();
            ctx.Require(vals != null, $"'{nickname}' has no <priorities><vals> in the raw save - vanilla's own node is missing");

            var named = workSettings.Element("workStudioPriorities")?.Elements("li")
                .ToDictionary(e => e.Element("key")?.Value, e => int.Parse(e.Element("value")?.Value ?? "0"));
            ctx.Require(named != null,
                $"'{nickname}' has no <workStudioPriorities> in the raw save - Patch_WorkSettingsExposeData did not run");

            // The order PriorityMemory and the ExposeData patch actually iterate: DefDatabase's own
            // list, aligned with def.index and therefore with the positional save - not the
            // priority-sorted order Driver.TypesInOrder() gives the editor's left column.
            var typesInOrder = DefDatabase<WorkTypeDef>.AllDefsListForReading;
            var nonCustom = typesInOrder.Where(t => !WorkTypeRuntime.IsCustom(t)).ToList();
            var custom = typesInOrder.Where(WorkTypeRuntime.IsCustom).ToList();
            ctx.Require(custom.Count > 0, "no custom type exists in this scenario to prove the point with");

            ctx.Assert(typesInOrder.Skip(nonCustom.Count).SequenceEqual(custom),
                "a custom type is not at the end of DefDatabase<WorkTypeDef> - removing the mod would shift more than just the tail");
            ctx.Assert(vals.Count == typesInOrder.Count,
                $"the raw priorities list has {vals.Count} entries for {typesInOrder.Count} work types");

            for (var i = 0; i < nonCustom.Count; i++)
            {
                var type = nonCustom[i];
                named.TryGetValue(type.defName, out var expected);
                ctx.Assert(vals[i] == expected,
                    $"position {i} ({type.defName}) holds {vals[i]} positionally but {expected} by name - " +
                    "a mod-less load reading this position would misassign it");
            }

            for (var i = 0; i < custom.Count; i++)
            {
                var type = custom[i];
                named.TryGetValue(type.defName, out var expected);
                var position = nonCustom.Count + i;
                ctx.Assert(vals[position] == expected,
                    $"the trailing position {position} for custom type '{type.defName}' holds {vals[position]} " +
                    $"positionally but {expected} by name - it would not simply drop off the end as expected");
            }
        }
    }
}
