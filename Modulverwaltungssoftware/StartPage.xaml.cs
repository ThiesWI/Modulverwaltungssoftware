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

            // Echte Daten aus Repository laden
            LoadModulePreviews();
        }

        private void LoadModulePreviews()
        {
            // Alle Module abrufen (nicht Versionen!)
            var alleModule = ModuleDataRepository.GetAllModules();

            // Temporäre Liste für Sortierung
            var tempList = new List<ModulePreview>();

            foreach (var modul in alleModule)
            {
                // Neueste Version des Moduls holen
                var neuesteVersion = modul.Versionen.OrderByDescending(v => v.ErstellDatum).FirstOrDefault();

                if (neuesteVersion != null)
                {
                    tempList.Add(new ModulePreview
                    {
                        Title = modul.ModulName,  // MODULNAME
                        Studiengang = neuesteVersion.Daten.Studiengang,
                        Version = $"{neuesteVersion.VersionsNummer} ({neuesteVersion.Status})",
                        ContentPreview = GenerateContentPreview(neuesteVersion.Daten),
                        ModulId = modul.ModulId
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

            // Falls weniger als 10 Module vorhanden sind, leere Platzhalter hinzufügen
            while (ModulePreviews.Count < 10)
            {
                ModulePreviews.Add(new ModulePreview
                {
                    Title = $"Modul {ModulePreviews.Count + 1}",
                    Studiengang = string.Empty,
                    Version = string.Empty,
                    ContentPreview = string.Empty,
                    ModulId = string.Empty
                });
            }
        }

        private string GenerateContentPreview(ModuleDataRepository.ModuleData data)
        {
            // Kurze Vorschau aus Lernzielen oder Lehrinhalten generieren
            if (!string.IsNullOrWhiteSpace(data.Lernziele))
            {
                return data.Lernziele.Length > 100 
                    ? data.Lernziele.Substring(0, 100) + "..." 
                    : data.Lernziele;
            }
            if (!string.IsNullOrWhiteSpace(data.Lehrinhalte))
            {
                return data.Lehrinhalte.Length > 100 
                    ? data.Lehrinhalte.Substring(0, 100) + "..." 
                    : data.Lehrinhalte;
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
                    this.NavigationService?.Navigate(new ModulView(preview.ModulId));
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
