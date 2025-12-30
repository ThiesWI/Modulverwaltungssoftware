using Modulverwaltungssoftware.Helpers;
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
    /// ═══════════════════════════════════════════════════════════════════
    /// EDITINGVIEW - MODUL BEARBEITEN/ERSTELLEN
    /// ═══════════════════════════════════════════════════════════════════
    /// 
    /// ZWECK:
    /// Zentrale View zum Erstellen neuer Module und Bearbeiten bestehender
    /// Modulversionen. Bietet ein Formular mit allen Modulfeldern inkl.
    /// Live-Validierung für ECTS/Workload-Verhältnis.
    /// 
    /// NAVIGATION:
    /// - Von StartPage: Neues Modul erstellen (EditingView(true))
    /// - Von ModulView: Bestehendes Modul bearbeiten (EditingView(modulId, version, data))
    /// 
    /// FEATURES:
    /// ✅ Live-Plausibilitätsprüfung für ECTS/Workload (28-32h/ECTS)
    /// ✅ Visuelle Feld-Validierung (rote Rahmen bei Fehlern)
    /// ✅ Unterscheidung: Neue Version vs. Update bestehende Version
    /// ✅ Kommentierte Versionen → Neue Version erstellen
    /// ✅ Nicht-kommentierte Versionen → In-Place Update
    /// 
    /// WICHTIGE METHODEN:
    /// - EntwurfSpeichern_Click(): Speichert/Validiert Änderungen
    /// - ValidateBasicInputs(): Prüft alle Pflichtfelder
    /// - ValidierePlausibilitaet(): Live-Feedback für ECTS/Workload
    /// 
    /// DATENFLUSS:
    /// UI → Validierung → ModulVersion-Objekt → ModulRepository.Speichere()
    /// ═══════════════════════════════════════════════════════════════════
    /// </summary>
    public partial class EditingView : Page
    {
        #region ═══════════════════════════════════════════════════════════
        // FELDER & EIGENSCHAFTEN
        #endregion ════════════════════════════════════════════════════════

        private ScrollViewer _contentScrollViewer;      // Scrollbarer Inhaltsbereich
        private string _modulId;                        // Modul-ID (null = Neues Modul)
        private string _versionNummer;                  // Aktuelle Version (z.B. "2.1")
        private bool _isEditMode;                       // true = Bearbeiten, false = Neu erstellen
        private bool _isCommentedVersion;               // true = Version hat Kommentare → Neue Version erstellen

        #region ═══════════════════════════════════════════════════════════
        // DATENKLASSEN
        #endregion ════════════════════════════════════════════════════════

        /// <summary>
        /// Daten-Transfer-Objekt für Modulversionen (kompatibel mit ModulView.ModuleData)
        /// Wird verwendet um Daten zwischen Views zu transportieren
        /// </summary>
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

        #region ═══════════════════════════════════════════════════════════
        // KONSTRUKTOREN
        #endregion ════════════════════════════════════════════════════════

        /// <summary>
        /// Standard-Konstruktor (wird von anderen Konstruktoren aufgerufen)
        /// Initialisiert UI-Komponenten und registriert Event-Handler
        /// </summary>
        public EditingView()
        {
            InitializeComponent();
            _contentScrollViewer = FindName("ContentScrollViewer") as ScrollViewer;
            _isEditMode = false; // Standard: Neues Modul

            // 🔴 LIVE-VALIDIERUNG: Event-Handler für ECTS/Workload-Prüfung
            EctsTextBox.TextChanged += ValidierePlausibilitaet;
            WorkloadPraesenzTextBox.TextChanged += ValidierePlausibilitaet;
            WorkloadSelbststudiumTextBox.TextChanged += ValidierePlausibilitaet;
        }

        /// <summary>
        /// Konstruktor für NEUES MODUL
        /// Navigation: StartPage → "Neues Modul erstellen"
        /// </summary>
        /// <param name="createNew">Flag für neues Modul (immer true)</param>
        public EditingView(bool createNew) : this()
        {
            _isEditMode = false;
            _modulId = null;
            _versionNummer = "1.0";
            VersionTextBox.Text = "1.0 (Entwurf)";
        }

        /// <summary>
        /// Konstruktor für BESTEHENDES MODUL BEARBEITEN
        /// Navigation: ModulView → "Bearbeiten"-Button
        /// 
        /// Prüft automatisch ob die Version kommentiert ist:
        /// - Kommentiert → Neue Version wird erstellt (ErstelleNeueVersionMitAenderungen)
        /// - Nicht kommentiert → In-Place Update (AktualisiereBestehendeVersion)
        /// </summary>
        /// <param name="modulId">Modul-ID als String</param>
        /// <param name="versionNummer">Version (z.B. "2.1" oder "2.1K" für kommentiert)</param>
        /// <param name="moduleData">Vorhandene Modul-Daten zum Befüllen der Felder</param>
        public EditingView(string modulId, string versionNummer, ModuleDataRepository.ModuleData moduleData) : this()
        {
            _isEditMode = true;
            _modulId = modulId;
            _versionNummer = versionNummer;

            // 🔍 PRÜFUNG: Ist die aktuelle Version kommentiert?
            using (var db = new Services.DatabaseContext())
            {
                int modulIdInt = int.Parse(modulId);
                string cleanVersion = versionNummer.TrimEnd('K'); // "2.1K" → "2.1"
                int versionsnummerInt = ParseVersionsnummer(cleanVersion); // "2.1" → 21

                var dbVersion = db.ModulVersion
                    .FirstOrDefault(v => v.ModulId == modulIdInt && v.Versionsnummer == versionsnummerInt);

                _isCommentedVersion = dbVersion != null && dbVersion.hatKommentar;
            }

            LoadModuleData(moduleData);
        }

        #region ═══════════════════════════════════════════════════════════
        // DATEN LADEN & ANZEIGEN
        #endregion ════════════════════════════════════════════════════════

        /// <summary>
        /// Befüllt alle UI-Felder mit den übergebenen Modul-Daten
        /// Wird beim Bearbeiten eines bestehenden Moduls aufgerufen
        /// </summary>
        /// <param name="data">Modul-Daten aus ModulView</param>
        private void LoadModuleData(ModuleDataRepository.ModuleData data)
        {
            if (data == null) return;

            // 📝 TEXTFELDER befüllen
            TitelTextBox.Text = data.Titel;
            VersionTextBox.Text = $"{_versionNummer} (Entwurf)"; // Read-only Anzeige
            StudiengangTextBox.Text = data.Studiengang;
            EctsTextBox.Text = data.Ects.ToString();
            WorkloadPraesenzTextBox.Text = data.WorkloadPraesenz.ToString();
            WorkloadSelbststudiumTextBox.Text = data.WorkloadSelbststudium.ToString();
            VerantwortlicherTextBox.Text = data.Verantwortlicher;
            VoraussetzungenTextBox.Text = data.Voraussetzungen;
            LernzieleTextBox.Text = data.Lernziele;
            LehrinhalteTextBox.Text = data.Lehrinhalte;
            LiteraturTextBox.Text = data.Literatur;

            // 📋 LISTBOXEN befüllen (Single-Selection)
            SelectListBoxItems(ModultypListBox, data.Modultypen);
            SelectListBoxItems(SemesterListBox, data.Semester);
            SelectListBoxItems(PruefungsformListBox, data.Pruefungsformen);
            SelectListBoxItems(TurnusListBox, data.Turnus);
        }

        /// <summary>
        /// Wählt ein Item in einer ListBox aus (nur erstes Item bei Multi-Selection)
        /// 
        /// MATCHING-STRATEGIE:
        /// 1. Exakte Übereinstimmung (case-insensitive)
        /// 2. Fallback: Teil-Übereinstimmung
        /// 3. Kein Match → null-Auswahl
        /// </summary>
        /// <param name="listBox">Ziel-ListBox</param>
        /// <param name="itemsToSelect">Liste der zu selektierenden Items</param>
        private void SelectListBoxItems(ListBox listBox, List<string> itemsToSelect)
        {
            if (itemsToSelect == null || itemsToSelect.Count == 0)
            {
                listBox.SelectedItem = null;
                return;
            }

            // ⚠️ WICHTIG: Nur erstes Item wählen (Single-Selection-Modus)
            string firstItemToSelect = itemsToSelect[0].Trim();

            foreach (var item in listBox.Items)
            {
                if (item is ListBoxItem lbi)
                {
                    string itemText = lbi.Content.ToString().Trim();

                    // ✅ EXAKTE ÜBEREINSTIMMUNG (bevorzugt)
                    if (string.Equals(itemText, firstItemToSelect, StringComparison.OrdinalIgnoreCase))
                    {
                        listBox.SelectedItem = lbi;
                        return;
                    }

                    // 🔄 FALLBACK: Teil-Übereinstimmung
                    if (itemText.Contains(firstItemToSelect) || firstItemToSelect.Contains(itemText))
                    {
                        listBox.SelectedItem = lbi;
                        return;
                    }
                }
            }

            // ⚠️ KEIN MATCH GEFUNDEN
            System.Diagnostics.Debug.WriteLine($"⚠️ Kein Match für '{firstItemToSelect}' in {listBox.Name} gefunden");
            listBox.SelectedItem = null;
        }

        #region ═══════════════════════════════════════════════════════════
        // SPEICHERN & VALIDIERUNG
        #endregion ════════════════════════════════════════════════════════

        /// <summary>
        /// ═══════════════════════════════════════════════════════════════════
        /// EVENT: ENTWURF SPEICHERN
        /// ═══════════════════════════════════════════════════════════════════
        /// 
        /// ABLAUF:
        /// 1. Validierungs-Highlights zurücksetzen
        /// 2. Basis-Validierung (Pflichtfelder, Datentypen)
        /// 3. Bei Fehlern → Abbruch mit visueller Hervorhebung
        /// 4. Bei Edit-Mode:
        ///    - Kommentierte Version? → Neue Version erstellen
        ///    - Nicht kommentiert? → Bestehende Version aktualisieren
        /// 5. Bei Neu-Modus:
        ///    - Modul + initiale Version erstellen
        /// 6. Navigation zurück zu ModulView
        /// 
        /// VALIDIERUNG:
        /// - Titel, ECTS, Workload, Verantwortlicher, Lernziele, Lehrinhalte
        /// - Modultyp, Semester, Prüfungsform, Turnus (ListBoxen)
        /// - ECTS/Workload-Verhältnis (28-32h/ECTS)
        /// ═══════════════════════════════════════════════════════════════════
        /// </summary>
        private void EntwurfSpeichern_Click(object sender, RoutedEventArgs e)
        {
            // 🔄 SCHRITT 1: Alle Validierungs-Highlights zurücksetzen
            ResetValidationHighlights();

            // ✅ SCHRITT 2: Basis-Validierung durchführen
            if (!ValidateBasicInputs(out int ects, out int workloadPraesenz, out int workloadSelbststudium))
            {
                return; // ⛔ Validierung fehlgeschlagen → Felder sind rot markiert
            }

            // 📝 SCHRITT 3: EDIT-MODUS oder NEU-MODUS?
            if (_isEditMode)
            {
                // ═══════════════════════════════════════════════════════════
                // BESTEHENDES MODUL BEARBEITEN
                // ═══════════════════════════════════════════════════════════
                try
                {
                    int modulIdInt = int.Parse(_modulId);

                    if (_isCommentedVersion)
                    {
                        // 🆕 KOMMENTIERTE VERSION → Neue Version erstellen
                        // (Kommentare bleiben an alter Version)
                        ErstelleNeueVersionMitAenderungen(modulIdInt, ects, workloadPraesenz, workloadSelbststudium);
                    }
                    else
                    {
                        // 🔄 NICHT-KOMMENTIERTE VERSION → In-Place Update
                        // (Bestehende Version wird überschrieben)
                        AktualisiereBestehendeVersion(modulIdInt, ects, workloadPraesenz, workloadSelbststudium);
                    }

                    MessageBox.Show($"Änderungen wurden erfolgreich gespeichert.",
                        "Gespeichert", MessageBoxButton.OK, MessageBoxImage.Information);

                    // 🔙 Zurück zur ModulView
                    this.NavigationService?.Navigate(new ModulView(modulIdInt));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fehler beim Speichern: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                // ═══════════════════════════════════════════════════════════
                // NEUES MODUL ERSTELLEN
                // ═══════════════════════════════════════════════════════════
                try
                {
                    // 🏗️ MODUL-OBJEKT erstellen
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

                    // 💾 Modul in Datenbank speichern
                    int neueModulId = ModulRepository.addModul(tempModul);

                    if (neueModulId == -1)
                    {
                        return; // ⛔ Fehler beim Speichern (MessageBox wurde bereits angezeigt)
                    }

                    // 🏗️ INITIALE MODULVERSION erstellen (Version 1.0)
                    var neueVersion = new ModulVersion
                    {
                        ModulId = neueModulId,
                        Versionsnummer = 10, // 1.0 (Faktor 10!)
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
                    neueVersion.Modul = WorkflowController.getModulDetails(neueModulId);
                    ModulRepository.Speichere(neueVersion);

                    MessageBox.Show($"Neues Modul '{TitelTextBox.Text}' wurde erfolgreich erstellt.",
                        "Gespeichert", MessageBoxButton.OK, MessageBoxImage.Information);

                    // 🔙 Zur ModulView des neu erstellten Moduls navigieren
                    this.NavigationService?.Navigate(new ModulView(neueModulId));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fehler beim Erstellen des Moduls: {ex.Message}",
                        "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// ═══════════════════════════════════════════════════════════════════
        /// VALIDIERUNG: BASIS-EINGABEN PRÜFEN
        /// ═══════════════════════════════════════════════════════════════════
        /// 
        /// Prüft alle Pflichtfelder und hebt fehlerhafte Felder visuell hervor:
        /// - Roter Rahmen (2px, #DC3545)
        /// - Transparenter roter Hintergrund
        /// - Tooltip mit Fehlermeldung "❌ [Nachricht]"
        /// 
        /// VALIDIERUNGSREGELN:
        /// 
        /// TEXTFELDER:
        /// - Titel: Nicht leer
        /// - ECTS: Ganzzahl > 0
        /// - Workload Präsenz: Ganzzahl
        /// - Workload Selbststudium: Ganzzahl
        /// - Verantwortlicher: Nicht leer
        /// - Lernziele: Mindestens 1 Zeile
        /// - Lehrinhalte: Mindestens 1 Zeile
        /// 
        /// LISTBOXEN (Single-Selection):
        /// - Modultyp: Auswahl erforderlich
        /// - Semester: 1-8
        /// - Prüfungsform: Auswahl erforderlich
        /// - Turnus: Auswahl erforderlich
        /// 
        /// RÜCKGABE:
        /// - true: Alle Felder gültig
        /// - false: Fehler gefunden (Felder sind rot markiert + MessageBox)
        /// ═══════════════════════════════════════════════════════════════════
        /// </summary>
        private bool ValidateBasicInputs(out int ects, out int workloadPraesenz, out int workloadSelbststudium)
        {
            ects = 0;
            workloadPraesenz = 0;
            workloadSelbststudium = 0;
            bool isValid = true;

            // ═══════════════════════════════════════════════════════════════
            // TEXTFELD-VALIDIERUNG
            // ═══════════════════════════════════════════════════════════════

            if (string.IsNullOrWhiteSpace(TitelTextBox.Text))
            {
                ValidationHelper.MarkAsInvalid(TitelTextBox, "Titel darf nicht leer sein.");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(EctsTextBox.Text) || !int.TryParse(EctsTextBox.Text, out ects) || ects <= 0)
            {
                ValidationHelper.MarkAsInvalid(EctsTextBox, "Bitte gültige ECTS-Punktzahl (> 0) eingeben.");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(WorkloadPraesenzTextBox.Text) || !int.TryParse(WorkloadPraesenzTextBox.Text, out workloadPraesenz))
            {
                ValidationHelper.MarkAsInvalid(WorkloadPraesenzTextBox, "Bitte gültige Workload Präsenz eingeben.");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(WorkloadSelbststudiumTextBox.Text) || !int.TryParse(WorkloadSelbststudiumTextBox.Text, out workloadSelbststudium))
            {
                ValidationHelper.MarkAsInvalid(WorkloadSelbststudiumTextBox, "Bitte gültige Workload Selbststudium eingeben.");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(VerantwortlicherTextBox.Text))
            {
                ValidationHelper.MarkAsInvalid(VerantwortlicherTextBox, "Verantwortlicher darf nicht leer sein.");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(LernzieleTextBox.Text))
            {
                ValidationHelper.MarkAsInvalid(LernzieleTextBox, "Bitte mindestens ein Lernziel angeben.");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(LehrinhalteTextBox.Text))
            {
                ValidationHelper.MarkAsInvalid(LehrinhalteTextBox, "Bitte mindestens einen Lehrinhalt angeben.");
                isValid = false;
            }

            // ═══════════════════════════════════════════════════════════════
            // LISTBOX-VALIDIERUNG (Dropdown-Auswahl erforderlich)
            // ═══════════════════════════════════════════════════════════════

            if (ModultypListBox.SelectedItem == null)
            {
                ValidationHelper.MarkAsInvalid(ModultypListBox, "Bitte einen Modultyp auswählen.");
                isValid = false;
            }

            if (SemesterListBox.SelectedItem == null)
            {
                ValidationHelper.MarkAsInvalid(SemesterListBox, "Bitte ein empfohlenes Semester auswählen (1-8).");
                isValid = false;
            }

            if (PruefungsformListBox.SelectedItem == null)
            {
                ValidationHelper.MarkAsInvalid(PruefungsformListBox, "Bitte eine Prüfungsform auswählen.");
                isValid = false;
            }

            if (TurnusListBox.SelectedItem == null)
            {
                ValidationHelper.MarkAsInvalid(TurnusListBox, "Bitte einen Turnus auswählen.");
                isValid = false;
            }

            // ═══════════════════════════════════════════════════════════════
            // FEHLER-FEEDBACK
            // ═══════════════════════════════════════════════════════════════

            if (!isValid)
            {
                MessageBox.Show(
                    "Bitte korrigieren Sie die markierten Felder.\n\n" +
                    "Fehlerhafte Felder sind rot umrandet und enthalten einen Tooltip mit Details.",
                    "Validierungsfehler",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }

            return isValid;
        }

        /// <summary>
        /// Setzt alle Validierungs-Hervorhebungen zurück
        /// Wird vor jeder Validierung aufgerufen um alte Fehler zu löschen
        /// </summary>
        private void ResetValidationHighlights()
        {
            ValidationHelper.ResetAll(
                TitelTextBox,
                EctsTextBox,
                WorkloadPraesenzTextBox,
                WorkloadSelbststudiumTextBox,
                VerantwortlicherTextBox,
                LernzieleTextBox,
                LehrinhalteTextBox,
                ModultypListBox,
                SemesterListBox,
                PruefungsformListBox,
                TurnusListBox
            );
        }

        #region ═══════════════════════════════════════════════════════════
        // VERSIONSVERWALTUNG
        #endregion ════════════════════════════════════════════════════════

        /// <summary>
        /// ═══════════════════════════════════════════════════════════════════
        /// NEUE VERSION MIT ÄNDERUNGEN ERSTELLEN
        /// ═══════════════════════════════════════════════════════════════════
        /// 
        /// ZWECK:
        /// Erstellt eine neue Modulversion basierend auf einer kommentierten Version
        /// Die alte Version (mit Kommentaren) bleibt unverändert erhalten!
        /// 
        /// ABLAUF:
        /// 1. Höchste vorhandene Versionsnummer ermitteln
        /// 2. Neue Versionsnummer = Höchste + 1 (z.B. 2.1 → 2.2)
        /// 3. Alle Modul-Daten (Titel, Modultyp, etc.) aktualisieren
        /// 4. Neue ModulVersion mit Status "Entwurf" erstellen
        /// 5. In Datenbank speichern (ModulRepository.Speichere)
        /// 
        /// WICHTIG:
        /// - Versionsnummern werden * 10 gespeichert (2.1 → 21)
        /// - Status wird auf "Entwurf" gesetzt
        /// - hatKommentar = false (neue Version ist unkommentiert)
        /// ═══════════════════════════════════════════════════════════════════
        /// </summary>
        private void ErstelleNeueVersionMitAenderungen(int modulId, int ects, int workloadPraesenz, int workloadSelbststudium)
        {
            // 🔍 Aktuelle Version laden
            string cleanVersion = _versionNummer.TrimEnd('K'); // "2.1K" → "2.1"
            int aktuelleVersionsnummer = ParseVersionsnummer(cleanVersion);

            ModulVersion v = ModulRepository.getModulVersion(modulId);

            // 🆕 NEUE VERSION erstellen (Versionsnummer +1)
            var neueVersion = new ModulVersion
            {
                ModulId = v.ModulId,
                Versionsnummer = v.Versionsnummer + 1, // z.B. 21 → 22 (2.1 → 2.2)
                GueltigAbSemester = "Entwurf",
                ModulStatus = ModulVersion.Status.Entwurf,
                LetzteAenderung = DateTime.Now,
                WorkloadPraesenz = workloadPraesenz,
                WorkloadSelbststudium = workloadSelbststudium,
                EctsPunkte = ects,
                Ersteller = Benutzer.CurrentUser?.Name ?? "Unbekannt",
                hatKommentar = false // Neue Version ist unkommentiert!
            };

            // 🏗️ MODUL-DATEN aus UI auslesen und aktualisieren
            Modul modul = WorkflowController.getModulDetails(modulId);

            // 📋 MODULTYP (Enum: Wahlpflicht / Grundlagen)
            if (ModultypListBox.SelectedItem is ListBoxItem modultypItem)
            {
                string modultypString = modultypItem.Content.ToString();
                if (modultypString.Contains("Wahlpflicht"))
                    modul.Modultyp = Modul.ModultypEnum.Wahlpflicht;
                else if (modultypString.Contains("Grundlagen"))
                    modul.Modultyp = Modul.ModultypEnum.Grundlagen;
            }

            // 📋 TURNUS (Enum: JedesSemester / NurWintersemester / NurSommersemester)
            if (TurnusListBox.SelectedItem is ListBoxItem turnusItem)
            {
                string turnusString = turnusItem.Content.ToString();
                if (turnusString.Contains("Jedes Semester"))
                    modul.Turnus = Modul.TurnusEnum.JedesSemester;
                else if (turnusString.Contains("WiSe"))
                    modul.Turnus = Modul.TurnusEnum.NurWintersemester;
                else if (turnusString.Contains("SoSe"))
                    modul.Turnus = Modul.TurnusEnum.NurSommersemester;
            }

            // 📋 EMPFOHLENES SEMESTER (1-8)
            if (SemesterListBox.SelectedItem is ListBoxItem semesterItem &&
                int.TryParse(semesterItem.Content.ToString(), out int semester))
            {
                modul.EmpfohlenesSemester = semester;
            }

            // 📝 VORAUSSETZUNGEN (Multi-Line String → List<string>)
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

            // 📝 STUDIENGANG
            modul.Studiengang = StudiengangTextBox.Text;

            // 📋 PRÜFUNGSFORM (versionsspezifisch!)
            string pruefungsform = GetSelectedListBoxItem(PruefungsformListBox);
            if (!string.IsNullOrEmpty(pruefungsform))
                neueVersion.Pruefungsform = pruefungsform;
            else
                neueVersion.Pruefungsform = "Klausur"; // Fallback

            // 📝 LERNZIELE (versionsspezifisch, Multi-Line → List<string>)
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

            // 📝 LEHRINHALTE (versionsspezifisch, Multi-Line → List<string>)
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

            // 📚 LITERATUR (versionsspezifisch, Multi-Line → List<string>)
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

            // 🔗 Modul-Objekt an Version anhängen
            neueVersion.Modul = modul;

            // 💾 In Datenbank speichern
            ModulRepository.Speichere(neueVersion);
        }

        /// <summary>
        /// ═══════════════════════════════════════════════════════════════════
        /// BESTEHENDE VERSION AKTUALISIEREN (IN-PLACE UPDATE)
        /// ═══════════════════════════════════════════════════════════════════
        /// 
        /// ZWECK:
        /// Aktualisiert eine nicht-kommentierte Modulversion direkt
        /// KEINE neue Version wird erstellt!
        /// 
        /// ABLAUF:
        /// 1. PLAUSIBILITÄTSPRÜFUNG: ECTS/Workload-Verhältnis prüfen (28-32h/ECTS)
        /// 2. Bei Fehler → Abbruch mit detaillierter Fehlermeldung
        /// 3. Bei Erfolg → Modul-Daten aus UI in Datenbank-Objekt übertragen
        /// 4. ModulRepository.Speichere() aufrufen
        /// 
        /// WICHTIG:
        /// - Nur für Versionen mit Status "Entwurf" oder "Änderungsbedarf"
        /// - Versionsnummer bleibt UNVERÄNDERT
        /// - Kommentare bleiben erhalten (falls vorhanden)
        /// ═══════════════════════════════════════════════════════════════════
        /// </summary>
        private void AktualisiereBestehendeVersion(int modulId, int ects, int workloadPraesenz, int workloadSelbststudium)
        {
            // 🔍 Aktuelle Version aus Datenbank laden
            string cleanVersion = _versionNummer.TrimEnd('K');
            int versionsnummerInt = ParseVersionsnummer(cleanVersion);

            var dbVersion = ModulRepository.getModulVersion(modulId, versionsnummerInt);

            if (dbVersion == null)
            {
                MessageBox.Show("Modulversion nicht gefunden.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // ═══════════════════════════════════════════════════════════════
            // PLAUSIBILITÄTSPRÜFUNG: ECTS/WORKLOAD-VERHÄLTNIS
            // ═══════════════════════════════════════════════════════════════
            int workloadGesamt = workloadPraesenz + workloadSelbststudium;
            string plausibilitaetsErgebnis = PlausibilitaetsService.pruefeWorkloadStandard(workloadGesamt, ects);

            // ⛔ FEHLER: Workload entspricht NICHT dem Standard (28-32h/ECTS)
            if (plausibilitaetsErgebnis != "Der Workload entspricht dem Standard." &&
                plausibilitaetsErgebnis != "Der Workload liegt im akzeptablen Bereich.")
            {
                // 📊 Detaillierte Fehlerberechnung
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
                
                return; // ⛔ ABBRUCH - Änderungen werden NICHT gespeichert
            }

            // ═══════════════════════════════════════════════════════════════
            // ✅ PLAUSIBILITÄTSPRÜFUNG ERFOLGREICH - DATEN AKTUALISIEREN
            // ═══════════════════════════════════════════════════════════════

            // 📝 MODUL-BASIS-DATEN
            dbVersion.Modul.ModulnameDE = TitelTextBox.Text;
            dbVersion.Modul.Studiengang = StudiengangTextBox.Text;

            // 📋 MODULTYP (Enum)
            string modultyp = GetSelectedListBoxItem(ModultypListBox);
            if (!string.IsNullOrEmpty(modultyp))
            {
                if (modultyp.Contains("Wahlpflicht"))
                    dbVersion.Modul.Modultyp = Modul.ModultypEnum.Wahlpflicht;
                else if (modultyp.Contains("Grundlagen") || modultyp.Contains("Pflichtmodul"))
                    dbVersion.Modul.Modultyp = Modul.ModultypEnum.Grundlagen;

                System.Diagnostics.Debug.WriteLine($"💾 Speichere Modultyp: '{modultyp}' → {dbVersion.Modul.Modultyp}");
            }

            // 📋 TURNUS (Enum)
            string turnus = GetSelectedListBoxItem(TurnusListBox);
            if (!string.IsNullOrEmpty(turnus))
            {
                if (turnus.Contains("WiSe") || turnus.Contains("Wintersemester"))
                    dbVersion.Modul.Turnus = Modul.TurnusEnum.NurWintersemester;
                else if (turnus.Contains("SoSe") || turnus.Contains("Sommersemester"))
                    dbVersion.Modul.Turnus = Modul.TurnusEnum.NurSommersemester;
                else if (turnus.Contains("Jedes Semester") || turnus.Contains("Halbjährlich"))
                    dbVersion.Modul.Turnus = Modul.TurnusEnum.JedesSemester;

                System.Diagnostics.Debug.WriteLine($"💾 Speichere Turnus: '{turnus}' → {dbVersion.Modul.Turnus}");
            }

            // 📋 PRÜFUNGSFORM (versionsspezifisch)
            string pruefungsform = GetSelectedListBoxItem(PruefungsformListBox);
            if (!string.IsNullOrEmpty(pruefungsform))
            {
                dbVersion.Pruefungsform = pruefungsform;
                System.Diagnostics.Debug.WriteLine($"💾 Speichere Prüfungsform: '{pruefungsform}'");
            }

            // 📋 SEMESTER (1-8)
            string semester = GetSelectedListBoxItem(SemesterListBox);
            if (!string.IsNullOrEmpty(semester) && int.TryParse(semester, out int sem))
            {
                dbVersion.Modul.EmpfohlenesSemester = sem;
                System.Diagnostics.Debug.WriteLine($"💾 Speichere Semester: {sem}");
            }

            // 📝 VORAUSSETZUNGEN (Multi-Line → List<string>)
            if (!string.IsNullOrWhiteSpace(VoraussetzungenTextBox.Text))
            {
                dbVersion.Modul.Voraussetzungen = VoraussetzungenTextBox.Text
                    .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                    .ToList();
            }
            else
            {
                dbVersion.Modul.Voraussetzungen = new List<string>();
            }

            // 📊 MODULVERSION-DATEN (versionsspezifisch)
            dbVersion.EctsPunkte = ects;
            dbVersion.WorkloadPraesenz = workloadPraesenz;
            dbVersion.WorkloadSelbststudium = workloadSelbststudium;
            dbVersion.Ersteller = VerantwortlicherTextBox.Text;

            // 📝 LERNZIELE (Multi-Line → List<string>)
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

            // 📝 LEHRINHALTE (Multi-Line → List<string>)
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

            // 📚 LITERATUR (Multi-Line → List<string>)
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

            // 🕒 Zeitstempel aktualisieren
            dbVersion.LetzteAenderung = DateTime.Now;

            // 💾 IN DATENBANK SPEICHERN
            System.Diagnostics.Debug.WriteLine("💾 Speichere Änderungen in Datenbank...");
            bool erfolg = ModulRepository.Speichere(dbVersion);
            
            if (erfolg)
            {
                System.Diagnostics.Debug.WriteLine("✅ Erfolgreich gespeichert!");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("❌ Fehler beim Speichern!");
            }
        }

        #region ═══════════════════════════════════════════════════════════
        // EVENT-HANDLER & HILFSMETHODEN
        #endregion ════════════════════════════════════════════════════════

        /// <summary>
        /// EVENT: Entwurf verwerfen
        /// Navigiert zurück zur ModulView (bei Bearbeitung) oder StartPage (bei neuem Modul)
        /// Zeigt Sicherheitsabfrage an
        /// </summary>
        private void EntwurfVerwerfen_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
        "Möchten Sie den Entwurf wirklich verwerfen?",
        "Warnung",
        MessageBoxButton.YesNo,
        MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                if (_isEditMode && !string.IsNullOrEmpty(_modulId))
                {
                    // 🔙 Zurück zur ModulView (bei Bearbeitung)
                    this.NavigationService?.Navigate(new ModulView(int.Parse(_modulId)));
                }
                else
                {
                    // 🔙 Zurück zur StartPage (bei neuem Modul)
                    this.NavigationService?.Navigate(new StartPage());
                }
            }
        }

        /// <summary>
        /// Ermöglicht Scrollen unabhängig vom Maus-Fokus
        /// Überschreibt das Scroll-Verhalten für bessere UX
        /// </summary>
        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            base.OnPreviewMouseWheel(e);
            
            if (_contentScrollViewer != null)
            {
                _contentScrollViewer.ScrollToVerticalOffset(_contentScrollViewer.VerticalOffset - e.Delta);
                e.Handled = true;
            }
        }

        /// <summary>
        /// INPUT-VALIDIERUNG: Nur Zahlen in numerischen Feldern erlauben
        /// Wird für ECTS und Workload-Felder verwendet
        /// </summary>
        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextNumeric(e.Text);
        }

        /// <summary>
        /// Hilfsmethode: Prüft ob ein String nur Ziffern enthält
        /// </summary>
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
        /// EINZELAUSWAHL-LOGIK: Ermöglicht Abwählen durch erneutes Klicken
        /// Wird für alle ListBoxen verwendet (Modultyp, Semester, Prüfungsform, Turnus)
        /// </summary>
        private void ListBox_SingleSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox listBox)
            {
                // Wenn bereits ausgewähltes Item erneut geklickt wird → Abwählen
                if (e.AddedItems.Count > 0 && e.RemovedItems.Count > 0)
                {
                    var added = e.AddedItems[0];
                    var removed = e.RemovedItems[0];

                    if (added == removed)
                    {
                        listBox.SelectedItem = null; // Abwählen
                    }
                }

                System.Diagnostics.Debug.WriteLine($"ListBox '{listBox.Name}': SelectedItem = {listBox.SelectedItem?.ToString() ?? "null"}");
            }
        }

        #region ═══════════════════════════════════════════════════════════
        // LIVE-PLAUSIBILITÄTSPRÜFUNG (ECTS/WORKLOAD)
        #endregion ════════════════════════════════════════════════════════

        /// <summary>
        /// ═══════════════════════════════════════════════════════════════════
        /// LIVE-PLAUSIBILITÄTSPRÜFUNG: ECTS/WORKLOAD-VERHÄLTNIS
        /// ═══════════════════════════════════════════════════════════════════
        /// 
        /// ZWECK:
        /// Gibt dem Benutzer während der Eingabe sofortiges Feedback zum
        /// ECTS/Workload-Verhältnis (Standard: 30h/ECTS, akzeptabel: 28-32h/ECTS)
        /// 
        /// VISUELLES FEEDBACK:
        /// 
        /// ✅ STANDARD (28-32h/ECTS):
        ///    - Icon: ✅
        ///    - Hintergrund: Hellgrün (#E8F5E9)
        ///    - Border: Grün (#4CAF50)
        /// 
        /// ⚠️ AKZEPTABEL (75-450h gesamt):
        ///    - Icon: ⚠️
        ///    - Hintergrund: Hellorange (#FFF3E0)
        ///    - Border: Orange (#FF9800)
        /// 
        /// ❌ FEHLER (außerhalb Bereich):
        ///    - Icon: ❌
        ///    - Hintergrund: Hellrot (#FFEBEE)
        ///    - Border: Rot (#F44336)
        /// 
        /// HINWEIS:
        /// Dieses Live-Feedback ist nur eine Warnung!
        /// Speichern wird erst durch ValidateBasicInputs() blockiert.
        /// ═══════════════════════════════════════════════════════════════════
        /// </summary>
        private void ValidierePlausibilitaet(object sender, TextChangedEventArgs e)
        {
            // 📊 Werte aus UI auslesen
            if (!int.TryParse(EctsTextBox.Text, out int ects))
                ects = 0;
            if (!int.TryParse(WorkloadPraesenzTextBox.Text, out int workloadPraesenz))
                workloadPraesenz = 0;
            if (!int.TryParse(WorkloadSelbststudiumTextBox.Text, out int workloadSelbststudium))
                workloadSelbststudium = 0;

            int workloadGesamt = workloadPraesenz + workloadSelbststudium;

            // ℹ️ STANDARDZUSTAND: Keine vollständige Eingabe
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

            // 🔍 PLAUSIBILITÄTSPRÜFUNG DURCHFÜHREN
            string ergebnis = PlausibilitaetsService.pruefeWorkloadStandard(workloadGesamt, ects);
            double stundenProEcts = ects > 0 ? (double)workloadGesamt / ects : 0;
            double berechneteEcts = workloadGesamt / 30.0;

            // 📝 Details-Text erstellen
            string details = $"📊 Workload Gesamt: {workloadGesamt}h | ECTS: {ects} | Stunden/ECTS: {stundenProEcts:0.##}h\n" +
                           $"💡 Empfehlung: Für {workloadGesamt}h sind ca. {berechneteEcts:0.#} ECTS üblich (30h/ECTS-Standard)";

            // 🎨 VISUELLES FEEDBACK ANZEIGEN
            if (ergebnis == "Der Workload entspricht dem Standard.")
            {
                // ✅ PERFEKT: 28-32h/ECTS
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
                // ⚠️ AKZEPTABEL: 75-450h gesamt
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
                // ❌ FEHLER: Außerhalb gültiger Bereiche
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
        /// Aktualisiert Icon, Text, Details, Hintergrund- und Border-Farben
        /// </summary>
        /// <param name="icon">Emoji-Icon (✅, ⚠️, ❌, ℹ️)</param>
        /// <param name="meldung">Hauptnachricht</param>
        /// <param name="details">Detaillierte Berechnung (optional)</param>
        /// <param name="backgroundColor">Hintergrundfarbe (Hex)</param>
        /// <param name="borderColor">Border-Farbe (Hex)</param>
        /// <param name="textColor">Text-Farbe (Hex)</param>
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

        #region ═══════════════════════════════════════════════════════════
        // LISTBOX-HILFSMETHODEN
        #endregion ════════════════════════════════════════════════════════

        /// <summary>
        /// Gibt alle ausgewählten Items einer ListBox als String-Liste zurück
        /// (Legacy-Methode, wird aktuell nicht verwendet - Single-Selection!)
        /// </summary>
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

        /// <summary>
        /// Gibt das ausgewählte Item einer ListBox als String zurück
        /// Verwendet für Single-Selection ListBoxen
        /// </summary>
        /// <returns>Content-Text des ausgewählten Items oder null</returns>
        private string GetSelectedListBoxItem(ListBox listBox)
        {
            if (listBox.SelectedItem is ListBoxItem lbi)
            {
                return lbi.Content.ToString();
            }
            return null;
        }

        /// <summary>
        /// Konvertiert Versionsnummer von String zu Integer
        /// Format: "2.1" → 21 (Faktor 10!)
        /// Fallback: 10 (= Version 1.0)
        /// </summary>
        private int ParseVersionsnummer(string version)
        {
            if (decimal.TryParse(version, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal dec))
                return (int)(dec * 10);
            return 10; // Fallback: Version 1.0
        }
    }
}
