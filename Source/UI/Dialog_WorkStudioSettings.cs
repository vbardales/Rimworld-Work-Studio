using UnityEngine;
using Verse;

namespace WorkStudio
{
    /// <summary>
    /// The window the MainButtons shortcut opens. It draws the exact same
    /// <see cref="WorkStudioMod.DoSettingsWindowContents"/> as Mod options -> Work Studio, against
    /// the same <see cref="WorkStudioMod.Settings"/> instance, so both routes share values and
    /// persistence - this is a second door onto the same room, not a second room.
    /// </summary>
    public class Dialog_WorkStudioSettings : Window
    {
        public override Vector2 InitialSize => new Vector2(480f, 460f);

        public Dialog_WorkStudioSettings()
        {
            doCloseButton = true;
            doCloseX = true;
            forcePause = true;
            absorbInputAroundWindow = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            WorkStudioMod.Instance.DoSettingsWindowContents(inRect);
        }
    }
}
