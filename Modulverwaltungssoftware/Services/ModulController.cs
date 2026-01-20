using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;

namespace Modulverwaltungssoftware
{
    public class ModulController
    {
        public static int create(int modulID)
        {
            if (Benutzer.CurrentUser.AktuelleRolle.DarfBearbeiten == false) { MessageBox.Show("Fehlende Berechtigungen zum Erstellen."); return 0; }
            try
            {
                int neueVersionsnummer;
                using (var db = new Services.DatabaseContext())
                {
                    var alteVersion = db.ModulVersion
                        .Where(v => v.ModulId == modulID)
                        .OrderByDescending(v => v.Versionsnummer)
                        .FirstOrDefault();

                    if (alteVersion == null)
                    {
                        var erstelleVersion = new ModulVersion
                        {
                            ModulId = modulID,
                            Versionsnummer = 10,
                            GueltigAbSemester = "Entwurf",
                            ModulStatus = ModulVersion.Status.Entwurf,
                            LetzteAenderung = DateTime.Now,
                            WorkloadPraesenz = 0,
                            WorkloadSelbststudium = 0,
                            EctsPunkte = 0,
                            Pruefungsform = "",
                            Literatur = new List<string>(),
                            Ersteller = Benutzer.CurrentUser.Name,
                            Lernergebnisse = new List<string>(),
                            Inhaltsgliederung = new List<string>()
                        };
                        db.ModulVersion.Add(erstelleVersion);
                        db.SaveChanges();
                        return 1;
                    }
                    neueVersionsnummer = (alteVersion.Versionsnummer / 10 + 1) * 10;
                    var neueVersion = new ModulVersion
                    {
                        ModulId = alteVersion.ModulId,
                        Versionsnummer = neueVersionsnummer,
                        GueltigAbSemester = "Entwurf",
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
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Erstellen der neuen Modulversion: {ex.Message}");
                return -1;
            }
        } // Neue Modulversion erstellen, basierend auf der letzten Version
        public static int create(int modulID, ModulVersion version)
        {
            if (Benutzer.CurrentUser.AktuelleRolle.DarfBearbeiten == false) { MessageBox.Show("Fehlende Berechtigungen zum Erstellen."); return 0; }
            try
            {
                using (var db = new Services.DatabaseContext())
                {
                    var alteVersion = db.ModulVersion
                        .Where(v => v.ModulId == modulID)
                        .OrderByDescending(v => v.Versionsnummer)
                        .FirstOrDefault();

                    if (alteVersion == null)
                    {
                        var erstelleVersion = new ModulVersion
                        {
                            ModulId = modulID,
                            Versionsnummer = version.Versionsnummer,
                            GueltigAbSemester = "Entwurf",
                            ModulStatus = ModulVersion.Status.Entwurf,
                            LetzteAenderung = DateTime.Now,
                            WorkloadPraesenz = version.WorkloadPraesenz,
                            WorkloadSelbststudium = version.WorkloadSelbststudium,
                            EctsPunkte = version.EctsPunkte,
                            Pruefungsform = version.Pruefungsform,
                            Literatur = version.Literatur,
                            Ersteller = version.Ersteller,
                            Lernergebnisse = version.Lernergebnisse,
                            Inhaltsgliederung = version.Inhaltsgliederung
                        };
                        db.ModulVersion.Add(erstelleVersion);
                        Modul modul = db.Modul.FirstOrDefault(m => m.ModulID == modulID);
                        modul.Turnus = version.Modul.Turnus;
                        modul.Modultyp = version.Modul.Modultyp;
                        modul.PruefungsForm = version.Modul.PruefungsForm;
                        modul.EmpfohlenesSemester = version.Modul.EmpfohlenesSemester;
                        modul.GueltigAb = version.Modul.GueltigAb;
                        modul.DauerInSemestern = version.Modul.DauerInSemestern;
                        modul.Voraussetzungen = version.Modul.Voraussetzungen;
                        db.SaveChanges();
                        return version.Versionsnummer;
                    }
                    int neueVersionsnummer = (alteVersion.Versionsnummer / 10 + 1) * 10;
                    var neueVersion = new ModulVersion
                    {
                        ModulId = version.ModulId,
                        Versionsnummer = neueVersionsnummer,
                        GueltigAbSemester = "Entwurf",
                        ModulStatus = ModulVersion.Status.Entwurf,
                        LetzteAenderung = DateTime.Now,
                        WorkloadPraesenz = version.WorkloadPraesenz,
                        WorkloadSelbststudium = version.WorkloadSelbststudium,
                        EctsPunkte = version.EctsPunkte,
                        Pruefungsform = version.Pruefungsform,
                        Literatur = version.Literatur,
                        Ersteller = version.Ersteller,
                        Lernergebnisse = version.Lernergebnisse,
                        Inhaltsgliederung = version.Inhaltsgliederung,
                    };
                    db.ModulVersion.Add(neueVersion);
                    Modul mod = db.Modul.FirstOrDefault(m => m.ModulID == modulID);
                    mod.Turnus = version.Modul.Turnus;
                    mod.Modultyp = version.Modul.Modultyp;
                    mod.PruefungsForm = version.Modul.PruefungsForm;
                    mod.EmpfohlenesSemester = version.Modul.EmpfohlenesSemester;
                    mod.GueltigAb = version.Modul.GueltigAb;
                    mod.DauerInSemestern = version.Modul.DauerInSemestern;
                    mod.Voraussetzungen = version.Modul.Voraussetzungen;
                    db.SaveChanges();

                    // Erfolgsmeldung nur im Frontend!
                    return neueVersionsnummer;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Erstellen der neuen Modulversion: {ex.Message}");
                return -1;
            }
        } // Neue Modulversion erstellen, basierend auf der letzten Version
    }
}
