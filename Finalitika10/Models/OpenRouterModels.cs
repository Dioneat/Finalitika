using System.Text.Json.Serialization;

namespace Finalitika10.Models
{
    public class OpenRouterRequest
    {
        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("messages")]
        public List<OpenRouterMessage> Messages { get; set; } = new();
    }

    public class OpenRouterMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }

    public class OpenRouterResponse
    {
        [JsonPropertyName("choices")]
        public List<OpenRouterChoice>? Choices { get; set; }

        [JsonPropertyName("error")]
        public OpenRouterError? Error { get; set; }
    }

    public class OpenRouterChoice
    {
        [JsonPropertyName("message")]
        public OpenRouterMessage? Message { get; set; }
    }

    public class OpenRouterError
    {
        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }

    public class OpenRouterModelsUserResponse
    {
        [JsonPropertyName("data")]
        public List<OpenRouterModelItem>? Data { get; set; }
    }

    public class OpenRouterModelItem
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}