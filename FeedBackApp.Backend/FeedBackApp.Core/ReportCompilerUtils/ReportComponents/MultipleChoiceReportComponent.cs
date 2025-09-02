using FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedBackApp.Core.ReportCompilerUtils.ReportComponents
{
    public sealed class MultipleChoiceReportComponent
    {
        public required MultipleChoiceEvaluationData DataSource { get; init; }
        public MultipleChoiceReportComponent() { }
    }
}
