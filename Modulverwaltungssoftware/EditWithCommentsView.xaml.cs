using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Modulverwaltungssoftware
{
    /// <summary>
    /// Interaktionslogik für EditWithCommentsView.xaml
    /// </summary>
    public partial class EditWithCommentsView : Page
    {
        private string _modulId;
        private string _versionNummer;
        private ModuleDataRepository.CommentData _comments;

        public EditWithCommentsView()
        {
            InitializeComponent();
            
            // ✨ SUCHFUNKTION: TextChanged Event für SearchBox
            var searchBox = FindName("SearchBox") as TextBox;
            if (searchBox != null)
                searchBox.TextChanged += SearchBox_TextChanged;
        }

        public EditWithCommentsView(string modulId, string versionNummer, ModuleDataRepository.ModuleData moduleData, ModuleDataRepository.CommentData comments) : this()
        {
            _modulId = modulId;
            _versionNummer = versionNummer;
            _comments = comments;

            LoadModuleData(moduleData);
            DisplayComments(comments);
        }

        private void LoadModuleData(ModuleDataRepository.ModuleData data)
        {
            if (data == null) return;

            // Linke Spalte: EDITIERBARE Felder (Modulfelder werden jetzt editierbar gemacht)
            TitelTextBox.Text = data.Titel;
            TitelTextBox.IsReadOnly = false;  // EDITIERBAR machen
            TitelTextBox.Background = System.Windows.Media.Brushes.White;

            StudiengangTextBox.Text = data.Studiengang;
            StudiengangTextBox.IsReadOnly = false;  // EDITIERBAR machen
            StudiengangTextBox.Background = System.Windows.Media.Brushes.White;

            EctsTextBox.Text = data.Ects.ToString();
            EctsTextBox.IsReadOnly = false;  // EDITIERBAR machen
            EctsTextBox.Background = System.Windows.Media.Brushes.White;

            WorkloadPraesenzTextBox.Text = data.WorkloadPraesenz.ToString();
            WorkloadPraesenzTextBox.IsReadOnly = false;  // EDITIERBAR machen
            WorkloadPraesenzTextBox.Background = System.Windows.Media.Brushes.White;

            WorkloadSelbststudiumTextBox.Text = data.WorkloadSelbststudium.ToString();
            WorkloadSelbststudiumTextBox.IsReadOnly = false;  // EDITIERBAR machen
            WorkloadSelbststudiumTextBox.Background = System.Windows.Media.Brushes.White;

            VerantwortlicherTextBox.Text = data.Verantwortlicher;
            VerantwortlicherTextBox.IsReadOnly = false;  // EDITIERBAR machen
            VerantwortlicherTextBox.Background = System.Windows.Media.Brushes.White;

            VoraussetzungenTextBox.Text = data.Voraussetzungen;
            VoraussetzungenTextBox.IsReadOnly = false;  // EDITIERBAR machen
            VoraussetzungenTextBox.Background = System.Windows.Media.Brushes.White;

            LernzieleTextBox.Text = data.Lernziele;
            LernzieleTextBox.IsReadOnly = false;  // EDITIERBAR machen
            LernzieleTextBox.Background = System.Windows.Media.Brushes.White;

            LehrinhalteTextBox.Text = data.Lehrinhalte;
            LehrinhalteTextBox.IsReadOnly = false;  // EDITIERBAR machen
            LehrinhalteTextBox.Background = System.Windows.Media.Brushes.White;

            LiteraturTextBox.Text = data.Literatur;
            LiteraturTextBox.IsReadOnly = false;  // EDITIERBAR machen
            LiteraturTextBox.Background = System.Windows.Media.Brushes.White;

            VersionTextBox.Text = _versionNummer + "K";  // Mit "K" anzeigen

            // ListBoxen aktivieren und befüllen
            ModultypListBox.IsEnabled = true;  // EDITIERBAR machen
            ModultypListBox.Background = System.Windows.Media.Brushes.White;
            SelectListBoxItems(ModultypListBox, data.Modultypen);

            SemesterListBox.IsEnabled = true;  // EDITIERBAR machen
            SemesterListBox.Background = System.Windows.Media.Brushes.White;
            SelectListBoxItems(SemesterListBox, data.Semester);

            PruefungsformListBox.IsEnabled = true;  // EDITIERBAR machen
            PruefungsformListBox.Background = System.Windows.Media.Brushes.White;
            SelectListBoxItems(PruefungsformListBox, data.Pruefungsformen);

            TurnusListBox.IsEnabled = true;  // EDITIERBAR machen
            TurnusListBox.Background = System.Windows.Media.Brushes.White;
            // ✅ FIX: Turnus aus DB laden
            if (data.Turnus != null && data.Turnus.Count > 0)
            {
                SelectListBoxItems(TurnusListBox, data.Turnus);
            }
            else
            {
                // ✅ FALLBACK: Wenn Turnus aus ModuleDataRepository kommt (alte Struktur)
                // Lade aus DB
                try
                {
                    using (var db = new Services.DatabaseContext())
                    {
                        int modulId = int.Parse(_modulId);
                        var modul = db.Modul.FirstOrDefault(m => m.ModulID == modulId);
                        if (modul != null)
                        {
                            var turnusString = ConvertTurnusEnumToDisplayString(modul.Turnus);
                            SelectListBoxItems(TurnusListBox, new List<string> { turnusString });
                        }
                    }
                }
                catch
                {
                    // Ignoriere Fehler beim Laden
                }
            }
        }
        
        /// <summary>
        /// Konvertiert Turnus-Enum zu Display-String für ListBox
        /// </summary>
        private string ConvertTurnusEnumToDisplayString(Modul.TurnusEnum turnus)
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
                    return "Halbjährlich (Jedes Semester)";
            }
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

        private void DisplayComments(ModuleDataRepository.CommentData comments)
        {
            // WICHTIG: ALLE Kommentarfelder standardmäßig READ-ONLY setzen!
            SetAllCommentFieldsReadOnly();
            
            if (comments == null || comments.FieldComments == null || comments.FieldComments.Count == 0)
            {
                // Keine Kommentare vorhanden - alle Kommentarfelder bleiben leer und READ-ONLY
                return;
            }

            // Rechte Spalte: READ-ONLY Kommentare anzeigen
            // Kommentarfelder sperren und mit existierenden Kommentaren befüllen
            foreach (var fieldComment in comments.FieldComments)
            {
                TextBox kommentarBox = null;
                
                switch (fieldComment.FieldName)
                {
                    case "Titel":
                        kommentarBox = TitelKommentarTextBox;
                        break;
                    case "Modultyp":
                        kommentarBox = ModultypKommentarTextBox;
                        break;
                    case "Studiengang":
                        kommentarBox = StudiengangKommentarTextBox;
                        break;
                    case "Semester":
                        kommentarBox = SemesterKommentarTextBox;
                        break;
                    case "Prüfungsform":
                        kommentarBox = PruefungsformKommentarTextBox;
                        break;
                    case "Turnus":
                        kommentarBox = TurnusKommentarTextBox;
                        break;
                    case "ECTS":
                        kommentarBox = EctsKommentarTextBox;
                        break;
                    case "Workload Präsenz":
                        kommentarBox = WorkloadPraesenzKommentarTextBox;
                        break;
                    case "Workload Selbststudium":
                        kommentarBox = WorkloadSelbststudiumKommentarTextBox;
                        break;
                    case "Verantwortlicher":
                        kommentarBox = VerantwortlicherKommentarTextBox;
                        break;
                    case "Voraussetzungen":
                        kommentarBox = VoraussetzungenKommentarTextBox;
                        break;
                    case "Lernziele":
                        kommentarBox = LernzieleKommentarTextBox;
                        break;
                    case "Lehrinhalte":
                        kommentarBox = LehrinhalteKommentarTextBox;
                        break;
                    case "Literatur":
                        kommentarBox = LiteraturKommentarTextBox;
                        break;
                }

                if (kommentarBox != null)
                {
                    kommentarBox.Text = $"{fieldComment.Comment}\n\n— {fieldComment.Commenter}, {fieldComment.CommentDate:dd.MM.yyyy}";
                    kommentarBox.IsReadOnly = true;  // READ-ONLY
                    kommentarBox.Background = new SolidColorBrush(Color.FromRgb(255, 255, 224));  // Hellgelb
                    kommentarBox.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 165, 0));  // Orange Border
                }
            }
        }
        
        // Hilfsmethode: Alle Kommentarfelder auf READ-ONLY setzen
        private void SetAllCommentFieldsReadOnly()
        {
            var kommentarBoxen = new[] {
                TitelKommentarTextBox, ModultypKommentarTextBox, StudiengangKommentarTextBox,
                SemesterKommentarTextBox, PruefungsformKommentarTextBox, TurnusKommentarTextBox,
                EctsKommentarTextBox, WorkloadPraesenzKommentarTextBox, WorkloadSelbststudiumKommentarTextBox,
                VerantwortlicherKommentarTextBox, VoraussetzungenKommentarTextBox, LernzieleKommentarTextBox,
                LehrinhalteKommentarTextBox, LiteraturKommentarTextBox
            };
            
            foreach (var box in kommentarBoxen)
            {
                if (box != null)
                {
                    box.IsReadOnly = true;
                    box.Background = new SolidColorBrush(Color.FromRgb(245, 245, 245));  // Hellgrau
                    box.Text = string.Empty;  // Leer lassen
                }
            }
        }

        private void KommentarAbschicken_Click(object sender, RoutedEventArgs e)
        {
            // Validierung
            if (string.IsNullOrWhiteSpace(TitelTextBox.Text))
            {
                MessageBox.Show("Bitte Titel eingeben.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!int.TryParse(EctsTextBox.Text, out int ects))
            {
                MessageBox.Show("Bitte gültige ECTS-Punktzahl eingeben.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!int.TryParse(WorkloadPraesenzTextBox.Text, out int workloadPraesenz))
            {
                MessageBox.Show("Bitte gültige Workload Präsenz eingeben.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!int.TryParse(WorkloadSelbststudiumTextBox.Text, out int workloadSelbststudium))
            {
                MessageBox.Show("Bitte gültige Workload Selbststudium eingeben.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                int modulId = int.Parse(_modulId);
                
                // ✅ NEUE VERSION DIREKT IN DB ERSTELLEN (nicht über ModulRepository.Speichere!)
                using (var db = new Services.DatabaseContext())
                {
                    var modul = db.Modul.FirstOrDefault(m => m.ModulID == modulId);
                    if (modul == null)
                    {
                        MessageBox.Show("Fehler: Modul konnte nicht gefunden werden.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    
                    // ✅ Höchste bestehende Versionsnummer ermitteln
                    var hoechsteVersion = db.ModulVersion
                        .Where(v => v.ModulId == modulId)
                        .OrderByDescending(v => v.Versionsnummer)
                        .FirstOrDefault();
                    
                    int neueVersionsnummer = (hoechsteVersion?.Versionsnummer ?? 10) + 1;
                    
                    // ✅ NEUE Version erstellen
                    ModulVersion neueVersion = new ModulVersion
                    {
                        ModulId = modulId,  // ✅ NUR die ID setzen, NICHT die Navigation Property!
                        Versionsnummer = neueVersionsnummer,  // ✅ NEUE Versionsnummer!
                        EctsPunkte = ects,
                        WorkloadPraesenz = workloadPraesenz,
                        WorkloadSelbststudium = workloadSelbststudium,
                        Ersteller = VerantwortlicherTextBox.Text,
                        ModulStatus = ModulVersion.Status.Entwurf,
                        LetzteAenderung = DateTime.Now,
                        GueltigAbSemester = "Entwurf",
                        hatKommentar = false
                    };
                    
                    var pruefungsformen = GetSelectedListBoxItems(PruefungsformListBox);
                    if (pruefungsformen.Count > 0)
                        neueVersion.Pruefungsform = pruefungsformen[0];
                    else
                        neueVersion.Pruefungsform = "Klausur";

                    // Lernziele
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

                    // Lehrinhalte
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

                    // Literatur
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
                    
                    // ✅ Plausibilitätsprüfung VOR dem Speichern
                    // ⚠️ WICHTIG: Modul-Objekt aus DB neu laden für Validierung
                    neueVersion.Modul = db.Modul
                        .Include("ModulVersionen")
                        .FirstOrDefault(m => m.ModulID == modulId);
                    
                    string plausibilitaet = PlausibilitaetsService.pruefeForm(neueVersion);
                    if (plausibilitaet != "Keine Fehler gefunden.")
                    {
                        MessageBox.Show(plausibilitaet, "Validierungsfehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    
                    // ✅ Modul-Referenz wieder entfernen vor dem Speichern
                    neueVersion.Modul = null;
                    
                    // ✅ Modul-Daten aus UI übernehmen (Titel, Studiengang, etc.)
                    modul.ModulnameDE = TitelTextBox.Text;
                    modul.Studiengang = StudiengangTextBox.Text;
                    
                    // Modultyp
                    var modultypen = GetSelectedListBoxItems(ModultypListBox);
                    if (modultypen.Count > 0)
                    {
                        string ersteAuswahl = modultypen[0];
                        if (ersteAuswahl.Contains("Wahlpflicht"))
                            modul.Modultyp = Modul.ModultypEnum.Wahlpflicht;
                        else
                            modul.Modultyp = Modul.ModultypEnum.Grundlagen;
                    }
                    
                    // Turnus
                    var turnusList = GetSelectedListBoxItems(TurnusListBox);
                    if (turnusList.Count > 0)
                    {
                        string ersteAuswahl = turnusList[0];
                        if (ersteAuswahl.Contains("WiSe") || ersteAuswahl.Contains("Wintersemester"))
                            modul.Turnus = Modul.TurnusEnum.NurWintersemester;
                        else if (ersteAuswahl.Contains("SoSe") || ersteAuswahl.Contains("Sommersemester"))
                            modul.Turnus = Modul.TurnusEnum.NurSommersemester;
                        else
                            modul.Turnus = Modul.TurnusEnum.JedesSemester;
                    }
                    
                    // Semester
                    var semester = GetSelectedListBoxItems(SemesterListBox);
                    if (semester.Count > 0 && int.TryParse(semester[0], out int sem))
                    {
                        modul.EmpfohlenesSemester = sem;
                    }
                    
                    // Voraussetzungen
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
                    
                    // ✅ In DB einfügen
                    db.ModulVersion.Add(neueVersion);
                    db.SaveChanges();
                    
                    MessageBox.Show($"Änderungen wurden als neue Version {neueVersionsnummer / 10.0:0.0} gespeichert.\nDie kommentierte Version bleibt erhalten.", 
                        "Gespeichert", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Zurück zur ModulView
                    this.NavigationService?.Navigate(new ModulView(modulId));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Speichern: {ex.Message}\n\n{ex.StackTrace}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int ParseVersionsnummer(string version)
        {
            string cleanVersion = version.TrimEnd('K');
            if (decimal.TryParse(cleanVersion, System.Globalization.NumberStyles.Any, 
                System.Globalization.CultureInfo.InvariantCulture, out decimal dec))
                return (int)(dec * 10);
            return 10;
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

        private void KommentarVerwerfen_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Möchten Sie die Änderungen wirklich verwerfen?",
                "Warnung",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                // Zurück zur ModulView
                this.NavigationService?.Navigate(new ModulView(int.Parse(_modulId)));
            }
        }

        /// <summary>
        /// Durchsucht alle Felder (Namen + Inhalte + Kommentare) und scrollt zum ersten Treffer
        /// </summary>
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchBox = sender as TextBox;
            string suchbegriff = searchBox?.Text?.Trim().ToLower();

            // Alle Hintergrundfarben zurücksetzen
            ResetHighlights();

            if (string.IsNullOrEmpty(suchbegriff))
                return;

            // Liste aller durchsuchbaren Felder (Links: Editierbare Modul-Daten, Rechts: Read-Only Kommentare)
            var searchableFields = new List<(string FeldName, Control DataControl, Control CommentControl)>
            {
                ("Titel", FindName("TitelTextBox") as TextBox, FindName("TitelKommentarTextBox") as TextBox),
                ("Modultyp", FindName("ModultypListBox") as ListBox, FindName("ModultypKommentarTextBox") as TextBox),
                ("Studiengang", FindName("StudiengangTextBox") as TextBox, FindName("StudiengangKommentarTextBox") as TextBox),
                ("Semester", FindName("SemesterListBox") as ListBox, FindName("SemesterKommentarTextBox") as TextBox),
                ("Prüfungsform", FindName("PruefungsformListBox") as ListBox, FindName("PruefungsformKommentarTextBox") as TextBox),
                ("Turnus", FindName("TurnusListBox") as ListBox, FindName("TurnusKommentarTextBox") as TextBox),
                ("ECTS", FindName("EctsTextBox") as TextBox, FindName("EctsKommentarTextBox") as TextBox),
                ("Workload Präsenz", FindName("WorkloadPraesenzTextBox") as TextBox, FindName("WorkloadPraesenzKommentarTextBox") as TextBox),
                ("Workload Selbststudium", FindName("WorkloadSelbststudiumTextBox") as TextBox, FindName("WorkloadSelbststudiumKommentarTextBox") as TextBox),
                ("Verantwortlicher", FindName("VerantwortlicherTextBox") as TextBox, FindName("VerantwortlicherKommentarTextBox") as TextBox),
                ("Voraussetzungen", FindName("VoraussetzungenTextBox") as TextBox, FindName("VoraussetzungenKommentarTextBox") as TextBox),
                ("Lernziele", FindName("LernzieleTextBox") as TextBox, FindName("LernzieleKommentarTextBox") as TextBox),
                ("Lehrinhalte", FindName("LehrinhalteTextBox") as TextBox, FindName("LehrinhalteKommentarTextBox") as TextBox),
                ("Literatur", FindName("LiteraturTextBox") as TextBox, FindName("LiteraturKommentarTextBox") as TextBox)
            };

            UIElement ersterTreffer = null;
            int trefferAnzahl = 0;

            foreach (var (feldName, dataControl, commentControl) in searchableFields)
            {
                bool istTreffer = false;
                Control trefferControl = null;

                // Prüfe Feldname
                if (feldName.ToLower().Contains(suchbegriff))
                {
                    istTreffer = true;
                    trefferControl = dataControl ?? commentControl;
                }
                // Prüfe Daten-Inhalt (links - editierbar)
                else if (dataControl is TextBox dataTextBox && !string.IsNullOrEmpty(dataTextBox.Text))
                {
                    if (dataTextBox.Text.ToLower().Contains(suchbegriff))
                    {
                        istTreffer = true;
                        trefferControl = dataTextBox;
                    }
                }
                else if (dataControl is ListBox dataListBox)
                {
                    foreach (var item in dataListBox.SelectedItems)
                    {
                        if (item is ListBoxItem lbi && lbi.Content.ToString().ToLower().Contains(suchbegriff))
                        {
                            istTreffer = true;
                            trefferControl = dataListBox;
                            break;
                        }
                    }
                }

                // Prüfe Kommentar-Inhalt (rechts - read-only)
                if (!istTreffer && commentControl is TextBox commentTextBox && !string.IsNullOrEmpty(commentTextBox.Text))
                {
                    if (commentTextBox.Text.ToLower().Contains(suchbegriff))
                    {
                        istTreffer = true;
                        trefferControl = commentTextBox;
                    }
                }

                if (istTreffer && trefferControl != null)
                {
                    // Hervorheben mit gelber Markierung
                    if (trefferControl.Background is SolidColorBrush currentBrush)
                    {
                        // Speichere original Farbe für späteres Zurücksetzen
                        // Highlight: Gelb transparent über bestehender Farbe
                        trefferControl.Background = new SolidColorBrush(Color.FromArgb(150, 255, 255, 0)); // Gelb transparent
                    }
                    else
                    {
                        trefferControl.Background = new SolidColorBrush(Color.FromArgb(100, 255, 255, 0)); // Gelb transparent
                    }
                    
                    trefferAnzahl++;

                    if (ersterTreffer == null)
                        ersterTreffer = trefferControl;
                }
            }

            // Zum ersten Treffer scrollen
            if (ersterTreffer != null)
            {
                // Cast zu FrameworkElement für BringIntoView
                if (ersterTreffer is FrameworkElement element)
                {
                    element.BringIntoView();
                    System.Diagnostics.Debug.WriteLine($"EditWithCommentsView Suche '{suchbegriff}': {trefferAnzahl} Treffer");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"EditWithCommentsView Suche '{suchbegriff}': Keine Treffer");
            }
        }

        /// <summary>
        /// Setzt alle Hintergrundfarben auf Standard zurück
        /// </summary>
        private void ResetHighlights()
        {
            // Alle editierbaren TextBoxen (weiß)
            var editableTextBoxNames = new[] {
                "TitelTextBox", "StudiengangTextBox", "EctsTextBox", "WorkloadPraesenzTextBox",
                "WorkloadSelbststudiumTextBox", "VerantwortlicherTextBox", "VoraussetzungenTextBox",
                "LernzieleTextBox", "LehrinhalteTextBox", "LiteraturTextBox"
            };

            // Alle Kommentar-TextBoxen (hellgelb wenn mit Kommentar, hellgrau wenn leer)
            var commentTextBoxNames = new[] {
                "TitelKommentarTextBox", "ModultypKommentarTextBox", "StudiengangKommentarTextBox",
                "SemesterKommentarTextBox", "PruefungsformKommentarTextBox", "TurnusKommentarTextBox",
                "EctsKommentarTextBox", "WorkloadPraesenzKommentarTextBox", "WorkloadSelbststudiumKommentarTextBox",
                "VerantwortlicherKommentarTextBox", "VoraussetzungenKommentarTextBox", "LernzieleKommentarTextBox",
                "LehrinhalteKommentarTextBox", "LiteraturKommentarTextBox"
            };

            // Alle ListBoxen (weiß, da editierbar)
            var listBoxNames = new[] {
                "ModultypListBox", "SemesterListBox", "PruefungsformListBox", "TurnusListBox"
            };

            // Editierbare Felder zurücksetzen (weiß)
            foreach (var name in editableTextBoxNames)
            {
                var textBox = FindName(name) as TextBox;
                if (textBox != null)
                    textBox.Background = Brushes.White;
            }

            // Kommentarfelder zurücksetzen (je nach Inhalt)
            foreach (var name in commentTextBoxNames)
            {
                var textBox = FindName(name) as TextBox;
                if (textBox != null)
                {
                    // Wenn Kommentar vorhanden: Hellgelb, sonst Hellgrau
                    if (!string.IsNullOrEmpty(textBox.Text))
                    {
                        textBox.Background = new SolidColorBrush(Color.FromRgb(255, 255, 224)); // Hellgelb
                    }
                    else
                    {
                        textBox.Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)); // Hellgrau
                    }
                }
            }

            // ListBoxen zurücksetzen (weiß, da editierbar)
            foreach (var name in listBoxNames)
            {
                var listBox = FindName(name) as ListBox;
                if (listBox != null)
                    listBox.Background = Brushes.White;
            }
        }
    }
}

