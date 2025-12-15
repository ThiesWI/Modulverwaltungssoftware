using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Modulverwaltungssoftware
{
    /// <summary>
    /// Interaktionslogik für EditingView.xaml
    /// </summary>
    public partial class EditingView : Page
    {
        private ScrollViewer _contentScrollViewer;
        public string SelectedVersion { get; }

        public EditingView()
        {
            InitializeComponent();
            _contentScrollViewer = FindName("ContentScrollViewer") as ScrollViewer;
        }

        public EditingView(string selectedVersion) : this()
        {
            SelectedVersion = selectedVersion;
        }

        private void EntwurfSpeichern_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Entwurf wurde gespeichert (Button gedrückt).", "Bestätigung", MessageBoxButton.OK, MessageBoxImage.Information);

            // Nach dem Speichern zur ModulView wechseln
            this.NavigationService?.Navigate(new ModulView());
        }

        private void EntwurfVerwerfen_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
        "Möchten Sie den Entwurf wirklich verwerfen?",
        "Warnung",
        MessageBoxButton.YesNo,
        MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                // Zurück zur StartPage navigieren
                this.NavigationService?.Navigate(new StartPage());
            }
            // Bei Nein passiert nichts, der Entwurf bleibt bestehen
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            base.OnPreviewMouseWheel(e);
            // Ensure scroll always works regardless of mouse focus
            if (_contentScrollViewer != null)
            {
                _contentScrollViewer.ScrollToVerticalOffset(_contentScrollViewer.VerticalOffset - e.Delta);
                e.Handled = true;
            }
        }
    }
}
