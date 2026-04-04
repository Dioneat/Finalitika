using Finalitika10.Models;
using System.Text.Json;

namespace Finalitika10.Services.PlanServices
{
    public interface IProjectService
    {
        List<FinancialProject> GetAllProjects();
        FinancialProject? GetProjectById(string id);
        void SaveProject(FinancialProject project);
        void DeleteProject(string id);
    }

    public class ProjectService : IProjectService
    {
        private const string ProjectsKey = "UserFinancialProjects_v2";

        public List<FinancialProject> GetAllProjects()
        {
            var json = Preferences.Default.Get(ProjectsKey, string.Empty);

            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<FinancialProject>();
            }

            return JsonSerializer.Deserialize<List<FinancialProject>>(json) ?? new List<FinancialProject>();
        }

        public FinancialProject? GetProjectById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            return GetAllProjects().FirstOrDefault(p => p.Id == id);
        }

        public void SaveProject(FinancialProject project)
        {
            var projects = GetAllProjects();
            var existing = projects.FirstOrDefault(p => p.Id == project.Id);

            if (existing is not null)
            {
                projects[projects.IndexOf(existing)] = project;
            }
            else
            {
                projects.Add(project);
            }

            Preferences.Default.Set(ProjectsKey, JsonSerializer.Serialize(projects));
        }

        public void DeleteProject(string id)
        {
            var projects = GetAllProjects();
            var projectToRemove = projects.FirstOrDefault(p => p.Id == id);

            if (projectToRemove is null)
            {
                return;
            }

            projects.Remove(projectToRemove);
            Preferences.Default.Set(ProjectsKey, JsonSerializer.Serialize(projects));
        }
    }
}