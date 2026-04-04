using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Finalitika10.Models;
using Finalitika10.Services;
using Microcharts;
using SkiaSharp;
using System.Collections.ObjectModel;

namespace Finalitika10.ViewModels.AnalysisViewModels
{
    public partial class AnalysisViewModel : ObservableObject
    {
        private readonly ITransactionService _transactionService;
        private readonly IAccountService _accountService;
        private readonly ICategoryService _categoryService;

        private bool _isUpdatePending = false;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(WeatherIcon), nameof(WeatherTitle), nameof(WeatherDescription), nameof(WeatherGradientStart), nameof(WeatherGradientEnd))]
        private int financialHealthScore = 50;

        [ObservableProperty] private double expectedIncome;
        [ObservableProperty] private double expectedExpenses;

        public ObservableCollection<FinancialInsight> Insights { get; } = new();
        public ObservableCollection<HiddenExpense> HiddenExpenses { get; } = new();
        public ObservableCollection<EmotionalPurchase> EmotionalPurchases { get; } = new();
        public ObservableCollection<TaxDeductionOpportunity> TaxDeductions { get; } = new();

        public double TotalHiddenAmount => HiddenExpenses.Sum(e => e.Amount);
        public double TotalPotentialRefund => TaxDeductions.Sum(d => d.PotentialRefund);

        [ObservableProperty] private Chart cashFlowChart;
        [ObservableProperty] private Chart categoriesChart;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FabIcon))]
        private bool isFabMenuOpen = false;

        public string FabIcon => IsFabMenuOpen ? "✕" : "+";

        public AnalysisViewModel(ITransactionService transactionService, IAccountService accountService, ICategoryService categoryService)
        {
            _transactionService = transactionService;
            _accountService = accountService;
            _categoryService = categoryService;

            RequestDataUpdate();

            WeakReferenceMessenger.Default.Register<TransactionAddedMessage>(this, (r, m) => RequestDataUpdate());
        }

        private async void RequestDataUpdate()
        {
            if (_isUpdatePending) return;
            _isUpdatePending = true;

            await Task.Delay(50);

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await UpdateDynamicDataAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ANALYSIS ERROR]: {ex.Message}");
                }
                finally
                {
                    _isUpdatePending = false;
                }
            });
        }

        private string GetRelativeDate(DateTime date)
        {
            var span = DateTime.Today - date.Date;
            if (span.Days == 0) return "Сегодня";
            if (span.Days == 1) return "Вчера";
            if (span.Days < 7) return $"{span.Days} дн. назад";
            return date.ToString("dd MMM");
        }

        private async Task UpdateDynamicDataAsync()
        {
            var allTransactions = await _transactionService.GetTransactionsAsync();
            var allCategories = await _categoryService.GetCategoriesAsync();
            var currentAccounts = await _accountService.GetAccountsAsync();

            var last30DaysTxs = allTransactions.Where(t => t.Date >= DateTime.Today.AddDays(-30)).ToList();

            ExpectedIncome = last30DaysTxs.Where(t => t.Type == "Доход").Sum(t => t.Amount);
            ExpectedExpenses = last30DaysTxs.Where(t => t.Type == "Расход").Sum(t => t.Amount);

            double totalBalance = currentAccounts.Sum(a => a.Balance);

            FinancialHealthScore = CalculateHealthScore(ExpectedIncome, ExpectedExpenses, totalBalance);

            OnPropertyChanged(nameof(WeatherIcon));
            OnPropertyChanged(nameof(WeatherTitle));
            OnPropertyChanged(nameof(WeatherDescription));
            OnPropertyChanged(nameof(WeatherGradientStart));
            OnPropertyChanged(nameof(WeatherGradientEnd));

            EmotionalPurchases.Clear();

            var recentExpenses = last30DaysTxs
                .Where(t => t.Type == "Расход")
                .OrderByDescending(t => t.Date)
                .ToList();

            foreach (var t in recentExpenses)
            {
                var category = allCategories.FirstOrDefault(c => c.Id == t.CategoryId);
                string catName = category?.Name ?? "Прочее";
                string title = string.IsNullOrWhiteSpace(t.Comment) ? catName : t.Comment;

                if (t.Date.Hour >= 0 && t.Date.Hour < 6)
                {
                    EmotionalPurchases.Add(new EmotionalPurchase { Icon = "🌙", Title = title, Amount = t.Amount, DateText = GetRelativeDate(t.Date), Tag = "Ночной шопинг", TagColor = "#8E44AD" });
                    continue;
                }

                double largeThreshold = ExpectedIncome > 0 ? ExpectedIncome * 0.2 : 15000;
                if (t.Amount >= largeThreshold && catName != "Жилье" && catName != "Продукты" && catName != "Коммуналка и связь")
                {
                    EmotionalPurchases.Add(new EmotionalPurchase { Icon = "🔥", Title = title, Amount = t.Amount, DateText = GetRelativeDate(t.Date), Tag = "Крупная трата", TagColor = "#E74C3C" });
                    continue;
                }
            }

            var frequentPurchases = recentExpenses
                .GroupBy(t => new { t.Date.Date, t.CategoryId })
                .Where(g => g.Count() >= 3)
                .ToList();

            foreach (var group in frequentPurchases)
            {
                var latestTx = group.OrderByDescending(t => t.Date).First();
                var category = allCategories.FirstOrDefault(c => c.Id == latestTx.CategoryId);

                if (!EmotionalPurchases.Any(e => e.Amount == latestTx.Amount && e.Title == latestTx.Comment))
                {
                    EmotionalPurchases.Add(new EmotionalPurchase { Icon = category?.Icon ?? "🍔", Title = $"Много трат: {category?.Name ?? "Категория"}", Amount = group.Sum(t => t.Amount), DateText = GetRelativeDate(latestTx.Date), Tag = "Частые траты", TagColor = "#F39C12" });
                }
            }

            TaxDeductions.Clear();
            var currentYear = DateTime.Today.Year;
            var currentYearExpenses = allTransactions.Where(t => t.Type == "Расход" && t.Date.Year == currentYear).ToList();

            double healthAmount = 0, educationAmount = 0, fitnessAmount = 0, investAmount = 0;

            foreach (var t in currentYearExpenses)
            {
                var category = allCategories.FirstOrDefault(c => c.Id == t.CategoryId);
                string catName = category?.Name ?? "";
                string comment = t.Comment?.ToLower() ?? "";

                if (catName == "Здоровье" || comment.Contains("стоматолог") || comment.Contains("анализ") || comment.Contains("аптека") || comment.Contains("инвитро")) healthAmount += t.Amount;
                else if (catName == "Образование" || comment.Contains("курс") || comment.Contains("обучение") || comment.Contains("школа") || comment.Contains("университет")) educationAmount += t.Amount;
                else if (comment.Contains("фитнес") || comment.Contains("спортзал") || comment.Contains("бассейн") || comment.Contains("абонемент")) fitnessAmount += t.Amount;
                else if (catName == "Инвестиции" || comment.Contains("иис") || comment.Contains("брокер")) investAmount += t.Amount;
            }

            if (healthAmount > 0) TaxDeductions.Add(new TaxDeductionOpportunity { Icon = "🏥", Title = "Медицина и лекарства", Description = "Приемы врачей, анализы", EligibleAmount = healthAmount });
            if (educationAmount > 0) TaxDeductions.Add(new TaxDeductionOpportunity { Icon = "🎓", Title = "Образование", Description = "Оплата курсов, ВУЗа", EligibleAmount = educationAmount });
            if (fitnessAmount > 0) TaxDeductions.Add(new TaxDeductionOpportunity { Icon = "🏋️", Title = "Спорт и фитнес", Description = "Абонементы в спортзал", EligibleAmount = fitnessAmount });
            if (investAmount > 0) TaxDeductions.Add(new TaxDeductionOpportunity { Icon = "📈", Title = "Пополнение ИИС", Description = "Инвестиционный вычет (Тип А)", EligibleAmount = investAmount });

            OnPropertyChanged(nameof(TotalPotentialRefund));

            HiddenExpenses.Clear();
            var foundHiddenExpenses = new Dictionary<string, HiddenExpense>();

            foreach (var t in recentExpenses)
            {
                var category = allCategories.FirstOrDefault(c => c.Id == t.CategoryId);
                string catName = category?.Name ?? "";
                string desc = t.Comment?.ToLower() ?? "";
                string cleanTitle = string.IsNullOrWhiteSpace(t.Comment) ? catName : t.Comment.Split('(')[0].Trim();

                bool isFee = catName.Contains("Комисси") || desc.Contains("комисси") || desc.Contains("обслуживание");
                bool isSubscription = catName.Contains("Подписк") || desc.Contains("подписк") || desc.Contains("premium") || desc.Contains("plus") || desc.Contains("music") || desc.Contains("netflix");

                if (isFee || isSubscription)
                {
                    if (!foundHiddenExpenses.ContainsKey(cleanTitle))
                    {
                        foundHiddenExpenses[cleanTitle] = new HiddenExpense { Icon = isFee ? "🏦" : "🎵", Title = cleanTitle, Amount = t.Amount, Frequency = "в месяц", StatusColor = isFee ? "#E74C3C" : "#7F8C8D" };
                    }
                    else foundHiddenExpenses[cleanTitle].Amount += t.Amount;
                }
            }

            foreach (var expense in foundHiddenExpenses.Values.OrderByDescending(e => e.Amount))
                HiddenExpenses.Add(expense);

            OnPropertyChanged(nameof(TotalHiddenAmount));
            GenerateSmartInsights(allTransactions, currentAccounts, allCategories);

            var cashFlowEntries = new List<ChartEntry>();
            var months = Enumerable.Range(0, 3).Select(i => DateTime.Today.AddMonths(-i)).Reverse().ToList();

            foreach (var month in months)
            {
                var monthTx = allTransactions.Where(t => t.Date.Year == month.Year && t.Date.Month == month.Month);
                double income = monthTx.Where(t => t.Type == "Доход").Sum(t => t.Amount);
                double expense = monthTx.Where(t => t.Type == "Расход").Sum(t => t.Amount);
                string monthLabel = month.ToString("MMM");

                if (income > 0 || expense > 0)
                {
                    cashFlowEntries.Add(new ChartEntry((float)income) { Label = monthLabel, ValueLabel = income >= 1000 ? $"{income / 1000:F0}к" : income.ToString(), Color = SKColor.Parse("#27AE60") });
                    cashFlowEntries.Add(new ChartEntry((float)expense) { Label = monthLabel, ValueLabel = expense >= 1000 ? $"{expense / 1000:F0}к" : expense.ToString(), Color = SKColor.Parse("#E74C3C") });
                }
            }

            if (!cashFlowEntries.Any()) cashFlowEntries.Add(new ChartEntry(0) { Label = "Нет данных", ValueLabel = "0", Color = SKColor.Parse("#BDC3C7") });
            CashFlowChart = new BarChart { Entries = cashFlowEntries, BackgroundColor = SKColors.White, LabelTextSize = 32, LabelOrientation = Orientation.Horizontal, ValueLabelOrientation = Orientation.Horizontal, Margin = 20 };

            var groupedExpenses = recentExpenses
                .GroupBy(t => t.CategoryId)
                .Select(g => new { CategoryId = g.Key, Total = g.Sum(t => t.Amount) })
                .OrderByDescending(x => x.Total)
                .ToList();

            var categoryEntries = new List<ChartEntry>();

            foreach (var exp in groupedExpenses)
            {
                var category = allCategories.FirstOrDefault(c => c.Id == exp.CategoryId);
                string label = category?.Name ?? "Прочее (Удалено)";
                string hexColor = category?.ColorHex ?? "#95A5A6";

                if (!hexColor.StartsWith("#")) hexColor = "#" + hexColor;

                if (!SKColor.TryParse(hexColor, out SKColor parsedColor))
                {
                    parsedColor = SKColor.Parse("#95A5A6");
                }

                categoryEntries.Add(new ChartEntry((float)exp.Total)
                {
                    Label = label,
                    ValueLabel = exp.Total >= 1000 ? $"{exp.Total / 1000:F0}к" : exp.Total.ToString(),
                    Color = parsedColor
                });
            }

            if (!categoryEntries.Any())
            {
                categoryEntries.Add(new ChartEntry(1) { Label = "Нет трат", ValueLabel = "0", Color = SKColor.Parse("#BDC3C7") });
            }

            CategoriesChart = new DonutChart { Entries = categoryEntries, BackgroundColor = SKColors.White, LabelTextSize = 32, HoleRadius = 0.6f };
        }

        public string WeatherIcon => FinancialHealthScore >= 80 ? "☀️" : FinancialHealthScore >= 50 ? "⛅" : FinancialHealthScore >= 30 ? "🌧️" : "⛈️";
        public string WeatherTitle => FinancialHealthScore >= 80 ? "Финансовое лето" : FinancialHealthScore >= 50 ? "Переменная облачность" : FinancialHealthScore >= 30 ? "Затягиваем пояса" : "Финансовый шторм";
        public string WeatherDescription => FinancialHealthScore >= 80 ? "Доходы стабильно превышают расходы." : FinancialHealthScore >= 50 ? "Ситуация под контролем, но есть риск." : FinancialHealthScore >= 30 ? "Расходы догоняют доходы." : "Внимание: кассовый разрыв.";
        public string WeatherGradientStart => FinancialHealthScore >= 80 ? "#4facfe" : FinancialHealthScore >= 50 ? "#a1c4fd" : "#7f8c8d";
        public string WeatherGradientEnd => FinancialHealthScore >= 80 ? "#00f2fe" : FinancialHealthScore >= 50 ? "#c2e9fb" : "#2c3e50";

        private int CalculateHealthScore(double income, double expenses, double totalBalance)
        {
            if (income == 0 && expenses == 0) return 50;
            int score = 50;

            if (income > 0)
            {
                double savingsRatio = (income - expenses) / income;
                score = savingsRatio >= 0.30 ? 95 : savingsRatio >= 0.15 ? 85 : savingsRatio >= 0.05 ? 70 : savingsRatio >= 0 ? 60 : savingsRatio >= -0.20 ? 40 : 20;
            }
            else if (expenses > 0) score = 30;

            if (totalBalance < 0) score -= 20;
            else if (expenses > 0)
            {
                double cushionRatio = totalBalance / expenses;
                score += cushionRatio >= 6 ? 15 : cushionRatio >= 3 ? 10 : cushionRatio >= 1 ? 5 : (cushionRatio < 0.2 ? -5 : 0);
            }
            return Math.Clamp(score, 0, 100);
        }

        private void GenerateSmartInsights(List<TransactionRecord> transactions, List<BankAccount> accounts, List<TransactionCategory> categories)
        {
            Insights.Clear();
            var currentMonthTxs = transactions.Where(t => t.Date >= DateTime.Today.AddDays(-30)).ToList();
            double totalExpenses = currentMonthTxs.Where(t => t.Type == "Расход").Sum(t => t.Amount);
            double totalIncome = currentMonthTxs.Where(t => t.Type == "Доход").Sum(t => t.Amount);
            double totalBalance = accounts.Sum(a => a.Balance);

            if (totalBalance >= 50000)
            {
                Insights.Add(new FinancialInsight { Icon = "📈", Title = "Деньги простаивают", Description = $"У вас {totalBalance:N0} ₽. На вкладе под 15% вы получите +{(totalBalance * 0.15):N0} ₽ за год.", ActionText = "Открыть вклад", BackgroundColor = "#E8F8F5", TextColor = "#16A085" });
            }

            var fastfoodCategory = categories.FirstOrDefault(c => c.Name.Contains("Кафе") || c.Name.Contains("Фастфуд"));
            if (fastfoodCategory != null)
            {
                double fastfoodSpends = currentMonthTxs.Where(t => t.CategoryId == fastfoodCategory.Id).Sum(t => t.Amount);
                if (fastfoodSpends > 3000) Insights.Add(new FinancialInsight { Icon = "☕", Title = "Скрытый потенциал", Description = $"Вы потратили {fastfoodSpends:N0} ₽ на кафе. Сократив вдвое, за год накопите {((fastfoodSpends / 2) * 12):N0} ₽!", ActionText = "Перевести в копилку", BackgroundColor = "#FFF8E1", TextColor = "#F39C12" });
            }

            if (totalIncome > 0 && totalExpenses > 0)
            {
                double savingsRate = (totalIncome - totalExpenses) / totalIncome;
                if (savingsRate >= 0.20 && savingsRate <= 0.80) Insights.Add(new FinancialInsight { Icon = "🌟", Title = "Отличная дисциплина!", Description = $"Вы сохранили {(savingsRate * 100):N0}% дохода. Самое время направить их в инвестиции.", ActionText = "Купить активы", BackgroundColor = "#E3F2FD", TextColor = "#2980B9" });
            }

            if (!Insights.Any()) Insights.Add(new FinancialInsight { Icon = "💡", Title = "Умный алгоритм обучается", Description = "Продолжайте вести учет. ИИ скоро найдет для вас персональные точки роста.", ActionText = "Добавить расход", BackgroundColor = "#F4F6F8", TextColor = "#7F8C8D" });
        }

        [RelayCommand] private void ToggleFabMenu() => IsFabMenuOpen = !IsFabMenuOpen;
        [RelayCommand] private async Task OpenImportAsync() { IsFabMenuOpen = false; await Shell.Current.GoToAsync("ImportPage"); }
        [RelayCommand] private async Task AddTransactionAsync(string type) { IsFabMenuOpen = false; await Shell.Current.Navigation.PushModalAsync(new Views.AddTransactionPage(new AddTransactionViewModel(_transactionService, _accountService, _categoryService, type))); }
        [RelayCommand] private async Task AddTransferAsync() { IsFabMenuOpen = false; await Shell.Current.Navigation.PushModalAsync(new Views.AddTransferPage(new AddTransferViewModel(_transactionService, _accountService))); }
        [RelayCommand] private async Task AddAccountAsync() { IsFabMenuOpen = false; await Shell.Current.GoToAsync("AccountsPage"); }
        [RelayCommand] private async Task AddCategoryAsync() { IsFabMenuOpen = false; await Shell.Current.GoToAsync("CategoriesPage"); }
        [RelayCommand] private async Task GenerateTaxReportAsync() { await Shell.Current.DisplayAlertAsync("Подготовка 3-НДФЛ", "Переход на вкладку 'Документы'. Приложение автоматически сформирует PDF-справку и реестр чеков.", "Отлично!"); }
    }
}