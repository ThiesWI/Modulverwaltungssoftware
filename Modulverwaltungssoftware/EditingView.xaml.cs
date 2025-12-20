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
        private string _modulId;           // Aktuelles Modul (null bei neuem)
        private string _versionNummer;     // Aktuelle Version
        private bool _isEditMode;          // true = Bearbeiten, false = Neues Modul

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
            LoadModuleData(moduleData);
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

            if (_isEditMode)
            {
                // BEARBEITUNGSMODUS: Daten unter bestehender Version aktualisieren
                ModuleDataRepository.UpdateModuleVersion(_modulId, _versionNummer, moduleData);
                MessageBox.Show($"Änderungen wurden unter Version {_versionNummer} gespeichert.", 
                    "Gespeichert", MessageBoxButton.OK, MessageBoxImage.Information);

                // Zurück zur ModulView mit diesem Modul
                this.NavigationService?.Navigate(new ModulView(int.Parse(_modulId)));
            }
            else
            {
                // NEUES MODUL: Modul mit Version 1.0 erstellen
                string currentUser = "P. Brandenburg"; // Später aus Login-System
                string neueModulId = ModuleDataRepository.CreateModule(
                    TitelTextBox.Text,  // Modulname = Titel
                    currentUser,
                    moduleData
                );

                MessageBox.Show($"Neues Modul '{TitelTextBox.Text}' wurde erstellt.", 
                    "Gespeichert", MessageBoxButton.OK, MessageBoxImage.Information);

                // Zur ModulView mit dem neu erstellten Modul navigieren
                this.NavigationService?.Navigate(new ModulView(int.Parse(neueModulId)));
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

        // Validierung: Nur Zahlen in numerischen Feldern erlauben
        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Nur Ziffern erlauben
            e.Handled = !IsTextNumeric(e.Text);
        }

        private bool IsTextNumeric(string text)
        {
            foreach (char c in text)
            {
                if (!char.IsDigit(c))
                    return false;
            }
            return true;
        }
    }
}
