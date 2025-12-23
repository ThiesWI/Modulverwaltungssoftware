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
            using (var db = new Services.DatabaseContext())
            {
                // Neueste Version laden (unabhängig vom Status)
                var version = db.ModulVersion
                    .Include("Modul")
                    .Where(v => v.ModulId == modulID)
                    .OrderByDescending(v => v.Versionsnummer)
                    .FirstOrDefault();

                return version;
            }
        } // aktuellste ModulVersion für Modul aus DB abrufen
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
        } // alle Versionen von Modul abrufen
        public static void Speichere (ModulVersion version) // Entwurf speichern für Dozent
        {
            string fehlermeldung = PlausibilitaetsService.pruefeForm(version);
            if (fehlermeldung != "Keine Fehler gefunden.")
            {
                MessageBox.Show(fehlermeldung, "Moduldaten wurden nicht in die Datenbank übernommen.");
                return;
            }
            if (version == null)
            {
                MessageBox.Show("ModulVersion darf nicht null sein.");
                return;
            }
            else if (Benutzer.CurrentUser.AktuelleRolle.DarfFreigeben == false) { MessageBox.Show("Nur Benutzer mit Freigaberechten können speichern."); return; }
            try
                {
                    if (version.ModulStatus == ModulVersion.Status.Entwurf || version.ModulStatus == ModulVersion.Status.Aenderungsbedarf)
                    {
                        ModulVersion.setDaten(version);
                    }
                    else if (version.ModulStatus == ModulVersion.Status.Archiviert || version.ModulStatus == ModulVersion.Status.Freigegeben)
                    {
                        int neueVersionID = ModulController.create((int)version.Versionsnummer, (int)version.ModulId);
                        if (neueVersionID == 0)
                        {
                            MessageBox.Show("Fehler beim Erstellen einer neuen Version.");
                            return;
                        }
                        ModulVersion.setDaten(version);
                    }
                    else MessageBox.Show("Speichern im Status 'InPruefung' nicht erlaubt.");
                }
                catch (Exception ex) { throw; }
            }
        public static List<Modul> sucheModule(string suchbegriff)
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
                    }
                }

                throw new InvalidOperationException($"Validierung fehlgeschlagen: {string.Join(", ", fehlermeldungen)}", valEx);
            }
            catch (DbUpdateConcurrencyException conEx)
            {
                throw new InvalidOperationException("Der Datensatz wurde zwischenzeitlich von einem anderen Benutzer geändert. Bitte laden Sie die Seite neu.", conEx);
            }
            catch (DbUpdateException dbEx)
            {
                var innerMessage = dbEx.InnerException?.InnerException?.Message ?? dbEx.Message;

                if (innerMessage.Contains("UNIQUE constraint failed"))
                {
                    throw new InvalidOperationException("Dieser Eintrag existiert bereits (Duplikat).", dbEx);
                }

                throw new Exception($"Datenbankfehler beim Speichern: {innerMessage}", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception("Ein unerwarteter Fehler ist aufgetreten.", ex);
            }
        } // alle gültigen Module abrufen
        
        /// <summary>
        /// Gibt Module zurück die für den aktuellen Benutzer sichtbar sind (basierend auf Status und Rolle)
        /// </summary>
        public static List<Modul> GetModuleForUser()
        {
            try
            {
                using (var db = new Services.DatabaseContext())
                {
                    string currentUser = Benutzer.CurrentUser?.Name;
                    string rolle = Benutzer.CurrentUser?.RollenName ?? "Gast";
                    
                    // Admin, Koordination und Gremium sehen ALLE Module
                    if (rolle == "Admin" || rolle == "Koordination" || rolle == "Gremium")
                    {
                        return db.Modul
                            .Include("ModulVersionen")
                            .Where(m => m.GueltigAb != null && m.GueltigAb < DateTime.Now)
                            .OrderBy(m => m.ModulnameDE)
                            .ToList();
                    }
                    
                    // Dozent: Eigene Module (alle Stati) + Freigegebene Module anderer
                    if (rolle == "Dozent")
                    {
                        var modulIds = db.ModulVersion
                            .Where(v => 
                                // Eigene Module (alle Stati)
                                v.Ersteller == currentUser ||
                                // ODER: Freigegebene Module
                                v.ModulStatus == ModulVersion.Status.Freigegeben)
                            .Select(v => v.ModulId)
                            .Distinct()
                            .ToList();
                        
                        return db.Modul
                            .Include("ModulVersionen")
                            .Where(m => modulIds.Contains(m.ModulID) && 
                                       m.GueltigAb != null && m.GueltigAb < DateTime.Now)
                            .OrderBy(m => m.ModulnameDE)
                            .ToList();
                    }
                    
                    // Gast: NUR freigegebene Module
                    var freigegebeneModulIds = db.ModulVersion
                        .Where(v => v.ModulStatus == ModulVersion.Status.Freigegeben)
                        .Select(v => v.ModulId)
                        .Distinct()
                        .ToList();
                    
                    return db.Modul
                        .Include("ModulVersionen")
                        .Where(m => freigegebeneModulIds.Contains(m.ModulID) && 
                                   m.GueltigAb != null && m.GueltigAb < DateTime.Now)
                        .OrderBy(m => m.ModulnameDE)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Fehler beim Laden der Module: {ex.Message}", ex);
            }
        }
    }
}
