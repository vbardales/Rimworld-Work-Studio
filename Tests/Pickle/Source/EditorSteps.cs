using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RimWorld;
using RimWorks.Pickle;
using Verse;

namespace WorkStudio.PickleSteps
{
    /// <summary>The editor: types, tasks, order, labels, visibility.</summary>
    [PickleSteps]
    public class EditorSteps
    {
        // ---------------------------------------------------------------- opening and looking

        // Every mutation step below opens the editor implicitly through Driver.Editor, because what
        // it needs is the object, not the window. The three steps here exist for the screenshot
        // scenarios, where the window being on screen, with the right thing selected, IS the test.
        // They wait frames rather than ticks: the editor does not pause the game, but the settings
        // window does, and a tick-based wait there would sit until its timeout.

        [When("I open the work type editor")]
        public async Task OpenEditor(PickleContext ctx)
        {
            Driver.Editor(ctx);
            await ctx.WaitFrames(2);
        }

        [When("I close the work type editor")]
        public async Task CloseEditor(PickleContext ctx)
        {
            Find.WindowStack.WindowOfType<Dialog_EditWorkType>()?.Close(doCloseSound: false);
            Find.WindowStack.WindowOfType<Dialog_WorkTypes>()?.Close(doCloseSound: false);
            await ctx.WaitFrames(2);
        }

        [When("I select the work type {string}")]
        public async Task SelectType(PickleContext ctx, string type)
        {
            Driver.Select(ctx, Driver.Editor(ctx), Driver.WorkType(ctx, type));
            await ctx.WaitFrames(2);
        }

        [When("I search the other tasks for {string}")]
        public async Task SearchOtherTasks(PickleContext ctx, string text)
        {
            Driver.Search(ctx, Driver.Editor(ctx), text);
            await ctx.WaitFrames(2);
        }

        // ---------------------------------------------------------------- mutations

        /// <summary>
        /// The editor's New button creates the type, then opens its sheet; the label typed there is
        /// what a player names it. Same two calls, and the sheet is closed instead of typed into.
        /// </summary>
        [When("I create the work type {string}")]
        public void Create(PickleContext ctx, string label)
        {
            var dialog = Driver.Editor(ctx);
            Driver.Select(ctx, dialog, null);
            var before = WorkStudioMod.Settings.customTypes.Select(e => e.id).ToList();

            Driver.Call(ctx, dialog, "CreateType");
            Find.WindowStack.WindowOfType<Dialog_EditWorkType>()?.Close(doCloseSound: false);

            var entry = WorkStudioMod.Settings.customTypes.FirstOrDefault(e => !before.Contains(e.id));
            ctx.Assert(entry != null, "the New button added no type to the settings");
            var def = DefDatabase<WorkTypeDef>.GetNamedSilentFail(entry.id);
            ctx.Assert(def != null, $"the type '{entry.id}' is in the settings but no WorkTypeDef was created");
            Driver.Call(ctx, dialog, "Rename", def, label);
        }

        [When("I move the task {string} into {string}")]
        public void MoveTask(PickleContext ctx, string giver, string type)
        {
            Driver.Call(ctx, Driver.Editor(ctx), "MoveTask", Driver.Task(ctx, giver), Driver.WorkType(ctx, type));
        }

        [When("I send the task {string} back to its original type")]
        public void MoveTaskBack(PickleContext ctx, string giver)
        {
            Driver.Call(ctx, Driver.Editor(ctx), "MoveTask", Driver.Task(ctx, giver), null);
        }

        [When("I delete the work type {string}")]
        public void Delete(PickleContext ctx, string type)
        {
            Driver.Call(ctx, Driver.Editor(ctx), "DeleteType", Driver.WorkType(ctx, type));
        }

        [When("I rename the work type {string} to {string}")]
        public void Rename(PickleContext ctx, string type, string label)
        {
            Driver.Call(ctx, Driver.Editor(ctx), "Rename", Driver.WorkType(ctx, type), label);
        }

        [When("I hide the work type {string}")]
        public void Hide(PickleContext ctx, string type)
        {
            Driver.Call(ctx, Driver.Editor(ctx), "SetVisible", Driver.WorkType(ctx, type), false);
        }

        [When("I show the work type {string}")]
        public void Show(PickleContext ctx, string type)
        {
            Driver.Call(ctx, Driver.Editor(ctx), "SetVisible", Driver.WorkType(ctx, type), true);
        }

        [When("I reset Work Studio's whole setup")]
        public void ResetAll(PickleContext ctx) => WorkTypeRuntime.ResetAll();

        /// <summary>
        /// Replays a drop through the callback the column registered on its last repaint. Rows are
        /// named rather than numbered, so a mod list that adds work types does not move the target;
        /// the step turns the name into ReorderableWidget's insertion index, counted before the
        /// dragged row is removed: above a row is that row's index, below it is the next one.
        /// </summary>
        [When("I drag the work type {string} {word} {string}")]
        public async Task DragType(PickleContext ctx, string moved, string where, string target)
        {
            var rows = Driver.TypesInOrder();
            var (from, insertion) = Indices(ctx, rows, Driver.WorkType(ctx, moved), Driver.WorkType(ctx, target), where);
            await Replay(ctx, null, "typeReorderGroup", from, insertion);
        }

        [When("I drag the task {string} of {string} {word} {string}")]
        public async Task DragTask(PickleContext ctx, string moved, string type, string where, string target)
        {
            var def = Driver.WorkType(ctx, type);
            var rows = def.workGiversByPriority.ToList();
            var (from, insertion) = Indices(ctx, rows, Driver.Task(ctx, moved), Driver.Task(ctx, target), where);
            await Replay(ctx, def, "taskReorderGroup", from, insertion);
        }

        private static (int, int) Indices<T>(PickleContext ctx, List<T> rows, T moved, T target, string where) where T : Def
        {
            ctx.Require(where == "above" || where == "below", $"write above or below, not '{where}'");
            var from = rows.IndexOf(moved);
            var to = rows.IndexOf(target);
            ctx.Require(from >= 0 && to >= 0, $"both rows must be in the list {Driver.Names(rows)}");
            return (from, where == "above" ? to : to + 1);
        }

        private static async Task Replay(PickleContext ctx, WorkTypeDef select, string groupField, int from, int insertion)
        {
            ReorderCapture.Ensure();
            var dialog = Driver.Editor(ctx);
            if (select != null)
            {
                Driver.Select(ctx, dialog, select);
            }

            await ctx.WaitFrames(3);

            var id = Driver.ReorderGroup(dialog, groupField);
            ctx.Require(id >= 0 && ReorderCapture.Actions.ContainsKey(id),
                $"the column registered no reorder callback ({groupField} = {id}): a drag could never start");
            ReorderCapture.Actions[id](from, insertion);
        }

        private sealed class PositionBefore
        {
            public List<WorkTypeDef> Rows;
        }

        /// <summary>
        /// An arrow is the one editor control a scenario drives with no repaint behind it, and the
        /// editor's row list is a cache <c>DoWindowContents</c> drops at the top of every pass —
        /// so a real click always acts on a list rebuilt that same frame, while this step, left to
        /// itself, would act on whatever the last pass built, possibly several scenarios ago. That
        /// is what turned "the arrows move a type one place" red on 2026-09-18: the arrow rewrote
        /// every priority from a list that predated the sandbox reset between scenarios, putting
        /// Research back where it had been rather than moving Cooking. A drag never had the problem,
        /// because <c>ReorderableWidget</c> only hands out its callback during a repaint, so those
        /// steps cannot run without one. Letting the window draw first gives the arrow the same
        /// footing.
        /// </summary>
        [When("I press the {word} arrow on the work type {string}")]
        public async Task TypeArrow(PickleContext ctx, string direction, string type)
        {
            Driver.Editor(ctx);
            await ctx.WaitFrames(2);

            ctx.Set(new PositionBefore { Rows = Driver.TypesInOrder() });
            Driver.Call(ctx, Driver.Editor(ctx), "ShiftType", Driver.WorkType(ctx, type), Delta(ctx, direction));
        }

        /// <summary>The whole list is compared: one row swapped with its neighbour, everything else in place.</summary>
        [Then("the work type {string} moved {int} place(s) {word}")]
        public void Moved(PickleContext ctx, string type, int places, string direction)
        {
            var def = Driver.WorkType(ctx, type);
            var expected = ctx.Get<PositionBefore>().Rows.ToList();
            var from = expected.IndexOf(def);
            var to = from + (direction == "down" ? places : -places);
            ctx.Require(to >= 0 && to < expected.Count, $"{Driver.Describe(def)} has no room to move {places} {direction}");
            expected.RemoveAt(from);
            expected.Insert(to, def);
            var actual = Driver.TypesInOrder();

            // The order comes out of naturalPriority, which the editor rewrites for every row at
            // once: when a row lands somewhere unexpected, those numbers say whether the editor
            // wrote them wrong or something else reordered the list afterwards.
            ctx.Assert(actual.SequenceEqual(expected),
                $"expected {Driver.Names(expected)}; the game orders {Driver.Names(actual)}. naturalPriority now: " +
                string.Join(", ", actual.Select(t => $"{t.defName}={t.naturalPriority}")) +
                ". Overrides written by the mod: " +
                string.Join(", ", WorkStudioMod.Settings.priorityOverrides.Select(p => $"{p.Key}={p.Value}")));
        }

        [When("I press the {word} arrow on the task {string} of {string}")]
        public async Task TaskArrow(PickleContext ctx, string direction, string giver, string type)
        {
            var dialog = Driver.Editor(ctx);
            Driver.Select(ctx, dialog, Driver.WorkType(ctx, type));
            await ctx.WaitFrames(2); // same reason as TypeArrow above
            Driver.Call(ctx, dialog, "ShiftTask", Driver.Task(ctx, giver), Delta(ctx, direction));
        }

        [When("I press the up arrow on the first work type")]
        public async Task FirstTypeUp(PickleContext ctx)
        {
            Driver.Editor(ctx);
            await ctx.WaitFrames(2); // same reason as TypeArrow above
            Driver.Call(ctx, Driver.Editor(ctx), "ShiftType", Driver.TypesInOrder().First(), -1);
        }

        [When("I press the down arrow on the last task of {string}")]
        public async Task LastTaskDown(PickleContext ctx, string type)
        {
            var dialog = Driver.Editor(ctx);
            var def = Driver.WorkType(ctx, type);
            Driver.Select(ctx, dialog, def);
            await ctx.WaitFrames(2); // same reason as TypeArrow above
            Driver.Call(ctx, dialog, "ShiftTask", def.workGiversByPriority.Last(), 1);
        }

        private static int Delta(PickleContext ctx, string direction)
        {
            ctx.Require(direction == "up" || direction == "down", $"write up or down, not '{direction}'");
            return direction == "up" ? -1 : 1;
        }

        // ---------------------------------------------------------------- checks

        [Then("a work type labelled {string} exists")]
        public void Exists(PickleContext ctx, string label)
        {
            var def = Driver.WorkType(ctx, label);
            ctx.Assert(DefDatabase<WorkTypeDef>.AllDefsListForReading.Contains(def), $"'{label}' is not in the def database");
        }

        [Then("no work type labelled {string} exists")]
        public void Gone(PickleContext ctx, string label)
        {
            var left = DefDatabase<WorkTypeDef>.AllDefsListForReading.Where(t => t.label == label).ToList();
            ctx.Assert(left.Count == 0, $"still in the def database: {Driver.Names(left)}");
        }

        [Then("the Work tab has a column for {string}")]
        public void HasColumn(PickleContext ctx, string type)
        {
            var def = Driver.WorkType(ctx, type);
            var columns = Driver.WorkColumns();
            ctx.Assert(columns.Any(c => c.workType == def),
                $"no Work tab column for {Driver.Describe(def)}; columns: {string.Join(", ", columns.Select(c => c.workType.defName))}");
        }

        [Then("the Work tab has no column for {string}")]
        public void HasNoColumn(PickleContext ctx, string type)
        {
            var def = Driver.WorkType(ctx, type);
            ctx.Assert(Driver.WorkColumns().All(c => c.workType != def), $"{Driver.Describe(def)} still has a column");
        }

        [Then("the task {string} belongs to {string}")]
        public void BelongsTo(PickleContext ctx, string giver, string type)
        {
            var task = Driver.Task(ctx, giver);
            var def = Driver.WorkType(ctx, type);
            ctx.Assert(task.workType == def && def.workGiversByPriority.Contains(task),
                $"'{giver}' should be in {Driver.Describe(def)}; its workType is '{task.workType?.defName}', " +
                $"and that type lists {Driver.Names(def.workGiversByPriority)}");
        }

        [Then("{string} no longer lists the task {string}")]
        public void NotListed(PickleContext ctx, string type, string giver)
        {
            var def = Driver.WorkType(ctx, type);
            var task = Driver.Task(ctx, giver);
            ctx.Assert(!def.workGiversByPriority.Contains(task), $"{Driver.Describe(def)} still lists '{giver}'");
            ctx.Assert(def.workGiversByPriority.Distinct().Count() == def.workGiversByPriority.Count,
                $"{Driver.Describe(def)} lists a task twice: {Driver.Names(def.workGiversByPriority)}");
        }

        private sealed class RememberedCount
        {
            public Dictionary<string, int> Counts;
        }

        [When("I remember how many tasks each work type holds")]
        public void RememberCounts(PickleContext ctx)
        {
            ctx.Set(new RememberedCount
            {
                Counts = DefDatabase<WorkTypeDef>.AllDefsListForReading.ToDictionary(t => t.defName, t => t.workGiversByPriority.Count)
            });
        }

        [Then("{string} holds {int} task(s) fewer than before")]
        public void Fewer(PickleContext ctx, string type, int fewer)
        {
            var def = Driver.WorkType(ctx, type);
            var before = ctx.Get<RememberedCount>().Counts[def.defName];
            ctx.Assert(def.workGiversByPriority.Count == before - fewer,
                $"{Driver.Describe(def)} held {before} tasks and should hold {before - fewer}; it holds {def.workGiversByPriority.Count}");
        }

        [Then("the work types come in the order {string}")]
        public void TypeOrder(PickleContext ctx, string commaSeparated)
        {
            // Only the named types are compared, in the order they appear among all of them.
            var wanted = commaSeparated.Split(',').Select(s => Driver.WorkType(ctx, s.Trim())).ToList();
            var actual = Driver.TypesInOrder().Where(wanted.Contains).ToList();
            ctx.Assert(actual.SequenceEqual(wanted),
                $"expected {Driver.Names(wanted)}; the game orders them {Driver.Names(actual)}");

            var columns = Driver.WorkColumns().Select(c => c.workType).Where(wanted.Contains).ToList();
            ctx.Assert(columns.SequenceEqual(wanted),
                $"the Work tab columns disagree with the editor: {Driver.Names(columns)}");
        }

        /// <summary>Adjacency, not just relative order: a drop one row too far still keeps A before B.</summary>
        [Then("the work type {string} comes right {word} {string}")]
        public void TypeAdjacent(PickleContext ctx, string moved, string where, string anchor)
        {
            var rows = Driver.TypesInOrder();
            AssertAdjacent(ctx, rows, Driver.WorkType(ctx, moved), Driver.WorkType(ctx, anchor), where, "the editor and the game");
            var columns = Driver.WorkColumns().Select(c => c.workType).ToList();
            var hidden = rows.Where(t => !t.visible).ToList();
            ctx.Assert(columns.SequenceEqual(rows.Except(hidden)),
                $"the Work tab columns {Driver.Names(columns)} disagree with the type order {Driver.Names(rows.Except(hidden))}");
        }

        [Then("the task {string} of {string} comes right {word} {string}")]
        public void TaskAdjacent(PickleContext ctx, string moved, string type, string where, string anchor)
        {
            var def = Driver.WorkType(ctx, type);
            AssertAdjacent(ctx, def.workGiversByPriority.ToList(), Driver.Task(ctx, moved), Driver.Task(ctx, anchor), where,
                Driver.Describe(def));
        }

        private static void AssertAdjacent<T>(PickleContext ctx, List<T> rows, T moved, T anchor, string where, string owner) where T : Def
        {
            ctx.Require(where == "after" || where == "before", $"write after or before, not '{where}'");
            var expected = rows.IndexOf(anchor) + (where == "after" ? 1 : -1);
            ctx.Assert(rows.IndexOf(moved) == expected,
                $"'{moved.defName}' should come right {where} '{anchor.defName}'; {owner} order is {Driver.Names(rows)}");
        }

        [Then("the work type {string} is first")]
        public void First(PickleContext ctx, string type)
        {
            var rows = Driver.TypesInOrder();
            ctx.Assert(rows[0] == Driver.WorkType(ctx, type), $"the first type is '{rows[0].defName}'; order: {Driver.Names(rows)}");
        }

        [Given("the work types start in the order {string}")]
        public void TypeOrderPrecondition(PickleContext ctx, string commaSeparated)
        {
            var wanted = commaSeparated.Split(',').Select(s => Driver.WorkType(ctx, s.Trim())).ToList();
            var all = Driver.TypesInOrder();
            ctx.Require(all.Take(wanted.Count).SequenceEqual(wanted),
                $"this scenario counts rows from the top and needs {Driver.Names(wanted)} there; this mod list starts {Driver.Names(all.Take(wanted.Count))}");
        }

        [Then("the tasks of {string} come in the order {string}")]
        public void TaskOrder(PickleContext ctx, string type, string commaSeparated)
        {
            var def = Driver.WorkType(ctx, type);
            var wanted = commaSeparated.Split(',').Select(s => s.Trim()).ToList();
            var actual = def.workGiversByPriority.Select(g => g.defName).Where(wanted.Contains).ToList();
            ctx.Assert(actual.SequenceEqual(wanted),
                $"expected [{string.Join(", ", wanted)}]; {Driver.Describe(def)} orders them [{string.Join(", ", actual)}]");
        }

        /// <summary>
        /// Only the relative order of the named tasks: the drags find their rows by name, so tasks a
        /// mod list slips in between change nothing. What matters is that a drag "below" really
        /// moves a row downwards.
        /// </summary>
        [Given("the tasks of {string} include {string} in that order")]
        public void TaskOrderPrecondition(PickleContext ctx, string type, string commaSeparated)
        {
            var def = Driver.WorkType(ctx, type);
            var wanted = commaSeparated.Split(',').Select(s => s.Trim()).ToList();
            var actual = def.workGiversByPriority.Select(g => g.defName).Where(wanted.Contains).ToList();
            ctx.Require(actual.SequenceEqual(wanted),
                $"this scenario needs [{string.Join(", ", wanted)}] in that order; {Driver.Describe(def)} orders them " +
                $"[{string.Join(", ", actual)}] among [{string.Join(", ", def.workGiversByPriority.Select(g => g.defName))}]");
        }

        [Then("the Work tab column of {string} is headed {string} and measured afresh")]
        public void Header(PickleContext ctx, string type, string label)
        {
            var def = Driver.WorkType(ctx, type);
            ctx.Assert(def.labelShort == label, $"the header draws labelShort, which is '{def.labelShort}', not '{label}'");
            var column = Driver.WorkColumns().FirstOrDefault(c => c.workType == def);
            ctx.Require(column != null, $"{Driver.Describe(def)} has no column");
            var worker = typeof(PawnColumnDef).GetField("workerInt", System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public).GetValue(column);
            ctx.Assert(worker == null,
                "the column still holds the worker that measured the old label: its width will not follow the new name");
        }
    }
}
