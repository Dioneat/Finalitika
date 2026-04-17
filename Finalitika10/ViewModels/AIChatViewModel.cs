using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finalitika10.Models;
using Finalitika10.Services;
using Finalitika10.Services.Ai;
using Finalitika10.Services.AppServices;
using Finalitika10.Services.Interfaces;
using Microsoft.Maui.Graphics;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Finalitika10.ViewModels
{
    public partial class AIChatViewModel : ObservableObject
    {
        private const string ConversationId = "main_fin_assistant";

        private readonly IAiChatOrchestrator _chatOrchestrator;
        private readonly IAiModelCatalogService _modelCatalogService;
        private readonly IAiChatHistoryRepository _historyRepository;
        private readonly IAppSpeechToTextService _speechService;
        private readonly IPreferencesService _preferencesService;
        private readonly IClipboardService _clipboardService;
        private readonly IToastService _toastService;
        private readonly IHapticService _hapticService;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly IMarkdownRenderer _markdownRenderer;

        private readonly SemaphoreSlim _sendLock = new(1, 1);
        private CancellationTokenSource? _messageCts;
        private CancellationTokenSource? _listeningCts;

        private bool _isInitialized;

        public ObservableCollection<AiModelOption> AvailableModels { get; } = new();
        public ObservableCollection<ChatMessage> Messages { get; } = new();
        public ObservableCollection<string> SuggestedPrompts { get; } = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(MicIcon), nameof(MicColor))]
        private bool isListening;

        [ObservableProperty]
        private AiModelOption? selectedModel;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanSendMessage))]
        private string currentMessageText = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanSendMessage))]
        private bool isTyping;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanSendMessage))]
        private bool isSending;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsEmptyChat))]
        private int messageCount;

        [ObservableProperty]
        private bool isLoadingHistory;

        public string MicIcon => IsListening ? "⏹️" : "🎤";
        public Color MicColor => IsListening ? Color.FromArgb("#E74C3C") : Color.FromArgb("#95A5A6");
        public bool IsEmptyChat => MessageCount == 0;

        public bool CanSendMessage =>
            !IsTyping &&
            !IsListening &&
            !IsSending &&
            !string.IsNullOrWhiteSpace(CurrentMessageText);

        public AIChatViewModel(
            IAiChatOrchestrator chatOrchestrator,
            IAiModelCatalogService modelCatalogService,
            IAiChatHistoryRepository historyRepository,
            IAppSpeechToTextService speechService,
            IPreferencesService preferencesService,
            IClipboardService clipboardService,
            IToastService toastService,
            IHapticService hapticService,
            IDialogService dialogService,
            INavigationService navigationService,
            IMarkdownRenderer markdownRenderer)
        {
            _chatOrchestrator = chatOrchestrator;
            _modelCatalogService = modelCatalogService;
            _historyRepository = historyRepository;
            _speechService = speechService;
            _preferencesService = preferencesService;
            _clipboardService = clipboardService;
            _toastService = toastService;
            _hapticService = hapticService;
            _dialogService = dialogService;
            _navigationService = navigationService;
            _markdownRenderer = markdownRenderer;

            Messages.CollectionChanged += OnMessagesCollectionChanged;

            LoadModels();
            LoadSuggestedPrompts();
        }

        public async Task InitializeAsync()
        {
            if (_isInitialized)
                return;

            IsLoadingHistory = true;

            try
            {
                var history = await _historyRepository.GetMessagesAsync(ConversationId);

                Messages.Clear();

                foreach (var message in history)
                {
                    Messages.Add(message);
                }

                _isInitialized = true;
            }
            finally
            {
                IsLoadingHistory = false;
            }
        }

        private void LoadModels()
        {
            AvailableModels.Clear();

            foreach (var model in _modelCatalogService.GetAvailableModels())
            {
                AvailableModels.Add(model);
            }

            string savedModelId = _preferencesService.GetString(
                "SelectedAiModelId",
                _modelCatalogService.GetDefaultModel().ModelId);

            SelectedModel = AvailableModels.FirstOrDefault(x => x.ModelId == savedModelId)
                            ?? AvailableModels.FirstOrDefault()
                            ?? _modelCatalogService.GetDefaultModel();
        }

        private void LoadSuggestedPrompts()
        {
            SuggestedPrompts.Clear();
            SuggestedPrompts.Add("📊 Проанализируй мои траты за месяц");
            SuggestedPrompts.Add("🏖️ Как накопить на отпуск?");
            SuggestedPrompts.Add("📉 Где я могу сэкономить?");
        }

        partial void OnSelectedModelChanged(AiModelOption? value)
        {
            if (value is null)
                return;

            _preferencesService.SetString("SelectedAiModelId", value.ModelId);
        }

        partial void OnCurrentMessageTextChanged(string value) =>
            OnPropertyChanged(nameof(CanSendMessage));

        partial void OnIsTypingChanged(bool value) =>
            OnPropertyChanged(nameof(CanSendMessage));

        partial void OnIsListeningChanged(bool value) =>
            OnPropertyChanged(nameof(CanSendMessage));

        partial void OnIsSendingChanged(bool value) =>
            OnPropertyChanged(nameof(CanSendMessage));

        [RelayCommand]
        private async Task ToggleVoiceInputAsync()
        {
            if (IsListening)
            {
                await StopVoiceInputInternalAsync();
                return;
            }

            _listeningCts?.Cancel();
            _listeningCts?.Dispose();
            _listeningCts = new CancellationTokenSource();

            IsListening = true;

            try
            {
                string? spokenText = await _speechService.ListenOnceAsync("ru-RU", _listeningCts.Token);

                if (!string.IsNullOrWhiteSpace(spokenText))
                {
                    string prefix = string.IsNullOrWhiteSpace(CurrentMessageText)
                        ? string.Empty
                        : CurrentMessageText.EndsWith(" ")
                            ? CurrentMessageText
                            : CurrentMessageText + " ";

                    CurrentMessageText = prefix + spokenText;
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (InvalidOperationException ex)
            {
                await _dialogService.AlertAsync("Доступ запрещён", ex.Message);
            }
            catch (Exception)
            {
                await _dialogService.AlertAsync("Ошибка", "Голосовой ввод недоступен на этом устройстве.");
            }
            finally
            {
                IsListening = false;
            }
        }

        private async Task StopVoiceInputInternalAsync()
        {
            try
            {
                _listeningCts?.Cancel();
                await _speechService.StopListeningAsync();
            }
            catch
            {
            }
            finally
            {
                IsListening = false;
            }
        }

        [RelayCommand]
        private Task SendSuggestedPromptAsync(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
                return Task.CompletedTask;

            CurrentMessageText = prompt;
            return SendMessageAsync();
        }

        [RelayCommand]
        private async Task SendMessageAsync()
        {
            if (!CanSendMessage || SelectedModel is null)
                return;

            if (!await _sendLock.WaitAsync(0))
                return;

            try
            {
                IsSending = true;

                string userText = CurrentMessageText.Trim();
                CurrentMessageText = string.Empty;

                var userMessage = CreateUserMessage(userText);
                Messages.Add(userMessage);
                await _historyRepository.AddMessageAsync(ConversationId, userMessage);

                IsTyping = true;

                _messageCts?.Cancel();
                _messageCts?.Dispose();
                _messageCts = new CancellationTokenSource();

                string aiResponse = await _chatOrchestrator.GetReplyAsync(
                    userText,
                    SelectedModel.ModelId,
                    _messageCts.Token);

                var assistantMessage = CreateAssistantMessage(aiResponse);
                Messages.Add(assistantMessage);
                await _historyRepository.AddMessageAsync(ConversationId, assistantMessage);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                await _dialogService.AlertAsync("Ошибка AI", ex.Message);
            }
            finally
            {
                IsTyping = false;
                IsSending = false;
                _sendLock.Release();
            }
        }

        [RelayCommand]
        private async Task ClearHistoryAsync()
        {
            if (Messages.Count == 0)
                return;

            bool confirmed = await _dialogService.ConfirmAsync(
                "Очистить историю",
                "Удалить всю историю чата с ФинАссистентом?",
                "Удалить",
                "Отмена");

            if (!confirmed)
                return;

            await _historyRepository.ClearConversationAsync(ConversationId);
            Messages.Clear();

            _hapticService.PerformClick();
            await _toastService.ShowAsync("История чата очищена");
        }

        private ChatMessage CreateUserMessage(string text)
        {
            return new ChatMessage
            {
                IsUser = true,
                Text = text,
                HtmlText = _markdownRenderer.ToHtml(text),
                Timestamp = DateTime.Now
            };
        }

        private ChatMessage CreateAssistantMessage(string text)
        {
            return new ChatMessage
            {
                IsUser = false,
                Text = text,
                HtmlText = _markdownRenderer.ToHtml(text),
                Timestamp = DateTime.Now
            };
        }

        [RelayCommand]
        private async Task CopyMessageAsync(string textToCopy)
        {
            if (string.IsNullOrWhiteSpace(textToCopy))
                return;

            await _clipboardService.SetTextAsync(textToCopy);
            _hapticService.PerformClick();
            await _toastService.ShowAsync("Текст скопирован");
        }

        [RelayCommand]
        private Task OpenSettingsAsync() =>
            _navigationService.GoToAsync("SettingsPage");

        private void OnMessagesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            MessageCount = Messages.Count;
        }
    }
}