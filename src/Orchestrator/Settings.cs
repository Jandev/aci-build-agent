using System;

namespace Orchestrator
{
    internal class Settings
    {
        public string LocalDevelopment { get; }
        public string AzureDevOpsProjectUrl { get; }
        public string AzureDevOpsPAT { get; }
        public string AgentImageName { get; }
        public string AgentResourceGroup { get; }
        public string AzureTenantId { get; }
        public string AzureSubscriptionId { get; }
        public Settings()
        {
            this.LocalDevelopment = System.Environment.GetEnvironmentVariable("LocalDevelopment", EnvironmentVariableTarget.Process);
            this.AzureDevOpsProjectUrl = System.Environment.GetEnvironmentVariable("AzureDevOpsProjectUrl", EnvironmentVariableTarget.Process);
            this.AzureDevOpsPAT = System.Environment.GetEnvironmentVariable("AzureDevOpsPAT", EnvironmentVariableTarget.Process);
            this.AgentImageName = System.Environment.GetEnvironmentVariable("AgentImageName", EnvironmentVariableTarget.Process);
            this.AgentResourceGroup = System.Environment.GetEnvironmentVariable("AgentResourceGroup", EnvironmentVariableTarget.Process);
            this.AzureTenantId = System.Environment.GetEnvironmentVariable("AzureTenantId", EnvironmentVariableTarget.Process);
            this.AzureSubscriptionId = System.Environment.GetEnvironmentVariable("AzureSubscriptionId", EnvironmentVariableTarget.Process);
        }
    }
}
