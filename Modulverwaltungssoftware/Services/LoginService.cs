using System;
using System.Linq;
using System.Security.Authentication;

namespace Modulverwaltungssoftware.Services
{
    public class LoginService
    {
        public static Benutzer Login(string benutzernameOderEmail, string passwort)
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
                        return null;
                    }
                    else
                    {
                        benutzer.Passwort = null;
                        return benutzer;
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
    }
}
