using HarmonyLib;
using UnityEngine;
using Verse;

namespace WorkStudio
{
    public class WorkStudioMod : Mod
    {
        public const string HarmonyId = "nelim.workstudio";

        public static WorkStudioMod Instance { get; private set; }
        public static WorkStudioSettings Settings { get; private set; }
        public static Harmony HarmonyInstance { get; private set; }

        public WorkStudioMod(ModContentPack content) : base(content)
        {
            Instance = this;
            Settings = GetSettings<WorkStudioSettings>();

            HarmonyInstance = new Harmony(HarmonyId);
            HarmonyInstance.PatchAll();
        }

        public override string SettingsCategory() => "Work Studio";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var listing = new Listing_Standard();
            listing.Begin(inRect);

            listing.Label("WorkStudio.Settings.Intro".Translate());
            listing.Gap();

            if (listing.ButtonText("WorkStudio.Settings.OpenEditor".Translate()))
            {
                Find.WindowStack.Add(new Dialog_WorkTypes());
            }

            listing.Gap();

            if (listing.ButtonText("WorkStudio.Settings.Presets".Translate()))
            {
                Find.WindowStack.Add(new Dialog_ConfigFiles());
            }

            if (WorkTypeRuntime.HasOverrides() && listing.ButtonText("WorkStudio.Settings.ResetAll".Translate()))
            {
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                    "WorkStudio.Settings.ConfirmResetAll".Translate(),
                    WorkTypeRuntime.ResetAll,
                    destructive: true));
            }

            listing.GapLine(10f);

            listing.Label("WorkStudio.Icons.Section".Translate());
            listing.CheckboxLabeled("WorkStudio.Icons.Skills".Translate(), ref Settings.showSkillIcons,
                "WorkStudio.Icons.SkillsDesc".Translate());
            listing.CheckboxLabeled("WorkStudio.Icons.WorkTypes".Translate(), ref Settings.showWorkTypeIcons,
                "WorkStudio.Icons.WorkTypesDesc".Translate());

            listing.Gap(4f);
            listing.Label("WorkStudio.Icons.HeaderMode".Translate());

            if (!Settings.showWorkTypeIcons)
            {
                // The three choices below only decide how an icon shares the header with its label,
                // so they mean nothing while no icon is drawn there. Saying so beats offering
                // controls that do nothing, which is what MOD_SETTINGS.md asks of a dependent
                // control.
                GUI.color = new Color(1f, 1f, 1f, 0.6f);
                listing.Label("WorkStudio.Icons.HeaderModeUnavailable".Translate());
                GUI.color = Color.white;
            }
            else
            {
                if (listing.RadioButton("WorkStudio.Icons.HeaderBoth".Translate(),
                        Settings.workTabHeaderMode == WorkTypeIcons.HeaderIconAndLabel, 8f,
                        "WorkStudio.Icons.HeaderBothDesc".Translate()))
                {
                    Settings.workTabHeaderMode = WorkTypeIcons.HeaderIconAndLabel;
                }

                if (listing.RadioButton("WorkStudio.Icons.HeaderIcon".Translate(),
                        Settings.workTabHeaderMode == WorkTypeIcons.HeaderIconOnly, 8f,
                        "WorkStudio.Icons.HeaderIconDesc".Translate()))
                {
                    Settings.workTabHeaderMode = WorkTypeIcons.HeaderIconOnly;
                }

                if (listing.RadioButton("WorkStudio.Icons.HeaderLabel".Translate(),
                        Settings.workTabHeaderMode == WorkTypeIcons.HeaderLabelOnly, 8f,
                        "WorkStudio.Icons.HeaderLabelDesc".Translate()))
                {
                    Settings.workTabHeaderMode = WorkTypeIcons.HeaderLabelOnly;
                }
            }


            listing.Gap();

            GUI.color = new Color(1f, 1f, 1f, 0.6f);
            listing.Label("WorkStudio.Settings.SaveNote".Translate());
            GUI.color = Color.white;

            listing.End();
        }
    }
}
