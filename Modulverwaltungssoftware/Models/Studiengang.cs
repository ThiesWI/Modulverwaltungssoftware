using System;

namespace Modulverwaltungssoftware
{
    internal class Studiengang
    {
        string kuerzel { get; set; }
        string nameDE { get; set; }
        string nameEN { get; set; }
        int gesamtECTS { get; set; }
        DateTime gueltigAb { get; set; }
        Benutzer verantwortlicher { get; set; }
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
