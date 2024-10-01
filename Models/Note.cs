using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotes.Models
{
    public class Note : INotifyPropertyChanged
    {
        public Note(Note note)
        {
            Id = note.Id;
            Created = note.Created;
            Modify = DateTime.Now;
            Title = note.Title;
            Content = note.Content;
        }
        public Note()
        {
            Id = Guid.NewGuid();
            Created = DateTime.Now;
            Modify = Created;
            Title = $"{Created.ToString("dd.MM.yy-HH.mm.ss")}";
        }
        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modify { get; set; }
        public String Title { get; set; } 
        public String Content { get; set; }
        public Boolean IsChecked { get; set; } = false;

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
