using PDF_Test;
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
        private bool _isModuleLoaded = false;  // Flag um zu tracken ob ein Modul geladen wurde
        private ModulVersion _loadedModulVersion = null;  // ? Zwischenspeicher für geladene ModulVersion

        public ModulView()
        {
            InitializeComponent();
            this.DataContext = this;
            
            // ? SUCHFUNKTION: TextChanged Event für SearchBox
            SearchBox.TextChanged += SearchBox_TextChanged;
            
            // ? BUTTON-STEUERUNG: Buttons setzen nach dem Laden der Page
            this.Loaded += ModulView_Loaded;
        }

        private void ModulView_Loaded(object sender, RoutedEventArgs e)
        {
            // ? STRATEGIE: Wenn Modul geladen wurde, Button-States basierend auf Daten setzen
            // Ansonsten: Initial alle Buttons deaktivieren
            if (_loadedModulVersion != null)
            {
                // Modul wurde im Konstruktor geladen ? Buttons basierend auf Rolle + Status setzen
                UpdateButtonStates(_loadedModulVersion);
                System.Diagnostics.Debug.WriteLine("? Loaded-Event: UpdateButtonStates() mit geladenen Daten aufgerufen");
            }
            else
            {
                // Keine Modul-Daten ? Buttons initial deaktivieren
                UpdateButtonStatesInitial();
                System.Diagnostics.Debug.WriteLine("? Loaded-Event: UpdateButtonStatesInitial() aufgerufen");
            }
        }

        // Konstruktor mit ModulID (von StartPage/MainWindow)
        public ModulView(int modulId) : this()
        {
            _currentModulId = modulId.ToString();
            var versionen = ModulRepository.getAllModulVersionen(modulId);
            if (versionen == null || versionen.Count == 0) return;

            // Versionen-Dropdown füllen (mit "K" für kommentierte Versionen)
            var versionDisplay = versionen.Select(v =>
            {
                string displayVersion = FormatVersionsnummer(v.Versionsnummer);
                return v.hatKommentar ? $"{displayVersion}K" : displayVersion;
            });
            UpdateVersions(versionDisplay);
            
            // DEBUG: Versionsnummern-Ausgabe
            System.Diagnostics.Debug.WriteLine($"Versionen für Modul {modulId}:");
            foreach (var v in versionen)
            {
                System.Diagnostics.Debug.WriteLine($"  - Versionsnummer DB: {v.Versionsnummer}, Anzeige: {FormatVersionsnummer(v.Versionsnummer)}");
            }

            // Neueste Version laden
            var neuesteVersion = versionen
                .OrderByDescending(v => v.Versionsnummer)
                .FirstOrDefault();

            if (neuesteVersion != null)
                LoadModuleVersion(FormatVersionsnummer(neuesteVersion.Versionsnummer));
        }

        // Hilfsmethode: Konvertiere interne Versionsnummer zu Anzeige-Format (10 ? "1.0")
        private string FormatVersionsnummer(int versionsnummer)
        {
            decimal version = versionsnummer / 10.0m;
            return version.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
        }

        private void LoadModuleVersion(string versionNummer)
        {
            // Problem 3 Fix: Spezifische Version laden statt nur neueste!
            int modulId = int.Parse(_currentModulId);
            int versionsnummerInt = ParseVersionsnummer(versionNummer);
            
            using (var db = new Services.DatabaseContext())
            {
                var data = db.ModulVersion
                    .Include("Modul")
                    .FirstOrDefault(v => v.ModulId == modulId && v.Versionsnummer == versionsnummerInt);
                    
                if (data == null)
                    return;

                _currentVersion = versionNummer; // Aktuell geladene Version merken (z.B. "1.0")
                _loadedModulVersion = data;  // ? Daten zwischenspeichern

                // Problem 2 Fix: Version-Feld zeigt "K" bei kommentierten Versionen
                bool hasComments = data.hatKommentar;
                string versionDisplay = hasComments ? $"{versionNummer}K" : versionNummer;

                // Textfelder befüllen
                TitelTextBox.Text = data.Modul.ModulnameDE;
                VersionTextBox.Text = versionDisplay;
                StudiengangTextBox.Text = data.Modul.Studiengang;
                EctsTextBox.Text = data.EctsPunkte.ToString();
                WorkloadPraesenzTextBox.Text = data.WorkloadPraesenz.ToString();
                WorkloadSelbststudiumTextBox.Text = data.WorkloadSelbststudium.ToString();
                VerantwortlicherTextBox.Text = data.Ersteller;

                // Listen zu Strings zusammenfügen
                VoraussetzungenTextBox.Text = data.Modul.Voraussetzungen != null
                    ? string.Join(Environment.NewLine, data.Modul.Voraussetzungen)
                    : string.Empty;
                LernzieleTextBox.Text = data.Lernergebnisse != null
                    ? string.Join(Environment.NewLine, data.Lernergebnisse)
                    : string.Empty;
                LehrinhalteTextBox.Text = data.Inhaltsgliederung != null
                    ? string.Join(Environment.NewLine, data.Inhaltsgliederung)
                    : string.Empty;
                LiteraturTextBox.Text = data.Literatur != null
                    ? string.Join(Environment.NewLine, data.Literatur)
                    : string.Empty;

                // ListBoxen korrekt befüllen
                SelectListBoxItems(ModultypListBox, new List<string> { ConvertModultypEnumToUIString(data.Modul.Modultyp) });
                SelectListBoxItems(SemesterListBox, new List<string> { data.Modul.EmpfohlenesSemester.ToString() });
                SelectListBoxItems(PruefungsformListBox, new List<string> { data.Pruefungsform });
                SelectListBoxItems(TurnusListBox, new List<string> { ConvertTurnusEnumToUIString(data.Modul.Turnus) });
                
                // DEBUG: Ausgabe zur Kontrolle
                System.Diagnostics.Debug.WriteLine($"Lade Modul {data.Modul.ModulnameDE}:");
                System.Diagnostics.Debug.WriteLine($"  Modultyp: {data.Modul.Modultyp} -> UI: {ConvertModultypEnumToUIString(data.Modul.Modultyp)}");
                System.Diagnostics.Debug.WriteLine($"  Turnus: {data.Modul.Turnus} -> UI: {ConvertTurnusEnumToUIString(data.Modul.Turnus)}");
                System.Diagnostics.Debug.WriteLine($"  Prüfungsform: {data.Pruefungsform}");
                System.Diagnostics.Debug.WriteLine($"  Status: {data.ModulStatus}");
                
                // STATUS-BADGE aktualisieren
                UpdateStatusBadge(data.ModulStatus);
                
                // ? BUTTON-STEUERUNG: Wenn Page bereits geladen ist (z.B. nach Einreichen), sofort Buttons updaten
                // Ansonsten wird UpdateButtonStates() im Loaded-Event aufgerufen
                if (_isModuleLoaded)
                {
                    // Page ist bereits geladen ? Buttons sofort updaten (z.B. nach Statusänderung)
                    UpdateButtonStates(data);
                    System.Diagnostics.Debug.WriteLine("? LoadModuleVersion: UpdateButtonStates() direkt aufgerufen (Page bereits geladen)");
                }
                else
                {
                    // Page wird gerade geladen ? Buttons werden im Loaded-Event gesetzt
                    System.Diagnostics.Debug.WriteLine("?? LoadModuleVersion: UpdateButtonStates() wird im Loaded-Event aufgerufen");
                }
            }

            _isModuleLoaded = true;  // Modul wurde erfolgreich geladen
        }

        // Methode so anpassen, dass sie auch null akzeptiert und flexibles Matching verwendet
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
                        System.Diagnostics.Debug.WriteLine($"  ListBox '{listBox.Name}': Selektiere '{itemText}' (Exaktes Match: '{firstItemToSelect}')");
                        return;
                    }
                    
                    // Fallback: Teil-Übereinstimmung
                    if (itemText.Contains(firstItemToSelect) || firstItemToSelect.Contains(itemText))
                    {
                        listBox.SelectedItem = lbi;
                        System.Diagnostics.Debug.WriteLine($"  ListBox '{listBox.Name}': Selektiere '{itemText}' (Teil-Match: '{firstItemToSelect}')");
                        return;
                    }
                }
            }
            
            System.Diagnostics.Debug.WriteLine($"?? Kein Match für '{firstItemToSelect}' in {listBox.Name} gefunden");
            listBox.SelectedItem = null;
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
            var details = ModulRepository.getModulVersion(int.Parse(_currentModulId));
            string gewaehlterModultyp = details.Modul.Modultyp.ToString(); // Enum-Wert
            string turnus = details.Modul.Turnus.ToString();

            // Direkt an die Methode übergeben (sofern ErstellePDF den Enum-Typ erwartet)
            PDFService.ErstellePDF(details);

            MessageBox.Show($"Modulversion {version} wurde im Download-Ordner hinterlegt.",
                "Export abgeschlossen", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ModulversionBearbeiten_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentVersion) || string.IsNullOrEmpty(_currentModulId))
            {
                MessageBox.Show("Bitte wählen Sie zuerst eine Version aus.", "Keine Version", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Daten aus der Datenbank laden
            var dbVersion = ModulRepository.getModulVersion(int.Parse(_currentModulId));
            
            if (dbVersion == null)
            {
                MessageBox.Show("Fehler beim Laden der Modulversion aus der Datenbank.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Daten in ModuleData-Format konvertieren
            var moduleData = new ModuleDataRepository.ModuleData
            {
                Titel = dbVersion.Modul.ModulnameDE,
                Modultypen = new List<string> { ConvertModultypEnumToUIString(dbVersion.Modul.Modultyp) },  // ? FIX: UI-String statt Enum
                Studiengang = dbVersion.Modul.Studiengang,
                Semester = new List<string> { dbVersion.Modul.EmpfohlenesSemester.ToString() },
                Pruefungsformen = new List<string> { dbVersion.Pruefungsform },
                Turnus = new List<string> { ConvertTurnusEnumToUIString(dbVersion.Modul.Turnus) },  // ? FIX: UI-String statt Enum
                Ects = dbVersion.EctsPunkte,
                WorkloadPraesenz = dbVersion.WorkloadPraesenz,
                WorkloadSelbststudium = dbVersion.WorkloadSelbststudium,
                Verantwortlicher = dbVersion.Ersteller,
                Voraussetzungen = dbVersion.Modul.Voraussetzungen != null
                    ? string.Join(Environment.NewLine, dbVersion.Modul.Voraussetzungen)
                    : string.Empty,
                Lernziele = dbVersion.Lernergebnisse != null
                    ? string.Join(Environment.NewLine, dbVersion.Lernergebnisse)
                    : string.Empty,
                Lehrinhalte = dbVersion.Inhaltsgliederung != null
                    ? string.Join(Environment.NewLine, dbVersion.Inhaltsgliederung)
                    : string.Empty,
                Literatur = dbVersion.Literatur != null
                    ? string.Join(Environment.NewLine, dbVersion.Literatur)
                    : string.Empty
            };

            // ? Problem 2 Fix: Navigation basierend auf hatKommentar-DB-Feld
            bool hasComments = dbVersion.hatKommentar;
            
            if (hasComments)
            {
                // Kommentare aus der Datenbank laden
                var kommentare = Kommentar.getKommentareFuerVersion(int.Parse(_currentModulId), dbVersion.ModulVersionID);
                
                // CommentData erstellen (auch wenn Liste leer ist!)
                var commentData = new ModuleDataRepository.CommentData
                {
                    FieldComments = new List<ModuleDataRepository.FieldComment>(),
                    SubmittedDate = DateTime.Now,
                    SubmittedBy = "Unbekannt"
                };
                
                if (kommentare != null && kommentare.Count > 0)
                {
                    // Kommentare in ModuleDataRepository.CommentData Format konvertieren
                    commentData.FieldComments = kommentare.Select(k => new ModuleDataRepository.FieldComment
                    {
                        FieldName = k.FeldName ?? "Allgemein",
                        Comment = k.Text,
                        CommentDate = k.ErstellungsDatum ?? DateTime.Now,
                        Commenter = k.Ersteller ?? "Unbekannt"
                    }).ToList();
                    
                    commentData.SubmittedDate = kommentare.First().ErstellungsDatum ?? DateTime.Now;
                    commentData.SubmittedBy = kommentare.First().Ersteller ?? "Unbekannt";
                }

                // ? EditWithCommentsView öffnen (auch wenn Liste leer - basierend auf hatKommentar!)
                this.NavigationService?.Navigate(
                    new EditWithCommentsView(_currentModulId, _currentVersion, moduleData, commentData)
                );
            }
            else
            {
                // Version ohne Kommentare ? Normale EditingView
                this.NavigationService?.Navigate(
                    new EditingView(_currentModulId, _currentVersion, moduleData)
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

            try
            {
                int modulId = int.Parse(_currentModulId);
                string cleanVersion = _currentVersion.TrimEnd('K');
                int versionsnummer = ParseVersionsnummer(cleanVersion);

                using (var db = new Services.DatabaseContext())
                {
                    // Anzahl der Versionen für dieses Modul zählen
                    var anzahlVersionen = db.ModulVersion.Count(v => v.ModulId == modulId);

                    if (anzahlVersionen <= 1)
                    {
                        // Letzte Version ? Ganzes Modul löschen
                        var result = MessageBox.Show(
                            $"Dies ist die letzte Version des Moduls.\n\nMöchten Sie das gesamte Modul endgültig aus der Datenbank löschen?",
                            "Modul endgültig löschen?",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Warning);

                        if (result != MessageBoxResult.Yes)
                            return;

                        // Modul mit allen zugehörigen Daten löschen
                        var modul = db.Modul.Include("ModulVersionen").FirstOrDefault(m => m.ModulID == modulId);
                        if (modul != null)
                        {
                            // Erst alle Kommentare zu allen Versionen löschen
                            var kommentare = db.Kommentar.Where(k => k.GehoertZuModulID == modulId).ToList();
                            db.Kommentar.RemoveRange(kommentare);

                            // Dann alle Versionen löschen
                            db.ModulVersion.RemoveRange(modul.ModulVersionen);

                            // Schließlich das Modul selbst
                            db.Modul.Remove(modul);
                            db.SaveChanges();

                            MessageBox.Show($"Das Modul wurde vollständig gelöscht.",
                                "Modul gelöscht", MessageBoxButton.OK, MessageBoxImage.Information);

                            // Zur Startseite navigieren
                            this.NavigationService?.Navigate(new StartPage());
                        }
                    }
                    else
                    {
                        // Mehrere Versionen vorhanden ? Nur die ausgewählte Version löschen
                        var result = MessageBox.Show(
                            $"Möchten Sie die Version {_currentVersion} wirklich endgültig löschen?",
                            "Version löschen?",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                        if (result != MessageBoxResult.Yes)
                            return;

                        var version = db.ModulVersion.FirstOrDefault(v => v.ModulId == modulId && v.Versionsnummer == versionsnummer);
                        if (version != null)
                        {
                            // Erst alle Kommentare zu dieser Version löschen
                            var kommentare = db.Kommentar.Where(k => k.GehoertZuModulVersionID == version.ModulVersionID).ToList();
                            db.Kommentar.RemoveRange(kommentare);

                            // Dann die Version löschen
                            db.ModulVersion.Remove(version);
                            db.SaveChanges();

                            MessageBox.Show($"Version {_currentVersion} wurde gelöscht.",
                                "Version gelöscht", MessageBoxButton.OK, MessageBoxImage.Information);

                            // ? Problem 3 Fix: Versions-Dropdown aktualisieren!
                            var verbleibendeVersionen = db.ModulVersion
                                .Where(v => v.ModulId == modulId)
                                .OrderByDescending(v => v.Versionsnummer)
                                .ToList();
                            
                            var versionDisplay = verbleibendeVersionen.Select(v =>
                            {
                                string displayVersion = FormatVersionsnummer(v.Versionsnummer);
                                return v.hatKommentar ? $"{displayVersion}K" : displayVersion;
                            });
                            UpdateVersions(versionDisplay);
                            
                            // Neueste verbleibende Version laden
                            if (verbleibendeVersionen.Any())
                            {
                                var neuesteVersion = verbleibendeVersionen.First();
                                LoadModuleVersion(FormatVersionsnummer(neuesteVersion.Versionsnummer));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Löschen: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int ParseVersionsnummer(string version)
        {
            if (decimal.TryParse(version, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out decimal dec))
                return (int)(dec * 10);
            return 10;
        }

        // Hilfsmethoden: Enum zu UI-String Konvertierung
        private string ConvertModultypEnumToUIString(Modul.ModultypEnum modultyp)
        {
            switch (modultyp)
            {
                case Modul.ModultypEnum.Wahlpflicht:
                    return "Wahlpflichtmodul";
                case Modul.ModultypEnum.Grundlagen:
                    return "Grundlagenmodul (Pflichtmodul)";
                default:
                    return modultyp.ToString();
            }
        }

        private string ConvertTurnusEnumToUIString(Modul.TurnusEnum turnus)
        {
            switch (turnus)
            {
                case Modul.TurnusEnum.JedesSemester:
                    return "Halbjährlich (Jedes Semester)";
                case Modul.TurnusEnum.NurWintersemester:
                    return "Jährlich (WiSe)";
                case Modul.TurnusEnum.NurSommersemester:
                    return "Jährlich (SoSe)";
                default:
                    return turnus.ToString();
            }
        }

        /// <summary>
        /// Aktualisiert das Status-Badge mit Farbe, Icon und Text basierend auf dem Modul-Status
        /// </summary>
        private void UpdateStatusBadge(ModulVersion.Status status)
        {
            var statusBadge = this.FindName("StatusBadge") as System.Windows.Controls.Border;
            var statusIcon = this.FindName("StatusIcon") as TextBlock;
            var statusText = this.FindName("StatusText") as TextBlock;

            if (statusBadge == null || statusIcon == null || statusText == null)
                return;

            // Badge sichtbar machen
            statusBadge.Visibility = Visibility.Visible;

            // Farben und Text je nach Status
            switch (status)
            {
                case ModulVersion.Status.Entwurf:
                    statusBadge.Background = new SolidColorBrush(Color.FromRgb(189, 189, 189)); // Grau
                    statusIcon.Foreground = new SolidColorBrush(Colors.White);
                    statusText.Foreground = new SolidColorBrush(Colors.White);
                    statusIcon.Text = "?"; // Stift-Symbol
                    statusText.Text = "Entwurf";
                    break;

                case ModulVersion.Status.InPruefungKoordination:
                    statusBadge.Background = new SolidColorBrush(Color.FromRgb(255, 152, 0)); // Orange
                    statusIcon.Foreground = new SolidColorBrush(Colors.White);
                    statusText.Foreground = new SolidColorBrush(Colors.White);
                    statusIcon.Text = "?"; // Sanduhr
                    statusText.Text = "In Prüfung";
                    break;

                case ModulVersion.Status.InPruefungGremium:
                    statusBadge.Background = new SolidColorBrush(Color.FromRgb(255, 193, 7)); // Gelb/Gold
                    statusIcon.Foreground = new SolidColorBrush(Colors.Black);
                    statusText.Foreground = new SolidColorBrush(Colors.Black);
                    statusIcon.Text = "?"; // Waage-Symbol
                    statusText.Text = "Gremium";
                    break;

                case ModulVersion.Status.Aenderungsbedarf:
                    statusBadge.Background = new SolidColorBrush(Color.FromRgb(244, 67, 54)); // Rot
                    statusIcon.Foreground = new SolidColorBrush(Colors.White);
                    statusText.Foreground = new SolidColorBrush(Colors.White);
                    statusIcon.Text = "?"; // Warnung
                    statusText.Text = "Änderung";
                    break;

                case ModulVersion.Status.Freigegeben:
                    statusBadge.Background = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Grün
                    statusIcon.Foreground = new SolidColorBrush(Colors.White);
                    statusText.Foreground = new SolidColorBrush(Colors.White);
                    statusIcon.Text = "?"; // Haken
                    statusText.Text = "Freigegeben";
                    break;

                case ModulVersion.Status.Archiviert:
                    statusBadge.Background = new SolidColorBrush(Color.FromRgb(158, 158, 158)); // Dunkelgrau
                    statusIcon.Foreground = new SolidColorBrush(Colors.White);
                    statusText.Foreground = new SolidColorBrush(Colors.White);
                    statusIcon.Text = "??"; // Archiv-Box
                    statusText.Text = "Archiviert";
                    break;

                default:
                    statusBadge.Visibility = Visibility.Collapsed;
                    break;
            }

            System.Diagnostics.Debug.WriteLine($"Status-Badge aktualisiert: {status} -> {statusText.Text}");
        }

        /// <summary>
        /// Aktualisiert die Button-Zustände (Enabled/Disabled) basierend auf Benutzerrolle und Modul-Status
        /// </summary>
        private void UpdateButtonStates(ModulVersion data)
        {
            // Hole aktuelle Benutzer-Informationen
            string currentUser = Benutzer.CurrentUser?.Name;
            string rolle = Benutzer.CurrentUser?.RollenName ?? "Gast";
            bool isAdmin = rolle == "Admin";
            bool isKoordination = rolle == "Koordination";
            bool isGremium = rolle == "Gremium";
            bool isDozent = rolle == "Dozent";
            bool isGast = rolle == "Gast";
            
            // ? ROBUSTERE ERSTELLER-PRÜFUNG (case-insensitive + Null-Check)
            bool isErsteller = !string.IsNullOrEmpty(data.Ersteller) && 
                               !string.IsNullOrEmpty(currentUser) &&
                               data.Ersteller.Equals(currentUser, StringComparison.OrdinalIgnoreCase);
            
            var status = data.ModulStatus;

            // Finde alle Buttons (mit Null-Check)
            var exportButton = FindButtonInVisualTree("Exportieren");
            var bearbeitenButton = FindButtonInVisualTree("Bearbeiten");
            var loeschenButton = FindButtonInVisualTree("Löschen");
            var kommentierenButton = FindButtonInVisualTree("Kommentieren");
            var einreichenButton = FindButtonInVisualTree("Einreichen");

            System.Diagnostics.Debug.WriteLine($"UpdateButtonStates: Rolle={rolle}, Status={status}, Ersteller={data.Ersteller}, CurrentUser={currentUser}, isErsteller={isErsteller}");

            // ? GAST: IMMER ALLE BUTTONS DEAKTIVIEREN (außer Exportieren)
            if (isGast)
            {
                if (exportButton != null) exportButton.IsEnabled = true;
                if (bearbeitenButton != null) { bearbeitenButton.IsEnabled = false; bearbeitenButton.ToolTip = "Keine Berechtigung"; }
                if (loeschenButton != null) { loeschenButton.IsEnabled = false; loeschenButton.ToolTip = "Keine Berechtigung"; }
                if (kommentierenButton != null) { kommentierenButton.IsEnabled = false; kommentierenButton.ToolTip = "Keine Berechtigung"; }
                if (einreichenButton != null) { einreichenButton.IsEnabled = false; einreichenButton.ToolTip = "Keine Berechtigung"; }
                System.Diagnostics.Debug.WriteLine("GAST: Alle Buttons deaktiviert (außer Exportieren)");
                return;
            }

            // ? DOZENT: KOMMENTIEREN IMMER DEAKTIVIERT
            if (isDozent && kommentierenButton != null)
            {
                kommentierenButton.IsEnabled = false;
                kommentierenButton.ToolTip = "Dozenten dürfen nicht kommentieren";
            }

            // ? KOORDINATION: BEARBEITEN & LÖSCHEN IMMER DEAKTIVIERT
            if (isKoordination)
            {
                if (bearbeitenButton != null) { bearbeitenButton.IsEnabled = false; bearbeitenButton.ToolTip = "Koordination darf nicht bearbeiten"; }
                if (loeschenButton != null) { loeschenButton.IsEnabled = false; loeschenButton.ToolTip = "Koordination darf nicht löschen"; }
            }

            // ? GREMIUM: BEARBEITEN & LÖSCHEN IMMER DEAKTIVIERT
            if (isGremium)
            {
                if (bearbeitenButton != null) { bearbeitenButton.IsEnabled = false; bearbeitenButton.ToolTip = "Gremium darf nicht bearbeiten"; }
                if (loeschenButton != null) { loeschenButton.IsEnabled = false; loeschenButton.ToolTip = "Gremium darf nicht löschen"; }
            }

            // EXPORTIEREN: Immer aktiv
            if (exportButton != null) exportButton.IsEnabled = true;

            // STATUS-ABHÄNGIGE LOGIK
            switch (status)
            {
                case ModulVersion.Status.Entwurf:
                case ModulVersion.Status.Aenderungsbedarf:
                    // ? BEARBEITEN: Dozent/Admin, wenn Ersteller ODER Admin
                    if (bearbeitenButton != null)
                    {
                        if (isKoordination || isGremium)
                        {
                            // Bereits durch Rollen-Check deaktiviert
                            bearbeitenButton.IsEnabled = false;
                        }
                        else
                        {
                            bearbeitenButton.IsEnabled = isErsteller || isAdmin;
                            if (!bearbeitenButton.IsEnabled)
                                bearbeitenButton.ToolTip = "Nur der Ersteller oder Admin können eigene Module im Entwurf bearbeiten";
                            
                            System.Diagnostics.Debug.WriteLine($"ENTWURF: Bearbeiten={bearbeitenButton.IsEnabled} (isErsteller={isErsteller}, isAdmin={isAdmin})");
                        }
                    }
                    
                    // ? LÖSCHEN: Dozent/Admin, wenn Ersteller ODER Admin
                    if (loeschenButton != null)
                    {
                        if (isKoordination || isGremium)
                        {
                            // Bereits durch Rollen-Check deaktiviert
                            loeschenButton.IsEnabled = false;
                        }
                        else
                        {
                            loeschenButton.IsEnabled = isErsteller || isAdmin;
                            if (!loeschenButton.IsEnabled)
                                loeschenButton.ToolTip = "Nur der Ersteller oder Admin können eigene Module im Entwurf löschen";
                        }
                    }
                    
                    // ? EINREICHEN: Dozent/Admin, wenn Ersteller ODER Admin
                    if (einreichenButton != null)
                    {
                        einreichenButton.IsEnabled = isErsteller || isAdmin;
                        if (!einreichenButton.IsEnabled)
                            einreichenButton.ToolTip = "Nur der Ersteller oder Admin können Module einreichen";
                        
                        System.Diagnostics.Debug.WriteLine($"ENTWURF: Einreichen={einreichenButton.IsEnabled} (isErsteller={isErsteller}, isAdmin={isAdmin})");
                    }
                    
                    // ? KOMMENTIEREN: Im Entwurf deaktiviert
                    if (kommentierenButton != null && !isDozent)
                        kommentierenButton.IsEnabled = false;
                    break;

                case ModulVersion.Status.InPruefungKoordination:
                case ModulVersion.Status.InPruefungGremium:
                    // ? IN PRÜFUNG: Nur Admin darf bearbeiten, alle anderen nicht
                    if (bearbeitenButton != null)
                    {
                        bearbeitenButton.IsEnabled = isAdmin;
                        if (!isAdmin)
                            bearbeitenButton.ToolTip = "Während der Prüfung kann das Modul nur vom Admin bearbeitet werden";
                    }
                    if (loeschenButton != null)
                    {
                        loeschenButton.IsEnabled = isAdmin;  // Nur Admin darf löschen
                        if (!isAdmin)
                            loeschenButton.ToolTip = "Während der Prüfung kann das Modul nur vom Admin gelöscht werden";
                    }
                    
                    // Koordination/Gremium dürfen einreichen & kommentieren (je nach Status)
                    if (status == ModulVersion.Status.InPruefungKoordination)
                    {
                        if (einreichenButton != null)
                            einreichenButton.IsEnabled = isKoordination || isAdmin;
                        if (kommentierenButton != null && !isDozent)
                            kommentierenButton.IsEnabled = isKoordination || isAdmin;
                    }
                    else if (status == ModulVersion.Status.InPruefungGremium)
                    {
                        if (einreichenButton != null)
                            einreichenButton.IsEnabled = isGremium || isAdmin;
                        if (kommentierenButton != null && !isDozent)
                            kommentierenButton.IsEnabled = isGremium || isAdmin;
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"IN_PRÜFUNG: Bearbeiten={isAdmin}, Löschen={isAdmin}, Kommentieren={kommentierenButton?.IsEnabled}, Einreichen={einreichenButton?.IsEnabled}");
                    break;

                case ModulVersion.Status.Freigegeben:
                    // ? FREIGEGEBEN: NUR ADMIN darf ALLES, alle anderen nichts
                    if (bearbeitenButton != null)
                    {
                        bearbeitenButton.IsEnabled = isAdmin;
                        if (!isAdmin)
                            bearbeitenButton.ToolTip = "Freigegebene Module können nur vom Admin bearbeitet werden";
                    }
                    if (loeschenButton != null)
                    {
                        loeschenButton.IsEnabled = isAdmin;
                        if (!isAdmin)
                            loeschenButton.ToolTip = "Freigegebene Module können nur vom Admin gelöscht werden";
                    }
                    if (einreichenButton != null)
                    {
                        einreichenButton.IsEnabled = false;
                        einreichenButton.ToolTip = "Modul ist bereits freigegeben";
                    }
                    if (kommentierenButton != null)
                    {
                        kommentierenButton.IsEnabled = isAdmin;
                        if (!isAdmin)
                            kommentierenButton.ToolTip = "Freigegebene Module können nur vom Admin kommentiert werden";
                    }
                    System.Diagnostics.Debug.WriteLine($"FREIGEGEBEN: Bearbeiten={isAdmin}, Löschen={isAdmin}, Einreichen=false, Kommentieren={isAdmin}");
                    break;

                case ModulVersion.Status.Archiviert:
                    if (bearbeitenButton != null && !isKoordination && !isGremium)
                        bearbeitenButton.IsEnabled = isAdmin;
                    if (loeschenButton != null && !isKoordination && !isGremium)
                        loeschenButton.IsEnabled = isAdmin;
                    if (einreichenButton != null)
                        einreichenButton.IsEnabled = false;
                    if (kommentierenButton != null)
                        kommentierenButton.IsEnabled = false;
                    break;

                default:
                    if (bearbeitenButton != null) bearbeitenButton.IsEnabled = false;
                    if (loeschenButton != null) loeschenButton.IsEnabled = false;
                    if (einreichenButton != null) einreichenButton.IsEnabled = false;
                    if (kommentierenButton != null) kommentierenButton.IsEnabled = false;
                    break;
            }

            System.Diagnostics.Debug.WriteLine($"  Bearbeiten: {bearbeitenButton?.IsEnabled ?? false}, Löschen: {loeschenButton?.IsEnabled ?? false}, Kommentieren: {kommentierenButton?.IsEnabled ?? false}, Einreichen: {einreichenButton?.IsEnabled ?? false}");
        }

        /// <summary>
        /// Deaktiviert Buttons initial für alle User-Rollen (wird beim Page-Load aufgerufen)
        /// </summary>
        private void UpdateButtonStatesInitial()
        {
            string rolle = Benutzer.CurrentUser?.RollenName ?? "Gast";
            bool isAdmin = rolle == "Admin";
            bool isKoordination = rolle == "Koordination";
            bool isGremium = rolle == "Gremium";
            bool isDozent = rolle == "Dozent";
            bool isGast = rolle == "Gast";

            // Finde alle Buttons
            var exportButton = FindButtonInVisualTree("Exportieren");
            var bearbeitenButton = FindButtonInVisualTree("Bearbeiten");
            var loeschenButton = FindButtonInVisualTree("Löschen");
            var kommentierenButton = FindButtonInVisualTree("Kommentieren");
            var einreichenButton = FindButtonInVisualTree("Einreichen");

            System.Diagnostics.Debug.WriteLine($"UpdateButtonStatesInitial: Rolle={rolle}");

            // EXPORTIEREN: Für alle Rollen immer aktiv
            if (exportButton != null) exportButton.IsEnabled = true;

            // ? GAST: ALLE BUTTONS DEAKTIVIEREN (außer Exportieren)
            if (isGast)
            {
                if (bearbeitenButton != null) 
                { 
                    bearbeitenButton.IsEnabled = false; 
                    bearbeitenButton.ToolTip = "Keine Berechtigung"; 
                }
                if (loeschenButton != null) 
                { 
                    loeschenButton.IsEnabled = false; 
                    loeschenButton.ToolTip = "Keine Berechtigung"; 
                }
                if (kommentierenButton != null) 
                { 
                    kommentierenButton.IsEnabled = false; 
                    kommentierenButton.ToolTip = "Keine Berechtigung"; 
                }
                if (einreichenButton != null) 
                { 
                    einreichenButton.IsEnabled = false; 
                    einreichenButton.ToolTip = "Keine Berechtigung"; 
                }
                System.Diagnostics.Debug.WriteLine("GAST: Alle Buttons initial deaktiviert (außer Exportieren)");
                return;
            }

            // ? KOORDINATION: BEARBEITEN & LÖSCHEN IMMER DEAKTIVIERT
            if (isKoordination)
            {
                if (bearbeitenButton != null) 
                { 
                    bearbeitenButton.IsEnabled = false; 
                    bearbeitenButton.ToolTip = "Koordination darf nicht bearbeiten"; 
                }
                if (loeschenButton != null) 
                { 
                    loeschenButton.IsEnabled = false; 
                    loeschenButton.ToolTip = "Koordination darf nicht löschen"; 
                }
                // Kommentieren & Einreichen: Werden erst aktiv wenn Modul geladen ist
                if (kommentierenButton != null) 
                { 
                    kommentierenButton.IsEnabled = false; 
                    kommentierenButton.ToolTip = "Bitte wählen Sie zuerst ein Modul aus"; 
                }
                if (einreichenButton != null) 
                { 
                    einreichenButton.IsEnabled = false; 
                    einreichenButton.ToolTip = "Bitte wählen Sie zuerst ein Modul aus"; 
                }
                System.Diagnostics.Debug.WriteLine("KOORDINATION: Bearbeiten & Löschen deaktiviert");
                return;
            }

            // ? GREMIUM: BEARBEITEN & LÖSCHEN IMMER DEAKTIVIERT
            if (isGremium)
            {
                if (bearbeitenButton != null) 
                { 
                    bearbeitenButton.IsEnabled = false; 
                    bearbeitenButton.ToolTip = "Gremium darf nicht bearbeiten"; 
                }
                if (loeschenButton != null) 
                { 
                    loeschenButton.IsEnabled = false; 
                    loeschenButton.ToolTip = "Gremium darf nicht löschen"; 
                }
                // Kommentieren & Einreichen: Werden erst aktiv wenn Modul geladen ist
                if (kommentierenButton != null) 
                { 
                    kommentierenButton.IsEnabled = false; 
                    kommentierenButton.ToolTip = "Bitte wählen Sie zuerst ein Modul aus"; 
                }
                if (einreichenButton != null) 
                { 
                    einreichenButton.IsEnabled = false; 
                    einreichenButton.ToolTip = "Bitte wählen Sie zuerst ein Modul aus"; 
                }
                System.Diagnostics.Debug.WriteLine("GREMIUM: Bearbeiten & Löschen deaktiviert");
                return;
            }

            // ? DOZENT: KOMMENTIEREN IMMER DEAKTIVIERT
            if (isDozent)
            {
                if (kommentierenButton != null) 
                { 
                    kommentierenButton.IsEnabled = false; 
                    kommentierenButton.ToolTip = "Dozenten dürfen nicht kommentieren"; 
                }
                // Bearbeiten, Löschen & Einreichen: Werden erst aktiv wenn Modul geladen ist
                if (bearbeitenButton != null) 
                { 
                    bearbeitenButton.IsEnabled = false; 
                    bearbeitenButton.ToolTip = "Bitte wählen Sie zuerst ein Modul aus"; 
                }
                if (loeschenButton != null) 
                { 
                    loeschenButton.IsEnabled = false; 
                    loeschenButton.ToolTip = "Bitte wählen Sie zuerst ein Modul aus"; 
                }
                if (einreichenButton != null) 
                { 
                    einreichenButton.IsEnabled = false; 
                    einreichenButton.ToolTip = "Bitte wählen Sie zuerst ein Modul aus"; 
                }
                System.Diagnostics.Debug.WriteLine("DOZENT: Kommentieren deaktiviert, Rest initial deaktiviert");
                return;
            }

            // ? ADMIN: Alle Buttons initial deaktiviert bis Modul geladen ist
            if (isAdmin)
            {
                if (bearbeitenButton != null) 
                { 
                    bearbeitenButton.IsEnabled = false; 
                    bearbeitenButton.ToolTip = "Bitte wählen Sie zuerst ein Modul aus"; 
                }
                if (loeschenButton != null) 
                { 
                    loeschenButton.IsEnabled = false; 
                    loeschenButton.ToolTip = "Bitte wählen Sie zuerst ein Modul aus"; 
                }
                if (kommentierenButton != null) 
                { 
                    kommentierenButton.IsEnabled = false; 
                    kommentierenButton.ToolTip = "Bitte wählen Sie zuerst ein Modul aus"; 
                }
                if (einreichenButton != null) 
                { 
                    einreichenButton.IsEnabled = false; 
                    einreichenButton.ToolTip = "Bitte wählen Sie zuerst ein Modul aus"; 
                }
                System.Diagnostics.Debug.WriteLine("ADMIN: Alle Buttons initial deaktiviert bis Modul geladen");
                return;
            }

            // Default: Alle Buttons deaktivieren
            if (bearbeitenButton != null) bearbeitenButton.IsEnabled = false;
            if (loeschenButton != null) loeschenButton.IsEnabled = false;
            if (kommentierenButton != null) kommentierenButton.IsEnabled = false;
            if (einreichenButton != null) einreichenButton.IsEnabled = false;
        }

        /// <summary>
        /// Hilfsmethode: Findet einen Button anhand seines Content-Texts im Visual Tree
        /// </summary>
        private Button FindButtonInVisualTree(string buttonContent)
        {
            return FindVisualChildren<Button>(this)
                .FirstOrDefault(b => b.Content?.ToString() == buttonContent);
        }

        /// <summary>
        /// Hilfsmethode: Durchsucht den Visual Tree nach Elementen eines bestimmten Typs
        /// </summary>
        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) yield break;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                
                if (child is T tChild)
                    yield return tChild;

                foreach (var childOfChild in FindVisualChildren<T>(child))
                    yield return childOfChild;
            }
        }

        private void ModulversionKommentieren_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentVersion) || string.IsNullOrEmpty(_currentModulId))
            {
                MessageBox.Show("Bitte wählen Sie zuerst eine Version aus.", "Keine Version", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Daten aus der Datenbank laden
            var dbVersion = ModulRepository.getModulVersion(int.Parse(_currentModulId));
            
            if (dbVersion == null)
            {
                MessageBox.Show("Fehler beim Laden der Modulversion aus der Datenbank.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Daten in ModuleData-Format konvertieren (für CommentView)
            var commentData = new CommentView.ModuleData
            {
                Titel = dbVersion.Modul.ModulnameDE,
                Modultypen = new List<string> { ConvertModultypEnumToUIString(dbVersion.Modul.Modultyp) },  // ? FIX: UI-String statt Enum
                Studiengang = dbVersion.Modul.Studiengang,
                Semester = new List<string> { dbVersion.Modul.EmpfohlenesSemester.ToString() },
                Pruefungsformen = new List<string> { dbVersion.Pruefungsform },
                Turnus = new List<string> { ConvertTurnusEnumToUIString(dbVersion.Modul.Turnus) },  // ? FIX: UI-String statt Enum
                Ects = dbVersion.EctsPunkte,
                WorkloadPraesenz = dbVersion.WorkloadPraesenz,
                WorkloadSelbststudium = dbVersion.WorkloadSelbststudium,
                Verantwortlicher = dbVersion.Ersteller,
                Voraussetzungen = dbVersion.Modul.Voraussetzungen != null
                    ? string.Join(Environment.NewLine, dbVersion.Modul.Voraussetzungen)
                    : string.Empty,
                Lernziele = dbVersion.Lernergebnisse != null
                    ? string.Join(Environment.NewLine, dbVersion.Lernergebnisse)
                    : string.Empty,
                Lehrinhalte = dbVersion.Inhaltsgliederung != null
                    ? string.Join(Environment.NewLine, dbVersion.Inhaltsgliederung)
                    : string.Empty,
                Literatur = dbVersion.Literatur != null
                    ? string.Join(Environment.NewLine, dbVersion.Literatur)
                    : string.Empty
            };

            // Zur CommentView navigieren mit den geladenen Daten
            this.NavigationService?.Navigate(new CommentView(commentData, _currentModulId, _currentVersion));
        }

        private void ModulversionEinreichen_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentVersion) || string.IsNullOrEmpty(_currentModulId))
            {
                MessageBox.Show("Bitte wählen Sie zuerst eine Version aus.", "Keine Version", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                int modulId = int.Parse(_currentModulId);
                string cleanVersion = _currentVersion.TrimEnd('K');
                int versionsnummer = ParseVersionsnummer(cleanVersion);

                using (var db = new Services.DatabaseContext())
                {
                    var version = db.ModulVersion
                        .Include("Modul")
                        .FirstOrDefault(v => v.ModulId == modulId && v.Versionsnummer == versionsnummer);

                    if (version == null)
                    {
                        MessageBox.Show("Fehler: Modulversion nicht gefunden.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    string rolle = Benutzer.CurrentUser?.RollenName ?? "Gast";
                    string currentUser = Benutzer.CurrentUser?.Name ?? "Unbekannt";
                    var aktuellerStatus = version.ModulStatus;

                    // ? STATUS-WORKFLOW IMPLEMENTIERUNG
                    switch (rolle)
                    {
                        case "Admin":
                            // ? ADMIN: KANN ALLES (StatusübergänGE wie Koordination/Gremium/Dozent)
                            if (aktuellerStatus == ModulVersion.Status.Entwurf || 
                                aktuellerStatus == ModulVersion.Status.Aenderungsbedarf)
                            {
                                // Entwurf ? InPruefungKoordination
                                var result = MessageBox.Show(
                                    $"Möchten Sie das Modul '{version.Modul.ModulnameDE}' zur Koordination einreichen?",
                                    "Einreichung bestätigen",
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question);

                                if (result != MessageBoxResult.Yes)
                                    return;

                                WorkflowController.starteGenehmigung(version.Versionsnummer, version.ModulId);

                                MessageBox.Show(
                                    $"Das Modul '{version.Modul.ModulnameDE}' wurde erfolgreich eingereicht.\n\nStatus: In Prüfung (Koordination)",
                                    "Einreichung erfolgreich",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);

                                LoadModuleVersion(_currentVersion.TrimEnd('K'));
                            }
                            else if (aktuellerStatus == ModulVersion.Status.InPruefungKoordination)
                            {
                                // InPruefungKoordination ? InPruefungGremium
                                var result = MessageBox.Show(
                                    $"Möchten Sie das Modul '{version.Modul.ModulnameDE}' an das Gremium weiterleiten?",
                                    "Weiterleitung bestätigen",
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question);

                                if (result != MessageBoxResult.Yes)
                                    return;

                                WorkflowController.leiteWeiter(version.Versionsnummer, version.ModulId);

                                MessageBox.Show(
                                    $"Das Modul '{version.Modul.ModulnameDE}' wurde erfolgreich an das Gremium weitergeleitet.\n\nStatus: In Prüfung (Gremium)",
                                    "Weiterleitung erfolgreich",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);

                                LoadModuleVersion(_currentVersion.TrimEnd('K'));
                            }
                            else if (aktuellerStatus == ModulVersion.Status.InPruefungGremium)
                            {
                                // InPruefungGremium ? Freigegeben
                                var result = MessageBox.Show(
                                    $"Möchten Sie das Modul '{version.Modul.ModulnameDE}' freigeben?",
                                    "Freigabe bestätigen",
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question);

                                if (result != MessageBoxResult.Yes)
                                    return;

                                WorkflowController.schliesseGenehmigungAb(version.Versionsnummer, version.ModulId);

                                MessageBox.Show(
                                    $"Das Modul '{version.Modul.ModulnameDE}' wurde erfolgreich freigegeben.\n\nStatus: Freigegeben",
                                    "Freigabe erfolgreich",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);

                                LoadModuleVersion(_currentVersion.TrimEnd('K'));
                            }
                            else
                            {
                                MessageBox.Show(
                                    $"Admin kann aus Status '{aktuellerStatus}' nicht weiter einreichen.",
                                    "Hinweis",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                            }
                            break;

                        case "Dozent":
                            // Dozent: Entwurf ? InPruefungKoordination
                            if (aktuellerStatus == ModulVersion.Status.Entwurf || 
                                aktuellerStatus == ModulVersion.Status.Aenderungsbedarf)
                            {
                                var result = MessageBox.Show(
                                    $"Möchten Sie das Modul '{version.Modul.ModulnameDE}' wirklich zur Koordination einreichen?\n\n" +
                                    $"Das Modul wird zur Prüfung weitergeleitet und Sie können es nicht mehr bearbeiten, bis es freigegeben oder kommentiert wird.",
                                    "Einreichung bestätigen",
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question);

                                if (result != MessageBoxResult.Yes)
                                    return;

                                // Status ändern
                                WorkflowController.starteGenehmigung(version.Versionsnummer, version.ModulId);

                                MessageBox.Show(
                                    $"Das Modul '{version.Modul.ModulnameDE}' wurde erfolgreich eingereicht und an die Koordination weitergegeben.\n\n" +
                                    $"Status: In Prüfung (Koordination)",
                                    "Einreichung erfolgreich",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);

                                // Seite neu laden
                                LoadModuleVersion(_currentVersion.TrimEnd('K'));
                            }
                            else
                            {
                                MessageBox.Show(
                                    "Nur Module mit Status 'Entwurf' oder 'Änderungsbedarf' können eingereicht werden.",
                                    "Ungültiger Status",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                            }
                            break;

                        case "Koordination":
                            // Koordination: InPruefungKoordination ? InPruefungGremium
                            if (aktuellerStatus == ModulVersion.Status.InPruefungKoordination)
                            {
                                var result = MessageBox.Show(
                                    $"Möchten Sie das Modul '{version.Modul.ModulnameDE}' wirklich an das Gremium weiterleiten?\n\n" +
                                    $"Das Modul wird zur finalen Genehmigung an das Gremium weitergeleitet.",
                                    "Weiterleitung bestätigen",
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question);

                                if (result != MessageBoxResult.Yes)
                                    return;

                                // Status ändern
                                WorkflowController.leiteWeiter(version.Versionsnummer, version.ModulId);

                                MessageBox.Show(
                                    $"Das Modul '{version.Modul.ModulnameDE}' wurde erfolgreich an das Gremium weitergeleitet.\n\n" +
                                    $"Status: In Prüfung (Gremium)",
                                    "Weiterleitung erfolgreich",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);

                                // Seite neu laden
                                LoadModuleVersion(_currentVersion.TrimEnd('K'));
                            }
                            else
                            {
                                MessageBox.Show(
                                    "Nur Module mit Status 'In Prüfung (Koordination)' können ans Gremium weitergeleitet werden.",
                                    "Ungültiger Status",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                            }
                            break;

                        case "Gremium":
                            // Gremium: InPruefungGremium ? Freigegeben
                            if (aktuellerStatus == ModulVersion.Status.InPruefungGremium)
                            {
                                var result = MessageBox.Show(
                                    $"Möchten Sie das Modul '{version.Modul.ModulnameDE}' wirklich freigeben?\n\n" +
                                    $"Das Modul wird offiziell veröffentlicht und kann von allen Benutzern eingesehen werden.",
                                    "Freigabe bestätigen",
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question);

                                if (result != MessageBoxResult.Yes)
                                    return;

                                // Status ändern
                                WorkflowController.schliesseGenehmigungAb(version.Versionsnummer, version.ModulId);

                                MessageBox.Show(
                                    $"Das Modul '{version.Modul.ModulnameDE}' wurde erfolgreich freigegeben und ist jetzt offiziell veröffentlicht.\n\n" +
                                    $"Status: Freigegeben",
                                    "Freigabe erfolgreich",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);

                                // Seite neu laden
                                LoadModuleVersion(_currentVersion.TrimEnd('K'));
                            }
                            else
                            {
                                MessageBox.Show(
                                    "Nur Module mit Status 'In Prüfung (Gremium)' können freigegeben werden.",
                                    "Ungültiger Status",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                            }
                            break;

                        default:
                            MessageBox.Show(
                                "Sie haben keine Berechtigung, Module einzureichen.",
                                "Keine Berechtigung",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Einreichen: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

        /// <summary>
        /// Durchsucht alle Felder (Namen + Inhalte) im Modul und scrollt zum ersten Treffer
        /// </summary>
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string suchbegriff = SearchBox.Text?.Trim().ToLower();

            // Alle Hintergrundfarben zurücksetzen
            ResetHighlights();

            if (string.IsNullOrEmpty(suchbegriff))
                return;

            // Liste aller durchsuchbaren Felder
            var searchableFields = new List<(string FeldName, Control Control)>
            {
                ("Titel", TitelTextBox),
                ("Version", VersionTextBox),
                ("Modultyp", ModultypListBox),
                ("Studiengang", StudiengangTextBox),
                ("Semester", SemesterListBox),
                ("Prüfungsform", PruefungsformListBox),
                ("Turnus", TurnusListBox),
                ("ECTS", EctsTextBox),
                ("Workload Präsenz", WorkloadPraesenzTextBox),
                ("Workload Selbststudium", WorkloadSelbststudiumTextBox),
                ("Verantwortlicher", VerantwortlicherTextBox),
                ("Voraussetzungen", VoraussetzungenTextBox),
                ("Lernziele", LernzieleTextBox),
                ("Lehrinhalte", LehrinhalteTextBox),
                ("Literatur", LiteraturTextBox)
            };

            UIElement ersterTreffer = null;
            int trefferAnzahl = 0;

            foreach (var (feldName, control) in searchableFields)
            {
                bool istTreffer = false;

                // Prüfe Feldname
                if (feldName.ToLower().Contains(suchbegriff))
                {
                    istTreffer = true;
                }
                // Prüfe Inhalt
                else if (control is TextBox textBox && !string.IsNullOrEmpty(textBox.Text))
                {
                    if (textBox.Text.ToLower().Contains(suchbegriff))
                        istTreffer = true;
                }
                else if (control is ListBox listBox)
                {
                    foreach (var item in listBox.SelectedItems)
                    {
                        if (item is ListBoxItem lbi && lbi.Content.ToString().ToLower().Contains(suchbegriff))
                        {
                            istTreffer = true;
                            break;
                        }
                    }
                }

                if (istTreffer)
                {
                    // Hervorheben
                    control.Background = new SolidColorBrush(Color.FromArgb(100, 255, 255, 0)); // Gelb transparent
                    trefferAnzahl++;

                    if (ersterTreffer == null)
                        ersterTreffer = control;
                }
            }

            // Zum ersten Treffer scrollen
            if (ersterTreffer != null && ContentScrollViewer != null)
            {
                // Cast zu FrameworkElement für BringIntoView
                if (ersterTreffer is FrameworkElement element)
                {
                    element.BringIntoView();
                    System.Diagnostics.Debug.WriteLine($"ModulView Suche '{suchbegriff}': {trefferAnzahl} Treffer");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"ModulView Suche '{suchbegriff}': Keine Treffer");
            }
        }

        /// <summary>
        /// Setzt alle Hintergrundfarben auf Standard zurück
        /// </summary>
        private void ResetHighlights()
        {
            var fieldsToReset = new List<Control>
            {
                TitelTextBox, VersionTextBox, StudiengangTextBox,
                EctsTextBox, WorkloadPraesenzTextBox, WorkloadSelbststudiumTextBox,
                VerantwortlicherTextBox, VoraussetzungenTextBox, LernzieleTextBox,
                LehrinhalteTextBox, LiteraturTextBox,
                ModultypListBox, SemesterListBox, PruefungsformListBox, TurnusListBox
            };

            foreach (var control in fieldsToReset)
            {
                if (control is TextBox)
                    control.Background = Brushes.White;
                else if (control is ListBox)
                    control.Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)); // #F5F5F5
            }
        }
    }
}
