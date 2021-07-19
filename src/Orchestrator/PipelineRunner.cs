using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace Orchestrator
{
    internal interface IPipelineRunner
    {
        Task<int> PendingBuilds();
        Task<int> PendingReleases();
    }

    internal class FakePipelineRunner : IPipelineRunner
    {
        /// <summary>
        /// The Azure DevOps REST API is a bit flaky, so using this for faster feedback.
        /// </summary>
        /// <returns>A small number.</returns>
        public Task<int> PendingBuilds()
        {
            return Task.FromResult(new Random().Next(0, 2));
        }

        /// <summary>
        /// The Azure DevOps REST API is a bit flaky, so using this for faster feedback.
        /// </summary>
        /// <returns>A small number.</returns>
        public Task<int> PendingReleases()
        {
            return Task.FromResult(new Random().Next(0, 2));
        }
    }

    internal class AzureDevOpsPipelineRunner : IPipelineRunner
    {
        private readonly HttpClient httpClient;

        public AzureDevOpsPipelineRunner(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        /// <summary>
        /// Makes a REST API call to Azure DevOps to query all builds with the status Not Started.
        /// </summary>
        /// <returns>The number of not started builds.</returns>
        public async Task<int> PendingBuilds()
        {
            var request = CreateAuthenticatedRequest(
                "https://dev.azure.com/janv/Livecoding/_apis/build/builds?statusFilter=notStarted&api-version=6.0");

            var response = await this.httpClient.SendAsync(request);

            var content = await response.Content.ReadAsStringAsync();
            var builds = JsonSerializer.Deserialize<Builds>(content);

            return builds.count;
        }

        /// <summary>
        /// Makes a REST API call to Azure DevOps to query all releases which are Not Started.
        /// </summary>
        /// <remarks>
        /// The queries to VSTS RM don't work properly.
        /// Waiting for an answer on my GitHub issue over here: https://github.com/MicrosoftDocs/vsts-rest-api-specs/issues/485
        /// 
        /// https://docs.microsoft.com/en-us/rest/api/azure/devops/release/releases/list?view=azure-devops-rest-6.0#releasestatus
        ///
        /// Got the following information from GitHub:
        /// environmentStatusFilter values Undefined = 0,  NotStarted = 1, InProgress = 2, Succeeded = 4, Canceled = 8, Rejected = 16, Qeued = 32, Scheduled = 64, PartiallySucceeded = 128
        ///
        /// https://vsrm.dev.azure.com/janv/Livecoding/_apis/release/releases?api-version=6.1-preview.8&$expand=environments&environmentStatusFilter=4
        /// </remarks>
        /// <returns>The number of all Not Started releases.</returns>
        public async Task<int> PendingReleases()
        {
            var request = CreateAuthenticatedRequest(
                "https://vsrm.dev.azure.com/janv/Livecoding/_apis/release/releases?api-version=6.1-preview.8&$expand=environments&environmentStatusFilter=4");
            var response = await this.httpClient.SendAsync(request);

            var content = await response.Content.ReadAsStringAsync();
            var builds = JsonSerializer.Deserialize<Builds>(content);

            return builds.count;
        }

        private HttpRequestMessage CreateAuthenticatedRequest(string url)
        {
            var credentials = Convert.ToBase64String(
                System.Text.ASCIIEncoding.ASCII.GetBytes(
                    string.Format("{0}:{1}", "", "moalox7pp5zq4l3ecgho67ihcnrofslc2bk67odvmma7tplogo7a")));

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get,
                Headers = { Authorization = new AuthenticationHeaderValue("Basic", credentials) }
            };
            return request;
        }
    }
}
