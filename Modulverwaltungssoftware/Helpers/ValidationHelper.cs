using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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

        // Farben für Validierungsstatus
        private static readonly Brush ErrorBorderBrush = new SolidColorBrush(Color.FromRgb(220, 53, 69));  // Bootstrap Red
        private static readonly Brush ErrorBackgroundBrush = new SolidColorBrush(Color.FromArgb(30, 220, 53, 69));  // Transparentes Rot
        private static readonly Brush ValidBorderBrush = new SolidColorBrush(Colors.LightGray);
        private static readonly Brush ValidBackgroundBrush = Brushes.White;

        /// <summary>
        /// Markiert ein Feld als fehlerhaft (roter Rahmen + Hintergrund)
        /// </summary>
        public static void MarkAsInvalid(Control control, string errorMessage = "")
        {
            if (control == null) return;

            control.BorderBrush = ErrorBorderBrush;
            control.BorderThickness = new Thickness(2);

            if (control is TextBox textBox)
            {
                textBox.Background = ErrorBackgroundBrush;

                // Tooltip mit Fehlermeldung
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    textBox.ToolTip = $"? {errorMessage}";
                }
            }
            else if (control is ListBox listBox)
            {
                listBox.Background = ErrorBackgroundBrush;

                if (!string.IsNullOrEmpty(errorMessage))
                {
                    listBox.ToolTip = $"? {errorMessage}";
                }
            }
        }

        /// <summary>
        /// Markiert ein Feld als gültig (Standarddarstellung)
        /// </summary>
        public static void MarkAsValid(Control control)
        {
            if (control == null) return;

            control.BorderBrush = ValidBorderBrush;
            control.BorderThickness = new Thickness(1);

            if (control is TextBox textBox)
            {
                textBox.Background = ValidBackgroundBrush;
                textBox.ToolTip = null;
            }
            else if (control is ListBox listBox)
            {
                listBox.Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)); // #F5F5F5
                listBox.ToolTip = null;
            }
        }

        /// <summary>
        /// Setzt alle Felder auf den Standard-Status zurück
        /// </summary>
        public static void ResetAll(params Control[] controls)
        {
            foreach (var control in controls)
            {
                MarkAsValid(control);
            }
        }
    }
}
