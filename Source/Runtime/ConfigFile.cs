using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RimWorld;
using Verse;

namespace WorkStudio
{
    /// <summary>
    /// Reads and writes the configuration to standalone files, next to the saves: a way to carry a
    /// setup from one game to another, from one machine to another, or simply to set it aside
    /// before trying something else.
    /// </summary>
    public static class ConfigFile
    {
        private const string Extension = ".xml";
        private const string RootNode = "workStudioConfig";

        /// <summary>Configurations folder, created when needed, next to Saves and Config.</summary>
        public static string Folder => GenFilePaths.FolderUnderSaveData("WorkStudio");

        public static List<FileInfo> AllFiles()
        {
            try
            {
                return new DirectoryInfo(Folder)
                    .GetFiles("*" + Extension)
                    .OrderByDescending(file => file.LastWriteTime)
                    .ToList();
            }
            catch (Exception exception)
            {
                Log.Warning("[Work Studio] Could not read the setups folder: " + exception.Message);
                return new List<FileInfo>();
            }
        }

        public static string NameOf(FileInfo file) => Path.GetFileNameWithoutExtension(file.Name);

        public static string PathFor(string name)
        {
            return Path.Combine(Folder, GenText.SanitizeFilename(name) + Extension);
        }

        public static bool Exists(string name) => File.Exists(PathFor(name));

        public static bool Export(string name, out string error)
        {
            error = null;

            try
            {
                Scribe.saver.InitSaving(PathFor(name), RootNode);
                try
                {
                    // The game version is useless on load, but it makes the file readable by a
                    // human wondering where it came from.
                    var version = VersionControl.CurrentVersionStringWithRev;
                    Scribe_Values.Look(ref version, "gameVersion");

                    WorkStudioMod.Settings.ExposeConfig();
                }
                finally
                {
                    Scribe.saver.FinalizeSaving();
                }

                return true;
            }
            catch (Exception exception)
            {
                Scribe.ForceStop();
                error = exception.Message;
                Log.Error("[Work Studio] Export failed: " + exception);
                return false;
            }
        }

        /// <summary>
        /// Loads a file and adopts it, but only if it was read in full.
        /// <para>
        /// Reading goes into a fresh settings instance: otherwise a truncated or malformed file
        /// would leave the current configuration half replaced, which is worse than importing
        /// nothing at all.
        /// </para>
        /// </summary>
        public static bool Import(string path, out string error)
        {
            error = null;

            try
            {
                var incoming = new WorkStudioSettings();

                Scribe.loader.InitLoading(path);
                try
                {
                    incoming.ExposeConfig();
                }
                finally
                {
                    Scribe.loader.FinalizeLoading();
                }

                WorkStudioMod.Settings.AdoptConfig(incoming);
                WorkStudioMod.Instance.WriteSettings();
                WorkTypeRuntime.Apply();

                return true;
            }
            catch (Exception exception)
            {
                Scribe.ForceStop();
                error = exception.Message;
                Log.Error("[Work Studio] Import failed: " + exception);
                return false;
            }
        }

        public static bool Delete(FileInfo file, out string error)
        {
            error = null;

            try
            {
                file.Delete();
                return true;
            }
            catch (Exception exception)
            {
                error = exception.Message;
                return false;
            }
        }
    }
}
