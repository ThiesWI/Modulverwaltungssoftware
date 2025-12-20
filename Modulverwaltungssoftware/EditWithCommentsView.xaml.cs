using System;
using System.Collections.Generic;
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
            if (comments == null || comments.FieldComments == null || comments.FieldComments.Count == 0)
            {
                // Keine Kommentare vorhanden - alle Kommentarfelder bleiben leer und editierbar (sollte nicht passieren)
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

            // Daten aus UI auslesen
            var moduleData = new ModuleDataRepository.ModuleData
            {
                Titel = TitelTextBox.Text,
                Modultypen = GetSelectedListBoxItems(ModultypListBox),
                Studiengang = StudiengangTextBox.Text,
                Semester = GetSelectedListBoxItems(SemesterListBox),
                Pruefungsformen = GetSelectedListBoxItems(PruefungsformListBox),
                Turnus = GetSelectedListBoxItems(TurnusListBox),
                Ects = ects,
                WorkloadPraesenz = workloadPraesenz,
                WorkloadSelbststudium = workloadSelbststudium,
                Verantwortlicher = VerantwortlicherTextBox.Text,
                Voraussetzungen = VoraussetzungenTextBox.Text,
                Lernziele = LernzieleTextBox.Text,
                Lehrinhalte = LehrinhalteTextBox.Text,
                Literatur = LiteraturTextBox.Text
            };

            // Daten speichern (Kommentare bleiben erhalten)
            ModuleDataRepository.UpdateModuleVersion(_modulId, _versionNummer, moduleData);
            
            MessageBox.Show($"Änderungen wurden gespeichert.\nDie Kommentare bleiben erhalten.", 
                "Gespeichert", MessageBoxButton.OK, MessageBoxImage.Information);

            // Zurück zur ModulView
            this.NavigationService?.Navigate(new ModulView(int.Parse(_modulId)));
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

