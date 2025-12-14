using System;
using System.Collections.Generic;
using System.Data;

namespace Modulverwaltungssoftware
{
    public class ModulController
    {
        public void ErstelleModulVersion(string titel, string sprache, string pruefungsleistung, int sws, string modulCode, string turnus, int ects, int workloadPraesenz, int workloadSelbststudium, List<string> voraussetzungen, string verantwortlicher, List<string> lernziele, List<string> lehrinhalte, List<string> literatur)
        {
            throw new NotImplementedException(); // Lokale Erstellung einer ModulVersion?
        }
        public void create(ModulVersion version)
        {
            throw new NotImplementedException(); // ModulVersion in DB erstellen
        }
    }
}
