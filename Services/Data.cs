using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotes.Services
{
    static class Data
    {
        public static String DefaultData { get; } = "/storage/emulated/0/Android/Data/com.CKStudios.mynotes/notes";
        public static String BulletedList { get; } = "• ";
    }
}
