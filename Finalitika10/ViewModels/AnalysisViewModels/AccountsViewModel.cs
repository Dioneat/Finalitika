using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Finalitika10.Models;
using Finalitika10.Services;
using Finalitika10.Views;
using System.Collections.ObjectModel;

namespace Finalitika10.ViewModels.AnalysisViewModels
{
    public partial class AccountsViewModel : ObservableObject
    {
        private readonly IAccountService _accountService;
        private bool _isUpdatePending = false;

        public ObservableCollection<BankAccount> Accounts { get; } = new();

        [ObservableProperty] private double totalBalance;

        public AccountsViewModel(IAccountService accountService)
        {
            _accountService = accountService;
            RequestDataUpdate();

            WeakReferenceMessenger.Default.Register<AccountUpdatedMessage>(this, (r, m) => RequestDataUpdate());
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
                    await LoadAccountsAsync();
                }
                finally
                {
                    _isUpdatePending = false;
                }
            });
        }

        private async Task LoadAccountsAsync()
        {
            Accounts.Clear();
            var allAccounts = await _accountService.GetAccountsAsync();

            foreach (var acc in allAccounts)
                Accounts.Add(acc);

            TotalBalance = Accounts.Sum(a => a.Balance);
        }

        [RelayCommand]
        private async Task AddAccountAsync()
        {
            var vm = new AddEditAccountViewModel(_accountService, null);
            await Shell.Current.Navigation.PushModalAsync(new AddEditAccountPage(vm));
        }

        [RelayCommand]
        private async Task EditAccountAsync(BankAccount account)
        {
            if (account == null) return;
            var vm = new AddEditAccountViewModel(_accountService, account);
            await Shell.Current.Navigation.PushModalAsync(new AddEditAccountPage(vm));
        }

        [RelayCommand]
        private async Task DeleteAccountAsync(BankAccount account)
        {
            if (account == null) return;

            if (Accounts.Count <= 1)
            {
                await Shell.Current.DisplayAlertAsync("Внимание", "Нельзя удалить единственный счет.", "ОК");
                return;
            }

            bool confirm = await Shell.Current.DisplayAlertAsync("Удаление", $"Удалить счет '{account.Name}'?", "Да", "Отмена");
            if (confirm)
            {
                await _accountService.DeleteAccountAsync(account);
                WeakReferenceMessenger.Default.Send(new AccountUpdatedMessage());
            }
        }
    }
}