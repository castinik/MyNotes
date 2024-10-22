using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Mvvm.ComponentModel;
using MyNotes.Models;
using MyNotes.Pages;
using MyNotes.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Dispatching;
using Folder = MyNotes.Models.Folder;

namespace MyNotes.ViewModels
{
    public partial class FolderViewModel : ObservableObject
    {
        private readonly INoteService _noteService;

        public ICommand NoteTappedCommand { get; }
        public ICommand NoteDeleteCommand { get; }
        public ICommand ComeBackCommand { get; }
        public ICommand LongPressCommand { get; }

        [ObservableProperty]
        Boolean isBusy;
        [ObservableProperty]
        Boolean isNotesCount;
        [ObservableProperty]
        Boolean isNoteCheckerVisible;
        [ObservableProperty]
        Boolean isPopUpInfoOpen;
        [ObservableProperty]
        Boolean isOnLongPressure;
        [ObservableProperty]
        Boolean isOnTrashOpen;
        [ObservableProperty]
        ObservableCollection<Note> notes;
        [ObservableProperty]
        ObservableCollection<Note> notesLeft;
        [ObservableProperty]
        ObservableCollection<Note> notesRight;
        [ObservableProperty]
        Folder folder;

        [ObservableProperty]
        Note infoNote;

        public FolderViewModel(INoteService noteService)
        {
            _noteService = noteService;
            IsNoteCheckerVisible = false;
            IsPopUpInfoOpen = false;
            IsOnLongPressure = false;
            IsOnTrashOpen = false;
            Notes = new ObservableCollection<Note>();
            NotesLeft = new ObservableCollection<Note>();
            NotesRight = new ObservableCollection<Note>();
            NoteTappedCommand = new Command<Note>(OnNoteTapped);
            NoteDeleteCommand = new Command<Note>(OnNoteDelete);
            ComeBackCommand = new Command(async () => await ComeBackAsync());
            LongPressCommand = new Command<Note>(OnLongPressCommand);
        }

        public async Task LoadNotesAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                ObservableCollection<Note> notes = await _noteService.LoadNotesAsync(Folder.Name);

                Notes.Clear();
                NotesLeft.Clear();
                NotesRight.Clear();
                foreach (Note n in notes)
                {
                    Notes.Add(n);
                }
                return;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Uh Oh!", "Something went wrong", "Ok");
                return;
            }
            finally
            {
                ReorderNotes();
                IsBusy = false;
            }
        }

        public void DeleteElementsChecked()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            // Prima elimino le note nella home selezionate
            List<Note> notesToDelete = new List<Note>();
            notesToDelete.AddRange(NotesLeft.Where(note => note.IsChecked).ToList());
            notesToDelete.AddRange(NotesRight.Where(note => note.IsChecked).ToList());
            foreach (Note note in notesToDelete)
            {
                if (note.IsChecked)
                {
                    NoteService.DeleteNote(note, Folder.Name);
                    Notes.Remove(note);
                    NotesLeft.Remove(note);
                    NotesRight.Remove(note);
                }
            }
            ReorderNotes();
            IsBusy = false;
        }

        private async void OnNoteTapped(Note note)
        {
            if (note == null || IsOnLongPressure)
                return;

            if (IsOnTrashOpen)
            {
                note.IsChecked = true;
                return;
            }

            Preferences.Set("tempNote", note.Id.ToString());

            await Shell.Current.GoToAsync($"///{nameof(NotePage)}", new Dictionary<string, object>
            {
                { "Note", note }
            });
        }

        private async void OnNoteDelete(Note note)
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                String response = await Shell.Current.DisplayActionSheet("Delete?", "Cancel", null, new String[] { "Yes" });
                if (response == null || response == "Cancel")
                {
                    return;
                }
                else
                {
                    if (NoteService.DeleteNote(note, Folder.Name))
                    {
                        Notes.Remove(note);
                        return;
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("Uh Oh!", "Something went wrong", "Ok");
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                await Shell.Current.DisplayAlert("Uh Oh!", "Something went wrong", "Ok");
                return;
            }
            finally
            {
                IsNotesCount = Notes.Count == 0 ? true : false;
                IsBusy = false;
            }
        }

        private async Task ComeBackAsync()
        {
            // Naviga indietro alla pagina precedente
            await Shell.Current.GoToAsync($"///{nameof(MainPage)}");
            //await Shell.Current.GoToAsync("..");
        }

        public async void OnLongPressCommand(Note note)
        {
            IsOnLongPressure = true;
            IsPopUpInfoOpen = true;
            InfoNote = Notes.Where(n => n.Id == note.Id).First();
            //await Shell.Current.DisplayAlert("Pressione Prolungata", "Hai tenuto premuto sulla Label!", "OK");
        }

        public void ReorderNotes()
        {
            if (Notes.Count > 0)
            {
                Int32 count = 0;
                List<Note> listNotes = Notes.OrderByDescending(n => n.Created).ToList();
                Notes.Clear();
                NotesLeft.Clear();
                NotesRight.Clear();
                foreach (Note n in listNotes)
                {
                    Notes.Add(n);
                    if (count % 2 == 0)
                    {
                        NotesLeft.Add(n);
                    }
                    else
                    {
                        NotesRight.Add(n);
                    }
                    count++;
                }
                IsNotesCount = false;
            }
            else
            {
                IsNotesCount = true;
            }
        }
    }
}
