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
    /// Interaktionslogik für CommentView.xaml
    /// </summary>
    public partial class CommentView : Page
    {
        // Datenklasse für Modulversion (muss mit ModulView.ModuleData kompatibel sein)
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

        private ModuleData _moduleData;
        private string _modulId;   // ModulID merken
        private string _version;   // Version merken

        public CommentView()
        {
            InitializeComponent();
        }

        public CommentView(ModuleData moduleData, string modulId, string version) : this()
        {
            _moduleData = moduleData;
            _modulId = modulId;
            _version = version;
            LoadModuleData();
        }

        // Legacy-Konstruktor (falls noch verwendet)
        public CommentView(ModuleData moduleData, string version) : this(moduleData, null, version)
        {
        }

        private void LoadModuleData()
        {
            if (_moduleData == null)
                return;

            // Linke Spalte (read-only) befüllen
            TitelTextBox.Text = _moduleData.Titel;
            VersionTextBox.Text = _version; // Version anzeigen
            StudiengangTextBox.Text = _moduleData.Studiengang;
            EctsTextBox.Text = _moduleData.Ects.ToString();
            WorkloadPraesenzTextBox.Text = _moduleData.WorkloadPraesenz.ToString();
            WorkloadSelbststudiumTextBox.Text = _moduleData.WorkloadSelbststudium.ToString();
            VerantwortlicherTextBox.Text = _moduleData.Verantwortlicher;
            VoraussetzungenTextBox.Text = _moduleData.Voraussetzungen;
            LernzieleTextBox.Text = _moduleData.Lernziele;
            LehrinhalteTextBox.Text = _moduleData.Lehrinhalte;
            LiteraturTextBox.Text = _moduleData.Literatur;

            // ListBoxen befüllen
            SelectListBoxItems(ModultypListBox, _moduleData.Modultypen);
            SelectListBoxItems(SemesterListBox, _moduleData.Semester);
            SelectListBoxItems(PruefungsformListBox, _moduleData.Pruefungsformen);
            SelectListBoxItems(TurnusListBox, _moduleData.Turnus);
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

        private void KommentarAbschicken_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
                return;

            var result = MessageBox.Show(
                "Soll der Kommentar wirklich final an den Modulersteller weitergereicht werden?",
                "Bestätigung senden",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                var feldKommentare = CollectComments();
                
                if (feldKommentare.Count == 0)
                {
                    MessageBox.Show("Bitte geben Sie mindestens einen Kommentar ein.", 
                        "Keine Kommentare", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                SaveCommentsToDatabase(feldKommentare);
                
                MessageBox.Show($"Der Kommentar wurde erfolgreich eingereicht ({feldKommentare.Count} Kommentar(e)).", 
                    "Bestätigung", MessageBoxButton.OK, MessageBoxImage.Information);

                NavigateToModulView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Speichern der Kommentare: {ex.Message}", 
                    "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrEmpty(_modulId))
            {
                MessageBox.Show("Fehler: Modul-ID nicht gesetzt.", 
                    "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrEmpty(_version))
            {
                MessageBox.Show("Fehler: Version nicht gesetzt.", 
                    "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private List<Kommentar.FeldKommentar> CollectComments()
        {
            var feldKommentare = new List<Kommentar.FeldKommentar>();
            
            AddKommentarIfNotEmpty(feldKommentare, "Titel", TitelKommentarTextBox.Text);
            AddKommentarIfNotEmpty(feldKommentare, "Modultyp", ModultypKommentarTextBox.Text);
            AddKommentarIfNotEmpty(feldKommentare, "Studiengang", StudiengangKommentarTextBox.Text);
            AddKommentarIfNotEmpty(feldKommentare, "Semester", SemesterKommentarTextBox.Text);
            AddKommentarIfNotEmpty(feldKommentare, "Prüfungsform", PruefungsformKommentarTextBox.Text);
            AddKommentarIfNotEmpty(feldKommentare, "Turnus", TurnusKommentarTextBox.Text);
            AddKommentarIfNotEmpty(feldKommentare, "ECTS", EctsKommentarTextBox.Text);
            AddKommentarIfNotEmpty(feldKommentare, "Workload Präsenz", WorkloadPraesenzKommentarTextBox.Text);
            AddKommentarIfNotEmpty(feldKommentare, "Workload Selbststudium", WorkloadSelbststudiumKommentarTextBox.Text);
            AddKommentarIfNotEmpty(feldKommentare, "Verantwortlicher", VerantwortlicherKommentarTextBox.Text);
            AddKommentarIfNotEmpty(feldKommentare, "Voraussetzungen", VoraussetzungenKommentarTextBox.Text);
            AddKommentarIfNotEmpty(feldKommentare, "Lernziele", LernzieleKommentarTextBox.Text);
            AddKommentarIfNotEmpty(feldKommentare, "Lehrinhalte", LehrinhalteKommentarTextBox.Text);
            AddKommentarIfNotEmpty(feldKommentare, "Literatur", LiteraturKommentarTextBox.Text);

            return feldKommentare;
        }

        private void SaveCommentsToDatabase(List<Kommentar.FeldKommentar> feldKommentare)
        {
            int modulId = int.Parse(_modulId);
            
            using (var db = new Services.DatabaseContext())
            {
                var modulVersion = db.ModulVersion
                    .FirstOrDefault(v => v.ModulId == modulId);
                
                if (modulVersion == null)
                    throw new InvalidOperationException("Modulversion nicht gefunden.");

                string currentUser = Benutzer.CurrentUser?.Name ?? "Unbekannt";  // ✅ FIX: Aktueller User!
                
                // Neue Version mit Kommentaren erstellen
                int neueVersionID = Kommentar.addFeldKommentareMitNeuerVersion(
                    modulId, 
                    modulVersion.ModulVersionID, 
                    feldKommentare, 
                    currentUser  // ← Statt fest codiert!
                );
            }
        }

        private void NavigateToModulView()
        {
            if (!string.IsNullOrEmpty(_modulId))
            {
                this.NavigationService?.Navigate(new ModulView(int.Parse(_modulId)));
            }
            else
            {
                this.NavigationService?.Navigate(new StartPage());
            }
        }

        private void AddKommentarIfNotEmpty(List<Kommentar.FeldKommentar> kommentare, string feldName, string kommentarText)
        {
            if (!string.IsNullOrWhiteSpace(kommentarText))
            {
                kommentare.Add(new Kommentar.FeldKommentar
                {
                    FeldName = feldName,
                    Text = kommentarText.Trim()
                });
            }
        }

        private void KommentarVerwerfen_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Möchten Sie den Kommentar wirklich verwerfen?",
                "Warnung",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                // Zurück zur ModulView mit korrekter ModulID
                if (!string.IsNullOrEmpty(_modulId))
                {
                    this.NavigationService?.Navigate(new ModulView(int.Parse(_modulId)));
                }
                else
                {
                    this.NavigationService?.Navigate(new StartPage());
                }
            }
            // Bei Nein passiert nichts
        }
    }
}
