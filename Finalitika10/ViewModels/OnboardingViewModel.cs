using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finalitika10.Models;
using Finalitika10.Services;
using Finalitika10.Settings;
using Microsoft.Maui.Storage;

namespace Finalitika10.ViewModels
{
    public partial class OnboardingViewModel : ObservableObject
    {
        private readonly IAccountService _accountService;

        [ObservableProperty] private int currentStep = 0;

        // Данные пользователя
        [ObservableProperty] private bool useDocuments = true;
        [ObservableProperty] private bool useInvestments = false;
        [ObservableProperty] private string tinkoffToken = "";
        [ObservableProperty] private string userFio = "";
        [ObservableProperty] private bool usePin = false;
        [ObservableProperty] private string pinCode = "";
        [ObservableProperty] private string initialBalance = ""; // Сделаем string для красивых плейсхолдеров

        public OnboardingViewModel(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [RelayCommand]
        private void NextStep() => CurrentStep++;

        [RelayCommand]
        private void PreviousStep() => CurrentStep--;

        [RelayCommand]
        private async Task UploadStatementAsync()
        {
            await Shell.Current.DisplayAlert("Загрузка", "Импорт выписок появится в следующем обновлении.", "ОК");
        }

        [RelayCommand]
        private async Task FinishAsync()
        {
            // 1. ИНТЕГРАЦИЯ С ПЕРСОНАЛЬНЫМИ ДАННЫМИ (Разбиваем ФИО)
            if (!string.IsNullOrWhiteSpace(UserFio))
            {
                var nameParts = UserFio.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (nameParts.Length > 0) Preferences.Default.Set("User_LastName", nameParts[0]);
                if (nameParts.Length > 1) Preferences.Default.Set("User_FirstName", nameParts[1]);
                if (nameParts.Length > 2) Preferences.Default.Set("User_Patronymic", nameParts[2]);
            }

            // 2. ИНТЕГРАЦИЯ С НАСТРОЙКАМИ (Используем твои SettingsKeys!)
            Preferences.Default.Set(SettingsKeys.UseDocsTab, UseDocuments);

            // Если у тебя есть ключ для инвестиций, используй его (например, SettingsKeys.UseInvestmentsTab). 
            // Если нет, оставь просто строку "UseInvestments"
            Preferences.Default.Set("UseInvestments", UseInvestments);

            // Токен Тинькофф строго в SecureStorage по твоему ключу
            if (UseInvestments && !string.IsNullOrWhiteSpace(TinkoffToken))
            {
                await SecureStorage.Default.SetAsync(SettingsKeys.TinkoffApiToken, TinkoffToken);
            }

            // Пин-код тоже по твоему ключу
            if (UsePin && !string.IsNullOrWhiteSpace(PinCode))
            {
                await SecureStorage.Default.SetAsync(SettingsKeys.AppPinCode, PinCode);
                Preferences.Default.Set(SettingsKeys.HasPinCode, true);
            }

            // 3. СОЗДАНИЕ СЧЕТА
            if (double.TryParse(InitialBalance, out double balance) && balance >= 0)
            {
                await _accountService.SaveAccountAsync(new BankAccount
                {
                    Name = "Основной счет",
                    Balance = balance,
                });
            }

            // 4. ЗАВЕРШЕНИЕ ОНБОРДИНГА
            Preferences.Default.Set("IsOnboardingComplete", true);
            Application.Current.MainPage = new AppShell();
        }
    }
}