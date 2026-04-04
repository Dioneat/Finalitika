using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Finalitika10.Models;
using Finalitika10.Services.PlanServices;
using Finalitika10.Views.PlanPages;
using System.Collections.ObjectModel;

namespace Finalitika10.ViewModels.PlanViewModels
{
    public partial class AllProjectsViewModel : ObservableObject
    {
        private readonly IProjectService _projectService;

        public ObservableCollection<FinancialProject> Projects { get; } = new();

        public AllProjectsViewModel(IProjectService projectService)
        {
            _projectService = projectService;

            LoadProjects();

            WeakReferenceMessenger.Default.Register<ProjectUpdatedMessage>(this, (_, _) => LoadProjects());
        }

        private void LoadProjects()
        {
            var allProjects = _projectService.GetAllProjects();

            Projects.Clear();

            foreach (var project in allProjects)
            {
                Projects.Add(project);
            }
        }

        [RelayCommand]
        private async Task OpenProjectAsync(FinancialProject? project)
        {
            if (project is null || Shell.Current is null)
            {
                return;
            }

            await Shell.Current.GoToAsync($"ProjectDetailPage?ProjectId={project.Id}");
        }

        [RelayCommand]
        private async Task AddNewProjectAsync()
        {
            if (Shell.Current is null)
            {
                return;
            }

            var viewModel = new AddProjectViewModel(_projectService);
            var page = new AddProjectPage(viewModel);

            await Shell.Current.Navigation.PushModalAsync(page);
        }

        [RelayCommand]
        private async Task DeleteProjectAsync(FinancialProject? project)
        {
            if (project is null)
            {
                return;
            }

            bool answer = await GetCurrentPage().DisplayAlertAsync(
                "Удаление",
                $"Удалить проект '{project.Title}'?",
                "Да",
                "Отмена");

            if (!answer)
            {
                return;
            }

            _projectService.DeleteProject(project.Id);
            Projects.Remove(project);
            WeakReferenceMessenger.Default.Send(new ProjectUpdatedMessage());
        }

        private static Page GetCurrentPage()
        {
            return Shell.Current?.CurrentPage
                   ?? Application.Current?.Windows.FirstOrDefault()?.Page
                   ?? throw new InvalidOperationException("Активная страница не найдена.");
        }
    }
}