using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace WorkStudio
{
    /// <summary>
    /// An icon for each skill and each work type, drawn in the character tab's skill list and at
    /// the foot of each Work tab column header.
    /// <para>
    /// The drawings come from the SkillIcons mod, handed over 2026-09-18 (its commit
    /// <c>80d3446</c>); see <c>BACKLOG.md</c>. They obey the <b>opposite</b> rule to that mod's
    /// passion icons: there colour carries the meaning, so here shape does and the set is
    /// monochrome. A tint would make both unreadable at once.
    /// </para>
    /// <para>
    /// Textures live under <c>Textures/WorkStudio/</c> rather than at the top level, so that a
    /// player who still has SkillIcons installed during the handover does not end up with two mods
    /// claiming the same <c>ContentFinder</c> paths.
    /// </para>
    /// </summary>
    public static class WorkTypeIcons
    {
        /// <summary>Column header layout: icon and label, icon alone, or label alone.</summary>
        public const int HeaderIconAndLabel = 0;
        public const int HeaderIconOnly = 1;
        public const int HeaderLabelOnly = 2;

        private const float SkillIconSize = 20f;
        private const float SkillIconGap = 3f;
        private const float HeaderIconMaxSize = 22f;

        /// <summary>
        /// <see cref="ContentFinder{T}"/> is expensive and logs every miss, so each key is asked
        /// once — including when the answer is "there is no icon for this one", which is the normal
        /// case for a work type added by another mod, or one the player just created.
        /// </summary>
        private static readonly Dictionary<string, Texture2D> Cache = new Dictionary<string, Texture2D>();

        private static Texture2D Load(string path)
        {
            if (Cache.TryGetValue(path, out var texture))
            {
                return texture;
            }

            texture = ContentFinder<Texture2D>.Get(path, false);
            Cache[path] = texture;
            return texture;
        }

        public static Texture2D For(SkillDef def) =>
            def == null ? null : Load("WorkStudio/Skills/" + def.defName);

        public static Texture2D For(WorkTypeDef def) =>
            def == null ? null : Load("WorkStudio/WorkTypes/" + def.defName);

        private static WorkStudioSettings Settings => WorkStudioMod.Settings;

        /// <summary>
        /// Called from <see cref="StartupInit"/> rather than from a static constructor of its own:
        /// both targets are resolved by reflection, and one of them may not exist.
        /// </summary>
        public static void Apply(Harmony harmony)
        {
            harmony.Patch(
                AccessTools.Method(typeof(SkillUI), nameof(SkillUI.DrawSkill), new[]
                {
                    typeof(SkillRecord), typeof(Rect), typeof(SkillUI.SkillDrawMode), typeof(string)
                }),
                prefix: new HarmonyMethod(typeof(WorkTypeIcons), nameof(SkillPrefix)));

            // The priority column may not redeclare DoHeader. AccessTools then falls back to
            // PawnColumnWorker and EVERY column is patched, Name and Sex included - harmless, since
            // those carry no workType and the prefix hands control straight back, but alarming in a
            // profiler, so it is written down rather than discovered. If neither is found, nothing
            // is patched: a failed Harmony call here would otherwise take the whole startup with it.
            var doHeader = AccessTools.DeclaredMethod(typeof(PawnColumnWorker_WorkPriority), "DoHeader")
                           ?? AccessTools.Method(typeof(PawnColumnWorker), "DoHeader");

            if (doHeader == null)
            {
                Log.Warning("[Work Studio] DoHeader not found: Work tab column headers will stay "
                            + "without an icon.");
                return;
            }

            harmony.Patch(doHeader, prefix: new HarmonyMethod(typeof(WorkTypeIcons), nameof(HeaderPrefix)));
        }

        /// <summary>
        /// Shrinks the rect and draws in the room freed, then lets the game draw as it always does.
        /// That is what keeps this independent of <c>DrawSkill</c>'s own layout: it never looks for
        /// where the label, the bar or the passion icon sit, it just leaves them less room.
        /// </summary>
        public static void SkillPrefix(SkillRecord skill, ref Rect holdingRect)
        {
            if (Settings?.showSkillIcons != true || skill?.def == null)
            {
                return;
            }

            var icon = For(skill.def);
            if (icon == null)
            {
                return;
            }

            var square = new Rect(holdingRect.x, holdingRect.y + (holdingRect.height - SkillIconSize) / 2f,
                SkillIconSize, SkillIconSize);

            var previous = GUI.color;
            GUI.color = Color.white;
            GUI.DrawTexture(square, icon);
            GUI.color = previous;

            holdingRect = new Rect(holdingRect.x + SkillIconSize + SkillIconGap, holdingRect.y,
                holdingRect.width - SkillIconSize - SkillIconGap, holdingRect.height);
        }

        /// <summary>
        /// Returning false is what implements "icon only": the game never draws its label. The other
        /// two modes hand control back.
        /// </summary>
        public static bool HeaderPrefix(Rect rect, PawnColumnWorker __instance)
        {
            var settings = Settings;
            if (settings?.showWorkTypeIcons != true || settings.workTabHeaderMode == HeaderLabelOnly)
            {
                return true;
            }

            var type = __instance?.def?.workType;
            var icon = For(type);
            if (icon == null)
            {
                return true;
            }

            // A header is narrow and tall: the icon sits at its foot, centred, and the game's
            // vertical label keeps whatever room is left above.
            var size = Mathf.Min(rect.width - 2f, HeaderIconMaxSize);
            var square = new Rect(rect.x + (rect.width - size) / 2f, rect.yMax - size - 2f, size, size);

            var previous = GUI.color;
            GUI.color = Color.white;
            GUI.DrawTexture(square, icon);
            GUI.color = previous;

            if (settings.workTabHeaderMode != HeaderIconOnly)
            {
                return true;
            }

            // The one thing the suppressed label did that the icon cannot: say what the column is.
            if (type != null)
            {
                TooltipHandler.TipRegion(rect,
                    () => type.gerundLabel.CapitalizeFirst() + "\n\n" + type.description,
                    type.shortHash);
            }

            return false;
        }
    }
}
