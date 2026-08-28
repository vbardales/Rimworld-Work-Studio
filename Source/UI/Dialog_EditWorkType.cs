using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace WorkStudio
{
    /// <summary>
    /// Fiche d'un type de travail personnalise : les libelles que le jeu affiche un peu partout, et
    /// les competences qui comptent pour lui.
    /// </summary>
    public class Dialog_EditWorkType : Window
    {
        private const float RowHeight = 30f;
        private const float LabelWidth = 190f;

        private readonly CustomWorkTypeEntry entry;
        private readonly Action onChanged;

        private Vector2 skillScroll;

        public override Vector2 InitialSize => new Vector2(620f, 640f);

        public Dialog_EditWorkType(CustomWorkTypeEntry entry, Action onChanged)
        {
            this.entry = entry;
            this.onChanged = onChanged;

            forcePause = false;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = false;
            doCloseX = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            var y = inRect.y;

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(inRect.x, y, inRect.width, 34f), "WorkStudio.EditTitle".Translate());
            Text.Font = GameFont.Small;
            y += 40f;

            entry.label = Field(inRect, ref y, "WorkStudio.Field.Label".Translate(), entry.label);
            entry.labelShort = Field(inRect, ref y, "WorkStudio.Field.LabelShort".Translate(), entry.labelShort);
            entry.pawnLabel = Field(inRect, ref y, "WorkStudio.Field.PawnLabel".Translate(), entry.pawnLabel);
            entry.gerundLabel = Field(inRect, ref y, "WorkStudio.Field.Gerund".Translate(), entry.gerundLabel);
            entry.verb = Field(inRect, ref y, "WorkStudio.Field.Verb".Translate(), entry.verb);

            y += 6f;
            Widgets.Label(new Rect(inRect.x, y, inRect.width, 24f), "WorkStudio.Field.Description".Translate());
            y += 26f;
            entry.description = Widgets.TextArea(new Rect(inRect.x, y, inRect.width, 60f), entry.description ?? "");
            y += 68f;

            var alwaysActive = entry.alwaysStartActive;
            Widgets.CheckboxLabeled(new Rect(inRect.x, y, inRect.width, RowHeight),
                "WorkStudio.Field.AlwaysStartActive".Translate(), ref alwaysActive);
            entry.alwaysStartActive = alwaysActive;
            y += RowHeight;

            var requireCapable = entry.requireCapableColonist;
            Widgets.CheckboxLabeled(new Rect(inRect.x, y, inRect.width, RowHeight),
                "WorkStudio.Field.RequireCapable".Translate(), ref requireCapable);
            entry.requireCapableColonist = requireCapable;
            y += RowHeight + 8f;

            Widgets.Label(new Rect(inRect.x, y, inRect.width, 24f), "WorkStudio.Field.Skills".Translate());
            y += 26f;

            var applyRow = new Rect(inRect.x, inRect.yMax - 32f, inRect.width, 30f);
            var skillRect = new Rect(inRect.x, y, inRect.width, applyRow.y - 8f - y);
            DrawSkills(skillRect);

            // La validation se fait a la fermeture, quel qu'en soit le chemin : ce bouton ferme, et
            // la croix aussi. Sans quoi un reglage saisi puis ferme par la croix serait perdu.
            if (Widgets.ButtonText(applyRow, "WorkStudio.Apply".Translate()))
            {
                Close();
            }
        }

        private static string Field(Rect inRect, ref float y, string label, string value)
        {
            Widgets.Label(new Rect(inRect.x, y, LabelWidth, RowHeight), label);
            var result = Widgets.TextField(
                new Rect(inRect.x + LabelWidth, y + 2f, inRect.width - LabelWidth, RowHeight - 6f),
                value ?? "");
            y += RowHeight;
            return result;
        }

        private void DrawSkills(Rect rect)
        {
            Widgets.DrawMenuSection(rect);
            var inner = rect.ContractedBy(6f);

            var skills = DefDatabase<SkillDef>.AllDefsListForReading
                .OrderBy(s => s.label ?? s.defName)
                .ToList();

            var viewRect = new Rect(0f, 0f, inner.width - 16f, skills.Count * 26f);
            Widgets.BeginScrollView(inner, ref skillScroll, viewRect);

            var y = 0f;
            foreach (var skill in skills)
            {
                var row = new Rect(0f, y, viewRect.width, 24f);
                y += 26f;

                var selected = entry.relevantSkills.Contains(skill.defName);
                var wasSelected = selected;

                Widgets.CheckboxLabeled(row, skill.LabelCap, ref selected);

                if (selected == wasSelected)
                {
                    continue;
                }

                if (selected)
                {
                    entry.relevantSkills.Add(skill.defName);
                }
                else
                {
                    entry.relevantSkills.Remove(skill.defName);
                }
            }

            Widgets.EndScrollView();
        }

        public override void PreClose()
        {
            base.PreClose();
            onChanged?.Invoke();
        }
    }
}
