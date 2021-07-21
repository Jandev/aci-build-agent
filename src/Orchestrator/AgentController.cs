using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;

namespace Orchestrator
{
    internal interface IAgentController
    {
        Task<int> GetNumberOfRunningBuildAgents();
        Task StartAgents();
        Task StopAgents();
    }

    internal class AzureContainerInstanceController : IAgentController
    {
        private static IAzure azure;
        private readonly Settings settings;
        private readonly ILogger<AzureContainerInstanceController> logger;

        public AzureContainerInstanceController(
            Settings settings,
            ILogger<AzureContainerInstanceController> logger)
        {
            this.settings = settings;
            this.logger = logger;
        }
        public async Task<int> GetNumberOfRunningBuildAgents()
        {
            string imageName = this.settings.AgentImageName;
            var azureContext = await GetAzureContext();
            int totalNumberOfAgents = 0;
            
            foreach (var containerGroup in await azureContext.ContainerGroups.ListByResourceGroupAsync(settings.AgentResourceGroup))
            {
                var allAgentContainers = containerGroup.Containers.Count(c => 
                    imageName.Equals(c.Value.Image, StringComparison.InvariantCultureIgnoreCase) &&
                    ("Waiting".Equals(c.Value.InstanceView.CurrentState.State, StringComparison.InvariantCultureIgnoreCase) ||
                     "Started".Equals(c.Value.InstanceView.CurrentState.State, StringComparison.InvariantCultureIgnoreCase)));

                totalNumberOfAgents += allAgentContainers;
            }
            this.logger.LogInformation("Found `{numberOfAgents}` ready to run.", totalNumberOfAgents);

            return totalNumberOfAgents;
        }

        public async Task StartAgents()
        {
            var azureContext = await GetAzureContext();

            foreach (var containerGroup in await azureContext.ContainerGroups.ListByResourceGroupAsync(settings.AgentResourceGroup))
            {
                this.logger.LogInformation("Starting the container group {containerGroup}", containerGroup.Name);
                // For some reason there is a `Stop` and `Restart` method on an `IContainerGroup`, but not a `Start`,
                // so I'm using the following to solve this, which invoe
                await containerGroup.Manager.ContainerGroups.StartAsync(settings.AgentResourceGroup, containerGroup.Name);
            }
        }

        public async Task StopAgents()
        {
            var azureContext = await GetAzureContext();

            foreach (var containerGroup in await azureContext.ContainerGroups.ListByResourceGroupAsync(settings.AgentResourceGroup))
            {
                this.logger.LogInformation("Stopping the container group {containerGroup}", containerGroup.Name);
                await containerGroup.StopAsync();
            }
        }


        private async Task<IAzure> GetAzureContext()
        {
            string tenantId = settings.AzureTenantId;
            string subscriptionId = settings.AzureSubscriptionId;

            if (azure != null) return azure;

            string localDevelopment = settings.LocalDevelopment;
            if (!string.IsNullOrEmpty(localDevelopment) &&
                string.Equals(localDevelopment, "true", StringComparison.InvariantCultureIgnoreCase))
            {
                this.logger.LogWarning("Logging in via local workaround.");
                // Using implementation from issue mentioned over here: https://github.com/Azure/azure-libraries-for-net/issues/585
                var astp = new AzureServiceTokenProvider();
                string graphToken = await astp.GetAccessTokenAsync("https://graph.windows.net/", tenantId);
                var astp2 = new AzureServiceTokenProvider();
                string rmToken = await astp2.GetAccessTokenAsync("https://management.azure.com/", tenantId);

                var customTokenProvider = new AzureCredentials(
                    new TokenCredentials(rmToken),
                    new TokenCredentials(graphToken),
                    tenantId,
                    AzureEnvironment.AzureGlobalCloud);

                var client = RestClient
                    .Configure()
                    .WithEnvironment(AzureEnvironment.AzureGlobalCloud)
                    .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                    .WithCredentials(customTokenProvider)
                    .Build();

                azure = Azure.Authenticate(client, tenantId).WithSubscription(subscriptionId);
                return azure;
            }
            else
            {
                this.logger.LogInformation("Logging in via MSI.");
                var credentials = new AzureCredentials(
                    new MSILoginInformation(MSIResourceType.AppService),
                    AzureEnvironment.AzureGlobalCloud,
                    tenantId);
                azure = Azure.Authenticate(credentials).WithSubscription(subscriptionId);
                return azure;
            }
        }
    }
}