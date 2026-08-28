using Verse;

namespace WorkStudio
{
    /// <summary>
    /// Applique la configuration une fois tous les defs charges et resolus.
    /// <para>
    /// Le moment compte : la configuration doit etre en place <b>avant</b> qu'une sauvegarde soit
    /// chargee. Les priorites de travail y sont ecrites positionnellement, alignees sur l'ordre de la
    /// <c>DefDatabase</c> ; si l'on ajoutait nos types apres coup, chaque pion recuperait les
    /// priorites de son voisin.
    /// </para>
    /// </summary>
    [StaticConstructorOnStartup]
    public static class StartupInit
    {
        static StartupInit()
        {
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                WorkTypeRuntime.Apply();

                // Apres Apply : le bilan doit porter sur le paysage tel qu'il est vraiment, une fois
                // nos types crees et les taches rangees.
                ConfigDrift.ReportAtStartup();
            });
        }
    }
}
