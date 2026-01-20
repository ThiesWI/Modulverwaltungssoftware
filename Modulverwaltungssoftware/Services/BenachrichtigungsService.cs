using Modulverwaltungssoftware.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Modulverwaltungssoftware
{
    public class BenachrichtigungsService
    {
        public static void SendeBenachrichtigung(string empfaenger, string nachricht, int betroffeneModulVersionID = 0)
        {
            try
            {
                if (Benutzer.CurrentUser == null)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Benachrichtigung kann nicht gesendet werden: CurrentUser ist null");
                    return;
                }

                using (var db = new Services.DatabaseContext())
                {
                    // ✅ FIX: Wenn Empfänger eine ROLLE ist, an ALLE Benutzer mit dieser Rolle senden
                    var empfaengerRollen = new[] { "Gast", "Dozent", "Koordination", "Gremium", "Admin" };

                    if (empfaengerRollen.Contains(empfaenger))
                    {
                        // ROLLEN-BASIERTE BENACHRICHTIGUNG
                        // Hole alle Benutzer mit dieser Rolle
                        var benutzerMitRolle = db.Benutzer
                            .Where(b => b.RollenName == empfaenger)
                            .ToList();

                        System.Diagnostics.Debug.WriteLine($"📨 Sende Benachrichtigung an ROLLE '{empfaenger}': {benutzerMitRolle.Count} Empfänger gefunden");

                        foreach (var benutzer in benutzerMitRolle)
                        {
                            var benachrichtigung = new Benachrichtigung
                            {
                                BetroffeneModulVersionID = betroffeneModulVersionID > 0 ? (int?)betroffeneModulVersionID : null,
                                Sender = Benutzer.CurrentUser.Name,
                                Empfaenger = benutzer.Name,  // ✅ BENUTZERNAME, nicht Rolle!
                                Nachricht = nachricht,
                                GesendetAm = System.DateTime.Now,
                                Gelesen = false
                            };
                            db.Benachrichtigung.Add(benachrichtigung);

                            System.Diagnostics.Debug.WriteLine($"   → Benachrichtigung an '{benutzer.Name}' erstellt");
                        }
                    }
                    else
                    {
                        // EINZELBENUTZER-BENACHRICHTIGUNG
                        var benachrichtigung = new Benachrichtigung
                        {
                            BetroffeneModulVersionID = betroffeneModulVersionID > 0 ? (int?)betroffeneModulVersionID : null,
                            Sender = Benutzer.CurrentUser.Name,
                            Empfaenger = empfaenger,  // Direkter Benutzername
                            Nachricht = nachricht,
                            GesendetAm = System.DateTime.Now,
                            Gelesen = false
                        };
                        db.Benachrichtigung.Add(benachrichtigung);

                        System.Diagnostics.Debug.WriteLine($"📨 Benachrichtigung an Einzelbenutzer '{empfaenger}' erstellt");
                    }

                    db.SaveChanges();
                    System.Diagnostics.Debug.WriteLine($"✅ Benachrichtigungen erfolgreich gespeichert");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ FEHLER beim Senden der Benachrichtigung: {ex.Message}");
                MessageBox.Show(ex.Message, "Ein Fehler ist aufgetreten");
                return;
            }
        } // Benachrichtigung "senden" (in DB speichern)

        public static List<Benachrichtigung> EmpfangeBenachrichtigung()
        {
            try
            {
                if (Benutzer.CurrentUser == null)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Benachrichtigungen können nicht geladen werden: CurrentUser ist null");
                    return new List<Benachrichtigung>();
                }

                using (var db = new Services.DatabaseContext())
                {
                    var benachrichtigungen = db.Benachrichtigung
                        .Where(b => b.Empfaenger == Benutzer.CurrentUser.Name && b.Gelesen == false)
                        .OrderByDescending(b => b.GesendetAm);
                    return benachrichtigungen.ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ein Fehler ist aufgetreten"); ;
                return null;
            }
        } // Alle Benachrichtigungen mit istGelesen == false für aktuellen Benuter aus DB abfragen

        public static void MarkiereAlsGelesen()
        {
            try
            {
                if (Benutzer.CurrentUser == null)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Benachrichtigungen können nicht als gelesen markiert werden: CurrentUser ist null");
                    return;
                }

                using (var db = new Services.DatabaseContext())
                {
                    var ungelesene = db.Benachrichtigung
                        .Where(b => b.Empfaenger == Benutzer.CurrentUser.Name && b.Gelesen == false)
                        .ToList();

                    foreach (var n in ungelesene)
                    {
                        n.Gelesen = true;
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ein Fehler ist aufgetreten"); ;
                return;
            }
        }   // Alle Benachrichtigungen für aktuellen Benutzer als gelesen markieren
    }
}
