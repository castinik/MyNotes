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

namespace MyNotes.ViewModels
{
    public partial class FolderViewModel : ObservableObject
    {
        private readonly INoteService _noteService;

        public ICommand NoteTappedCommand { get; }
        public ICommand NoteDeleteCommand { get; }
        public ICommand ComeBackCommand { get; }

        [ObservableProperty]
        Boolean isBusy;
        [ObservableProperty]
        Boolean isNotesCount;
        [ObservableProperty]
        Boolean isNoteCheckerVisible;
        [ObservableProperty]
        Boolean isInfoNoteVisible;
        [ObservableProperty]
        ObservableCollection<Note> notes;
        [ObservableProperty]
        Folder folder;

        public FolderViewModel(INoteService noteService)
        {
            _noteService = noteService;
            IsNoteCheckerVisible = false;
            IsInfoNoteVisible = true;
            Notes = new ObservableCollection<Note>();
            NoteTappedCommand = new Command<Note>(OnNoteTapped);
            NoteDeleteCommand = new Command<Note>(OnNoteDelete);
            ComeBackCommand = new Command(async () => await ComeBackAsync());
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
                if (Notes.Count > 0)
                {
                    List<Note> listNotes = Notes.OrderByDescending(n => n.Created).ToList();
                    Notes.Clear();
                    foreach (Note n in listNotes)
                    {
                        Notes.Add(n);
                    }
                    IsNotesCount = false;
                }
                else
                {
                    IsNotesCount = true;
                }
                IsBusy = false;
            }
        }

        public async Task DeleteElementsChecked()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            Device.BeginInvokeOnMainThread(async () =>
            {
                // Prima elimino le note nella home selezionate
                List<Note> notesToDelete = Notes.Where(note => note.IsChecked).ToList();
                foreach (Note note in notesToDelete)
                {
                    if (note.IsChecked)
                    {
                        NoteService.DeleteNote(note, Folder.Name);
                        Notes.Remove(note);
                    }
                }
                IsBusy = false;
            });
        }

        private async void OnNoteTapped(Note note)
        {
            if (note == null)
                return;

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
    }
}
