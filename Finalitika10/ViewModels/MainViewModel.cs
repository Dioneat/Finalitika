using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Finalitika10.Models;
using Finalitika10.Services;
using Finalitika10.Services.PlanServices;
using Finalitika10.ViewModels.AnalysisViewModels;
using Finalitika10.ViewModels.PlanViewModels;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Finalitika10.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ITransactionService _transactionService;
        private readonly IAccountService _accountService;
        private readonly ICategoryService _categoryService;
        private readonly IProjectService _projectService;
        private readonly IPaymentService _paymentService;
        private readonly IJobProfileService _jobProfileService;

        [ObservableProperty] private string currentPeriod = "";
        [ObservableProperty] private string balanceText = "0 ₽";
        private DateTime _selectedMonth = DateTime.Today;

        private bool _isUpdatePending = false;

        // --- БЮДЖЕТ ДНЯ ---
        [ObservableProperty] private double dailyBudgetLimit = 2500;
        [ObservableProperty] private double dailyBudgetSpent = 850;
        [ObservableProperty] private bool isGoalsVisible = true;
        public ObservableCollection<FinancialGoal> ActiveGoals { get; } = new();

        public double DailyBudgetProgress => Math.Min(DailyBudgetSpent / DailyBudgetLimit, 1.0);
        public string DailyBudgetColor => DailyBudgetSpent > DailyBudgetLimit ? "#E74C3C" : "#27AE60";
        public string DailyBudgetMessage => DailyBudgetSpent > DailyBudgetLimit ? "🔥 Лимит превышен!" : "🌟 Отличный темп!";

        public ObservableCollection<BankAccount> ActiveAccounts { get; } = new();

        // --- НАСТРОЙКИ ВИДИМОСТИ ---
        [ObservableProperty] private bool isDailyBudgetVisible = true;
        [ObservableProperty] private bool isAccountsVisible = true;
        [ObservableProperty] private bool isPaymentCalendarVisible = true;
        public ObservableCollection<PaymentEvent> UpcomingPayments { get; } = new();

        // --- ПРЕДУПРЕЖДЕНИЕ О КАССОВОМ РАЗРЫВЕ ---
        [ObservableProperty] private bool hasCashGapWarning;
        [ObservableProperty] private string cashGapMessage = "";

        // --- ФИНАНСОВЫЙ ОТЧЕТ НЕДЕЛИ ---
        [ObservableProperty] private bool isWeeklyReportVisible = true;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(WeeklyBudgetProgress), nameof(WeeklyPercentageText), nameof(WeeklyBudgetColor))]
        private double weeklyEarned = 45000;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(WeeklyBudgetProgress), nameof(WeeklyPercentageText), nameof(WeeklyBudgetColor))]
        private double weeklySpent = 12500;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(WeeklyBudgetProgress), nameof(WeeklyPercentageText), nameof(WeeklyBudgetColor))]
        private double weeklyBudgetLimit = 15000;

        public double WeeklyBudgetProgress => WeeklyBudgetLimit > 0 ? Math.Min(WeeklySpent / WeeklyBudgetLimit, 1.0) : 0;
        public string WeeklyPercentageText => WeeklyBudgetLimit > 0 ? $"{(WeeklySpent / WeeklyBudgetLimit * 100):F0}%" : "0%";
        public string WeeklyBudgetColor => WeeklySpent > WeeklyBudgetLimit ? "#E74C3C" : "#8E44AD";

        [RelayCommand] private void ToggleWeeklyReport() => IsWeeklyReportVisible = !IsWeeklyReportVisible;

        // --- FAB МЕНЮ ---
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FabIcon))]
        private bool isFabMenuOpen = false;

        public string FabIcon => IsFabMenuOpen ? "✕" : "+";

        public MainViewModel(ITransactionService transactionService, IAccountService accountService, ICategoryService categoryService, IProjectService projectService, IPaymentService paymentService, IJobProfileService jobProfileService)
        {
            _transactionService = transactionService;
            _accountService = accountService;
            _categoryService = categoryService;
            _projectService = projectService;
            _paymentService = paymentService;
            _jobProfileService = jobProfileService;

            RequestDataUpdate();

            WeakReferenceMessenger.Default.Register<AccountUpdatedMessage>(this, (r, m) => RequestDataUpdate());
            WeakReferenceMessenger.Default.Register<TransactionAddedMessage>(this, (r, m) => RequestDataUpdate());
            WeakReferenceMessenger.Default.Register<PaymentAddedMessage>(this, (r, m) => RequestDataUpdate());
            WeakReferenceMessenger.Default.Register<SalaryCalculatedMessage>(this, (r, m) => RequestDataUpdate());
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
                finally
                {
                    _isUpdatePending = false;
                }
            });
        }

        private async Task UpdateDynamicDataAsync()
        {
            var culture = new CultureInfo("ru-RU");
            string monthName = _selectedMonth.ToString("MMMM yyyy", culture);
            CurrentPeriod = char.ToUpper(monthName[0]) + monthName.Substring(1);

            ActiveAccounts.Clear();

            var accounts = await _accountService.GetAccountsAsync();
            double totalBalance = 0;

            foreach (var acc in accounts)
            {
                ActiveAccounts.Add(acc);
                totalBalance += acc.Balance;
            }

            bool isPrivacyMode = Preferences.Default.Get("PrivacyMode", false);
            BalanceText = isPrivacyMode ? "*** ₽" : $"{totalBalance:N0} ₽";

            UpcomingPayments.Clear();
            var reminders = _paymentService.GetAllPayments();

            foreach (var p in reminders)
            {
                int day = Math.Min(p.MonthlyDay, DateTime.DaysInMonth(_selectedMonth.Year, _selectedMonth.Month));
                var paymentDate = new DateTime(_selectedMonth.Year, _selectedMonth.Month, day);

                UpcomingPayments.Add(new PaymentEvent
                {
                    Id = p.Id,
                    Date = paymentDate,
                    Title = p.Title,
                    Subtitle = "Ежемесячный платеж",
                    Amount = p.Amount,
                    Type = "Расход",
                    Icon = p.Icon ?? "📅",
                });
            }

            var jobProfile = _jobProfileService.GetProfile();

            if (jobProfile.Rate > 0 && jobProfile.CalculatedMainSalary > 0)
            {
                int salDay = Math.Min(jobProfile.SalaryDay, DateTime.DaysInMonth(_selectedMonth.Year, _selectedMonth.Month));
                UpcomingPayments.Add(new PaymentEvent
                {
                    Date = new DateTime(_selectedMonth.Year, _selectedMonth.Month, salDay),
                    Title = "Зарплата",
                    Subtitle = jobProfile.PositionName ?? "Основной доход",
                    Amount = jobProfile.CalculatedMainSalary,
                    Type = "Доход",
                    Icon = "💰",
                    IconBackgroundColor = "#27AE60"
                });

                if (jobProfile.HasAdvance && jobProfile.CalculatedAdvance > 0)
                {
                    int advDay = Math.Min(jobProfile.AdvanceDay, DateTime.DaysInMonth(_selectedMonth.Year, _selectedMonth.Month));
                    UpcomingPayments.Add(new PaymentEvent
                    {
                        Date = new DateTime(_selectedMonth.Year, _selectedMonth.Month, advDay),
                        Title = "Аванс",
                        Subtitle = jobProfile.PositionName ?? "Часть ЗП",
                        Amount = jobProfile.CalculatedAdvance,
                        Type = "Доход",
                        Icon = "💸",
                        IconBackgroundColor = "#2ECC71"
                    });
                }
            }

            ActiveGoals.Clear();
            var personalGoals = _projectService.GetAllProjects()
                .Where(proj => proj.ProjectType == ProjectType.PersonalGoal)
                .ToList();

            foreach (var goal in personalGoals)
            {
                ActiveGoals.Add(new FinancialGoal
                {
                    Title = goal.Title,
                    Icon = goal.Emoji ?? "🎯",
                    TargetAmount = goal.TargetAmount,
                    CurrentAmount = goal.CollectedAmount
                });
            }

            await CalculateDailyBudgetAsync(totalBalance);
            CalculateCashFlow();
        }

        private async Task CalculateDailyBudgetAsync(double totalBalance)
        {
            var today = DateTime.Today;

            if (_selectedMonth.Month != today.Month || _selectedMonth.Year != today.Year)
            {
                DailyBudgetLimit = 0;
                return;
            }

            int daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
            int daysLeft = (daysInMonth - today.Day) + 1;

            double mandatoryRemains = UpcomingPayments
                .Where(p => p.Date.Day >= today.Day)
                .Sum(p => p.Amount);

            double availableMoney = totalBalance - mandatoryRemains;
            DailyBudgetLimit = Math.Max(0, availableMoney / daysLeft);

            var transactions = await _transactionService.GetTransactionsAsync();
            DailyBudgetSpent = transactions
                .Where(t => t.Date.Date == today && t.Type == "Расход")
                .Sum(t => t.Amount);
        }

        private void CalculateCashFlow()
        {
            double currentTotalBalance = ActiveAccounts.Sum(a => a.Balance);
            double runningBalance = currentTotalBalance;

            HasCashGapWarning = false;

            foreach (var payment in UpcomingPayments.OrderBy(p => p.Date))
            {
                if (payment.Type == "Расход") runningBalance -= payment.Amount;
                else if (payment.Type == "Доход") runningBalance += payment.Amount;

                if (runningBalance < 0 && !HasCashGapWarning)
                {
                    HasCashGapWarning = true;
                    CashGapMessage = $"Внимание! {payment.DisplayDate} на счетах не хватит {(Math.Abs(runningBalance)):N0} ₽ для оплаты «{payment.Title}».";
                }
            }
        }

        [RelayCommand]
        private void PreviousMonth()
        {
            _selectedMonth = _selectedMonth.AddMonths(-1);
            RequestDataUpdate();
        }

        [RelayCommand]
        private void NextMonth()
        {
            _selectedMonth = _selectedMonth.AddMonths(1);
            RequestDataUpdate();
        }

        [RelayCommand] private void ToggleGoals() => IsGoalsVisible = !IsGoalsVisible;
        [RelayCommand] private void TogglePaymentCalendar() => IsPaymentCalendarVisible = !IsPaymentCalendarVisible;
        [RelayCommand] private void ToggleDailyBudget() => IsDailyBudgetVisible = !IsDailyBudgetVisible;
        [RelayCommand] private void ToggleAccounts() => IsAccountsVisible = !IsAccountsVisible;
        [RelayCommand] private void ToggleFabMenu() => IsFabMenuOpen = !IsFabMenuOpen;

        [RelayCommand]
        private async Task AddTransactionAsync(string type)
        {
            IsFabMenuOpen = false;
            var vm = new AddTransactionViewModel(_transactionService, _accountService, _categoryService, type);
            await Shell.Current.Navigation.PushModalAsync(new Views.AddTransactionPage(vm));
        }

        [RelayCommand]
        private async Task AddTransferAsync()
        {
            IsFabMenuOpen = false;
            var vm = new AddTransferViewModel(_transactionService, _accountService);
            await Shell.Current.Navigation.PushModalAsync(new Views.AddTransferPage(vm));
        }
    }
}