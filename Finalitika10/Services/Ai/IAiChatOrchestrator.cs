using Finalitika10.Models;

namespace Finalitika10.Services.Ai
{
    public interface IAiChatOrchestrator
    {
        Task<string> GetReplyAsync(string userMessage, string modelId, CancellationToken cancellationToken = default);
    }

}
