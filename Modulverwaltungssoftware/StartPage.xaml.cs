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
    /// Interaktionslogik für StartPage.xaml
    /// </summary>
    public partial class StartPage : Page
    {
        public class ModulePreview
        {
            public string Title { get; set; }
            public string Studiengang { get; set; }
            public string Version { get; set; }
            public string ContentPreview { get; set; }
            public string ModulId { get; set; }  // NEUE Property für Navigation
        }

        public System.Collections.ObjectModel.ObservableCollection<ModulePreview> ModulePreviews { get; } =
            new System.Collections.ObjectModel.ObservableCollection<ModulePreview>();

        public StartPage()
        {
            InitializeComponent();
            this.DataContext = this;
            
            // Module beim Laden der Seite aktualisieren
            this.Loaded += StartPage_Loaded;
        }

        // Wird aufgerufen wenn die Seite geladen wird
        private void StartPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Module bei jedem Laden neu laden (auch nach Navigation zurück)
            LoadModulePreviews();
        }

        private void LoadModulePreviews()
        {
            // Collection leeren (wichtig bei erneutem Laden!)
            ModulePreviews.Clear();

            // Alle Module abrufen (nicht Versionen!)
            var alleModule = ModulRepository.getAllModule();

            // Temporäre Liste für Sortierung
            var tempList = new List<ModulePreview>();

            foreach (var modul in alleModule)
            {
                // Neueste Version anhand der höchsten Versionsnummer holen
                var neuesteVersion = ModulRepository.getModulVersion(modul.ModulID);

                if (neuesteVersion != null)
                {
                    // Versionsnummer formatieren
                    string versionDisplay = FormatVersionsnummer(neuesteVersion.Versionsnummer);
                    
                    tempList.Add(new ModulePreview
                    {
                        Title = modul.ModulnameDE,  // MODULNAME
                        Studiengang = neuesteVersion.Modul.Studiengang,
                        Version = $"{versionDisplay} ({neuesteVersion.ModulStatus})",
                        ContentPreview = GenerateContentPreview(neuesteVersion),
                        ModulId = modul.ModulID.ToString()
                    });
                }
            }

            // Alphabetisch nach Titel sortieren
            var sortedModules = tempList.OrderBy(m => m.Title).ToList();

            // Sortierte Module zur ObservableCollection hinzufügen
            foreach (var module in sortedModules)
            {
                ModulePreviews.Add(module);
            }
        }

        // Hilfsmethode: Konvertiere interne Versionsnummer zu Anzeige-Format (10 → "1.0")
        private string FormatVersionsnummer(int versionsnummer)
        {
            decimal version = versionsnummer / 10.0m;
            return version.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
        }

        private string GenerateContentPreview(ModulVersion data)
        {
            // Kurze Vorschau aus Lernzielen oder Lehrinhalten generieren
            if (data.Lernergebnisse != null && data.Lernergebnisse.Any())
            {
                var joined = string.Join("; ", data.Lernergebnisse);
                return joined.Length > 100
                    ? joined.Substring(0, 100) + "..."
                    : joined;
            }
            if (data.Inhaltsgliederung != null && data.Inhaltsgliederung.Any())
            {
                var joined = string.Join("; ", data.Inhaltsgliederung);
                return joined.Length > 100
                    ? joined.Substring(0, 100) + "..."
                    : joined;
            }
            return "Keine Vorschau verfügbar";
        }

        private void NeuesModulButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigation zur EditingView im "Neues Modul"-Modus
            this.NavigationService?.Navigate(new EditingView(createNew: true));
        }

        private void OeffnenButton_Click(object sender, RoutedEventArgs e)
        {
            // Logik für den "Neues Modul" Button hier einfügen
            MessageBox.Show("Oeffnen Button wurde geklickt!");
        }

        private void SearchBox_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {

        }

        private void ModulePreview_Click(object sender, RoutedEventArgs e)
        {
            var preview = (sender as Button)?.DataContext as ModulePreview;
            if (preview != null)
            {
                // Wenn ModulId vorhanden, zur ModulView navigieren
                if (!string.IsNullOrEmpty(preview.ModulId))
                {
                    this.NavigationService?.Navigate(new ModulView(int.Parse(preview.ModulId)));
                }
                else
                {
                    // Leeres Modul → zur EditingView für neues Modul
                    this.NavigationService?.Navigate(new EditingView(createNew: true));
                }
            }
        }
    }
}
