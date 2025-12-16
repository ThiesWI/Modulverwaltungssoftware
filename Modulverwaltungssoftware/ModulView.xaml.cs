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

        // Platzhalter-Daten für Modulversionen
        private Dictionary<string, ModuleData> _moduleVersions = new Dictionary<string, ModuleData>();

        public ModulView()
        {
            InitializeComponent();
            this.DataContext = this;

            // Platzhalter-Versionen erstellen
            InitializePlaceholderData();

            // Beispielhafte Initialisierung, später durch echte Daten ersetzen
            var versions = new[] { "v1.0", "v1.1", "v2.0" };
            UpdateVersions(versions);

            // Initial erste Version laden
            if (Versions.Count > 0)
                LoadModuleVersion(Versions[0]);
        }

        private void InitializePlaceholderData()
        {
            _moduleVersions["v1.0"] = new ModuleData
            {
                Titel = "Softwareentwicklung Grundlagen",
                Modultypen = new List<string> { "Pflichtmodul", "Grundlagenmodul" },
                Studiengang = "Wirtschaftsinformatik",
                Semester = new List<string> { "1", "2" },
                Pruefungsformen = new List<string> { "Klausur", "Projektarbeit (Belegarbeit)" },
                Turnus = new List<string> { "Halbjährlich (Jedes Semester)" },
                Ects = 6,
                WorkloadPraesenz = 60,
                WorkloadSelbststudium = 120,
                Verantwortlicher = "Prof. Dr. Müller",
                Voraussetzungen = "Keine formalen Voraussetzungen, Grundkenntnisse in Programmierung von Vorteil.",
                Lernziele = "Die Studierenden können objektorientierte Programmierkonzepte anwenden und eigenständig kleine Softwareprojekte umsetzen.",
                Lehrinhalte = "Einführung in OOP, UML-Diagramme, Design Patterns, Versionskontrolle mit Git, Agile Methoden.",
                Literatur = "Gamma et al.: Design Patterns (1994), Martin: Clean Code (2008)"
            };

            _moduleVersions["v1.1"] = new ModuleData
            {
                Titel = "Softwareentwicklung Grundlagen (überarbeitet)",
                Modultypen = new List<string> { "Pflichtmodul" },
                Studiengang = "Wirtschaftsinformatik, Angewandte Informatik",
                Semester = new List<string> { "1" },
                Pruefungsformen = new List<string> { "Klausur" },
                Turnus = new List<string> { "Jährlich (WiSe)" },
                Ects = 5,
                WorkloadPraesenz = 45,
                WorkloadSelbststudium = 105,
                Verantwortlicher = "Prof. Dr. Müller, Dr. Schmidt",
                Voraussetzungen = "Keine",
                Lernziele = "Erweiterte OOP-Kenntnisse, TDD-Ansätze verstehen.",
                Lehrinhalte = "OOP-Vertiefung, Test-Driven Development, Refactoring.",
                Literatur = "Fowler: Refactoring (2018), Beck: TDD by Example (2002)"
            };

            _moduleVersions["v2.0"] = new ModuleData
            {
                Titel = "Advanced Software Engineering",
                Modultypen = new List<string> { "Spezialisierungsmodul", "Wahlmodul (Freies Wahlfach)" },
                Studiengang = "Master Wirtschaftsinformatik",
                Semester = new List<string> { "3", "4" },
                Pruefungsformen = new List<string> { "Hausarbeit (Seminararbeit)", "Präsentation" },
                Turnus = new List<string> { "Jährlich (SoSe)" },
                Ects = 8,
                WorkloadPraesenz = 48,
                WorkloadSelbststudium = 192,
                Verantwortlicher = "Prof. Dr. Lange",
                Voraussetzungen = "Abgeschlossenes Modul Softwareentwicklung Grundlagen, Kenntnisse in Java/C#.",
                Lernziele = "Architekturen großer Systeme entwerfen, Microservices entwickeln, CI/CD implementieren.",
                Lehrinhalte = "Architekturmuster, Microservices, Docker/Kubernetes, CI/CD-Pipelines, Cloud-Deployment.",
                Literatur = "Newman: Building Microservices (2021), Fowler: Patterns of Enterprise Application Architecture (2002)"
            };
        }

        private void LoadModuleVersion(string version)
        {
            if (!_moduleVersions.ContainsKey(version))
                return;

            var data = _moduleVersions[version];

            // Textfelder befüllen
            TitelTextBox.Text = data.Titel;
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

        // Datenklasse für Modulversion
        private class ModuleData
        {
            public string Titel { get; set; }
            public List<string> Modultypen { get; set; }
            public string Studiengang { get; set; }
            public List<string> Semester { get; set; }
            public List<string> Pruefungsformen { get; set; }
            public List<string> Turnus { get; set; }
            public int Ects { get; set; }
            public int WorkloadPraesenz { get; set; }
            public int WorkloadSelbststudium { get; set; }
            public string Verantwortlicher { get; set; }
            public string Voraussetzungen { get; set; }
            public string Lernziele { get; set; }
            public string Lehrinhalte { get; set; }
            public string Literatur { get; set; }
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

        // Öffnet/Schließt das Versionen-Popup unter dem Button
        private void ToggleVersionsPopup(object sender, RoutedEventArgs e)
        {
            var popup = this.FindName("VersionsPopup") as System.Windows.Controls.Primitives.Popup;
            if (popup != null)
            {
                popup.IsOpen = !popup.IsOpen;
            }
        }

        // Klick auf Popup-Item: Version auswählen
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
                // Version laden
                LoadModuleVersion(selectedVersion);
                MessageBox.Show($"Version {selectedVersion} geladen", "Version", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
