using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Modulverwaltungssoftware
{
    public class ModulController
    {
        public static int create(int versionID, int modulID)
        {
            var neueVersionID = versionID + 1;
            using (var db = new Services.DatabaseContext())
            {
                var alteVersion = db.ModulVersion
                    .FirstOrDefault(v => v.ModulVersionID == versionID && v.ModulId == modulID);

                if (alteVersion == null)
                {
                    throw new KeyNotFoundException($"ModulVersion mit ID {versionID} für Modul {modulID} nicht gefunden.");
                }

                var neueVersion = new ModulVersion
                {
                    ModulId = alteVersion.ModulId,
                    Versionsnummer = neueVersionID,
                    GueltigAbSemester = alteVersion.GueltigAbSemester,
                    Modul = alteVersion.Modul,
                    ModulStatus = alteVersion.ModulStatus,
                    LetzteAenderung = DateTime.Now,
                    WorkloadPraesenz = alteVersion.WorkloadPraesenz,
                    WorkloadSelbststudium = alteVersion.WorkloadSelbststudium,
                    EctsPunkte = alteVersion.EctsPunkte,
                    Pruefungsform = alteVersion.Pruefungsform,
                    Literatur = new List<string>(alteVersion.Literatur),
                    Ersteller = alteVersion.Ersteller,
                    Lernergebnisse = new List<string>(alteVersion.Lernergebnisse),
                    Inhaltsgliederung = new List<string>(alteVersion.Inhaltsgliederung),
                    LernergebnisseDb = alteVersion.LernergebnisseDb,
                    InhaltsgliederungDb = alteVersion.InhaltsgliederungDb
                };

                db.ModulVersion.Add(neueVersion);
                db.SaveChanges();

                return neueVersionID;
            }
        }
    }
}
