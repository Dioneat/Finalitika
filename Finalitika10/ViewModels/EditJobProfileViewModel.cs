using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Finalitika10.Services;
using Finalitika10.ViewModels.JobsViewModels;


namespace Finalitika10.ViewModels
{
    public partial class EditJobProfileViewModel : BaseJobViewModel
    {
        private readonly IJobProfileService _jobService;

        public EditJobProfileViewModel(IJobProfileService jobService)
        {
            _jobService = jobService;
            Job = _jobService.GetProfile(); 
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            _jobService.SaveProfile(Job);
            WeakReferenceMessenger.Default.Send(new JobProfileUpdatedMessage());
            await App.Current.MainPage.DisplayAlertAsync("Успех", "Профиль сохранен!", "ОК");
            await Shell.Current.GoToAsync("..");
        }
    }
}
