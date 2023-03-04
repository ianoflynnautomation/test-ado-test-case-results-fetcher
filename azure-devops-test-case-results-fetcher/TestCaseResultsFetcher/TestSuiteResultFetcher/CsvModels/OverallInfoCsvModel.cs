
namespace TestCaseResultsFetcher.TestSuiteResultFetcher.CsvModels
{
    public class OverallInfoCsvModel
    {
        public int TotalTests { get; set; }

        public int TotalPassedTests { get; set; }

        public int TotalFailedTests { get; set; }

        public int TotalUnspecifiedTests { get; set; }

        public string OverallPassRate { get; set; }

        public string OverallDuration { get; set; }
    }
}