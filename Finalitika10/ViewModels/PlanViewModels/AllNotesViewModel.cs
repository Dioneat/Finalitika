using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Finalitika10.Models;
using Finalitika10.Services.PlanServices;
using System.Collections.ObjectModel;

namespace Finalitika10.ViewModels.PlanViewModels
{
    public partial class AllNotesViewModel : ObservableObject
    {
        private readonly INotesService _notesService;

        public ObservableCollection<ProjectNote> Notes { get; } = new();

        public AllNotesViewModel(INotesService notesService)
        {
            _notesService = notesService;
            LoadNotes();
            WeakReferenceMessenger.Default.Register<NotesUpdatedMessage>(this, (r, m) =>
            {
                LoadNotes();
            });
        }

        private void LoadNotes()
        {
            Notes.Clear();
            var allNotes = _notesService.GetAllNotes().OrderByDescending(n => n.CreatedAt).ToList();

            foreach (var note in allNotes)
            {
                Notes.Add(note);
            }
        }

        [RelayCommand]
        private async Task OpenNoteAsync(ProjectNote note)
        {
            if (note == null) return;
            await Shell.Current.GoToAsync($"NotePage?NoteId={note.Id}");
        }

        [RelayCommand]
        private async Task AddNewNoteAsync()
        {
            await Shell.Current.GoToAsync("NotePage");
        }

        [RelayCommand]
        private async Task DeleteNoteAsync(ProjectNote note)
        {
            if (note == null) return;

            bool answer = await Shell.Current.DisplayAlertAsync(
                "Удаление",
                $"Вы уверены, что хотите удалить заметку '{note.Title}'?",
                "Да, удалить",
                "Отмена");

            if (answer)
            {
                _notesService.DeleteNote(note.Id);
                Notes.Remove(note); 

                WeakReferenceMessenger.Default.Send(new NotesUpdatedMessage());
            }
        }
    }
}
