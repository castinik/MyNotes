using MyNotes.Models;
using MyNotes.Services;
using MyNotes.ViewModels;

namespace MyNotes.Pages
{
    public partial class FolderPage : ContentPage, IQueryAttributable
    {
        private readonly INoteService _noteService;
        private readonly FolderViewModel _vm;

        public FolderPage(INoteService noteService, FolderViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
            _vm = vm;
            _noteService = noteService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            ConfirmDeleteSelected.IsVisible = false;
            PopupInfoNote.IsVisible = false;

            String folderName = Preferences.Get("tempFolder", null);
            if (!string.IsNullOrEmpty(folderName))
            {
                // Carica il folder usando il nome memorizzato nelle Preferences
                this.Folder = new Folder { Name = folderName };
            }
            await _vm.LoadNotesAsync();
        }

        public Folder Folder
        {
            get => (BindingContext as FolderViewModel)?.Folder;
            set
            {
                var viewModel = BindingContext as FolderViewModel;
                if (viewModel != null)
                {
                    viewModel.Folder = value;
                }
            }
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
                _vm.IsNoteCheckerVisible = false;
                _vm.IsInfoNoteVisible = true;
                //await _vm.UnsetElementChecker();
            }
            else
            {
                ConfirmDeleteSelected.IsVisible = true;
                _vm.IsNoteCheckerVisible = true;
                _vm.IsInfoNoteVisible = false;
            }
        }
        private async void DeleteSelected(object sender, EventArgs e)
        {
            ConfirmDeleteSelected.IsVisible = false;
            _vm.IsNoteCheckerVisible = false;
            _vm.IsInfoNoteVisible = true;
            await _vm.DeleteElementsChecked();
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
                Note note = button.CommandParameter as Note;

                InfoNoteName.Text = note.Title;
                InfoNoteCreated.Text = note.Created.ToString("dd/MM/yy HH:mm");
                InfoNoteModified.Text = note.Modify.ToString("dd/MM/yy HH:mm");

                PopupInfoNote.IsVisible = true;
            }
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("Folder", out var folderObj) && folderObj is Folder folder)
            {
                _vm.Folder = folder;
            }
        }
    }
}
