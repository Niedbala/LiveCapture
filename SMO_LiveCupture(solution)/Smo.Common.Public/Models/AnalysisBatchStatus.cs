using Smo.Common.Public.Enums;

namespace Smo.Common.Public.Models
{
    public class AnalysisBatchStatus
    {
        public AnalyzerStatus AnalyzerStatus { get; set; }
        public double Percentage { get; set; }

        public int TotalCount { get; set; }
        public int AnalyzedCount { get; set; }
    }
}