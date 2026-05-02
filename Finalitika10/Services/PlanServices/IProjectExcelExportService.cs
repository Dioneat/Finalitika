using Finalitika10.Models;

namespace Finalitika10.Services.PlanServices
{
    public interface IProjectExcelExportService
    {
        Task<ProjectExcelExportResult> ExportAsync(FinancialProject project, CancellationToken cancellationToken = default);
    }

    public sealed class ProjectExcelExportResult
    {
        public bool IsSuccess { get; init; }
        public string Message { get; init; } = string.Empty;
        public string? FilePath { get; init; }
    }
}