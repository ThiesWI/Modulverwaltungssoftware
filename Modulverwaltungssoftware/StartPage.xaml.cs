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
        public StartPage()
        {
            InitializeComponent();
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
    }
}
