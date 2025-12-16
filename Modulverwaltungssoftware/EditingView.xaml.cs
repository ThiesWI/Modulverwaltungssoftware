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
    /// <summary>
    /// Interaktionslogik für EditingView.xaml
    /// </summary>
    public partial class EditingView : Page
    {
        private ScrollViewer _contentScrollViewer;
        public string SelectedVersion { get; }

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
        }

        public EditingView(string selectedVersion) : this()
        {
            SelectedVersion = selectedVersion;
        }

        // Neuer Konstruktor: Daten direkt laden + Version merken
        public EditingView(ModuleDataRepository.ModuleData moduleData, string version) : this()
        {
            SelectedVersion = version;
            LoadModuleData(moduleData);
        }

        private void LoadModuleData(ModuleDataRepository.ModuleData data)
        {
            if (data == null) return;

            // Textfelder befüllen
            TitelTextBox.Text = data.Titel;
            VersionTextBox.Text = SelectedVersion; // Version anzeigen (read-only)
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
                if (item is ListBoxItem lbi && itemsToSelect.Contains(lbi.Content.ToString()))
                {
                    listBox.SelectedItems.Add(lbi);
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

            // Daten aus UI auslesen
            var updatedData = new ModuleDataRepository.ModuleData
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

            // Daten unter derselben Version speichern
            if (!string.IsNullOrEmpty(SelectedVersion))
            {
                ModuleDataRepository.SaveVersion(SelectedVersion, updatedData);
                MessageBox.Show($"Änderungen wurden unter Version {SelectedVersion} gespeichert.", "Bestätigung", MessageBoxButton.OK, MessageBoxImage.Information);

                // Nach dem Speichern zur ModulView wechseln und diese Version anzeigen
                this.NavigationService?.Navigate(new ModulView(SelectedVersion));
            }
            else
            {
                MessageBox.Show("Fehler: Keine Version vorhanden.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void EntwurfVerwerfen_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
        "Möchten Sie den Entwurf wirklich verwerfen?",
        "Warnung",
        MessageBoxButton.YesNo,
        MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                // Zurück zur StartPage navigieren
                this.NavigationService?.Navigate(new StartPage());
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
    }
}
