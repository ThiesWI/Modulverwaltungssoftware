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
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Observable collection that holds project names for the dropdown menu
        public System.Collections.ObjectModel.ObservableCollection<string> Projects { get; } =
            new System.Collections.ObjectModel.ObservableCollection<string>();

        // Property für User-Info Anzeige (Platzhalter, später dynamisch)
        public string UserInfo => $"User: {Benutzer.CurrentUser.Name}\nRolle: {Benutzer.CurrentUser.RollenName}";

        public MainWindow()
        {
            InitializeComponent();
            // Set DataContext so bindings in XAML can resolve to this window
            this.DataContext = this;

            // Popup beim Fenster-Deaktivieren schließen
            this.Deactivated += (s, e) =>
            {
                if (ProjectsPopup != null) ProjectsPopup.IsOpen = false;
            };

            // MEINE PROJEKTE laden (nur Module des Users)
            // ✅ INTELLIGENTE FILTERUNG: RefreshMyProjects() übernimmt die Logik
            RefreshMyProjects();

            // Initial navigation
            MainFrame.Navigate(new StartPage());

            // Dynamischer Titel & Navigation History löschen
            MainFrame.Navigated += (s, e) =>
            {
                if (e.Content is Page page && !string.IsNullOrEmpty(page.Title))
                    this.Title = $"Modulverwaltung – {page.Title}";
                else
                    this.Title = "Modulverwaltung";

                // Navigation History löschen
                while (MainFrame.CanGoBack)
                    MainFrame.RemoveBackEntry();

                // "Meine Projekte" bei jeder Navigation aktualisieren
                RefreshMyProjects();
            };
        }

        // Lädt "Meine Projekte" neu
        private void RefreshMyProjects()
        {
            // ✅ INTELLIGENTE FILTERUNG FÜR "MEINE PROJEKTE"
            string currentUser = Benutzer.CurrentUser?.Name;
            string rolle = Benutzer.CurrentUser?.RollenName ?? "Gast";
            
            List<string> modulNamen = new List<string>();

            using (var db = new Services.DatabaseContext())
            {
                IQueryable<Modul> meineModule = null;

                switch (rolle)
                {
                    case "Gast":
                        // Gäste haben KEINE eigenen Projekte
                        modulNamen = new List<string> { "Keine eigenen Projekte vorhanden" };
                        UpdateProjects(modulNamen);
                        return;

                    case "Dozent":
                        // Dozenten sehen NUR ihre selbst erstellten Module
                        var dozentenModulIds = db.ModulVersion
                            .Where(v => v.Ersteller == currentUser)
                            .Select(v => v.ModulId)
                            .Distinct()
                            .ToList();
                        
                        meineModule = db.Modul.Where(m => dozentenModulIds.Contains(m.ModulID));
                        break;

                    case "Koordination":
                        // Koordination sieht Module mit Status "InPruefungKoordination"
                        var koordinationModulIds = db.ModulVersion
                            .Where(v => v.ModulStatus == ModulVersion.Status.InPruefungKoordination)
                            .Select(v => v.ModulId)
                            .Distinct()
                            .ToList();
                        
                        meineModule = db.Modul.Where(m => koordinationModulIds.Contains(m.ModulID));
                        break;

                    case "Gremium":
                        // Gremium sieht Module mit Status "InPruefungGremium"
                        var gremiumModulIds = db.ModulVersion
                            .Where(v => v.ModulStatus == ModulVersion.Status.InPruefungGremium)
                            .Select(v => v.ModulId)
                            .Distinct()
                            .ToList();
                        
                        meineModule = db.Modul.Where(m => gremiumModulIds.Contains(m.ModulID));
                        break;

                    case "Admin":
                        // Admin sieht ALLE Module
                        meineModule = db.Modul;
                        break;

                    default:
                        // Sicherheitshalber: Keine Module
                        modulNamen = new List<string> { "Keine eigenen Projekte vorhanden" };
                        UpdateProjects(modulNamen);
                        return;
                }

                if (meineModule != null)
                {
                    // ✅ ERST LADEN, DANN SORTIEREN (Entity Framework kann StringComparer nicht übersetzen)
                    modulNamen = meineModule
                        .Select(m => m.ModulnameDE)
                        .ToList()  // ← WICHTIG: ToList() VORHER ausführen
                        .OrderBy(m => m, StringComparer.CurrentCultureIgnoreCase)
                        .ToList();
                }
            }

            // Falls keine Module gefunden → Meldung anzeigen
            if (modulNamen == null || modulNamen.Count == 0)
            {
                modulNamen = new List<string> { "Keine eigenen Projekte vorhanden" };
            }

            UpdateProjects(modulNamen);
            
            System.Diagnostics.Debug.WriteLine($"'Meine Projekte' geladen: Rolle={rolle}, Anzahl={modulNamen.Count}");
        }

        // Aktualisiert die ObservableCollection mit den übergebenen Projektnamen
        private void UpdateProjects(IEnumerable<string> projektListe)
        {
            Projects.Clear();
            if (projektListe == null)
                return;

            foreach (var p in projektListe)
            {
                if (p != null)
                    Projects.Add(p);
            }
        }

        private void LogoButton_Click(object sender, RoutedEventArgs e)
        {
            // Prüfen, ob aktuell eine bearbeitbare Ansicht geöffnet ist
            var currentPage = MainFrame.Content as Page;
            bool isEditingOrComment = currentPage is EditingView || currentPage is CommentView;

            if (!isEditingOrComment)
            {
                MainFrame.Navigate(new StartPage());
                return;
            }

            var result = MessageBox.Show(
                "Der aktuelle Stand wurde noch nicht gespeichert. Soll dieser verworfen werden?",
                "Warnung",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                MainFrame.Navigate(new StartPage());
            }
            // Bei Nein passiert nichts
        }

        private void OpenProjectsMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.ContextMenu != null)
            {
                // Ensure ContextMenu has the correct placement target so binding to PlacementTarget.DataContext works
                btn.ContextMenu.PlacementTarget = btn;
                // Force placement directly below the button
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.HorizontalOffset = 0;
                btn.ContextMenu.VerticalOffset = 0;
                btn.ContextMenu.IsOpen = true;
            }
        }

        private void ProjectMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Optional: determine which project was clicked
            string projectName = null;
            if (sender is MenuItem mi && mi.DataContext is string s)
                projectName = s;

            // Navigate to ModulView (could pass projectName via constructor or set on the page)
            MainFrame.Navigate(new ModulView());
        }

        // Öffnet/Schließt das Projekte-Popup unter dem Button
        private void ToggleProjectsPopup(object sender, RoutedEventArgs e)
        {
            // Find the popup by name in the visual tree
            var popup = this.FindName("ProjectsPopup") as System.Windows.Controls.Primitives.Popup;
            if (popup != null)
            {
                popup.IsOpen = !popup.IsOpen;
            }
        }

        // Klick auf Popup-Item: zur ModulView navigieren
        private void ProjectPopupItem_Click(object sender, RoutedEventArgs e)
        {
            string modulName = (sender as Button)?.Content?.ToString();

            if (string.IsNullOrEmpty(modulName))
                return;

            // ✨ Prüfe ob es die Meldung "Keine eigenen Projekte vorhanden" ist
            if (modulName == "Keine eigenen Projekte vorhanden")
            {
                MessageBox.Show("Sie haben momentan keine eigenen Projekte.", 
                    "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Popup schließen
                var popup = this.FindName("ProjectsPopup") as System.Windows.Controls.Primitives.Popup;
                if (popup != null)
                    popup.IsOpen = false;
                
                return;
            }

            // ModulId anhand Modulname aus echter Datenbank finden
            using (var db = new Services.DatabaseContext())
            {
                var modul = db.Modul.FirstOrDefault(m => m.ModulnameDE == modulName);

                if (modul != null)
                {
                    // Zur ModulView mit gefundener ModulID navigieren
                    MainFrame.Navigate(new ModulView(modul.ModulID));
                }
                else
                {
                    MessageBox.Show($"Modul '{modulName}' konnte nicht gefunden werden.", 
                        "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            // Popup schließen
            var popup2 = this.FindName("ProjectsPopup") as System.Windows.Controls.Primitives.Popup;
            if (popup2 != null)
            {
                popup2.IsOpen = false;
            }
        }

        private void NotificationButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Keine neuen Benachrichtigungen.", "Benachrichtigungen", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Tastatur-Unterstützung für Projekte-Dropdown (Enter/Space)
        private void ProjectsButton_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Space)
            {
                ToggleProjectsPopup(sender, new RoutedEventArgs());
                e.Handled = true;
            }
        }

    }
}
