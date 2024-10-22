using CommunityToolkit.Mvvm.ComponentModel;
using MyNotes.Models;
using MyNotes.Pages;
using MyNotes.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyNotes.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly INoteService _noteService;

        [ObservableProperty]
        Boolean isBusy;
        [ObservableProperty]
        Boolean isNotesCount;
        [ObservableProperty]
        Boolean isFoldersCount;
        [ObservableProperty]
        Boolean isPopUpFolderOpen;
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
        ObservableCollection<Folder> folders;

        public ICommand NoteTappedCommand { get; }
        public ICommand NoteDeleteCommand { get; }
        public ICommand FolderTappedCommand { get; }
        public ICommand LongPressCommand { get; }

        [ObservableProperty]
        Note infoNote;

        public MainViewModel(INoteService noteService)
        {
            _noteService = noteService;
            IsPopUpFolderOpen = false;
            IsPopUpInfoOpen = false;
            IsOnLongPressure = false;
            Notes = new ObservableCollection<Note>();
            NotesLeft = new ObservableCollection<Note>();
            NotesRight = new ObservableCollection<Note>();
            Folders = new ObservableCollection<Folder>();
            NoteTappedCommand = new Command<Note>(OnNoteTapped);
            NoteDeleteCommand = new Command<Note>(OnNoteDelete);
            FolderTappedCommand = new Command<Folder>(OnFolderTapped);

            LongPressCommand = new Command<Note>(OnLongPressCommand);
            IsOnTrashOpen = false;
        }

        public async Task LoadFoldersAsync(Boolean isOrding = false)
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                // è solo ordinamento della lista Folders; vado direttamente a "finally"
                if (isOrding)
                    return;

                ObservableCollection<Folder> folders = await _noteService.LoadFoldersAsync();
                Folders.Clear();
                foreach (Folder f in folders)
                {
                    Folders.Add(f);
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
                ReorderFolders();
                IsBusy = false;
            }
        }

        public async Task LoadNotesAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                ObservableCollection<Note> notes = await _noteService.LoadNotesAsync();

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

        public async Task AddNewFolder(String folderName)
        {
            if (IsBusy)
                return;

            try
            {
                if (String.IsNullOrEmpty(folderName))
                {
                    await Shell.Current.DisplayAlert("Uh Oh!", "Type a name for the folder", "Ok");
                    return;
                }
                else if (Folders.Where(f => f.Name == folderName).Any())
                {
                    await Shell.Current.DisplayAlert("Uh Oh!", "The folder name already exists", "Ok");
                    return;
                }
                else
                {
                    await _noteService.AddFolderAsync(folderName);
                    Folder newFolder = new Folder() { Name = folderName };
                    Folders.Add(newFolder);
                    LoadFoldersAsync(true);
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Uh Oh!", "Something went wrong", "Ok");
                return;
            }
            finally
            {
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
                    NoteService.DeleteNote(note);
                    Notes.Remove(note);
                    NotesLeft.Remove(note);
                    NotesRight.Remove(note);
                }
            }
            // Poi elimino le note dentro ciascuna cartella selezionata
            List<Folder> foldersToDelete = Folders.Where(folder => folder.IsChecked).ToList();
            foreach (Folder folder in foldersToDelete)
            {
                if (folder.IsChecked)
                {
                    NoteService.DeleteFolder(folder.Name);
                    Folders.Remove(folder);
                }
            }
            ReorderNotes();
            IsBusy = false;
        }

        public async void OnNoteTapped(Note note)
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
    
        public async void OnNoteDelete(Note note)
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
                    if (NoteService.DeleteNote(note))
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
    
        public async void OnFolderTapped(Folder folder)
        {
            if (folder == null)
                return;

            if (IsBusy)
                return;

            IsBusy = true;

            Preferences.Set("tempFolder", folder.Name);

            await Shell.Current.GoToAsync($"///{nameof(FolderPage)}", new Dictionary<string, object>
            {
                { "Folder", folder }
            });

            IsBusy = false;
        }

        public async void OnLongPressCommand(Note note)
        {
            IsOnLongPressure = true;
            IsPopUpInfoOpen = true;
            InfoNote = Notes.Where(n => n.Id == note.Id).First();
            //await Shell.Current.DisplayAlert("Pressione Prolungata", "Hai tenuto premuto sulla Label!", "OK");
        }
        
        public async void ReorderNotes()
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
    
        public void ReorderFolders()
        {
            if (Folders.Count > 0)
            {
                List<Folder> listFolder = Folders.OrderBy(n => n.Name).ToList();
                Folders.Clear();
                foreach (Folder f in listFolder)
                {
                    Folders.Add(f);
                }
                IsFoldersCount = false;
            }
            else
            {
                IsFoldersCount = true;
            }
        }
    }
}
