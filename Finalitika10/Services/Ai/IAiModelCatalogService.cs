using Finalitika10.Models;

namespace Finalitika10.Services.Ai
{
    public interface IAiModelCatalogService
    {
        IReadOnlyList<AiModelOption> GetAvailableModels();
        AiModelOption GetDefaultModel();
    }
}
