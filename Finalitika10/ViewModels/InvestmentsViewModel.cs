using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finalitika10.Models;
using Finalitika10.Services.Investments;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Tinkoff.InvestApi.V1;
using OperationData = Finalitika10.Models.OperationData;
using Page = Microsoft.Maui.Controls.Page;

namespace Finalitika10.ViewModels
{
    public partial class InvestmentsViewModel : ObservableObject
    {
        private readonly ICurrencyService _currencyService;
        private readonly IInvestmentService _investmentService;
        private readonly ICentralBankService _centralBankService;
        private readonly IMarketMoodService _marketMoodService;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private string totalValue = "Загрузка...";

        [ObservableProperty]
        private string allTimeYield = "—";

        [ObservableProperty]
        private Color allTimeYieldColor = Colors.Gray;

        [ObservableProperty]
        private string dailyYield = "—";

        [ObservableProperty]
        private string annualYield = "Н/Д";

        [ObservableProperty]
        private string marketMood = "Загрузка...";

        [ObservableProperty]
        private string marketMoodDescription = "Ожидание данных...";

        [ObservableProperty]
        private double marketMoodProgress;

        [ObservableProperty]
        private Color marketMoodColor = Colors.Gray;

        [ObservableProperty]
        private string keyRate = "Загрузка...";

        public ObservableCollection<SmartNotification> SmartNotifications { get; } = new();
        public ObservableCollection<CurrencyRate> TrackedCurrencies { get; } = new();
        public ObservableCollection<AssetCategory> AssetAllocations { get; } = new();
        public ObservableCollection<Models.PositionData> TopAssets { get; } = new();
        public ObservableCollection<OperationData> RecentOperations { get; } = new();

        public InvestmentsViewModel(
            IInvestmentService investmentService,
            ICentralBankService centralBankService,
            ICurrencyService currencyService,
            IMarketMoodService marketMoodService)
        {
            _investmentService = investmentService;
            _centralBankService = centralBankService;
            _currencyService = currencyService;
            _marketMoodService = marketMoodService;

            _ = RefreshAsync();
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                var trackedIds = _currencyService.GetTrackedCurrencyIds();

                var ratesTask = _currencyService.GetRatesAsync(DateTime.Now);
                var moodTask = _marketMoodService.GetMarketMoodAsync();
                var keyRateTask = _centralBankService.GetLatestKeyRateAsync();
                var portfolioTask = _investmentService.GetPortfolioAsync();

                await Task.WhenAll(ratesTask, moodTask, keyRateTask, portfolioTask);

                ApplyCurrencies(ratesTask.Result, trackedIds);
                ApplyMarketMood(moodTask.Result);
                KeyRate = keyRateTask.Result;
                ApplyPortfolio(portfolioTask.Result);
                BuildNotifications();
                LoadRecentOperationsStub();
            }
            catch (UnauthorizedAccessException)
            {
                TotalValue = "Требуется токен";
                AllTimeYield = "—";
                DailyYield = "—";
                AnnualYield = "—";

                bool goToSettings = await GetCurrentPage().DisplayAlertAsync(
                    "Доступ закрыт",
                    "Для просмотра портфеля необходимо указать токен Tinkoff API в настройках.",
                    "Перейти в настройки",
                    "Отмена");

                if (goToSettings && Shell.Current is not null)
                {
                    await Shell.Current.GoToAsync(nameof(SettingsPage));
                }
            }
            catch (Exception ex)
            {
                TotalValue = "Ошибка загрузки";
                AllTimeYield = "—";
                DailyYield = "—";
                AnnualYield = "—";

                await GetCurrentPage().DisplayAlertAsync(
                    "Ошибка API",
                    $"Не удалось загрузить портфель: {ex.Message}",
                    "ОК");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ApplyCurrencies(List<CurrencyRate> allRates, List<string> trackedIds)
        {
            TrackedCurrencies.Clear();

            foreach (var rate in allRates.Where(r => trackedIds.Contains(r.Id)))
            {
                TrackedCurrencies.Add(rate);
            }
        }

        private void ApplyMarketMood(MarketMoodResult moodResult)
        {
            MarketMood = $"{moodResult.MoodEmoji} {moodResult.MoodText}";
            MarketMoodDescription = moodResult.RviValue > 0
                ? $"Индекс волатильности (RVI): {moodResult.RviValue:F2}"
                : "Нет актуальных данных по RVI";

            MarketMoodProgress = moodResult.Progress;
            MarketMoodColor = Color.FromArgb(moodResult.ColorHex);
        }

        private void ApplyPortfolio(PortfolioData data)
        {
            TotalValue = $"{data.TotalAmountPortfolio:N2} ₽";

            var dailyYieldPercent = ToDecimal(data.DailyYieldRelative);
            DailyYield = $"{FormatSignedMoney(data.DailyYield)} ({dailyYieldPercent:P2})";

            var expectedYieldValue = ToDecimal(data.ExpectedYield);
            var baseInvestedAmount = data.TotalAmountPortfolio - expectedYieldValue;

            if (baseInvestedAmount > 0)
            {
                var totalYieldPercent = expectedYieldValue / baseInvestedAmount;
                AllTimeYield = $"{totalYieldPercent:P2}";
            }
            else
            {
                AllTimeYield = "Н/Д";
            }

            AllTimeYieldColor = GetYieldColor(expectedYieldValue);

            AnnualYield = "Н/Д";

            AssetAllocations.Clear();

            if (data.TotalAmountPortfolio > 0)
            {
                AddAssetAllocation("Акции", data.TotalShares, data.TotalAmountPortfolio, "#E74C3C");
                AddAssetAllocation("Облигации", data.TotalBonds, data.TotalAmountPortfolio, "#3498DB");
                AddAssetAllocation("Фонды", data.TotalEtfs, data.TotalAmountPortfolio, "#2ECC71");
                AddAssetAllocation("Валюта", data.TotalCurrencies, data.TotalAmountPortfolio, "#F1C40F");
            }

            TopAssets.Clear();

            foreach (var position in data.Positions
                         .OrderByDescending(p => p.MarketValue)
                         .Take(3))
            {
                TopAssets.Add(position);
            }
        }

        private void AddAssetAllocation(string name, decimal amount, decimal total, string colorHex)
        {
            if (amount <= 0 || total <= 0)
                return;

            AssetAllocations.Add(new AssetCategory
            {
                Name = name,
                Amount = amount,
                Percent = amount / total * 100m,
                Color = Color.FromArgb(colorHex)
            });
        }

        private void BuildNotifications()
        {
            SmartNotifications.Clear();

            SmartNotifications.Add(new SmartNotification
            {
                Icon = "🔔",
                Title = "Портфель обновлён",
                Message = "Данные успешно загружены от брокера."
            });

            if (AllTimeYieldColor == Colors.Red)
            {
                SmartNotifications.Add(new SmartNotification
                {
                    Icon = "⚠️",
                    Title = "Портфель в просадке",
                    Message = "Проверьте структуру активов и риск-профиль."
                });
            }

            if (TrackedCurrencies.Count == 0)
            {
                SmartNotifications.Add(new SmartNotification
                {
                    Icon = "💱",
                    Title = "Валюты не выбраны",
                    Message = "Добавьте отслеживаемые валюты, чтобы видеть курсы ЦБ.",
                    ActionCommand = GoToCurrenciesCommand
                });
            }
        }

        private void LoadRecentOperationsStub()
        {
            RecentOperations.Clear();

            RecentOperations.Add(new OperationData
            {
                Date = DateTime.Now.ToString("dd.MM.yyyy"),
                Type = "Пополнение",
                Asset = "Рубли",
                Amount = "+50 000 ₽",
                Status = "Выполнено"
            });

            RecentOperations.Add(new OperationData
            {
                Date = DateTime.Now.AddDays(-2).ToString("dd.MM.yyyy"),
                Type = "Дивиденды",
                Asset = "Сбербанк",
                Amount = "+4 500 ₽",
                Status = "Зачислено"
            });
        }

        private static string FormatSignedMoney(decimal value)
        {
            return value >= 0
                ? $"+{value:N2} ₽"
                : $"{value:N2} ₽";
        }

        private static decimal ToDecimal(Quotation? value)
        {
            if (value is null)
                return 0m;

            return value.Units + value.Nano / 1_000_000_000m;
        }

        private static Color GetYieldColor(decimal yield)
        {
            if (yield > 0) return Colors.Green;
            if (yield < 0) return Colors.Red;
            return Colors.Gray;
        }

        private static Page GetCurrentPage()
        {
            return Shell.Current?.CurrentPage
                   ?? Application.Current?.Windows.FirstOrDefault()?.Page
                   ?? throw new InvalidOperationException("Активная страница не найдена.");
        }

        [RelayCommand]
        private async Task ShowAllAssetsAsync()
        {
            if (Shell.Current is not null)
                await Shell.Current.GoToAsync("AllAssetsPage");
        }

        [RelayCommand]
        private async Task GoToCurrenciesAsync()
        {
            if (Shell.Current is not null)
                await Shell.Current.GoToAsync("AddCurrencyPage");
        }

        [RelayCommand]
        private async Task ShowAllOperationsAsync()
        {
            if (Shell.Current is not null)
                await Shell.Current.GoToAsync("AllOperationsPage");
        }

        [RelayCommand]
        private async Task CheckNewsAsync()
        {
            await GetCurrentPage().DisplayAlertAsync("Новости", "Экран новостей пока в разработке.", "ОК");
        }

        [RelayCommand]
        private async Task DownloadReportAsync()
        {
            await GetCurrentPage().DisplayAlertAsync("PDF", "Генерация PDF-отчёта будет добавлена отдельным сервисом.", "ОК");
        }
    }

    public class SmartNotification
    {
        public string Icon { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public ICommand? ActionCommand { get; set; }
        public bool HasAction => ActionCommand is not null;
    }
}