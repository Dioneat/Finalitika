using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Finalitika10.Models;
using Finalitika10.Services;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace Finalitika10.ViewModels
{
    public record JobProfileUpdatedMessage();

    public partial class ProfileViewModel : ObservableObject
    {
        private readonly IJobProfileService _jobProfileService;

        [ObservableProperty] private string userName = "Пользователь";
        [ObservableProperty] private string userInitials = "П";

        [ObservableProperty] private JobProfile userJob;

        public ProfileViewModel(IJobProfileService jobProfileService)
        {
            _jobProfileService = jobProfileService;
            LoadProfileData();
            WeakReferenceMessenger.Default.Register<JobProfileUpdatedMessage>(this, (r, m) => LoadProfileData());
        }

        private void LoadProfileData()
        {
            UserJob = _jobProfileService.GetProfile();

            string lastName = Preferences.Default.Get("User_LastName", "Пользователь");
            string firstName = Preferences.Default.Get("User_FirstName", "");
            UserName = $"{firstName} {lastName}".Trim();

            if (string.IsNullOrWhiteSpace(UserName))
            {
                UserName = "Тест Тестович";
                firstName = "Тест";
                lastName = "Тестович";
            }

            string initials = "";
            if (!string.IsNullOrWhiteSpace(firstName))
                initials += firstName[0];

            if (!string.IsNullOrWhiteSpace(lastName))
                initials += lastName[0];

            UserInitials = string.IsNullOrWhiteSpace(initials) ? "П" : initials.ToUpper();
        }

        [RelayCommand] private async Task EditJobAsync() => await Shell.Current.GoToAsync("EditJobProfilePage");
        [RelayCommand] private async Task GoToDocumentsAsync() => await Shell.Current.GoToAsync("DocumentsPage");
        [RelayCommand] private async Task GoToCurrenciesAsync() => await Shell.Current.GoToAsync("AddCurrencyPage");
        [RelayCommand] private async Task GoToSettingsAsync() => await Shell.Current.GoToAsync("SettingsPage");
        [RelayCommand] private async Task GoToPersonalDataAsync() => await Shell.Current.GoToAsync("PersonalDataPage");
        [RelayCommand]
        private async Task GoToAiChatAsync()
        {
            await Shell.Current.GoToAsync("AIChatPage");
        }
        [RelayCommand]
        private async Task LogoutAsync()
        {
            await Shell.Current.DisplayAlertAsync("Выход", "Выход из учетной записи...", "ОК");
        }
    }
}