using Finalitika10.Services.Ai;

namespace Finalitika10.Services
{
    public sealed class AiChatOrchestrator : IAiChatOrchestrator
    {
        private readonly IAiService _aiService;
        private readonly IAiChatContextBuilder _contextBuilder;

        public AiChatOrchestrator(
            IAiService aiService,
            IAiChatContextBuilder contextBuilder)
        {
            _aiService = aiService;
            _contextBuilder = contextBuilder;
        }

        public async Task<string> GetReplyAsync(
            string userMessage,
            string modelId,
            CancellationToken cancellationToken = default)
        {
            string context = await _contextBuilder.BuildAsync(cancellationToken);
            return await _aiService.GetFinancialAdviceAsync(userMessage, context, modelId);
        }
    }
}
