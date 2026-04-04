using Finalitika10.Models;

namespace Finalitika10.Services.Interfaces.DocumentsService
{

    public interface IDocumentGeneratorStrategy
    {
        ExportFormat Format { get; }
        Task<DocumentResult> GenerateAsync(DocumentRequest request);
    }

    public class GenerateDocumentUseCase
    {
        private readonly IEnumerable<IDocumentGeneratorStrategy> _generators;

        public GenerateDocumentUseCase(IEnumerable<IDocumentGeneratorStrategy> generators)
        {
            _generators = generators;
        }

        public async Task<List<DocumentResult>> ExecuteAsync(DocumentRequest request)
        {
            var tasks = _generators
                .Where(s => request.RequestedFormats.Contains(s.Format))
                .Select(s => s.GenerateAsync(request));

            return (await Task.WhenAll(tasks)).ToList();
        }
    }
}
