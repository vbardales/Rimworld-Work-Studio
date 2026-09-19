using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using HarmonyLib;
using RimWorld;
using Verse;

namespace WorkStudio.PickleSteps
{
    /// <summary>
    /// Records every change to one pawn's priority for one work type, and where it came from.
    /// <para>
    /// Four runs have narrowed the scenario 8 failure by elimination - not the suite initializing
    /// the pawn (2026-09-19 says <c>initialized by the suite=False</c>), not Enhanced Work Tab
    /// answering <c>GetPriority</c> from its own store (the raw <c>DefMap</c> reads 0 as well), not
    /// 1trickPwnyta's Defaults, not Concord, not Simply More FPS - and each run could only say the
    /// value was wrong by the time the <c>Then</c> looked. None of them could say <b>when</b> it
    /// died, or <b>who</b> killed it.
    /// </para>
    /// <para>
    /// This watches three places at once: around <see cref="WorkTypeRuntime.Apply"/>, after
    /// <see cref="PriorityMemory.Restore"/>, and on every call to
    /// <c>Pawn_WorkSettings.SetPriority</c> that writes a zero — with the stack trace, which names
    /// the caller outright whether it is vanilla's own <c>Disable</c> on a disabled type, this
    /// mod's restore, or a mod nobody has looked at yet. <c>Restore</c> writes the <c>DefMap</c>'s
    /// backing list directly and never calls <c>SetPriority</c>, so the two together also separate
    /// "the restore wrote the wrong value" from "the right value was written and then overwritten".
    /// </para>
    /// </summary>
    public static class PriorityProbe
    {
        private static readonly List<string> Timeline = new List<string>();
        private static Pawn watchedPawn;
        private static WorkTypeDef watchedType;
        private static bool patched;

        public static void Watch(Pawn pawn, WorkTypeDef type)
        {
            Ensure();
            watchedPawn = pawn;
            watchedType = type;
            Timeline.Clear();
            Note("watching");
        }

        public static void Stop()
        {
            watchedPawn = null;
            watchedType = null;
        }

        public static string Report()
        {
            return Timeline.Count == 0 ? "(the probe recorded nothing)" : string.Join(" | ", Timeline);
        }

        private static void Ensure()
        {
            if (patched)
            {
                return;
            }

            patched = true;
            var harmony = new Harmony("nelim.workstudio.pickle.probe");

            harmony.Patch(AccessTools.Method(typeof(WorkTypeRuntime), nameof(WorkTypeRuntime.Apply)),
                prefix: new HarmonyMethod(typeof(PriorityProbe), nameof(BeforeApply)),
                postfix: new HarmonyMethod(typeof(PriorityProbe), nameof(AfterApply)));

            harmony.Patch(AccessTools.Method(typeof(PriorityMemory), nameof(PriorityMemory.Restore)),
                postfix: new HarmonyMethod(typeof(PriorityProbe), nameof(AfterRestore)));

            harmony.Patch(AccessTools.Method(typeof(Pawn_WorkSettings), nameof(Pawn_WorkSettings.SetPriority)),
                prefix: new HarmonyMethod(typeof(PriorityProbe), nameof(BeforeSetPriority)));
        }

        public static void BeforeApply() => Note("before Apply");

        public static void AfterApply() => Note("after Apply");

        public static void AfterRestore() => Note("after PriorityMemory.Restore");

        public static void BeforeSetPriority(Pawn_WorkSettings __instance, WorkTypeDef w, int priority)
        {
            if (watchedPawn == null || w != watchedType || !ReferenceEquals(PawnOf(__instance), watchedPawn))
            {
                return;
            }

            // Only a write that takes the value away is worth a trace; the scenario's own write of
            // the value it wants is noise, and the traces are long.
            Timeline.Add(priority == 0
                ? "SetPriority(0) FROM " + Caller()
                : "SetPriority(" + priority + ")");
        }

        private static void Note(string label)
        {
            if (watchedPawn == null || watchedType == null)
            {
                return;
            }

            Timeline.Add(label + "=" + Raw(watchedPawn, watchedType));
        }

        private static Pawn PawnOf(Pawn_WorkSettings settings)
        {
            return typeof(Pawn_WorkSettings)
                .GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(settings) as Pawn;
        }

        /// <summary>The DefMap's own backing list, read without going through GetPriority.</summary>
        private static int Raw(Pawn pawn, WorkTypeDef def)
        {
            var priorities = typeof(Pawn_WorkSettings)
                .GetField("priorities", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(pawn.workSettings);
            var values = priorities?.GetType()
                .GetField("values", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(priorities) as System.Collections.IList;
            return values != null && def.index < values.Count ? (int)values[def.index] : -1;
        }

        /// <summary>
        /// The first frames of the stack that are not this probe and not the game's own accessor,
        /// which is what names the mod responsible.
        /// </summary>
        private static string Caller()
        {
            var trace = new System.Diagnostics.StackTrace(2, false);
            var text = new StringBuilder();
            int kept = 0;

            for (int i = 0; i < trace.FrameCount && kept < 6; i++)
            {
                MethodBase method = trace.GetFrame(i).GetMethod();
                if (method?.DeclaringType == null || method.DeclaringType == typeof(PriorityProbe))
                {
                    continue;
                }

                if (kept > 0)
                {
                    text.Append(" <- ");
                }

                text.Append(method.DeclaringType.FullName).Append('.').Append(method.Name);
                kept++;
            }

            return text.Length == 0 ? "(no frames)" : text.ToString();
        }
    }
}
