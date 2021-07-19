using System.Threading.Tasks;

namespace Orchestrator
{
    internal interface IAgentController
    {
        Task<int> GetNumberOfRunningBuildAgents();
        Task AddNewAgent();
    }

    class AzureContainerInstanceController : IAgentController
    {
    }
}