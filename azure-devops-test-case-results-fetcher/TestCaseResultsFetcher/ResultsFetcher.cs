using System.Globalization;
using CsvHelper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using TestCaseResultsFetcher.AzureHttpClient;
using TestCaseResultsFetcher.TestSuiteResultFetcher.CsvModels;
using TestCaseResultsFetcher.TestSuiteResultFetcher.SuiteModels;
using TestCaseResultsFetcher.TestSuiteResultFetcher.TestSuiteModel;

namespace AzureDevOpsTestResultsFetcher.TestSuiteResultFetcher
{
    /// <summary>
    /// Gets latest test cases runs for specified suites and generates .csv files wih fetched results.
    /// How to use:
    /// 1. Specify Test Plan Id.
    /// 2. Specify Test Suites.
    /// 3. Specifiy Configuration Id in appSettings.json file. Optional. Default Configuration id is 123.
    /// 4. Specify Output Directory full path. Optional. Default value: Empty String.
    /// 5. Run FetchSuitesInfo test.
    /// 
    /// </summary>
    public class ResultsFetcher
    {
        private const string Passed = "passed";
        private const string Failed = "failed";
        private const string Unspecified = "unspecified";
        private const string ContinuationTokenName = "x-ms-continuationtoken";
        private const string AppSettings = "appsettings.json";
        private readonly uint _configurationId = GetTestConfigurationId();
        private static int TestPlanId = GetTestPlanId();

        // Default Configuration Id.
        private const uint DefaultConfiguartionId = 123;

        // Specify test suites here.
        private readonly List<Suite> _testSuites = new()
        {
            AutomatedSuiteDetails.SmokeTests,
            AutomatedSuiteDetails.RegressionTests,
            ManualSuiteDetails.ManualTests
        };

        private readonly string _outputDirectoryPath = string.Empty;

        /// <summary>
        /// Fetches latest results for provided test suites.
        /// Generates .csv file with overall and detailed results for each specified suite.
        /// </summary>
        [Test]
        public void FetchSuitesInfo()
        {
            var suiteInfo = _testSuites.Select(testSuite => (testSuite, GetSuiteInfo(testSuite.Id))).ToList();
            GenerateDetailedSuiteCsv(suiteInfo, _outputDirectoryPath);
            GenerateGeneralSuiteCsv(suiteInfo, _outputDirectoryPath);
        }

        /// <summary>
        /// Gets Test Case Results By Configuration Id.
        /// </summary>
        /// <param name="testSuiteId"></param>
        /// <returns>
        /// Filtered by specified configuration collection of Test Case Results.
        /// </returns>
        private IEnumerable<TestCaseInfo> GetSuiteInfo(int testSuiteId) => GetSuiteData(testSuiteId).Result.Value
        .Where(testCaseInfo => testCaseInfo.Configuration.Id.Equals(_configurationId))
        .OrderBy(info => info.Results.Outcome);

        /// <summary>
        /// Get Latest Test Case Results by specified Test Suite Id.
        /// </summary>
        /// <param name="testSuiteId"></param>
        /// <returns>
        /// SuiteInfo object which contains latest run details of specified suite.
        /// </returns>
        private static async Task<SuiteInfo> GetSuiteData(int testSuiteId)
        {
            var client = new AzureHttpClient();
            var continuationToken = string.Empty;
            var overallSuiteInfo = new SuiteInfo();

            do
            {
                var GetTestCaseResultsResponse = await client.GetTestCaseResults(TestPlanId, testSuiteId, continuationToken);
                continuationToken = GetContinuationTokenValue(GetTestCaseResultsResponse);
                var responseBody = await GetTestCaseResultsResponse.Content.ReadAsStringAsync();
                var suiteInfoJson = JObject.Parse(responseBody);
                var suiteInfo = JsonConvert.DeserializeObject<SuiteInfo>(suiteInfoJson!.ToString());
                overallSuiteInfo.Value.AddRange(suiteInfo!.Value);
            } while (!string.IsNullOrEmpty(continuationToken));

            return overallSuiteInfo;
        }


        /// <summary>
        /// Generates .csv file with detailed fetched results for each specified suite.
        /// </summary>
        /// <param name="suitesInfo"></param>
        /// <param name="outputDirectoryPath"></param>
        private static void GenerateDetailedSuiteCsv(
             List<(Suite suite, IEnumerable<TestCaseInfo> testCaseInfo)> suitesInfo, string outputDirectoryPath)
        {
            const string generalTestSuiteOutcomeFileName = "DetailedTestSuiteOutcome.csv";
            var filePath = string.IsNullOrEmpty(outputDirectoryPath)
            ? $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\{generalTestSuiteOutcomeFileName}"
            : $"{outputDirectoryPath}\\{generalTestSuiteOutcomeFileName}";

            using var writer = new StreamWriter(filePath);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.WriteHeader<DetailedCsvModel>();
            csv.NextRecord();
            csv.NextRecord();
            foreach (var suiteInfo in suitesInfo)
            {
                csv.WriteRecord(suiteInfo.testCaseInfo.Select(testCaseInfo =>
                {
                    var totalCaseDurationValue = testCaseInfo.Results.LastResultsDetails.Duration.ToString();
                    var totalCaseDuration = double.Parse(totalCaseDurationValue);
                    return new DetailedCsvModel
                    {
                        TestSuiteId = suiteInfo.suite.Id,
                        TestSuiteName = suiteInfo.suite.Name,
                        TestCaseId = testCaseInfo.TestCaseReference.Id,
                        Outcome = TimeSpan.FromMilliseconds(totalCaseDuration).ToString(@"mm\:ss"),
                        LastRunDate = testCaseInfo.Results.LastResultsDetails.DateCompleted.ToString("G")
                    };
                }));
                csv.NextRecord();
                csv.NextRecord();
            }
        }

        /// <summary>
        /// Generates .csv file with overall fetched results for each specified suite.
        /// </summary>
        /// <param name="suitesInfo"></param>
        /// <param name="outputDirectoryPath"></param>
        private static void GenerateGeneralSuiteCsv(
             List<(Suite suite, IEnumerable<TestCaseInfo> testCaseInfo)> suitesInfo, string outputDirectoryPath)
        {
            const string generalTestSuiteOutcomeFileName = "GeneralTestSuiteOutcome.csv";
            var filePath = string.IsNullOrEmpty(outputDirectoryPath)
            ? $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\{generalTestSuiteOutcomeFileName}"
            : $"{outputDirectoryPath}\\{generalTestSuiteOutcomeFileName}";

            using var writer = new StreamWriter(filePath);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.WriteHeader<GeneralCsvModel>();
            csv.NextRecord();
            csv.NextRecord();
            var generalCsvRecords = suitesInfo.Select(suitesInfo =>
            {
                var totalSuiteDuration = suitesInfo.testCaseInfo.Sum(testCase => testCase.Results.LastResultsDetails.Duration).ToString();
                var totalSuiteDurationValue = double.Parse(totalSuiteDuration);
                var totalSuiteDurationTimeSpan = TimeSpan.FromMilliseconds(totalSuiteDurationValue);
                var totalPassedTests = suitesInfo.testCaseInfo.Count(TestCaseInfo => TestCaseInfo.Results.Outcome.Equals(Passed));
                var totalTests = suitesInfo.testCaseInfo.Count();
                var passRate = !totalTests.Equals(0) ? totalPassedTests * 100 / totalTests : 0;
                return new GeneralCsvModel
                {
                    SuiteName = suitesInfo.suite.Name,
                    SuitId = suitesInfo.suite.Id,
                    TotalTests = totalPassedTests,
                    TotalFailedTests = suitesInfo.testCaseInfo.Count(testCaseInfo => testCaseInfo.Results.Outcome.Equals(Failed)),
                    totalUnspecifiedTests = suitesInfo.testCaseInfo.Count(testCaseInfo => testCaseInfo.Results.Outcome.Equals(Unspecified)),
                    SuiteDuration = $"{totalSuiteDurationTimeSpan.TotalMinutes:0}",
                    PassRate = $"{passRate}%"

                };
            }).ToList();
            csv.WriteRecord(generalCsvRecords);
            csv.NextRecord();
            csv.WriteHeader(typeof(OverallInfoCsvModel));
            csv.NextRecord();
            var overallInfo = GetOverallInfo(suitesInfo);
            csv.WriteRecord(overallInfo);
        }

        /// <summary>
        /// Generates overall info for specified suites.
        /// </summary>
        /// <param name="suitesInfo"></param>
        /// <returns></returns>
        private static OverallInfoCsvModel GetOverallInfo(
            List<(Suite suits, IEnumerable<TestCaseInfo> testCaseInfo)> suitesInfo)
        {
            var totalTests = suitesInfo.Sum(suitesInfo => suitesInfo.testCaseInfo.Count());
            var totalPassedTests = suitesInfo.Sum(suitesInfo => suitesInfo.testCaseInfo.Count(testCase => testCase.Results.Outcome.Equals(Passed)));
            var totalFailedTests = suitesInfo.Sum(suitesInfo => suitesInfo.testCaseInfo.Count(testCase => testCase.Results.Outcome.Equals(Failed)));
            var totalUnspecifiedTests = suitesInfo.Sum(suitesInfo => suitesInfo.testCaseInfo.Count(testCase => testCase.Results.Outcome.Equals(Unspecified)));
            var overallDuration = suitesInfo.Sum(suitesInfo => suitesInfo.testCaseInfo.Sum(testCase => testCase.Results.LastResultsDetails.Duration)).ToString();
            var overallDurationValue = double.Parse(overallDuration);
            var overallDurationTimeSpan = TimeSpan.FromMilliseconds(overallDurationValue);
            var overallPassRate = !totalTests.Equals(0) ? totalPassedTests * 100 / totalTests : 0;

            return new OverallInfoCsvModel
            {
                TotalTests = totalTests,
                TotalPassedTests = totalPassedTests,
                TotalFailedTests = totalFailedTests,
                OverallPassRate = $"{overallPassRate}%",
                OverallDuration = $"{overallDurationTimeSpan.TotalMinutes:0}"
            };
        }

        /// <summary>
        /// Gets Continuation Token from Test Suite Info Response.
        /// </summary>
        /// <param name="response"></param>
        /// <returns>
        /// Continuation Token if exists.
        /// Otherwise returns empty string.
        /// </returns>
        private static string GetContinuationTokenValue(HttpResponseMessage response)
        {
            var itExists = response.Headers.TryGetValues(ContinuationTokenName, out var continuationTokenValue);

            return itExists
            ? continuationTokenValue.Single()
            : string.Empty;
        }

        /// <summary>
        /// Gets Test Configuration Id from appSettings.json file.
        /// </summary>
        /// <returns>
        /// Test Configuration Id.
        /// </returns>
        private static uint GetTestConfigurationIdFromAppSettings()
        {
            var appSettings = JObject.Parse(File.ReadAllText(AppSettings));
            var testConfigValue = appSettings
            .SelectToken($"['Azure.RestClient'].TestConfigurationId")!
            .ToString();

            return string.IsNullOrEmpty(testConfigValue)
            ? uint.MinValue
            : uint.Parse(testConfigValue);
        }

        /// <summary>
        /// Get application configuration Id.
        /// If Configuration Id is not specified in appSettings.json file,
        /// then Default Configuration Id will be returned.
        /// </summary>
        /// <returns>
        /// Configuration Id value.
        /// </returns>
        private static uint GetTestConfigurationId()
        {
            var appSettingsConfigurationId = GetTestConfigurationIdFromAppSettings();

            return appSettingsConfigurationId.Equals(uint.MinValue)
            ? DefaultConfiguartionId
            : appSettingsConfigurationId;
        }

        /// <summary>
        /// Gets Test Plan Id from appSettings file.
        /// </summary>
        /// <returns>
        /// Test Plan Id value.
        /// </returns>
        private static int GetTestPlanId()
        {
            var appSettings = JObject.Parse(File.ReadAllText(AppSettings));
            var testPlanId = appSettings.SelectToken($"['Azure.RestClient'].TestPlanId")!.ToString();

            return string.IsNullOrEmpty(testPlanId)
            ? int.MinValue
            : int.Parse(testPlanId);
        }

    }
}