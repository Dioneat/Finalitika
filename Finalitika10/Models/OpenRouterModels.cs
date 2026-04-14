using System.Text.Json.Serialization;

namespace Finalitika10.Models
{
    public class OpenRouterRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = "openai/gpt-oss-20b:free";
        [JsonPropertyName("messages")]
        public List<OpenRouterMessage> Messages { get; set; } = new();
    }

    public class OpenRouterMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } // "system", "user" или "assistant"

        [JsonPropertyName("content")]
        public string Content { get; set; }
    }

    // --- МОДЕЛИ ОТВЕТА ОТ ИИ ---
    public class OpenRouterResponse
    {
        [JsonPropertyName("choices")]
        public List<OpenRouterChoice> Choices { get; set; }
    }

    public class OpenRouterChoice
    {
        [JsonPropertyName("message")]
        public OpenRouterMessage Message { get; set; }
    }
}