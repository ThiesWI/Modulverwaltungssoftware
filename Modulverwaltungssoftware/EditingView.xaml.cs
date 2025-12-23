using System;
using System.Collections.Generic;
using System.Data.Entity;
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
        private string _modulId;           // Aktuelles Modul (null bei neuem)
        private string _versionNummer;     // Aktuelle Version
        private bool _isEditMode;          // true = Bearbeiten, false = Neues Modul
        private bool _isCommentedVersion;  // true = Version wurde kommentiert

        // Datenklasse für Modulversion (identisch mit ModulView.ModuleData)
        public class ModuleData
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

        public EditingView()
        {
            InitializeComponent();
            _contentScrollViewer = FindName("ContentScrollViewer") as ScrollViewer;
            _isEditMode = false; // Standard: Neues Modul
        }

        // Konstruktor für NEUES Modul (aus StartPage)
        public EditingView(bool createNew) : this()
        {
            _isEditMode = false;
            _modulId = null;
            _versionNummer = "1.0";
            VersionTextBox.Text = "1.0 (Entwurf)";
        }

        // Konstruktor für BEARBEITEN (aus ModulView)
        public EditingView(string modulId, string versionNummer, ModuleDataRepository.ModuleData moduleData) : this()
        {
            _isEditMode = true;
            _modulId = modulId;
            _versionNummer = versionNummer;
            
            // Prüfen, ob die AKTUELLE Version kommentiert ist
            using (var db = new Services.DatabaseContext())
            {
                // WICHTIG: Parsing VOR der LINQ-Query durchführen!
                int modulIdInt = int.Parse(modulId);
                
                // Versionsnummer korrekt parsen (z.B. "2.1K" → "2.1" → 21)
                string cleanVersion = versionNummer.TrimEnd('K');
                int versionsnummerInt = ParseVersionsnummer(cleanVersion);
                
                var dbVersion = db.ModulVersion
                    .FirstOrDefault(v => v.ModulId == modulIdInt && v.Versionsnummer == versionsnummerInt);
                    
                _isCommentedVersion = dbVersion != null && dbVersion.hatKommentar;
            }
            
            LoadModuleData(moduleData);
        }

        // Hilfsmethode: Konvertiere Versionsnummer-String zu Integer ("2.1" → 21)
        private int ParseVersionsnummer(string version)
        {
            if (decimal.TryParse(version, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal dec))
                return (int)(dec * 10);
            return 10; // Fallback: Version 1.0
        }

        private void LoadModuleData(ModuleDataRepository.ModuleData data)
        {
            if (data == null) return;

            // Textfelder befüllen
            TitelTextBox.Text = data.Titel;
            VersionTextBox.Text = $"{_versionNummer} (Entwurf)"; // Version anzeigen (read-only)
            StudiengangTextBox.Text = data.Studiengang;
            EctsTextBox.Text = data.Ects.ToString();
            WorkloadPraesenzTextBox.Text = data.WorkloadPraesenz.ToString();
            WorkloadSelbststudiumTextBox.Text = data.WorkloadSelbststudium.ToString();
            VerantwortlicherTextBox.Text = data.Verantwortlicher;
            VoraussetzungenTextBox.Text = data.Voraussetzungen;
            LernzieleTextBox.Text = data.Lernziele;
            LehrinhalteTextBox.Text = data.Lehrinhalte;
            LiteraturTextBox.Text = data.Literatur;

            // ListBoxen befüllen
            SelectListBoxItems(ModultypListBox, data.Modultypen);
            SelectListBoxItems(SemesterListBox, data.Semester);
            SelectListBoxItems(PruefungsformListBox, data.Pruefungsformen);
            SelectListBoxItems(TurnusListBox, data.Turnus);
        }

        private void SelectListBoxItems(ListBox listBox, List<string> itemsToSelect)
        {
            if (itemsToSelect == null) return;
            listBox.SelectedItems.Clear();
            foreach (var item in listBox.Items)
            {
                if (item is ListBoxItem lbi)
                {
                    string itemText = lbi.Content.ToString();
                    // Prüfe ob einer der zu selektierenden Strings mit dem Item übereinstimmt
                    if (itemsToSelect.Any(s => itemText.Contains(s) || s.Contains(itemText)))
                    {
                        listBox.SelectedItems.Add(lbi);
                    }
                }
            }
        }

        private void EntwurfSpeichern_Click(object sender, RoutedEventArgs e)
        {
            // Validierung vor dem Speichern
            if (string.IsNullOrWhiteSpace(TitelTextBox.Text))
            {
                MessageBox.Show("Bitte Titel eingeben.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(EctsTextBox.Text) || !int.TryParse(EctsTextBox.Text, out int ects))
            {
                MessageBox.Show("Bitte gültige ECTS-Punktzahl eingeben.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(WorkloadPraesenzTextBox.Text) || !int.TryParse(WorkloadPraesenzTextBox.Text, out int workloadPraesenz))
            {
                MessageBox.Show("Bitte gültige Workload Präsenz eingeben.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(WorkloadSelbststudiumTextBox.Text) || !int.TryParse(WorkloadSelbststudiumTextBox.Text, out int workloadSelbststudium))
            {
                MessageBox.Show("Bitte gültige Workload Selbststudium eingeben.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_isEditMode)
            {
                try
                {
                    int modulIdInt = int.Parse(_modulId);
                    
                    if (_isCommentedVersion)
                    {
                        // Bei kommentierten Versionen: Neue Version erstellen
                        ErstelleNeueVersionMitAenderungen(modulIdInt, ects, workloadPraesenz, workloadSelbststudium);
                    }
                    else
                    {
                        // Bei nicht-kommentierten Versionen: Bestehende Version aktualisieren
                        AktualisiereBestehendeVersion(modulIdInt, ects, workloadPraesenz, workloadSelbststudium);
                    }

                    MessageBox.Show($"Änderungen wurden erfolgreich gespeichert.", 
                        "Gespeichert", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Zurück zur ModulView mit diesem Modul
                    this.NavigationService?.Navigate(new ModulView(modulIdInt));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fehler beim Speichern: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                // NEUES MODUL: In echte Datenbank schreiben
                try
                {
                    int neueModulId = ErstelleNeuesModul(ects, workloadPraesenz, workloadSelbststudium);
                    
                    MessageBox.Show($"Neues Modul '{TitelTextBox.Text}' wurde erstellt.", 
                        "Gespeichert", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Zur ModulView mit dem neuen Modul navigieren
                    this.NavigationService?.Navigate(new ModulView(neueModulId));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fehler beim Erstellen des Moduls: {ex.Message}", 
                        "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private List<string> GetSelectedListBoxItems(ListBox listBox)
        {
            var selected = new List<string>();
            foreach (var item in listBox.SelectedItems)
            {
                if (item is ListBoxItem lbi)
                {
                    selected.Add(lbi.Content.ToString());
                }
            }
            return selected;
        }

        private void ErstelleNeueVersionMitAenderungen(int modulId, int ects, int workloadPraesenz, int workloadSelbststudium)
        {
            using (var db = new Services.DatabaseContext())
            {
                // SPEZIFISCHE kommentierte Version laden (nicht die erste!)
                string cleanVersion = _versionNummer.TrimEnd('K');
                int aktuelleVersionsnummer = ParseVersionsnummer(cleanVersion);
                
                var alteVersion = db.ModulVersion
                    .Include("Modul")
                    .FirstOrDefault(v => v.ModulId == modulId && v.Versionsnummer == aktuelleVersionsnummer);

                if (alteVersion == null)
                    throw new InvalidOperationException("Modulversion nicht gefunden.");

                // ✅ Problem 4 Fix: Höchste Versionsnummer für dieses Modul finden
                var hoechsteVersionsnummer = db.ModulVersion
                    .Where(v => v.ModulId == modulId)
                    .Max(v => (int?)v.Versionsnummer) ?? 10;
                
                int neueVersionsnummer = hoechsteVersionsnummer + 1;
                
                // ✅ Problem 3 Fix: Prüfen ob Version bereits existiert
                if (db.ModulVersion.Any(v => v.ModulId == modulId && v.Versionsnummer == neueVersionsnummer))
                    throw new InvalidOperationException($"Version {neueVersionsnummer / 10.0:0.0} existiert bereits!");

                // Neue Version erstellen (NUR versionsspezifische Daten!)
                var neueVersion = new ModulVersion
                {
                    ModulId = alteVersion.ModulId,
                    Versionsnummer = neueVersionsnummer,
                    GueltigAbSemester = "Entwurf",
                    ModulStatus = ModulVersion.Status.Entwurf,
                    LetzteAenderung = DateTime.Now,
                    WorkloadPraesenz = workloadPraesenz,
                    WorkloadSelbststudium = workloadSelbststudium,
                    EctsPunkte = ects,
                    Ersteller = Benutzer.CurrentUser?.Name ?? "Unbekannt",  // ← Problem 4: Aktueller User!
                    hatKommentar = false
                };

                // Prüfungsform (versionsspezifisch)
                var pruefungsformen = GetSelectedListBoxItems(PruefungsformListBox);
                if (pruefungsformen.Count > 0)
                    neueVersion.Pruefungsform = pruefungsformen[0];
                else
                    neueVersion.Pruefungsform = "Klausur";

                // Lernziele (versionsspezifisch)
                if (!string.IsNullOrWhiteSpace(LernzieleTextBox.Text))
                {
                    neueVersion.Lernergebnisse = LernzieleTextBox.Text
                        .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                }
                else
                {
                    neueVersion.Lernergebnisse = new List<string>();
                }

                // Lehrinhalte (versionsspezifisch)
                if (!string.IsNullOrWhiteSpace(LehrinhalteTextBox.Text))
                {
                    neueVersion.Inhaltsgliederung = LehrinhalteTextBox.Text
                        .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                }
                else
                {
                    neueVersion.Inhaltsgliederung = new List<string>();
                }

                // Literatur (versionsspezifisch)
                if (!string.IsNullOrWhiteSpace(LiteraturTextBox.Text))
                {
                    neueVersion.Literatur = LiteraturTextBox.Text
                        .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                }
                else
                {
                    neueVersion.Literatur = new List<string>();
                }

                // WICHTIG: Modul-Daten werden NICHT geändert!
                // Titel, Studiengang, Modultyp, Turnus, Semester, Voraussetzungen
                // sind GLOBAL für alle Versionen und können nicht versioniert werden.
                // Diese Felder sollten in der UI als "read-only" markiert werden oder
                // Änderungen sollten mit einer Warnung versehen werden.

                db.ModulVersion.Add(neueVersion);
                db.SaveChanges();
            }
        }

        private void AktualisiereBestehendeVersion(int modulId, int ects, int workloadPraesenz, int workloadSelbststudium)
        {
            using (var db = new Services.DatabaseContext())
            {
                // Lade die AKTUELLE Version (nicht neueste, sondern die mit der passenden Versionsnummer)
                string cleanVersion = _versionNummer.TrimEnd('K');
                int versionsnummerInt = ParseVersionsnummer(cleanVersion);
                
                var dbVersion = db.ModulVersion
                    .Include("Modul")
                    .FirstOrDefault(v => v.ModulId == modulId && v.Versionsnummer == versionsnummerInt);

                if (dbVersion == null)
                    throw new InvalidOperationException("Modulversion nicht gefunden.");

                // Modul-Daten aktualisieren
                dbVersion.Modul.ModulnameDE = TitelTextBox.Text;
                dbVersion.Modul.Studiengang = StudiengangTextBox.Text;

                // Modultyp - NIMM NUR DIE ERSTE AUSWAHL!
                var modultypen = GetSelectedListBoxItems(ModultypListBox);
                if (modultypen.Count > 0)
                {
                    string ersteAuswahl = modultypen[0];
                    if (ersteAuswahl.Contains("Wahlpflicht"))
                        dbVersion.Modul.Modultyp = Modul.ModultypEnum.Wahlpflicht;
                    else if (ersteAuswahl.Contains("Grundlagen") || ersteAuswahl.Contains("Pflichtmodul"))
                        dbVersion.Modul.Modultyp = Modul.ModultypEnum.Grundlagen;
                    
                    System.Diagnostics.Debug.WriteLine($"Speichere Modultyp: '{ersteAuswahl}' -> {dbVersion.Modul.Modultyp}");
                }

                // Turnus - NIMM NUR DIE ERSTE AUSWAHL!
                var turnusList = GetSelectedListBoxItems(TurnusListBox);
                if (turnusList.Count > 0)
                {
                    string ersteAuswahl = turnusList[0];
                    if (ersteAuswahl.Contains("WiSe") || ersteAuswahl.Contains("Wintersemester"))
                        dbVersion.Modul.Turnus = Modul.TurnusEnum.NurWintersemester;
                    else if (ersteAuswahl.Contains("SoSe") || ersteAuswahl.Contains("Sommersemester"))
                        dbVersion.Modul.Turnus = Modul.TurnusEnum.NurSommersemester;
                    else if (ersteAuswahl.Contains("Jedes Semester") || ersteAuswahl.Contains("Halbjährlich"))
                        dbVersion.Modul.Turnus = Modul.TurnusEnum.JedesSemester;
                    
                    System.Diagnostics.Debug.WriteLine($"Speichere Turnus: '{ersteAuswahl}' -> {dbVersion.Modul.Turnus}");
                }

                // Prüfungsform - NIMM NUR DIE ERSTE AUSWAHL!
                var pruefungsformen = GetSelectedListBoxItems(PruefungsformListBox);
                if (pruefungsformen.Count > 0)
                {
                    dbVersion.Pruefungsform = pruefungsformen[0];
                    System.Diagnostics.Debug.WriteLine($"Speichere Prüfungsform: '{pruefungsformen[0]}'");
                }

                // Semester - NIMM NUR DIE ERSTE AUSWAHL!
                var semester = GetSelectedListBoxItems(SemesterListBox);
                if (semester.Count > 0 && int.TryParse(semester[0], out int sem))
                {
                    dbVersion.Modul.EmpfohlenesSemester = sem;
                    System.Diagnostics.Debug.WriteLine($"Speichere Semester: {sem}");
                }

                // Voraussetzungen
                if (!string.IsNullOrWhiteSpace(VoraussetzungenTextBox.Text))
                {
                    dbVersion.Modul.Voraussetzungen = VoraussetzungenTextBox.Text
                        .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                }
                else
                {
                    dbVersion.Modul.Voraussetzungen = new List<string>();  // Leere Liste!
                }

                // ModulVersion-Daten
                dbVersion.EctsPunkte = ects;
                dbVersion.WorkloadPraesenz = workloadPraesenz;
                dbVersion.WorkloadSelbststudium = workloadSelbststudium;
                dbVersion.Ersteller = VerantwortlicherTextBox.Text;

                // Lernziele (versionsspezifisch)
                if (!string.IsNullOrWhiteSpace(LernzieleTextBox.Text))
                {
                    dbVersion.Lernergebnisse = LernzieleTextBox.Text
                        .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                }
                else
                {
                    dbVersion.Lernergebnisse = new List<string>();
                }

                // Lehrinhalte (versionsspezifisch)
                if (!string.IsNullOrWhiteSpace(LehrinhalteTextBox.Text))
                {
                    dbVersion.Inhaltsgliederung = LehrinhalteTextBox.Text
                        .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                }
                else
                {
                    dbVersion.Inhaltsgliederung = new List<string>();
                }

                // Literatur (versionsspezifisch)
                if (!string.IsNullOrWhiteSpace(LiteraturTextBox.Text))
                {
                    dbVersion.Literatur = LiteraturTextBox.Text
                        .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                }
                else
                {
                    dbVersion.Literatur = new List<string>();
                }

                dbVersion.LetzteAenderung = DateTime.Now;
                
                System.Diagnostics.Debug.WriteLine("Speichere Änderungen in Datenbank...");
                db.SaveChanges();
                System.Diagnostics.Debug.WriteLine("Erfolgreich gespeichert!");
            }
        }

        private int ErstelleNeuesModul(int ects, int workloadPraesenz, int workloadSelbststudium)
        {
            using (var db = new Services.DatabaseContext())
            {
                // Neues Modul erstellen
                var neuesModul = new Modul
                {
                    ModulnameDE = TitelTextBox.Text,
                    Studiengang = StudiengangTextBox.Text,
                    GueltigAb = DateTime.Now,
                    DauerInSemestern = 1
                };

                // Modultyp
                var modultypen = GetSelectedListBoxItems(ModultypListBox);
                if (modultypen.Count > 0)
                {
                    if (modultypen[0].Contains("Wahlpflicht"))
                        neuesModul.Modultyp = Modul.ModultypEnum.Wahlpflicht;
                    else if (modultypen[0].Contains("Grundlagen") || modultypen[0].Contains("Pflichtmodul"))
                        neuesModul.Modultyp = Modul.ModultypEnum.Grundlagen;
                }
                else
                {
                    neuesModul.Modultyp = Modul.ModultypEnum.Wahlpflicht;  // Standardwert
                }

                // Turnus
                var turnusList = GetSelectedListBoxItems(TurnusListBox);
                if (turnusList.Count > 0)
                {
                    if (turnusList[0].Contains("WiSe") || turnusList[0].Contains("Wintersemester"))
                        neuesModul.Turnus = Modul.TurnusEnum.NurWintersemester;
                    else if (turnusList[0].Contains("SoSe") || turnusList[0].Contains("Sommersemester"))
                        neuesModul.Turnus = Modul.TurnusEnum.NurSommersemester;
                    else if (turnusList[0].Contains("Jedes Semester") || turnusList[0].Contains("Halbjährlich"))
                        neuesModul.Turnus = Modul.TurnusEnum.JedesSemester;
                }
                else
                {
                    neuesModul.Turnus = Modul.TurnusEnum.JedesSemester;  // Standardwert
                }

                // Prüfungsform (wird im Modul als Enum gespeichert, aber wir nehmen erstmal einen Standardwert)
                neuesModul.PruefungsForm = Modul.PruefungsFormEnum.PL;

                // Semester
                var semester = GetSelectedListBoxItems(SemesterListBox);
                if (semester.Count > 0 && int.TryParse(semester[0], out int sem))
                    neuesModul.EmpfohlenesSemester = sem;
                else
                    neuesModul.EmpfohlenesSemester = 1;

                // Voraussetzungen
                if (!string.IsNullOrWhiteSpace(VoraussetzungenTextBox.Text))
                {
                    neuesModul.Voraussetzungen = VoraussetzungenTextBox.Text
                        .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                }
                else
                {
                    neuesModul.Voraussetzungen = new List<string>();
                }

                // Modul zur Datenbank hinzufügen
                db.Modul.Add(neuesModul);
                db.SaveChanges(); // Speichern, um ModulID zu erhalten

                // Erste Version erstellen (Version 1.0 = Versionsnummer 10)
                var ersteVersion = new ModulVersion
                {
                    ModulId = neuesModul.ModulID,
                    Versionsnummer = 10, // 1.0 (10 / 10.0 = 1.0)
                    GueltigAbSemester = "Entwurf",
                    ModulStatus = ModulVersion.Status.Entwurf,
                    LetzteAenderung = DateTime.Now,
                    WorkloadPraesenz = workloadPraesenz,
                    WorkloadSelbststudium = workloadSelbststudium,
                    EctsPunkte = ects,
                    Ersteller = Benutzer.CurrentUser?.Name ?? "Unbekannt",  // ← Problem 4: Aktueller User!
                    hatKommentar = false
                };

                // Prüfungsform
                var pruefungsformen = GetSelectedListBoxItems(PruefungsformListBox);
                if (pruefungsformen.Count > 0)
                    ersteVersion.Pruefungsform = pruefungsformen[0];
                else
                    ersteVersion.Pruefungsform = "Klausur";

                // Lernziele
                if (!string.IsNullOrWhiteSpace(LernzieleTextBox.Text))
                {
                    ersteVersion.Lernergebnisse = LernzieleTextBox.Text
                        .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                }
                else
                {
                    ersteVersion.Lernergebnisse = new List<string>();
                }

                // Lehrinhalte
                if (!string.IsNullOrWhiteSpace(LehrinhalteTextBox.Text))
                {
                    ersteVersion.Inhaltsgliederung = LehrinhalteTextBox.Text
                        .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                }
                else
                {
                    ersteVersion.Inhaltsgliederung = new List<string>();
                }

                // Literatur
                if (!string.IsNullOrWhiteSpace(LiteraturTextBox.Text))
                {
                    ersteVersion.Literatur = LiteraturTextBox.Text
                        .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                }
                else
                {
                    ersteVersion.Literatur = new List<string>();
                }

                db.ModulVersion.Add(ersteVersion);
                db.SaveChanges();

                return neuesModul.ModulID;
            }
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
                // ✅ Problem 2 Fix: Zurück zur ModulView (falls vorhanden) oder StartPage
                if (_isEditMode && !string.IsNullOrEmpty(_modulId))
                {
                    this.NavigationService?.Navigate(new ModulView(int.Parse(_modulId)));
                }
                else
                {
                    this.NavigationService?.Navigate(new StartPage());
                }
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

        // Validierung: Nur Zahlen in numerischen Feldern erlauben
        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Nur Ziffern erlauben, keine Buchstaben oder Sonderzeichen
            e.Handled = !IsTextNumeric(e.Text);
        }

        private bool IsTextNumeric(string text)
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
    }
}
