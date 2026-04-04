namespace Finalitika10.Services.Import
{
    public class ImportedTransaction
    {
        public DateTime Date { get; set; }

        public double Amount { get; set; }

        public string Currency { get; set; } = "RUB";
        public string Category { get; set; } = "";
        public string Description { get; set; } = "";

        public Dictionary<string, string> AdditionalInfo { get; set; } = new();
        public string AmountColor => Amount < 0 ? "#E74C3C" : "#27AE60";
        public string MappedCategoryId { get; set; } = "";
        public string MappedCategoryName { get; set; } = "Не определено";
        public string MappedCategoryIcon { get; set; } = "❓";
        public string MappedCategoryColor { get; set; } = "#95A5A6";
        public bool IsDuplicate { get; set; } = false;

        public string GenerateHash()
        {
            var cleanDesc = Description?.Trim().ToLowerInvariant() ?? "";

            string datePart = Date.ToString("yyyy-MM-dd_HH:mm");

            return $"{datePart}_{Amount:F2}_{cleanDesc}";
        }
    }
}