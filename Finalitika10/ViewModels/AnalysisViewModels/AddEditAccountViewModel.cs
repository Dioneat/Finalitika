using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Finalitika10.Models;
using Finalitika10.Services;

namespace Finalitika10.ViewModels.AnalysisViewModels
{
    public partial class AddEditAccountViewModel : ObservableObject
    {
        private readonly IAccountService _accountService;
        private readonly BankAccount _editingAccount;

        [ObservableProperty] private string pageTitle;
        [ObservableProperty] private string name;
        [ObservableProperty] private string icon = "💳";
        [ObservableProperty] private double balance;

        public AddEditAccountViewModel(IAccountService accountService, BankAccount existingAccount)
        {
            _accountService = accountService;
            _editingAccount = existingAccount;

            if (_editingAccount != null)
            {
                PageTitle = "Редактирование счета";
                Name = _editingAccount.Name;
                Icon = _editingAccount.Icon;
                Balance = _editingAccount.Balance;
            }
            else
            {
                PageTitle = "Новый счет";
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                await Shell.Current.DisplayAlertAsync("Ошибка", "Введите название счета", "ОК");
                return;
            }

            var account = _editingAccount ?? new BankAccount { ColorHex = GetRandomColor() };
            account.Name = Name;
            account.Icon = string.IsNullOrWhiteSpace(Icon) ? "💳" : Icon;
            account.Balance = Balance;

            await _accountService.SaveAccountAsync(account);
            WeakReferenceMessenger.Default.Send(new AccountUpdatedMessage());

            await Shell.Current.Navigation.PopModalAsync();
        }

        [RelayCommand]
        private async Task CloseAsync()
        {
            await Shell.Current.Navigation.PopModalAsync();
        }

        private string GetRandomColor()
        {
            var colors = new[] { "#34495E", "#27AE60", "#2980B9", "#8E44AD", "#E67E22", "#C0392B", "#16A085" };
            return colors[Random.Shared.Next(colors.Length)];
        }
    }
}