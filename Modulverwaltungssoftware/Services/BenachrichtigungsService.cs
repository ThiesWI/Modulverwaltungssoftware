
using Modulverwaltungssoftware.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;

namespace Modulverwaltungssoftware
{
    public class BenachrichtigungsService
    {
        public static void SendeBenachrichtigung(string empfaenger, string nachricht, int betroffeneModulVersionID = 0)
        {
            try
            {
                using (var db = new Services.DatabaseContext())
                {
                    if (betroffeneModulVersionID == 0)
                    {
                        var benachrichtigung = new Benachrichtigung
                        {
                            Sender = Benutzer.CurrentUser.Name,
                            Empfaenger = empfaenger,
                            Nachricht = nachricht,
                            GesendetAm = System.DateTime.Now,
                            Gelesen = false
                        };
                        db.Benachrichtigung.Add(benachrichtigung);
                    }
                    else
                    {
                        var benachrichtigung = new Benachrichtigung
                        {
                            BetroffeneModulVersionID = betroffeneModulVersionID,
                            Sender = Benutzer.CurrentUser.Name,
                            Empfaenger = empfaenger,
                            Nachricht = nachricht,
                            GesendetAm = System.DateTime.Now,
                            Gelesen = false
                        };
                        db.Benachrichtigung.Add(benachrichtigung);
                    }
                    db.SaveChanges();
                }
            }
            catch (DbEntityValidationException valEx)
            {
                var fehlermeldungen = new List<string>();

                foreach (var validationErrors in valEx.EntityValidationErrors)
                {
                    foreach (var error in validationErrors.ValidationErrors)
                    {
                        string meldung = $"Feld '{error.PropertyName}': {error.ErrorMessage}";
                        fehlermeldungen.Add(meldung);
                        // Optional: Logging hier (Console.WriteLine(meldung));
                    }
                }

                // Wirf eine neue, saubere Exception mit allen Details, damit das Frontend sie anzeigen kann
                throw new InvalidOperationException($"Validierung fehlgeschlagen: {string.Join(", ", fehlermeldungen)}", valEx);
            }
            // 2. Gleichzeitigkeitsfehler (Optimistic Concurrency)
            // Tritt auf, wenn zwei User gleichzeitig speichern wollen.
            catch (DbUpdateConcurrencyException conEx)
            {
                // Reload der Werte oder Fehlermeldung
                throw new InvalidOperationException("Der Datensatz wurde zwischenzeitlich von einem anderen Benutzer geändert. Bitte laden Sie die Seite neu.", conEx);
            }
            // 3. Datenbank-Update-Fehler (z.B. Foreign Key Verletzung, Unique Constraint)
            catch (DbUpdateException dbEx)
            {
                var innerMessage = dbEx.InnerException?.InnerException?.Message ?? dbEx.Message;

                if (innerMessage.Contains("UNIQUE constraint failed"))
                {
                    throw new InvalidOperationException("Dieser Eintrag existiert bereits (Duplikat).", dbEx);
                }

                throw new Exception($"Datenbankfehler beim Speichern: {innerMessage}", dbEx);
            }
            // 4. Allgemeine Fehler (NullReference, Logikfehler etc.)
            catch (Exception ex)
            {
                // Hier solltest du idealerweise Loggen (in eine Datei oder DB)
                // Logger.LogError(ex);

                throw new Exception("Ein unerwarteter Fehler ist aufgetreten.", ex);
            }
        } // Benachrichtigung "senden" (in DB speichern)
        public static List<Benachrichtigung> EmpfangeBenachrichtigung()
        {
            try
            {
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
                throw;
            }
        } // Alle Benachrichtigungen mit istGelesen == false für aktuellen Benuter aus DB abfragen
        public static void MarkiereAlsGelesen()
        {
            try
            {
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
            catch (Exception ex) { throw; }
        }   // Alle Benachrichtigungen für aktuellen Benutzer als gelesen markieren
    }
}
