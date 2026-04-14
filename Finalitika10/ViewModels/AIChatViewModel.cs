using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finalitika10.Models;
using Finalitika10.Services;
using System.Collections.ObjectModel;

namespace Finalitika10.ViewModels
{
    public partial class AIChatViewModel : ObservableObject
    {
        private readonly IAiService _aiService;
        private readonly IAccountService _accountService;
        private readonly ITransactionService _transactionService;

        public ObservableCollection<ChatMessage> Messages { get; } = new();
        public ObservableCollection<string> SuggestedPrompts { get; } = new();

        [ObservableProperty] private string currentMessageText = "";
        [ObservableProperty] private bool isTyping = false;

        public bool IsEmptyChat => Messages.Count == 0;

        public AIChatViewModel(IAiService aiService, IAccountService accountService, ITransactionService transactionService)
        {
            _aiService = aiService;
            _accountService = accountService;
            _transactionService = transactionService;

            SuggestedPrompts.Add("📊 Проанализируй мои траты за месяц");
            SuggestedPrompts.Add("🏖️ Как накопить на отпуск?");
            SuggestedPrompts.Add("📉 Где я могу сэкономить?");
        }

        [RelayCommand]
        private async Task SendSuggestedPromptAsync(string prompt)
        {
            CurrentMessageText = prompt;
            await SendMessageAsync();
        }

        [RelayCommand]
        private async Task SendMessageAsync()
        {
            if (string.IsNullOrWhiteSpace(CurrentMessageText)) return;

            string userMsg = CurrentMessageText.Trim();
            CurrentMessageText = "";

            Messages.Add(new ChatMessage { Text = userMsg, IsUser = true, Timestamp = DateTime.Now });
            OnPropertyChanged(nameof(IsEmptyChat));

            IsTyping = true;

            string context = await BuildFinancialContextAsync();

            string aiResponseText = await _aiService.GetFinancialAdviceAsync(userMsg, context);

            Messages.Add(new ChatMessage { Text = aiResponseText, IsUser = false, Timestamp = DateTime.Now });

            IsTyping = false;
        }

        private async Task<string> BuildFinancialContextAsync()
        {
            try
            {
                var accounts = await _accountService.GetAccountsAsync();
                var transactions = await _transactionService.GetTransactionsAsync();

                double totalBalance = accounts.Sum(a => a.Balance);

                var thisMonthTxs = transactions.Where(t => t.Date.Month == DateTime.Now.Month && t.Date.Year == DateTime.Now.Year).ToList();
                double income = thisMonthTxs.Where(t => t.Type == "Доход").Sum(t => t.Amount);
                double expense = thisMonthTxs.Where(t => t.Type == "Расход").Sum(t => t.Amount);

                return $@"
                Ты — персональный финансовый ассистент в приложении 'Финалитика'.
                Твоя задача — давать пользователю короткие, четкие и полезные финансовые советы.
                Отвечай кратко, не лей воду. Максимум 3-4 предложения. 
                Используй эмодзи для дружелюбия, но сохраняй профессионализм.

                ТЕКУЩАЯ ФИНАНСОВАЯ СИТУАЦИЯ ПОЛЬЗОВАТЕЛЯ:
                - Общий баланс на счетах: {totalBalance:N0} руб.
                - Заработано в этом месяце: {income:N0} руб.
                - Потрачено в этом месяце: {expense:N0} руб.

                Если пользователь спрашивает советы по экономии, обращай внимание на эти цифры. 
                Не придумывай данные, которых здесь нет. Если данных для ответа не хватает, так и скажи.";
            }
            catch
            {
                return "Ты — финансовый ассистент. Отвечай кратко и дружелюбно."; 
            }
        }

        [RelayCommand]
        private async Task OpenSettingsAsync()
        {
            await Shell.Current.GoToAsync("SettingsPage");
        }
    }
}