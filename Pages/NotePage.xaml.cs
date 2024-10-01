using MyNotes.Models;
using MyNotes.ViewModels;

namespace MyNotes.Pages
{
    public partial class NotePage : ContentPage, IQueryAttributable
    {
        private readonly NoteViewModel _vm;

        public NotePage(NoteViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
            _vm = vm;
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
    }
}