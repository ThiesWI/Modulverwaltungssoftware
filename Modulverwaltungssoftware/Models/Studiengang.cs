using System;

namespace Modulverwaltungssoftware
{
    internal class Studiengang
    {
        public string Kuerzel { get; set; }
        public string NameDE { get; set; }
        public string NameEN { get; set; }
        public int GesamtECTS { get; set; }
        public DateTime GueltigAb { get; set; }
        public Benutzer Verantwortlicher { get; set; }
        public void getAktuelleModule() 
        {
            throw new NotImplementedException();
        }
        public void addModul(Modul modul)
        {
            throw new NotImplementedException();
        }
        public void removeModul(Modul modul)
        {
            throw new NotImplementedException();
        }   
        public bool istKomplett()
        {
            throw new NotImplementedException();
        }
    }
}
