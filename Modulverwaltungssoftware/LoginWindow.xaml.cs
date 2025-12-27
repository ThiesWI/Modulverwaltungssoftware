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
            bool autoLoginFlag = true; // Setze auf true, um die Auto-Login-Funktion zu aktivieren
            bool istDozentFlag = false; // Setze auf true, um als Dozent einzuloggen
            bool istKoordinationFlag = false; // Setze auf true, um als Koordination einzuloggen
            bool istGremiumFlag = false; // Setze auf true, um als Gremium einzuloggen
            bool istAdminFlag = true; // Setze auf true, um als Admin einzuloggen
            void autoLogin() // DEBUG-Methode zum automatischen Einloggen
            {
                if (autoLoginFlag == true)
                {
                    if (istDozentFlag == true)
                    {
                        Benutzer.CurrentUser = new Benutzer
                        {
                            BenutzerID = 1,
                            Name = "Dr. Max Mustermann",
                            Email = "",
                            RollenName = "Dozent",
                            AktuelleRolle = new Rolle
                            {
                                RollenName = "Dozent",
                                DarfBearbeiten = true,
                                DarfFreigeben = false,
                                DarfStatusAendern = false
                            }
                        };
                    }
                    else if (istKoordinationFlag == true)
                    {
                        Benutzer.CurrentUser = new Benutzer
                        {
                            BenutzerID = 2,
                            Name = "Sabine Beispiel",
                            Email = "",
                            RollenName = "Koordination",
                            AktuelleRolle = new Rolle
                            {
                                RollenName = "Koordination",
                                DarfBearbeiten = false,
                                DarfFreigeben = false,
                                DarfKommentieren = true,
                                DarfStatusAendern = true
                            }
                        };
                    }
                    else if (istGremiumFlag == true)
                    {
                        Benutzer.CurrentUser = new Benutzer
                        {
                            BenutzerID = 3,
                            Name = "Prof. Erika Musterfrau",
                            Email = "",
                            RollenName = "Gremium",
                            AktuelleRolle = new Rolle
                            {
                                RollenName = "Gremium",
                                DarfBearbeiten = false,
                                DarfFreigeben = true,
                                DarfKommentieren = true,
                                DarfStatusAendern = true
                            }
                        };
                    }
                    else if (istAdminFlag == true)
                    {
                        Benutzer.CurrentUser = new Benutzer
                        {
                            BenutzerID = 99,
                            Name = "Philipp Admin",
                            Email = "",
                            RollenName = "Admin",
                            AktuelleRolle = new Rolle
                            {
                                RollenName = "Admin",
                                DarfBearbeiten = true,
                                DarfFreigeben = true,
                                DarfKommentieren = true,
                                DarfStatusAendern = true
                            }
                        };
                    }
                }
                var mainWindow = new MainWindow(); // Mainwindow öffnen, LoginWindow schließen
                mainWindow.Show();
                this.Close();
                return;
            }
            autoLogin();
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
