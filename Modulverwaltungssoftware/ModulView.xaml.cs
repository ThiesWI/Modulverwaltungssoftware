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
    public partial class ModulView : Page
    {
        public System.Collections.ObjectModel.ObservableCollection<string> Versions { get; } =
            new System.Collections.ObjectModel.ObservableCollection<string>();

        public ModulView()
        {
            InitializeComponent();
            this.DataContext = this;

            // Beispielhafte Initialisierung, später durch echte Daten ersetzen
            var versions = new[] { "v1.0", "v1.1", "v2.0" };
            UpdateVersions(versions);
        }

        private void UpdateVersions(System.Collections.Generic.IEnumerable<string> versions)
        {
            Versions.Clear();
            if (versions == null) return;
            foreach (var v in versions)
            {
                if (v != null) Versions.Add(v);
            }
        }

        private void ModulversionExportieren_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Ersetze "X" durch die tatsächliche Modulversionsbezeichnung,
            // z.B. aus dem DataContext oder einer im UI ausgewählten Item-Property.
            string version = "X";

            // Pfad zum Downloads-Ordner (häufig Benutzerprofil\Downloads)
            string downloadsPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

            MessageBox.Show($"Modulversion {version} wurde im Download-Ordner hinterlegt:\n{downloadsPath}",
                "Export abgeschlossen", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ModulversionBearbeiten_Click(object sender, RoutedEventArgs e)
        {
            // Ersetze diese Platzhalter-Logik mit der tatsächlichen ausgewählten Version
            string selectedVersion = Versions.FirstOrDefault();
            if (string.IsNullOrEmpty(selectedVersion))
            {
                MessageBox.Show("Keine Version ausgewählt.", "Bearbeiten", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            // Zur EditingView navigieren und Version übergeben
            this.NavigationService?.Navigate(new EditingView(selectedVersion));
        }

        private void ModulversionLöschen_Click(object sender, RoutedEventArgs e)
        {
            // Ersetze "XY" durch die tatsächlich ausgewählte Version
            string version = "XY";
            var result = MessageBox.Show(
                $"Soll die aktuelle Version {version} wirklich gelöscht werden?",
                "Löschen bestätigen",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                MessageBox.Show($"Version {version} wurde gelöscht.",
                    "Gelöscht", MessageBoxButton.OK, MessageBoxImage.Information);
                // TODO: Löschlogik hier ausführen
            }
            // Bei Nein passiert nichts
        }

        private void ModulversionKommentieren_Click(object sender, RoutedEventArgs e)
        {
            // Zur CommentView navigieren
            this.NavigationService?.Navigate(new CommentView());
        }

        private void ModulversionEinreichen_Click(object sender, RoutedEventArgs e)
        {
            // Bestätigung vor dem Einreichen
            var result = MessageBox.Show(
                "Soll die aktuelle Modulversion wirklich zur Koordination eingereicht werden?",
                "Einreichung bestätigen",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                // Bei Nein: nichts tun
                return;
            }

            // Einreichung durchführen
            string version = "ausgewählte Version"; // Optional: mit tatsächlicher Auswahl ersetzen
            MessageBox.Show($"Die {version} wurde erfolgreich eingereicht.",
                "Einreichung", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void VersionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Optional: Ausgewählte Version auswerten
            string selectedVersion = null;
            if (sender is MenuItem mi && mi.DataContext is string s)
            {
                selectedVersion = s;
            }
            // Hier könnte die View entsprechend der Version aktualisiert werden
            MessageBox.Show(selectedVersion != null ? $"Version {selectedVersion} gewählt" : "Version gewählt", "Version", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
