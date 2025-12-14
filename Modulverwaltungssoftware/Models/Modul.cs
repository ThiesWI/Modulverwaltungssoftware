using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Modulverwaltungssoftware
{
    public class Modul
    {
        public int ModulID { get; set; }
        public string ModulnameDE { get; set; }
        public string ModulnameEN { get; set; }
        public enum Modultyp { Wahlpflicht, Grundlagen}
        public int EmpfohlenesSemester { get; set; }
        public enum Turnus { JedesSemester, NurWintersemester, NurSommersemester}
        public int DauerInSemestern { get; set; }
        public enum PruefungsForm { PL, SP, SL }
        public virtual ICollection<ModulVersion> ModulVersionen { get; set; }
        [NotMapped]
        public List<string> Voraussetzungen { get; set; }

        public string VoraussetzungenDb { get { return JsonConvert.SerializeObject(Voraussetzungen); }  set { if (string.IsNullOrEmpty(value)) { Voraussetzungen = new List<string>(); } else { Voraussetzungen = JsonConvert.DeserializeObject<List<string>>(value); } } }
    
        public Modul()
        {
            Voraussetzungen = new List<string>();
        }
    }
}
