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
        public void starteGenehmigung(int versionID, int modulID, string aktuellerBenutzer)
        {
            try
            {
                if (aktuellerBenutzer != "Dozent" || aktuellerBenutzer != "Koordination" || aktuellerBenutzer != "Admin")
                {
                    MessageBox.Show("Der aktuelle Benutzer hat nicht die erforderlichen Rechte, um die Genehmigung zu starten.");
                }
                else
                {
                    ModulVersion.setStatus(versionID, modulID, ModulVersion.Status.InPruefungKoordination); // Set Status to "In Prüfung durch Koordination" & Sende Benachrichtigung an Koordination
                    BenachrichtigungsService.SendeBenachrichtigung(aktuellerBenutzer, "Koordination", $"{aktuellerBenutzer} hat Version {versionID} für Modul {modulID} zur Prüfung eingereicht.", versionID);
                }
            }
            catch (Exception ex) { throw; }
        }
        public void lehneAb(int modulID, int versionID, string kommentarText, string aktuellerBenutzer)
        {
            try
            {
                if (aktuellerBenutzer != "Koordination" || aktuellerBenutzer != "Gremium" || aktuellerBenutzer != "Admin")
                {
                    MessageBox.Show("Der aktuelle Benutzer hat nicht die erforderlichen Rechte, um die Genehmigung zu starten.");
                }
                else
                {
                    ModulVersion.setStatus(versionID, modulID, ModulVersion.Status.Aenderungsbedarf);
                    BenachrichtigungsService.SendeBenachrichtigung(aktuellerBenutzer, "Dozent", $"{aktuellerBenutzer} hat Version {versionID} für Modul {modulID} abgelehnt. Kommentar: {kommentarText}", versionID);
                    Kommentar.addKommentar(modulID, versionID, kommentarText);
                }
            }
            catch (Exception ex) { throw; }
        }
        public void leiteWeiter(int modulID, int versionID, string aktuellerBenutzer)
        {
            try
            {
                if (aktuellerBenutzer != "Koordination" || aktuellerBenutzer != "Admin")
                {
                    MessageBox.Show("Der aktuelle Benutzer hat nicht die erforderlichen Rechte, um die Genehmigung zu starten.");
                }
                else
                {
                    ModulVersion.setStatus(versionID, modulID, ModulVersion.Status.InPruefungGremium);
                    BenachrichtigungsService.SendeBenachrichtigung(aktuellerBenutzer, "Gremium", $"{aktuellerBenutzer} hat Version {versionID} für Modul {modulID} zur Prüfung durch das Gremium weitergeleitet.", versionID);
                }
            }
            catch (Exception ex) { throw; }
        }
        public void lehneFinalAb(int modulID, int versionID, string kommentarText, string aktuellerBenutzer)
        {
            try
            {
                if (aktuellerBenutzer != "Gremium" || aktuellerBenutzer != "Admin")
                {
                    MessageBox.Show("Der aktuelle Benutzer hat nicht die erforderlichen Rechte, um die Genehmigung zu starten.");
                }
                else
                {
                    ModulVersion.setStatus(versionID, modulID, ModulVersion.Status.Aenderungsbedarf);
                    BenachrichtigungsService.SendeBenachrichtigung(aktuellerBenutzer, "Dozent", $"{aktuellerBenutzer} hat Version {versionID} für Modul {modulID} final abgelehnt. Kommentar: {kommentarText}", versionID);
                    Kommentar.addKommentar(modulID, versionID, kommentarText);
                }
            }
            catch (Exception ex) { throw; }
        }
        public void schliesseGenehmigungAb(int modulID, int versionID, string aktuellerBenutzer)
        {
            try
            {
                if (aktuellerBenutzer != "Gremium" || aktuellerBenutzer != "Admin")
                {
                    MessageBox.Show("Der aktuelle Benutzer hat nicht die erforderlichen Rechte, um die Genehmigung zu starten.");
                }
                else
                {
                    ModulVersion.setStatus(versionID, modulID, ModulVersion.Status.Freigegeben);
                    BenachrichtigungsService.SendeBenachrichtigung(aktuellerBenutzer, "Dozent", $"{aktuellerBenutzer} hat Version {versionID} für Modul {modulID} freigegeben.", versionID);
                }
            }
            catch (Exception ex) { throw; }
            }
        public void archiviereVersion(int modulID, int versionID, string aktuellerBenutzer)
        {
            try
            {
                if (aktuellerBenutzer != "Gremium" || aktuellerBenutzer != "Admin")
                {
                    MessageBox.Show("Der aktuelle Benutzer hat nicht die erforderlichen Rechte, um die Genehmigung zu starten.");
                }
                else
                {
                    ModulVersion.setStatus(versionID, modulID, ModulVersion.Status.Archiviert);
                }
            }
            catch (Exception ex) { throw; }
            }
        public static Modul getModulDetails(int modulID)
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