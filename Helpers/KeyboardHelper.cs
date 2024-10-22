using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotes.Helpers
{
    public static class KeyboardHelper
    {
        // Evento che verrà sollevato quando la tastiera appare o scompare
        public static event EventHandler<double> KeyboardAppearedEvent;

        public static void TriggerKeyboardAppeared(double keyboardHeight)
        {
            // Controlla se ci sono sottoscrittori prima di sollevare l'evento
            KeyboardAppearedEvent?.Invoke(null, keyboardHeight);
        }

        public static void TriggerKeyboardDisappeared()
        {
            // Solleva un evento con altezza 0 (indicando che la tastiera è scomparsa)
            KeyboardAppearedEvent?.Invoke(null, 0);
        }
    }
}
