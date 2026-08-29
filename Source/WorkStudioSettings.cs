using System.Collections.Generic;
using Verse;

namespace WorkStudio
{
    public class WorkStudioSettings : ModSettings
    {
        /// <summary>Version du schema de configuration, pour migrer sans casser les reglages existants.</summary>
        public int schemaVersion = 1;

        /// <summary>Types de travail crees par l'utilisateur. Recrees en memoire a chaque demarrage.</summary>
        public List<CustomWorkTypeEntry> customTypes = new List<CustomWorkTypeEntry>();

        /// <summary>
        /// defName d'un <see cref="RimWorld.WorkGiverDef"/> vers le defName du type de travail voulu.
        /// Une cle absente signifie "laisser le type d'origine du def".
        /// </summary>
        public Dictionary<string, string> giverAssignments = new Dictionary<string, string>();

        /// <summary>
        /// defName d'un <see cref="WorkTypeDef"/> vers sa priorite naturelle voulue. C'est le seul
        /// levier d'ordonnancement : il pilote a la fois l'ordre des colonnes de l'onglet Travail et
        /// l'ordre dans lequel les pions prennent les taches.
        /// </summary>
        public Dictionary<string, int> priorityOverrides = new Dictionary<string, int>();

        /// <summary>
        /// defName d'un <see cref="RimWorld.WorkGiverDef"/> vers sa priorite voulue au sein de son
        /// type. C'est l'ordre dans lequel un colon attrape les taches d'un meme travail : nourrir
        /// les animaux avant de les tondre, par exemple.
        /// </summary>
        public Dictionary<string, int> giverOrderOverrides = new Dictionary<string, int>();

        /// <summary>defName d'un <see cref="WorkTypeDef"/> vers son libelle voulu.</summary>
        public Dictionary<string, string> labelOverrides = new Dictionary<string, string>();

        /// <summary>Types masques : pas de colonne dans l'onglet Travail.</summary>
        public List<string> hiddenTypes = new List<string>();

        /// <summary>
        /// Types de travail non personnalises vus au dernier demarrage, pour reperer qu'un mod est
        /// arrive ou reparti. Voir <see cref="ConfigDrift"/>.
        /// </summary>
        public List<string> knownWorkTypes = new List<string>();

        /// <summary>Taches introuvables deja signalees : on ne repete pas le meme avertissement.</summary>
        public List<string> reportedMissingTasks = new List<string>();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref schemaVersion, "schemaVersion", 1);

            ExposeConfig();

            Scribe_Collections.Look(ref knownWorkTypes, "knownWorkTypes", LookMode.Value);
            Scribe_Collections.Look(ref reportedMissingTasks, "reportedMissingTasks", LookMode.Value);

            if (Scribe.mode == LoadSaveMode.LoadingVars || Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (knownWorkTypes == null)
                {
                    knownWorkTypes = new List<string>();
                }
                if (reportedMissingTasks == null)
                {
                    reportedMissingTasks = new List<string>();
                }
            }
        }

        /// <summary>
        /// La part transportable des reglages : tout ce qui decrit la configuration voulue, et rien
        /// de ce qui n'a de sens que sur cette installation.
        /// <para>
        /// C'est exactement ce qu'un fichier d'export contient, et la raison pour laquelle cette
        /// methode existe separement : partager <c>knownWorkTypes</c>, qui photographie une liste de
        /// mods, ferait crier au changement des le premier import.
        /// </para>
        /// </summary>
        public void ExposeConfig()
        {
            Scribe_Collections.Look(ref customTypes, "customTypes", LookMode.Deep);
            Scribe_Collections.Look(ref giverAssignments, "giverAssignments", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref priorityOverrides, "priorityOverrides", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref giverOrderOverrides, "giverOrderOverrides", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref labelOverrides, "labelOverrides", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref hiddenTypes, "hiddenTypes", LookMode.Value);

            if (Scribe.mode == LoadSaveMode.LoadingVars || Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                // Un noeud absent vaut "rien de surcharge", pas "garder ce qu'il y avait" : un
                // import doit remplacer la configuration, pas se melanger a elle.
                if (customTypes == null)
                {
                    customTypes = new List<CustomWorkTypeEntry>();
                }
                if (giverAssignments == null)
                {
                    giverAssignments = new Dictionary<string, string>();
                }
                if (priorityOverrides == null)
                {
                    priorityOverrides = new Dictionary<string, int>();
                }
                if (giverOrderOverrides == null)
                {
                    giverOrderOverrides = new Dictionary<string, int>();
                }
                if (labelOverrides == null)
                {
                    labelOverrides = new Dictionary<string, string>();
                }
                if (hiddenTypes == null)
                {
                    hiddenTypes = new List<string>();
                }
            }

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                customTypes.RemoveAll(t => t == null || t.id.NullOrEmpty());
            }
        }

        /// <summary>Reprend la configuration d'un autre jeu de reglages, sans toucher au reste.</summary>
        public void AdoptConfig(WorkStudioSettings other)
        {
            customTypes = other.customTypes;
            giverAssignments = other.giverAssignments;
            priorityOverrides = other.priorityOverrides;
            giverOrderOverrides = other.giverOrderOverrides;
            labelOverrides = other.labelOverrides;
            hiddenTypes = other.hiddenTypes;
        }
    }
}
