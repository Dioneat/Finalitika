using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finalitika10.Models;
using Microsoft.Maui.ApplicationModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Storage;

namespace Finalitika10.ViewModels
{
    [QueryProperty(nameof(Documents), "GeneratedDocuments")]
    public partial class PdfPreviewViewModel : ObservableObject
    {
        private List<DocumentResult> _documents;

        public List<DocumentResult> Documents
        {
            get => _documents;
            set
            {
                _documents = value;
                OnPropertyChanged(nameof(Documents));

                PreparePdfPreview();
                OnPropertyChanged(nameof(HasXml));
            }
        }

        [ObservableProperty]
        private string pdfFilePath;

        [ObservableProperty]
        private bool isPdfLoading = true; 

        public bool HasXml => Documents?.Any(d => d.Format == ExportFormat.Xml) ?? false;

        private async void PreparePdfPreview()
        {
            IsPdfLoading = true;

            var pdfDoc = Documents?.FirstOrDefault(d => d.Format == ExportFormat.Pdf);
            if (pdfDoc != null)
            {
                string tempPath = Path.Combine(FileSystem.CacheDirectory, pdfDoc.FileName);
                await File.WriteAllBytesAsync(tempPath, pdfDoc.Content);

                await Launcher.Default.OpenAsync(new OpenFileRequest("Документ", new ReadOnlyFile(tempPath)));


                await Shell.Current.Navigation.PopAsync();
            }

            IsPdfLoading = false;
        }

        [RelayCommand]
        private async Task DownloadAsync(string formatString)
        {
            if (!Enum.TryParse<ExportFormat>(formatString, out var format)) return;

            var doc = Documents?.FirstOrDefault(d => d.Format == format);
            if (doc == null) return;

            string tempFilePath = Path.Combine(FileSystem.CacheDirectory, doc.FileName);

            if (!File.Exists(tempFilePath))
            {
                await File.WriteAllBytesAsync(tempFilePath, doc.Content);
            }

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = $"Отправить {doc.FileName}",
                File = new ShareFile(tempFilePath)
            });
        }

        [RelayCommand]
        private async Task CloseAsync()
        {
            await Shell.Current.Navigation.PopAsync();
        }
    }
}