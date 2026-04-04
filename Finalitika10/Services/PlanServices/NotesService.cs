using Finalitika10.Models;
using System.Text.Json;

namespace Finalitika10.Services.PlanServices
{
    public interface INotesService
    {
        List<ProjectNote> GetAllNotes();
        void SaveNote(ProjectNote note);
        void DeleteNote(string id);
    }

    public class NotesService : INotesService
    {
        private const string NotesKey = "UserProjectNotes";

        public List<ProjectNote> GetAllNotes()
        {
            var json = Preferences.Default.Get(NotesKey, string.Empty);
            if (string.IsNullOrEmpty(json)) return new List<ProjectNote>();

            return JsonSerializer.Deserialize<List<ProjectNote>>(json) ?? new List<ProjectNote>();
        }

        public void SaveNote(ProjectNote note)
        {
            var notes = GetAllNotes();
            var existing = notes.FirstOrDefault(n => n.Id == note.Id);

            if (existing != null)
            {
                notes[notes.IndexOf(existing)] = note;
            }
            else
            {
                notes.Add(note);
            }

            Preferences.Default.Set(NotesKey, JsonSerializer.Serialize(notes));
        }
        public void DeleteNote(string id)
        {
            var notes = GetAllNotes();
            var noteToRemove = notes.FirstOrDefault(n => n.Id == id);
            if (noteToRemove != null)
            {
                notes.Remove(noteToRemove);
                Preferences.Default.Set(NotesKey, JsonSerializer.Serialize(notes));
            }
        }
    }
}