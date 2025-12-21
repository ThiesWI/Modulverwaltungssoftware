using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Data.Entity;
using System.Windows; // <--- Diese using-Direktive ergänzen

namespace Modulverwaltungssoftware
{
    public class ModulRepository
    {
        public static ModulVersion getModulVersion(int modulID)
        {
            try
            {
                using (var db = new Services.DatabaseContext())
                {
                    var version = db.ModulVersion
                        .Include("Modul") // <--- String-Überladung statt Lambda verwenden
                        .Where(v => v.ModulId == modulID && v.ModulStatus == ModulVersion.Status.Freigegeben)
                        .Include("Modul")
                        .OrderByDescending(v => v.Versionsnummer)
                        .FirstOrDefault();

                    if (version == null)
                        return null;
                    else
                        return version;
                }
            }
            catch (Exception ex) { throw; }
            }
        public static List<ModulVersion> getAllModulVersionen(int modulID)
        {
            try
            {
                using (var db = new Services.DatabaseContext())
                {
                    var versionen = db.ModulVersion
                        .Include("Modul")
                        .Include("Kommentar") // Kommentare mitladen
                        .Where(v => v.ModulId == modulID)
                        .OrderByDescending(v => v.Versionsnummer)
                        .ToList();

                    if (versionen == null || versionen.Count == 0)
                    {
                        MessageBox.Show($"Keine Version für Modul mit ID {modulID} gefunden.");
                        return null;
                    }
                    else
                        return versionen;
                }
            }
            catch (Exception ex) { throw; }
        }
        public void Speichere (ModulVersion version, string aktuellerNutzer, string status) // Entwurf speichern für Dozent
        {
            try
            {
                if (status == "Entwurf" || status == "Aenderungsbedarf")
                {
                    ModulVersion.setDaten(version, (int)version.ModulVersionID, (int)version.ModulId, aktuellerNutzer);
                }
                else if (status == "Archiviert" || status == "Freigegeben")
                {
                    int neueVersionID = ModulController.create((int)version.Versionsnummer, (int)version.ModulId);
                    if (neueVersionID == 0)
                    {
                        MessageBox.Show("Fehler beim Erstellen einer neuen Version.");
                        return;
                    }
                    ModulVersion.setDaten(version, neueVersionID, (int)version.ModulId, aktuellerNutzer);
                }
                else MessageBox.Show("Speichern im Status 'InPruefung' nicht erlaubt.");
            }
            catch (Exception ex) { throw; }
            }
        public List<Modul> sucheModule(string suchbegriff)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(suchbegriff))
                {
                    MessageBox.Show("Suchbegriff darf nicht leer sein.");
                }
                var term = suchbegriff.ToLower();

                using (var db = new Services.DatabaseContext())
                {
                    var result = db.Modul
                        .Where(m =>
                            (m.ModulnameDE != null && m.ModulnameDE.ToLower().Contains(term)) ||
                            (m.ModulnameEN != null && m.ModulnameEN.ToLower().Contains(term)))
                        .ToList();

                    return result;
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
    public static List<Modul> getAllModule()
        {
            try
            {
                using (var db = new Services.DatabaseContext())
                {
                    var result = db.Modul
                        .Where(m =>
                            (m.GueltigAb != null && m.GueltigAb < DateTime.Now))
                        .ToList();

                    return result;
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
