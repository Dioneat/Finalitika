using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Finalitika10.Models;
using Finalitika10.Services.PlanServices;
using Markdig;

namespace Finalitika10.ViewModels.PlanViewModels
{
    [QueryProperty(nameof(NoteId), "NoteId")]
    public partial class NoteViewModel : ObservableObject
    {
        private readonly INotesService _notesService;
        private ProjectNote _currentNote;

        [ObservableProperty] private string noteTitle;
        [ObservableProperty] private string rawMarkdownText;
        [ObservableProperty] private HtmlWebViewSource htmlPreviewSource = new();
        [ObservableProperty] private bool isEditMode = true; 
        public bool IsPreviewMode => !IsEditMode;

        private string _noteId;
        public string NoteId
        {
            get => _noteId;
            set
            {
                _noteId = value;
                LoadExistingNote(value);
            }
        }
        public string ModeButtonText => IsEditMode ? "Готово" : "Редактировать";
        public NoteViewModel(INotesService notesService)
        {
            _notesService = notesService;

            _currentNote = new ProjectNote
            {
                ColorHex = GetRandomColor() 
            };
            NoteTitle = _currentNote.Title;
            RawMarkdownText = _currentNote.MarkdownText;
        }
        private void LoadExistingNote(string id)
        {
            var note = _notesService.GetAllNotes().FirstOrDefault(n => n.Id == id);
            if (note != null)
            {
                _currentNote = note;
                NoteTitle = note.Title;
                RawMarkdownText = note.MarkdownText;

                IsEditMode = string.IsNullOrWhiteSpace(note.MarkdownText);
                OnPropertyChanged(nameof(IsPreviewMode));
                UpdatePreview();
            }
        }

        [RelayCommand]
        private void ToggleMode()
        {
            IsEditMode = !IsEditMode;
            OnPropertyChanged(nameof(IsPreviewMode));

            OnPropertyChanged(nameof(ModeButtonText));

            if (!IsEditMode)
            {
                UpdatePreview();
                SaveNote();
            }
        }

        private void SaveNote()
        {
            _currentNote.Title = string.IsNullOrWhiteSpace(NoteTitle) ? "Без названия" : NoteTitle;
            _currentNote.MarkdownText = RawMarkdownText;

            _currentNote.PreviewText = RawMarkdownText?
                .Replace("#", "")
                .Replace("*", "")
                .Trim();

            if (_currentNote.PreviewText?.Length > 100)
                _currentNote.PreviewText = _currentNote.PreviewText.Substring(0, 100) + "...";

            _notesService.SaveNote(_currentNote);

            WeakReferenceMessenger.Default.Send(new NotesUpdatedMessage());
        }
        private string GetRandomColor()
        {
            var colors = new[] { "#FFF9C4", "#E8F8F5", "#E8EAF6", "#FCE4EC", "#E3F2FD" };
            return colors[new Random().Next(colors.Length)];
        }

        private void UpdatePreview()
        {
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

            string htmlBody = Markdown.ToHtml(RawMarkdownText ?? string.Empty, pipeline);

            string fullHtml = $@"
            <html>
            <head>
                <meta name='viewport' content='width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=0'/>
                <style>
                    body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; padding: 15px; color: #2C3E50; line-height: 1.6; }}
                    h1, h2, h3 {{ color: #16A085; }}
                    ul {{ padding-left: 20px; }}
                    li {{ margin-bottom: 5px; }}
                    strong {{ color: #27AE60; }}
                    code {{ background-color: #F4F6F8; padding: 2px 4px; border-radius: 4px; color: #E74C3C; }}
                </style>
            </head>
            <body>
                {htmlBody}
            </body>
            </html>";

            HtmlPreviewSource.Html = fullHtml;
        }
    }
}
