using Finalitika10.Models;
using System.Globalization;
using System.Text;
using System.Xml.Linq;

namespace Finalitika10.Services.Investments
{
    public interface ICurrencyService
    {
        Task<List<CurrencyItem>> GetAvailableCurrenciesAsync();
        Task<List<CurrencyRate>> GetRatesAsync(DateTime date);
        List<string> GetTrackedCurrencyIds();
        void AddTrackedCurrency(string id);
        void RemoveTrackedCurrency(string id);
    }

    public class CbrCurrencyService : ICurrencyService
    {
        private readonly HttpClient _httpClient;
        private const string TrackedKey = "TrackedCurrencies";

        public CbrCurrencyService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public async Task<List<CurrencyItem>> GetAvailableCurrenciesAsync()
        {
            var response = await _httpClient.GetByteArrayAsync("https://www.cbr.ru/scripts/XML_val.asp?d=0");
            var xmlString = Encoding.GetEncoding("windows-1251").GetString(response);
            var doc = XDocument.Parse(xmlString);

            return doc.Descendants("Item").Select(x => new CurrencyItem
            {
                Id = x.Attribute("ID")?.Value,
                Name = x.Element("Name")?.Value,
                EngName = x.Element("EngName")?.Value,
                Nominal = int.TryParse(x.Element("Nominal")?.Value, out int n) ? n : 1
            }).ToList();
        }

        public async Task<List<CurrencyRate>> GetRatesAsync(DateTime date)
        {
            string dateStr = date.ToString("dd/MM/yyyy");
            var response = await _httpClient.GetByteArrayAsync($"https://www.cbr.ru/scripts/XML_daily.asp?date_req={dateStr}");
            var xmlString = Encoding.GetEncoding("windows-1251").GetString(response);
            var doc = XDocument.Parse(xmlString);

            var culture = CultureInfo.GetCultureInfo("ru-RU");

            return doc.Descendants("Valute").Select(x => new CurrencyRate
            {
                Id = x.Attribute("ID")?.Value,
                CharCode = x.Element("CharCode")?.Value,
                Name = x.Element("Name")?.Value,
                Nominal = int.Parse(x.Element("Nominal")?.Value ?? "1"),
                Value = double.Parse(x.Element("Value")?.Value ?? "0", culture),
                UnitRate = double.Parse(x.Element("VunitRate")?.Value ?? "0", culture)
            }).ToList();
        }

        public List<string> GetTrackedCurrencyIds()
        {
            var saved = Preferences.Default.Get(TrackedKey, "R01235,R01239");
            return saved.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        public void AddTrackedCurrency(string id)
        {
            var tracked = GetTrackedCurrencyIds();
            if (!tracked.Contains(id))
            {
                tracked.Add(id);
                Preferences.Default.Set(TrackedKey, string.Join(",", tracked));
            }
        }

        public void RemoveTrackedCurrency(string id)
        {
            var tracked = GetTrackedCurrencyIds();
            if (tracked.Remove(id))
            {
                Preferences.Default.Set(TrackedKey, string.Join(",", tracked));
            }
        }
    }
}