using Finalitika10.Models;
using Finalitika10.Services.Ai;
using Finalitika10.Services.AppServices;
using System.Text;

namespace Finalitika10.Services
{
    public sealed class AiChatContextBuilder : IAiChatContextBuilder
    {
        private readonly IAccountService _accountService;
        private readonly ITransactionService _transactionService;
        private readonly IPreferencesService _preferencesService;

        public AiChatContextBuilder(
            IAccountService accountService,
            ITransactionService transactionService,
            IPreferencesService preferencesService)
        {
            _accountService = accountService;
            _transactionService = transactionService;
            _preferencesService = preferencesService;
        }

        public async Task<string> BuildAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                bool canShareBalances = _preferencesService.GetBool("ShareAiBalances", true);
                bool canShareCategories = _preferencesService.GetBool("ShareAiCategories", true);
                bool canShareTransactions = _preferencesService.GetBool("ShareAiTransactions", false);

                var builder = new StringBuilder();
                builder.AppendLine("Ты — персональный финансовый ассистент 'Финалитика'.");
                builder.AppendLine("Твоя задача — давать полезные финансовые советы.");
                builder.AppendLine("Отвечай кратко. Не выдумывай цифры.");

                var accounts = await _accountService.GetAccountsAsync() ?? new List<BankAccount>();
                var allTransactions = await _transactionService.GetTransactionsAsync() ?? new List<TransactionRecord>();

                var now = DateTime.Now;
                var thisMonthTransactions = allTransactions
                    .Where(t => t.Date.Month == now.Month && t.Date.Year == now.Year)
                    .ToList();

                if (canShareBalances || canShareCategories || canShareTransactions)
                {
                    builder.AppendLine();
                    builder.AppendLine("РЕАЛЬНЫЕ ФИНАНСОВЫЕ ДАННЫЕ ПОЛЬЗОВАТЕЛЯ:");
                }

                if (canShareBalances)
                {
                    decimal totalBalance = accounts.Sum(a => Convert.ToDecimal(a.Balance));
                    builder.AppendLine($"- Общий баланс на всех счетах: {totalBalance:N0} руб.");
                }

                if (canShareCategories)
                {
                    decimal income = thisMonthTransactions
                        .Where(t => t.Type == "Доход")
                        .Sum(t => Convert.ToDecimal(t.Amount));

                    decimal expense = thisMonthTransactions
                        .Where(t => t.Type == "Расход")
                        .Sum(t => Convert.ToDecimal(t.Amount));

                    builder.AppendLine($"- Доходы в этом месяце: {income:N0} руб.");
                    builder.AppendLine($"- Расходы в этом месяце: {expense:N0} руб.");
                }

                if (canShareTransactions)
                {
                    var recentTransactions = thisMonthTransactions
                        .OrderByDescending(t => t.Date)
                        .Take(10)
                        .ToList();

                    if (recentTransactions.Count > 0)
                    {
                        builder.AppendLine();
                        builder.AppendLine("ПОСЛЕДНИЕ ТРАНЗАКЦИИ ДЛЯ АНАЛИЗА:");

                        foreach (var tx in recentTransactions)
                        {
                            builder.AppendLine(
                                $"[{tx.Date:dd.MM}] {tx.Comment ?? "Покупка"} ({tx.CategoryId}): {tx.Amount:N0} руб.");
                        }
                    }
                }

                return builder.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AI CONTEXT ERROR]: {ex.Message}");
                return $"Ты — ассистент. Скажи пользователю, что произошла системная ошибка при загрузке данных: {ex.Message}";
            }
        }
    }
}
