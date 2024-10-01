using MyNotes.Models;
using MyNotes.Services;
using MyNotes.ViewModels;

namespace MyNotes.Pages
{
    public partial class MainPage : ContentPage
    {
        private readonly INoteService _noteService;
        private MainViewModel _vm;

        public MainPage(INoteService noteService, MainViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
            _vm = vm;
            _noteService = noteService;
        }

        protected override async void OnAppearing()
        {
            await RequestStoragePermissions();
            PopupAddFolder.IsVisible = false;
            PopupInfoNote.IsVisible = false;
            ConfirmDeleteSelected.IsVisible = false;
            base.OnAppearing();
            Preferences.Clear();

            // controllo del primo avvio per avere una directory dove salvare i file
            String pathFiles = NoteService.ReadInfo("pathfiles");
            if (String.IsNullOrEmpty(pathFiles))
            {
                //FolderPickerResult folder = await _folderPicker.PickAsync();
                //String path = folder.Folder.Path;
                NoteService.WriteInfo("pathfiles", Data.DefaultData);
            }

            await _vm.LoadFoldersAsync();
            await _vm.LoadNotesAsync();
        }

        public async Task RequestStoragePermissions()
        {
            PermissionStatus statusRead = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            PermissionStatus statusWrite = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();

            if (statusRead != PermissionStatus.Granted)
            {
                statusRead = await Permissions.RequestAsync<Permissions.StorageRead>();
            }
            if (statusWrite != PermissionStatus.Granted)
            {
                statusWrite = await Permissions.RequestAsync<Permissions.StorageWrite>();
            }

            //ContextCompat.CheckSelfPermission
            //if (status != PermissionStatus.Granted)
            //{
            //    status = await Permissions.RequestAsync<Permissions.StorageWrite>();
            //    if (status != PermissionStatus.Granted)
            //    {
            //        Application.Current.Quit();
            //    }
            //    status = await Permissions.RequestAsync<Permissions.StorageRead>();
            //    if (status != PermissionStatus.Granted)
            //    {
            //        Application.Current.Quit();
            //    }
            //}
        }

        private async void OnAddNoteClicked(object sender, EventArgs e)
        {
            var noteViewModel = new NoteViewModel(_noteService); // Pass INoteService instance
            await Navigation.PushAsync(new NotePage(noteViewModel));
        }

        private async void SelectToDelete(object sender, EventArgs e)
        {
            if (ConfirmDeleteSelected.IsVisible)
            {
                ConfirmDeleteSelected.IsVisible = false;
                _vm.IsFolderCheckerVisible = false;
                _vm.IsInfoNoteVisible = true;
                await _vm.UnsetElementChecker();
            }
            else
            {
                ConfirmDeleteSelected.IsVisible = true;
                _vm.IsFolderCheckerVisible = true;
                _vm.IsInfoNoteVisible = false;
            }
        }
        private async void DeleteSelected(object sender, EventArgs e)
        {
            ConfirmDeleteSelected.IsVisible = false;
            _vm.IsFolderCheckerVisible = false;
            _vm.IsInfoNoteVisible = true;
            await _vm.DeleteElementsChecked();
        }

        private void CreateNewFolder(object sender, EventArgs e)
        {
            String folderName = EntryFolderName.Text;
            _vm.AddNewFolder(folderName);
            EntryFolderName.Text = String.Empty;
            PopupAddFolder.IsVisible = false;
        }
        private void CloseAddFolder(object sender, EventArgs e)
        {
            EntryFolderName.Text = String.Empty;
            PopupAddFolder.IsVisible = false;
        }
        private void OpenAddFolder(object sender, EventArgs e)
        {
            PopupAddFolder.IsVisible = true;
        }
    
        private void OpenCloseInfoNote(object sender, EventArgs e)
        {
            if (PopupInfoNote.IsVisible)
            {
                PopupInfoNote.IsVisible = false;
            }
            else
            {
                ImageButton button = sender as ImageButton;
                Note note =  button.CommandParameter as Note;

                InfoNoteName.Text = note.Title;
                InfoNoteCreated.Text = note.Created.ToString("dd/MM/yy HH:mm");
                InfoNoteModified.Text = note.Modify.ToString("dd/MM/yy HH:mm");

                PopupInfoNote.IsVisible = true;
            }
        }
    }
}
