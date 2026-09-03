using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace WorkStudio
{
    /// <summary>
    /// Ajoute un bouton d'ouverture de l'editeur dans le bandeau de l'onglet Travail. C'est la que
    /// l'on constate qu'une colonne manque ou qu'un travail est mal range, donc c'est de la qu'il
    /// faut pouvoir corriger - sans passer par le menu des options.
    /// <para>
    /// Le patch ne peut pas viser <c>MainTabWindow_Work</c> en dur. Un onglet Travail de
    /// remplacement - Better Work Tab, par exemple - se declare dans le <c>tabWindowClass</c> du
    /// <c>MainButtonDef</c>, herite de la classe vanilla mais redefinit <c>DoWindowContents</c>
    /// <b>sans appeler <c>base</c></b> : la methode vanilla n'est alors jamais executee, et le
    /// bouton n'apparaitrait nulle part. On patche donc la classe reellement utilisee, quelle
    /// qu'elle soit.
    /// </para>
    /// </summary>
    public static class Patch_WorkTabButton
    {
        private const string WorkButtonDefName = "Work";

        /// <summary>
        /// A appeler une fois les defs chargees : c'est seulement a ce moment que le
        /// <c>tabWindowClass</c> est resolu, et que les patchs XML des autres mods l'ont deja
        /// remplace le cas echeant.
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
        /// Premiere classe de la hierarchie qui declare vraiment <c>DoWindowContents</c>. Patcher
        /// une methode heritee toucherait toutes les tables de pions - Animaux, Restrictions - d'ou
        /// le garde-fou sur le def au moment de dessiner.
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
        /// Le rectangle est pris par position (<c>__0</c>) et non par nom. Harmony injecte les
        /// parametres d'apres leur nom, et celui-ci n'est pas le meme partout : <c>rect</c> chez
        /// vanilla, <c>inRect</c> chez Better Work Tab. Demander <c>rect</c> faisait rejeter le
        /// patch avec « Parameter "rect" not found », donc pas de bouton du tout.
        /// </summary>
        public static void Postfix(MainTabWindow __instance, Rect __0)
        {
            var rect = __0;

            if (Event.current.type == EventType.Layout || __instance.def?.defName != WorkButtonDefName)
            {
                return;
            }

            // Le bandeau vanilla occupe la gauche (cases a cocher) et le centre (fleches de
            // priorite, dessinees a des abscisses fixes) : la droite est libre.
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
