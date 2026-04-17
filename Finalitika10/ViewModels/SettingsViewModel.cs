using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Finalitika10.Messages;
using Finalitika10.Services;
using Finalitika10.Services.Ai;
using Finalitika10.Services.AppServices;
using Finalitika10.Settings;
using Microsoft.Maui.Controls;
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

        [ObservableProperty]
        private string appVersion = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsBiometricAvailable))]
        [NotifyPropertyChangedFor(nameof(BiometricSectionOpacity))]
        private bool isPinEnabled;

        [ObservableProperty]
        private bool isBiometricEnabled;

        [ObservableProperty]
        private bool useDocsTab;

        [ObservableProperty]
        private bool privacyMode;

        [ObservableProperty]
        private string token = string.Empty;

        [ObservableProperty]
        private bool isSavingToken;

        [ObservableProperty]
        private string openRouterToken = string.Empty;

        [ObservableProperty]
        private bool isSavingOpenRouterToken;

        [ObservableProperty]
        private bool isAiAssistantEnabled = true;

        [ObservableProperty]
        private string openRouterBaseUrl = "https://openrouter.ai/api/v1";

        [ObservableProperty]
        private string defaultAiModel = "meta-llama/llama-3.3-70b-instruct:free";

        [ObservableProperty]
        private string fallbackAiModel = string.Empty;

        [ObservableProperty]
        private bool shareAiBalances = true;

        [ObservableProperty]
        private bool shareAiCategories = true;

        [ObservableProperty]
        private bool shareAiTransactions;

        [ObservableProperty]
        private bool shareAiPayments = true;

        [ObservableProperty]
        private bool shareAiPlans = true;

        [ObservableProperty]
        private bool shareAiInvestments = true;

        [ObservableProperty]
        private bool storeAiHistoryLocally = true;
        [ObservableProperty] private bool isCheckingOpenRouterConnection;
        [ObservableProperty] private string openRouterConnectionStatus = string.Empty;

        [ObservableProperty]
        private bool protectAiChatWithBiometrics;

        [ObservableProperty]
        private bool sendOnlyAggregatedAiData = true;
        private readonly IAiService _aiService;
        public bool IsBiometricAvailable => IsPinEnabled;
        public double BiometricSectionOpacity => IsPinEnabled ? 1.0 : 0.5;

        public SettingsViewModel(
      IPreferencesService preferencesService,
      ISecureStorageService secureStorageService,
      IDialogService dialogService,
      IHapticService hapticService,
      IAppInfoService appInfoService,
      IMessenger messenger,
      IAiService aiService)
        {
            _preferencesService = preferencesService;
            _secureStorageService = secureStorageService;
            _dialogService = dialogService;
            _hapticService = hapticService;
            _appInfoService = appInfoService;
            _messenger = messenger;
            _aiService = aiService;
        }

        public async Task LoadAsync()
        {
            if (_isLoaded)
                return;

            _isInitializing = true;

            try
            {
                Token = await _secureStorageService.GetAsync(SettingsKeys.TinkoffApiToken) ?? string.Empty;
                OpenRouterToken = await _secureStorageService.GetAsync(SettingsKeys.OpenRouterApiToken) ?? string.Empty;

                UseDocsTab = _preferencesService.GetBool(SettingsKeys.UseDocsTab);
                PrivacyMode = _preferencesService.GetBool(SettingsKeys.PrivacyMode);

                IsPinEnabled = _preferencesService.GetBool(SettingsKeys.HasPinCode);
                IsBiometricEnabled = _preferencesService.GetBool(SettingsKeys.UseBiometrics);

                IsAiAssistantEnabled = _preferencesService.GetBool(SettingsKeys.IsAiAssistantEnabled, true);
                OpenRouterBaseUrl = _preferencesService.GetString(SettingsKeys.OpenRouterBaseUrl, "https://openrouter.ai/api/v1");
                DefaultAiModel = _preferencesService.GetString(SettingsKeys.DefaultAiModel, "meta-llama/llama-3.3-70b-instruct:free");
                FallbackAiModel = _preferencesService.GetString(SettingsKeys.FallbackAiModel, string.Empty);

                ShareAiBalances = _preferencesService.GetBool(SettingsKeys.ShareAiBalances, true);
                ShareAiCategories = _preferencesService.GetBool(SettingsKeys.ShareAiCategories, true);
                ShareAiTransactions = _preferencesService.GetBool(SettingsKeys.ShareAiTransactions, false);
                ShareAiPayments = _preferencesService.GetBool(SettingsKeys.ShareAiPayments, true);
                ShareAiPlans = _preferencesService.GetBool(SettingsKeys.ShareAiPlans, true);
                ShareAiInvestments = _preferencesService.GetBool(SettingsKeys.ShareAiInvestments, true);

                StoreAiHistoryLocally = _preferencesService.GetBool(SettingsKeys.StoreAiHistoryLocally, true);
                ProtectAiChatWithBiometrics = _preferencesService.GetBool(SettingsKeys.ProtectAiChatWithBiometrics, false);
                SendOnlyAggregatedAiData = _preferencesService.GetBool(SettingsKeys.SendOnlyAggregatedAiData, true);

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
            if (_isInitializing)
                return;

            _preferencesService.SetBool(SettingsKeys.UseDocsTab, value);
            _messenger.Send(new TabUpdateMessage());
            _hapticService.PerformClick();
        }

        partial void OnPrivacyModeChanged(bool value)
        {
            if (_isInitializing)
                return;

            _preferencesService.SetBool(SettingsKeys.PrivacyMode, value);
            _messenger.Send(new PrivacyModeChangedMessage(value));
            _hapticService.PerformClick();
        }

        partial void OnIsPinEnabledChanged(bool value)
        {
            if (_isInitializing || _isHandlingPinToggle)
                return;

            _ = HandlePinToggleChangedAsync(value);
        }

        partial void OnIsBiometricEnabledChanged(bool value)
        {
            if (_isInitializing || _isHandlingBiometricToggle)
                return;

            _ = HandleBiometricToggleChangedAsync(value);
        }

        partial void OnIsAiAssistantEnabledChanged(bool value)
        {
            if (_isInitializing)
                return;

            _preferencesService.SetBool(SettingsKeys.IsAiAssistantEnabled, value);
            _hapticService.PerformClick();
        }

        partial void OnOpenRouterBaseUrlChanged(string value)
        {
            if (_isInitializing)
                return;

            _preferencesService.SetString(SettingsKeys.OpenRouterBaseUrl, value ?? string.Empty);
        }

        partial void OnDefaultAiModelChanged(string value)
        {
            if (_isInitializing)
                return;

            _preferencesService.SetString(SettingsKeys.DefaultAiModel, value ?? string.Empty);
        }

        partial void OnFallbackAiModelChanged(string value)
        {
            if (_isInitializing)
                return;

            _preferencesService.SetString(SettingsKeys.FallbackAiModel, value ?? string.Empty);
        }

        partial void OnShareAiBalancesChanged(bool value)
        {
            if (_isInitializing)
                return;

            _preferencesService.SetBool(SettingsKeys.ShareAiBalances, value);
            _hapticService.PerformClick();
        }

        partial void OnShareAiCategoriesChanged(bool value)
        {
            if (_isInitializing)
                return;

            _preferencesService.SetBool(SettingsKeys.ShareAiCategories, value);
            _hapticService.PerformClick();
        }

        partial void OnShareAiTransactionsChanged(bool value)
        {
            if (_isInitializing)
                return;

            _preferencesService.SetBool(SettingsKeys.ShareAiTransactions, value);
            _hapticService.PerformClick();
        }

        partial void OnShareAiPaymentsChanged(bool value)
        {
            if (_isInitializing)
                return;

            _preferencesService.SetBool(SettingsKeys.ShareAiPayments, value);
            _hapticService.PerformClick();
        }

        partial void OnShareAiPlansChanged(bool value)
        {
            if (_isInitializing)
                return;

            _preferencesService.SetBool(SettingsKeys.ShareAiPlans, value);
            _hapticService.PerformClick();
        }

        partial void OnShareAiInvestmentsChanged(bool value)
        {
            if (_isInitializing)
                return;

            _preferencesService.SetBool(SettingsKeys.ShareAiInvestments, value);
            _hapticService.PerformClick();
        }

        partial void OnStoreAiHistoryLocallyChanged(bool value)
        {
            if (_isInitializing)
                return;

            _preferencesService.SetBool(SettingsKeys.StoreAiHistoryLocally, value);
            _hapticService.PerformClick();
        }

        partial void OnProtectAiChatWithBiometricsChanged(bool value)
        {
            if (_isInitializing)
                return;

            _preferencesService.SetBool(SettingsKeys.ProtectAiChatWithBiometrics, value);
            _hapticService.PerformClick();
        }

        partial void OnSendOnlyAggregatedAiDataChanged(bool value)
        {
            if (_isInitializing)
                return;

            _preferencesService.SetBool(SettingsKeys.SendOnlyAggregatedAiData, value);
            _hapticService.PerformClick();
        }

        private async Task HandlePinToggleChangedAsync(bool isEnabled)
        {
            _isHandlingPinToggle = true;

            try
            {
                _hapticService.PerformClick();

                if (isEnabled)
                {
                    string? pin = await _dialogService.PromptAsync(
                        "ПИН-код",
                        "Придумайте 4-значный ПИН-код:",
                        maxLength: 4,
                        keyboard: Keyboard.Numeric);

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

                    if (ProtectAiChatWithBiometrics)
                    {
                        _preferencesService.SetBool(SettingsKeys.ProtectAiChatWithBiometrics, false);
                        ProtectAiChatWithBiometrics = false;
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
            if (IsSavingToken)
                return;

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
            if (IsSavingOpenRouterToken)
                return;

            string normalizedToken = OpenRouterToken?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(normalizedToken))
            {
                await _dialogService.AlertAsync("Ошибка", "Ключ AI Ассистента не может быть пустым");
                return;
            }

            IsSavingOpenRouterToken = true;

            try
            {
                await _secureStorageService.SetAsync(SettingsKeys.OpenRouterApiToken, normalizedToken);
                OpenRouterToken = normalizedToken;

                await _dialogService.AlertAsync("Успех", "Ключ ИИ надёжно зашифрован и сохранён");
                _hapticService.PerformClick();
            }
            finally
            {
                IsSavingOpenRouterToken = false;
            }
        }

        [RelayCommand]
        private async Task CheckOpenRouterConnectionAsync()
        {
            if (IsCheckingOpenRouterConnection)
                return;

            if (!IsAiAssistantEnabled)
            {
                await _dialogService.AlertAsync("AI отключён", "Сначала включите AI Ассистента.");
                return;
            }

            string token = OpenRouterToken?.Trim() ?? string.Empty;
            string baseUrl = OpenRouterBaseUrl?.Trim() ?? string.Empty;
            string defaultModel = DefaultAiModel?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(token))
            {
                await _dialogService.AlertAsync("Проверка подключения", "Сначала укажите OpenRouter API ключ.");
                return;
            }

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                await _dialogService.AlertAsync("Проверка подключения", "Укажите Base URL.");
                return;
            }

            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out _))
            {
                await _dialogService.AlertAsync("Проверка подключения", "Base URL имеет неверный формат.");
                return;
            }

            if (string.IsNullOrWhiteSpace(defaultModel))
            {
                await _dialogService.AlertAsync("Проверка подключения", "Укажите модель по умолчанию.");
                return;
            }

            IsCheckingOpenRouterConnection = true;
            OpenRouterConnectionStatus = "Проверяем подключение...";

            try
            {
                // сохраняем текущие введённые значения, чтобы сервис читал актуальные данные
                await _secureStorageService.SetAsync(SettingsKeys.OpenRouterApiToken, token);
                _preferencesService.SetString(SettingsKeys.OpenRouterBaseUrl, baseUrl);
                _preferencesService.SetString(SettingsKeys.DefaultAiModel, defaultModel);
                _preferencesService.SetString(SettingsKeys.FallbackAiModel, FallbackAiModel?.Trim() ?? string.Empty);

                var result = await _aiService.CheckConnectionAsync();

                OpenRouterConnectionStatus = result.Message;

                if (result.IsSuccess)
                {
                    string preview = result.AvailableModels.Count > 0
                        ? "\n\nПримеры доступных моделей:\n- " + string.Join("\n- ", result.AvailableModels.Take(5).Select(x => x.ModelId))
                        : string.Empty;

                    await _dialogService.AlertAsync(
                        "OpenRouter",
                        result.Message + preview);
                }
                else
                {
                    await _dialogService.AlertAsync(
                        "OpenRouter",
                        result.Message);
                }
            }
            catch (Exception ex)
            {
                OpenRouterConnectionStatus = ex.Message;
                await _dialogService.AlertAsync("OpenRouter", ex.Message);
            }
            finally
            {
                IsCheckingOpenRouterConnection = false;
            }
        }

        [RelayCommand]
        private async Task ClearAiHistoryAsync()
        {
            bool confirmed = await _dialogService.ConfirmAsync(
                "Очистить историю AI",
                "Удалить всю локальную историю AI-чата?",
                "Удалить",
                "Отмена");

            if (!confirmed)
                return;

            // Сюда позже подключите IAiChatHistoryRepository.ClearConversationAsync(...)
            await _dialogService.AlertAsync("Готово", "История AI очищена.");
            _hapticService.PerformClick();
        }

        [RelayCommand]
        private async Task ClearOpenRouterTokenAsync()
        {
            bool confirmed = await _dialogService.ConfirmAsync(
                "Удалить AI ключ",
                "Удалить сохранённый OpenRouter API ключ с устройства?",
                "Удалить",
                "Отмена");

            if (!confirmed)
                return;

            _secureStorageService.Remove(SettingsKeys.OpenRouterApiToken);
            OpenRouterToken = string.Empty;

            await _dialogService.AlertAsync("Готово", "AI ключ удалён.");
            _hapticService.PerformClick();
        }

        private static bool IsValidPin(string? value) =>
            !string.IsNullOrWhiteSpace(value) && Regex.IsMatch(value, @"^\d{4}$");
    }
}