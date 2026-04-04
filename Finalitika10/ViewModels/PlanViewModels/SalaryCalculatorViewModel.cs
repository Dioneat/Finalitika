using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Finalitika10.Models;
using Finalitika10.Services;
using Finalitika10.ViewModels.JobsViewModels;
using System.Collections.ObjectModel;

namespace Finalitika10.ViewModels.PlanViewModels
{
    public partial class SalaryCalculatorViewModel : BaseJobViewModel
    {
        private readonly IJobProfileService _jobProfileService;

        [ObservableProperty] private double finalIncome;
        [ObservableProperty] private double advanceAmount;
        [ObservableProperty] private int totalWorkDays;
        [ObservableProperty] private bool isCalculated = false;

        public ObservableCollection<ShiftDay> CalendarDays { get; } = new();

        public SalaryCalculatorViewModel(IJobProfileService jobService)
        {
            _jobProfileService = jobService; 
            var savedJob = _jobProfileService.GetProfile();
        }

        [RelayCommand]
        private void Calculate()
        {
            CalendarDays.Clear();
            int year = DateTime.Now.Year;
            int month = DateTime.Now.Month;
            int daysInMonth = DateTime.DaysInMonth(year, month);

            int workDaysCount = 0;
            int workDaysFirstHalf = 0;

            for (int i = 1; i <= daysInMonth; i++)
            {
                DateTime date = new DateTime(year, month, i);
                bool isWorkDay = false;

                switch (Job.ScheduleType)
                {
                    case "5/2": isWorkDay = date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday; break;
                    case "2/2": isWorkDay = ((i - 1) % 4) < 2; break;
                    case "1/3": isWorkDay = ((i - 1) % 4) == 0; break;
                    case "3/2": isWorkDay = ((i - 1) % 5) < 3; break;
                    case "3/3": isWorkDay = ((i - 1) % 6) < 3; break;
                    case "6/1": isWorkDay = ((i - 1) % 7) < 6; break;
                    case "Свободный": isWorkDay = true; break;
                }

                if (isWorkDay)
                {
                    workDaysCount++;
                    if (i <= 15) workDaysFirstHalf++;
                }

                CalendarDays.Add(new ShiftDay { DayNumber = i.ToString(), IsWorkDay = isWorkDay });
            }

            TotalWorkDays = workDaysCount;

            double dailyRate = Job.RateType == "За час" ? (Job.Rate * Job.HoursPerDay) : Job.Rate;
            double baseGrossIncome = workDaysCount * dailyRate;
            double bonusAmount = baseGrossIncome * (Job.BonusPercentage / 100.0);
            double totalGrossIncome = baseGrossIncome + bonusAmount - Job.Penalty;
            if (totalGrossIncome < 0) totalGrossIncome = 0;

            double netIncome = Job.DeductNdfl ? totalGrossIncome * 0.87 : totalGrossIncome;
            FinalIncome = netIncome;

            if (Job.HasAdvance)
            {
                if (Job.AdvanceType == "С 1 по 15 число")
                {
                    double firstHalfGross = workDaysFirstHalf * dailyRate;
                    AdvanceAmount = Job.DeductNdfl ? firstHalfGross * 0.87 : firstHalfGross;
                }
                else
                {
                    AdvanceAmount = netIncome * 0.5;
                }
            }
            else
            {
                AdvanceAmount = 0;
            }

            IsCalculated = true;
        }

        [RelayCommand]
        private async Task ApplyAndCloseAsync()
        {
            double mainSalaryAmount = FinalIncome - (Job.HasAdvance ? AdvanceAmount : 0);

            Job.CalculatedMainSalary = mainSalaryAmount;
            Job.CalculatedAdvance = AdvanceAmount;

            _jobProfileService.SaveProfile(Job);

            WeakReferenceMessenger.Default.Send(new SalaryCalculatedMessage(
                mainSalaryAmount,
                AdvanceAmount,
                Job.HasAdvance,
                Job.SalaryDay,
                Job.AdvanceDay));

            await Shell.Current.Navigation.PopModalAsync();
        }

        [RelayCommand]
        private async Task CloseAsync() => await Shell.Current.Navigation.PopModalAsync();
    }

    public record SalaryCalculatedMessage(
          double MainSalaryAmount,
          double AdvanceAmount,
          bool HasAdvance,
          int SalaryDay,
          int AdvanceDay);
}