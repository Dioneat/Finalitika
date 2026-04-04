using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Finalitika10.Models;
using Finalitika10.Services;

namespace Finalitika10.ViewModels
{
    public record JobProfileUpdatedMessage();
    public partial class ProfileViewModel : ObservableObject
    {
        private readonly IJobProfileService _jobProfileService;
        [ObservableProperty] private string userName = "Тест Тестович";
        [ObservableProperty] private string userEmail = "test@example.com";
        [ObservableProperty] private string investorLevel = "Серебряный тестер";
        [ObservableProperty] private JobProfile userJob;
        public ProfileViewModel(IJobProfileService jobProfileService)
        {
            _jobProfileService = jobProfileService;

            LoadProfileData();

            WeakReferenceMessenger.Default.Register<JobProfileUpdatedMessage>(this, (r, m) =>
            {
                LoadProfileData();
            });
        }
        private void LoadProfileData()
        {
            UserJob = _jobProfileService.GetProfile();
        }

        [RelayCommand]
        private async Task EditJobAsync()
        {
            await Shell.Current.GoToAsync("EditJobProfilePage");
        }
        [RelayCommand]
        private async Task GoToDocumentsAsync()
        {
            await Shell.Current.GoToAsync("DocumentsPage");
        }

        [RelayCommand]
        private async Task GoToCurrenciesAsync()
        {
            await Shell.Current.GoToAsync("AddCurrencyPage");
        }

        [RelayCommand]
        private async Task GoToSettingsAsync()
        {

            await Shell.Current.GoToAsync("SettingsPage");
        }

        [RelayCommand]
        private async Task LogoutAsync()
        {
            await Shell.Current.DisplayAlertAsync("Выход", "Выход из учетной записи...", "ОК");
        }
    }
}
