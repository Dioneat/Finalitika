using Finalitika10.Models;
using Tinkoff.InvestApi;
using Tinkoff.InvestApi.V1;

namespace Finalitika10.Services.Investments
{
    public class TinkoffInvestmentService : IInvestmentService
    {
        private InvestApiClient? _apiClient;
        private string? _cachedToken;
        private readonly Dictionary<string, string> _instrumentNamesCache = new(StringComparer.Ordinal);

        public TinkoffInvestmentService()
        {
        }

        private async Task<InvestApiClient> GetClientAsync()
        {
            var token = await SecureStorage.Default.GetAsync("tinkoff_api_token");

            if (string.IsNullOrWhiteSpace(token))
                throw new UnauthorizedAccessException("Токен не найден. Добавьте его в настройках.");

            if (_apiClient is null || _cachedToken != token)
            {
                _apiClient = InvestApiClientFactory.Create(token);
                _cachedToken = token;
            }

            return _apiClient;
        }

        public async Task<PortfolioData> GetPortfolioAsync()
        {
            var client = await GetClientAsync();

            var accountsResponse = await client.Users.GetAccountsAsync(new GetAccountsRequest());

            var openAccount = accountsResponse.Accounts.FirstOrDefault(a => a.Status == AccountStatus.Open)
                              ?? throw new InvalidOperationException("Открытый брокерский счёт не найден.");

            var portfolio = await client.Operations.GetPortfolioAsync(new PortfolioRequest
            {
                AccountId = openAccount.Id
            });

            var positions = portfolio.Positions
                .Select(p => new Models.PositionData
                {
                    Figi = p.Figi,
                    InstrumentType = p.InstrumentType,
                    Name = GetInstrumentName(client, p.InstrumentType, p.Figi),
                    Quantity = ToDecimal(p.Quantity),
                    AveragePositionPrice = ToDecimal(p.AveragePositionPrice),
                    CurrentPrice = ToDecimal(p.CurrentPrice)
                })
                .ToList();

            return new PortfolioData
            {
                TotalShares = ToDecimal(portfolio.TotalAmountShares),
                TotalBonds = ToDecimal(portfolio.TotalAmountBonds),
                TotalEtfs = ToDecimal(portfolio.TotalAmountEtf),
                TotalCurrencies = ToDecimal(portfolio.TotalAmountCurrencies),
                TotalAmountPortfolio = ToDecimal(portfolio.TotalAmountPortfolio),

                ExpectedYield = portfolio.ExpectedYield,
                DailyYield = ToDecimal(portfolio.DailyYield),
                DailyYieldRelative = portfolio.DailyYieldRelative,

                Positions = positions
            };
        }

        private string GetInstrumentName(InvestApiClient client, string instrumentType, string figi)
        {
            if (string.IsNullOrWhiteSpace(figi))
                return "Актив";

            if (_instrumentNamesCache.TryGetValue(figi, out var cachedName))
                return cachedName;

            var request = new InstrumentRequest
            {
                IdType = InstrumentIdType.Figi,
                Id = figi
            };

            try
            {
                var name = instrumentType.ToLowerInvariant() switch
                {
                    "share" or "shares" => client.Instruments.ShareBy(request).Instrument.Name,
                    "bond" or "bonds" => client.Instruments.BondBy(request).Instrument.Name,
                    "etf" or "etfs" => client.Instruments.EtfBy(request).Instrument.Name,
                    "currency" or "currencies" => client.Instruments.CurrencyBy(request).Instrument.Name,
                    _ => "Неизвестный актив"
                };

                _instrumentNamesCache[figi] = name;
                return name;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при получении имени инструмента {figi}: {ex.Message}");
                return "Актив брокера";
            }
        }

        private static decimal ToDecimal(MoneyValue? value)
        {
            if (value is null)
                return 0m;

            return value.Units + value.Nano / 1_000_000_000m;
        }

        private static decimal ToDecimal(Quotation? value)
        {
            if (value is null)
                return 0m;

            return value.Units + value.Nano / 1_000_000_000m;
        }
    }
}