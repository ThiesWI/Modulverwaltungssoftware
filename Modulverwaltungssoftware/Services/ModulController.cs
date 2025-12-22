using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Windows;

namespace Modulverwaltungssoftware
{
    public class ModulController
    {
        public static int create(int versionID, int modulID)
        {
            if (Benutzer.CurrentUser.AktuelleRolle.DarfBearbeiten == false ) { MessageBox.Show("Fehlende Berechtigungen zum Erstellen."); return 0; }
            try
            {
                var neueVersionID = versionID + 1;
                using (var db = new Services.DatabaseContext())
                {
                    var alteVersion = db.ModulVersion
                        .FirstOrDefault(v => v.ModulVersionID == versionID && v.ModulId == modulID);

                    if (alteVersion == null)
                    {
                        return 0;
                    }

                    var neueVersion = new ModulVersion
                    {
                        ModulId = alteVersion.ModulId,
                        Versionsnummer = neueVersionID,
                        GueltigAbSemester = "Entwurf",
                        Modul = alteVersion.Modul,
                        ModulStatus = ModulVersion.Status.Entwurf,
                        LetzteAenderung = DateTime.Now,
                        WorkloadPraesenz = alteVersion.WorkloadPraesenz,
                        WorkloadSelbststudium = alteVersion.WorkloadSelbststudium,
                        EctsPunkte = alteVersion.EctsPunkte,
                        Pruefungsform = alteVersion.Pruefungsform,
                        Literatur = new List<string>(alteVersion.Literatur),
                        Ersteller = Benutzer.CurrentUser.Name,
                        Lernergebnisse = new List<string>(alteVersion.Lernergebnisse),
                        Inhaltsgliederung = new List<string>(alteVersion.Inhaltsgliederung),
                        LernergebnisseDb = alteVersion.LernergebnisseDb,
                        InhaltsgliederungDb = alteVersion.InhaltsgliederungDb
                    };

                    db.ModulVersion.Add(neueVersion);
                    db.SaveChanges();

                    return neueVersionID;
                }
            }
            catch (DbEntityValidationException valEx) // Exception Handler-Block
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
        } // Neue ModulVersion erstellen und mit alten Daten füllen, DefaultStatus "Entwurf"
    }
}
