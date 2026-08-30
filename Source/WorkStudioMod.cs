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

            listing.Gap();

            GUI.color = new Color(1f, 1f, 1f, 0.6f);
            listing.Label("WorkStudio.Settings.SaveNote".Translate());
            GUI.color = Color.white;

            listing.End();
        }
    }
}
