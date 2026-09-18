using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace WorkStudio
{
    public class WorkStudioSettings : ModSettings
    {
        /// <summary>Configuration schema version, to migrate without breaking existing settings.</summary>
        public int schemaVersion = 1;

        /// <summary>Work types created by the user. Recreated in memory at every startup.</summary>
        public List<CustomWorkTypeEntry> customTypes = new List<CustomWorkTypeEntry>();

        /// <summary>
        /// defName of a <see cref="RimWorld.WorkGiverDef"/> to the defName of the desired work type.
        /// A missing key means "keep the def's original type".
        /// </summary>
        public Dictionary<string, string> giverAssignments = new Dictionary<string, string>();

        /// <summary>
        /// defName of a <see cref="WorkTypeDef"/> to its desired natural priority. This is the only
        /// ordering lever: it drives both the column order of the Work tab and the order in which
        /// pawns pick up tasks.
        /// </summary>
        public Dictionary<string, int> priorityOverrides = new Dictionary<string, int>();

        /// <summary>
        /// defName of a <see cref="RimWorld.WorkGiverDef"/> to its desired priority within its
        /// type. This is the order in which a colonist grabs the tasks of a single job: feeding
        /// animals before shearing them, for instance.
        /// </summary>
        public Dictionary<string, int> giverOrderOverrides = new Dictionary<string, int>();

        /// <summary>defName of a <see cref="WorkTypeDef"/> to its desired label.</summary>
        public Dictionary<string, string> labelOverrides = new Dictionary<string, string>();

        /// <summary>Hidden types: no column in the Work tab.</summary>
        public List<string> hiddenTypes = new List<string>();

        /// <summary>
        /// Non-custom work types seen at the last startup, to notice that a mod has arrived or
        /// left. See <see cref="ConfigDrift"/>.
        /// </summary>
        public List<string> knownWorkTypes = new List<string>();

        /// <summary>Missing tasks already reported: the same warning is not repeated.</summary>
        public List<string> reportedMissingTasks = new List<string>();

        /// <summary>
        /// Icons in the character tab's skill list, and at the foot of each Work tab column
        /// header. Display preferences, deliberately kept out of <see cref="ExposeConfig"/>:
        /// importing someone else's setup should not silently change what your own screen draws.
        /// </summary>
        public bool showSkillIcons = true;

        public bool showWorkTypeIcons = true;

        /// <summary>One of <see cref="WorkTypeIcons"/>'s three header constants.</summary>
        public int workTabHeaderMode = WorkTypeIcons.HeaderIconAndLabel;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref schemaVersion, "schemaVersion", 1);

            ExposeConfig();

            Scribe_Collections.Look(ref knownWorkTypes, "knownWorkTypes", LookMode.Value);
            Scribe_Collections.Look(ref reportedMissingTasks, "reportedMissingTasks", LookMode.Value);

            Scribe_Values.Look(ref showSkillIcons, "showSkillIcons", true);
            Scribe_Values.Look(ref showWorkTypeIcons, "showWorkTypeIcons", true);
            Scribe_Values.Look(ref workTabHeaderMode, "workTabHeaderMode", WorkTypeIcons.HeaderIconAndLabel);
            workTabHeaderMode = Mathf.Clamp(workTabHeaderMode, WorkTypeIcons.HeaderIconAndLabel,
                WorkTypeIcons.HeaderLabelOnly);

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
        /// The portable part of the settings: everything that describes the desired configuration,
        /// and nothing that only makes sense on this installation.
        /// <para>
        /// This is exactly what an export file contains, and the reason this method exists
        /// separately: sharing <c>knownWorkTypes</c>, a snapshot of a mod list, would cry change
        /// on the very first import.
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
                // A missing node means "nothing overridden", not "keep what was there": an
                // import must replace the configuration, not blend into it.
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

        /// <summary>Takes over the configuration of another settings instance, leaving the rest untouched.</summary>
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
