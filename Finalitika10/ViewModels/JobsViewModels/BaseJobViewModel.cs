using CommunityToolkit.Mvvm.ComponentModel;
using Finalitika10.Models;

namespace Finalitika10.ViewModels.JobsViewModels
{
    public partial class BaseJobViewModel : ObservableObject
    {
        [ObservableProperty] private JobProfile job = new();

        public List<string> Schedules { get; } = new() { "5/2", "2/2", "1/3", "3/2", "3/3", "6/1", "Свободный" };
        public List<string> RateTypes { get; } = new() { "За смену", "За час" };
        public List<string> AdvanceTypes { get; } = new() { "50% от ЗП", "С 1 по 15 число" };
    }
}
