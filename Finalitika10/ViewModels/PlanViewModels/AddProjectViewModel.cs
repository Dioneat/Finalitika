using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Finalitika10.Models;
using Finalitika10.Services.PlanServices;

namespace Finalitika10.ViewModels.PlanViewModels
{
    public partial class AddProjectViewModel : ObservableObject
    {
        private readonly IProjectService _projectService;

        public ProjectType[] ProjectTypes { get; } = Enum.GetValues<ProjectType>();
        public TargetCalculationType[] CalcTypes { get; } = Enum.GetValues<TargetCalculationType>();

        [ObservableProperty]
        private string title = string.Empty;

        [ObservableProperty]
        private string emoji = "🎯";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsFixedAmount))]
        private TargetCalculationType selectedCalcType = TargetCalculationType.FixedAmount;

        [ObservableProperty]
        private ProjectType selectedProjectType = ProjectType.Split;

        [ObservableProperty]
        private decimal targetAmount;

        public bool IsFixedAmount => SelectedCalcType == TargetCalculationType.FixedAmount;

        public AddProjectViewModel(IProjectService projectService)
        {
            _projectService = projectService;
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                await GetCurrentPage().DisplayAlertAsync("Ошибка", "Введите название проекта", "ОК");
                return;
            }

            if (IsFixedAmount && TargetAmount <= 0)
            {
                await GetCurrentPage().DisplayAlertAsync("Ошибка", "Укажите сумму цели", "ОК");
                return;
            }

            var newProject = new FinancialProject
            {
                Title = Title.Trim(),
                Emoji = string.IsNullOrWhiteSpace(Emoji) ? "🎯" : Emoji.Trim(),
                ProjectType = SelectedProjectType,
                TargetCalculationType = SelectedCalcType,
                FixedTargetAmount = IsFixedAmount ? TargetAmount : 0m,
                BackgroundColor = GetRandomProjectColor()
            };

            newProject.Participants.Add(new ProjectParticipant
            {
                Name = "Я",
                IsSplitMode = SelectedProjectType == ProjectType.Split
            });

            _projectService.SaveProject(newProject);
            WeakReferenceMessenger.Default.Send(new ProjectUpdatedMessage());

            if (Shell.Current is null)
            {
                return;
            }

            await Shell.Current.Navigation.PopModalAsync();
            await Shell.Current.GoToAsync($"ProjectDetailPage?ProjectId={newProject.Id}");
        }

        [RelayCommand]
        private async Task CloseAsync()
        {
            if (Shell.Current is null)
            {
                return;
            }

            await Shell.Current.Navigation.PopModalAsync();
        }

        private static string GetRandomProjectColor()
        {
            var colors = new[]
            {
                "#3498DB",
                "#9B59B6",
                "#E67E22",
                "#1ABC9C",
                "#34495E",
                "#2C3E50"
            };

            return colors[Random.Shared.Next(colors.Length)];
        }

        private static Page GetCurrentPage()
        {
            return Shell.Current?.CurrentPage
                   ?? Application.Current?.Windows.FirstOrDefault()?.Page
                   ?? throw new InvalidOperationException("Активная страница не найдена.");
        }
    }
}