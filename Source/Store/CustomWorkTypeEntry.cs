using System.Collections.Generic;
using Verse;

namespace WorkStudio
{
    /// <summary>
    /// Un type de travail cree par l'utilisateur. Les types fournis par le jeu ou par d'autres mods
    /// ne sont pas decrits ici : on les reference par leur defName et on ne surcharge que le libelle,
    /// la priorite et la visibilite.
    /// </summary>
    public class CustomWorkTypeEntry : IExposable
    {
        /// <summary>defName du <see cref="WorkTypeDef"/> recree a chaque demarrage.</summary>
        public string id;

        public string label;

        /// <summary>
        /// Libelle court, seul affiche en en-tete de colonne dans l'onglet Travail. Laisse vide, il
        /// reprend le nom complet - au risque d'une colonne large comme le titre.
        /// </summary>
        public string labelShort;

        public string description;

        /// <summary>Forme en -ant, affichee dans l'inspecteur du pion ("en train de soigner").</summary>
        public string gerundLabel;

        /// <summary>Infinitif, utilise dans les menus d'ordre direct.</summary>
        public string verb;

        /// <summary>Nom du pion qui fait ce travail ("Medecin", "Cuisinier").</summary>
        public string pawnLabel;

        /// <summary>Le travail est actif d'office chez un nouveau colon, hors quota des six premiers.</summary>
        public bool alwaysStartActive;

        /// <summary>Le jeu previent si plus aucun colon capable ne peut faire ce travail.</summary>
        public bool requireCapableColonist;

        /// <summary>defName des <see cref="RimWorld.SkillDef"/> qui comptent pour ce travail.</summary>
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
