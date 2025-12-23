using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Windows;

namespace Modulverwaltungssoftware
{
    public class ModulController
    {
        public static int create(int modulID)
        {
            if (Benutzer.CurrentUser.AktuelleRolle.DarfBearbeiten == false ) { MessageBox.Show("Fehlende Berechtigungen zum Erstellen."); return 0; }
            try
            {
                int neueVersionsnummer;
                using (var db = new Services.DatabaseContext())
                {
                    var alteVersion = db.ModulVersion
                        .LastOrDefault(v => v.ModulId == modulID);

                    if (alteVersion == null)
                    {
                        return 0;
                    }
                    neueVersionsnummer = alteVersion.Versionsnummer + 1;
                    var neueVersion = new ModulVersion
                    {
                        ModulId = alteVersion.ModulId,
                        Versionsnummer = neueVersionsnummer,
                        GueltigAbSemester = "Entwurf",
                        Modul = alteVersion.Modul,
                        ModulStatus = ModulVersion.Status.Entwurf,
                        LetzteAenderung = DateTime.Now,
                        WorkloadPraesenz = alteVersion.WorkloadPraesenz,
                        WorkloadSelbststudium = alteVersion.WorkloadSelbststudium,
                        EctsPunkte = alteVersion.EctsPunkte,
                        Pruefungsform = alteVersion.Pruefungsform,
                        Literatur = new List<string>(alteVersion.Literatur),
                        Ersteller = Benutzer.CurrentUser.Name,
                        Lernergebnisse = new List<string>(alteVersion.Lernergebnisse),
                        Inhaltsgliederung = new List<string>(alteVersion.Inhaltsgliederung),
                        LernergebnisseDb = alteVersion.LernergebnisseDb,
                        InhaltsgliederungDb = alteVersion.InhaltsgliederungDb
                    };

                    db.ModulVersion.Add(neueVersion);
                    db.SaveChanges();

                    return neueVersionsnummer;
                }
            }
        catch(Exception ex)
        {
            MessageBox.Show($"Fehler beim Erstellen der neuen Modulversion: {ex.Message}");
            return 0;
        }
        } // Neue Modulversion erstellen, basierend auf der letzten Version
    }
}
