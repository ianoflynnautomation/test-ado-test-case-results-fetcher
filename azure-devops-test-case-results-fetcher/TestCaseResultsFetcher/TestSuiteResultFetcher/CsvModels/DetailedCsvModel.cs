
namespace TestCaseResultsFetcher.TestSuiteResultFetcher.CsvModels
{
    public class DetailedCsvModel
    {
        public string TestSuiteName { get; set; }
        public int TestSuiteId { get; set; }

        public int TestCaseId { get; set; }

        public string Outcome { get; set; }

        public string Duration { get; set; }

        public string LastRunDate { get; set; }

    }
}