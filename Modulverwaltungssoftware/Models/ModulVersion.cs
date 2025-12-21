using Newtonsoft.Json;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Transactions;

namespace Modulverwaltungssoftware
{
    public class ModulVersion
    {
        public int ModulVersionID { get; set; }
        [Required]
        public int ModulId { get; set; }
        [Required]
        public int Versionsnummer { get; set; }
        [Required]
        public bool hatKommentar { get; set; } = false;
        public virtual Kommentar Kommentar { get; set; }
        public virtual Modul Modul { get; set; }
        [Required]
        [StringLength(25)]
        public string GueltigAbSemester { get; set; }
        public enum Status { Entwurf = 0, InPruefungKoordination = 1, InPruefungGremium = 2, Aenderungsbedarf = 3, Freigegeben = 4, Archiviert = 5 }
        [Required]
        public Status ModulStatus { get; set; }
        [Required]
        public DateTime LetzteAenderung { get; set; } = DateTime.Now;
        [Required]
        public int WorkloadPraesenz { get; set; }
        [Required]
        public int WorkloadSelbststudium { get; set; }
        [Required]
        public int EctsPunkte { get; set; }
        [Required]
        [StringLength(100)]
        public string Pruefungsform { get; set; }
        public List<string> Literatur { get; set; }
        public string Ersteller { get; set; }
        [NotMapped]
        public List<string> Lernergebnisse { get; set; }
        [NotMapped]
        public List<string> Inhaltsgliederung { get; set; }
        [Required]
        [StringLength(4000)]
        public string LernergebnisseDb
        {
            get => JsonConvert.SerializeObject(Lernergebnisse);
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    Lernergebnisse = new List<string>();
                }
                else
                {
                    try
                    {
                        Lernergebnisse = JsonConvert.DeserializeObject<List<string>>(value);
                    }
                    catch
                    {
                        Lernergebnisse = new List<string> { value };
                    }
                }
            }
        }

        [Required]
        [StringLength(4000)]
        public string InhaltsgliederungDb
        {
            get => JsonConvert.SerializeObject(Inhaltsgliederung);
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    Inhaltsgliederung = new List<string>();
                }
                else
                {
                    try
                    {
                        Inhaltsgliederung = JsonConvert.DeserializeObject<List<string>>(value);
                    }
                    catch
                    {
                        Inhaltsgliederung = new List<string> { value };
                    }
                }
            }
        }

        public ModulVersion()
        {
            Lernergebnisse = new List<string>();
            Inhaltsgliederung = new List<string>();
        }
        public static void setStatus(int versionID, int modulID, Status neuerStatus)
        {
            try
            {
                using (var db = new Services.DatabaseContext())
                {
                    var modulVersion = db.ModulVersion.FirstOrDefault(mv => mv.ModulVersionID == versionID && mv.ModulId == modulID);
                    if (modulVersion != null)
                    {
                        modulVersion.ModulStatus = neuerStatus;
                        modulVersion.LetzteAenderung = DateTime.Now;
                        db.SaveChanges();
                    }
                    else
                    {
                        throw new Exception("ModulVersion nicht gefunden");
                    }
                }
            }
            catch (Exception ex) { throw; }
            }
        public static void setDaten(ModulVersion version, int modulVersionID, int modulID, string aktuelleBenutzerRolle)
        {
            try
            {
                using (var db = new Services.DatabaseContext())
                {
                    var modulVersion = db.ModulVersion.FirstOrDefault(mv => mv.ModulVersionID == modulVersionID && mv.ModulId == modulID);
                    if (modulVersion == null)
                    {
                        throw new KeyNotFoundException($"ModulVersion mit ID {modulVersionID} und/oder ModulID {modulID} nicht gefunden.");
                    }
                    if (aktuelleBenutzerRolle == "Dozent" || aktuelleBenutzerRolle == "Admin")
                    {
                        if (modulVersion.ModulStatus != Status.Entwurf && modulVersion.ModulStatus != Status.Aenderungsbedarf)
                        {
                            throw new UnauthorizedAccessException("Nur Module mit dem Status Entwurf oder Aenderungsbedarf können bearbeitet werden.");
                        }
                        modulVersion.GueltigAbSemester = version.GueltigAbSemester;
                        modulVersion.WorkloadPraesenz = version.WorkloadPraesenz;
                        modulVersion.WorkloadSelbststudium = version.WorkloadSelbststudium;
                        modulVersion.EctsPunkte = version.EctsPunkte;
                        modulVersion.Pruefungsform = version.Pruefungsform;
                        modulVersion.Literatur = version.Literatur;
                        modulVersion.Lernergebnisse = version.Lernergebnisse;
                        modulVersion.Inhaltsgliederung = version.Inhaltsgliederung;
                        modulVersion.LetzteAenderung = DateTime.Now;
                    }
                    else
                    {
                        throw new UnauthorizedAccessException("Der aktuelle Benutzer hat nicht die erforderlichen Rechte, um diese Aktion durchzuführen.");
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
        }
    }
}
