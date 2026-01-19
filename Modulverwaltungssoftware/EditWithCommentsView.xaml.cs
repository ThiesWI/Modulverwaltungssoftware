using Modulverwaltungssoftware.Helpers;
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
            // ✅ Alle Felder zurücksetzen
            ResetValidationHighlights();
            
            // ✅ Basis-Validierung
            if (!ValidateBasicInputs(out double ects, out int workloadPraesenz, out int workloadSelbststudium))
            {
                return; // Fehlermeldung wurde bereits angezeigt
            }

            try
            {
                int modulId = int.Parse(_modulId);
                ModulVersion tempModulVersion = new ModulVersion
                {
                    ModulId = modulId,
                    Modul = WorkflowController.getModulDetails(modulId),
                    Versionsnummer = ParseVersionsnummer(_versionNummer),
                    EctsPunkte = ects,
                    WorkloadPraesenz = workloadPraesenz,
                    WorkloadSelbststudium = workloadSelbststudium
                };
                var pruefungsformen = GetSelectedListBoxItem(PruefungsformListBox);
                if (!string.IsNullOrEmpty(pruefungsformen))
                    tempModulVersion.Pruefungsform = pruefungsformen;
                else
                    tempModulVersion.Pruefungsform = "Klausur";
                tempModulVersion.Ersteller = Benutzer.CurrentUser?.Name ?? "Unbekannt"; // ✅ FIX: Aktueller Benutzer statt Textfeld
                // Lernziele (versionsspezifisch)
                if (!string.IsNullOrWhiteSpace(LernzieleTextBox.Text))
                {
                    tempModulVersion.Lernergebnisse = LernzieleTextBox.Text
                        .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                }
                else
                {
                    tempModulVersion.Lernergebnisse = new List<string>();
                }

                // Lehrinhalte (versionsspezifisch)
                if (!string.IsNullOrWhiteSpace(LehrinhalteTextBox.Text))
                {
                    tempModulVersion.Inhaltsgliederung = LehrinhalteTextBox.Text
                        .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                }
                else
                {
                    tempModulVersion.Inhaltsgliederung = new List<string>();
                }

                // Literatur (versionsspezifisch)
                if (!string.IsNullOrWhiteSpace(LiteraturTextBox.Text))
                {
                    tempModulVersion.Literatur = LiteraturTextBox.Text
                        .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                }
                else
                {
                    tempModulVersion.Literatur = new List<string>();
                }
                
                // Modul-Objekt aus dem aktuellen Kontext holen
                var modul = tempModulVersion.Modul;

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

                // Kommentierte Version bearbeiten → Neue Version erstellen
                bool s = ModulRepository.Speichere(tempModulVersion);
                ModulVersion neueVersion = ModulRepository.getModulVersion(modulId);

                ModulVersion.setStatus(neueVersion.Versionsnummer, modulId, ModulVersion.Status.Entwurf);
                if (s == true)
                {
                    MessageBox.Show($"Änderungen erfolgreich gespeichert.\nDie Kommentare bleiben erhalten.",
                    "Gespeichert", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.NavigationService?.Navigate(new ModulView(modulId));
                }
                return;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Speichern: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// ✅ NEU: Validiert Basis-Eingaben und hebt fehlerhafte Felder hervor
        /// </summary>
        private bool ValidateBasicInputs(out double ects, out int workloadPraesenz, out int workloadSelbststudium)
        {
            ects = 0;
            workloadPraesenz = 0;
            workloadSelbststudium = 0;
            bool isValid = true;

            // Titel prüfen
            if (string.IsNullOrWhiteSpace(TitelTextBox.Text))
            {
                ValidationHelper.MarkAsInvalid(TitelTextBox, "Titel darf nicht leer sein.");
                isValid = false;
            }

            // ECTS prüfen
            if (!double.TryParse(EctsTextBox.Text, out ects) || ects <= 0)
            {
                ValidationHelper.MarkAsInvalid(EctsTextBox, "Bitte gültige ECTS-Punktzahl (> 0) eingeben.");
                isValid = false;
            }

            // Workload Präsenz prüfen
            if (!int.TryParse(WorkloadPraesenzTextBox.Text, out workloadPraesenz))
            {
                ValidationHelper.MarkAsInvalid(WorkloadPraesenzTextBox, "Bitte gültige Workload Präsenz eingeben.");
                isValid = false;
            }

            // Workload Selbststudium prüfen
            if (!int.TryParse(WorkloadSelbststudiumTextBox.Text, out workloadSelbststudium))
            {
                ValidationHelper.MarkAsInvalid(WorkloadSelbststudiumTextBox, "Bitte gültige Workload Selbststudium eingeben.");
                isValid = false;
            }

            // Verantwortlicher prüfen
            if (string.IsNullOrWhiteSpace(VerantwortlicherTextBox.Text))
            {
                ValidationHelper.MarkAsInvalid(VerantwortlicherTextBox, "Verantwortlicher darf nicht leer sein.");
                isValid = false;
            }

            // Lernziele prüfen
            if (string.IsNullOrWhiteSpace(LernzieleTextBox.Text))
            {
                ValidationHelper.MarkAsInvalid(LernzieleTextBox, "Bitte mindestens ein Lernziel angeben.");
                isValid = false;
            }

            // Lehrinhalte prüfen
            if (string.IsNullOrWhiteSpace(LehrinhalteTextBox.Text))
            {
                ValidationHelper.MarkAsInvalid(LehrinhalteTextBox, "Bitte mindestens einen Lehrinhalt angeben.");
                isValid = false;
            }

            // Modultyp prüfen
            if (ModultypListBox.SelectedItem == null)
            {
                ValidationHelper.MarkAsInvalid(ModultypListBox, "Bitte einen Modultyp auswählen.");
                isValid = false;
            }

            // Semester prüfen
            if (SemesterListBox.SelectedItem == null)
            {
                ValidationHelper.MarkAsInvalid(SemesterListBox, "Bitte ein empfohlenes Semester auswählen (1-8).");
                isValid = false;
            }

            // Prüfungsform prüfen
            if (PruefungsformListBox.SelectedItem == null)
            {
                ValidationHelper.MarkAsInvalid(PruefungsformListBox, "Bitte eine Prüfungsform auswählen.");
                isValid = false;
            }

            // Turnus prüfen
            if (TurnusListBox.SelectedItem == null)
            {
                ValidationHelper.MarkAsInvalid(TurnusListBox, "Bitte einen Turnus auswählen.");
                isValid = false;
            }

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
        /// ✅ NEU: Setzt alle Validierungs-Hervorhebungen zurück
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

        private string GetSelectedListBoxItem(ListBox listBox)
        {
            if (listBox.SelectedItem is ListBoxItem lbi)
            {
                return lbi.Content.ToString();
            }
            return null;
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

