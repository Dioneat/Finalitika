using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finalitika10.Models;

namespace Finalitika10.ViewModels
{
    [QueryProperty(nameof(Documents), "GeneratedDocuments")]
    public partial class PdfPreviewViewModel : ObservableObject
    {
        [ObservableProperty]
        private List<DocumentResult> documents;

        [RelayCommand]
        private async Task DownloadAsync(string formatString)
        {
            if (!Enum.TryParse<ExportFormat>(formatString, out var format)) return;

            var doc = Documents?.FirstOrDefault(d => d.Format == format);
            if (doc == null) return;

            string tempFilePath = Path.Combine(FileSystem.CacheDirectory, doc.FileName);
            await File.WriteAllBytesAsync(tempFilePath, doc.Content);

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = $"Сохранить документ {doc.FileName}",
                File = new ShareFile(tempFilePath)
            });
        }
    }
}
