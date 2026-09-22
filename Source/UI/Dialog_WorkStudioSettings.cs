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
        // The shared settings content wraps its introduction and save note. Give both lines room
        // at 100% and 150% UI scale; the old 480x460 dialog put the close button on top of the note.
        public override Vector2 InitialSize => new Vector2(800f, 620f);

        public Dialog_WorkStudioSettings()
        {
            // The X already closes this window. A footer button steals space from the shared
            // Mod options layout and can cover its final paragraph.
            doCloseButton = false;
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
