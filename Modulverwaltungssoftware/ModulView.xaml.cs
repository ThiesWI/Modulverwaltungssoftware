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

        private string _currentModulId;  // Aktuelles Modul
        private string _currentVersion;  // Aktuelle Version

        public ModulView()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        // Konstruktor mit ModulID (von StartPage/MainWindow)
        public ModulView(string modulId) : this()
        {
            _currentModulId = modulId;

            var modul = ModuleDataRepository.GetModule(modulId);
            if (modul == null) return;

            // Versionen-Dropdown füllen (mit "K" für kommentierte Versionen)
            var versionDisplay = modul.Versionen.Select(v => 
                v.HasComments ? $"{v.VersionsNummer}K" : v.VersionsNummer);
            UpdateVersions(versionDisplay);

            // Neueste Version laden
            var neuesteVersion = modul.Versionen
                .OrderByDescending(v => v.ErstellDatum)
                .FirstOrDefault();

            if (neuesteVersion != null)
                LoadModuleVersion(neuesteVersion.VersionsNummer);
        }

        private void LoadModuleVersion(string versionNummer)
        {
            // Daten aus Repository holen
            var data = ModuleDataRepository.GetModuleVersion(_currentModulId, versionNummer);
            if (data == null)
                return;

            _currentVersion = versionNummer; // Aktuell geladene Version merken

            // Prüfen, ob Version kommentiert wurde
            var modul = ModuleDataRepository.GetModule(_currentModulId);
            var version = modul?.Versionen.FirstOrDefault(v => v.VersionsNummer == versionNummer);
            bool hasComments = version?.HasComments ?? false;
            string versionDisplay = hasComments ? $"{versionNummer}K" : versionNummer;

            // Textfelder befüllen
            TitelTextBox.Text = data.Titel;
            VersionTextBox.Text = versionDisplay; // Version mit "K"-Suffix anzeigen
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
            if (string.IsNullOrEmpty(_currentVersion) || string.IsNullOrEmpty(_currentModulId))
            {
                MessageBox.Show("Bitte wählen Sie zuerst eine Version aus.", "Keine Version", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var modul = ModuleDataRepository.GetModule(_currentModulId);
            var version = modul?.Versionen.FirstOrDefault(v => v.VersionsNummer == _currentVersion);
            
            if (version == null)
            {
                MessageBox.Show("Fehler beim Laden der Version.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var sourceData = ModuleDataRepository.GetModuleVersion(_currentModulId, _currentVersion);
            
            if (version.HasComments)
            {
                // Version hat Kommentare ? EditWithCommentsView (Bearbeiten MIT Kommentar-Kontext)
                var comments = ModuleDataRepository.GetComments(_currentModulId, _currentVersion);
                this.NavigationService?.Navigate(
                    new EditWithCommentsView(_currentModulId, _currentVersion, sourceData, comments)
                );
            }
            else
            {
                // Version ohne Kommentare ? Normale EditingView
                this.NavigationService?.Navigate(
                    new EditingView(_currentModulId, _currentVersion, sourceData)
                );
            }
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
            if (string.IsNullOrEmpty(_currentVersion) || string.IsNullOrEmpty(_currentModulId))
            {
                MessageBox.Show("Bitte wählen Sie zuerst eine Version aus.", "Keine Version", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var sourceData = ModuleDataRepository.GetModuleVersion(_currentModulId, _currentVersion);
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

            this.NavigationService?.Navigate(new CommentView(commentData, _currentModulId, _currentVersion));
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
                // "K"-Suffix entfernen, falls vorhanden
                string actualVersion = selectedVersion.EndsWith("K") 
                    ? selectedVersion.Substring(0, selectedVersion.Length - 1) 
                    : selectedVersion;
                    
                LoadModuleVersion(actualVersion);
                MessageBox.Show($"Version {selectedVersion} geladen", "Version", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
