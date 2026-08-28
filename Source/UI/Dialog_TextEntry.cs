using System;
using UnityEngine;
using Verse;

namespace WorkStudio
{
    /// <summary>Petite fenetre de saisie d'une ligne, pour renommer un type de travail.</summary>
    public class Dialog_TextEntry : Window
    {
        private readonly string title;
        private readonly Action<string> onAccept;
        private string text;
        private bool focused;

        public override Vector2 InitialSize => new Vector2(420f, 160f);

        public Dialog_TextEntry(string title, string initial, Action<string> onAccept)
        {
            this.title = title;
            this.onAccept = onAccept;
            text = initial ?? "";

            forcePause = false;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = true;
            doCloseX = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            var font = Text.Font;
            Text.Font = GameFont.Small;

            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 28f), title);

            GUI.SetNextControlName("WorkStudioTextEntry");
            text = Widgets.TextField(new Rect(inRect.x, inRect.y + 32f, inRect.width, 30f), text);

            if (!focused)
            {
                UI.FocusControl("WorkStudioTextEntry", this);
                focused = true;
            }

            var buttonWidth = (inRect.width - 10f) / 2f;
            var buttonY = inRect.yMax - 32f;

            if (Widgets.ButtonText(new Rect(inRect.x, buttonY, buttonWidth, 30f), "Cancel".Translate()))
            {
                Close();
            }

            if (Widgets.ButtonText(new Rect(inRect.x + buttonWidth + 10f, buttonY, buttonWidth, 30f),
                    "OK".Translate()))
            {
                Accept();
            }

            if (Event.current.type == EventType.KeyDown &&
                (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter))
            {
                Event.current.Use();
                Accept();
            }

            Text.Font = font;
        }

        private void Accept()
        {
            onAccept?.Invoke(text.Trim());
            Close();
        }
    }
}
