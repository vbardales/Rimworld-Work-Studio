using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace WorkStudio
{
    /// <summary>
    /// Adds a button that opens the editor to the header strip of the Work tab. That is where one
    /// notices a missing column or a misfiled job, so that is where fixing it should be possible -
    /// without going through the options menu.
    /// <para>
    /// The patch cannot hard-code <c>MainTabWindow_Work</c>. A replacement Work tab - Better Work
    /// Tab, for instance - declares itself in the <c>tabWindowClass</c> of the
    /// <c>MainButtonDef</c>, inherits the vanilla class but overrides <c>DoWindowContents</c>
    /// <b>without calling <c>base</c></b>: the vanilla method then never runs, and the button would
    /// show up nowhere. So the class actually in use is patched, whatever it is.
    /// </para>
    /// </summary>
    public static class Patch_WorkTabButton
    {
        private const string WorkButtonDefName = "Work";

        /// <summary>
        /// To be called once defs are loaded: only then is the <c>tabWindowClass</c> resolved, and
        /// other mods' XML patches have already replaced it where they do.
        /// </summary>
        public static void Apply(Harmony harmony)
        {
            try
            {
                var button = DefDatabase<MainButtonDef>.GetNamedSilentFail(WorkButtonDefName);
                var target = FindDrawMethod(button?.tabWindowClass);

                if (target == null)
                {
                    Log.Warning("[Work Studio] Could not find the Work tab window: the in-tab " +
                                "button will not be shown. The editor is still reachable from the " +
                                "mod settings.");
                    return;
                }

                var postfix = AccessTools.Method(typeof(Patch_WorkTabButton), nameof(Postfix));
                harmony.Patch(target, postfix: new HarmonyMethod(postfix));
            }
            catch (Exception exception)
            {
                Log.Warning("[Work Studio] Could not add the button to the Work tab: " +
                            exception.Message);
            }
        }

        /// <summary>
        /// First class in the hierarchy that actually declares <c>DoWindowContents</c>. Patching an
        /// inherited method would touch every pawn table - Animals, Restrict - hence the guard on
        /// the def at draw time.
        /// </summary>
        private static MethodInfo FindDrawMethod(Type windowClass)
        {
            for (var type = windowClass; type != null && type != typeof(object); type = type.BaseType)
            {
                var declared = AccessTools.DeclaredMethod(type, nameof(Window.DoWindowContents));
                if (declared != null)
                {
                    return declared;
                }
            }

            return null;
        }

        /// <summary>
        /// The rectangle is taken by position (<c>__0</c>), not by name. Harmony injects parameters
        /// by name, and this one is not named the same everywhere: <c>rect</c> in vanilla,
        /// <c>inRect</c> in Better Work Tab. Asking for <c>rect</c> got the patch rejected with
        /// "Parameter "rect" not found", so no button at all.
        /// </summary>
        public static void Postfix(MainTabWindow __instance, Rect __0)
        {
            var rect = __0;

            if (Event.current.type == EventType.Layout || __instance.def?.defName != WorkButtonDefName)
            {
                return;
            }

            // The vanilla header strip takes the left (checkboxes) and the centre (priority
            // arrows, drawn at fixed x positions): the right is free.
            var button = new Rect(rect.xMax - 160f, rect.y + 3f, 155f, 28f);

            var font = Text.Font;
            Text.Font = GameFont.Small;

            if (Widgets.ButtonText(button, "WorkStudio.OpenEditorShort".Translate()))
            {
                Find.WindowStack.Add(new Dialog_WorkTypes());
            }

            Text.Font = font;
        }
    }
}
