using System.Globalization;
using System.Text.RegularExpressions;

namespace Finalitika10.Services.Investments
{
    public class MarketMoodResult
    {
        public double RviValue { get; set; }
        public string MoodText { get; set; } = string.Empty;
        public string MoodEmoji { get; set; } = string.Empty;
        public double Progress { get; set; }
        public string ColorHex { get; set; } = "#95A5A6";
    }

    public interface IMarketMoodService
    {
        Task<MarketMoodResult> GetMarketMoodAsync();
    }

    public class MarketMoodService : IMarketMoodService
    {
        private readonly HttpClient _httpClient;

        public MarketMoodService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<MarketMoodResult> GetMarketMoodAsync()
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, "https://ru.investing.com/indices/russian-vix");
                request.Headers.Add("User-Agent", "Mozilla/5.0");

                using var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var html = await response.Content.ReadAsStringAsync();

                var match = Regex.Match(html, @"составляет\s+([0-9]+,[0-9]+)", RegexOptions.IgnoreCase);

                if (!match.Success)
                {
                    throw new InvalidOperationException("Не удалось найти значение RVI на странице.");
                }

                var rviString = match.Groups[1].Value;
                var rvi = double.Parse(rviString, new CultureInfo("ru-RU"));

                return CalculateFearAndGreed(rvi);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка парсинга RVI: {ex.Message}");

                return new MarketMoodResult
                {
                    MoodText = "Нет данных",
                    MoodEmoji = "🤔",
                    Progress = 0.5,
                    ColorHex = "#95A5A6",
                    RviValue = 0
                };
            }
        }

        private static MarketMoodResult CalculateFearAndGreed(double rvi)
        {
            var result = new MarketMoodResult { RviValue = rvi };

            if (rvi < 20)
            {
                result.MoodText = "Экстремальная жадность";
                result.MoodEmoji = "🤑";
                result.ColorHex = "#27AE60";
                result.Progress = 0.9;
            }
            else if (rvi < 26)
            {
                result.MoodText = "Оптимизм";
                result.MoodEmoji = "😊";
                result.ColorHex = "#2ECC71";
                result.Progress = 0.7;
            }
            else if (rvi < 35)
            {
                result.MoodText = "Нейтрально";
                result.MoodEmoji = "😐";
                result.ColorHex = "#F39C12";
                result.Progress = 0.5;
            }
            else if (rvi < 45)
            {
                result.MoodText = "Страх";
                result.MoodEmoji = "😨";
                result.ColorHex = "#E67E22";
                result.Progress = 0.3;
            }
            else
            {
                result.MoodText = "Экстремальный страх";
                result.MoodEmoji = "😱";
                result.ColorHex = "#E74C3C";
                result.Progress = 0.1;
            }

            return result;
        }
    }
}