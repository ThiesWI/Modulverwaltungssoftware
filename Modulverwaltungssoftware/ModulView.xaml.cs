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

        public ModulView()
        {
            InitializeComponent();
            this.DataContext = this;
            
            // ✨ SUCHFUNKTION: TextChanged Event für SearchBox
            SearchBox.TextChanged += SearchBox_TextChanged;
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
            // ? Problem 3 Fix: Spezifische Version laden statt nur neueste!
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

                // ? Problem 2 Fix: Version-Feld zeigt "K" bei kommentierten Versionen
                bool hasComments = data.hatKommentar;
                string versionDisplay = hasComments ? $"{versionNummer}K" : versionNummer;

                // Textfelder befüllen
                TitelTextBox.Text = data.Modul.ModulnameDE;
                VersionTextBox.Text = versionDisplay; // ? Mit "K" bei Kommentaren!
                StudiengangTextBox.Text = data.Modul.Studiengang;
                EctsTextBox.Text = data.EctsPunkte.ToString();
                WorkloadPraesenzTextBox.Text = data.WorkloadPraesenz.ToString();
                WorkloadSelbststudiumTextBox.Text = data.WorkloadSelbststudium.ToString();
                VerantwortlicherTextBox.Text = data.Ersteller;

                // Listen zu Strings zusammenfügen (z.B. durch Zeilenumbruch)
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

                // ListBoxen korrekt befüllen - Enum-Werte zu UI-Strings konvertieren
                // WICHTIG: Nur EINE Auswahl möglich (da DB nur einen Wert speichert)
                SelectListBoxItems(ModultypListBox, new List<string> { ConvertModultypEnumToUIString(data.Modul.Modultyp) });
                SelectListBoxItems(SemesterListBox, new List<string> { data.Modul.EmpfohlenesSemester.ToString() });
                SelectListBoxItems(PruefungsformListBox, new List<string> { data.Pruefungsform });  // Ist bereits String!
                SelectListBoxItems(TurnusListBox, new List<string> { ConvertTurnusEnumToUIString(data.Modul.Turnus) });
                
                // DEBUG: Ausgabe zur Kontrolle
                System.Diagnostics.Debug.WriteLine($"Lade Modul {data.Modul.ModulnameDE}:");
                System.Diagnostics.Debug.WriteLine($"  Modultyp: {data.Modul.Modultyp} -> UI: {ConvertModultypEnumToUIString(data.Modul.Modultyp)}");
                System.Diagnostics.Debug.WriteLine($"  Turnus: {data.Modul.Turnus} -> UI: {ConvertTurnusEnumToUIString(data.Modul.Turnus)}");
                System.Diagnostics.Debug.WriteLine($"  Prüfungsform: {data.Pruefungsform}");
                
                // ✨ STATUS-BADGE aktualisieren
                UpdateStatusBadge(data.ModulStatus);
                
                // ✨ BUTTON-STEUERUNG basierend auf Rolle und Status
                UpdateButtonStates(data);
            }  // ✅ Schließende Klammer für using-Block!
        }

        // Methode so anpassen, dass sie auch null akzeptiert und flexibles Matching verwendet
        private void SelectListBoxItems(ListBox listBox, List<string> itemsToSelect)
        {
            listBox.SelectedItems.Clear();
            if (itemsToSelect == null || itemsToSelect.Count == 0)
                return;
            
            foreach (var item in listBox.Items)
            {
                if (item is ListBoxItem lbi)
                {
                    string itemText = lbi.Content.ToString();
                    // Flexibles Matching: Prüfe ob einer der zu selektierenden Strings Teil des Items ist
                    foreach (var toSelect in itemsToSelect)
                    {
                        if (!string.IsNullOrEmpty(toSelect) && 
                            (itemText.Contains(toSelect) || toSelect.Contains(itemText)))
                        {
                            listBox.SelectedItems.Add(lbi);
                            System.Diagnostics.Debug.WriteLine($"  ListBox '{listBox.Name}': Selektiere '{itemText}' (Match: '{toSelect}')");
                            break; // Nur einmal pro Item selektieren
                        }
                    }
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
            var details = ModulRepository.getModulVersion(int.Parse(_currentModulId));
            string gewaehlterModultyp = details.Modul.Modultyp.ToString(); // Enum-Wert
            string turnus = details.Modul.Turnus.ToString();

            // Direkt an die Methode übergeben (sofern ErstellePDF den Enum-Typ erwartet)
            PDFService.ErstellePDF(details.Modul.ModulnameDE, gewaehlterModultyp, details.Modul.EmpfohlenesSemester, details.Pruefungsform, turnus, details.EctsPunkte, details.WorkloadPraesenz, details.WorkloadSelbststudium, details.Ersteller, details.Modul.Voraussetzungen.ToString(), details.Lernergebnisse, details.Inhaltsgliederung, details.Versionsnummer, details.Literatur);

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
                Modultypen = new List<string> { dbVersion.Modul.Modultyp.ToString() },
                Studiengang = dbVersion.Modul.Studiengang,
                Semester = new List<string> { dbVersion.Modul.EmpfohlenesSemester.ToString() },
                Pruefungsformen = new List<string> { dbVersion.Pruefungsform },
                Turnus = new List<string> { dbVersion.Modul.Turnus.ToString() },
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
                    statusIcon.Text = "✎"; // Stift-Symbol
                    statusText.Text = "Entwurf";
                    break;

                case ModulVersion.Status.InPruefungKoordination:
                    statusBadge.Background = new SolidColorBrush(Color.FromRgb(255, 152, 0)); // Orange
                    statusIcon.Foreground = new SolidColorBrush(Colors.White);
                    statusText.Foreground = new SolidColorBrush(Colors.White);
                    statusIcon.Text = "⏳"; // Sanduhr
                    statusText.Text = "In Prüfung";
                    break;

                case ModulVersion.Status.InPruefungGremium:
                    statusBadge.Background = new SolidColorBrush(Color.FromRgb(255, 193, 7)); // Gelb/Gold
                    statusIcon.Foreground = new SolidColorBrush(Color.FromRgb(66, 66, 66));
                    statusText.Foreground = new SolidColorBrush(Color.FromRgb(66, 66, 66));
                    statusIcon.Text = "⚖"; // Waage-Symbol
                    statusText.Text = "Gremium";
                    break;

                case ModulVersion.Status.Aenderungsbedarf:
                    statusBadge.Background = new SolidColorBrush(Color.FromRgb(244, 67, 54)); // Rot
                    statusIcon.Foreground = new SolidColorBrush(Colors.White);
                    statusText.Foreground = new SolidColorBrush(Colors.White);
                    statusIcon.Text = "⚠"; // Warnung
                    statusText.Text = "Änderung";
                    break;

                case ModulVersion.Status.Freigegeben:
                    statusBadge.Background = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Grün
                    statusIcon.Foreground = new SolidColorBrush(Colors.White);
                    statusText.Foreground = new SolidColorBrush(Colors.White);
                    statusIcon.Text = "✓"; // Haken
                    statusText.Text = "Freigegeben";
                    break;

                case ModulVersion.Status.Archiviert:
                    statusBadge.Background = new SolidColorBrush(Color.FromRgb(158, 158, 158)); // Dunkelgrau
                    statusIcon.Foreground = new SolidColorBrush(Colors.White);
                    statusText.Foreground = new SolidColorBrush(Colors.White);
                    statusIcon.Text = "📦"; // Archiv-Box
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
            bool isErsteller = data.Ersteller == currentUser;
            
            var status = data.ModulStatus;

            // Finde alle Buttons (mit Null-Check)
            var exportButton = FindButtonInVisualTree("Exportieren");
            var bearbeitenButton = FindButtonInVisualTree("Bearbeiten");
            var loeschenButton = FindButtonInVisualTree("Löschen");
            var kommentierenButton = FindButtonInVisualTree("Kommentieren");
            var einreichenButton = FindButtonInVisualTree("Einreichen");

            System.Diagnostics.Debug.WriteLine($"UpdateButtonStates: Rolle={rolle}, Status={status}, Ersteller={data.Ersteller}, CurrentUser={currentUser}");

            // ✅ GAST: IMMER ALLE BUTTONS DEAKTIVIEREN (außer Exportieren)
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

            // ✅ DOZENT: KOMMENTIEREN IMMER DEAKTIVIERT
            if (isDozent && kommentierenButton != null)
            {
                kommentierenButton.IsEnabled = false;
                kommentierenButton.ToolTip = "Dozenten dürfen nicht kommentieren";
            }

            // ✅ KOORDINATION: BEARBEITEN & LÖSCHEN IMMER DEAKTIVIERT
            if (isKoordination)
            {
                if (bearbeitenButton != null) { bearbeitenButton.IsEnabled = false; bearbeitenButton.ToolTip = "Koordination darf nicht bearbeiten"; }
                if (loeschenButton != null) { loeschenButton.IsEnabled = false; loeschenButton.ToolTip = "Koordination darf nicht löschen"; }
            }

            // ✅ GREMIUM: BEARBEITEN & LÖSCHEN IMMER DEAKTIVIERT
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
                    if (bearbeitenButton != null && !isKoordination && !isGremium)
                        bearbeitenButton.IsEnabled = isErsteller || isAdmin;
                    if (loeschenButton != null && !isKoordination && !isGremium)
                        loeschenButton.IsEnabled = isErsteller || isAdmin;
                    if (einreichenButton != null)
                        einreichenButton.IsEnabled = isErsteller || isAdmin;
                    if (kommentierenButton != null && !isDozent)
                        kommentierenButton.IsEnabled = false;
                    break;

                case ModulVersion.Status.InPruefungKoordination:
                    if (einreichenButton != null)
                        einreichenButton.IsEnabled = isKoordination || isAdmin;
                    if (kommentierenButton != null && !isDozent)
                        kommentierenButton.IsEnabled = isKoordination || isAdmin;
                    if (loeschenButton != null && !isKoordination && !isGremium)
                        loeschenButton.IsEnabled = isAdmin;
                    break;

                case ModulVersion.Status.InPruefungGremium:
                    if (einreichenButton != null)
                        einreichenButton.IsEnabled = isGremium || isAdmin;
                    if (kommentierenButton != null && !isDozent)
                        kommentierenButton.IsEnabled = isGremium || isAdmin;
                    if (loeschenButton != null && !isKoordination && !isGremium)
                        loeschenButton.IsEnabled = isAdmin;
                    break;

                case ModulVersion.Status.Freigegeben:
                    // ✅ FREIGEGEBEN: NUR ADMIN darf ALLES, alle anderen nichts
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
                Modultypen = new List<string> { dbVersion.Modul.Modultyp.ToString() },
                Studiengang = dbVersion.Modul.Studiengang,
                Semester = new List<string> { dbVersion.Modul.EmpfohlenesSemester.ToString() },
                Pruefungsformen = new List<string> { dbVersion.Pruefungsform },
                Turnus = new List<string> { dbVersion.Modul.Turnus.ToString() },
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

                    // ✨ STATUS-WORKFLOW IMPLEMENTIERUNG
                    switch (rolle)
                    {
                        case "Admin":
                            // ✅ ADMIN: KANN ALLES (StatusübergänGE wie Koordination/Gremium/Dozent)
                            if (aktuellerStatus == ModulVersion.Status.Entwurf || 
                                aktuellerStatus == ModulVersion.Status.Aenderungsbedarf)
                            {
                                // Entwurf → InPruefungKoordination
                                var result = MessageBox.Show(
                                    $"Möchten Sie das Modul '{version.Modul.ModulnameDE}' zur Koordination einreichen?",
                                    "Einreichung bestätigen",
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question);

                                if (result != MessageBoxResult.Yes)
                                    return;

                                version.ModulStatus = ModulVersion.Status.InPruefungKoordination;
                                version.LetzteAenderung = DateTime.Now;
                                db.SaveChanges();

                                BenachrichtigungsService.SendeBenachrichtigung(
                                    "Koordination",
                                    $"{currentUser} (Admin) hat das Modul '{version.Modul.ModulnameDE}' (Version {FormatVersionsnummer(version.Versionsnummer)}) zur Prüfung eingereicht.",
                                    version.ModulVersionID
                                );

                                MessageBox.Show(
                                    $"Das Modul '{version.Modul.ModulnameDE}' wurde erfolgreich eingereicht.\n\nStatus: In Prüfung (Koordination)",
                                    "Einreichung erfolgreich",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);

                                LoadModuleVersion(_currentVersion.TrimEnd('K'));
                            }
                            else if (aktuellerStatus == ModulVersion.Status.InPruefungKoordination)
                            {
                                // InPruefungKoordination → InPruefungGremium
                                var result = MessageBox.Show(
                                    $"Möchten Sie das Modul '{version.Modul.ModulnameDE}' an das Gremium weiterleiten?",
                                    "Weiterleitung bestätigen",
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question);

                                if (result != MessageBoxResult.Yes)
                                    return;

                                version.ModulStatus = ModulVersion.Status.InPruefungGremium;
                                version.LetzteAenderung = DateTime.Now;
                                db.SaveChanges();

                                BenachrichtigungsService.SendeBenachrichtigung(
                                    "Gremium",
                                    $"{currentUser} (Admin) hat das Modul '{version.Modul.ModulnameDE}' (Version {FormatVersionsnummer(version.Versionsnummer)}) zur finalen Genehmigung weitergeleitet.",
                                    version.ModulVersionID
                                );

                                MessageBox.Show(
                                    $"Das Modul '{version.Modul.ModulnameDE}' wurde erfolgreich an das Gremium weitergeleitet.\n\nStatus: In Prüfung (Gremium)",
                                    "Weiterleitung erfolgreich",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);

                                LoadModuleVersion(_currentVersion.TrimEnd('K'));
                            }
                            else if (aktuellerStatus == ModulVersion.Status.InPruefungGremium)
                            {
                                // InPruefungGremium → Freigegeben
                                var result = MessageBox.Show(
                                    $"Möchten Sie das Modul '{version.Modul.ModulnameDE}' freigeben?",
                                    "Freigabe bestätigen",
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question);

                                if (result != MessageBoxResult.Yes)
                                    return;

                                version.ModulStatus = ModulVersion.Status.Freigegeben;
                                version.LetzteAenderung = DateTime.Now;
                                db.SaveChanges();

                                BenachrichtigungsService.SendeBenachrichtigung(
                                    version.Ersteller,
                                    $"Glückwunsch! Ihr Modul '{version.Modul.ModulnameDE}' (Version {FormatVersionsnummer(version.Versionsnummer)}) wurde vom Admin freigegeben.",
                                    version.ModulVersionID
                                );

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
                            // Dozent: Entwurf → InPruefungKoordination
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
                                version.ModulStatus = ModulVersion.Status.InPruefungKoordination;
                                version.LetzteAenderung = DateTime.Now;
                                db.SaveChanges();

                                // Benachrichtigung an Koordination
                                BenachrichtigungsService.SendeBenachrichtigung(
                                    "Koordination",
                                    $"{currentUser} hat das Modul '{version.Modul.ModulnameDE}' (Version {FormatVersionsnummer(version.Versionsnummer)}) zur Prüfung eingereicht.",
                                    version.ModulVersionID
                                );

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
                            // Koordination: InPruefungKoordination → InPruefungGremium
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
                                version.ModulStatus = ModulVersion.Status.InPruefungGremium;
                                version.LetzteAenderung = DateTime.Now;
                                db.SaveChanges();

                                // Benachrichtigung an Gremium
                                BenachrichtigungsService.SendeBenachrichtigung(
                                    "Gremium",
                                    $"{currentUser} (Koordination) hat das Modul '{version.Modul.ModulnameDE}' (Version {FormatVersionsnummer(version.Versionsnummer)}) zur finalen Genehmigung weitergeleitet.",
                                    version.ModulVersionID
                                );

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
                            // Gremium: InPruefungGremium → Freigegeben
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
                                version.ModulStatus = ModulVersion.Status.Freigegeben;
                                version.LetzteAenderung = DateTime.Now;
                                db.SaveChanges();

                                // Benachrichtigung an Ersteller
                                BenachrichtigungsService.SendeBenachrichtigung(
                                    version.Ersteller,
                                    $"Glückwunsch! Ihr Modul '{version.Modul.ModulnameDE}' (Version {FormatVersionsnummer(version.Versionsnummer)}) wurde vom Gremium freigegeben und ist jetzt offiziell veröffentlicht.",
                                    version.ModulVersionID
                                );

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
