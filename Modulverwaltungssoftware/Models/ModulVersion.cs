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
using System.Windows;

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
        public string Ersteller { get; set; }
        [NotMapped]
        public List<string> Lernergebnisse { get; set; }
        [NotMapped]
        public List<string> Inhaltsgliederung { get; set; }
        [NotMapped]
        public List<string> Literatur { get; set; }
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

        [StringLength(4000)]
        public string LiteraturDb
        {
            get => JsonConvert.SerializeObject(Literatur);
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    Literatur = new List<string>();
                }
                else
                {
                    try
                    {
                        Literatur = JsonConvert.DeserializeObject<List<string>>(value);
                    }
                    catch
                    {
                        Literatur = new List<string> { value };
                    }
                }
            }
        }

        public ModulVersion()
        {
            Lernergebnisse = new List<string>();
            Inhaltsgliederung = new List<string>();
            Literatur = new List<string>();
        }
        public static void setStatus(int versionID, int modulID, Status neuerStatus) //Setzt den Status der Modulversion
        {
            try
            {
                if (Benutzer.CurrentUser.RollenName != "Dozent" && Benutzer.CurrentUser.RollenName != "Admin" && Benutzer.CurrentUser.RollenName != "Koordination" && Benutzer.CurrentUser.RollenName != "Gremium")
                {
                    MessageBox.Show("Der aktuelle Benutzer hat nicht die erforderlichen Rechte, um diese Aktion durchzuführen.");
                    return;
                }
                using (var db = new Services.DatabaseContext())
                {
                    var modulVersion = db.ModulVersion.FirstOrDefault(mv => mv.Versionsnummer == versionID && mv.ModulId == modulID);
                    if (modulVersion != null)
                    {
                        modulVersion.ModulStatus = neuerStatus;
                        modulVersion.LetzteAenderung = DateTime.Now;
                        db.SaveChanges();
                    }
                    else
                    {
                        MessageBox.Show("ModulVersion nicht gefunden");
                    }
                }
            }
            catch (Exception ex) { throw; }
            }
        public static void setDaten(ModulVersion version) //Setzt die Daten der Modulversion in der DB
        {
            string fehlermeldung = PlausibilitaetsService.pruefeForm(version);
            if (fehlermeldung != "Keine Fehler gefunden.")
            {
                MessageBox.Show(fehlermeldung, "Moduldaten wurden nicht in die Datenbank übernommen.");
                return;
            }
            try
            {
                using (var db = new Services.DatabaseContext())
                {
                    var modulVersion = db.ModulVersion.FirstOrDefault(mv => mv.Versionsnummer == version.Versionsnummer && mv.ModulId == version.ModulId);
                    if (modulVersion == null)
                    {
                        MessageBox.Show($"ModulVersion mit ID {version.Versionsnummer} und/oder ModulID {version.ModulId} nicht gefunden.");
                        return;
                    }
                    if (Benutzer.CurrentUser.AktuelleRolle.DarfBearbeiten == true)
                    {
                        if (modulVersion.ModulStatus != Status.Entwurf && modulVersion.ModulStatus != Status.Aenderungsbedarf)
                        {
                            MessageBox.Show("Nur Module mit dem Status Entwurf oder Aenderungsbedarf können bearbeitet werden.");
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
                       MessageBox.Show("Der aktuelle Benutzer hat nicht die erforderlichen Rechte, um diese Aktion durchzuführen.");
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
