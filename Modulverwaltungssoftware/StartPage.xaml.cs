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
        }

        public System.Collections.ObjectModel.ObservableCollection<ModulePreview> ModulePreviews { get; } =
            new System.Collections.ObjectModel.ObservableCollection<ModulePreview>();

        public StartPage()
        {
            InitializeComponent();
            this.DataContext = this;

            // Demo-Daten: 10 leere Masken (später durch echte Daten aus lokaler DB ersetzen)
            for (int i = 1; i <= 10; i++)
            {
                ModulePreviews.Add(new ModulePreview
                {
                    Title = $"Modul {i}",
                    Studiengang = string.Empty,
                    Version = string.Empty,
                    ContentPreview = string.Empty
                });
            }
        }

        private void NeuesModulButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigation zur EditingView-Seite
            this.NavigationService?.Navigate(new EditingView());
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
                // Navigation zur ModulView oder Detailansicht
                this.NavigationService?.Navigate(new ModulView());
            }
        }
    }
}
