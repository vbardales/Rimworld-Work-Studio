using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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
        }

        [Then("{string} has priority {int} for {string}")]
        public void HasPriority(PickleContext ctx, string nickname, int expected, string type)
        {
            var pawn = Colonist(ctx, nickname);
            var def = Driver.WorkType(ctx, type);
            var actual = pawn.workSettings.GetPriority(def);
            ctx.Assert(actual == expected,
                $"'{nickname}' should have {expected} for {Driver.Describe(def)}; it has {actual}. Every priority: " +
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
    }
}
