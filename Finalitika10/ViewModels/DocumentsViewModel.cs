using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finalitika10.Models;
using Finalitika10.Views;
using System.Collections.ObjectModel;

namespace Finalitika10.ViewModels
{
    public partial class DocumentsViewModel : ObservableObject
    {
        public ObservableCollection<DocumentTemplateModel> AvailableDocuments { get; } = new()
        {
       new DocumentTemplateModel
        {
            Title = "Заявление об увольнении",
            DocumentType = "ResignationLetter" 
        },
       new DocumentTemplateModel
        {
            Title = "Договор беспроцентного займа",
            DocumentType = "LoanAgreement" 
        },
       new DocumentTemplateModel
        {
            Title = "Расписка о получении денег",
            DocumentType = "MoneyReceipt"
        },
       new DocumentTemplateModel
        {
            Title = "Исковое заявление о взыскании долга",
            DocumentType = "ClaimStatement"
        },
       new DocumentTemplateModel
        {
            Title = "Заявление на ежегодный отпуск",
            DocumentType = "PaidLeaveApplication"
        },
        new DocumentTemplateModel
        {
            Title = "Заявление на отпуск за свой счет",
            DocumentType = "UnpaidLeaveApplication"
        },
        new DocumentTemplateModel
        {
            Title = "Декларация 3-НДФЛ",
            DocumentType = "Ndfl3Declaration"
        }
    };

        [RelayCommand]
        private async Task OpenEditorAsync(DocumentTemplateModel documentType)
        {
            await Shell.Current.GoToAsync($"{nameof(DocumentEditorPage)}?DocType={documentType.DocumentType}");
        }
    }
}
