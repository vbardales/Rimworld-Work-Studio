using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorks.Pickle;
using UnityEngine;
using Verse;

namespace WorkStudio.PickleSteps
{
    /// <summary>
    /// Bills at a workbench: a player reported (Work Studio#2) that with the mod on, no pawn takes the
    /// bills of a campfire or another furniture, and the right-click "prioritize" option is missing.
    /// The vanilla menu builds that option from <c>WorkTypeDef.workGiversByPriority</c> of every type,
    /// and a pawn's work comes from the same lists, so both are checked here.
    /// </summary>
    [PickleSteps]
    public class BillSteps
    {
        private static Pawn Colonist(PickleContext ctx, string nickname)
        {
            var pawn = PawnsFinder.AllMaps_FreeColonists.FirstOrDefault(p =>
                string.Equals(p.Name?.ToStringShort, nickname, StringComparison.OrdinalIgnoreCase));
            ctx.Require(pawn != null, $"no free colonist is called '{nickname}'");
            return pawn;
        }

        private static List<WorkGiverDef> BillTasks()
        {
            return DefDatabase<WorkGiverDef>.AllDefsListForReading
                .Where(g => typeof(WorkGiver_DoBill).IsAssignableFrom(g.giverClass))
                .ToList();
        }

        [Then("every bill task is listed under its own work type and no other")]
        public void EveryBillTaskIsListedOnce(PickleContext ctx)
        {
            var tasks = BillTasks();
            ctx.Require(tasks.Count > 0, "no work giver derives from WorkGiver_DoBill");
            var problems = new List<string>();
            foreach (var task in tasks)
            {
                var owners = DefDatabase<WorkTypeDef>.AllDefsListForReading
                    .Where(t => t.workGiversByPriority.Contains(task))
                    .Select(t => t.defName)
                    .ToList();
                if (task.workType == null)
                {
                    problems.Add($"{task.defName}: workType is null");
                }
                else if (owners.Count != 1 || owners[0] != task.workType.defName)
                {
                    problems.Add($"{task.defName}: workType {task.workType.defName}, listed under [{string.Join(", ", owners)}]");
                }
            }

            ctx.Assert(problems.Count == 0,
                $"{problems.Count} of {tasks.Count} bill tasks are not listed under exactly their own type: " +
                string.Join("; ", problems));
        }

        [Then("{string} goes through every bill task of the types they work")]
        public void WouldPickUpEveryBillTask(PickleContext ctx, string nickname)
        {
            var pawn = Colonist(ctx, nickname);
            var reached = pawn.workSettings.WorkGiversInOrderNormal.Select(w => w.def).ToList();
            var missing = BillTasks()
                .Where(t => t.workType != null && !pawn.WorkTypeIsDisabled(t.workType)
                            && pawn.workSettings.GetPriority(t.workType) > 0 && !reached.Contains(t))
                .Select(t => $"{t.defName} ({t.workType.defName}={pawn.workSettings.GetPriority(t.workType)})")
                .ToList();
            ctx.Assert(missing.Count == 0,
                $"'{nickname}' works these types but never goes through their bill tasks: {string.Join(", ", missing)}");
        }

        [When("a fuelled campfire with the Work Studio bill {string} stands next to {string}")]
        public void CampfireWithBill(PickleContext ctx, string recipeName, string nickname)
        {
            var pawn = Colonist(ctx, nickname);
            var map = pawn.Map;
            var recipe = DefDatabase<RecipeDef>.GetNamedSilentFail(recipeName);
            ctx.Require(recipe != null, $"no recipe is named '{recipeName}'");
            var campfireDef = DefDatabase<ThingDef>.GetNamedSilentFail("Campfire");
            ctx.Require(campfireDef != null, "no thing is named 'Campfire'");

            ctx.Require(CellFinder.TryFindRandomCellNear(pawn.Position, map, 8,
                    c => c.Standable(map) && c.GetEdifice(map) == null && c.GetFirstItem(map) == null && !c.Fogged(map),
                    out var cell),
                $"no free cell near '{nickname}' for a campfire");

            var campfire = (Building_WorkTable)ThingMaker.MakeThing(campfireDef);
            GenSpawn.Spawn(campfire, cell, map);
            campfire.SetFaction(Faction.OfPlayer);
            campfire.GetComp<CompRefuelable>()?.Refuel(100f);
            campfire.BillStack.AddBill(BillUtility.MakeNewBill(recipe));

            // A meal needs raw food on the map, or the option reads "missing materials" and the test
            // could not tell a lost option from a legitimately disabled one.
            var foodDef = DefDatabase<ThingDef>.GetNamedSilentFail("RawPotatoes");
            if (foodDef != null)
            {
                var food = ThingMaker.MakeThing(foodDef);
                food.stackCount = 40;
                GenPlace.TryPlaceThing(food, cell + IntVec3.East, map, ThingPlaceMode.Near);
            }

            ctx.Require(campfire.BillStack.Count == 1, $"the campfire holds {campfire.BillStack.Count} bills");
        }

        [Then("right-clicking the campfire offers {string} a Work Studio option about the bill {string}")]
        public void MenuOffersBillOption(PickleContext ctx, string nickname, string recipeName)
        {
            var pawn = Colonist(ctx, nickname);
            var campfire = pawn.Map.listerThings.AllThings.OfType<Building_WorkTable>()
                .FirstOrDefault(b => b.def.defName == "Campfire");
            ctx.Require(campfire != null, "no campfire stands on the map");

            var options = FloatMenuMakerMap.GetOptions(new List<Pawn> { pawn }, campfire.DrawPos, out _);
            // The building label is the stable part of the option text in every language.
            var ofCampfire = options.Where(o => o.Label != null && o.Label.IndexOf(campfire.LabelShort,
                StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            ctx.Assert(ofCampfire.Count > 0,
                $"right-clicking the campfire offers '{nickname}' nothing about it for the bill '{recipeName}'. " +
                $"The menu holds: {string.Join(" | ", options.Select(o => o.Label))}");
        }
    }
}
