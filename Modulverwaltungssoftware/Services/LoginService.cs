using System;
using System.Linq;
using System.Security.Authentication;

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
                catch (Exception)
                {
                    throw;
                }
            }
        } // Nutzerdaten mit DB abgleichen, bei Erfolg Passwort aus Arbeitsspeicher löschen und Nutzerdaten in Benutzer.CurrentUser hinterlegen
    }
}
