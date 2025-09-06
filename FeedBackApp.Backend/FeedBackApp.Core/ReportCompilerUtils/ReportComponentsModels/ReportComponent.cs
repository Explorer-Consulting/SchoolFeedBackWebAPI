using FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels;
using QuestPDF.Infrastructure;

namespace FeedBackApp.Core.ReportCompilerUtils.ReportComponentsModels
{
    /// <summary>
    /// Absztrakt bázisosztály riportkomponensekhez, típusbiztos adatszolgáltatással.
    /// <para>
    /// Minden konkrét riportkomponens egy <see cref="EvaluationData"/> származékhoz kötődik.
    /// A generikus típusparaméter (<typeparamref name="T"/>) biztosítja, hogy a komponens
    /// csak a saját adattípusával dolgozzon, így nincs szükség típuskonverzióra (castolásra).
    /// </para>
    /// </summary>
    /// <typeparam name="T">
    /// Az adatmodell típusa, amelyből a riportkomponens építkezik.
    /// Leszármazottnak kell lennie az <see cref="EvaluationData"/> osztályból.
    /// </typeparam>
    /// <remarks>
    /// Használat:
    /// <list type="number">
    /// <item>Származtass belőle egy konkrét osztályt (pl. <c>LikertScaleReportComponent</c>).</item>
    /// <item>Add meg a megfelelő <see cref="EvaluationData"/> származékot a generikus paraméterben (pl. <c>LikertScaleEvaluationData</c>).</item>
    /// <item>A <see cref="Compose(IContainer)"/> metódusban definiáld a komponens vizuális megjelenítését.</item>
    /// </list>
    /// </remarks>
    public abstract class ReportComponent<T>(T dataSource) : IComponent
        where T : EvaluationData
    {
        /// <summary>
        /// Az adott komponenshez tartozó adattípus (pl. Likert-skála adatok, nyílt végű válaszok).
        /// </summary>
        protected T DataSource { get; } = dataSource;

        /// <summary>
        /// A komponens megjelenítésének leírása.
        /// <para>
        /// A leszármazott osztályok itt definiálják a komponens layoutját a
        /// QuestPDF <see cref="IContainer"/> API-ját használva.
        /// </para>
        /// </summary>
        /// <param name="container">A QuestPDF konténer, amelybe a komponens renderel.</param>
        public abstract void Compose(IContainer container);
    }
}
