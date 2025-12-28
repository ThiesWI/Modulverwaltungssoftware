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
            
            // ✨ SUCHFUNKTION: TextChanged Event für SearchBox
            var searchBox = FindName("SearchBox") as TextBox;
            if (searchBox != null)
                searchBox.TextChanged += SearchBox_TextChanged;
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

        private void KommentarAbschicken_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
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

                int modulId = int.Parse(_modulId);
                string rolle = Benutzer.CurrentUser?.RollenName ?? "Gast";
                string currentUser = Benutzer.CurrentUser?.Name ?? "Unbekannt";

                // ✨ STATUSWECHSEL BEI KOMMENTIERUNG
                string statusInfoText = "";
                string bestaetigungsText = "";

                switch (rolle)
                {
                    case "Koordination":
                        statusInfoText = "Das Modul wird als 'Änderungsbedarf' markiert und an den Ersteller zurückgegeben.";
                        bestaetigungsText = "Der Kommentar wurde erfolgreich eingereicht.\n\n" +
                                          "Das Modul wurde an den Ersteller zurückgegeben und hat jetzt den Status 'Änderungsbedarf'.";
                        break;

                    case "Gremium":
                        statusInfoText = "Das Modul wird als 'Änderungsbedarf' markiert und an den Ersteller zurückgegeben.";
                        bestaetigungsText = "Der Kommentar wurde erfolgreich eingereicht.\n\n" +
                                          "Das Modul wurde an den Ersteller zurückgegeben und hat jetzt den Status 'Änderungsbedarf'.";
                        break;

                    case "Admin":
                        statusInfoText = "Das Modul wird als 'Änderungsbedarf' markiert.";
                        bestaetigungsText = $"Der Kommentar wurde erfolgreich eingereicht ({feldKommentare.Count} Kommentar(e)).\n\n" +
                                          "Das Modul hat jetzt den Status 'Änderungsbedarf'.";
                        break;

                    default:
                        MessageBox.Show("Sie haben keine Berechtigung, Kommentare abzugeben.",
                            "Keine Berechtigung", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                }

                // Bestätigung mit Status-Info
                var result = MessageBox.Show(
                    $"Möchten Sie den Kommentar wirklich an den Modulersteller weitergeben?\n\n" +
                    $"{statusInfoText}\n\n" +
                    $"Anzahl Kommentare: {feldKommentare.Count}",
                    "Kommentierung bestätigen",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;

                // Kommentare speichern
                Kommentar.SaveCommentsToDatabase(feldKommentare, int.Parse(_modulId));

                // ✨ STATUS AUF ÄNDERUNGSBEDARF SETZEN

                    var modulVersion = ModulRepository.getModulVersion(
                        modulId,
                        versionsnummer: ParseVersionsnummer(_version)
                    );

                    if (modulVersion != null)
                    {
                        var alterStatus = modulVersion.ModulStatus;
                        ModulVersion.setStatus(
                            modulVersion.Versionsnummer,
                            modulVersion.ModulId,
                            ModulVersion.Status.Aenderungsbedarf
                        );

                    // Benachrichtigung an Ersteller
                    BenachrichtigungsService.SendeBenachrichtigung(
                            modulVersion.Ersteller,
                            $"{currentUser} ({rolle}) hat Ihr Modul '{modulVersion.Modul.ModulnameDE}' kommentiert. " +
                            $"Bitte überarbeiten Sie das Modul entsprechend der Kommentare. " +
                            $"(Status: {alterStatus} → Änderungsbedarf)",
                            modulVersion.ModulVersionID
                        );

                        System.Diagnostics.Debug.WriteLine($"Status geändert: {alterStatus} → Änderungsbedarf (durch {rolle})");
                    }
                

                // Erfolgs-Meldung
                MessageBox.Show(bestaetigungsText,
                    "Kommentierung erfolgreich", MessageBoxButton.OK, MessageBoxImage.Information);

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

            // Liste aller durchsuchbaren Felder (Links: Modul-Daten, Rechts: Kommentare)
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
                // Prüfe Daten-Inhalt (links)
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

                // Prüfe Kommentar-Inhalt (rechts)
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
                    // Hervorheben
                    trefferControl.Background = new SolidColorBrush(Color.FromArgb(100, 255, 255, 0)); // Gelb transparent
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
                    System.Diagnostics.Debug.WriteLine($"CommentView Suche '{suchbegriff}': {trefferAnzahl} Treffer");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"CommentView Suche '{suchbegriff}': Keine Treffer");
            }
        }

        /// <summary>
        /// Setzt alle Hintergrundfarben auf Standard zurück
        /// </summary>
        private void ResetHighlights()
        {
            // Alle möglichen TextBoxen und ListBoxen zurücksetzen
            var textBoxNames = new[] {
                "TitelTextBox", "StudiengangTextBox", "EctsTextBox", "WorkloadPraesenzTextBox",
                "WorkloadSelbststudiumTextBox", "VerantwortlicherTextBox", "VoraussetzungenTextBox",
                "LernzieleTextBox", "LehrinhalteTextBox", "LiteraturTextBox",
                "TitelKommentarTextBox", "ModultypKommentarTextBox", "StudiengangKommentarTextBox",
                "SemesterKommentarTextBox", "PruefungsformKommentarTextBox", "TurnusKommentarTextBox",
                "EctsKommentarTextBox", "WorkloadPraesenzKommentarTextBox", "WorkloadSelbststudiumKommentarTextBox",
                "VerantwortlicherKommentarTextBox", "VoraussetzungenKommentarTextBox", "LernzieleKommentarTextBox",
                "LehrinhalteKommentarTextBox", "LiteraturKommentarTextBox"
            };

            var listBoxNames = new[] {
                "ModultypListBox", "SemesterListBox", "PruefungsformListBox", "TurnusListBox"
            };

            foreach (var name in textBoxNames)
            {
                var textBox = FindName(name) as TextBox;
                if (textBox != null)
                    textBox.Background = Brushes.White;
            }

            foreach (var name in listBoxNames)
            {
                var listBox = FindName(name) as ListBox;
                if (listBox != null)
                    listBox.Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)); // #F5F5F5
            }
        }

        private int ParseVersionsnummer(string version)
        {
            if (decimal.TryParse(version, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out decimal dec))
                return (int)(dec * 10);
            return 10;
        }
    }
}