using MyNotes.Helpers;
using MyNotes.Models;
using MyNotes.Services;
using MyNotes.ViewModels;
using Windows.Devices.Enumeration;

namespace MyNotes.Pages
{
    public partial class NotePage : ContentPage, IQueryAttributable
    {
        private readonly NoteViewModel _vm;
        private Boolean _isUpdatingText = false;
        private Double _previousHeight;

        public NotePage(NoteViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
            _vm = vm;
            KeyboardHelper.KeyboardAppearedEvent += OnKeyboardAppeared;
            
        }

        protected override bool OnBackButtonPressed()
        {
            _vm.ComeBackCommand.Execute(null);
            return true;
        }

        private void OnKeyboardAppeared(object sender, double keyboardHeight)
        {
            if (keyboardHeight > 0)
            {
                // Sposta l'Editor in base all'altezza della tastiera
                MainEditor.Margin = new Thickness(0, 0, 0, keyboardHeight);
            }
            else
            {
                // Ripristina il margine quando la tastiera scompare
                MainEditor.Margin = new Thickness(0);
            }
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            // Sottoscrizione rimossa per prevenire memory leaks
            KeyboardHelper.KeyboardAppearedEvent -= OnKeyboardAppeared;
        }

        public Note Note
        {
            get => (BindingContext as NoteViewModel)?.Note;
            set
            {
                var viewModel = BindingContext as NoteViewModel;
                if (viewModel != null)
                {
                    viewModel.Note = new Note(value);
                }
            }
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("Note", out var noteObj) && noteObj is Note note)
            {
                _vm.Note = note;
            }
            else
            {
                _vm.Note = new Note();
            }
        }


        //METODI DI EDITING TESTUALE

        private void EditorTextChanged(object sender, TextChangedEventArgs e)
        {
            //String mainEditor = MainEditor.Text;
            //String lastChars = String.Empty;
            //if (!String.IsNullOrEmpty(mainEditor) && mainEditor.Length >= 2)
            //{
            //    Int32 cursorPosition = MainEditor.CursorPosition;
            //    lastChars += mainEditor.ToCharArray()[cursorPosition - 2];
            //    lastChars += mainEditor[cursorPosition - 1];
            //}  && (lastChars != Data.BulletedList)
            if (e.NewTextValue.EndsWith("\n"))
            {
                // Scorri verso il basso dopo aver aggiunto una nuova riga
                Dispatcher.Dispatch(async () =>
                {
                    //await EditorScroll.ScrollToAsync(MainEditor, ScrollToPosition.End, true);
                    EditorScroll.ForceLayout();
                });
                //if (MainEditor.MinimumHeightRequest >= 450)
                //{
                //    EditorScroll.ScrollToAsync(0, Y - 30, true);
                //}
                //else
                //{
                //    MainEditor.MinimumHeightRequest += 30;
                //}
            }

            if (_vm.IsInList && e.NewTextValue.EndsWith("\n"))
            {
                String newContent = $"{MainEditor.Text}{Data.BulletedList}";
                //MainEditor.Text = newContent;
                MainEditor.UpdateText(newContent);
                MainEditor.CursorPosition = newContent.Length;
            }
            if (e.NewTextValue.EndsWith(Data.BulletedList))
            {
                MainEditor.CursorPosition = MainEditor.Text.Length;
            }
            else
            {

            }
            return;
        }

        private void ChangeEditorText(String text)
        {
            MainEditor.UpdateText(text);
            return;
        }

        private void ElencoButtonClicked(object sender, EventArgs e)
        {
            // Tolgo l'elenco puntato
            if (_vm.IsInList)
            {
                _vm.IsInList = false;
                BulletedList.BorderColor = (Color)Application.Current.Resources["First"];
            }
            else
            {
                _vm.IsInList = true;
                if (!String.IsNullOrEmpty(MainEditor.Text))
                {
                    String[] lines = MainEditor.Text.Split("\n");
                    String lastLine = lines[lines.Length - 1];
                    if (String.IsNullOrEmpty(lastLine))
                    {
                        MainEditor.Text = $"{MainEditor.Text}{Data.BulletedList}";
                    }
                    else
                    {
                        MainEditor.Text = $"{MainEditor.Text}\n{Data.BulletedList}";
                    }
                }
                else
                {
                    MainEditor.Text = $"{Data.BulletedList}";
                }
                BulletedList.BorderColor = (Color)Application.Current.Resources["Text1"];
                MainEditor.CursorPosition = MainEditor.Text.Length;
            }
        }

        private void TodoButtonClicked(object sender, EventArgs e)
        {
            // Tolgo la lista
            if (_vm.IsInTodo)
            {
                _vm.IsInTodo = false;
                CheckList.BorderColor = (Color)Application.Current.Resources["First"];
            }
            else
            {
                _vm.IsInTodo = true;
                if (!String.IsNullOrEmpty(MainEditor.Text))
                {
                    String[] lines = MainEditor.Text.Split("\n");
                    String lastLine = lines[lines.Length - 1];
                    if (String.IsNullOrEmpty(lastLine))
                    {
                        MainEditor.Text = $"{MainEditor.Text}{Data.BulletedList}";
                    }
                    else
                    {
                        MainEditor.Text = $"{MainEditor.Text}\n{Data.BulletedList}";
                    }
                }
                else
                {
                    MainEditor.Text = $"{Data.BulletedList}";
                }
                CheckList.BorderColor = (Color)Application.Current.Resources["Text1"];
                MainEditor.CursorPosition = MainEditor.Text.Length;
            }
        }
    }
}