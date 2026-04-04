using CommunityToolkit.Mvvm.ComponentModel;

namespace Finalitika10.Models
{
    public partial class PaymentReminder : ObservableObject
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public int NotificationId => Math.Abs(Id.GetHashCode());
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Icon))] 
        private string title = "";

        [ObservableProperty] private string description = "";
        [ObservableProperty] private double amount;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DisplayDate))]
        private string reminderType = "Ежемесячный";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DisplayDate))]
        private int monthlyDay = 15;

        [ObservableProperty] private bool isPushEnabled = true;
        [ObservableProperty] private TimeSpan notifyTime = new TimeSpan(10, 0, 0);

        private DateTime _exactDate = DateTime.Today.AddDays(1);
        public DateTime ExactDate
        {
            get => _exactDate;
            set
            {
                SetProperty(ref _exactDate, value.Date); 
                OnPropertyChanged(nameof(DisplayDate));  
            }
        }
        public string DisplayDate => ReminderType == "Ежемесячный"
            ? $"Каждое {MonthlyDay} число"
            : ExactDate.ToString("dd MMMM yyyy");

        public string Icon => Title.ToLower().Contains("подписка") ? "🎵" :
                              Title.ToLower().Contains("кредит") ? "🏦" :
                              Title.ToLower().Contains("интернет") ? "🌐" : "💳";
    }
}
