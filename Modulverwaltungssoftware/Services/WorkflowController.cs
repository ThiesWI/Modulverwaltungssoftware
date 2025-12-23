using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Windows;

namespace Modulverwaltungssoftware
{
    public class WorkflowController
    {
        public static void starteGenehmigung(int versionsnummer, int modulID)
        {
            try
            {
                if (Benutzer.CurrentUser.AktuelleRolle.DarfBearbeiten)
                {
                    MessageBox.Show("Der aktuelle Benutzer hat nicht die erforderlichen Rechte, um die Genehmigung zu starten.");
                }
                else
                {
                    ModulVersion.setStatus(versionsnummer, modulID, ModulVersion.Status.InPruefungKoordination); // Set Status to "In Prüfung durch Koordination" & Sende Benachrichtigung an Koordination
                    BenachrichtigungsService.SendeBenachrichtigung("Koordination", $"{Benutzer.CurrentUser.Name} hat Version {versionsnummer} für Modul {modulID} zur Prüfung eingereicht.", versionsnummer);
                }
            }
            catch (Exception ex) { throw; }
        } // Modul zur Prüfung einreichen für Dozent und Admin
        public static void lehneAb(int versionsnummer, int modulID, string kommentarText)
        {
            try
            {
                if (Benutzer.CurrentUser.AktuelleRolle.DarfFreigeben == false)
                {
                    MessageBox.Show("Der aktuelle Benutzer hat nicht die erforderlichen Rechte, um die Genehmigung zu starten.");
                }
                else
                {
                    ModulVersion.setStatus(versionsnummer, modulID, ModulVersion.Status.Aenderungsbedarf);
                    BenachrichtigungsService.SendeBenachrichtigung ("Dozent", $"{Benutzer.CurrentUser.Name} hat Version {versionsnummer} für Modul {modulID} abgelehnt. Kommentar: {kommentarText}", versionsnummer);
                    Kommentar.addKommentar(modulID, versionsnummer, kommentarText);
                }
            }
            catch (Exception ex) { throw; }
        } // Ablehnen für Koordination + Admin
        public static void leiteWeiter(int versionsnummer, int modulID)
        {
            try
            {
                if (Benutzer.CurrentUser.RollenName != "Koordination" || Benutzer.CurrentUser.RollenName != "Admin")
                {
                    MessageBox.Show("Der aktuelle Benutzer hat nicht die erforderlichen Rechte, um die Genehmigung zu starten.");
                }
                else
                {
                    ModulVersion.setStatus(versionsnummer, modulID, ModulVersion.Status.InPruefungGremium);
                    BenachrichtigungsService.SendeBenachrichtigung("Gremium", $"{Benutzer.CurrentUser.Name} hat Version {versionsnummer} für Modul {modulID} zur Prüfung durch das Gremium weitergeleitet.", versionsnummer);
                }
            }
            catch (Exception ex) { throw; }
        } // Modul-Entwurf an Gremium weiterleiten (Koordination + Admin)
        public static void lehneFinalAb(int versionsnummer, int modulID, string kommentarText)
        {
            try
            {
                if (Benutzer.CurrentUser.RollenName != "Gremium" || Benutzer.CurrentUser.RollenName != "Admin")
                {
                    MessageBox.Show("Der aktuelle Benutzer hat nicht die erforderlichen Rechte, um die Genehmigung zu starten.");
                }
                else
                {
                    ModulVersion.setStatus(versionsnummer, modulID, ModulVersion.Status.Aenderungsbedarf);
                    BenachrichtigungsService.SendeBenachrichtigung("Dozent", $"{Benutzer.CurrentUser.Name} hat Version {versionsnummer} für Modul {modulID} final abgelehnt. Kommentar: {kommentarText}", versionsnummer);
                    Kommentar.addKommentar(modulID, versionsnummer, kommentarText);
                }
            }
            catch (Exception ex) { throw; }
        } // Ablehnen für Gremium + Admin
        public static void schliesseGenehmigungAb(int versionsnummer, int modulID)
        {
            try
            {
                if (Benutzer.CurrentUser.RollenName != "Gremium" || Benutzer.CurrentUser.RollenName != "Admin")
                {
                    MessageBox.Show("Der aktuelle Benutzer hat nicht die erforderlichen Rechte, um die Genehmigung zu starten.");
                }
                else
                {
                    ModulVersion.setStatus(versionsnummer, modulID, ModulVersion.Status.Freigegeben);
                    BenachrichtigungsService.SendeBenachrichtigung("Dozent", $"{Benutzer.CurrentUser.Name} hat Version {versionsnummer} für Modul {modulID} freigegeben.", versionsnummer);
                }
            }
            catch (Exception ex) { throw; }
            } // Gremium + Admin only -> Modul freigeben
        public static void archiviereVersion(int modulID, int versionID)
        {
            try
            {
                if (Benutzer.CurrentUser.AktuelleRolle.DarfStatusAendern == false)
                {
                    MessageBox.Show("Der aktuelle Benutzer hat nicht die erforderlichen Rechte, um die Genehmigung zu starten.");
                }
                else
                {
                    ModulVersion.setStatus(versionID, modulID, ModulVersion.Status.Archiviert);
                }
            }
            catch (Exception ex) { throw; }
            } // Status auf Archiviert setzen
        public static Modul getModulDetails(int modulID) // Modul aus DB abrufen
        {try
            {
                using (var db = new Services.DatabaseContext())
                {
                    var modul = db.Modul
                        .Where(m => m.ModulID == modulID)
                        .FirstOrDefault();

                    if (modul == null)
                    {
                        MessageBox.Show($"Modul mit ID {modulID} nicht gefunden.");
                        return null;
                    }
                    else
                    {
                        return modul;
                    }
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
        }
    }
}