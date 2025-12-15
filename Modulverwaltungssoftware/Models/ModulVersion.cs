using Newtonsoft.Json;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
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
        public string LernergebnisseDb { get { return JsonConvert.SerializeObject(Lernergebnisse); } set { if (string.IsNullOrEmpty(value)) { Lernergebnisse = new List<string>(); } else { Lernergebnisse = JsonConvert.DeserializeObject<List<string>>(value); } } }
        [Required]
        [StringLength(4000)]
        public string InhaltsgliederungDb { get { return JsonConvert.SerializeObject(Inhaltsgliederung); } set { if (string.IsNullOrEmpty(value)) { Inhaltsgliederung = new List<string>(); } else { Inhaltsgliederung = JsonConvert.DeserializeObject<List<string>>(value); } } }

        public ModulVersion()
        {
            Lernergebnisse = new List<string>();
            Inhaltsgliederung = new List<string>();
        }
        public static void setStatus(int versionID, int modulID, Status neuerStatus)
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
        public static void setDaten(ModulVersion version, int modulVersionID, int modulID, string aktuelleBenutzerRolle)
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
    }
}
