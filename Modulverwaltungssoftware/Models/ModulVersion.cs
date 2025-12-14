using System;
using System.Collections.Generic;

namespace Modulverwaltungssoftware
{
    internal class ModulVersion
    {
        public int VersionID { get; set; }
        public string GueltigAbSemester { get; set; }
        public enum Status { Entwurf, InPruefungKoordination, InPruefungGremium, Aenderungsbedarf, Freigegeben, Archiviert}
        public List<string> Lernergebnisse { get; set; }
        public List<string> Inhaltsgliederung { get; set; }
        public int WorkloadPraesenz { get; set; }
        public int WorkloadSelbststudium { get; set; }
        public int EctsPunkte { get; set; }
        public string Pruefungsform { get; set; }
        public List<string> Literatur { get; set; }
        public Benutzer Ersteller { get; set; }
        public List<int> KommentarIDs { get; set; } = new List<int>();
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
