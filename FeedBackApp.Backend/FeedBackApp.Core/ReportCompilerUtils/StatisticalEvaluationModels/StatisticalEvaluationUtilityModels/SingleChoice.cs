namespace FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels.StatisticalEvaluationUtilityModels
{
    /// <summary>
    /// Egyválasztós (Single Choice) kérdések típusait jelölő felsorolás.
    /// </summary>
    public enum SingleChoice
    {
        /// <summary>
        /// Normál egyválasztós kérdés, előre definiált opciókkal.
        /// <para>
        /// Példa: „Melyik órát kedvelted a legjobban?” – opciók: „Matematika”, „Fizika”, „Biológia”.
        /// A válaszok eloszlása számszerűen és százalékosan kerül megjelenítésre.
        /// </para>
        /// </summary>
        REGULAR,

        /// <summary>
        /// Egyedi / szabad szöveges válaszokat engedő egyválasztós kérdés („Egyéb” típus).
        /// <para>
        /// Példa: „Melyik tantárgyat hiányolod a tantervből?” – a válaszadók saját szöveges opciót írhatnak be.
        /// Ezek a válaszok listaként jelennek meg, statisztikai eloszlás nélkül.
        /// </para>
        /// </summary>
        CUSTOM
    }
}
