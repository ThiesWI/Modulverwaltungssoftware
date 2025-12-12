using System;

namespace Modulverwaltungssoftware
{
    internal class ModulVersion
    {
        int versionID { get; set; }
        string gueltigAbSemester { get; set; }
        enum Status { Entwurf, InPruefungKoordination, InPruefungGremium, Aenderungsbedarf, Freigegeben, Archiviert}
        string lernergebnisse { get; set; }
        string inhaltsgliederung { get; set; }
        int workloadPraesenz { get; set; }
        int workloadSelbststudium { get; set; }
        int ectsPunkte { get; set; }
        string pruefungsform { get; set; }
        string literatur { get; set; }
        Benutzer ersteller { get; set; }
        public void setStatus()
        {
            throw new NotImplementedException();
        }
        public void setDaten()
        {
            throw new NotImplementedException();
        }
    }
}
