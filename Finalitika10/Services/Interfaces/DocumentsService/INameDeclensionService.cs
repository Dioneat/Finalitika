using Finalitika10.Models;

namespace Finalitika10.Services.Interfaces.DocumentsService
{
    public interface INameDeclensionService
    {
        string DeclineName(string fullName, NameCase targetCase);
    }
}
