using FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels;
using QuestPDF.Infrastructure;

namespace FeedBackApp.Core.ReportCompilerUtils.ReportComponentsModels
{
    // generikussa tettuk, hogy ne kelljen castolni es elkeruljuk a run-time hibakat, meg igy konyebb volt nekem, bocsi a bonyiert:)
    public abstract class ReportComponent<T>(T dataSource) : IComponent
        where T : EvaluationData
    {
        protected T DataSource { get; } = dataSource;

        public abstract void Compose(IContainer container);
    }
}
