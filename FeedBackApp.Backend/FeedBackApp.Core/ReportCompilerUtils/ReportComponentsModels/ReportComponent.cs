using FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels;
using QuestPDF.Infrastructure;

namespace FeedBackApp.Core.ReportCompilerUtils.ReportComponentsModels
{
    /// <summary>
    /// Abstract base class for report components with type-safe data binding.
    /// <para>
    /// Every concrete report component is tied to a derived <see cref="EvaluationData"/>.
    /// The generic type parameter (<typeparamref name="T"/>) ensures that the component
    /// only works with its own data type, removing the need for type casting.
    /// </para>
    /// </summary>
    /// <typeparam name="T">
    /// The data model type used by the report component.
    /// Must derive from <see cref="EvaluationData"/>.
    /// </typeparam>
    /// <remarks>
    /// Usage:
    /// <list type="number">
    /// <item>Derive from this class to create a concrete component (e.g. <c>LikertScaleReportComponent</c>).</item>
    /// <item>Specify the correct <see cref="EvaluationData"/> derivative as the generic parameter (e.g. <c>LikertScaleEvaluationData</c>).</item>
    /// <item>Implement the <see cref="Compose(IContainer)"/> method to define the component’s visual layout.</item>
    /// </list>
    /// </remarks>
    public abstract class ReportComponent<T>(T dataSource) : IComponent
        where T : EvaluationData
    {
        /// <summary>
        /// The data model associated with this component (e.g. Likert-scale data, open-ended responses).
        /// </summary>
        public T DataSource { get; } = dataSource;

        /// <summary>
        /// Defines how the component is rendered.
        /// <para>
        /// Derived classes implement this method to define the component layout
        /// using the QuestPDF <see cref="IContainer"/> API.
        /// </para>
        /// </summary>
        /// <param name="container">The QuestPDF container into which the component is rendered.</param>
        public abstract void Compose(IContainer container);
    }
}
