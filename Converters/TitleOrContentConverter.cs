using MyNotes.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotes.Converters
{
    public class TitleOrContentConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Note note)
            {
                if (note.Content == null || note.Content.Length == 0)
                {
                    return note.Title;
                }
                else if (note.Content.Length <= 60)
                {
                    return note.Content;
                }
                else
                {
                    return note.Content.Substring(0, 60);
                }
            }
            else
            {
                return "No content";
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
