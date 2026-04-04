using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Finalitika10.Models;
using Finalitika10.Services;
using System.Collections.ObjectModel;

namespace Finalitika10.ViewModels.AnalysisViewModels
{
    public partial class AddTransactionViewModel : ObservableObject
    {
        private readonly ITransactionService _transactionService;
        private readonly IAccountService _accountService;
        private readonly ICategoryService _categoryService;

        public string TransactionType { get; }
        public string PageTitle => TransactionType == "Expense" ? "Новый расход" : "Новый доход";
        public string AmountColor => TransactionType == "Expense" ? "#E74C3C" : "#27AE60";

        public ObservableCollection<BankAccount> Accounts { get; } = new();
        public ObservableCollection<TransactionCategory> Categories { get; } = new();

        [ObservableProperty] private BankAccount selectedAccount;
        [ObservableProperty] private TransactionCategory selectedCategory;
        [ObservableProperty] private double amount;
        [ObservableProperty] private DateTime date = DateTime.Today;
        [ObservableProperty] private string comment = "";

        public AddTransactionViewModel(
            ITransactionService transactionService,
            IAccountService accountService,
            ICategoryService categoryService,
            string type)
        {
            _transactionService = transactionService;
            _accountService = accountService;
            _categoryService = categoryService;
            TransactionType = type;

            LoadData();
        }

        private async void LoadData()
        {
            var accounts = await _accountService.GetAccountsAsync();
            foreach (var acc in accounts) Accounts.Add(acc);
            if (Accounts.Any()) SelectedAccount = Accounts.First();

            string categoryTypeFilter = TransactionType == "Expense" ? "Расход" : "Доход";
            var allCategories = await _categoryService.GetCategoriesAsync();
            var filteredCategories = allCategories.Where(c => c.Type == categoryTypeFilter);

            foreach (var cat in filteredCategories) Categories.Add(cat);
            if (Categories.Any()) SelectedCategory = Categories.First();
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (Amount <= 0)
            {
                await Shell.Current.DisplayAlertAsync("Ошибка", "Введите сумму больше нуля", "ОК");
                return;
            }

            if (SelectedAccount == null || SelectedCategory == null)
            {
                await Shell.Current.DisplayAlertAsync("Ошибка", "Выберите счет и категорию", "ОК");
                return;
            }

            if (TransactionType == "Expense")
                SelectedAccount.Balance -= Amount;
            else
                SelectedAccount.Balance += Amount;

            await _accountService.SaveAccountAsync(SelectedAccount);

            var record = new TransactionRecord
            {
                Type = TransactionType == "Expense" ? "Расход" : "Доход",
                Amount = Amount,
                Date = Date,
                Comment = Comment,
                AccountId = SelectedAccount.Id,
                CategoryId = SelectedCategory.Id
            };

            await _transactionService.SaveTransactionAsync(record);

            WeakReferenceMessenger.Default.Send(new AccountUpdatedMessage());
            WeakReferenceMessenger.Default.Send(new TransactionAddedMessage());

            await Shell.Current.Navigation.PopModalAsync();
        }

        [RelayCommand]
        private async Task CloseAsync() => await Shell.Current.Navigation.PopModalAsync();
    }
}