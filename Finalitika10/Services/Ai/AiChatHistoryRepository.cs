using Finalitika10.Models;

namespace Finalitika10.Services.Ai
{
    public sealed class AiChatHistoryRepository : IAiChatHistoryRepository
    {
        private const int CurrentEncryptionVersion = 1;

        private readonly FinalitikaDatabase _database;
        private readonly IMarkdownRenderer _markdownRenderer;
        private readonly IAesGcmEncryptionService _encryptionService;

        public AiChatHistoryRepository(
            FinalitikaDatabase database,
            IMarkdownRenderer markdownRenderer,
            IAesGcmEncryptionService encryptionService)
        {
            _database = database;
            _markdownRenderer = markdownRenderer;
            _encryptionService = encryptionService;
        }

        public async Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(
            string conversationId,
            CancellationToken cancellationToken = default)
        {
            var connection = await _database.GetConnectionAsync();

            var items = await connection.Table<AIChatHistoryEntity>()
                .Where(x => x.ConversationId == conversationId)
                .OrderBy(x => x.TimestampUtc)
                .ToListAsync();

            var result = new List<ChatMessage>(items.Count);

            foreach (var item in items)
            {
                string text = await ReadTextAsync(item, cancellationToken);

                result.Add(new ChatMessage
                {
                    IsUser = item.IsUser,
                    Text = text,
                    HtmlText = _markdownRenderer.ToHtml(text),
                    Timestamp = item.TimestampUtc.ToLocalTime()
                });
            }

            return result;
        }

        public async Task AddMessageAsync(
            string conversationId,
            ChatMessage message,
            CancellationToken cancellationToken = default)
        {
            var connection = await _database.GetConnectionAsync();
            var entity = await MapToEntityAsync(conversationId, message, cancellationToken);
            await connection.InsertAsync(entity);
        }

        public async Task AddMessagesAsync(
            string conversationId,
            IEnumerable<ChatMessage> messages,
            CancellationToken cancellationToken = default)
        {
            var connection = await _database.GetConnectionAsync();

            var entities = new List<AIChatHistoryEntity>();

            foreach (var message in messages)
            {
                entities.Add(await MapToEntityAsync(conversationId, message, cancellationToken));
            }

            if (entities.Count == 0)
                return;

            await connection.InsertAllAsync(entities);
        }

        public async Task ClearConversationAsync(
            string conversationId,
            CancellationToken cancellationToken = default)
        {
            var connection = await _database.GetConnectionAsync();

            await connection.ExecuteAsync(
                "DELETE FROM AiChatHistory WHERE ConversationId = ?",
                conversationId);
        }

        private async Task<AIChatHistoryEntity> MapToEntityAsync(
            string conversationId,
            ChatMessage message,
            CancellationToken cancellationToken)
        {
            var encrypted = await _encryptionService.EncryptAsync(message.Text, cancellationToken);

            return new AIChatHistoryEntity
            {
                ConversationId = conversationId,
                IsUser = message.IsUser,
                Text = null,
                EncryptedTextBase64 = encrypted.CipherTextBase64,
                NonceBase64 = encrypted.NonceBase64,
                TagBase64 = encrypted.TagBase64,
                EncryptionVersion = CurrentEncryptionVersion,
                TimestampUtc = message.Timestamp.ToUniversalTime()
            };
        }

        private async Task<string> ReadTextAsync(
            AIChatHistoryEntity entity,
            CancellationToken cancellationToken)
        {
            bool hasEncryptedPayload =
                !string.IsNullOrWhiteSpace(entity.EncryptedTextBase64) &&
                !string.IsNullOrWhiteSpace(entity.NonceBase64) &&
                !string.IsNullOrWhiteSpace(entity.TagBase64);

            if (hasEncryptedPayload)
            {
                return await _encryptionService.DecryptAsync(
                    new EncryptedData
                    {
                        CipherTextBase64 = entity.EncryptedTextBase64!,
                        NonceBase64 = entity.NonceBase64!,
                        TagBase64 = entity.TagBase64!
                    },
                    cancellationToken);
            }

            return entity.Text ?? string.Empty;
        }
    }
}