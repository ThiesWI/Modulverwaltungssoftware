using Newtonsoft.Json;
using System;
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
        public string Studiengang { get; set; }

        [Required]
        [StringLength(200)]
        public string ModulnameDE { get; set; }

        [StringLength(200)]
        public string ModulnameEN { get; set; }

        // Enum-Typen
        public enum ModultypEnum { Wahlpflicht = 0, Grundlagen = 1 }
        public enum TurnusEnum { JedesSemester = 0, NurWintersemester = 1, NurSommersemester = 2 }
        public enum PruefungsFormEnum { PL = 0, SP = 1, SL = 2 }

        // Properties, die die Werte halten
        [Required]
        public ModultypEnum Modultyp { get; set; }

        [Required]
        public TurnusEnum Turnus { get; set; }

        [Required]
        public PruefungsFormEnum PruefungsForm { get; set; }

        public int EmpfohlenesSemester { get; set; }
        public DateTime GueltigAb { get; set; } = DateTime.Now;
        public int DauerInSemestern { get; set; } = 1;

        public virtual ICollection<ModulVersion> ModulVersionen { get; set; } = new List<ModulVersion>();

        [NotMapped]
        public List<string> Voraussetzungen { get; set; }

        [StringLength(4000)]
        public string VoraussetzungenDb
        {
            get => JsonConvert.SerializeObject(Voraussetzungen);
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    Voraussetzungen = new List<string>();
                }
                else
                {
                    try
                    {
                        Voraussetzungen = JsonConvert.DeserializeObject<List<string>>(value);
                    }
                    catch
                    {
                        Voraussetzungen = new List<string> { value };
                    }
                }
            }
        } // List<string> zu DB-fähiger json konvertieren

        public Modul()
        {
            Voraussetzungen = new List<string>();
        }
    }
}
