using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;


namespace TestCaseResultsFetcher.AzureHttpClient
{
    /// <summary>
    /// Http Client to perform requests to ADO.
    /// </summary>
    public class AzureHttpClient
    {
        private const string AppSettings = "appSettings";
        private readonly string _personalAccessToken;
        private readonly string _organisation;
        private readonly string _project;
        private readonly HttpClient _client;

        public AzureHttpClient()
        {
            _project = GetProject();
            _organisation = GetOrganisation();
            _personalAccessToken = GetPersonalAccessToken();
            _client = InitializeHttpClient();
        }
        
        /// <summary>
        /// Gets Test Cases Info for specific suite.
        /// </summary>
        /// <param name="testPlanId"></param>
        /// Test Plan Id where Test Suite is placed.
        /// <param name="testSuiteId"></param>
        /// Test Suite from which Test Case Data should be collected.
        /// <param name="continuationToken"></param>
        /// <returns>
        /// Http Response Message with Test Cases Data for all configurations.
        /// </returns>
        public async Task<HttpResponseMessage> GetTestCaseResults(int testPlanId, int testSuiteId, string continuationToken = "") => await GetAsync(
            $"_apis/testplan/Plans/{testPlanId}/Suites/{testSuiteId}/TestPoint?continuationToken={continuationToken}");

        /// <summary>
        /// Perform GET request.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private async Task<HttpResponseMessage> GetAsync(string url)
        {
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return response;
        }
        /// <summary>
        /// Create new object of Http Client.
        /// Setups base URL and Authorization by PAT.
        /// </summary>
        /// <returns></returns>
        private HttpClient InitializeHttpClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
            client.BaseAddress = new Uri($"https://dev.azure.com/{_organisation}/{_project}/");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(
                System.Text.Encoding.ASCII.GetBytes(
                    $":{_personalAccessToken}")));

            return client;
        }

        /// <summary>
        /// Gets Personal Access Token from appSettings file.
        /// </summary>
        /// <returns>
        /// Personal Access Token for ADO
        /// </returns>
        private string GetPersonalAccessToken()
        {
            var appSettings = JObject.Parse(File.ReadAllText(AppSettings));
            return appSettings.SelectToken($"['Azure.RestClient'].PersonalAccessToken")!.ToString();
        }

        /// <summary>
        /// Gets Organisation from appSettings file.
        /// </summary>
        /// <returns>
        /// Organisation value.
        /// </returns>
        private string GetOrganisation()
        {
            var appSettings = JObject.Parse(File.ReadAllText(AppSettings));
            return appSettings.SelectToken($"['Azure.RestClient'].Organisation")!.ToString();
        }

        /// <summary>
        /// Gets Project Name from appSettings file.
        /// </summary>
        /// <returns>
        /// Project Name value.
        /// </returns>
        private string GetProject()
        {
            var appSettings = JObject.Parse(File.ReadAllText(AppSettings));
            return appSettings.SelectToken($"['Azure.RestClient'].Project")!.ToString();
        }
    }
}