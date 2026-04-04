using CommunityToolkit.Mvvm.ComponentModel;

namespace Finalitika10.Models
{
    public partial class FinancialGoal : ObservableObject
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = "";
        public string Icon { get; set; } = "🎯";

        public string ProjectType { get; set; } = "Личная цель";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Progress), nameof(RemainingAmount), nameof(ProgressPercentText))]
        private decimal targetAmount;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Progress), nameof(RemainingAmount), nameof(ProgressPercentText))]
        private decimal currentAmount;

        public decimal Progress => (decimal)(TargetAmount > 0 ? Math.Min((byte)(CurrentAmount / TargetAmount), 1.0) : 0);

        public decimal RemainingAmount => Math.Max(TargetAmount - CurrentAmount, 0);

        public string ProgressPercentText => $"{(Progress * 100):F0}%";

        public string ProgressBarColor => (double)Progress >= 1.0 ? "#F1C40F" : "#3498DB";
    }
}
