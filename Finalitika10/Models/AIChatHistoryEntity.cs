using SQLite;

namespace Finalitika10.Models
{
    [Table("AiChatHistory")]
    public sealed class AIChatHistoryEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public string ConversationId { get; set; } = string.Empty;

        public bool IsUser { get; set; }

        public string? Text { get; set; }

        public string? EncryptedTextBase64 { get; set; }
        public string? NonceBase64 { get; set; }
        public string? TagBase64 { get; set; }

        public int EncryptionVersion { get; set; }

        [Indexed]
        public DateTime TimestampUtc { get; set; }
    }
}