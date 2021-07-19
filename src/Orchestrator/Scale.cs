using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Orchestrator
{
    internal class Scale
    {
        private readonly IPipelineRunner pipelineRunner;
        private readonly IAgentController agentController;
        private readonly ILogger logger;

        /// <summary>
        /// Default is 5, but this needs to be moved to the configuration of the service.
        /// </summary>
        private int MaxNumberOfBuildAgents = 5;

        public Scale(
            IPipelineRunner pipelineRunner,
            IAgentController agentController,
            ILogger<Scale> logger)
        {
            this.pipelineRunner = pipelineRunner;
            this.agentController = agentController;
            this.logger = logger;
        }

        [FunctionName(nameof(Scale))]
        public async Task Run(
            [TimerTrigger("0 */5 * * * *", RunOnStartup = true)]
            TimerInfo myTimer)
        {
            var numberOfPendingProcesses = await GetNumberOfPendingProcesses();

            if (numberOfPendingProcesses > 0)
            {
                var runningBuildAgents = await this.agentController.GetNumberOfRunningBuildAgents();
                if (runningBuildAgents <= MaxNumberOfBuildAgents)
                {
                    await this.agentController.AddNewAgent();
                }

            }
        }


        private Task<int> GetNumberOfRunningBuildAgents()
        {
            throw new System.NotImplementedException();
        }

        private async Task<int> GetNumberOfPendingProcesses()
        {
            var numberOfPendingBuilds = await this.pipelineRunner.PendingBuilds();
            this.logger.LogInformation("`{builds}` are pending to be built.", numberOfPendingBuilds);
            var numberOfPendingReleases = await this.pipelineRunner.PendingReleases();
            this.logger.LogInformation("`{releases}` are pending to be released.", numberOfPendingReleases);

            return numberOfPendingReleases + numberOfPendingReleases;
        }
    }
}
