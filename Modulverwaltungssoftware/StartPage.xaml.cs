using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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

            this.Loaded += StartPage_Loaded;

            SearchBox.TextChanged += SearchBox_TextChanged;
        }

        private void StartPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadModulePreviews();

            UpdateNeuesModulButton();
        }

        private void LoadModulePreviews()
        {
            ModulePreviews.Clear();

            var alleModule = ModulRepository.GetModuleForUser();

            var tempList = new List<ModulePreview>();

            using (var db = new Services.DatabaseContext())
            {
                foreach (var modul in alleModule)
                {
                    var freigegebeneVersion = db.ModulVersion
                        .Where(v => v.ModulId == modul.ModulID &&
                                    v.ModulStatus == ModulVersion.Status.Freigegeben)
                        .OrderByDescending(v => v.Versionsnummer)
                        .FirstOrDefault();

                    if (freigegebeneVersion != null)
                    {
                        string versionDisplay = FormatVersionsnummer(freigegebeneVersion.Versionsnummer);

                        tempList.Add(new ModulePreview
                        {
                            Title = modul.ModulnameDE,
                            Studiengang = modul.Studiengang,
                            Version = $"{versionDisplay} (Freigegeben)",
                            ContentPreview = GenerateContentPreview(freigegebeneVersion),
                            ModulId = modul.ModulID.ToString()
                        });

                        System.Diagnostics.Debug.WriteLine($"  ✅ Modul hinzugefügt: {modul.ModulnameDE} (Version {versionDisplay} - Freigegeben)");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"  ⏭️ Modul übersprungen: {modul.ModulnameDE} (Keine freigegebene Version)");
                    }
                }
            }

            var sortedModules = tempList
                .OrderBy(m => m.Title, StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var module in sortedModules)
            {
                ModulePreviews.Add(module);
            }

            System.Diagnostics.Debug.WriteLine($"📋 StartPage: {sortedModules.Count} FREIGEGEBENE Module geladen (alphabetisch sortiert)");
        }

        /// <summary>
        /// Konvertiert die interne Versionsnummer zum Anzeigeformat (z.B. 10 → "1.0").
        /// </summary>
        private string FormatVersionsnummer(int versionsnummer)
        {
            decimal version = versionsnummer / 10.0m;
            return version.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
        }

        private string GenerateContentPreview(ModulVersion data)
        {
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

        /// <summary>
        /// Aktiviert den "Neues Modul" Button nur für Admin und Dozent
        /// </summary>
        private void UpdateNeuesModulButton()
        {
            var neuesModulButton = FindName("NeuesModulButton") as Button;
            if (neuesModulButton == null)
            {
                // Suche im Visual Tree (falls nicht direkt per Namen gefunden)
                neuesModulButton = FindButtonInVisualTree("Neues Modul");
            }

            if (neuesModulButton != null)
            {
                string rolle = Benutzer.CurrentUser?.RollenName ?? "Gast";
                bool darfErstellen = rolle == "Admin" || rolle == "Dozent";

                neuesModulButton.IsEnabled = darfErstellen;

                // Tooltip setzen für deaktivierte Buttons
                if (!darfErstellen)
                {
                    neuesModulButton.ToolTip = "Nur Administratoren und Dozenten dürfen neue Module erstellen.";
                }
                else
                {
                    neuesModulButton.ToolTip = "Neues Modul erstellen";
                }

                System.Diagnostics.Debug.WriteLine($"'Neues Modul' Button: Rolle={rolle}, Enabled={darfErstellen}");
            }
        }

        private Button FindButtonInVisualTree(string buttonContent)
        {
            return FindVisualChildren<Button>(this)
                .FirstOrDefault(b => b.Content?.ToString() == buttonContent);
        }

        /// <summary>
        /// Durchsucht den Visual Tree nach Elementen eines bestimmten Typs
        /// </summary>
        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) yield break;

            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(depObj, i);

                if (child is T tChild)
                    yield return tChild;

                foreach (var childOfChild in FindVisualChildren<T>(child))
                    yield return childOfChild;
            }
        }

        private void SearchBox_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {

        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string suchbegriff = SearchBox.Text?.Trim();

            if (string.IsNullOrEmpty(suchbegriff))
            {
                LoadModulePreviews();
                return;
            }

            var gefundeneModule = ModulRepository.sucheModule(suchbegriff);

            if (gefundeneModule == null || gefundeneModule.Count == 0)
            {
                ModulePreviews.Clear();
                System.Diagnostics.Debug.WriteLine($"Suche '{suchbegriff}': Keine Treffer");
                return;
            }

            ModulePreviews.Clear();
            var tempList = new List<ModulePreview>();

            using (var db = new Services.DatabaseContext())
            {
                foreach (var modul in gefundeneModule)
                {
                    var freigegebeneVersion = db.ModulVersion
                        .Where(v => v.ModulId == modul.ModulID &&
                                    v.ModulStatus == ModulVersion.Status.Freigegeben)
                        .OrderByDescending(v => v.Versionsnummer)
                        .FirstOrDefault();

                    if (freigegebeneVersion != null)
                    {
                        string versionDisplay = FormatVersionsnummer(freigegebeneVersion.Versionsnummer);

                        tempList.Add(new ModulePreview
                        {
                            Title = modul.ModulnameDE,
                            Studiengang = modul.Studiengang,
                            Version = $"{versionDisplay} (Freigegeben)",
                            ContentPreview = GenerateContentPreview(freigegebeneVersion),
                            ModulId = modul.ModulID.ToString()
                        });
                    }
                }
            }

            var sortedModules = tempList.OrderBy(m => m.Title, StringComparer.OrdinalIgnoreCase).ToList();
            foreach (var module in sortedModules)
            {
                ModulePreviews.Add(module);
            }

            System.Diagnostics.Debug.WriteLine($"Suche '{suchbegriff}': {tempList.Count} freigegebene Module gefunden");
        }

        private void ModulePreview_Click(object sender, RoutedEventArgs e)
        {
            var preview = (sender as Button)?.DataContext as ModulePreview;
            if (preview != null)
            {
                if (!string.IsNullOrEmpty(preview.ModulId))
                {
                    this.NavigationService?.Navigate(new ModulView(int.Parse(preview.ModulId)));
                }
                else
                {
                    this.NavigationService?.Navigate(new EditingView(createNew: true));
                }
            }
        }
    }
}
