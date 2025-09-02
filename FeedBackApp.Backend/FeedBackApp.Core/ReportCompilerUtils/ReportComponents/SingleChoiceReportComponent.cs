using FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedBackApp.Core.ReportCompilerUtils.ReportComponents
{
    public sealed class SingleChoiceReportComponent
    {
        public required SingleChoiceEvaluationData DataSource { get; init; }
        public SingleChoiceReportComponent() { }
    }
}
