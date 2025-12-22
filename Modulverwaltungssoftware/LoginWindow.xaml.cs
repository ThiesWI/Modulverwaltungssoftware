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
using System.Windows.Shapes;

namespace Modulverwaltungssoftware
{
    /// <summary>
    /// Interaktionslogik für LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Validierung
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                ShowError("Bitte Nutername oder E-Mail eingeben.");
                return;
            }
            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                ShowError("Bitte Passwort eingeben.");
                return;
            }


            // Fehler zurücksetzen
            ErrorMessage.Visibility = Visibility.Collapsed;

            // TODO: Authentifizierung (z.B. gegen DB oder Service)
            // Beispiel (hart codiert):
            var anmeldung = Services.LoginService.Login(EmailTextBox.Text, PasswordBox.Password);
            if (anmeldung == true )
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
            else
            {
                ShowError("Ungültige Anmeldedaten.");
            }
        }

        private void ShowError(string message)
        {
            ErrorMessage.Text = message;
            ErrorMessage.Foreground = new SolidColorBrush(Colors.Red);
            ErrorMessage.Visibility = Visibility.Visible;
        }

        private void ForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            ErrorMessage.Text = "Bitte kontaktieren Sie die IT-Abteilung der Hochschule, um Ihr Passwort zurückzusetzen.";
            ErrorMessage.Foreground = new SolidColorBrush(Colors.Blue);
            ErrorMessage.Visibility = Visibility.Visible;
        }
    }
}
