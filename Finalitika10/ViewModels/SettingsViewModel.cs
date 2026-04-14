using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Finalitika10.Messages;
using Finalitika10.Services;
using Finalitika10.Services.AppServices;
using Finalitika10.Settings;
using System.Text.RegularExpressions;

namespace Finalitika10.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly IPreferencesService _preferencesService;
        private readonly ISecureStorageService _secureStorageService;
        private readonly IDialogService _dialogService;
        private readonly IHapticService _hapticService;
        private readonly IAppInfoService _appInfoService;
        private readonly IMessenger _messenger;

        private bool _isInitializing;
        private bool _isLoaded;
        private bool _isHandlingPinToggle;
        private bool _isHandlingBiometricToggle;

        [ObservableProperty] private string appVersion = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsBiometricAvailable))]
        [NotifyPropertyChangedFor(nameof(BiometricSectionOpacity))]
        private bool isPinEnabled;

        [ObservableProperty] private bool isBiometricEnabled;
        [ObservableProperty] private bool useDocsTab;
        [ObservableProperty] private bool privacyMode;

        // Токен Тинькофф
        [ObservableProperty] private string token = string.Empty;
        [ObservableProperty] private bool isSavingToken;

        // Токен ИИ (OpenRouter)
        [ObservableProperty] private string openRouterToken = string.Empty;
        [ObservableProperty] private bool isSavingOpenRouterToken;

        public bool IsBiometricAvailable => IsPinEnabled;
        public double BiometricSectionOpacity => IsPinEnabled ? 1.0 : 0.5;

        public SettingsViewModel(
            IPreferencesService preferencesService,
            ISecureStorageService secureStorageService,
            IDialogService dialogService,
            IHapticService hapticService,
            IAppInfoService appInfoService,
            IMessenger messenger)
        {
            _preferencesService = preferencesService;
            _secureStorageService = secureStorageService;
            _dialogService = dialogService;
            _hapticService = hapticService;
            _appInfoService = appInfoService;
            _messenger = messenger;
        }

        public async Task LoadAsync()
        {
            if (_isLoaded) return;

            _isInitializing = true;

            try
            {
                Token = await _secureStorageService.GetAsync(SettingsKeys.TinkoffApiToken) ?? string.Empty;

                // Загружаем токен ИИ. (Используйте SettingsKeys.OpenRouterApiToken, если добавили его в свой класс констант)
                OpenRouterToken = await _secureStorageService.GetAsync("OpenRouterApiToken") ?? string.Empty;

                UseDocsTab = _preferencesService.GetBool(SettingsKeys.UseDocsTab);
                PrivacyMode = _preferencesService.GetBool(SettingsKeys.PrivacyMode);

                IsPinEnabled = _preferencesService.GetBool(SettingsKeys.HasPinCode);
                IsBiometricEnabled = _preferencesService.GetBool(SettingsKeys.UseBiometrics);

                AppVersion = _appInfoService.Version;
            }
            finally
            {
                _isInitializing = false;
                _isLoaded = true;
            }
        }

        partial void OnUseDocsTabChanged(bool value)
        {
            if (_isInitializing) return;
            _preferencesService.SetBool(SettingsKeys.UseDocsTab, value);
            _messenger.Send(new TabUpdateMessage());
            _hapticService.PerformClick();
        }

        partial void OnPrivacyModeChanged(bool value)
        {
            if (_isInitializing) return;
            _preferencesService.SetBool(SettingsKeys.PrivacyMode, value);
            _messenger.Send(new PrivacyModeChangedMessage(value));
            _hapticService.PerformClick();
        }

        partial void OnIsPinEnabledChanged(bool value)
        {
            if (_isInitializing || _isHandlingPinToggle) return;
            _ = HandlePinToggleChangedAsync(value);
        }

        partial void OnIsBiometricEnabledChanged(bool value)
        {
            if (_isInitializing || _isHandlingBiometricToggle) return;
            _ = HandleBiometricToggleChangedAsync(value);
        }

        private async Task HandlePinToggleChangedAsync(bool isEnabled)
        {
            _isHandlingPinToggle = true;
            try
            {
                _hapticService.PerformClick();

                if (isEnabled)
                {
                    string? pin = await _dialogService.PromptAsync("ПИН-код", "Придумайте 4-значный ПИН-код:", maxLength: 4, keyboard: Keyboard.Numeric);

                    if (IsValidPin(pin))
                    {
                        await _secureStorageService.SetAsync(SettingsKeys.AppPinCode, pin!);
                        _preferencesService.SetBool(SettingsKeys.HasPinCode, true);
                    }
                    else
                    {
                        IsPinEnabled = false;
                        _preferencesService.SetBool(SettingsKeys.HasPinCode, false);
                    }
                }
                else
                {
                    _secureStorageService.Remove(SettingsKeys.AppPinCode);
                    _preferencesService.SetBool(SettingsKeys.HasPinCode, false);

                    if (IsBiometricEnabled)
                    {
                        _preferencesService.SetBool(SettingsKeys.UseBiometrics, false);
                        IsBiometricEnabled = false;
                    }
                }
            }
            finally
            {
                _isHandlingPinToggle = false;
            }
        }

        private Task HandleBiometricToggleChangedAsync(bool isEnabled)
        {
            _isHandlingBiometricToggle = true;
            try
            {
                if (!IsPinEnabled)
                {
                    IsBiometricEnabled = false;
                    return Task.CompletedTask;
                }

                _preferencesService.SetBool(SettingsKeys.UseBiometrics, isEnabled);
                _hapticService.PerformClick();
                return Task.CompletedTask;
            }
            finally
            {
                _isHandlingBiometricToggle = false;
            }
        }

        [RelayCommand]
        private async Task SaveTokenAsync()
        {
            if (IsSavingToken) return;

            string normalizedToken = Token?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(normalizedToken))
            {
                await _dialogService.AlertAsync("Ошибка", "Токен не может быть пустым");
                return;
            }

            IsSavingToken = true;

            try
            {
                await _secureStorageService.SetAsync(SettingsKeys.TinkoffApiToken, normalizedToken);
                Token = normalizedToken;

                await _dialogService.AlertAsync("Успех", "Токен надёжно сохранён");
                _hapticService.PerformClick();
            }
            finally
            {
                IsSavingToken = false;
            }
        }

        [RelayCommand]
        private async Task SaveOpenRouterTokenAsync()
        {
            if (IsSavingOpenRouterToken) return;

            string normalizedToken = OpenRouterToken?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(normalizedToken))
            {
                await _dialogService.AlertAsync("Ошибка", "Ключ AI Ассистента не может быть пустым");
                return;
            }

            IsSavingOpenRouterToken = true;

            try
            {
                await _secureStorageService.SetAsync("OpenRouterApiToken", normalizedToken);
                OpenRouterToken = normalizedToken;

                await _dialogService.AlertAsync("Успех", "Ключ ИИ надёжно зашифрован и сохранён");
                _hapticService.PerformClick();
            }
            finally
            {
                IsSavingOpenRouterToken = false;
            }
        }

        private static bool IsValidPin(string? value) =>
            !string.IsNullOrWhiteSpace(value) && Regex.IsMatch(value, @"^\d{4}$");
    }
}