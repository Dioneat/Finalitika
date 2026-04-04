using CommunityToolkit.Mvvm.ComponentModel;

namespace Finalitika10.Models
{
    public partial class PaymentEvent : ObservableObject
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime Date { get; set; }
        public string Title { get; set; } = "";
        public string Subtitle { get; set; } = ""; 
        public double Amount { get; set; }
        public string Type { get; set; } = "Расход"; 

        public string Icon { get; set; } = "📅";
        public string IconBackgroundColor { get; set; } = "#BDC3C7";

        [ObservableProperty]
        private bool isAutoPaymentEnabled; 

        public string DisplayDate
        {
            get
            {
                var span = Date.Date - DateTime.Today;
                if (span.Days == 0) return "Сегодня";
                if (span.Days == 1) return "Завтра";
                return Date.ToString("dd MMM");
            }
        }

        public string AmountColor => Type == "Доход" ? "#27AE60" : "#2C3E50";

        public bool IsExpense => Type == "Расход";
    }
}
