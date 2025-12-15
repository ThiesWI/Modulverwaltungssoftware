using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Modulverwaltungssoftware
{
    public class Modul
    {
        public int ModulID { get; set; }
        [Required]
        [StringLength(200)]
        public string ModulnameDE { get; set; }
        [StringLength(200)]
        public string ModulnameEN { get; set; }
        public enum Modultyp { Wahlpflicht, Grundlagen}
        public int EmpfohlenesSemester { get; set; }
        public enum Turnus { JedesSemester, NurWintersemester, NurSommersemester}
        public int DauerInSemestern { get; set; } = 1;
        public enum PruefungsForm { PL, SP, SL }
        public virtual ICollection<ModulVersion> ModulVersionen { get; set; } = new List<ModulVersion>();
        [NotMapped]
        public List<string> Voraussetzungen { get; set; }
        [StringLength(4000)]
        public string VoraussetzungenDb { get { return JsonConvert.SerializeObject(Voraussetzungen); }  set { if (string.IsNullOrEmpty(value)) { Voraussetzungen = new List<string>(); } else { Voraussetzungen = JsonConvert.DeserializeObject<List<string>>(value); } } }
    
        public Modul()
        {
            Voraussetzungen = new List<string>();
        }
    }
}
