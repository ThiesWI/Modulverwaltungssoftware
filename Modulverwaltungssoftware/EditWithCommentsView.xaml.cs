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
            if (itemsToSelect == null) return;
            listBox.SelectedItems.Clear();
            foreach (var item in listBox.Items)
            {
                if (item is ListBoxItem lbi && itemsToSelect.Contains(lbi.Content.ToString()))
                {
                    listBox.SelectedItems.Add(lbi);
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
                
                // Kommentierte Version bearbeiten → Neue Version erstellen
                ErstelleNeueVersionMitAenderungen(modulId, ects, workloadPraesenz, workloadSelbststudium);
                
                MessageBox.Show($"Änderungen wurden als neue Version gespeichert.\nDie kommentierte Version bleibt erhalten.", 
                    "Gespeichert", MessageBoxButton.OK, MessageBoxImage.Information);

                // Zurück zur ModulView
                this.NavigationService?.Navigate(new ModulView(modulId));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Speichern: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void ErstelleNeueVersionMitAenderungen(int modulId, int ects, int workloadPraesenz, int workloadSelbststudium)
        {
            using (var db = new Services.DatabaseContext())
            {
                // Kommentierte Version laden
                int aktuelleVersionsnummer = ParseVersionsnummer(_versionNummer);
                
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
                // Diese Änderungen würden sich auf ALLE Versionen auswirken!

                db.ModulVersion.Add(neueVersion);
                db.SaveChanges();
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
    }
}

