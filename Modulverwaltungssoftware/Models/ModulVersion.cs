using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Modulverwaltungssoftware
{
    public class ModulVersion
    {
        [Required]
        public int ModulVersionID { get; set; }
        [Required]
        public int ModulId { get; set; }
        public virtual Modul Modul { get; set; }
        [Required]
        [StringLength(25)]
        public string GueltigAbSemester { get; set; }
        public enum Status { Entwurf, InPruefungKoordination, InPruefungGremium, Aenderungsbedarf, Freigegeben, Archiviert}
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
        public void setStatus(int versionID)
        {
            // Status der ModulVersion in DB updaten
        }
        public void setDaten()
        {
            // Alle Daten der ModulVersion in DB updaten
        }
    }
}
