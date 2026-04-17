using Finalitika10.Models;

namespace Finalitika10.Services.Ai
{
    public interface IAiService
    {
        Task<string> GetFinancialAdviceAsync(
            string userMessage,
            string systemContext,
            string modelId,
            CancellationToken cancellationToken = default);

        Task<AiConnectionCheckResult> CheckConnectionAsync(
            CancellationToken cancellationToken = default);
    }

}
