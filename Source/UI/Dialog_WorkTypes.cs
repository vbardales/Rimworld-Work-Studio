using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace WorkStudio
{
    /// <summary>
    /// Work type editor: three columns - the types in their real order, the tasks of the selected
    /// type, and what to add to it. Every change is applied immediately. The window is neither
    /// modal nor blocking, so the Work tab can be seen rearranging itself live behind it.
    /// </summary>
    public class Dialog_WorkTypes : Window
    {
        private const float RowHeight = 30f;
        private const float ColumnGap = 12f;
        private const float TypeColumnWidth = 320f;
        private const float AddColumnWidth = 340f;
        private const float HeaderHeight = 28f;
        private const float SearchHeight = 28f;
        private const float ButtonHeight = 30f;
        private const float SmallButtonSize = 22f;
        private const float ArrowSize = 20f;
        private const float ArrowsWidth = ArrowSize * 2f + 2f;
        private const int MaxAddResults = 200;

        /// <summary>Ceiling that <c>WorkTypeDef.ConfigErrors</c> accepts for a natural priority.</summary>
        private const int MaxNaturalPriority = 10000;

        private static readonly Color DisabledArrowColor = new Color(1f, 1f, 1f, 0.25f);

        private WorkTypeDef selectedType;
        private string addSearch = "";

        private Vector2 typeScroll;
        private Vector2 taskScroll;
        private Vector2 addScroll;

        /// <summary>
        /// Drag-and-drop group identifiers. They must survive from one event pass to the next:
        /// <see cref="ReorderableWidget.NewGroup"/> only returns an identifier during
        /// <c>Repaint</c> and returns -1 everywhere else. Keeping them in a local variable would
        /// therefore pass -1 to <see cref="ReorderableWidget.Reorderable"/> on <c>MouseDown</c>, and
        /// the drag would never start - while the click would keep working.
        /// </summary>
        private int typeReorderGroup = -1;
        private int taskReorderGroup = -1;

        private List<WorkTypeDef> typesCache;

        public override Vector2 InitialSize => new Vector2(1180f, 760f);

        public Dialog_WorkTypes()
        {
            doCloseX = true;
            draggable = true;
            resizeable = true;
            forcePause = false;
            absorbInputAroundWindow = false;
            closeOnClickedOutside = false;
        }

        private static WorkStudioSettings Settings => WorkStudioMod.Settings;

        // ---------------------------------------------------------------- data

        /// <summary>The types in the order they really appear, from highest to lowest priority.</summary>
        private List<WorkTypeDef> Types =>
            typesCache ??= WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder.ToList();

        private void InvalidateCaches()
        {
            typesCache = null;
        }

        private static string TypeLabel(WorkTypeDef type)
        {
            return type.label.NullOrEmpty() ? type.defName : type.LabelCap.ToString();
        }

        private static string GiverLabel(WorkGiverDef giver)
        {
            return giver.label.NullOrEmpty() ? giver.defName : giver.LabelCap.ToString();
        }

        private static List<WorkGiverDef> TasksOf(WorkTypeDef type)
        {
            return type == null ? new List<WorkGiverDef>() : type.workGiversByPriority;
        }

        private static bool IsHidden(WorkTypeDef type) => Settings.hiddenTypes.Contains(type.defName);

        private static bool IsMoved(WorkGiverDef giver)
        {
            return giver.workType != WorkTypeRuntime.OriginalTypeOf(giver);
        }

        // ---------------------------------------------------------------- mutations

        private void Commit()
        {
            WorkStudioMod.Instance.WriteSettings();
            WorkTypeRuntime.Apply();
            InvalidateCaches();
        }

        private void MoveTask(WorkGiverDef giver, WorkTypeDef target)
        {
            if (target == null || target == WorkTypeRuntime.OriginalTypeOf(giver))
            {
                Settings.giverAssignments.Remove(giver.defName);
            }
            else
            {
                Settings.giverAssignments[giver.defName] = target.defName;
            }

            // A task's rank only makes sense within a given list. When it changes type it gets its
            // original priority back, which places it correctly among the vanilla tasks, rather
            // than a value tuned for a list it has just left.
            Settings.giverOrderOverrides.Remove(giver.defName);

            Commit();
        }

        private void SetVisible(WorkTypeDef type, bool visible)
        {
            if (visible)
            {
                Settings.hiddenTypes.Remove(type.defName);
            }
            else if (!Settings.hiddenTypes.Contains(type.defName))
            {
                Settings.hiddenTypes.Add(type.defName);
            }

            Commit();
        }

        private void Rename(WorkTypeDef type, string label)
        {
            if (WorkTypeRuntime.IsCustom(type))
            {
                var entry = WorkTypeRuntime.EntryOf(type.defName);
                if (entry != null)
                {
                    entry.label = label;
                }
            }
            else if (label.NullOrEmpty())
            {
                Settings.labelOverrides.Remove(type.defName);
            }
            else
            {
                Settings.labelOverrides[type.defName] = label;
            }

            Commit();
        }

        /// <summary>
        /// Spreads over a reordered list the priority values it already carried.
        /// <para>
        /// Those values are reused rather than inventing a scale: the game and mods work with
        /// precise orders of magnitude - 0 to 1400 for work types - and a mod loaded later must be
        /// able to slot into the middle of the list rather than end up relegated to the tail. Ties
        /// are pulled apart by one unit, otherwise the relative order of two neighbours would stay
        /// undefined.
        /// </para>
        /// </summary>
        private static List<int> Redistribute(IEnumerable<int> current)
        {
            var values = current.OrderByDescending(v => v).ToList();

            // Zero or one value: nothing to break ties on, and the two loops below do not run.
            // Return as is rather than leaving it to the guards.
            if (values.Count < 2)
            {
                return values;
            }

            for (var i = 1; i < values.Count; i++)
            {
                if (values[i] >= values[i - 1])
                {
                    values[i] = values[i - 1] - 1;
                }
            }

            // Pulling ties apart pushes the tail of the list down: if types all shared the same
            // priority, it dips below zero. Shift the whole scale back up.
            var lowest = values[values.Count - 1];
            if (lowest < 0)
            {
                for (var i = 0; i < values.Count; i++)
                {
                    values[i] -= lowest;
                }
            }

            // And if the top overflows, fall back to an even scale. The bound is not decorative:
            // Pawn_WorkSettings.CacheWorkGiversInOrder sorts work on
            // naturalPriority + (4 - colonist priority) * 100000. A large enough natural priority
            // would therefore jump over a whole priority band, and work set to 4 would be done
            // before work set to 1.
            if (values[0] > MaxNaturalPriority)
            {
                for (var i = 0; i < values.Count; i++)
                {
                    values[i] = Mathf.Max(MaxNaturalPriority - i, 0);
                }
            }

            return values;
        }

        /// <summary>
        /// Moves an item following the <see cref="ReorderableWidget"/> convention: <paramref
        /// name="to"/> is the insertion position <b>before</b> removal, not the final position.
        /// Removing first would shift the target by one for any downward move.
        /// </summary>
        private static bool DragMove<T>(List<T> list, int from, int to)
        {
            if (from < 0 || from >= list.Count || to < 0 || to > list.Count || from == to)
            {
                return false;
            }

            list.Insert(to, list[from]);
            list.RemoveAt(from < to ? from : from + 1);
            return true;
        }

        private static bool Swap<T>(List<T> list, int a, int b)
        {
            if (a < 0 || b < 0 || a >= list.Count || b >= list.Count || a == b)
            {
                return false;
            }

            var held = list[a];
            list[a] = list[b];
            list[b] = held;
            return true;
        }

        private void ApplyTypeOrder(List<WorkTypeDef> ordered)
        {
            var values = Redistribute(ordered.Select(t => t.naturalPriority));
            for (var i = 0; i < ordered.Count; i++)
            {
                Settings.priorityOverrides[ordered[i].defName] = values[i];
            }

            Commit();
        }

        private void ApplyTaskOrder(List<WorkGiverDef> ordered)
        {
            var values = Redistribute(ordered.Select(g => g.priorityInType));
            for (var i = 0; i < ordered.Count; i++)
            {
                Settings.giverOrderOverrides[ordered[i].defName] = values[i];
            }

            Commit();
        }

        /// <summary>Reorders the work types by reassigning their natural priorities.</summary>
        private void ReorderTypes(int from, int to)
        {
            var ordered = Types.ToList();
            if (DragMove(ordered, from, to))
            {
                ApplyTypeOrder(ordered);
            }
        }

        /// <summary>
        /// Reorders the tasks inside the selected type, by reassigning their
        /// <see cref="WorkGiverDef.priorityInType"/>: this is the order in which a colonist grabs
        /// them once they have taken up that work.
        /// </summary>
        private void ReorderTasks(int from, int to)
        {
            if (selectedType == null)
            {
                return;
            }

            var ordered = TasksOf(selectedType).ToList();
            if (DragMove(ordered, from, to))
            {
                ApplyTaskOrder(ordered);
            }
        }

        /// <summary>
        /// Moves a type by one step. The arrows back up drag-and-drop, which is not practical on a
        /// Steam Deck trackpad.
        /// </summary>
        private void ShiftType(WorkTypeDef type, int delta)
        {
            var ordered = Types.ToList();
            var index = ordered.IndexOf(type);
            if (Swap(ordered, index, index + delta))
            {
                ApplyTypeOrder(ordered);
            }
        }

        private void ShiftTask(WorkGiverDef giver, int delta)
        {
            if (selectedType == null)
            {
                return;
            }

            var ordered = TasksOf(selectedType).ToList();
            var index = ordered.IndexOf(giver);
            if (Swap(ordered, index, index + delta))
            {
                ApplyTaskOrder(ordered);
            }
        }

        /// <summary>
        /// Draws the two move arrows and reports the requested step, 0 if nothing is clicked. The
        /// ends of the list stay clickable but greyed out: a button that disappears moves all the
        /// others under the finger.
        /// </summary>
        private static int DrawArrows(Rect rect, bool canGoUp, bool canGoDown)
        {
            var up = new Rect(rect.x, rect.y + (rect.height - ArrowSize) / 2f, ArrowSize, ArrowSize);
            var down = new Rect(up.xMax + 2f, up.y, ArrowSize, ArrowSize);
            var delta = 0;

            if (DrawArrow(up, TexButton.ReorderUp, canGoUp, "WorkStudio.MoveUp"))
            {
                delta = -1;
            }

            if (DrawArrow(down, TexButton.ReorderDown, canGoDown, "WorkStudio.MoveDown"))
            {
                delta = 1;
            }

            return delta;
        }

        /// <summary>
        /// An inactive arrow is drawn faded but sets <b>no clickable area</b>: filtering it out
        /// afterwards would leave an invisible button swallowing the click and playing its sound at
        /// the ends of the list.
        /// </summary>
        private static bool DrawArrow(Rect rect, Texture2D texture, bool enabled, string tooltipKey)
        {
            if (!enabled)
            {
                var color = GUI.color;
                GUI.color = DisabledArrowColor;
                GUI.DrawTexture(rect, texture);
                GUI.color = color;
                return false;
            }

            return Widgets.ButtonImage(rect, texture, Color.white, GenUI.MouseoverColor, true,
                tooltipKey.Translate());
        }

        /// <summary>Does the type have tasks whose order has been adjusted?</summary>
        private static bool HasTaskOrder(WorkTypeDef type)
        {
            return TasksOf(type).Any(g => Settings.giverOrderOverrides.ContainsKey(g.defName));
        }

        private void ResetTaskOrder(WorkTypeDef type)
        {
            var removed = false;
            foreach (var giver in TasksOf(type).ToList())
            {
                removed |= Settings.giverOrderOverrides.Remove(giver.defName);
            }

            if (removed)
            {
                Commit();
            }
        }

        private void CreateType()
        {
            var entry = new CustomWorkTypeEntry(NewTypeId(), "WorkStudio.NewTypeLabel".Translate());
            Settings.customTypes.Add(entry);

            // A new type lands just below the selected type: it is almost always the one tasks are
            // about to be taken from. If that neighbour is already at the floor, land level with it
            // rather than in the negatives - it is up to the user to settle it.
            var anchor = selectedType?.naturalPriority ?? WorkTypeRuntime.DefaultCustomPriority + 1;
            Settings.priorityOverrides[entry.id] = Mathf.Max(anchor - 1, 0);

            Commit();

            selectedType = DefDatabase<WorkTypeDef>.GetNamedSilentFail(entry.id);
            Find.WindowStack.Add(new Dialog_EditWorkType(entry, Commit));
        }

        private static string NewTypeId()
        {
            var index = 1;
            string id;
            do
            {
                id = "WorkStudio_Type" + index;
                index++;
            }
            while (DefDatabase<WorkTypeDef>.GetNamedSilentFail(id) != null);

            return id;
        }

        private void DeleteType(WorkTypeDef type)
        {
            var entry = WorkTypeRuntime.EntryOf(type.defName);
            if (entry == null)
            {
                return;
            }

            // The deleted type's tasks need a home again: send them back to their origin. Their
            // order goes with the type: it was tuned for that particular list, and keeping it would
            // reorder the receiving type without anyone asking.
            foreach (var key in Settings.giverAssignments
                         .Where(pair => pair.Value == type.defName)
                         .Select(pair => pair.Key)
                         .ToList())
            {
                Settings.giverAssignments.Remove(key);
                Settings.giverOrderOverrides.Remove(key);
            }

            Settings.customTypes.Remove(entry);
            Settings.priorityOverrides.Remove(type.defName);
            Settings.labelOverrides.Remove(type.defName);
            Settings.hiddenTypes.Remove(type.defName);

            if (selectedType == type)
            {
                selectedType = null;
            }

            Commit();
        }

        /// <summary>Puts every task of this type back where the game had put it.</summary>
        private void ResetTasks(WorkTypeDef type)
        {
            foreach (var giver in DefDatabase<WorkGiverDef>.AllDefsListForReading)
            {
                if (giver.workType == type || WorkTypeRuntime.OriginalTypeOf(giver) == type)
                {
                    Settings.giverAssignments.Remove(giver.defName);
                    Settings.giverOrderOverrides.Remove(giver.defName);
                }
            }

            Commit();
        }

        private void ResetOrder()
        {
            Settings.priorityOverrides.Clear();
            Commit();
        }

        // ---------------------------------------------------------------- rendering

        public override void DoWindowContents(Rect inRect)
        {
            // The list is rebuilt on every pass: defs can move under our feet (reset from the
            // settings window, mod reloaded). It stays stable within a pass, which drag-and-drop
            // needs for its indices to line up.
            InvalidateCaches();

            // The selected type may have disappeared in the meantime.
            if (selectedType != null && !Types.Contains(selectedType))
            {
                selectedType = null;
            }

            var taskWidth = inRect.width - TypeColumnWidth - AddColumnWidth - ColumnGap * 2f;

            var typeRect = new Rect(inRect.x, inRect.y, TypeColumnWidth, inRect.height);
            var taskRect = new Rect(typeRect.xMax + ColumnGap, inRect.y, taskWidth, inRect.height);
            var addRect = new Rect(taskRect.xMax + ColumnGap, inRect.y, AddColumnWidth, inRect.height);

            DrawTypeColumn(typeRect);
            DrawTaskColumn(taskRect);
            DrawAddColumn(addRect);
        }

        private static void DrawColumnHeader(Rect rect, string label)
        {
            var font = Text.Font;
            Text.Font = GameFont.Small;
            Widgets.Label(rect, label);
            Text.Font = font;
        }

        private void DrawTypeColumn(Rect rect)
        {
            Widgets.DrawMenuSection(rect);
            var inner = rect.ContractedBy(6f);
            var y = inner.y;

            DrawColumnHeader(new Rect(inner.x, y, inner.width, HeaderHeight), "WorkStudio.Types".Translate());
            y += HeaderHeight + 4f;

            // The column footer stacks up from the bottom: the list takes what is left.
            var footerY = inner.yMax - ButtonHeight;
            var newRow = new Rect(inner.x, footerY, inner.width, ButtonHeight);
            footerY -= 6f;

            Rect? resetOrderRow = null;
            if (Settings.priorityOverrides.Count > 0)
            {
                footerY -= ButtonHeight;
                resetOrderRow = new Rect(inner.x, footerY, inner.width, ButtonHeight);
                footerY -= 6f;
            }

            var types = Types;
            var listRect = new Rect(inner.x, y, inner.width, Mathf.Max(RowHeight, footerY - y));
            var viewRect = new Rect(0f, 0f, listRect.width - 16f, types.Count * RowHeight);

            Widgets.BeginScrollView(listRect, ref typeScroll, viewRect);

            // The group is declared inside the scroll area: NewGroup anchors its rectangle on the
            // current GUI origin, so the screen coordinates follow the scrolling.
            if (Event.current.type == EventType.Repaint)
            {
                typeReorderGroup = ReorderableWidget.NewGroup(
                    ReorderTypes,
                    ReorderableDirection.Vertical,
                    listRect);
            }

            for (var i = 0; i < types.Count; i++)
            {
                var row = new Rect(0f, i * RowHeight, viewRect.width, RowHeight);

                ReorderableWidget.Reorderable(typeReorderGroup, row);
                DrawTypeRow(row, types[i], i, types.Count);
            }

            Widgets.EndScrollView();

            if (resetOrderRow.HasValue &&
                Widgets.ButtonText(resetOrderRow.Value, "WorkStudio.ResetOrder".Translate()))
            {
                ResetOrder();
            }

            if (Widgets.ButtonText(newRow, "WorkStudio.NewType".Translate()))
            {
                CreateType();
            }
        }

        private void DrawTypeRow(Rect row, WorkTypeDef type, int index, int count)
        {
            if (selectedType == type)
            {
                Widgets.DrawHighlightSelected(row);
            }
            else if (Mouse.IsOver(row))
            {
                Widgets.DrawHighlight(row);
            }

            var delta = DrawArrows(new Rect(row.x + 2f, row.y, ArrowsWidth, row.height),
                index > 0, index < count - 1);
            if (delta != 0 && !ReorderableWidget.Dragging)
            {
                ShiftType(type, delta);
                return;
            }

            var visible = !IsHidden(type);
            var wasVisible = visible;
            var checkRect = new Rect(row.x + ArrowsWidth + 6f, row.y + 5f, 20f, 20f);
            Widgets.Checkbox(checkRect.x, checkRect.y, ref visible, 20f);
            TooltipHandler.TipRegion(checkRect, "WorkStudio.VisibleTip".Translate());

            // A drag that ends on a row must not count as a click: without this guard, releasing
            // over a checkbox toggles it along the way.
            if (visible != wasVisible && !ReorderableWidget.Dragging)
            {
                SetVisible(type, visible);
                return;
            }

            var countRect = new Rect(row.xMax - 34f, row.y, 30f, row.height);
            var labelRect = new Rect(checkRect.xMax + 6f, row.y, countRect.x - checkRect.xMax - 10f, row.height);

            var anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;

            if (IsHidden(type))
            {
                GUI.color = new Color(1f, 1f, 1f, 0.5f);
            }

            var label = TypeLabel(type);
            if (WorkTypeRuntime.IsCustom(type))
            {
                label += " *";
            }
            Widgets.Label(labelRect, label.Truncate(labelRect.width));

            GUI.color = new Color(1f, 1f, 1f, 0.5f);
            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(countRect, TasksOf(type).Count.ToString());
            GUI.color = Color.white;
            Text.Anchor = anchor;

            var tip = type.defName + "\n" + "WorkStudio.PriorityTip".Translate(type.naturalPriority);
            if (WorkTypeRuntime.IsCustom(type))
            {
                tip += "\n" + "WorkStudio.CustomTip".Translate();
            }
            TooltipHandler.TipRegion(row, tip);

            if (Widgets.ButtonInvisible(labelRect) && !ReorderableWidget.Dragging)
            {
                selectedType = type;
                addSearch = "";
            }
        }

        private void DrawTaskColumn(Rect rect)
        {
            Widgets.DrawMenuSection(rect);
            var inner = rect.ContractedBy(6f);
            var y = inner.y;

            var header = selectedType == null
                ? "WorkStudio.Tasks".Translate()
                : "WorkStudio.TasksOf".Translate(TypeLabel(selectedType));
            DrawColumnHeader(new Rect(inner.x, y, inner.width, HeaderHeight), header);
            y += HeaderHeight + 4f;

            if (selectedType == null)
            {
                GUI.color = new Color(1f, 1f, 1f, 0.5f);
                Widgets.Label(new Rect(inner.x, y, inner.width, 60f), "WorkStudio.NoSelection".Translate());
                GUI.color = Color.white;
                return;
            }

            var type = selectedType;
            var custom = WorkTypeRuntime.IsCustom(type);

            var footerY = inner.yMax - ButtonHeight;
            var manageRow = new Rect(inner.x, footerY, inner.width, ButtonHeight);
            footerY -= 6f;

            Rect? resetOrderRow = null;
            if (HasTaskOrder(type))
            {
                footerY -= ButtonHeight;
                resetOrderRow = new Rect(inner.x, footerY, inner.width, ButtonHeight);
                footerY -= 6f;
            }

            var tasks = TasksOf(type).ToList();
            var listRect = new Rect(inner.x, y, inner.width, Mathf.Max(RowHeight, footerY - y));
            var viewRect = new Rect(0f, 0f, listRect.width - 16f, Mathf.Max(tasks.Count, 1) * RowHeight);

            Widgets.BeginScrollView(listRect, ref taskScroll, viewRect);

            if (tasks.Count == 0)
            {
                GUI.color = new Color(1f, 1f, 1f, 0.5f);
                Widgets.Label(new Rect(0f, 0f, viewRect.width, RowHeight), "WorkStudio.NoTasks".Translate());
                GUI.color = Color.white;
            }

            // Dragging a task changes its priority within the type: the order shown is already the
            // real order, since the list comes from workGiversByPriority.
            if (Event.current.type == EventType.Repaint)
            {
                taskReorderGroup = ReorderableWidget.NewGroup(
                    ReorderTasks,
                    ReorderableDirection.Vertical,
                    listRect);
            }

            var ambiguous = AmbiguousLabels(tasks);

            for (var i = 0; i < tasks.Count; i++)
            {
                var row = new Rect(0f, i * RowHeight, viewRect.width, RowHeight);

                ReorderableWidget.Reorderable(taskReorderGroup, row);
                DrawTaskRow(row, tasks[i], i, tasks.Count, ambiguous);
            }

            Widgets.EndScrollView();

            if (resetOrderRow.HasValue &&
                Widgets.ButtonText(resetOrderRow.Value, "WorkStudio.ResetTaskOrder".Translate()))
            {
                ResetTaskOrder(type);
            }

            DrawManageRow(manageRow, type, custom);
        }

        private void DrawTaskRow(Rect row, WorkGiverDef giver, int index, int count,
            HashSet<string> ambiguous)
        {
            if (Mouse.IsOver(row))
            {
                Widgets.DrawHighlight(row);
            }

            var delta = DrawArrows(new Rect(row.x + 2f, row.y, ArrowsWidth, row.height),
                index > 0, index < count - 1);
            if (delta != 0 && !ReorderableWidget.Dragging)
            {
                ShiftTask(giver, delta);
                return;
            }

            var moveRect = new Rect(row.xMax - SmallButtonSize - 2f, row.y + 4f, SmallButtonSize, SmallButtonSize);
            var labelRect = new Rect(row.x + ArrowsWidth + 6f, row.y, moveRect.x - row.x - ArrowsWidth - 12f,
                row.height);

            DrawGiverLabel(labelRect, giver, showOrigin: true, ambiguous: ambiguous);

            TooltipHandler.TipRegion(moveRect, "WorkStudio.MoveTip".Translate());
            if (Widgets.ButtonText(moveRect, ">") && !ReorderableWidget.Dragging)
            {
                OpenMoveMenu(giver);
            }
        }

        private void DrawManageRow(Rect row, WorkTypeDef type, bool custom)
        {
            var slots = custom ? 3 : 2;
            var width = (row.width - 6f * (slots - 1)) / slots;
            var x = row.x;

            if (Widgets.ButtonText(new Rect(x, row.y, width, ButtonHeight), "WorkStudio.Rename".Translate()))
            {
                Find.WindowStack.Add(new Dialog_TextEntry(
                    "WorkStudio.RenameTitle".Translate(),
                    TypeLabel(type),
                    label => Rename(type, label)));
            }
            x += width + 6f;

            if (custom)
            {
                if (Widgets.ButtonText(new Rect(x, row.y, width, ButtonHeight), "WorkStudio.Edit".Translate()))
                {
                    var entry = WorkTypeRuntime.EntryOf(type.defName);
                    if (entry != null)
                    {
                        Find.WindowStack.Add(new Dialog_EditWorkType(entry, Commit));
                    }
                }
                x += width + 6f;

                if (Widgets.ButtonText(new Rect(x, row.y, width, ButtonHeight), "WorkStudio.Delete".Translate()))
                {
                    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                        "WorkStudio.ConfirmDelete".Translate(TypeLabel(type)),
                        () => DeleteType(type),
                        destructive: true));
                }
            }
            else if (Widgets.ButtonText(new Rect(x, row.y, width, ButtonHeight), "WorkStudio.ResetTasks".Translate()))
            {
                ResetTasks(type);
            }
        }

        private void DrawAddColumn(Rect rect)
        {
            Widgets.DrawMenuSection(rect);
            var inner = rect.ContractedBy(6f);
            var y = inner.y;

            DrawColumnHeader(new Rect(inner.x, y, inner.width, HeaderHeight), "WorkStudio.OtherTasks".Translate());
            y += HeaderHeight + 4f;

            if (selectedType == null)
            {
                return;
            }

            addSearch = Widgets.TextField(new Rect(inner.x, y, inner.width, SearchHeight), addSearch);
            y += SearchHeight + 4f;

            // The gesture cannot be guessed from the header or from the rows: say so.
            //
            // Measured, not assumed: the {0} is a work type's label, and a player can rename a type
            // to anything. A fixed-height box clipped the wrap at both ends - the same defect
            // Architect Studio found on its own intro paragraphs 2026-09-19, reported across and
            // fixed here before anyone hit it.
            var color = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, 0.5f);
            Text.Font = GameFont.Tiny;
            var hint = "WorkStudio.AddHint".Translate(TypeLabel(selectedType));
            var hintHeight = Text.CalcHeight(hint, inner.width);
            Widgets.Label(new Rect(inner.x, y, inner.width, hintHeight), hint);
            Text.Font = GameFont.Small;
            GUI.color = color;
            y += hintHeight + 2f;

            var candidates = Candidates();
            var listRect = new Rect(inner.x, y, inner.width, inner.yMax - y);
            var contentWidth = listRect.width - 16f;

            // Measured once for the whole column, not per row: a ragged grey edge would be harder
            // to read than the few pixels it would win back.
            var typeWidth = GreyTypeWidth(candidates, contentWidth);
            var labelWidth = contentWidth - typeWidth - 6f;

            // Laid out before the scroll view opens, because the total height is the sum of rows
            // that are not all the same height any more. This column has no drag reordering - only
            // the middle one does - so nothing else depends on a row being where a fixed pitch
            // would have put it.
            var rendered = new string[candidates.Count];
            var heights = new float[candidates.Count];
            var total = 0f;
            for (var i = 0; i < candidates.Count; i++)
            {
                rendered[i] = LayOutGiverLabel(GiverLabel(candidates[i]), labelWidth, out heights[i]);
                total += heights[i];
            }

            var viewRect = new Rect(0f, 0f, contentWidth, total);
            var ambiguous = Duplicates(rendered);

            Widgets.BeginScrollView(listRect, ref addScroll, viewRect);

            var rowY = 0f;
            for (var i = 0; i < candidates.Count; i++)
            {
                var row = new Rect(0f, rowY, viewRect.width, heights[i]);
                rowY += heights[i];

                if (Mouse.IsOver(row))
                {
                    Widgets.DrawHighlight(row);
                }

                DrawGiverLabel(row, candidates[i], showOrigin: false, showCurrent: true, ambiguous,
                    typeWidth, rendered[i]);

                if (Widgets.ButtonInvisible(row))
                {
                    MoveTask(candidates[i], selectedType);
                }
            }

            Widgets.EndScrollView();
        }

        private List<WorkGiverDef> Candidates()
        {
            var search = addSearch.Trim();

            return DefDatabase<WorkGiverDef>.AllDefsListForReading
                .Where(g => g.workType != selectedType)
                .Where(g => search.NullOrEmpty()
                            || GiverLabel(g).IndexOf(search, System.StringComparison.OrdinalIgnoreCase) >= 0
                            || g.defName.IndexOf(search, System.StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(GiverLabel)
                .Take(MaxAddResults)
                .ToList();
        }

        /// <summary>
        /// The labels a list shows more than once. Two <see cref="WorkGiverDef"/>s may legitimately
        /// carry the same label — vanilla ships several, and a mod list adds more — and the rows are
        /// then indistinguishable on screen, which matters in a column whose whole purpose is
        /// clicking one row rather than the other. Their defNames are drawn beside them.
        /// </summary>
        private static HashSet<string> AmbiguousLabels(List<WorkGiverDef> givers)
        {
            return Duplicates(givers.Select(GiverLabel));
        }

        /// <summary>
        /// The strings a sequence holds more than once. Pure, and the part worth testing.
        /// <para>
        /// The right-hand column asks this of what it is about to <b>draw</b>, not of the full
        /// labels. A French run on 2026-09-21 put four rows reading "Apporter les ressources ..."
        /// there, two of them also sharing their grey type: distinct labels made equal by the cut.
        /// Comparing the originals cannot see a collision the drawing created.
        /// </para>
        /// </summary>
        internal static HashSet<string> Duplicates(IEnumerable<string> rendered)
        {
            return new HashSet<string>(rendered
                .GroupBy(text => text)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key));
        }

        private static readonly Dictionary<string, string> truncatedMiddle =
            new Dictionary<string, string>();

        /// <summary>
        /// Cuts out of the <b>middle</b> of a label rather than off its end.
        /// <para>
        /// These labels differ by their tail, never by their head: vanilla ships "deliver resources
        /// to blueprints" and "deliver resources to frames", and French stretches both past the
        /// column. Cutting the end throws away the only part that says which row this is, which is
        /// exactly what the screenshots of 2026-09-21 showed. The tail is given the larger share of
        /// what is left for the same reason.
        /// </para>
        /// </summary>
        internal static string TruncateMiddle(string label, float width, Func<string, float> measure = null)
        {
            if (label.NullOrEmpty())
            {
                return label;
            }

            // A caller that brings its own measure is a test, and its answers must not be cached
            // under the same key as the game's own font - nor read from it.
            if (measure != null)
            {
                return Shorten(label, width, measure);
            }

            var key = label + " " + width.ToString("0.#");
            if (truncatedMiddle.TryGetValue(key, out var cached))
            {
                return cached;
            }

            var result = Shorten(label, width, text => Text.CalcSize(text).x);
            truncatedMiddle[key] = result;
            return result;
        }

        /// <summary>
        /// How a row of the right-hand column reads, and how tall it has to be to read that way.
        /// <para>
        /// One line while the label fits, two when it does not, and only then is anything cut -
        /// out of the middle, as everywhere else here. Two lines are the ceiling: this column lists
        /// up to <see cref="MaxAddResults"/> tasks and is scrolled, so letting a row grow without
        /// limit would push the rest off the screen to spell out one name.
        /// </para>
        /// </summary>
        private static string LayOutGiverLabel(string label, float width, out float height)
        {
            height = RowHeight;
            if (label.NullOrEmpty() || width <= 0f)
            {
                return label;
            }

            var key = label + " " + width.ToString("0.#");
            if (laidOut.TryGetValue(key, out var cached))
            {
                height = cached.Height;
                return cached.Text;
            }

            var result = LayOut(label, width);
            laidOut[key] = result;
            height = result.Height;
            return result.Text;
        }

        private struct LaidOut
        {
            public string Text;
            public float Height;
        }

        private static readonly Dictionary<string, LaidOut> laidOut = new Dictionary<string, LaidOut>();

        private static LaidOut LayOut(string label, float width)
        {
            if (Text.CalcSize(label).x <= width)
            {
                return new LaidOut { Text = label, Height = RowHeight };
            }

            // The height two wrapped lines need, asked of the font rather than assumed: a label of
            // exactly two lines must not be cut, and a taller one must be.
            var twoLines = Text.CalcHeight("A\nA", width);

            if (Text.CalcHeight(label, width) <= twoLines)
            {
                return new LaidOut { Text = label, Height = twoLines + RowPadding };
            }

            for (var kept = label.Length - 1; kept > 0; kept--)
            {
                var candidate = Elide(label, kept);
                if (Text.CalcHeight(candidate, width) <= twoLines)
                {
                    return new LaidOut { Text = candidate, Height = twoLines + RowPadding };
                }
            }

            return new LaidOut { Text = Ellipsis, Height = twoLines + RowPadding };
        }

        private const string Ellipsis = "…";
        private const float RowPadding = 6f;

        /// <summary>The label reduced to <paramref name="kept"/> characters, the tail keeping the larger half.</summary>
        private static string Elide(string label, int kept)
        {
            var tail = Mathf.CeilToInt(kept * 0.6f);
            var head = kept - tail;
            return label.Substring(0, head) + Ellipsis + label.Substring(label.Length - tail);
        }

        private static string Shorten(string label, float width, Func<string, float> measure)
        {
            if (measure(label) <= width)
            {
                return label;
            }

            if (measure(Ellipsis) > width)
            {
                return string.Empty;
            }

            // Down from the whole label one character at a time, the tail keeping the larger half:
            // that is where these labels differ.
            for (var kept = label.Length - 1; kept > 0; kept--)
            {
                var candidate = Elide(label, kept);
                if (measure(candidate) <= width)
                {
                    return candidate;
                }
            }

            return Ellipsis;
        }

        /// <summary>
        /// What the grey type column actually needs, rather than a fixed share of the row.
        /// <para>
        /// It used to take 42% of the width whatever it held. On a French client that left the task
        /// label about 186 px where it needed 230, truncating rows that the freed 40 px would have
        /// kept distinct. The old share stays as the ceiling: a mod list with very long work type
        /// names cannot eat the column.
        /// </para>
        /// </summary>
        private static float GreyTypeWidth(List<WorkGiverDef> givers, float rowWidth)
        {
            var widest = 0f;
            foreach (var giver in givers)
            {
                if (giver.workType != null)
                {
                    widest = Mathf.Max(widest, Text.CalcSize(TypeLabel(giver.workType)).x);
                }
            }

            return Mathf.Min(widest + 4f, rowWidth * 0.42f);
        }

        private static void DrawGiverLabel(Rect rect, WorkGiverDef giver, bool showOrigin,
            bool showCurrent = false, HashSet<string> ambiguous = null, float typeWidth = -1f,
            string rendered = null)
        {
            var anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;

            // Both columns list tasks: without the owning type spelled out, nothing tells "what is
            // in this type" apart from "everything else".
            var labelRect = rect;
            if (showCurrent && giver.workType != null)
            {
                var grey = typeWidth >= 0f ? typeWidth : rect.width * 0.42f;
                var typeRect = new Rect(rect.xMax - grey, rect.y, grey, rect.height);
                labelRect = new Rect(rect.x, rect.y, typeRect.x - rect.x - 6f, rect.height);

                var color = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, 0.45f);
                Text.Anchor = TextAnchor.MiddleRight;
                Widgets.Label(typeRect, TypeLabel(giver.workType).Truncate(typeRect.width));
                GUI.color = color;
                Text.Anchor = TextAnchor.MiddleLeft;
            }

            var label = GiverLabel(giver);
            var drawn = rendered ?? TruncateMiddle(label, labelRect.width);
            Widgets.Label(labelRect, drawn);

            // Only when the label alone would not say which row this is: the defName is developer
            // text, and putting it on every row would make the column harder to read, not easier.
            // Matched on what is drawn, not on the full label: truncation is what makes two rows
            // read the same, so the full labels would look distinct while the screen does not.
            if (ambiguous != null && (ambiguous.Contains(label) || ambiguous.Contains(drawn)))
            {
                var used = Mathf.Min(Text.CalcSize(drawn).x, labelRect.width);
                var defNameRect = new Rect(labelRect.x + used + 6f, labelRect.y,
                    labelRect.width - used - 6f, labelRect.height);

                if (defNameRect.width > 0f)
                {
                    var color = GUI.color;
                    GUI.color = new Color(1f, 1f, 1f, 0.45f);
                    Widgets.Label(defNameRect, giver.defName.Truncate(defNameRect.width));
                    GUI.color = color;
                }
            }

            Text.Anchor = anchor;

            var tip = giver.defName;

            if (showCurrent && giver.workType != null)
            {
                tip = TypeLabel(giver.workType) + "\n" + tip;
            }

            if (showOrigin)
            {
                tip += "\n" + "WorkStudio.OrderTip".Translate(giver.priorityInType);

                if (IsMoved(giver))
                {
                    var origin = WorkTypeRuntime.OriginalTypeOf(giver);
                    tip += "\n" + "WorkStudio.OriginTip".Translate(origin == null ? "-" : TypeLabel(origin));
                }
            }

            TooltipHandler.TipRegion(rect, tip);
        }

        private void OpenMoveMenu(WorkGiverDef giver)
        {
            var options = new List<FloatMenuOption>();

            var origin = WorkTypeRuntime.OriginalTypeOf(giver);
            if (origin != null && origin != giver.workType)
            {
                options.Add(new FloatMenuOption(
                    "WorkStudio.BackToOrigin".Translate(TypeLabel(origin)),
                    () => MoveTask(giver, origin)));
            }

            foreach (var type in Types)
            {
                if (type == giver.workType)
                {
                    continue;
                }

                var target = type;
                options.Add(new FloatMenuOption(TypeLabel(target), () => MoveTask(giver, target)));
            }

            if (options.Count > 0)
            {
                Find.WindowStack.Add(new FloatMenu(options));
            }
        }
    }
}
