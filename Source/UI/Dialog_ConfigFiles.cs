using System.Collections.Generic;
using System.IO;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace WorkStudio
{
    /// <summary>
    /// List of saved configurations: one row per file, one button to write a new one. Importing
    /// replaces the current configuration, so every destructive action goes through a
    /// confirmation.
    /// </summary>
    public class Dialog_ConfigFiles : Window
    {
        private const float RowHeight = 34f;
        private const float ButtonHeight = 30f;
        private const float DeleteSize = 24f;

        private List<FileInfo> files;
        private Vector2 scroll;

        public override Vector2 InitialSize => new Vector2(560f, 520f);

        public Dialog_ConfigFiles()
        {
            forcePause = false;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = false;
            doCloseX = true;
        }

        private List<FileInfo> Files => files ??= ConfigFile.AllFiles();

        public override void DoWindowContents(Rect inRect)
        {
            var y = inRect.y;

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(inRect.x, y, inRect.width, 34f), "WorkStudio.Presets.Title".Translate());
            Text.Font = GameFont.Small;
            y += 40f;

            var exportRow = new Rect(inRect.x, inRect.yMax - ButtonHeight, inRect.width, ButtonHeight);
            var listRect = new Rect(inRect.x, y, inRect.width, exportRow.y - 8f - y);

            DrawList(listRect);

            if (Widgets.ButtonText(exportRow, "WorkStudio.Presets.Export".Translate()))
            {
                Find.WindowStack.Add(new Dialog_TextEntry(
                    "WorkStudio.Presets.ExportName".Translate(),
                    "WorkStudio.Presets.DefaultName".Translate(),
                    Export));
            }
        }

        private void DrawList(Rect rect)
        {
            Widgets.DrawMenuSection(rect);
            var inner = rect.ContractedBy(6f);

            if (Files.Count == 0)
            {
                GUI.color = new Color(1f, 1f, 1f, 0.5f);
                Widgets.Label(inner, "WorkStudio.Presets.None".Translate(ConfigFile.Folder));
                GUI.color = Color.white;
                return;
            }

            var viewRect = new Rect(0f, 0f, inner.width - 16f, Files.Count * RowHeight);
            Widgets.BeginScrollView(inner, ref scroll, viewRect);

            var y = 0f;
            foreach (var file in Files.ToList())
            {
                var row = new Rect(0f, y, viewRect.width, RowHeight);
                y += RowHeight;

                if (Mouse.IsOver(row))
                {
                    Widgets.DrawHighlight(row);
                }

                var deleteRect = new Rect(row.xMax - DeleteSize - 2f, row.y + 5f, DeleteSize, DeleteSize);
                var labelRect = new Rect(row.x + 4f, row.y, deleteRect.x - row.x - 10f, row.height);

                var anchor = Text.Anchor;
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(labelRect, ConfigFile.NameOf(file));
                Text.Anchor = anchor;

                TooltipHandler.TipRegion(labelRect,
                    file.LastWriteTime.ToString("g") + "\n" + "WorkStudio.Presets.ImportTip".Translate());

                if (Widgets.ButtonImage(deleteRect, TexButton.Delete, Color.white, GenUI.MouseoverColor,
                        true, "WorkStudio.Presets.DeleteTip".Translate()))
                {
                    ConfirmDelete(file);
                }
                else if (Widgets.ButtonInvisible(labelRect))
                {
                    ConfirmImport(file);
                }
            }

            Widgets.EndScrollView();
        }

        private void Export(string name)
        {
            if (name.NullOrEmpty())
            {
                return;
            }

            if (ConfigFile.Exists(name))
            {
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                    "WorkStudio.Presets.ConfirmOverwrite".Translate(name),
                    () => DoExport(name),
                    destructive: true));
                return;
            }

            DoExport(name);
        }

        private void DoExport(string name)
        {
            if (ConfigFile.Export(name, out var error))
            {
                Messages.Message("WorkStudio.Presets.Exported".Translate(name), MessageTypeDefOf.TaskCompletion,
                    historical: false);
                files = null;
            }
            else
            {
                Messages.Message("WorkStudio.Presets.Failed".Translate(error), MessageTypeDefOf.RejectInput,
                    historical: false);
            }
        }

        private void ConfirmImport(FileInfo file)
        {
            var name = ConfigFile.NameOf(file);

            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                "WorkStudio.Presets.ConfirmImport".Translate(name),
                delegate
                {
                    if (ConfigFile.Import(file.FullName, out var error))
                    {
                        Messages.Message("WorkStudio.Presets.Imported".Translate(name),
                            MessageTypeDefOf.TaskCompletion, historical: false);
                        Close();
                    }
                    else
                    {
                        Messages.Message("WorkStudio.Presets.Failed".Translate(error),
                            MessageTypeDefOf.RejectInput, historical: false);
                    }
                },
                destructive: true));
        }

        private void ConfirmDelete(FileInfo file)
        {
            var name = ConfigFile.NameOf(file);

            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                "WorkStudio.Presets.ConfirmDelete".Translate(name),
                delegate
                {
                    if (ConfigFile.Delete(file, out var error))
                    {
                        files = null;
                    }
                    else
                    {
                        Messages.Message("WorkStudio.Presets.Failed".Translate(error),
                            MessageTypeDefOf.RejectInput, historical: false);
                    }
                },
                destructive: true));
        }
    }
}
