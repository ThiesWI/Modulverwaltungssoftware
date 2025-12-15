using System;
using System.Collections.Generic;
using System.Linq;

namespace Modulverwaltungssoftware
{
    public class ModulRepository
    {
        public ModulVersion getModulVersion(int modulID)
        {
            using (var db = new Services.DatabaseContext())
            {
                var version = db.ModulVersion
                    .Where(v => v.ModulId == modulID && v.ModulStatus == ModulVersion.Status.Freigegeben)
                    .OrderByDescending(v => v.Versionsnummer)
                    .FirstOrDefault();

                if (version == null)
                {
                    throw new KeyNotFoundException($"Keine freigegebene Version für Modul mit ID {modulID} gefunden.");
                }
                else
                {
                    return version;
                }
            }
        }
        public void Speichere (ModulVersion version, string aktuellerNutzer, string status) // Entwurf speichern für Dozent
        {
            if (status == "Entwurf" || status == "Aenderungsbedarf")
            {
                ModulVersion.setDaten(version, (int)version.ModulVersionID, (int)version.ModulId, aktuellerNutzer);
            }
            else if (status == "Archiviert" || status == "Freigegeben")
            {
                int neueVersionID = ModulController.create((int)version.Versionsnummer, (int)version.ModulId);
                ModulVersion.setDaten(version, neueVersionID, (int)version.ModulId, aktuellerNutzer);
            }
            else throw new InvalidOperationException("Speichern im Status 'InPruefung' nicht erlaubt.");
        }
        public List<Modul> sucheModule(string suchbegriff)
        {
            if (string.IsNullOrWhiteSpace(suchbegriff))
            {
                throw new ArgumentException("Suchbegriff darf nicht leer sein.");
            }
            var term = suchbegriff.ToLower();

            using (var db = new Services.DatabaseContext())
            {
                var result = db.Modul
                    .Where(m =>
                        (m.ModulnameDE != null && m.ModulnameDE.ToLower().Contains(term)) ||
                        (m.ModulnameEN != null && m.ModulnameEN.ToLower().Contains(term)))
                    .ToList();

                return result;
            }
        }
    }
}
