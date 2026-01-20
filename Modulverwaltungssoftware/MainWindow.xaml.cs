using Modulverwaltungssoftware.Models;  // ✅ HINZUGEFÜGT für Benachrichtigung
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


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

        private System.Windows.Threading.DispatcherTimer _notificationTimer;

        public MainWindow()
        {
            InitializeComponent();
            // Set DataContext so bindings in XAML can resolve to this window
            this.DataContext = this;

            // Popup beim Fenster-Deaktivieren schließen
            this.Deactivated += (s, e) =>
            {
                if (ProjectsPopup != null) ProjectsPopup.IsOpen = false;
                if (NotificationsPopup != null) NotificationsPopup.IsOpen = false;
            };

            // MEINE PROJEKTE laden (nur Module des Users)
            // ✅ INTELLIGENTE FILTERUNG: RefreshMyProjects() übernimmt die Logik
            RefreshMyProjects();

            // ✅ BENACHRICHTIGUNGEN initial laden
            LoadNotifications();

            // ✅ Timer für automatische Aktualisierung (alle 30 Sekunden)
            _notificationTimer = new System.Windows.Threading.DispatcherTimer();
            _notificationTimer.Interval = TimeSpan.FromSeconds(30);
            _notificationTimer.Tick += (s, e) => LoadNotifications();
            _notificationTimer.Start();

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

                // ✅ Benachrichtigungen bei jeder Navigation aktualisieren
                LoadNotifications();
            };
        }

        // Lädt "Meine Projekte" neu
        private void RefreshMyProjects()
        {
            // ✅ INTELLIGENTE FILTERUNG FÜR "MEINE PROJEKTE"
            string currentUser = Benutzer.CurrentUser?.Name;
            string rolle = Benutzer.CurrentUser?.RollenName ?? "Gast";

            System.Diagnostics.Debug.WriteLine($"🔍 RefreshMyProjects aufgerufen - User: {currentUser}, Rolle: {rolle}");

            // ✅ NULL-CHECK: Falls CurrentUser nicht gesetzt ist
            if (Benutzer.CurrentUser == null)
            {
                System.Diagnostics.Debug.WriteLine("⚠️ Benutzer.CurrentUser ist NULL!");
                UpdateProjects(new List<string> { "Keine eigenen Projekte vorhanden" });
                return;
            }

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
                        System.Diagnostics.Debug.WriteLine("👤 GAST: Keine Projekte");
                        return;

                    case "Dozent":
                        // Dozenten sehen NUR ihre selbst erstellten Module
                        var dozentenModulIds = db.ModulVersion
                            .Where(v => v.Ersteller == currentUser)
                            .Select(v => v.ModulId)
                            .Distinct()
                            .ToList();

                        System.Diagnostics.Debug.WriteLine($"🔍 Dozent '{currentUser}' - Gefundene ModulIDs: {string.Join(", ", dozentenModulIds)}");

                        meineModule = db.Modul.Where(m => dozentenModulIds.Contains(m.ModulID));
                        break;

                    case "Koordination":
                        // Koordination sieht Module mit Status "InPruefungKoordination"
                        var koordinationModulIds = db.ModulVersion
                            .Where(v => v.ModulStatus == ModulVersion.Status.InPruefungKoordination)
                            .Select(v => v.ModulId)
                            .Distinct()
                            .ToList();

                        System.Diagnostics.Debug.WriteLine($"🔍 Koordination - Gefundene ModulIDs: {string.Join(", ", koordinationModulIds)}");

                        meineModule = db.Modul.Where(m => koordinationModulIds.Contains(m.ModulID));
                        break;

                    case "Gremium":
                        // Gremium sieht Module mit Status "InPruefungGremium"
                        var gremiumModulIds = db.ModulVersion
                            .Where(v => v.ModulStatus == ModulVersion.Status.InPruefungGremium)
                            .Select(v => v.ModulId)
                            .Distinct()
                            .ToList();

                        System.Diagnostics.Debug.WriteLine($"🔍 Gremium - Gefundene ModulIDs: {string.Join(", ", gremiumModulIds)}");

                        meineModule = db.Modul.Where(m => gremiumModulIds.Contains(m.ModulID));
                        break;

                    case "Admin":
                        // Admin sieht ALLE Module
                        meineModule = db.Modul;
                        System.Diagnostics.Debug.WriteLine("👑 Admin - Alle Module werden angezeigt");
                        break;

                    default:
                        // Sicherheitshalber: Keine Module
                        modulNamen = new List<string> { "Keine eigenen Projekte vorhanden" };
                        UpdateProjects(modulNamen);
                        System.Diagnostics.Debug.WriteLine($"⚠️ Unbekannte Rolle: {rolle}");
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

            System.Diagnostics.Debug.WriteLine($"✅ 'Meine Projekte' geladen: Rolle={rolle}, Anzahl={modulNamen.Count}, Module: {string.Join(", ", modulNamen)}");
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
            LoadNotifications(); // Aktualisieren vor dem Öffnen
            NotificationsPopup.IsOpen = !NotificationsPopup.IsOpen;
        }

        /// <summary>
        /// ✅ Lädt ungelesene Benachrichtigungen und aktualisiert das Badge
        /// </summary>
        private void LoadNotifications()
        {
            try
            {
                var ungelesene = BenachrichtigungsService.EmpfangeBenachrichtigung();

                if (ungelesene == null || ungelesene.Count == 0)
                {
                    NotificationBadge.Visibility = Visibility.Collapsed;
                    NotificationsList.ItemsSource = null;
                    return;
                }

                // ✅ FILTERUNG: Nur relevante Benachrichtigungen anzeigen
                var relevanteNotifications = FilterRelevantNotifications(ungelesene);

                if (relevanteNotifications.Count == 0)
                {
                    NotificationBadge.Visibility = Visibility.Collapsed;
                    NotificationsList.ItemsSource = null;
                    return;
                }

                // Badge anzeigen mit Anzahl
                NotificationBadge.Visibility = Visibility.Visible;
                NotificationCount.Text = relevanteNotifications.Count > 99 ? "99+" : relevanteNotifications.Count.ToString();

                // Badge-Farbe basierend auf Priorität
                if (relevanteNotifications.Any(n => n.Nachricht.Contains("Änderungsbedarf") || n.Nachricht.Contains("kommentiert")))
                {
                    NotificationBadge.Background = new SolidColorBrush(Colors.Red); // Dringend
                }
                else
                {
                    NotificationBadge.Background = new SolidColorBrush(Colors.OrangeRed); // Normal
                }

                // Liste befüllen (nach Datum sortiert, neueste zuerst)
                NotificationsList.ItemsSource = relevanteNotifications.OrderByDescending(n => n.GesendetAm).ToList();

                System.Diagnostics.Debug.WriteLine($"📬 {relevanteNotifications.Count} relevante Benachrichtigungen geladen");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Fehler beim Laden der Benachrichtigungen: {ex.Message}");
            }
        }

        /// <summary>
        /// ✅ Filtert Benachrichtigungen basierend auf Rolle und Modulstatus
        /// Koordination/Gremium: Nur Benachrichtigungen zu Modulen, die noch in ihrem Status sind
        /// </summary>
        private List<Benachrichtigung> FilterRelevantNotifications(List<Benachrichtigung> alle)
        {
            string rolle = Benutzer.CurrentUser?.RollenName ?? "Gast";

            // Für Dozenten, Admin und Gäste: Alle ungelesenen Benachrichtigungen anzeigen
            if (rolle == "Dozent" || rolle == "Admin" || rolle == "Gast")
            {
                return alle;
            }

            // Für Koordination und Gremium: Nur Benachrichtigungen zu Modulen anzeigen, 
            // die noch im entsprechenden Status sind
            var relevante = new List<Benachrichtigung>();

            using (var db = new Services.DatabaseContext())
            {
                foreach (var notification in alle)
                {
                    // Benachrichtigungen ohne ModulVersionID immer anzeigen (Systemmeldungen)
                    if (!notification.BetroffeneModulVersionID.HasValue)
                    {
                        relevante.Add(notification);
                        continue;
                    }

                    // ModulVersion-Status prüfen
                    var modulVersion = db.ModulVersion
                        .FirstOrDefault(v => v.ModulVersionID == notification.BetroffeneModulVersionID.Value);

                    if (modulVersion == null)
                    {
                        // Wenn ModulVersion nicht mehr existiert, Benachrichtigung ignorieren
                        continue;
                    }

                    // Koordination: Nur Benachrichtigungen zu Modulen mit Status "InPruefungKoordination"
                    if (rolle == "Koordination")
                    {
                        if (modulVersion.ModulStatus == ModulVersion.Status.InPruefungKoordination)
                        {
                            relevante.Add(notification);
                        }
                        // Benachrichtigungen zu anderen Stati werden NICHT angezeigt
                        // (= wurden bereits weitergeleitet/kommentiert)
                    }
                    // Gremium: Nur Benachrichtigungen zu Modulen mit Status "InPruefungGremium"
                    else if (rolle == "Gremium")
                    {
                        if (modulVersion.ModulStatus == ModulVersion.Status.InPruefungGremium)
                        {
                            relevante.Add(notification);
                        }
                        // Benachrichtigungen zu anderen Stati werden NICHT angezeigt
                        // (= wurden bereits freigegeben/kommentiert)
                    }
                }
            }

            return relevante;
        }

        /// <summary>
        /// ✅ Klick auf eine Benachrichtigung → zur ModulVersion navigieren
        /// </summary>
        private void NotificationItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Benachrichtigung notification)
            {
                // Benachrichtigung als gelesen markieren
                using (var db = new Services.DatabaseContext())
                {
                    var dbNotification = db.Benachrichtigung.Find(notification.BenachrichtigungsID);
                    if (dbNotification != null)
                    {
                        dbNotification.Gelesen = true;
                        db.SaveChanges();
                    }
                }

                // Zur ModulVersion navigieren (falls vorhanden)
                if (notification.BetroffeneModulVersionID.HasValue && notification.BetroffeneModulVersionID.Value > 0)
                {
                    // ModulID aus ModulVersion ermitteln
                    using (var db = new Services.DatabaseContext())
                    {
                        var modulVersion = db.ModulVersion
                            .FirstOrDefault(v => v.ModulVersionID == notification.BetroffeneModulVersionID.Value);

                        if (modulVersion != null)
                        {
                            MainFrame.Navigate(new ModulView(modulVersion.ModulId));
                            NotificationsPopup.IsOpen = false;
                            LoadNotifications(); // Badge aktualisieren
                            return;
                        }
                    }
                }

                // Falls keine ModulVersion verknüpft, nur Popup schließen
                NotificationsPopup.IsOpen = false;
                LoadNotifications();

                MessageBox.Show(notification.Nachricht, "Benachrichtigung",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// ✅ Alle Benachrichtigungen als gelesen markieren
        /// </summary>
        private void MarkAllAsRead_Click(object sender, RoutedEventArgs e)
        {
            BenachrichtigungsService.MarkiereAlsGelesen();
            LoadNotifications();
            NotificationsPopup.IsOpen = false;

            MessageBox.Show("Alle Benachrichtigungen wurden als gelesen markieren.",
                "Benachrichtigungen", MessageBoxButton.OK, MessageBoxImage.Information);
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
