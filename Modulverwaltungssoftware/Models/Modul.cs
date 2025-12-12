namespace Modulverwaltungssoftware
{
    internal class Modul
    {
        string modulnummer { get; set; }
        string modulnameDE { get; set; }
        string modulnameEN { get; set; }
        enum Modultyp { Placeholder }
        int empfohlenesSemester { get; set; }
        enum Turnus { JedesSemester, NurWintersemester, NurSommersemester}
        int dauerInSemestern { get; set; }
    }
}
