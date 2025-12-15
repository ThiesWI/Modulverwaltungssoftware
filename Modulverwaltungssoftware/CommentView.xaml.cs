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
    /// Interaktionslogik für CommentView.xaml
    /// </summary>
    public partial class CommentView : Page
    {
        public CommentView()
        {
            InitializeComponent();
        }

        private void KommentarAbschicken_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Soll der Kommentar wirklich final an den Modulersteller weitergereicht werden?",
                "Bestätigung senden",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                MessageBox.Show("Der Kommentar wurde eingereicht.", "Bestätigung", MessageBoxButton.OK, MessageBoxImage.Information);
                // TODO: Kommentarinhalt in Kombination mit der Version speichern
            }
            // Bei Nein passiert nichts
        }

        private void KommentarVerwerfen_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Möchten Sie den Kommentar wirklich verwerfen?",
                "Warnung",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                // Zurück zur ModulView navigieren
                this.NavigationService?.Navigate(new ModulView());
            }
            // Bei Nein passiert nichts
        }
    }
}
