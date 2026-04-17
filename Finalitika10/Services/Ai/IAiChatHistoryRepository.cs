using Finalitika10.Models;

namespace Finalitika10.Services.Ai
{
    public interface IAiChatHistoryRepository
    {
        Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(string conversationId, CancellationToken cancellationToken = default);
        Task AddMessageAsync(string conversationId, ChatMessage message, CancellationToken cancellationToken = default);
        Task AddMessagesAsync(string conversationId, IEnumerable<ChatMessage> messages, CancellationToken cancellationToken = default);
        Task ClearConversationAsync(string conversationId, CancellationToken cancellationToken = default);
    }
}