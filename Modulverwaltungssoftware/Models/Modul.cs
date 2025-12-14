using System.Collections.Generic;

namespace Modulverwaltungssoftware
{
    public class Modul
    {
        public List<ModulVersion> ModulVersionen { get; set; }
        public int Modulnummer { get; set; }
        public string ModulnameDE { get; set; }
        public string ModulnameEN { get; set; }
        public enum Modultyp { Wahlpflicht, Grundlagen}
        public List<int> Voraussetzungen { get; set; }
        public int EmpfohlenesSemester { get; set; }
        public enum Turnus { JedesSemester, NurWintersemester, NurSommersemester}
        public int DauerInSemestern { get; set; }
        public enum PruefungsForm { PL, SP, SL }
    }
}
