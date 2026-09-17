using System.IO;
using System.Reflection;
using RimWorks.Pickle;
using Verse;

namespace WorkStudio.PickleSteps
{
    /// <summary>
    /// Every editor action writes Work Studio's settings to disk at once, so a scenario run on the
    /// player's own game would otherwise rewrite their work types. Each scenario starts from an
    /// empty configuration, and the player's file is put back afterwards.
    ///
    /// The backup is a file, not a copy in memory: if the game dies in the middle of a scenario, the
    /// next scenario finds the backup still there and restores it instead of overwriting it with
    /// the test configuration. Saves and exports the scenarios wrote are deleted too.
    /// </summary>
    [PickleSteps]
    public class SettingsSandbox
    {
        /// <summary>Prefix of every save and export file a scenario writes, so cleanup can find them.</summary>
        public const string FilePrefix = "pickle-workstudio-";

        private static string SettingsPath
        {
            get
            {
                var mod = WorkStudioMod.Instance;
                var method = typeof(LoadedModManager).GetMethod("GetSettingsFilename",
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                return (string)method.Invoke(null, new object[] { mod.Content.FolderName, mod.GetType().Name });
            }
        }

        private static string BackupPath => SettingsPath + ".pickle-backup";

        [BeforeScenario]
        public void IsolateSettings(PickleContext ctx)
        {
            if (File.Exists(BackupPath))
            {
                // Left behind by a run that never finished: the backup is the player's real file.
                RestoreFromBackup();
            }

            WorkStudioMod.Instance.WriteSettings();
            File.Copy(SettingsPath, BackupPath, overwrite: false);

            WorkTypeRuntime.ResetAll();
        }

        [AfterScenario]
        public void RestoreSettings(PickleContext ctx)
        {
            if (File.Exists(BackupPath))
            {
                RestoreFromBackup();
            }

            DeleteScenarioFiles();
        }

        private static void RestoreFromBackup()
        {
            // Unwind the test configuration first: created types are removed and every live pawn's
            // priorities realigned, as a reset from the settings window would.
            WorkTypeRuntime.ResetAll();

            File.Copy(BackupPath, SettingsPath, overwrite: true);
            File.Delete(BackupPath);

            var mod = WorkStudioMod.Instance;
            typeof(Mod).GetField("modSettings", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(mod, null);
            var settings = mod.GetSettings<WorkStudioSettings>();
            typeof(WorkStudioMod).GetProperty(nameof(WorkStudioMod.Settings)).SetValue(null, settings);

            WorkTypeRuntime.Apply();
        }

        private static void DeleteScenarioFiles()
        {
            foreach (var folder in new[] { GenFilePaths.SavedGamesFolderPath, ConfigFile.Folder })
            {
                if (!Directory.Exists(folder))
                {
                    continue;
                }

                foreach (var file in Directory.GetFiles(folder, FilePrefix + "*"))
                {
                    File.Delete(file);
                }
            }
        }
    }
}
