using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RimWorld;
using RimWorks.Pickle;
using UnityEngine;
using Verse;

namespace WorkStudio.PickleSteps
{
    /// <summary>
    /// What other mods put in front of Work Studio, from BACKLOG.md: a mechanoid whose priorities are set in
    /// Mech Work Tab and must survive a save and a load, and a work type another mod wrote without a label.
    /// </summary>
    [PickleSteps]
    public class OtherModSteps
    {
        // ---------------------------------------------------------------- mechanoids

        private static Pawn Mech(PickleContext ctx, string name)
        {
            var pawn = Find.Maps.SelectMany(m => m.mapPawns.PawnsInFaction(Faction.OfPlayer))
                .FirstOrDefault(p => p.RaceProps.IsMechanoid && string.Equals(p.Name?.ToStringShort, name,
                    StringComparison.OrdinalIgnoreCase));
            ctx.Require(pawn != null, $"no mechanoid of the player is called '{name}'");
            ctx.Require(pawn.workSettings != null, $"the mechanoid '{name}' has no work settings");
            pawn.workSettings.EnableAndInitializeIfNotAlreadyInitialized();
            return pawn;
        }

        [Given("the Work Studio test colony owns a mechanoid of kind {string} named {string}")]
        public void SpawnMech(PickleContext ctx, string kindName, string name)
        {
            var kind = DefDatabase<PawnKindDef>.GetNamedSilentFail(kindName);
            ctx.Require(kind != null, $"no pawn kind is named '{kindName}'");
            var map = Find.CurrentMap;
            ctx.Require(map != null, "no current map");
            var anchor = PawnsFinder.AllMaps_FreeColonists.FirstOrDefault();
            ctx.Require(anchor != null, "no colonist to stand the mechanoid next to");

            var request = new PawnGenerationRequest(kind, Faction.OfPlayer, PawnGenerationContext.NonPlayer, map.Tile,
                forceGenerateNewPawn: true);
            var pawn = PawnGenerator.GeneratePawn(request);
            pawn.Name = new NameSingle(name);
            ctx.Require(CellFinder.TryFindRandomCellNear(anchor.Position, map, 6,
                    c => c.Standable(map) && c.GetFirstPawn(map) == null, out var cell),
                "no free cell near the colonist for the mechanoid");
            GenSpawn.Spawn(pawn, cell, map);
            ctx.Require(pawn.workSettings != null && pawn.Faction == Faction.OfPlayer,
                $"the {kindName} spawned without work settings or outside the player's faction");
        }

        [When("I set the mechanoid {string} to priority {int} for {string}")]
        public void SetMechPriority(PickleContext ctx, string name, int priority, string type)
        {
            var pawn = Mech(ctx, name);
            var def = Driver.WorkType(ctx, type);
            var enabled = DefDatabase<WorkTypeDef>.AllDefsListForReading.Where(t => !pawn.WorkTypeIsDisabled(t))
                .Select(t => t.defName);
            ctx.Require(!pawn.WorkTypeIsDisabled(def),
                $"the mechanoid '{name}' cannot do {Driver.Describe(def)}; it can do: {string.Join(", ", enabled)}");
            Current.Game.playSettings.useWorkPriorities = true;
            pawn.workSettings.SetPriority(def, priority);
            ctx.Assert(pawn.workSettings.GetPriority(def) == priority,
                $"the game did not keep priority {priority} for {Driver.Describe(def)} on the mechanoid '{name}': " +
                $"it reads {pawn.workSettings.GetPriority(def)}");
        }

        [Then("the mechanoid {string} has priority {int} for {string}")]
        public void MechHasPriority(PickleContext ctx, string name, int priority, string type)
        {
            var pawn = Mech(ctx, name);
            var def = Driver.WorkType(ctx, type);
            var actual = pawn.workSettings.GetPriority(def);
            ctx.Assert(actual == priority,
                $"the mechanoid '{name}' has priority {actual} for {Driver.Describe(def)}, expected {priority}");
        }

        // ---------------------------------------------------------------- a type with no label

        private static readonly List<string> errors = new List<string>();
        private static bool watching;

        private static void OnLog(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
            {
                errors.Add(condition);
            }
        }

        [Given("I start watching the game log for Work Studio errors")]
        public void WatchLog(PickleContext ctx)
        {
            errors.Clear();
            if (!watching)
            {
                Application.logMessageReceived += OnLog;
                watching = true;
            }
        }

        [Then("the game log held no Work Studio error since I started watching")]
        public void NoErrors(PickleContext ctx)
        {
            if (watching)
            {
                Application.logMessageReceived -= OnLog;
                watching = false;
            }

            ctx.Assert(errors.Count == 0,
                $"{errors.Count} error(s) logged: " + string.Join(" || ", errors.Take(5).Select(e => e.Split('\n')[0])));
        }

        /// <summary>
        /// A work type written the way some mods write them: no label at all. Added to the database and
        /// pushed through Work Studio's own refresh, the way a mod loaded before it would have left it.
        /// </summary>
        [Given("another mod added the work type {string} with no label")]
        public void AddLabellessType(PickleContext ctx, string defName)
        {
            ctx.Require(DefDatabase<WorkTypeDef>.GetNamedSilentFail(defName) == null, $"'{defName}' exists already");
            var def = new WorkTypeDef
            {
                defName = defName,
                label = null,
                labelShort = null,
                pawnLabel = null,
                gerundLabel = null,
                verb = null,
                description = null,
                naturalPriority = 1,
                visible = true,
            };
            DefDatabase<WorkTypeDef>.Add(def);
            WorkTypeRuntime.Apply();
            ctx.Require(DefDatabase<WorkTypeDef>.GetNamedSilentFail(defName) == def, $"'{defName}' is not in the database after Apply");
        }

        /// <summary>Takes the type out again: it would otherwise stay in the database for every scenario played after it.</summary>
        [When("the other mod's work type {string} is taken out again")]
        public void RemoveLabellessType(PickleContext ctx, string defName)
        {
            var def = DefDatabase<WorkTypeDef>.GetNamedSilentFail(defName);
            ctx.Require(def != null, $"'{defName}' is not in the database");
            RemoveDef(def);
            var column = DefDatabase<PawnColumnDef>.GetNamedSilentFail("WorkPriority_" + defName);
            if (column != null)
            {
                RemoveDef(column);
            }

            WorkTypeRuntime.Apply();
            ctx.Assert(DefDatabase<WorkTypeDef>.GetNamedSilentFail(defName) == null, $"'{defName}' is still in the database");
        }

        /// <summary>DefDatabase.Remove is not public: Work Studio itself reaches it through a publicized reference.</summary>
        private static void RemoveDef<T>(T def) where T : Def, new()
        {
            var remove = typeof(DefDatabase<T>).GetMethod("Remove",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (remove == null)
            {
                throw new InvalidOperationException("DefDatabase<" + typeof(T).Name + ">.Remove was not found");
            }

            remove.Invoke(null, new object[] { def });
        }

        [Then("Work Studio names the work type {string} {string}")]
        public void NamesType(PickleContext ctx, string defName, string expected)
        {
            var def = Driver.WorkType(ctx, defName);
            var actual = WorkTypeRuntime.DisplayLabel(def);
            ctx.Assert(actual == expected,
                $"'{defName}' is named '{actual}', expected '{expected}' (label '{def.label}', labelShort '{def.labelShort}')");
        }

        /// <summary>
        /// Language-independent form of the naming check: the name is neither the raw defName nor invented, it is one of the texts the
        /// game ships for the type (its label or its short label, capitalised). In English BasicWorker has a short label only ("Basic"); in
        /// French the game translates its label too ("Manutention"), so an English expectation fails there (min-fr-122, run fd10).
        /// </summary>
        [Then("Work Studio names the work type {string} with a text of the game, not its defName")]
        public void NamesTypeWithGameText(PickleContext ctx, string defName)
        {
            var def = Driver.WorkType(ctx, defName);
            var actual = WorkTypeRuntime.DisplayLabel(def);
            ctx.Assert(actual != def.defName,
                $"'{defName}' is named by its raw defName (label '{def.label}', labelShort '{def.labelShort}')");
            var texts = new List<string>();
            if (!def.label.NullOrEmpty())
            {
                texts.Add(def.LabelCap.ToString());
            }

            if (!def.labelShort.NullOrEmpty())
            {
                texts.Add(def.labelShort.CapitalizeFirst());
            }

            ctx.Assert(texts.Any(t => string.Equals(t, actual, StringComparison.Ordinal)),
                $"'{defName}' is named '{actual}', which is none of the game's texts for it: {string.Join(", ", texts)}");
        }

        [Then("the work type editor lists the work type {string}")]
        public void EditorLists(PickleContext ctx, string defName)
        {
            var editor = Find.WindowStack.WindowOfType<Dialog_WorkTypes>();
            ctx.Require(editor != null, "the work type editor is not open");
            var types = WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder;
            ctx.Assert(types.Any(t => t.defName == defName), $"the priority-ordered list holds no '{defName}'");
        }
    }
}
