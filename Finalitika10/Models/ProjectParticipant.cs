using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Finalitika10.Models
{
    public enum ProjectType
    {
        Split,
        PersonalGoal
    }

    public enum TargetCalculationType
    {
        FixedAmount,
        ExpenseList
    }

    public partial class ProjectContribution : ObservableObject
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime Date { get; set; } = DateTime.Now;

        [ObservableProperty]
        private decimal amount;

        [ObservableProperty]
        private string comment = string.Empty;

        public string DisplayDate => Date.ToString("dd.MM.yy HH:mm");
    }

    public partial class ProjectExpense : ObservableObject
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [ObservableProperty]
        private string title = string.Empty;

        [ObservableProperty]
        private decimal amount;
    }

    public partial class ProjectParticipant : ObservableObject
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [ObservableProperty]
        private string name = "Новый участник";

        public ObservableCollection<ProjectContribution> Contributions { get; set; } = new();

        public decimal Contributed => Contributions.Sum(c => c.Amount);

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Balance), nameof(StatusText), nameof(StatusColor))]
        private decimal requiredShare;

        public bool IsSplitMode { get; set; } = true;

        public decimal Balance => Contributed - RequiredShare;

        public string StatusText =>
            !IsSplitMode ? $"Внес: {Contributed:N0} ₽" :
            Balance < 0 ? $"Осталось: {-Balance:N0} ₽" :
            Balance > 0 ? $"Вернуть: {Balance:N0} ₽" :
            "Скинулся 🎉";

        public string StatusColor =>
            !IsSplitMode ? "#3498DB" :
            Balance < 0 ? "#E74C3C" :
            Balance > 0 ? "#F39C12" :
            "#27AE60";

        public bool HasContributions => Contributions.Count > 0;

        public void RefreshCalculations()
        {
            OnPropertyChanged(nameof(Contributed));
            OnPropertyChanged(nameof(Balance));
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(StatusColor));
            OnPropertyChanged(nameof(HasContributions));
        }
    }

    public partial class FinancialProject : ObservableObject
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = "Новый проект";
        public string Emoji { get; set; } = "🎯";
        public string BackgroundColor { get; set; } = "#3498DB";

        public ProjectType ProjectType { get; set; } = ProjectType.Split;

        public TargetCalculationType TargetCalculationType { get; set; } = TargetCalculationType.FixedAmount;

        [ObservableProperty]
        private decimal fixedTargetAmount;

        public ObservableCollection<ProjectExpense> Expenses { get; set; } = new();
        public ObservableCollection<ProjectParticipant> Participants { get; set; } = new();

        public decimal TargetAmount =>
            TargetCalculationType == TargetCalculationType.ExpenseList
                ? Expenses.Sum(x => x.Amount)
                : FixedTargetAmount;

        public decimal CollectedAmount => Participants.Sum(p => p.Contributed);

        public double Progress => TargetAmount > 0
            ? (double)(CollectedAmount / TargetAmount)
            : 0d;

        public bool HasMultipleParticipants => Participants.Count > 1;
        public int ParticipantsCount => Participants.Count;

        public void RefreshTargetAmount()
        {
            OnPropertyChanged(nameof(TargetAmount));
            OnPropertyChanged(nameof(Progress));
        }
    }
}