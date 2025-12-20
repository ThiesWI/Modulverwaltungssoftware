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
            var result = MessageBox.Show(
                "Soll der Kommentar wirklich final an den Modulersteller weitergereicht werden?",
                "Bestätigung senden",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Kommentare aus UI auslesen
                var fieldComments = new List<ModuleDataRepository.FieldComment>();
                
                // Alle Kommentarfelder durchgehen und nicht-leere Kommentare speichern
                AddCommentIfNotEmpty(fieldComments, "Titel", TitelKommentarTextBox.Text);
                AddCommentIfNotEmpty(fieldComments, "Modultyp", ModultypKommentarTextBox.Text);
                AddCommentIfNotEmpty(fieldComments, "Studiengang", StudiengangKommentarTextBox.Text);
                AddCommentIfNotEmpty(fieldComments, "Semester", SemesterKommentarTextBox.Text);
                AddCommentIfNotEmpty(fieldComments, "Prüfungsform", PruefungsformKommentarTextBox.Text);
                AddCommentIfNotEmpty(fieldComments, "Turnus", TurnusKommentarTextBox.Text);
                AddCommentIfNotEmpty(fieldComments, "ECTS", EctsKommentarTextBox.Text);
                AddCommentIfNotEmpty(fieldComments, "Workload Präsenz", WorkloadPraesenzKommentarTextBox.Text);
                AddCommentIfNotEmpty(fieldComments, "Workload Selbststudium", WorkloadSelbststudiumKommentarTextBox.Text);
                AddCommentIfNotEmpty(fieldComments, "Verantwortlicher", VerantwortlicherKommentarTextBox.Text);
                AddCommentIfNotEmpty(fieldComments, "Voraussetzungen", VoraussetzungenKommentarTextBox.Text);
                AddCommentIfNotEmpty(fieldComments, "Lernziele", LernzieleKommentarTextBox.Text);
                AddCommentIfNotEmpty(fieldComments, "Lehrinhalte", LehrinhalteKommentarTextBox.Text);
                AddCommentIfNotEmpty(fieldComments, "Literatur", LiteraturKommentarTextBox.Text);

                // Kommentare im Repository speichern
                if (!string.IsNullOrEmpty(_modulId) && !string.IsNullOrEmpty(_version))
                {
                    string currentUser = "Prof. Dr. Lange"; // Später aus Login-System
                    ModuleDataRepository.SaveComments(_modulId, _version, fieldComments, currentUser);
                    
                    MessageBox.Show($"Der Kommentar wurde eingereicht. Die Version wurde als kommentiert markiert ({fieldComments.Count} Kommentar(e)).", 
                        "Bestätigung", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Der Kommentar wurde eingereicht.", "Bestätigung", MessageBoxButton.OK, MessageBoxImage.Information);
                }

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

        private void AddCommentIfNotEmpty(List<ModuleDataRepository.FieldComment> comments, string fieldName, string commentText)
        {
            if (!string.IsNullOrWhiteSpace(commentText))
            {
                comments.Add(new ModuleDataRepository.FieldComment
                {
                    FieldName = fieldName,
                    Comment = commentText.Trim(),
                    CommentDate = DateTime.Now,
                    Commenter = "Prof. Dr. Lange" // Später aus Login-System
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
