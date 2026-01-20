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
        public System.Collections.ObjectModel.ObservableCollection<string> Projects { get; } =
            new System.Collections.ObjectModel.ObservableCollection<string>();

        public string UserInfo => $"User: {Benutzer.CurrentUser.Name}\nRolle: {Benutzer.CurrentUser.RollenName}";

        private System.Windows.Threading.DispatcherTimer _notificationTimer;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            this.Deactivated += (s, e) =>
            {
                if (ProjectsPopup != null) ProjectsPopup.IsOpen = false;
                if (NotificationsPopup != null) NotificationsPopup.IsOpen = false;
            };

            RefreshMyProjects();

            LoadNotifications();

            _notificationTimer = new System.Windows.Threading.DispatcherTimer();
            _notificationTimer.Interval = TimeSpan.FromSeconds(30);
            _notificationTimer.Tick += (s, e) => LoadNotifications();
            _notificationTimer.Start();

            MainFrame.Navigate(new StartPage());

            MainFrame.Navigated += (s, e) =>
            {
                if (e.Content is Page page && !string.IsNullOrEmpty(page.Title))
                    this.Title = $"Modulverwaltung – {page.Title}";
                else
                    this.Title = "Modulverwaltung";

                while (MainFrame.CanGoBack)
                    MainFrame.RemoveBackEntry();

                RefreshMyProjects();

                LoadNotifications();
            };
        }

        private void RefreshMyProjects()
        {
            string currentUser = Benutzer.CurrentUser?.Name;
            string rolle = Benutzer.CurrentUser?.RollenName ?? "Gast";

            System.Diagnostics.Debug.WriteLine($"🔍 RefreshMyProjects aufgerufen - User: {currentUser}, Rolle: {rolle}");

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
                        modulNamen = new List<string> { "Keine eigenen Projekte vorhanden" };
                        UpdateProjects(modulNamen);
                        System.Diagnostics.Debug.WriteLine("👤 GAST: Keine Projekte");
                        return;

                    case "Dozent":
                        var dozentenModulIds = db.ModulVersion
                            .Where(v => v.Ersteller == currentUser)
                            .Select(v => v.ModulId)
                            .Distinct()
                            .ToList();

                        System.Diagnostics.Debug.WriteLine($"🔍 Dozent '{currentUser}' - Gefundene ModulIDs: {string.Join(", ", dozentenModulIds)}");

                        meineModule = db.Modul.Where(m => dozentenModulIds.Contains(m.ModulID));
                        break;

                    case "Koordination":
                        var koordinationModulIds = db.ModulVersion
                            .Where(v => v.ModulStatus == ModulVersion.Status.InPruefungKoordination)
                            .Select(v => v.ModulId)
                            .Distinct()
                            .ToList();

                        System.Diagnostics.Debug.WriteLine($"🔍 Koordination - Gefundene ModulIDs: {string.Join(", ", koordinationModulIds)}");

                        meineModule = db.Modul.Where(m => koordinationModulIds.Contains(m.ModulID));
                        break;

                    case "Gremium":
                        var gremiumModulIds = db.ModulVersion
                            .Where(v => v.ModulStatus == ModulVersion.Status.InPruefungGremium)
                            .Select(v => v.ModulId)
                            .Distinct()
                            .ToList();

                        System.Diagnostics.Debug.WriteLine($"🔍 Gremium - Gefundene ModulIDs: {string.Join(", ", gremiumModulIds)}");

                        meineModule = db.Modul.Where(m => gremiumModulIds.Contains(m.ModulID));
                        break;

                    case "Admin":
                        meineModule = db.Modul;
                        System.Diagnostics.Debug.WriteLine("👑 Admin - Alle Module werden angezeigt");
                        break;

                    default:
                        modulNamen = new List<string> { "Keine eigenen Projekte vorhanden" };
                        UpdateProjects(modulNamen);
                        System.Diagnostics.Debug.WriteLine($"⚠️ Unbekannte Rolle: {rolle}");
                        return;
                }

                if (meineModule != null)
                {
                    modulNamen = meineModule
                        .Select(m => m.ModulnameDE)
                        .ToList()
                        .OrderBy(m => m, StringComparer.CurrentCultureIgnoreCase)
                        .ToList();
                }
            }

            if (modulNamen == null || modulNamen.Count == 0)
            {
                modulNamen = new List<string> { "Keine eigenen Projekte vorhanden" };
            }

            UpdateProjects(modulNamen);

            System.Diagnostics.Debug.WriteLine($"✅ 'Meine Projekte' geladen: Rolle={rolle}, Anzahl={modulNamen.Count}, Module: {string.Join(", ", modulNamen)}");
        }

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

        private void ToggleProjectsPopup(object sender, RoutedEventArgs e)
        {
            var popup = this.FindName("ProjectsPopup") as System.Windows.Controls.Primitives.Popup;
            if (popup != null)
            {
                popup.IsOpen = !popup.IsOpen;
            }
        }

        private void ProjectPopupItem_Click(object sender, RoutedEventArgs e)
        {
            string modulName = (sender as Button)?.Content?.ToString();

            if (string.IsNullOrEmpty(modulName))
                return;

            if (modulName == "Keine eigenen Projekte vorhanden")
            {
                MessageBox.Show("Sie haben momentan keine eigenen Projekte.",
                    "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                var popup = this.FindName("ProjectsPopup") as System.Windows.Controls.Primitives.Popup;
                if (popup != null)
                    popup.IsOpen = false;

                return;
            }

            using (var db = new Services.DatabaseContext())
            {
                var modul = db.Modul.FirstOrDefault(m => m.ModulnameDE == modulName);

                if (modul != null)
                {
                    MainFrame.Navigate(new ModulView(modul.ModulID));
                }
                else
                {
                    MessageBox.Show($"Modul '{modulName}' konnte nicht gefunden werden.",
                        "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            var popup2 = this.FindName("ProjectsPopup") as System.Windows.Controls.Primitives.Popup;
            if (popup2 != null)
            {
                popup2.IsOpen = false;
            }
        }

        private void NotificationButton_Click(object sender, RoutedEventArgs e)
        {
            LoadNotifications();
            NotificationsPopup.IsOpen = !NotificationsPopup.IsOpen;
        }

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

                var relevanteNotifications = FilterRelevantNotifications(ungelesene);

                if (relevanteNotifications.Count == 0)
                {
                    NotificationBadge.Visibility = Visibility.Collapsed;
                    NotificationsList.ItemsSource = null;
                    return;
                }

                NotificationBadge.Visibility = Visibility.Visible;
                NotificationCount.Text = relevanteNotifications.Count > 99 ? "99+" : relevanteNotifications.Count.ToString();

                if (relevanteNotifications.Any(n => n.Nachricht.Contains("Änderungsbedarf") || n.Nachricht.Contains("kommentiert")))
                {
                    NotificationBadge.Background = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    NotificationBadge.Background = new SolidColorBrush(Colors.OrangeRed);
                }

                NotificationsList.ItemsSource = relevanteNotifications.OrderByDescending(n => n.GesendetAm).ToList();

                System.Diagnostics.Debug.WriteLine($"📬 {relevanteNotifications.Count} relevante Benachrichtigungen geladen");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Fehler beim Laden der Benachrichtigungen: {ex.Message}");
            }
        }

        /// <summary>
        /// Filtert Benachrichtigungen basierend auf Rolle und Modulstatus.
        /// </summary>
        private List<Benachrichtigung> FilterRelevantNotifications(List<Benachrichtigung> alle)
        {
            string rolle = Benutzer.CurrentUser?.RollenName ?? "Gast";

            if (rolle == "Dozent" || rolle == "Admin" || rolle == "Gast")
            {
                return alle;
            }

            var relevante = new List<Benachrichtigung>();

            using (var db = new Services.DatabaseContext())
            {
                foreach (var notification in alle)
                {
                    if (!notification.BetroffeneModulVersionID.HasValue)
                    {
                        relevante.Add(notification);
                        continue;
                    }

                    var modulVersion = db.ModulVersion
                        .FirstOrDefault(v => v.ModulVersionID == notification.BetroffeneModulVersionID.Value);

                    if (modulVersion == null)
                    {
                        continue;
                    }

                    if (rolle == "Koordination")
                    {
                        if (modulVersion.ModulStatus == ModulVersion.Status.InPruefungKoordination)
                        {
                            relevante.Add(notification);
                        }
                    }
                    else if (rolle == "Gremium")
                    {
                        if (modulVersion.ModulStatus == ModulVersion.Status.InPruefungGremium)
                        {
                            relevante.Add(notification);
                        }
                    }
                }
            }

            return relevante;
        }

        /// <summary>
        /// Navigiert zur Modulansicht bei Klick auf eine Benachrichtigung.
        /// </summary>
        private void NotificationItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Benachrichtigung notification)
            {
                using (var db = new Services.DatabaseContext())
                {
                    var dbNotification = db.Benachrichtigung.Find(notification.BenachrichtigungsID);
                    if (dbNotification != null)
                    {
                        dbNotification.Gelesen = true;
                        db.SaveChanges();
                    }
                }

                if (notification.BetroffeneModulVersionID.HasValue && notification.BetroffeneModulVersionID.Value > 0)
                {
                    using (var db = new Services.DatabaseContext())
                    {
                        var modulVersion = db.ModulVersion
                            .FirstOrDefault(v => v.ModulVersionID == notification.BetroffeneModulVersionID.Value);

                        if (modulVersion != null)
                        {
                            MainFrame.Navigate(new ModulView(modulVersion.ModulId));
                            NotificationsPopup.IsOpen = false;
                            LoadNotifications();
                            return;
                        }
                    }
                }

                NotificationsPopup.IsOpen = false;
                LoadNotifications();

                MessageBox.Show(notification.Nachricht, "Benachrichtigung",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Markiert alle Benachrichtigungen als gelesen.
        /// </summary>
        private void MarkAllAsRead_Click(object sender, RoutedEventArgs e)
        {
            BenachrichtigungsService.MarkiereAlsGelesen();
            LoadNotifications();
            NotificationsPopup.IsOpen = false;

            MessageBox.Show("Alle Benachrichtigungen wurden als gelesen markieren.",
                "Benachrichtigungen", MessageBoxButton.OK, MessageBoxImage.Information);
        }

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
