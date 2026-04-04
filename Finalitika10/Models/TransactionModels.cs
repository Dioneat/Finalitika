using CommunityToolkit.Mvvm.ComponentModel;
using SQLite; 

namespace Finalitika10.Models
{
    public partial class TransactionCategory : ObservableObject
    {
        [PrimaryKey] 
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [ObservableProperty] private string name = "";
        [ObservableProperty] private string icon = "🏷️";
        [ObservableProperty] private string colorHex = "#3498DB";
        [ObservableProperty] private string type = "Расход";

        public bool IsDefault { get; set; } = false;

        [Ignore] 
        public bool IsNotDefault => !IsDefault;
    }

    public partial class BankAccount : ObservableObject
    {
        [PrimaryKey] 
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [ObservableProperty] private string name = "";
        [ObservableProperty] private string icon = "💳";
        [ObservableProperty] private string colorHex = "#2C3E50";

        [ObservableProperty] private double balance;
    }

    public class TransactionRecord
    {
        [PrimaryKey] 
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Type { get; set; } = "Расход"; 
        public double Amount { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string Comment { get; set; } = "";

        public string AccountId { get; set; } = "";
        public string CategoryId { get; set; } = "";

        public string ToAccountId { get; set; } = "";
        public string ImportHash { get; set; } = "";
    }

    public record TransactionAddedMessage();
    public record CategoryUpdatedMessage();
    public record AccountUpdatedMessage();
}