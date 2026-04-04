using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Finalitika10.Models;
using Finalitika10.Services.PlanServices;
using System.Globalization;

namespace Finalitika10.ViewModels.PlanViewModels
{
    [QueryProperty(nameof(ProjectId), "ProjectId")]
    public partial class ProjectDetailViewModel : ObservableObject
    {
        private readonly IProjectService _projectService;
        private string? _projectId;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsExpensesMode))]
        [NotifyPropertyChangedFor(nameof(IsNotExpensesMode))]
        [NotifyPropertyChangedFor(nameof(IsSplitMode))]
        private FinancialProject project = new();

        [ObservableProperty]
        private decimal totalCollected;

        [ObservableProperty]
        private double progressPercent;

        public bool IsExpensesMode => Project?.TargetCalculationType == TargetCalculationType.ExpenseList;
        public bool IsNotExpensesMode => !IsExpensesMode;
        public bool IsSplitMode => Project?.ProjectType == ProjectType.Split;

        public string? ProjectId
        {
            get => _projectId;
            set
            {
                if (SetProperty(ref _projectId, value) && !string.IsNullOrWhiteSpace(value))
                {
                    LoadProject(value);
                }
            }
        }

        public ProjectDetailViewModel(IProjectService projectService)
        {
            _projectService = projectService;
        }

        private void LoadProject(string id)
        {
            var loadedProject = _projectService.GetProjectById(id);

            if (loadedProject is null)
            {
                return;
            }

            Project = loadedProject;
            RecalculateViewState();
        }

        private void RecalculateViewState()
        {
            if (Project is null)
            {
                return;
            }

            Project.RefreshTargetAmount();

            TotalCollected = Project.CollectedAmount;
            ProgressPercent = Project.Progress;

            decimal fairShare = 0m;

            if (IsSplitMode && Project.Participants.Count > 0)
            {
                fairShare = Project.TargetAmount / Project.Participants.Count;
            }

            foreach (var participant in Project.Participants)
            {
                participant.IsSplitMode = IsSplitMode;
                participant.RequiredShare = fairShare;
                participant.RefreshCalculations();
            }

            OnPropertyChanged(nameof(IsExpensesMode));
            OnPropertyChanged(nameof(IsNotExpensesMode));
            OnPropertyChanged(nameof(IsSplitMode));
        }

        private Task PersistProjectAsync()
        {
            _projectService.SaveProject(Project);
            WeakReferenceMessenger.Default.Send(new ProjectUpdatedMessage());
            return Task.CompletedTask;
        }

        [RelayCommand]
        private async Task EditTargetAmountAsync()
        {
            if (IsExpensesMode)
            {
                await GetCurrentPage().DisplayAlertAsync(
                    "Инфо",
                    "Сумма рассчитывается по списку расходов. Добавьте новый расход.",
                    "ОК");
                return;
            }

            string? result = await GetCurrentPage().DisplayPromptAsync(
                "Изменение цели",
                "Введите новую общую сумму:",
                "ОК",
                "Отмена",
                keyboard: Keyboard.Numeric,
                initialValue: Project.FixedTargetAmount.ToString(CultureInfo.CurrentCulture));

            if (!TryParseMoney(result, out decimal newAmount) || newAmount <= 0)
            {
                return;
            }

            Project.FixedTargetAmount = newAmount;
            RecalculateViewState();
            await PersistProjectAsync();
        }

        [RelayCommand]
        private async Task AddExpenseAsync()
        {
            string? title = await GetCurrentPage().DisplayPromptAsync(
                "Новый расход",
                "На что нужны деньги?");

            if (string.IsNullOrWhiteSpace(title))
            {
                return;
            }

            string? amountText = await GetCurrentPage().DisplayPromptAsync(
                "Сумма",
                "Какая сумма?",
                "ОК",
                "Отмена",
                keyboard: Keyboard.Numeric);

            if (!TryParseMoney(amountText, out decimal amount) || amount <= 0)
            {
                return;
            }

            Project.Expenses.Add(new ProjectExpense
            {
                Title = title.Trim(),
                Amount = amount
            });

            RecalculateViewState();
            await PersistProjectAsync();
        }

        [RelayCommand]
        private async Task AddParticipantAsync()
        {
            string? name = await GetCurrentPage().DisplayPromptAsync(
                "Участник",
                "Имя:");

            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            Project.Participants.Add(new ProjectParticipant
            {
                Name = name.Trim(),
                IsSplitMode = IsSplitMode
            });

            RecalculateViewState();
            await PersistProjectAsync();
        }

        [RelayCommand]
        private async Task AddContributionAsync(ProjectParticipant? participant)
        {
            if (participant is null)
            {
                return;
            }

            string? amountText = await GetCurrentPage().DisplayPromptAsync(
                "Взнос",
                $"Сколько внес {participant.Name}?",
                "ОК",
                "Отмена",
                keyboard: Keyboard.Numeric);

            if (!TryParseMoney(amountText, out decimal amount) || amount <= 0)
            {
                return;
            }

            string? comment = await GetCurrentPage().DisplayPromptAsync(
                "Комментарий",
                "На что или за что? (опционально)",
                "ОК",
                "Пропустить",
                placeholder: "Перевод на карту");

            participant.Contributions.Add(new ProjectContribution
            {
                Amount = amount,
                Comment = string.IsNullOrWhiteSpace(comment) ? "Взнос" : comment.Trim()
            });

            participant.RefreshCalculations();
            RecalculateViewState();
            await PersistProjectAsync();
        }

        private static bool TryParseMoney(string? value, out decimal amount)
        {
            amount = 0m;

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var normalized = value.Trim();

            return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.CurrentCulture, out amount)
                   || decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out amount);
        }

        private static Page GetCurrentPage()
        {
            return Shell.Current?.CurrentPage
                   ?? Application.Current?.Windows.FirstOrDefault()?.Page
                   ?? throw new InvalidOperationException("Активная страница не найдена.");
        }
    }
}