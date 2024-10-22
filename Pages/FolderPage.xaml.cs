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
            String folderName = Preferences.Get("tempFolder", null);
            if (!string.IsNullOrEmpty(folderName))
            {
                // Carica il folder usando il nome memorizzato nelle Preferences
                this.Folder = new Folder { Name = folderName };
            }
            await _vm.LoadNotesAsync();
            RefreshNotes();
        }

        protected override bool OnBackButtonPressed()
        {
            ComeBack(new object(), new EventArgs());
            return true;
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

        private async void OnNoteTapped(object sender, TappedEventArgs e)
        {
            Note note = e.Parameter as Note;
            if (_vm.IsOnTrashOpen)
            {
                Frame frame = sender as Frame;
                Frame parentFrame = frame.Parent.Parent as Frame;
                if (note.IsChecked)
                {
                    parentFrame.BorderColor = Color.FromArgb("#005792");
                    parentFrame.BackgroundColor = Color.Parse("#26005792");
                    note.IsChecked = false;
                }
                else
                {
                    parentFrame.BorderColor = Color.FromArgb("#C92E2E");
                    parentFrame.BackgroundColor = Color.FromArgb("#613232");
                    note.IsChecked = true;
                }
            }
            else
            {
                _vm.NoteTappedCommand.Execute(note);
            }
        }

        private async void SelectToDelete(object sender, EventArgs e)
        {
            if (ConfirmDeleteSelected.IsVisible)
            {
                ConfirmDeleteSelected.IsVisible = false;
                _vm.IsOnTrashOpen = false;
                UncheckAllElements();
            }
            else
            {
                ConfirmDeleteSelected.IsVisible = true;
                _vm.IsOnTrashOpen = true;
            }
        }
        private async void DeleteSelected(object sender, EventArgs e)
        {
            ConfirmDeleteSelected.IsVisible = false;
            _vm.IsOnTrashOpen = false;
            await this.Dispatcher.DispatchAsync(() =>
            {
                _vm.DeleteElementsChecked();
                RefreshNotes();
            });
            UncheckAllElements();
        }
        
        private async void OnAddNoteClicked(object sender, EventArgs e)
        {
            if (_vm.IsOnTrashOpen)
            {
                UncheckAllElements();
            }
            var noteViewModel = new NoteViewModel(_noteService); // Pass INoteService instance
            await Navigation.PushAsync(new NotePage(noteViewModel));
        }
        private async void ComeBack(object sender, EventArgs e)
        {
            if (_vm.IsOnTrashOpen)
            {
                UncheckAllElements();
            }
            _vm.ComeBackCommand.Execute(null);
        }

        private void OpenCloseInfoNote(object sender, EventArgs e)
        {
            if (_vm.IsPopUpInfoOpen)
            {
                _vm.IsPopUpInfoOpen = false;
                _vm.IsOnLongPressure = false;

            }
        }
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("Folder", out var folderObj) && folderObj is Folder folder)
            {
                _vm.Folder = folder;
            }
        }

        #region ############### Functional ###############
        private void UncheckAllElements()
        {
            CollectionView collectionViewL = MainViewLeft;
            CollectionView collectionViewR = MainViewRight;
            foreach (var element in collectionViewL.GetVisualTreeDescendants())
            {
                if (element is Grid)
                {
                    Grid grid = element as Grid;
                    Frame frame = grid.Parent as Frame;
                    frame.BorderColor = Color.FromArgb("#005792");
                    frame.BackgroundColor = Color.Parse("#26005792");
                }
            }
            foreach (var element in collectionViewR.GetVisualTreeDescendants())
            {
                if (element is Grid)
                {
                    Grid grid = element as Grid;
                    Frame frame = grid.Parent as Frame;
                    frame.BorderColor = Color.FromArgb("#005792");
                    frame.BackgroundColor = Color.Parse("#26005792");
                }
            }
            foreach (Note note in _vm.NotesLeft)
            {
                note.IsChecked = false;
            }
            foreach (Note note in _vm.NotesRight)
            {
                note.IsChecked = false;
            }
        }
        private void RefreshNotes()
        {
            MainViewLeft.ItemsSource = null;
            MainViewLeft.ItemsSource = _vm.NotesLeft;
            MainViewRight.ItemsSource = null;
            MainViewRight.ItemsSource = _vm.NotesRight;
        }
        #endregion
    }
}
