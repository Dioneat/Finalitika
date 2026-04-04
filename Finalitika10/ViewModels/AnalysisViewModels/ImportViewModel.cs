using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Finalitika10.Models;
using Finalitika10.Services;
using Finalitika10.Services.Import;
using System.Collections.ObjectModel;

namespace Finalitika10.ViewModels.AnalysisViewModels
{
    public partial class ImportViewModel : ObservableObject
    {
        private readonly ITransactionService _transactionService;
        private readonly IAccountService _accountService;
        private readonly ICategoryService _categoryService;
        private readonly CategoryMappingService _mappingService;

        public ObservableCollection<BankAccount> Accounts { get; } = new();

        public List<string> Banks { get; } = new() { "Тинькофф", "Альфа-Банк" };
        public List<string> Formats { get; } = new() { "CSV", "XLSX", "OFX" };

        [ObservableProperty] private BankAccount selectedAccount;
        [ObservableProperty] private string selectedBank = "Тинькофф";
        [ObservableProperty] private string selectedFormat = "CSV";

        public ObservableCollection<ImportedTransaction> PreviewTransactions { get; } = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotPreviewVisible))]
        private bool isPreviewVisible;

        public bool IsNotPreviewVisible => !IsPreviewVisible;
        [ObservableProperty] private string summaryText = "";

        public ImportViewModel(ITransactionService transactionService, IAccountService accountService, ICategoryService categoryService, CategoryMappingService mappingService)
        {
            _transactionService = transactionService;
            _accountService = accountService;
            _categoryService = categoryService;
            _mappingService = mappingService;

            LoadAccountsAsync();
        }

        private async void LoadAccountsAsync()
        {
            var accounts = await _accountService.GetAccountsAsync();
            foreach (var acc in accounts) Accounts.Add(acc);
            if (Accounts.Any()) SelectedAccount = Accounts[0];
        }

        [RelayCommand]
        private async Task PickFileAndParseAsync()
        {
            if (SelectedAccount == null)
            {
                await Shell.Current.DisplayAlertAsync("Ошибка", "Выберите счет для импорта", "ОК");
                return;
            }

            try
            {
                var result = await FilePicker.Default.PickAsync();
                if (result == null) return;

                var formatType = GetFormatType(SelectedBank, SelectedFormat);
                var factory = new StatementParserFactory();
                var parser = factory.GetParser(formatType);

                using var stream = await result.OpenReadAsync();
                var transactions = await parser.ParseAsync(stream);

                PreviewTransactions.Clear();

                var allUserCategories = await _categoryService.GetCategoriesAsync();
                var allTransactions = await _transactionService.GetTransactionsAsync();
                var existingTransactions = allTransactions.Where(t => t.AccountId == SelectedAccount.Id);
                var existingHashes = new HashSet<string>(existingTransactions.Select(t => t.ImportHash).Where(h => !string.IsNullOrEmpty(h)));

                int duplicateCount = 0;

                foreach (var t in transactions.OrderBy(t => t.Date))
                {
                    var matchedCategory = _mappingService.MapTransaction(t, allUserCategories);
                    if (matchedCategory != null)
                    {
                        t.MappedCategoryId = matchedCategory.Id;
                        t.MappedCategoryName = matchedCategory.Name;
                        t.MappedCategoryIcon = matchedCategory.Icon;
                        t.MappedCategoryColor = matchedCategory.ColorHex;
                    }

                    string hash = t.GenerateHash();
                    if (existingHashes.Contains(hash))
                    {
                        t.IsDuplicate = true;
                        duplicateCount++;
                    }
                    else
                    {
                        existingHashes.Add(hash);
                    }

                    PreviewTransactions.Add(t);
                }

                IsPreviewVisible = PreviewTransactions.Any();

                SummaryText = duplicateCount > 0
                    ? $"Найдено: {PreviewTransactions.Count} (из них {duplicateCount} уже добавлено)"
                    : $"Найдено новых операций: {PreviewTransactions.Count}";
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Ошибка парсинга", $"Не удалось прочитать файл. Проверьте формат.\n\n{ex.Message}", "ОК");
                PreviewTransactions.Clear();
                IsPreviewVisible = false;
            }
        }

        [RelayCommand]
        private async Task SaveImportAsync()
        {
            if (!PreviewTransactions.Any() || SelectedAccount == null) return;

            var newTransactions = PreviewTransactions.Where(t => !t.IsDuplicate).ToList();

            if (!newTransactions.Any())
            {
                await Shell.Current.DisplayAlertAsync("Внимание", "Все найденные операции уже были добавлены ранее.", "ОК");
                return;
            }

            double totalAmountChange = 0;

            foreach (var imported in newTransactions)
            {
                var record = new TransactionRecord
                {
                    Type = imported.Amount >= 0 ? "Доход" : "Расход",
                    Amount = Math.Abs(imported.Amount),
                    Date = imported.Date,
                    Comment = $"{imported.Description} ({imported.Category})",
                    AccountId = SelectedAccount.Id,
                    CategoryId = imported.MappedCategoryId,
                    ImportHash = imported.GenerateHash()
                };

                await _transactionService.SaveTransactionAsync(record);
                totalAmountChange += imported.Amount;
            }

            SelectedAccount.Balance += totalAmountChange;
            await _accountService.SaveAccountAsync(SelectedAccount);

            WeakReferenceMessenger.Default.Send(new AccountUpdatedMessage());
            WeakReferenceMessenger.Default.Send(new TransactionAddedMessage());

            await Shell.Current.DisplayAlertAsync("Успех", $"Успешно загружено {newTransactions.Count} операций!", "ОК");
            await Shell.Current.Navigation.PopAsync();
        }

        private BankFormatType GetFormatType(string bank, string format)
        {
            if (bank == "Тинькофф" && format == "CSV") return BankFormatType.TinkoffCsv;
            if (bank == "Тинькофф" && format == "XLSX") return BankFormatType.TinkoffXlsx;
            if (bank == "Тинькофф" && format == "OFX") return BankFormatType.TinkoffOfx;
            if (bank == "Альфа-Банк" && format == "CSV") return BankFormatType.AlfaBankCsv;
            if (bank == "Альфа-Банк" && format == "XLSX") return BankFormatType.AlfaBankXlsx;

            throw new ArgumentException($"Связка {bank} + {format} пока не поддерживается.");
        }
    }
}