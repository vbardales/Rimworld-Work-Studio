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
using Verse.AI;

namespace WorkStudio.PickleSteps
{
    /// <summary>Colonists' priorities, the work they actually pick up, and saving and loading around a reconfiguration.</summary>
    [PickleSteps]
    public class ColonySteps
    {
        /// <summary>
        /// Every pawn this step class ever had to initialize, by thingIDNumber. A colonist whose
        /// work settings are initialized HERE, rather than by the game, comes back with vanilla's
        /// own starting spread - about six types at 3, the rest at 0 - and every value a scenario
        /// set earlier is gone. That is indistinguishable from "something reset the priorities"
        /// unless it is recorded, which is what this is for: see Describe below.
        /// </summary>
        private static readonly HashSet<int> initializedHere = new HashSet<int>();

        private static Pawn Colonist(PickleContext ctx, string nickname)
        {
            var pawn = PawnsFinder.AllMaps_FreeColonists.FirstOrDefault(p =>
                string.Equals(p.Name?.ToStringShort, nickname, StringComparison.OrdinalIgnoreCase));
            ctx.Require(pawn != null, $"no free colonist is called '{nickname}'");
            ctx.Require(pawn.workSettings != null, $"'{nickname}' has no work settings");
            if (!pawn.workSettings.EverWork)
            {
                initializedHere.Add(pawn.thingIDNumber);
            }

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

            // From here on, every change to this pawn's priority for this type is recorded, with
            // the stack trace of whatever writes a zero. Four runs have narrowed scenario 8 by
            // elimination without ever saying WHEN the value dies or WHO takes it.
            PriorityProbe.Watch(pawn, def);

            // And read the DefMap itself, not through GetPriority: a mod that answers GetPriority
            // from its own store (Enhanced Work Tab does, when time-aware priorities are on) would
            // make the assertion above pass while vanilla's own list never received the value. The
            // 2026-09-18 run needs this to tell a write that never landed from a value overwritten
            // afterwards - the Then step's raw reading alone cannot say which happened.
            ctx.Assert(RawPriority(pawn, def) == priority,
                $"the vanilla DefMap did not receive priority {priority} for {Driver.Describe(def)} on " +
                $"'{nickname}': SetPriority ran, GetPriority reads {pawn.workSettings.GetPriority(def)}, but the " +
                $"list itself holds {RawPriority(pawn, def)}. Something is answering for the priorities " +
                $"instead of storing them. {Describe(pawn, def)}");
        }

        private static string Describe(Pawn pawn, WorkTypeDef def)
        {
            return $"disabled={pawn.WorkTypeIsDisabled(def)}, visible={def.visible}, " +
                $"useWorkPriorities={Current.Game.playSettings.useWorkPriorities}, " +
                $"hidden by the mod={WorkStudioMod.Settings.hiddenTypes.Contains(def.defName)}, " +
                $"raw DefMap value={RawPriority(pawn, def)}, " +
                $"pawn #{pawn.thingIDNumber}, work settings initialized by the suite=" +
                $"{initializedHere.Contains(pawn.thingIDNumber)}";
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
                $"'{nickname}' should have {expected} for {Driver.Describe(def)}; it has {actual}. {Describe(pawn, def)}. " +
                $"Probe: {PriorityProbe.Report()}. Every priority: " +
                string.Join(", ", Driver.TypesInOrder().Select(t => $"{t.defName}={pawn.workSettings.GetPriority(t)}")));
        }

        /// <summary>
        /// Asks whether the colonist may really do this work, and asks it of a cache that has just
        /// been dropped.
        /// <para>
        /// This guard existed to keep a scenario from testing a work type its colonist is not
        /// allowed to touch, and on 2026-09-20 it turned out to be the thing that let exactly that
        /// happen. The fixture's "Keeper" is generated with random backstories; that run drew
        /// <c>Rancher43</c>, whose <c>workDisables</c> is <c>ManualDumb</c>, which vanilla's own
        /// Cleaning carries — so Keeper genuinely could not clean, and Hauling, HaulingUrgent and
        /// KAU_UrgentHaul were out too. The guard passed anyway, because
        /// <c>Pawn.GetDisabledWorkTypes</c> answers from a cache that nothing had invalidated yet.
        /// The scenario then set a priority the game later took away — correctly — the moment
        /// <c>WorkTypeRuntime.Apply</c> cleared the backstory caches and the question was asked
        /// honestly.
        /// </para>
        /// <para>
        /// Dropping the caches here asks the real question. It does not call
        /// <c>Notify_DisabledWorkTypesChanged</c>, which would zero priorities as a side effect;
        /// it only forces the next read to recompute.
        /// </para>
        /// </summary>
        [Given("{string} can do the work types {string} and {string}")]
        public void CanDo(PickleContext ctx, string nickname, string a, string b)
        {
            var pawn = Colonist(ctx, nickname);
            foreach (var type in new[] { a, b })
            {
                var def = Driver.WorkType(ctx, type);
                var cachedAnswer = pawn.WorkTypeIsDisabled(def);
                DropDisabledWorkTypeCaches(pawn);
                var realAnswer = pawn.WorkTypeIsDisabled(def);

                ctx.Require(!realAnswer,
                    $"the generated colonist '{nickname}' cannot do '{type}': " +
                    $"{DisablingBackstories(pawn)}, and {type} carries {def.workTags}. " +
                    (cachedAnswer == realAnswer
                        ? "Give them another backstory in the scenario, or pick another work type."
                        : "The cached answer said otherwise, which is how this went unnoticed until " +
                          "2026-09-20: the priority was set, then correctly taken away as soon as " +
                          "Work Studio cleared the backstory caches."));
            }
        }

        /// <summary>
        /// Gives the colonist backstories and traits that forbid no work at all.
        /// <para>
        /// The fixture generates "Keeper" with random ones, so every scenario that names a work type
        /// was a coin flip on that run's draw: 2026-09-20 drew <c>Rancher43</c>, a rancher, whose
        /// <c>workDisables</c> is <c>ManualDumb</c> — which vanilla's Cleaning, Hauling,
        /// HaulingUrgent and KAU_UrgentHaul all carry. The scenarios then set priorities the game
        /// was right to take away, and three of them had been failing on it for four runs.
        /// </para>
        /// <para>
        /// Replacing the backstory rather than picking a work type at random keeps the scenarios
        /// readable: they can go on naming Cleaning, and mean it. Skills and passions are untouched —
        /// only what the pawn is permitted to do changes.
        /// </para>
        /// </summary>
        [Given("{string} is given backstories that disable no work type")]
        public void NoWorkDisables(PickleContext ctx, string nickname)
        {
            var pawn = Colonist(ctx, nickname);
            ctx.Require(pawn.story != null, $"'{nickname}' has no story tracker to give backstories to");

            ReplaceIfDisabling(ctx, pawn, BackstorySlot.Childhood);
            ReplaceIfDisabling(ctx, pawn, BackstorySlot.Adulthood);

            // Traits forbid work too, and a generated pawn can carry one that does.
            if (pawn.story.traits != null)
            {
                foreach (var trait in pawn.story.traits.allTraits.ToList())
                {
                    if (trait.GetDisabledWorkTypes().Any())
                    {
                        pawn.story.traits.RemoveTrait(trait);
                    }
                }
            }

            DropDisabledWorkTypeCaches(pawn);
            pawn.Notify_DisabledWorkTypesChanged();

            var left = pawn.GetDisabledWorkTypes();
            ctx.Require(left.Count == 0,
                $"'{nickname}' still cannot do {Driver.Names(left)} after being given harmless " +
                $"backstories - something other than a backstory or a trait forbids it " +
                $"(a gene, a quest, an ideoligion role, its life stage). {DisablingBackstories(pawn)}");
        }

        private static void ReplaceIfDisabling(PickleContext ctx, Pawn pawn, BackstorySlot slot)
        {
            var current = pawn.story.GetBackstory(slot);
            if (current == null || current.workDisables == WorkTags.None)
            {
                return;
            }

            // Never a child backstory on an adult: the setter logs a warning for that, and a warning
            // in the middle of a scenario is noise nobody will thank us for.
            var replacement = DefDatabase<BackstoryDef>.AllDefsListForReading.FirstOrDefault(b =>
                b.slot == slot
                && b.workDisables == WorkTags.None
                && !b.spawnCategories.Contains("Child"));

            ctx.Require(replacement != null,
                $"no {slot} backstory in this mod list forbids no work at all, so the colonist "
                + "cannot be made harmless");

            if (slot == BackstorySlot.Childhood)
            {
                pawn.story.Childhood = replacement;
            }
            else
            {
                pawn.story.Adulthood = replacement;
            }
        }

        private static void DropDisabledWorkTypeCaches(Pawn pawn)
        {
            foreach (var name in new[] { "cachedDisabledWorkTypes", "cachedDisabledWorkTypesPermanent" })
            {
                typeof(Pawn).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.SetValue(pawn, null);
            }

            // Private, and this test assembly carries no publicizer, so reflection stands in for one.
            var cache = typeof(BackstoryDef).GetField("cachedDisabledWorkTypes",
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (cache != null)
            {
                foreach (var backstory in DefDatabase<BackstoryDef>.AllDefsListForReading)
                {
                    cache.SetValue(backstory, null);
                }
            }
        }

        private static string DisablingBackstories(Pawn pawn)
        {
            if (pawn.story == null)
            {
                return "it has no backstories";
            }

            var said = new List<string>();
            foreach (var backstory in pawn.story.AllBackstories)
            {
                said.Add($"{backstory.defName} disables {backstory.workDisables}");
            }

            return string.Join(", ", said.ToArray());
        }


        [When("{string} does nothing but the work type {string}")]
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

        [When("{string} starts a visible job for the task {string}")]
        public void StartVisibleJob(PickleContext ctx, string nickname, string taskName)
        {
            var pawn = Colonist(ctx, nickname);
            var task = Driver.Task(ctx, taskName);
            var job = JobMaker.MakeJob(JobDefOf.Wait);
            job.workGiverDef = task;
            pawn.jobs.StartJob(job, JobCondition.InterruptForced);
            Find.Selector.ClearSelection();
            Find.Selector.Select(pawn);
            ctx.Require(pawn.CurJob == job && pawn.CurJob.workGiverDef == task,
                $"'{nickname}' did not start a visible job attributed to '{taskName}'");
        }

        [Then("{string}'s current job report contains the work type label {string}")]
        public void JobReportContains(PickleContext ctx, string nickname, string label)
        {
            var report = Colonist(ctx, nickname).GetJobReport();
            ctx.Assert(report.IndexOf(label, StringComparison.OrdinalIgnoreCase) >= 0,
                $"current job report did not contain '{label}': {report}");
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

        [When("I save the Work Studio test game as {string}")]
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
        [When("I load the Work Studio test game {string}", TimeoutSeconds = 130f)]
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

            // Scribe does not write a Dictionary as one <li> per pair. With LookMode.Value on both
            // sides it writes two parallel lists, <keys> and <values>. Reading the node's own
            // Elements("li") therefore finds nothing, and ToDictionary hands back an EMPTY
            // dictionary - not null, so a null check passes it. Every lookup below then answers
            // "absent", TryGetValue returns 0, and the first colonist with a non-zero priority
            // reads as a disagreement between the two lists. That is what failed this scenario on
            // 2026-09-20, against saves whose lists agree perfectly: 27 entries for each of 9
            // colonists, checked afterwards, not one mismatch.
            var node = workSettings.Element("workStudioPriorities");
            var keys = node?.Element("keys")?.Elements("li").Select(e => e.Value).ToList();
            var numbers = node?.Element("values")?.Elements("li").Select(e => int.Parse(e.Value)).ToList();
            ctx.Require(keys != null && numbers != null,
                $"'{nickname}'s <workStudioPriorities> carries no <keys>/<values> lists in the raw save - either " +
                "Patch_WorkSettingsExposeData did not run, or Scribe_Collections changed the shape it writes a " +
                "dictionary in, and this step is reading for the old one");
            ctx.Require(keys.Count == numbers.Count,
                $"'{nickname}'s named priorities are malformed: {keys.Count} keys for {numbers.Count} values");
            ctx.Require(keys.Count > 0,
                $"'{nickname}'s named priorities are empty - nothing to compare the positional list against");

            var named = new Dictionary<string, int>(keys.Count);
            for (var i = 0; i < keys.Count; i++)
            {
                named[keys[i]] = numbers[i];
            }

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

            // A type absent from the named list and a type named with a 0 are two different
            // findings, and TryGetValue's default tells them apart for neither. Asking first keeps
            // a step that cannot read the node at all from reporting a mod defect.
            for (var i = 0; i < nonCustom.Count; i++)
            {
                var type = nonCustom[i];
                ctx.Assert(named.ContainsKey(type.defName),
                    $"position {i} ({type.defName}) is missing from the named list altogether - the two lists are " +
                    "written in lockstep, so a type in one and not the other means the save is not what this step thinks it is");
                var expected = named[type.defName];
                ctx.Assert(vals[i] == expected,
                    $"position {i} ({type.defName}) holds {vals[i]} positionally but {expected} by name - " +
                    "a mod-less load reading this position would misassign it");
            }

            for (var i = 0; i < custom.Count; i++)
            {
                var type = custom[i];
                var position = nonCustom.Count + i;
                ctx.Assert(named.ContainsKey(type.defName),
                    $"custom type '{type.defName}' is missing from the named list, so nothing says what its trailing " +
                    $"position {position} is supposed to hold");
                var expected = named[type.defName];
                ctx.Assert(vals[position] == expected,
                    $"the trailing position {position} for custom type '{type.defName}' holds {vals[position]} " +
                    $"positionally but {expected} by name - it would not simply drop off the end as expected");
            }
        }
    }
}
