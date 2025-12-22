using System;
using System.Windows.Input;

namespace Modulverwaltungssoftware.Helpers
{
    /// <summary>
    /// Hilfsklasse für Input-Validierung in TextBoxen
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// Validiert, ob die Eingabe nur aus Ziffern besteht
        /// </summary>
        /// <param name="text">Der zu prüfende Text</param>
        /// <returns>True wenn nur Zahlen, sonst False</returns>
        public static bool IsNumericInput(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            foreach (char c in text)
            {
                if (!char.IsDigit(c))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Event-Handler für numerische TextBox-Eingabe
        /// Kann direkt in XAML verwendet werden: PreviewTextInput="NumericTextBox_PreviewTextInput"
        /// </summary>
        public static void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Blockiere nicht-numerische Eingaben
            e.Handled = !IsNumericInput(e.Text);
        }

        /// <summary>
        /// Validiert, ob ein String sicher in eine Zahl konvertiert werden kann
        /// </summary>
        public static bool TryParseInt(string text, out int result)
        {
            result = 0;
            if (string.IsNullOrWhiteSpace(text))
                return false;

            return int.TryParse(text.Trim(), out result);
        }
    }
}
