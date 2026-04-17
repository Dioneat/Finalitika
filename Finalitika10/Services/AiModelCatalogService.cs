using Finalitika10.Models;
using Finalitika10.Services.Ai;

namespace Finalitika10.Services
{
    public sealed class AiModelCatalogService : IAiModelCatalogService
    {
        private static readonly IReadOnlyList<AiModelOption> Models = new List<AiModelOption>
        {
            new() { DisplayName = "Google: Gemma 4 26B A4B", ModelId = "google/gemma-4-26b-a4b-it:free" },
            new() { DisplayName = "Google: Gemma 4 31B", ModelId = "google/gemma-4-31b-it:free" },
            new() { DisplayName = "MiniMax: MiniMax M2.5", ModelId = "minimax/minimax-m2.5:free" },
            new() { DisplayName = "Qwen: Qwen3 Next 80B", ModelId = "qwen/qwen3-next-80b-a3b-instruct:free" },
            new() { DisplayName = "OpenAI: GPT-OSS 120B", ModelId = "openai/gpt-oss-120b:free" },
            new() { DisplayName = "OpenAI: GPT-OSS 20B", ModelId = "openai/gpt-oss-20b:free" },
            new() { DisplayName = "Z.ai: GLM 4.5 Air", ModelId = "z-ai/glm-4.5-air:free" },
            new() { DisplayName = "Google: Gemma 3n 2B", ModelId = "google/gemma-3n-e2b-it:free" },
            new() { DisplayName = "Google: Gemma 3n 4B", ModelId = "google/gemma-3n-e4b-it:free" },
            new() { DisplayName = "Meta: Llama Guard 4 12B", ModelId = "meta-llama/llama-guard-4-12b:free" },
            new() { DisplayName = "Google: Gemma 3 4B", ModelId = "google/gemma-3-4b-it:free" },
            new() { DisplayName = "Google: Gemma 3 12B", ModelId = "google/gemma-3-12b-it:free" },
            new() { DisplayName = "Google: Gemma 3 27B", ModelId = "google/gemma-3-27b-it:free" },
            new() { DisplayName = "Meta: Llama 3.3 70B", ModelId = "meta-llama/llama-3.3-70b-instruct:free" },
            new() { DisplayName = "Meta: Llama 3.2 3B", ModelId = "meta-llama/llama-3.2-3b-instruct:free" }
        };

        public IReadOnlyList<AiModelOption> GetAvailableModels() => Models;

        public AiModelOption GetDefaultModel() =>
            Models.First(x => x.ModelId == "meta-llama/llama-3.3-70b-instruct:free");
    }
}
