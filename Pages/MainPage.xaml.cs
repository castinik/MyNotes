using Java.Util;
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
            PopupAddFolder.IsVisible = false;
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
            RefreshNotes();
        }

        private async void OnFolderTapped(object sender, TappedEventArgs e)
        {
            Folder folder = e.Parameter as Folder;
            if (_vm.IsOnTrashOpen)
            {
                Frame frame = sender as Frame;
                Grid parent = frame.Parent as Grid;

                foreach (var element in parent.GetVisualTreeDescendants())
                {
                    if (element is Image)
                    {
                        Image image = element as Image;
                        if (folder.IsChecked)
                        {
                            image.Source = "folder.png";
                            folder.IsChecked = false;
                        }
                        else
                        {
                            image.Source = "rfolder.png";
                            folder.IsChecked = true;
                        }
                        break;
                    }
                }
            }
            else
            {
                _vm.FolderTappedCommand.Execute(folder);
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

        // Aggiunge una nota; va a NoteView
        private async void OnAddNoteClicked(object sender, EventArgs e)
        {
            if (_vm.IsOnTrashOpen)
            {
                UncheckAllElements();
            }
            var noteViewModel = new NoteViewModel(_noteService); // Pass INoteService instance
            await Navigation.PushAsync(new NotePage(noteViewModel));
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
            if (_vm.IsPopUpInfoOpen)
            {
                _vm.IsPopUpInfoOpen = false;
                _vm.IsOnLongPressure = false;

            }
        }


        #region ############### Functional ###############
        private void UncheckAllElements()
        {
            //CollectionView collectionViewL = MainViewLeft;
            //CollectionView collectionViewR = MainViewRight;
            //CollectionView cainNoteView = MainNoteView;
            foreach (var element in MainViewLeft.GetVisualTreeDescendants())
            {
                if (element is Grid)
                {
                    Grid grid = element as Grid;
                    Frame frame = grid.Parent as Frame;
                    frame.BorderColor = Color.FromArgb("#005792");
                    frame.BackgroundColor = Color.Parse("#26005792");
                }
            }
            foreach (var element in MainViewRight.GetVisualTreeDescendants())
            {
                if (element is Grid)
                {
                    Grid grid = element as Grid;
                    Frame frame = grid.Parent as Frame;
                    frame.BorderColor = Color.FromArgb("#005792");
                    frame.BackgroundColor = Color.Parse("#26005792");
                }
            }
            foreach (var element in MainNoteView.GetVisualTreeDescendants())
            {
                if (element is Image)
                {
                    Image image = element as Image;
                    image.Source = "folder.png";
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
            foreach (Folder folder in _vm.Folders)
            {
                folder.IsChecked = false;
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
