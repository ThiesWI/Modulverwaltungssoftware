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

        private string _currentVersion = null; // Aktuell geladene Version

        public ModulView()
        {
            InitializeComponent();
            this.DataContext = this;

            // Versionen aus Repository laden
            var versions = ModuleDataRepository.GetAllVersions();
            UpdateVersions(versions);

            // Initial erste Version laden
            if (Versions.Count > 0)
                LoadModuleVersion(Versions[0]);
        }

        // Konstruktor mit Versionsvorgabe (für Navigation aus EditingView)
        public ModulView(string versionToLoad) : this()
        {
            if (!string.IsNullOrEmpty(versionToLoad) && Versions.Contains(versionToLoad))
            {
                LoadModuleVersion(versionToLoad);
            }
        }

        private void LoadModuleVersion(string version)
        {
            // Daten aus Repository holen
            var data = ModuleDataRepository.GetVersion(version);
            if (data == null)
                return;

            _currentVersion = version; // Aktuell geladene Version merken

            // Textfelder befüllen
            TitelTextBox.Text = data.Titel;
            VersionTextBox.Text = version; // Version anzeigen
            StudiengangTextBox.Text = data.Studiengang;
            EctsTextBox.Text = data.Ects.ToString();
            WorkloadPraesenzTextBox.Text = data.WorkloadPraesenz.ToString();
            WorkloadSelbststudiumTextBox.Text = data.WorkloadSelbststudium.ToString();
            VerantwortlicherTextBox.Text = data.Verantwortlicher;
            VoraussetzungenTextBox.Text = data.Voraussetzungen;
            LernzieleTextBox.Text = data.Lernziele;
            LehrinhalteTextBox.Text = data.Lehrinhalte;
            LiteraturTextBox.Text = data.Literatur;

            // ListBoxen befüllen (vorhandene Items auswählen)
            SelectListBoxItems(ModultypListBox, data.Modultypen);
            SelectListBoxItems(SemesterListBox, data.Semester);
            SelectListBoxItems(PruefungsformListBox, data.Pruefungsformen);
            SelectListBoxItems(TurnusListBox, data.Turnus);
        }

        private void SelectListBoxItems(ListBox listBox, List<string> itemsToSelect)
        {
            listBox.SelectedItems.Clear();
            foreach (var item in listBox.Items)
            {
                if (item is ListBoxItem lbi && itemsToSelect.Contains(lbi.Content.ToString()))
                {
                    listBox.SelectedItems.Add(lbi);
                }
            }
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
            string version = _currentVersion ?? "unbekannt";
            string downloadsPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

            MessageBox.Show($"Modulversion {version} wurde im Download-Ordner hinterlegt:\n{downloadsPath}",
                "Export abgeschlossen", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ModulversionBearbeiten_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentVersion))
            {
                MessageBox.Show("Bitte wählen Sie zuerst eine Version aus.", "Keine Version", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var sourceData = ModuleDataRepository.GetVersion(_currentVersion);
            if (sourceData == null)
            {
                MessageBox.Show("Fehler beim Laden der Version.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            this.NavigationService?.Navigate(new EditingView(sourceData, _currentVersion));
        }

        private void ModulversionLöschen_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentVersion))
            {
                MessageBox.Show("Bitte wählen Sie zuerst eine Version aus.", "Keine Version", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Soll die Version {_currentVersion} wirklich gelöscht werden?",
                "Löschen bestätigen",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                MessageBox.Show($"Version {_currentVersion} wurde gelöscht.",
                    "Gelöscht", MessageBoxButton.OK, MessageBoxImage.Information);
                // TODO: Löschlogik hier ausführen
            }
        }

        private void ModulversionKommentieren_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentVersion))
            {
                MessageBox.Show("Bitte wählen Sie zuerst eine Version aus.", "Keine Version", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var sourceData = ModuleDataRepository.GetVersion(_currentVersion);
            if (sourceData == null)
            {
                MessageBox.Show("Fehler beim Laden der Version.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var commentData = new CommentView.ModuleData
            {
                Titel = sourceData.Titel,
                Modultypen = sourceData.Modultypen,
                Studiengang = sourceData.Studiengang,
                Semester = sourceData.Semester,
                Pruefungsformen = sourceData.Pruefungsformen,
                Turnus = sourceData.Turnus,
                Ects = sourceData.Ects,
                WorkloadPraesenz = sourceData.WorkloadPraesenz,
                WorkloadSelbststudium = sourceData.WorkloadSelbststudium,
                Verantwortlicher = sourceData.Verantwortlicher,
                Voraussetzungen = sourceData.Voraussetzungen,
                Lernziele = sourceData.Lernziele,
                Lehrinhalte = sourceData.Lehrinhalte,
                Literatur = sourceData.Literatur
            };

            this.NavigationService?.Navigate(new CommentView(commentData, _currentVersion));
        }

        private void ModulversionEinreichen_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentVersion))
            {
                MessageBox.Show("Bitte wählen Sie zuerst eine Version aus.", "Keine Version", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Soll die Version {_currentVersion} wirklich zur Koordination eingereicht werden?",
                "Einreichung bestätigen",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            MessageBox.Show($"Die Version {_currentVersion} wurde erfolgreich eingereicht.",
                "Einreichung", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void VersionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string selectedVersion = null;
            if (sender is MenuItem mi && mi.DataContext is string s)
            {
                selectedVersion = s;
            }
            MessageBox.Show(selectedVersion != null ? $"Version {selectedVersion} gewählt" : "Version gewählt", "Version", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ToggleVersionsPopup(object sender, RoutedEventArgs e)
        {
            var popup = this.FindName("VersionsPopup") as System.Windows.Controls.Primitives.Popup;
            if (popup != null)
            {
                popup.IsOpen = !popup.IsOpen;
            }
        }

        private void VersionPopupItem_Click(object sender, RoutedEventArgs e)
        {
            string selectedVersion = null;
            if (sender is Button btn && btn.DataContext is string s)
            {
                selectedVersion = s;
            }

            var popup = this.FindName("VersionsPopup") as System.Windows.Controls.Primitives.Popup;
            if (popup != null)
            {
                popup.IsOpen = false;
            }

            if (!string.IsNullOrEmpty(selectedVersion))
            {
                LoadModuleVersion(selectedVersion);
                MessageBox.Show($"Version {selectedVersion} geladen", "Version", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
