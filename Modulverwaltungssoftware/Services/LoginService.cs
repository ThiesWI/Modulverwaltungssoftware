using System;
using System.Security.Authentication;

namespace Modulverwaltungssoftware.Services
{
    public class LoginService
    {
        public string Login(string benutzername, string passwort) // DB einbinden
        {
            using (var db = new DatabaseContext())
            {
                try
                {
                    var benutzer = db.Benutzer.Find(benutzername, passwort);
                    if (benutzer == null)
                    {
                        throw new AuthenticationException("Anmeldung fehlgeschlagen. Überprüfen Sie ihre Eingaben.");
                    }
                    else return benutzer.RollenName;
                }
                catch (Exception ex) { throw; }
                }
        }
    }
}
