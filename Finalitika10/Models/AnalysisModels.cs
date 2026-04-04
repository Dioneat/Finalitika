namespace Finalitika10.Models
{
    public class FinancialInsight
    {
        public string Icon { get; set; } = "💡";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string ActionText { get; set; } = "Понятно";
        public string BackgroundColor { get; set; } = "#F0F3F4";
        public string TextColor { get; set; } = "#2C3E50";
    }

    public class QuickAction
    {
        public string Icon { get; set; } = "";
        public string Title { get; set; } = "";
        public string CommandParameter { get; set; } = "";
        public string BackgroundColor { get; set; } = "#FFFFFF";
    }
    public class HiddenExpense
    {
        public string Icon { get; set; } = "🔄";
        public string Title { get; set; } = "";
        public double Amount { get; set; }
        public string Frequency { get; set; } = "в месяц";
        public string StatusColor { get; set; } = "#7F8C8D"; 
    }

    public class EmotionalPurchase
    {
        public string Icon { get; set; } = "🛍️";
        public string Title { get; set; } = "";
        public double Amount { get; set; }
        public string Tag { get; set; } = "Спонтанно"; 
        public string TagColor { get; set; } = "#E74C3C";
        public string DateText { get; set; } = "";
    }
  
    public class TaxDeductionOpportunity
    {
        public string Icon { get; set; } = "📄";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public double EligibleAmount { get; set; }

        public double PotentialRefund => EligibleAmount * 0.13;
    }

}
