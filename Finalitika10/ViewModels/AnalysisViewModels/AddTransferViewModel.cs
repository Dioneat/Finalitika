using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Finalitika10.Models;
using Finalitika10.Services;
using System.Collections.ObjectModel;

namespace Finalitika10.ViewModels.AnalysisViewModels
{
    public partial class AddTransferViewModel : ObservableObject
    {
        private readonly ITransactionService _transactionService;
        private readonly IAccountService _accountService;

        public ObservableCollection<BankAccount> Accounts { get; } = new();

        [ObservableProperty] private BankAccount fromAccount;
        [ObservableProperty] private BankAccount toAccount;
        [ObservableProperty] private double amount;
        [ObservableProperty] private DateTime date = DateTime.Today;
        [ObservableProperty] private string comment = "";

        public AddTransferViewModel(ITransactionService transactionService, IAccountService accountService)
        {
            _transactionService = transactionService;
            _accountService = accountService;
            LoadAccountsAsync();
        }

        private async void LoadAccountsAsync()
        {
            var accounts = await _accountService.GetAccountsAsync();
            foreach (var acc in accounts) Accounts.Add(acc);

            if (Accounts.Count >= 2)
            {
                FromAccount = Accounts[0];
                ToAccount = Accounts[1];
            }
            else if (Accounts.Count == 1)
            {
                FromAccount = Accounts[0];
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (Amount <= 0)
            {
                await Shell.Current.DisplayAlertAsync("Ошибка", "Введите сумму больше нуля", "ОК");
                return;
            }

            if (FromAccount == null || ToAccount == null)
            {
                await Shell.Current.DisplayAlertAsync("Ошибка", "Выберите оба счета", "ОК");
                return;
            }

            if (FromAccount.Id == ToAccount.Id)
            {
                await Shell.Current.DisplayAlertAsync("Ошибка", "Счета списания и зачисления должны быть разными", "ОК");
                return;
            }

            FromAccount.Balance -= Amount;
            ToAccount.Balance += Amount;

            await _accountService.SaveAccountAsync(FromAccount);
            await _accountService.SaveAccountAsync(ToAccount);

            var record = new TransactionRecord
            {
                Type = "Перевод",
                Amount = Amount,
                Date = Date,
                Comment = string.IsNullOrWhiteSpace(Comment) ? "Перевод между своими счетами" : Comment,
                AccountId = FromAccount.Id,
                ToAccountId = ToAccount.Id,
                CategoryId = ""
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