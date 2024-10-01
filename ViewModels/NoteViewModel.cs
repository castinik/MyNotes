using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Controls;
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
    public partial class NoteViewModel : ObservableObject
    {
        private readonly INoteService _noteService;

        [ObservableProperty]
        Boolean isBusy;

        [ObservableProperty]
        Note note;

        public ICommand SaveNoteCommand { get; }
        public ICommand ComeBackCommand { get; }

        public NoteViewModel(INoteService noteService)
        {
            _noteService = noteService;
            note = new Note();
            SaveNoteCommand = new Command(async () => await SaveNoteAsync());
            ComeBackCommand = new Command(async () => await ComeBackAsync());
        }

        private async Task SaveNoteAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                // Recupero dati per gestione salavataggio
                String folderWork = Preferences.Get("tempFolder", null);
                String noteChooseWork = Preferences.Get("tempNote", null);
                ObservableCollection<Note> notesDb = await _noteService.LoadNotesAsync(folderWork);

                // CREATION - nuova nota
                Guid idNoteChooseWork = Guid.Empty;
                if (!Guid.TryParse(noteChooseWork, out idNoteChooseWork))
                {
                    // Se il titolo è lo stesso di un'altra nota in DB - esci
                    if (notesDb.Select(n => n.Title == Note.Title).Any(b => b == true))
                    {
                        await Shell.Current.DisplayAlert("Uh Oh!", "You have already used this title", "Ok");
                        IsBusy = false;
                        return;
                    }
                }
                // MODIFY - modifica della nota
                else
                {
                    // Se il titolo è lo stesso di un'altra nota in DB - esci
                    if (notesDb.Select(n => n.Title == Note.Title && n.Id != idNoteChooseWork).Any(b => b == true))
                    {
                        await Shell.Current.DisplayAlert("Uh Oh!", "You have already used this title", "Ok");
                        IsBusy = false;
                        return;
                    }
                    else
                    {
                        // Ottengo la vecchia nota
                        Note oldNote = notesDb.Where(n => n.Id == idNoteChooseWork).FirstOrDefault();
                        // Elimina nota per poi risalvarla
                        if (!NoteService.DeleteNote(oldNote, folderWork))
                        {
                            await Shell.Current.DisplayAlert("Uh Oh!", "Something went wrong", "Ok");
                            IsBusy = false;
                            return;
                        }
                        Note.Modify = DateTime.Now;
                    }
                }

                // Se passa i check salvo la nota
                await _noteService.SaveNoteAsync(Note, folderWork);
                Preferences.Remove("tempNote");

                // Verifico se la nota è stata modificata in una cartella; 
                String pageToRedirect = String.Empty;
                // Se tempFolder non è popolata
                if (String.IsNullOrEmpty(Preferences.Get("tempFolder", null)))
                    pageToRedirect = $"///{nameof(MainPage)}";
                else
                    pageToRedirect = $"///{nameof(FolderPage)}";

                IsBusy = false;
                // Naviga indietro alla pagina precedente
                await Shell.Current.GoToAsync(pageToRedirect);
                return;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Uh Oh!", "Something went wrong", "Ok");
                Preferences.Remove("tempNote");
                // Verifico se la nota è stata modificata in una cartella; 
                String pageToRedirect = String.Empty;
                // Se tempFolder non è popolata
                if (String.IsNullOrEmpty(Preferences.Get("tempFolder", null)))
                    pageToRedirect = $"///{nameof(MainPage)}";
                else
                    pageToRedirect = $"///{nameof(FolderPage)}";

                IsBusy = false;
                // Naviga indietro alla pagina precedente
                await Shell.Current.GoToAsync(pageToRedirect);
            }
        }
    
        private async Task ComeBackAsync()
        {
            // Verifico se la nota è stata modificata in una cartella; 
            String pageToRedirect = String.Empty;
            // Se tempFolder non è popolata
            if (String.IsNullOrEmpty(Preferences.Get("tempFolder", null)))
                pageToRedirect = $"///{nameof(MainPage)}";
            else
                pageToRedirect = $"///{nameof(FolderPage)}";
            // Naviga indietro alla pagina precedente
            await Shell.Current.GoToAsync(pageToRedirect);
            // await Shell.Current.GoToAsync("..");
        }
    }
}
