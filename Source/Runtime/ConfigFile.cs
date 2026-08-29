using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RimWorld;
using Verse;

namespace WorkStudio
{
    /// <summary>
    /// Lecture et ecriture de la configuration dans des fichiers autonomes, a cote des
    /// sauvegardes : de quoi transporter un reglage d'une partie a l'autre, d'une machine a l'autre,
    /// ou simplement le mettre de cote avant d'essayer autre chose.
    /// </summary>
    public static class ConfigFile
    {
        private const string Extension = ".xml";
        private const string RootNode = "workStudioConfig";

        /// <summary>Dossier des configurations, cree au besoin, voisin de Saves et de Config.</summary>
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
                Log.Warning("[Work Studio] Dossier de configurations illisible : " + exception.Message);
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
                    // La version du jeu ne sert a rien au chargement, mais elle rend le fichier
                    // lisible par un humain qui se demande d'ou il sort.
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
                Log.Error("[Work Studio] Export impossible : " + exception);
                return false;
            }
        }

        /// <summary>
        /// Charge un fichier et, seulement s'il est lu en entier, l'adopte.
        /// <para>
        /// La lecture se fait dans un jeu de reglages neuf : un fichier tronque ou mal forme
        /// laisserait sinon la configuration courante a moitie remplacee, ce qui est pire que de ne
        /// rien importer du tout.
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
                Log.Error("[Work Studio] Import impossible : " + exception);
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
