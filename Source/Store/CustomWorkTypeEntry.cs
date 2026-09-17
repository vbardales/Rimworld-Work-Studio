using System.Collections.Generic;
using Verse;

namespace WorkStudio
{
    /// <summary>
    /// A work type created by the user. Types provided by the game or by other mods are not
    /// described here: they are referenced by defName, and only their label, priority and
    /// visibility are overridden.
    /// </summary>
    public class CustomWorkTypeEntry : IExposable
    {
        /// <summary>defName of the <see cref="WorkTypeDef"/> recreated at every startup.</summary>
        public string id;

        public string label;

        /// <summary>
        /// Short label, the only one shown as a column header in the Work tab. Left empty, it falls
        /// back to the full name - at the risk of a column as wide as the title.
        /// </summary>
        public string labelShort;

        public string description;

        /// <summary>-ing form, shown in the pawn inspector ("treating patients").</summary>
        public string gerundLabel;

        /// <summary>Infinitive, used in the direct-order menus.</summary>
        public string verb;

        /// <summary>Name of the pawn doing this work ("Doctor", "Cook").</summary>
        public string pawnLabel;

        /// <summary>The work is active by default on a new colonist, outside the first-six quota.</summary>
        public bool alwaysStartActive;

        /// <summary>The game warns when no capable colonist can do this work any more.</summary>
        public bool requireCapableColonist;

        /// <summary>defNames of the <see cref="RimWorld.SkillDef"/>s that count for this work.</summary>
        public List<string> relevantSkills = new List<string>();

        public CustomWorkTypeEntry()
        {
        }

        public CustomWorkTypeEntry(string id, string label)
        {
            this.id = id;
            this.label = label;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref id, "id");
            Scribe_Values.Look(ref label, "label");
            Scribe_Values.Look(ref labelShort, "labelShort");
            Scribe_Values.Look(ref description, "description");
            Scribe_Values.Look(ref gerundLabel, "gerundLabel");
            Scribe_Values.Look(ref verb, "verb");
            Scribe_Values.Look(ref pawnLabel, "pawnLabel");
            Scribe_Values.Look(ref alwaysStartActive, "alwaysStartActive", false);
            Scribe_Values.Look(ref requireCapableColonist, "requireCapableColonist", false);
            Scribe_Collections.Look(ref relevantSkills, "relevantSkills", LookMode.Value);

            if (Scribe.mode == LoadSaveMode.LoadingVars && relevantSkills == null)
            {
                relevantSkills = new List<string>();
            }
        }
    }
}
