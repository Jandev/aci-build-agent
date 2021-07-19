using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Orchestrator.Startup))]
namespace Orchestrator
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();

            builder.Services.AddTransient<IPipelineRunner, AzureDevOpsPipelineRunner>();
            builder.Services.AddTransient<IAgentController, AzureContainerInstanceController>();
        }
    }
}
