using System.Collections.ObjectModel;
using DidiApp.Models;
using DidiApp.Services;

namespace DidiApp.ViewModels;

public sealed class JournalViewModel : ViewModelBase
{
    private readonly DatabaseService _db = new();
    private NoteItem? _selectedNote;
    private string _currentNoteContent = string.Empty;

    public JournalViewModel()
    {
        Notes = new ObservableCollection<NoteItem>(_db.LoadNotes());
        
        SaveCommand = new RelayCommand(SaveNote, () => !string.IsNullOrWhiteSpace(CurrentNoteContent));
        NewNoteCommand = new RelayCommand(CreateNewNote);
        DeleteNoteCommand = new RelayCommand(DeleteNote, () => SelectedNote != null);
    }

    public ObservableCollection<NoteItem> Notes { get; }

    public NoteItem? SelectedNote
    {
        get => _selectedNote;
        set
        {
            if (SetProperty(ref _selectedNote, value))
            {
                CurrentNoteContent = value?.Content ?? string.Empty;
                DeleteNoteCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string CurrentNoteContent
    {
        get => _currentNoteContent;
        set
        {
            SetProperty(ref _currentNoteContent, value);
            SaveCommand.RaiseCanExecuteChanged();
        }
    }

    public RelayCommand SaveCommand { get; }
    public RelayCommand NewNoteCommand { get; }
    public RelayCommand DeleteNoteCommand { get; }

    private void SaveNote()
    {
        if (SelectedNote == null)
        {
            SelectedNote = new NoteItem { Content = CurrentNoteContent };
            Notes.Insert(0, SelectedNote);
        }
        else
        {
            SelectedNote.Content = CurrentNoteContent;
            SelectedNote.UpdatedAt = System.DateTime.UtcNow;
        }
        _db.UpsertNote(SelectedNote);
    }

    private void CreateNewNote()
    {
        SelectedNote = null;
        CurrentNoteContent = string.Empty;
    }

    private void DeleteNote()
    {
        if (SelectedNote == null) return;
        _db.DeleteNote(SelectedNote.Id);
        Notes.Remove(SelectedNote);
        CreateNewNote();
    }
}
