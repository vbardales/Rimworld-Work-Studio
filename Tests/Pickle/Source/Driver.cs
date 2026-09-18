using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorks.Pickle;
using Verse;

namespace WorkStudio.PickleSteps
{
    /// <summary>
    /// Drives the editor the way its buttons do. The mutations live in private methods of
    /// <see cref="Dialog_WorkTypes"/>, so they are called by name: a renamed method fails the step
    /// saying which one, instead of the suite testing a copy of logic the mod no longer runs.
    /// </summary>
    public static class Driver
    {
        private const BindingFlags Instance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>A vanilla type by defName, or a created one by its label: a created type's defName depends on what exists.</summary>
        public static WorkTypeDef WorkType(PickleContext ctx, string defNameOrLabel)
        {
            var def = DefDatabase<WorkTypeDef>.GetNamedSilentFail(defNameOrLabel);
            if (def == null)
            {
                var entry = WorkStudioMod.Settings.customTypes.LastOrDefault(e => e.label == defNameOrLabel);
                def = entry != null ? DefDatabase<WorkTypeDef>.GetNamedSilentFail(entry.id) : null;
            }

            ctx.Require(def != null, $"no work type is named or labelled '{defNameOrLabel}'; types: " +
                string.Join(", ", DefDatabase<WorkTypeDef>.AllDefsListForReading.Select(t => t.defName)));
            return def;
        }

        public static WorkGiverDef Task(PickleContext ctx, string defName)
        {
            var def = DefDatabase<WorkGiverDef>.GetNamedSilentFail(defName);
            ctx.Require(def != null, $"no work giver is named '{defName}'");
            return def;
        }

        public static Dialog_WorkTypes Editor(PickleContext ctx)
        {
            var dialog = Find.WindowStack.WindowOfType<Dialog_WorkTypes>();
            if (dialog == null)
            {
                dialog = new Dialog_WorkTypes();
                Find.WindowStack.Add(dialog);
            }

            return dialog;
        }

        public static object Call(PickleContext ctx, object target, string method, params object[] args)
        {
            var info = target.GetType().GetMethod(method, Instance);
            ctx.Require(info != null,
                $"{target.GetType().Name}.{method} no longer exists: the scenario drives the editor through it, update the steps");
            try
            {
                return info.Invoke(target, args);
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException ?? ex;
            }
        }

        public static void Select(PickleContext ctx, Dialog_WorkTypes dialog, WorkTypeDef type)
        {
            var field = typeof(Dialog_WorkTypes).GetField("selectedType", Instance);
            ctx.Require(field != null, "Dialog_WorkTypes.selectedType no longer exists: update the steps");
            field.SetValue(dialog, type);
        }

        /// <summary>The right column's search box, typed into rather than clicked.</summary>
        public static void Search(PickleContext ctx, Dialog_WorkTypes dialog, string text)
        {
            var field = typeof(Dialog_WorkTypes).GetField("addSearch", Instance);
            ctx.Require(field != null, "Dialog_WorkTypes.addSearch no longer exists: update the steps");
            field.SetValue(dialog, text);
        }

        public static int ReorderGroup(Dialog_WorkTypes dialog, string field)
        {
            return (int)typeof(Dialog_WorkTypes).GetField(field, Instance).GetValue(dialog);
        }

        /// <summary>The order the editor's left column shows, which is also the order pawns go through types.</summary>
        public static List<WorkTypeDef> TypesInOrder() => WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder.ToList();

        public static List<PawnColumnDef> WorkColumns() =>
            PawnTableDefOf.Work.columns.Where(c => c.workType != null).ToList();

        public static string Names<T>(IEnumerable<T> defs) where T : Def => "[" + string.Join(", ", defs.Select(d => d.defName)) + "]";

        /// <summary>A created type's defName is not what a scenario wrote: show the label next to it.</summary>
        public static string Describe(WorkTypeDef type) =>
            WorkTypeRuntime.IsCustom(type) ? $"{type.label} ({type.defName})" : type.defName;
    }

    /// <summary>
    /// Keeps the callback each <c>ReorderableWidget.NewGroup</c> registers. The game clears its
    /// groups at the end of every repaint, so this is the only way to reach the method the editor
    /// hands over and replay a drop through it.
    /// </summary>
    [HarmonyPatch(typeof(ReorderableWidget), nameof(ReorderableWidget.NewGroup))]
    public static class ReorderCapture
    {
        public static readonly Dictionary<int, Action<int, int>> Actions = new Dictionary<int, Action<int, int>>();

        public static void Postfix(int __result, Action<int, int> reorderedAction)
        {
            if (__result >= 0)
            {
                Actions[__result] = reorderedAction;
            }
        }

        private static bool patched;

        public static void Ensure()
        {
            if (patched)
            {
                return;
            }

            new Harmony("nelim.workstudio.pickletests").CreateClassProcessor(typeof(ReorderCapture)).Patch();
            patched = true;
        }
    }
}
