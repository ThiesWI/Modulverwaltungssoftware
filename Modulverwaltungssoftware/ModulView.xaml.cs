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

                // ListBoxen korrekt befüllen
                SelectListBoxItems(ModultypListBox, new List<string> { data.Modul.Modultyp.ToString() });
                SelectListBoxItems(SemesterListBox, new List<string> { data.Modul.EmpfohlenesSemester.ToString() });
                SelectListBoxItems(PruefungsformListBox, new List<string> { data.Modul.PruefungsForm.ToString() });
                SelectListBoxItems(TurnusListBox, new List<string> { data.Modul.Turnus.ToString() });
            }  // ? Fehlende schließende Klammer für using-Block!
        }

        // Methode so anpassen, dass sie auch null akzeptiert
        private void SelectListBoxItems(ListBox listBox, List<string> itemsToSelect)
        {
            listBox.SelectedItems.Clear();
            if (itemsToSelect == null)
                return;
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
