using System.Collections.Generic;

namespace Modulverwaltungssoftware
{
    internal class Modul
    {
        public List<ModulVersion> ModulVersionen { get; set; }
        public int Modulnummer { get; set; }
        public string ModulnameDE { get; set; }
        public string ModulnameEN { get; set; }
        public enum Modultyp { Placeholder }
        public int EmpfohlenesSemester { get; set; }
        public enum Turnus { JedesSemester, NurWintersemester, NurSommersemester}
        public int DauerInSemestern { get; set; }
    }
}
