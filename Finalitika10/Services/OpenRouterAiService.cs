using Finalitika10.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Finalitika10.Services
{
    public interface IAiService
    {
        Task<string> GetFinancialAdviceAsync(string userMessage, string systemContext);
    }

    public class OpenRouterAiService : IAiService
    {
        private readonly HttpClient _httpClient;
        private const string ApiUrl = "https://openrouter.ai/api/v1/chat/completions";

        public OpenRouterAiService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "https://finalitika.app");
            _httpClient.DefaultRequestHeaders.Add("X-Title", "Finalitika App");
        }

        public async Task<string> GetFinancialAdviceAsync(string userMessage, string systemContext)
        {
            try
            {
                string token = await SecureStorage.Default.GetAsync("OpenRouterApiToken");

                if (string.IsNullOrWhiteSpace(token))
                {
                    return "❌ API-ключ не найден. Пожалуйста, укажите ключ OpenRouter в настройках приложения.";
                }

                var requestData = new OpenRouterRequest();

                requestData.Messages.Add(new OpenRouterMessage { Role = "system", Content = systemContext });
                requestData.Messages.Add(new OpenRouterMessage { Role = "user", Content = userMessage });

                string jsonContent = JsonSerializer.Serialize(requestData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsync(ApiUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    string errorJson = await response.Content.ReadAsStringAsync();
                    return $"⚠️ Ошибка 404. Детали от сервера:\n{errorJson}";
                }

                // 5. Читаем ответ
                string responseString = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<OpenRouterResponse>(responseString);

                if (responseObject?.Choices != null && responseObject.Choices.Count > 0)
                {
                    return responseObject.Choices[0].Message.Content;
                }

                return "ИИ не смог сформулировать ответ. Попробуйте перефразировать запрос.";
            }
            catch (Exception ex)
            {
                return $"🚨 Произошла ошибка сети: {ex.Message}";
            }
        }
    }
}