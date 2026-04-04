using Finalitika10.Models;
using Finalitika10.Services.Interfaces.DocumentsService;
using NPetrovich;

namespace Finalitika10.Services.DocumentsServices
{
    public class PetrovichDeclensionService : INameDeclensionService
    {

        public string DeclineName(string fullName, NameCase targetCase)
        {
            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) return fullName;

            var petrovich = new Petrovich
            {
                LastName = parts[0],
                FirstName = parts[1],
                MiddleName = parts.Length > 2 ? parts[2] : "",
                AutoDetectGender = true 
            };

            var nPetrovichCase = ConvertCase(targetCase);

            var inflected = petrovich.InflectTo(nPetrovichCase);

            return $"{inflected.LastName} {inflected.FirstName} {inflected.MiddleName}".Trim();
        }


        private Case ConvertCase(NameCase nameCase) => nameCase switch
        {
            NameCase.Genitive => Case.Genitive,
            NameCase.Dative => Case.Dative,
            NameCase.Accusative => Case.Accusative,
            NameCase.Instrumental => Case.Instrumental,
            NameCase.Prepositional => Case.Prepositional,
            _ => Case.Nominative
        };
    }
}
