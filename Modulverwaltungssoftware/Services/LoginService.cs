using System;
using System.Linq;
using System.Security.Authentication;
using System.Windows;

namespace Modulverwaltungssoftware.Services
{
    public class LoginService
    {
        public static bool Login(string benutzernameOderEmail, string passwort)
        {
            using (var db = new DatabaseContext())
            {
                try
                {
                    var benutzer = db.Benutzer
                        .FirstOrDefault(b =>
                            (b.Name == benutzernameOderEmail || b.Email == benutzernameOderEmail)
                            && b.Passwort == passwort);

                    if (benutzer == null)
                    {
                        return false;
                    }
                    else
                    {
                        benutzer.Passwort = null;
                        Benutzer.CurrentUser = benutzer;
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ein Fehler ist aufgetreten"); ;
                    return false;
                }
            }
        } // Nutzerdaten mit DB abgleichen, bei Erfolg Passwort aus Arbeitsspeicher löschen und Nutzerdaten in Benutzer.CurrentUser hinterlegen
    }
}
