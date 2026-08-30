using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace WorkStudio
{
    /// <summary>
    /// Editeur des types de travail : trois colonnes - les types dans leur ordre reel, les taches du
    /// type selectionne, et de quoi lui en ajouter. Chaque modification est appliquee immediatement.
    /// La fenetre n'est ni modale ni bloquante, pour qu'on voie l'onglet Travail se reorganiser en
    /// direct derriere.
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

        /// <summary>Plafond que <c>WorkTypeDef.ConfigErrors</c> accepte pour une priorite naturelle.</summary>
        private const int MaxNaturalPriority = 10000;

        private static readonly Color DisabledArrowColor = new Color(1f, 1f, 1f, 0.25f);

        private WorkTypeDef selectedType;
        private string addSearch = "";

        private Vector2 typeScroll;
        private Vector2 taskScroll;
        private Vector2 addScroll;

        /// <summary>
        /// Identifiants des groupes de glisser-deposer. Ils doivent survivre d'une passe d'evenement
        /// a l'autre : <see cref="ReorderableWidget.NewGroup"/> ne rend un identifiant qu'en
        /// <c>Repaint</c> et renvoie -1 partout ailleurs. Les garder dans une variable locale ferait
        /// donc passer -1 a <see cref="ReorderableWidget.Reorderable"/> lors du <c>MouseDown</c>, et
        /// le glisser ne demarrerait jamais - le clic, lui, continuerait de fonctionner.
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

        // ---------------------------------------------------------------- donnees

        /// <summary>Les types dans l'ordre ou ils apparaissent reellement, du plus prioritaire au moins.</summary>
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

            // Le rang d'une tache n'a de sens qu'au sein d'une liste donnee. En changeant de type
            // elle reprend sa priorite d'origine, qui la situe correctement parmi les taches
            // vanilla, plutot qu'une valeur reglee pour une liste qu'elle vient de quitter.
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
        /// Redistribue sur une liste reordonnee les valeurs de priorite qu'elle portait deja.
        /// <para>
        /// On reutilise ces valeurs plutot que d'inventer une echelle : le jeu et les mods
        /// travaillent avec des ordres de grandeur precis - de 0 a 1400 pour les types de travail -
        /// et un mod charge plus tard doit pouvoir se glisser au milieu de la liste plutot que de
        /// se retrouver relegue en queue. Les egalites sont ecartees d'une unite, sans quoi l'ordre
        /// relatif de deux voisins resterait indefini.
        /// </para>
        /// </summary>
        private static List<int> Redistribute(IEnumerable<int> current)
        {
            var values = current.OrderByDescending(v => v).ToList();

            // Zero ou une seule valeur : rien a departager, et les deux boucles ci-dessous ne
            // tournent pas. On sort tel quel plutot que de laisser les garde-fous s'en charger.
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

            // Ecarter les egalites fait descendre la queue de liste : si des types partagaient tous
            // la meme priorite, elle plonge sous zero. On remonte l'echelle entiere.
            var lowest = values[values.Count - 1];
            if (lowest < 0)
            {
                for (var i = 0; i < values.Count; i++)
                {
                    values[i] -= lowest;
                }
            }

            // Et si le sommet deborde, on retombe sur une echelle reguliere. La borne n'est pas
            // decorative : Pawn_WorkSettings.CacheWorkGiversInOrder classe les travaux sur
            // naturalPriority + (4 - priorite du colon) * 100000. Une priorite naturelle assez
            // grande passerait donc par-dessus une bande de priorite entiere, et un travail regle
            // sur 4 se ferait avant un travail regle sur 1.
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
        /// Deplace un element selon la convention de <see cref="ReorderableWidget"/> : <paramref
        /// name="to"/> est la position d'insertion <b>avant</b> retrait, pas la position finale.
        /// Retirer d'abord decalerait la cible d'un cran pour tout deplacement vers le bas.
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

        /// <summary>Reordonne les types de travail en reaffectant leurs priorites naturelles.</summary>
        private void ReorderTypes(int from, int to)
        {
            var ordered = Types.ToList();
            if (DragMove(ordered, from, to))
            {
                ApplyTypeOrder(ordered);
            }
        }

        /// <summary>
        /// Reordonne les taches a l'interieur du type selectionne, en reaffectant leurs
        /// <see cref="WorkGiverDef.priorityInType"/> : c'est l'ordre dans lequel un colon les
        /// attrape une fois qu'il s'est mis a ce travail.
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
        /// Deplace un type d'un cran. Les fleches doublent le glisser-deposer, qui n'est pas
        /// praticable au pave tactile d'une Steam Deck.
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
        /// Dessine les deux fleches de deplacement et signale le cran demande, 0 si rien n'est
        /// clique. Les extremites de liste restent cliquables mais grisees : un bouton qui
        /// disparait deplace tous les autres sous le doigt.
        /// </summary>
        private static int DrawArrows(Rect rect, bool canGoUp, bool canGoDown)
        {
            var up = new Rect(rect.x, rect.y + (rect.height - ArrowSize) / 2f, ArrowSize, ArrowSize);
            var down = new Rect(up.xMax + 2f, up.y, ArrowSize, ArrowSize);
            var delta = 0;

            // ButtonImage impose lui-meme GUI.color : le grisage doit passer par ses parametres,
            // pas par une couleur posee autour de l'appel.
            if (Widgets.ButtonImage(up, TexButton.ReorderUp,
                    canGoUp ? Color.white : DisabledArrowColor,
                    canGoUp ? GenUI.MouseoverColor : DisabledArrowColor,
                    canGoUp, "WorkStudio.MoveUp".Translate()) && canGoUp)
            {
                delta = -1;
            }

            if (Widgets.ButtonImage(down, TexButton.ReorderDown,
                    canGoDown ? Color.white : DisabledArrowColor,
                    canGoDown ? GenUI.MouseoverColor : DisabledArrowColor,
                    canGoDown, "WorkStudio.MoveDown".Translate()) && canGoDown)
            {
                delta = 1;
            }

            return delta;
        }

        /// <summary>Le type a-t-il des taches dont l'ordre a ete retouche ?</summary>
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

            // Un type neuf se pose juste sous le type selectionne : c'est presque toujours de lui
            // qu'on s'apprete a extraire des taches. Si ce voisin est deja au plancher, on s'y pose
            // a egalite plutot que dans le negatif - a charge pour l'utilisatrice de trancher.
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

            // Les taches du type supprime doivent retrouver un toit : on les renvoie a leur origine.
            // Leur ordre part avec le type : il avait ete regle pour cette liste-la, le garder
            // reordonnerait le type d'accueil sans que personne l'ait demande.
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

        /// <summary>Remet toutes les taches de ce type la ou le jeu les avait mises.</summary>
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

        // ---------------------------------------------------------------- rendu

        public override void DoWindowContents(Rect inRect)
        {
            // La liste est reconstruite a chaque passe : les defs peuvent bouger sous nos pieds
            // (reinitialisation depuis la fenetre de reglages, mod recharge). Elle reste stable au
            // sein d'une passe, ce dont le glisser-deposer a besoin pour que ses index collent.
            InvalidateCaches();

            // Le type selectionne a pu disparaitre entre-temps.
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

            // Le pied de colonne s'empile depuis le bas : la liste prend ce qui reste.
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

            // Le groupe se declare a l'interieur de la zone defilante : NewGroup ancre son rectangle
            // sur l'origine GUI courante, donc les coordonnees ecran suivent le defilement.
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

            // Un glisser qui se termine sur une ligne ne doit pas valoir clic : sans ce garde-fou,
            // relacher au-dessus d'une case a cocher la bascule au passage.
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

            // Glisser une tache change sa priorite au sein du type : l'ordre affiche est deja
            // l'ordre reel, puisque la liste vient de workGiversByPriority.
            if (Event.current.type == EventType.Repaint)
            {
                taskReorderGroup = ReorderableWidget.NewGroup(
                    ReorderTasks,
                    ReorderableDirection.Vertical,
                    listRect);
            }

            for (var i = 0; i < tasks.Count; i++)
            {
                var row = new Rect(0f, i * RowHeight, viewRect.width, RowHeight);

                ReorderableWidget.Reorderable(taskReorderGroup, row);
                DrawTaskRow(row, tasks[i], i, tasks.Count);
            }

            Widgets.EndScrollView();

            if (resetOrderRow.HasValue &&
                Widgets.ButtonText(resetOrderRow.Value, "WorkStudio.ResetTaskOrder".Translate()))
            {
                ResetTaskOrder(type);
            }

            DrawManageRow(manageRow, type, custom);
        }

        private void DrawTaskRow(Rect row, WorkGiverDef giver, int index, int count)
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

            DrawGiverLabel(labelRect, giver, showOrigin: true);

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

            // Le geste n'est devinable ni depuis l'en-tete ni depuis les lignes : on le dit.
            var color = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, 0.5f);
            Text.Font = GameFont.Tiny;
            Widgets.Label(new Rect(inner.x, y, inner.width, 20f),
                "WorkStudio.AddHint".Translate(TypeLabel(selectedType)));
            Text.Font = GameFont.Small;
            GUI.color = color;
            y += 22f;

            var candidates = Candidates();
            var listRect = new Rect(inner.x, y, inner.width, inner.yMax - y);
            var viewRect = new Rect(0f, 0f, listRect.width - 16f, candidates.Count * RowHeight);

            Widgets.BeginScrollView(listRect, ref addScroll, viewRect);

            var rowY = 0f;
            foreach (var giver in candidates)
            {
                var row = new Rect(0f, rowY, viewRect.width, RowHeight);
                rowY += RowHeight;

                if (Mouse.IsOver(row))
                {
                    Widgets.DrawHighlight(row);
                }

                DrawGiverLabel(row, giver, showOrigin: false, showCurrent: true);

                if (Widgets.ButtonInvisible(row))
                {
                    MoveTask(giver, selectedType);
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

        private static void DrawGiverLabel(Rect rect, WorkGiverDef giver, bool showOrigin,
            bool showCurrent = false)
        {
            var anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;

            // Les deux colonnes listent des taches : sans le type d'appartenance affiche en clair,
            // rien ne distingue "ce qui est dans ce type" de "tout le reste".
            var labelRect = rect;
            if (showCurrent && giver.workType != null)
            {
                var typeRect = new Rect(rect.xMax - rect.width * 0.42f, rect.y, rect.width * 0.42f,
                    rect.height);
                labelRect = new Rect(rect.x, rect.y, typeRect.x - rect.x - 6f, rect.height);

                var color = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, 0.45f);
                Text.Anchor = TextAnchor.MiddleRight;
                Widgets.Label(typeRect, TypeLabel(giver.workType).Truncate(typeRect.width));
                GUI.color = color;
                Text.Anchor = TextAnchor.MiddleLeft;
            }

            var label = GiverLabel(giver);
            Widgets.Label(labelRect, label.Truncate(labelRect.width));

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
