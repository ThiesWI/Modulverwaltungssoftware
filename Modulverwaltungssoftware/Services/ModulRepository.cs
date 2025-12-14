using System;

namespace Modulverwaltungssoftware
{
    internal class ModulRepository
    {
        public ModulVersion getModulVersion(string modulID)
        {
            throw new NotImplementedException(); // neueste ModulVersion aus DB holen
        }
        public void Speichere (ModulVersion version) // Entwurf speichern für Dozent
        {
            throw new NotImplementedException();
        }
        public void Speichere (ModulVersion version, Kommentar kommentar) // Entwurf speichern mit Kommentar für Koordination/Gremium
        {
            throw new NotImplementedException();
        }
    }
}
