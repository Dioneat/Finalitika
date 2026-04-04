namespace Finalitika10.Services
{
    public class PaymentCalendarItem
    {
        public DateTime Date { get; set; }
        public string Title { get; set; }
        public double Amount { get; set; }
        public string Type { get; set; } 
        public bool IsAutoPayment { get; set; }
    }
}
