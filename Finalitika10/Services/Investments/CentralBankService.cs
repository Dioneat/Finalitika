using System.Text;
using System.Xml.Linq;

namespace Finalitika10.Services.Investments
{
    public interface ICentralBankService
    {
        Task<string> GetLatestKeyRateAsync();
    }

    public class CentralBankService : ICentralBankService
    {
        private readonly HttpClient _httpClient;

        public CentralBankService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetLatestKeyRateAsync()
        {
            try
            {
                var toDate = DateTime.Now;
                var fromDate = toDate.AddDays(-14);

                string soapRequest = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soap:Body>
    <KeyRateXML xmlns=""http://web.cbr.ru/"">
      <fromDate>{fromDate:yyyy-MM-dd}</fromDate>
      <ToDate>{toDate:yyyy-MM-dd}</ToDate>
    </KeyRateXML>
  </soap:Body>
</soap:Envelope>";

                var content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");

                content.Headers.Add("SOAPAction", "\"http://web.cbr.ru/KeyRateXML\"");

                var response = await _httpClient.PostAsync("https://www.cbr.ru/DailyInfoWebServ/DailyInfo.asmx", content);
                response.EnsureSuccessStatusCode();

                var xmlResponse = await response.Content.ReadAsStringAsync();

                var doc = XDocument.Parse(xmlResponse);

                var rateNodes = doc.Descendants().Where(x => x.Name.LocalName == "Rate").ToList();

                if (rateNodes.Any())
                {
                    string latestRate = rateNodes.FirstOrDefault().Value;
                    return $"{latestRate}%";
                }

                return "Н/Д";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при получении ставки ЦБ: {ex.Message}");
                return "Ошибка";
            }
        }
    }
}