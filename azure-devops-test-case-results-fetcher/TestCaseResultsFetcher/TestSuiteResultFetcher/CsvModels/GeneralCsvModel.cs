
namespace TestCaseResultsFetcher.TestSuiteResultFetcher.CsvModels
{
    public class GeneralCsvModel
    {
        public string SuiteName { get; set; }

        public int SuitId { get; set; }

        public int TotalTests { get; set; }

        public int TotalPassedTests { get; set; }

        public int TotalFailedTests { get; set; }

        public int totalUnspecifiedTests { get; set; }

        public string SuiteDuration { get; set; }

        public string PassRate { get; set; }


    }
}