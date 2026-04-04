using CommunityToolkit.Mvvm.ComponentModel;

namespace Finalitika10.Models
{

    public partial class JobProfile : ObservableObject
    {
        [ObservableProperty] private string positionName = "Моя должность";
        [ObservableProperty] private string scheduleType = "5/2";
        [ObservableProperty] private string rateType = "За смену";
        [ObservableProperty] private double rate = 3000;
        [ObservableProperty] private double hoursPerDay = 8;
        [ObservableProperty] private double bonusPercentage = 0;
        [ObservableProperty] private double penalty = 0;
        [ObservableProperty] private bool deductNdfl = true;
        [ObservableProperty] private bool hasAdvance = true;
        [ObservableProperty] private string advanceType = "С 1 по 15 число";
        [ObservableProperty] private int salaryDay = 10;
        [ObservableProperty] private double calculatedMainSalary; 
        [ObservableProperty] private double calculatedAdvance;

        [ObservableProperty] private int advanceDay = 25;
    }

    public class ShiftDay
    {
        public string DayNumber { get; set; }
        public bool IsWorkDay { get; set; }
        public string BackgroundColor => IsWorkDay ? "#E8F8F5" : "#F4F6F8";
        public string TextColor => IsWorkDay ? "#16A085" : "#BDC3C7";
        public string FontAttribute => IsWorkDay ? "Bold" : "None";
    }
}
