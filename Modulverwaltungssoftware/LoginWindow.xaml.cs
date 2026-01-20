using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
            bool autoLoginFlag = false; // true = Auto-Login aktivieren, false = Manueller Login
            bool istDozentFlag = false; // Setze auf true, um als Dozent einzuloggen
            bool istKoordinationFlag = false; // Setze auf true, um als Koordination einzuloggen
            bool istGremiumFlag = false; // Setze auf true, um als Gremium einzuloggen
            bool istAdminFlag = false; // Setze auf true, um als Admin einzuloggen

            if (autoLoginFlag) // Nur wenn aktiviert
            {
                // ✅ FIX: Benutzer aus Datenbank laden statt manuell erstellen
                using (var db = new Services.DatabaseContext())
                {
                    Benutzer benutzer = null;

                    if (istDozentFlag)
                    {
                        benutzer = db.Benutzer.FirstOrDefault(b => b.Name == "Dr. Max Mustermann");
                    }
                    else if (istKoordinationFlag)
                    {
                        benutzer = db.Benutzer.FirstOrDefault(b => b.Name == "Sabine Beispiel");
                    }
                    else if (istGremiumFlag)
                    {
                        benutzer = db.Benutzer.FirstOrDefault(b => b.Name == "Prof. Erika Musterfrau");
                    }
                    else if (istAdminFlag)
                    {
                        benutzer = db.Benutzer.FirstOrDefault(b => b.Name == "Philipp Admin");
                    }

                    if (benutzer != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"✅ Auto-Login: Benutzer '{benutzer.Name}' aus DB geladen (Passwort vor Löschen: '{benutzer.Passwort}')");

                        // ⚠️ WICHTIG: Passwort nur aus dem IN-MEMORY Objekt löschen
                        // Entity Framework darf NICHT tracken, sonst wird DB geändert!
                        // LÖSUNG: Benutzer-Objekt detachen BEVOR wir Passwort löschen
                        db.Entry(benutzer).State = System.Data.Entity.EntityState.Detached;

                        // Jetzt können wir das Passwort sicher löschen (nur im Speicher)
                        benutzer.Passwort = null;

                        // ✅ AktuelleRolle wird automatisch über die Property gesetzt
                        Benutzer.CurrentUser = benutzer;

                        System.Diagnostics.Debug.WriteLine($"✅ Auto-Login erfolgreich: {benutzer.Name} (Rolle: {benutzer.RollenName}, ID: {benutzer.BenutzerID})");

                        var mainWindow = new MainWindow();
                        mainWindow.Show();
                        this.Close();
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("❌ Auto-Login fehlgeschlagen: Benutzer nicht in Datenbank gefunden");
                    }
                }
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
