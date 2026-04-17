using Finalitika10.Models;
using Finalitika10.Services.Ai;
using Finalitika10.Services.AppServices;
using Finalitika10.Settings;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Finalitika10.Services
{
    public sealed class OpenRouterAiService : IAiService
    {
        private const string DefaultBaseUrl = "https://openrouter.ai/api/v1";

        private readonly HttpClient _httpClient;
        private readonly ISecureStorageService _secureStorageService;
        private readonly IPreferencesService _preferencesService;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public OpenRouterAiService(
            HttpClient httpClient,
            ISecureStorageService secureStorageService,
            IPreferencesService preferencesService)
        {
            _httpClient = httpClient;
            _secureStorageService = secureStorageService;
            _preferencesService = preferencesService;
        }

        public async Task<string> GetFinancialAdviceAsync(
            string userMessage,
            string systemContext,
            string modelId,
            CancellationToken cancellationToken = default)
        {
            string token = await GetApiKeyAsync();
            string baseUrl = GetBaseUrl();

            var requestData = new OpenRouterRequest
            {
                Model = modelId,
                Messages =
                {
                    new OpenRouterMessage { Role = "system", Content = systemContext },
                    new OpenRouterMessage { Role = "user", Content = userMessage }
                }
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{baseUrl}/chat/completions");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.TryAddWithoutValidation("HTTP-Referer", "https://finalitika.app");
            request.Headers.TryAddWithoutValidation("X-OpenRouter-Title", "Finalitika App");

            string json = JsonSerializer.Serialize(requestData, JsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            string responseText = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string serverMessage = TryExtractErrorMessage(responseText);
                throw new InvalidOperationException(
                    $"OpenRouter вернул {(int)response.StatusCode} {response.ReasonPhrase}. {serverMessage}".Trim());
            }

            var responseObject = JsonSerializer.Deserialize<OpenRouterResponse>(responseText, JsonOptions);

            string? content = responseObject?.Choices?
                .FirstOrDefault()?
                .Message?
                .Content;

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new InvalidOperationException("OpenRouter не вернул текст ответа.");
            }

            return content;
        }

        public async Task<AiConnectionCheckResult> CheckConnectionAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                string token = await GetApiKeyAsync();
                string baseUrl = GetBaseUrl();

                using var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    $"{baseUrl}/models/user");

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Headers.TryAddWithoutValidation("HTTP-Referer", "https://finalitika.app");
                request.Headers.TryAddWithoutValidation("X-OpenRouter-Title", "Finalitika App");

                using var response = await _httpClient.SendAsync(request, cancellationToken);
                string responseText = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    return new AiConnectionCheckResult
                    {
                        IsSuccess = false,
                        Message = $"Ошибка {(int)response.StatusCode}: {TryExtractErrorMessage(responseText)}"
                    };
                }

                var parsed = JsonSerializer.Deserialize<OpenRouterModelsUserResponse>(responseText, JsonOptions);

                var availableModels = parsed?.Data?
                    .Select(x => new AiModelOption
                    {
                        ModelId = x.Id,
                        DisplayName = string.IsNullOrWhiteSpace(x.Name) ? x.Id : x.Name!
                    })
                    .ToList() ?? new List<AiModelOption>();

                return new AiConnectionCheckResult
                {
                    IsSuccess = true,
                    Message = availableModels.Count > 0
                        ? $"Подключение успешно. Доступно моделей: {availableModels.Count}"
                        : "Подключение успешно, но доступных моделей не найдено.",
                    AvailableModels = availableModels
                };
            }
            catch (Exception ex)
            {
                return new AiConnectionCheckResult
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        private async Task<string> GetApiKeyAsync()
        {
            string token = await _secureStorageService.GetAsync(SettingsKeys.OpenRouterApiToken) ?? string.Empty;

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new InvalidOperationException("API-ключ OpenRouter не найден.");
            }

            return token.Trim();
        }

        private string GetBaseUrl()
        {
            string baseUrl = _preferencesService.GetString(SettingsKeys.OpenRouterBaseUrl, DefaultBaseUrl);

            return string.IsNullOrWhiteSpace(baseUrl)
                ? DefaultBaseUrl
                : baseUrl.TrimEnd('/');
        }

        private static string TryExtractErrorMessage(string responseText)
        {
            if (string.IsNullOrWhiteSpace(responseText))
                return "Пустой ответ сервера.";

            try
            {
                var parsed = JsonSerializer.Deserialize<OpenRouterResponse>(responseText, JsonOptions);
                if (!string.IsNullOrWhiteSpace(parsed?.Error?.Message))
                {
                    return parsed.Error.Message!;
                }
            }
            catch
            {
            }

            return responseText;
        }
    }
}