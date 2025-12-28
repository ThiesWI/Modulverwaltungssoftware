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

            // ✨ LIVE-VALIDIERUNG: TextChanged-Events registrieren
            EctsTextBox.TextChanged += ValidierePlausibilitaet;
            WorkloadPraesenzTextBox.TextChanged += ValidierePlausibilitaet;
            WorkloadSelbststudiumTextBox.TextChanged += ValidierePlausibilitaet;
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
            if (itemsToSelect == null || itemsToSelect.Count == 0)
            {
                listBox.SelectedItem = null;
                return;
            }
            
            // Nur das erste Item auswählen (Single-Selection-Modus)
            string firstItemToSelect = itemsToSelect[0].Trim();
            
            foreach (var item in listBox.Items)
            {
                if (item is ListBoxItem lbi)
                {
                    string itemText = lbi.Content.ToString().Trim();
                    
                    // Exakte Übereinstimmung bevorzugen
                    if (string.Equals(itemText, firstItemToSelect, StringComparison.OrdinalIgnoreCase))
                    {
                        listBox.SelectedItem = lbi;
                        return;
                    }
                    
                    // Fallback: Teil-Übereinstimmung
                    if (itemText.Contains(firstItemToSelect) || firstItemToSelect.Contains(itemText))
                    {
                        listBox.SelectedItem = lbi;
                        return;
                    }
                }
            }
            
            System.Diagnostics.Debug.WriteLine($"⚠️ Kein Match für '{firstItemToSelect}' in {listBox.Name} gefunden");
            listBox.SelectedItem = null;
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
                    Modul tempModul = new Modul
                    {
                        ModulnameDE = TitelTextBox.Text,
                        Studiengang = StudiengangTextBox.Text,
                        Modultyp = ModultypListBox.SelectedItem is ListBoxItem typLbi && typLbi.Content.ToString().Contains("Wahlpflicht")
                        ? Modul.ModultypEnum.Wahlpflicht
                        : Modul.ModultypEnum.Grundlagen,
                        Turnus = TurnusListBox.SelectedItem is ListBoxItem turnusLbi
                        ? (turnusLbi.Content.ToString().Contains("WiSe") || turnusLbi.Content.ToString().Contains("Wintersemester")
                        ? Modul.TurnusEnum.NurWintersemester
                        : (turnusLbi.Content.ToString().Contains("SoSe") || turnusLbi.Content.ToString().Contains("Sommersemester")
                        ? Modul.TurnusEnum.NurSommersemester
                        : Modul.TurnusEnum.JedesSemester))
                        : Modul.TurnusEnum.JedesSemester,
                        EmpfohlenesSemester = SemesterListBox.SelectedItem is ListBoxItem semLbi && int.TryParse(semLbi.Content.ToString(), out int sem)
                        ? sem
                        : 1,
                        Voraussetzungen = !string.IsNullOrWhiteSpace(VoraussetzungenTextBox.Text)
                        ? VoraussetzungenTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList()
                        : new List<string>()
                    };

                    int neueModulId = ModulRepository.addModul(tempModul);

                    if (neueModulId == -1)
                    {
                        // Fehlermeldung wurde bereits angezeigt
                        return;
                    }

                    // Jetzt: Initiale ModulVersion mit allen Nutzereingaben anlegen!
                    var neueVersion = new ModulVersion
                    {
                        ModulId = neueModulId,
                        Versionsnummer = 10, // 1.0
                        GueltigAbSemester = "Entwurf",
                        ModulStatus = ModulVersion.Status.Entwurf,
                        LetzteAenderung = DateTime.Now,
                        WorkloadPraesenz = int.Parse(WorkloadPraesenzTextBox.Text),
                        WorkloadSelbststudium = int.Parse(WorkloadSelbststudiumTextBox.Text),
                        EctsPunkte = int.Parse(EctsTextBox.Text),
                        Pruefungsform = PruefungsformListBox.SelectedItem is ListBoxItem lbi ? lbi.Content.ToString() : "Klausur",
                        Literatur = !string.IsNullOrWhiteSpace(LiteraturTextBox.Text)
                            ? LiteraturTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList()
                            : new List<string>(),
                        Lernergebnisse = !string.IsNullOrWhiteSpace(LernzieleTextBox.Text)
                            ? LernzieleTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList()
                            : new List<string>(),
                        Inhaltsgliederung = !string.IsNullOrWhiteSpace(LehrinhalteTextBox.Text)
                            ? LehrinhalteTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList()
                            : new List<string>(),
                        Ersteller = VerantwortlicherTextBox.Text,
                        hatKommentar = false
                    };
                    ModulRepository.Speichere(neueVersion);

                    MessageBox.Show($"Neues Modul '{TitelTextBox.Text}' wurde erfolgreich erstellt.",
                        "Gespeichert", MessageBoxButton.OK, MessageBoxImage.Information);

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

        private string GetSelectedListBoxItem(ListBox listBox)
        {
            if (listBox.SelectedItem is ListBoxItem lbi)
            {
                return lbi.Content.ToString();
            }
            return null;
        }

        private void ErstelleNeueVersionMitAenderungen(int modulId, int ects, int workloadPraesenz, int workloadSelbststudium)
        {
            // SPEZIFISCHE kommentierte Version laden (nicht die erste!)
            string cleanVersion = _versionNummer.TrimEnd('K');
            int aktuelleVersionsnummer = ParseVersionsnummer(cleanVersion);

            ModulVersion v = ModulRepository.getModulVersion(modulId);

            // Neue Version erstellen (NUR versionsspezifische Daten!)
            var neueVersion = new ModulVersion
            {
                ModulId = v.ModulId,
                Versionsnummer = v.Versionsnummer + 1,
                GueltigAbSemester = "Entwurf",
                ModulStatus = ModulVersion.Status.Entwurf,
                LetzteAenderung = DateTime.Now,
                WorkloadPraesenz = workloadPraesenz,
                WorkloadSelbststudium = workloadSelbststudium,
                EctsPunkte = ects,
                Ersteller = Benutzer.CurrentUser?.Name ?? "Unbekannt",
                hatKommentar = false
            };
            // Modul-Objekt aus dem aktuellen Kontext holen
            var modul = v.Modul;

            // Modultyp (Enum)
            if (ModultypListBox.SelectedItem is ListBoxItem modultypItem)
            {
                // Annahme: Content ist der Anzeigename, z.B. "Wahlpflichtmodul"
                // Mapping von UI-String zu Enum
                string modultypString = modultypItem.Content.ToString();
                if (modultypString.Contains("Wahlpflicht"))
                    modul.Modultyp = Modul.ModultypEnum.Wahlpflicht;
                else if (modultypString.Contains("Grundlagen"))
                    modul.Modultyp = Modul.ModultypEnum.Grundlagen;
                // ggf. weitere Fälle ergänzen
            }

            // Turnus (Enum)
            if (TurnusListBox.SelectedItem is ListBoxItem turnusItem)
            {
                string turnusString = turnusItem.Content.ToString();
                if (turnusString.Contains("Jedes Semester"))
                    modul.Turnus = Modul.TurnusEnum.JedesSemester;
                else if (turnusString.Contains("WiSe"))
                    modul.Turnus = Modul.TurnusEnum.NurWintersemester;
                else if (turnusString.Contains("SoSe"))
                    modul.Turnus = Modul.TurnusEnum.NurSommersemester;
                // ggf. weitere Fälle ergänzen
            }

            // EmpfohlenesSemester (int)
            if (SemesterListBox.SelectedItem is ListBoxItem semesterItem &&
                int.TryParse(semesterItem.Content.ToString(), out int semester))
            {
                modul.EmpfohlenesSemester = semester;
            }

            // Voraussetzungen (List<string> aus TextBox)
            if (!string.IsNullOrWhiteSpace(VoraussetzungenTextBox.Text))
            {
                modul.Voraussetzungen = VoraussetzungenTextBox.Text
                    .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                    .ToList();
            }
            else
            {
                modul.Voraussetzungen = new List<string>();
            }

            // Studiengang (string)
            modul.Studiengang = StudiengangTextBox.Text;


            // Prüfungsform (versionsspezifisch)
            string pruefungsform = GetSelectedListBoxItem(PruefungsformListBox);
            if (!string.IsNullOrEmpty(pruefungsform))
                neueVersion.Pruefungsform = pruefungsform;
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

            neueVersion.Modul = modul;
            ModulRepository.Speichere(neueVersion);
            return;
        }

        private void AktualisiereBestehendeVersion(int modulId, int ects, int workloadPraesenz, int workloadSelbststudium)
        {
            string cleanVersion = _versionNummer.TrimEnd('K');
            int versionsnummerInt = ParseVersionsnummer(cleanVersion);

            var dbVersion = ModulRepository.getModulVersion(modulId, versionsnummerInt);

            if (dbVersion == null)
            {
                MessageBox.Show("Modulversion nicht gefunden.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // ✅ PLAUSIBILITÄTSPRÜFUNG VOR DEM SPEICHERN
            int workloadGesamt = workloadPraesenz + workloadSelbststudium;
            string plausibilitaetsErgebnis = PlausibilitaetsService.pruefeWorkloadStandard(workloadGesamt, ects);

            if (plausibilitaetsErgebnis != "Der Workload entspricht dem Standard." &&
                plausibilitaetsErgebnis != "Der Workload liegt im akzeptablen Bereich.")
            {
                // Berechne Details für Fehlermeldung
                double stundenProEcts = ects > 0 ? (double)workloadGesamt / ects : 0;
                double berechneteEcts = workloadGesamt / 30.0;

                string detaillierteFehlermeldung = $"❌ ECTS-Plausibilitätsprüfung fehlgeschlagen!\n\n" +
                    $"📊 Ihre Eingaben:\n" +
                    $"   • Workload Präsenz: {workloadPraesenz} Stunden\n" +
                    $"   • Workload Selbststudium: {workloadSelbststudium} Stunden\n" +
                    $"   • Workload Gesamt: {workloadGesamt} Stunden\n" +
                    $"   • ECTS: {ects}\n\n" +
                    $"📐 Plausibilitätsrechnung:\n" +
                    $"   • Stunden pro ECTS: {stundenProEcts:0.##} Stunden/ECTS\n" +
                    $"   • Standard: 30 Stunden/ECTS\n" +
                    $"   • Empfohlener ECTS-Wert für {workloadGesamt}h: {berechneteEcts:0.#} ECTS\n\n" +
                    $"⚠️ Systemmeldung:\n{plausibilitaetsErgebnis}\n\n" +
                    $"Bitte passen Sie die Werte an, damit die Plausibilitätsprüfung erfolgreich ist.";

                MessageBox.Show(detaillierteFehlermeldung,
                    "ECTS-Plausibilitätsprüfung fehlgeschlagen",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return; // Abbruch - Änderungen werden NICHT gespeichert
            }

            // ✅ Plausibilitätsprüfung erfolgreich - Daten aktualisieren

            // Modul-Daten aktualisieren
            dbVersion.Modul.ModulnameDE = TitelTextBox.Text;
            dbVersion.Modul.Studiengang = StudiengangTextBox.Text;

            // Modultyp - EINZELAUSWAHL
            string modultyp = GetSelectedListBoxItem(ModultypListBox);
            if (!string.IsNullOrEmpty(modultyp))
            {
                if (modultyp.Contains("Wahlpflicht"))
                    dbVersion.Modul.Modultyp = Modul.ModultypEnum.Wahlpflicht;
                else if (modultyp.Contains("Grundlagen") || modultyp.Contains("Pflichtmodul"))
                    dbVersion.Modul.Modultyp = Modul.ModultypEnum.Grundlagen;

                System.Diagnostics.Debug.WriteLine($"Speichere Modultyp: '{modultyp}' -> {dbVersion.Modul.Modultyp}");
            }

            // Turnus - EINZELAUSWAHL
            string turnus = GetSelectedListBoxItem(TurnusListBox);
            if (!string.IsNullOrEmpty(turnus))
            {
                if (turnus.Contains("WiSe") || turnus.Contains("Wintersemester"))
                    dbVersion.Modul.Turnus = Modul.TurnusEnum.NurWintersemester;
                else if (turnus.Contains("SoSe") || turnus.Contains("Sommersemester"))
                    dbVersion.Modul.Turnus = Modul.TurnusEnum.NurSommersemester;
                else if (turnus.Contains("Jedes Semester") || turnus.Contains("Halbjährlich"))
                    dbVersion.Modul.Turnus = Modul.TurnusEnum.JedesSemester;

                System.Diagnostics.Debug.WriteLine($"Speichere Turnus: '{turnus}' -> {dbVersion.Modul.Turnus}");
            }

            // Prüfungsform - EINZELAUSWAHL
            string pruefungsform = GetSelectedListBoxItem(PruefungsformListBox);
            if (!string.IsNullOrEmpty(pruefungsform))
            {
                dbVersion.Pruefungsform = pruefungsform;
                System.Diagnostics.Debug.WriteLine($"Speichere Prüfungsform: '{pruefungsform}'");
            }

            // Semester - EINZELAUSWAHL
            string semester = GetSelectedListBoxItem(SemesterListBox);
            if (!string.IsNullOrEmpty(semester) && int.TryParse(semester, out int sem))
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
            dbVersion.ModulStatus = ModulVersion.Status.Entwurf;
            dbVersion.LetzteAenderung = DateTime.Now;
            System.Diagnostics.Debug.WriteLine("Speichere Änderungen in Datenbank...");
            bool b = ModulRepository.Speichere(dbVersion);
            if (b == true)
            {
                System.Diagnostics.Debug.WriteLine("Erfolgreich gespeichert!");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Fehler beim Speichern!");
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

        /// <summary>
        /// ✨ EINZELAUSWAHL-LOGIK: Ermöglicht das Abwählen durch erneutes Klicken
        /// </summary>
        private void ListBox_SingleSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox listBox)
            {
                // Wenn ein bereits ausgewähltes Item erneut geklickt wird, abwählen
                if (e.AddedItems.Count > 0 && e.RemovedItems.Count > 0)
                {
                    // Prüfe ob das neu hinzugefügte Item das gleiche ist wie das entfernte
                    // (bedeutet: User hat auf das bereits ausgewählte Item geklickt)
                    var added = e.AddedItems[0];
                    var removed = e.RemovedItems[0];
                    
                    if (added == removed)
                    {
                        // Abwählen durch erneutes Setzen auf null
                        listBox.SelectedItem = null;
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"ListBox '{listBox.Name}': SelectedItem = {listBox.SelectedItem?.ToString() ?? "null"}");
            }
        }
        /// <summary>
        /// ✨ LIVE-PLAUSIBILITÄTSPRÜFUNG: Validiert ECTS/Workload während der Eingabe
        /// </summary>
        private void ValidierePlausibilitaet(object sender, TextChangedEventArgs e)
        {
            // Werte auslesen
            if (!int.TryParse(EctsTextBox.Text, out int ects))
                ects = 0;
            if (!int.TryParse(WorkloadPraesenzTextBox.Text, out int workloadPraesenz))
                workloadPraesenz = 0;
            if (!int.TryParse(WorkloadSelbststudiumTextBox.Text, out int workloadSelbststudium))
                workloadSelbststudium = 0;

            int workloadGesamt = workloadPraesenz + workloadSelbststudium;

            // Standardzustand (keine Eingabe)
            if (ects == 0 || workloadGesamt == 0)
            {
                SetPlausibilitaetsFeedback(
                    "ℹ️",
                    "Geben Sie ECTS und Workload ein, um die Plausibilitätsprüfung zu starten.",
                    "",
                    "#F0F0F0", // Grau
                    "#CCCCCC",
                    "#666666"
                );
                return;
            }

            // Plausibilitätsprüfung durchführen
            string ergebnis = PlausibilitaetsService.pruefeWorkloadStandard(workloadGesamt, ects);
            double stundenProEcts = ects > 0 ? (double)workloadGesamt / ects : 0;
            double berechneteEcts = workloadGesamt / 30.0;

            // Details-Text erstellen
            string details = $"📊 Workload Gesamt: {workloadGesamt}h | ECTS: {ects} | Stunden/ECTS: {stundenProEcts:0.##}h\n" +
                           $"💡 Empfehlung: Für {workloadGesamt}h sind ca. {berechneteEcts:0.#} ECTS üblich (30h/ECTS-Standard)";

            // Visuelles Feedback basierend auf Ergebnis
            if (ergebnis == "Der Workload entspricht dem Standard.")
            {
                SetPlausibilitaetsFeedback(
                    "✅",
                    ergebnis,
                    details,
                    "#E8F5E9", // Hellgrün
                    "#4CAF50", // Grün
                    "#2E7D32"  // Dunkelgrün
                );
            }
            else if (ergebnis == "Der Workload liegt im akzeptablen Bereich.")
            {
                SetPlausibilitaetsFeedback(
                    "⚠️",
                    ergebnis,
                    details,
                    "#FFF3E0", // Hellorange
                    "#FF9800", // Orange
                    "#E65100"  // Dunkelorange
                );
            }
            else
            {
                SetPlausibilitaetsFeedback(
                    "❌",
                    ergebnis,
                    details,
                    "#FFEBEE", // Hellrot
                    "#F44336", // Rot
                    "#C62828"  // Dunkelrot
                );
            }
        }

        /// <summary>
        /// Hilfsmethode: Setzt das visuelle Feedback für die Plausibilitätsprüfung
        /// </summary>
        private void SetPlausibilitaetsFeedback(string icon, string meldung, string details,
                                                 string backgroundColor, string borderColor, string textColor)
        {
            PlausibilitaetsIcon.Text = icon;
            PlausibilitaetsLabel.Text = meldung;
            PlausibilitaetsLabel.Foreground = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(textColor));

            if (!string.IsNullOrEmpty(details))
            {
                PlausibilitaetsDetails.Text = details;
                PlausibilitaetsDetails.Visibility = Visibility.Visible;
            }
            else
            {
                PlausibilitaetsDetails.Visibility = Visibility.Collapsed;
            }

            PlausibilitaetsBorder.Background = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(backgroundColor));
            PlausibilitaetsBorder.BorderBrush = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(borderColor));
        }
    }
}
