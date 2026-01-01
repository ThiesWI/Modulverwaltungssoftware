using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Diagnostics.Eventing.Reader;
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
#if DEBUG // Wird nur im DEBUG-Modus kompiliert
            bool autoLoginFlag = true; // true = Auto-Login aktivieren, false = Manueller Login
            bool istDozentFlag = true; // Setze auf true, um als Dozent einzuloggen
            bool istKoordinationFlag = false; // Setze auf true, um als Koordination einzuloggen
            bool istGremiumFlag = false; // Setze auf true, um als Gremium einzuloggen
            bool istAdminFlag = false; // Setze auf true, um als Admin einzuloggen
            
            if (autoLoginFlag) // Nur wenn aktiviert
            {
                if (istDozentFlag)
                {
                    Benutzer.CurrentUser = new Benutzer
                    {
                        BenutzerID = 1,
                        Name = "Dr. Max Mustermann",
                        Email = "max.mustermann@hs-example.de",
                        RollenName = "Dozent",
                        AktuelleRolle = Models.RollenKonfiguration.GetRolleByName("Dozent")
                    };
                }
                else if (istKoordinationFlag)
                {
                    Benutzer.CurrentUser = new Benutzer
                    {
                        BenutzerID = 2,
                        Name = "Sabine Beispiel",
                        Email = "sabine.beispiel@hs-example.de",
                        RollenName = "Koordination",
                        AktuelleRolle = Models.RollenKonfiguration.GetRolleByName("Koordination")
                    };
                }
                else if (istGremiumFlag)
                {
                    Benutzer.CurrentUser = new Benutzer
                    {
                        BenutzerID = 3,
                        Name = "Prof. Erika Musterfrau",
                        Email = "erika.musterfrau@hs-example.de",
                        RollenName = "Gremium",
                        AktuelleRolle = Models.RollenKonfiguration.GetRolleByName("Gremium")
                    };
                }
                else if (istAdminFlag)
                {
                    Benutzer.CurrentUser = new Benutzer
                    {
                        BenutzerID = 99,
                        Name = "Philipp Admin",
                        Email = "admin@hs-example.de",
                        RollenName = "Admin",
                        AktuelleRolle = Models.RollenKonfiguration.GetRolleByName("Admin")
                    };
                }
                
                var mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
#endif
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Validierung
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                ShowError("Bitte Nutzername oder E-Mail eingeben.");
                return;
            }
            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                ShowError("Bitte Passwort eingeben.");
                return;
            }


            // Fehler zurücksetzen
            ErrorMessage.Visibility = Visibility.Collapsed;

            var anmeldung = Services.LoginService.Login(EmailTextBox.Text, PasswordBox.Password);
            if (anmeldung == true)
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
