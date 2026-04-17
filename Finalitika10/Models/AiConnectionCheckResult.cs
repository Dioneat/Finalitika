namespace Finalitika10.Models
{
    public sealed class AiConnectionCheckResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<AiModelOption> AvailableModels { get; set; } = new();
    }
}