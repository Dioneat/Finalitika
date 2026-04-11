using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finalitika10.Models;
using Finalitika10.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Finalitika10.ViewModels
{
    public partial class DocumentsViewModel : ObservableObject
    {
        private readonly List<DocumentTemplateModel> _allDocuments;

        public ObservableCollection<DocumentTemplateModel> FilteredDocuments { get; } = new();

        [ObservableProperty]
        private string searchQuery = "";

        partial void OnSearchQueryChanged(string value)
        {
            FilterDocuments();
        }

        public DocumentsViewModel()
        {
            _allDocuments = new List<DocumentTemplateModel>
            {
                new DocumentTemplateModel { Title = "Декларация 3-НДФЛ", DocumentType = "Ndfl3Declaration", Icon = "🏛️", Description = "Оформление налогового вычета (возврат 13%)", IconBgColor = "#FDEDEC", Tag = "Топ", TagColor = "#E74C3C" },
                new DocumentTemplateModel { Title = "Договор займа", DocumentType = "LoanAgreement", Icon = "🤝", Description = "Беспроцентный займ между физ. лицами", IconBgColor = "#FEF5E7", Tag = "Важно", TagColor = "#F39C12" },
                new DocumentTemplateModel { Title = "Расписка о получении", DocumentType = "MoneyReceipt", Icon = "✍️", Description = "Юридическое подтверждение передачи денег", IconBgColor = "#F4F6F8" },
                new DocumentTemplateModel { Title = "Исковое заявление", DocumentType = "ClaimStatement", Icon = "⚖️", Description = "Взыскание долга через суд", IconBgColor = "#EBDEF0" },
                new DocumentTemplateModel { Title = "Заявление на отпуск", DocumentType = "PaidLeaveApplication", Icon = "🌴", Description = "Ежегодный оплачиваемый отпуск", IconBgColor = "#E8F8F5" },
                new DocumentTemplateModel { Title = "Отпуск за свой счет", DocumentType = "UnpaidLeaveApplication", Icon = "⏳", Description = "Заявление на отпуск без сохранения ЗП", IconBgColor = "#E8F8F5" },
                new DocumentTemplateModel { Title = "Увольнение", DocumentType = "ResignationLetter", Icon = "🚪", Description = "Заявление по собственному желанию", IconBgColor = "#EAEDED" }
            };

            FilterDocuments();
        }

        private void FilterDocuments()
        {
            FilteredDocuments.Clear();

            var filtered = string.IsNullOrWhiteSpace(SearchQuery)
                ? _allDocuments
                : _allDocuments.Where(d =>
                    d.Title.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                    d.Description.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));

            foreach (var doc in filtered)
            {
                FilteredDocuments.Add(doc);
            }
        }

        [RelayCommand]
        private async Task OpenEditorAsync(DocumentTemplateModel document)
        {
            if (document == null) return;
            await Shell.Current.GoToAsync($"{nameof(DocumentEditorPage)}?DocType={document.DocumentType}");
        }
    }
}