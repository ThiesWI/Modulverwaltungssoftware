using System;
using System.Linq;
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
                    System.Diagnostics.Debug.WriteLine($"🔐 LOGIN-VERSUCH:");
                    System.Diagnostics.Debug.WriteLine($"   Eingabe: '{benutzernameOderEmail}'");
                    System.Diagnostics.Debug.WriteLine($"   Passwort: '{passwort}'");

                    var benutzer = db.Benutzer
                        .FirstOrDefault(b =>
                            (b.Name == benutzernameOderEmail || b.Email == benutzernameOderEmail)
                            && b.Passwort == passwort);

                    if (benutzer == null)
                    {
                        // Debug: Prüfe ob User existiert (ohne Passwort-Check)
                        var userExists = db.Benutzer
                            .FirstOrDefault(b => b.Name == benutzernameOderEmail || b.Email == benutzernameOderEmail);

                        if (userExists != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"❌ LOGIN FEHLGESCHLAGEN: Benutzer gefunden, aber PASSWORT FALSCH!");
                            System.Diagnostics.Debug.WriteLine($"   Benutzer: '{userExists.Name}'");
                            System.Diagnostics.Debug.WriteLine($"   DB-Passwort: '{userExists.Passwort}'");
                            System.Diagnostics.Debug.WriteLine($"   Eingegebenes Passwort: '{passwort}'");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"❌ LOGIN FEHLGESCHLAGEN: Benutzer NICHT GEFUNDEN!");
                        }

                        return false;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"✅ LOGIN ERFOLGREICH: {benutzer.Name} (Rolle: {benutzer.RollenName})");

                        // ⚠️ WICHTIG: Benutzer-Objekt von Entity Framework detachen
                        // Sonst wird beim Löschen des Passworts die DB geändert!
                        db.Entry(benutzer).State = System.Data.Entity.EntityState.Detached;

                        // Jetzt sicher: Passwort nur aus dem IN-MEMORY Objekt löschen
                        benutzer.Passwort = null;
                        Benutzer.CurrentUser = benutzer;
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ LOGIN EXCEPTION: {ex.Message}");
                    MessageBox.Show(ex.Message, "Ein Fehler ist aufgetreten");
                    return false;
                }
            }
        } // Nutzerdaten mit DB abgleichen, bei Erfolg Passwort aus Arbeitsspeicher löschen und Nutzerdaten in Benutzer.CurrentUser hinterlegen
    }
}
