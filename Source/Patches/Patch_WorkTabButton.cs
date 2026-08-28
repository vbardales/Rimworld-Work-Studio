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
    /// </summary>
    [HarmonyPatch(typeof(MainTabWindow_Work), nameof(MainTabWindow_Work.DoWindowContents))]
    public static class Patch_WorkTabButton
    {
        public static void Postfix(Rect rect)
        {
            if (Event.current.type == EventType.Layout)
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
