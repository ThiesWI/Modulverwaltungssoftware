using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows; // <--- Diese using-Direktive ergänzen

namespace Modulverwaltungssoftware
{
    public class ModulRepository
    {
        public static ModulVersion getModulVersion(int modulID)
        {
            try
            {
                using (var db = new Services.DatabaseContext())
                {
                    // Neueste Version laden (unabhängig vom Status)
                    var version = db.ModulVersion
                        .Include("Modul")
                        .Include("Kommentar")
                        .Where(v => v.ModulId == modulID)
                        .OrderByDescending(v => v.Versionsnummer)
                        .FirstOrDefault();

                    return version;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ein Fehler ist aufgetreten"); ;
                return null;
            }
        } // aktuellste ModulVersion für Modul aus DB abrufen
        public static ModulVersion getModulVersion(int modulID, int versionsnummer)
        {
            try
            {
                using (var db = new Services.DatabaseContext())
                {
                    // Neueste Version laden (unabhängig vom Status)
                    var version = db.ModulVersion
                        .Include("Modul")
                        .Include("Kommentar")
                        .Where(v => v.ModulId == modulID && v.Versionsnummer == versionsnummer)
                        .OrderByDescending(v => v.Versionsnummer)
                        .FirstOrDefault();

                    return version;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ein Fehler ist aufgetreten"); ;
                return null;
            }
        } // aktuellste ModulVersion für Modul aus DB abrufen
        public static List<ModulVersion> getAllModulVersionen(int modulID)
        {
            try
            {
                using (var db = new Services.DatabaseContext())
                {
                    var versionen = db.ModulVersion
                        .Include("Modul")
                        .Include("Kommentar") // Kommentare mitladen
                        .Where(v => v.ModulId == modulID)
                        .OrderByDescending(v => v.Versionsnummer)
                        .ToList();

                    // Gib IMMER eine Liste zurück, auch wenn sie leer ist
                    return versionen;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ein Fehler ist aufgetreten"); ;
                return null;
            }
        } // alle Versionen von Modul abrufen
        public static bool Speichere(ModulVersion version)
        {
            string fehlermeldung = PlausibilitaetsService.pruefeForm(version);
            if (fehlermeldung != "Keine Fehler gefunden.")
            {
                MessageBox.Show(fehlermeldung, "Moduldaten wurden nicht in die Datenbank übernommen.");
                return false;
            }
            if (version == null)
            {
                MessageBox.Show("ModulVersion darf nicht null sein.");
                return false;
            }
            else if (Benutzer.CurrentUser.AktuelleRolle.DarfBearbeiten == false && Benutzer.CurrentUser.AktuelleRolle.DarfFreigeben == false)
            {
                MessageBox.Show("Nur Benutzer mit Bearbeitungs- oder Freigaberechten können speichern.");
                return false;
            }
            try
            {
                using (var db = new Services.DatabaseContext())
                {
                    // Prüfe, ob das Modul existiert
                    var modul = db.Modul.FirstOrDefault(m => m.ModulID == version.ModulId);

                    // 1. Modul existiert nicht: Modul anlegen und initiale Version speichern
                    if (modul == null)
                    {
                        if (Benutzer.CurrentUser.AktuelleRolle.DarfBearbeiten == false)
                        {
                            MessageBox.Show("Der aktuelle Benutzer hat keine Berechtigung zum Anlegen von Modulen.");
                            return false;
                        }
                        if (version.Modul.GueltigAb == default)
                        {
                            version.Modul.GueltigAb = DateTime.Now;
                        }
                        db.Modul.Add(version.Modul);
                        db.SaveChanges();

                        var neueVersion = new ModulVersion
                        {
                            ModulId = version.Modul.ModulID,
                            Versionsnummer = 10,
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
                        db.SaveChanges();
                        return true;
                    }

                    // 2. Modul existiert, Version im Entwurf/Aenderungsbedarf: Update bestehende Version
                    if (version.ModulStatus == ModulVersion.Status.Entwurf || version.ModulStatus == ModulVersion.Status.Aenderungsbedarf)
                    {
                        var modulVersion = db.ModulVersion.FirstOrDefault(mv => mv.ModulId == version.ModulId && mv.Versionsnummer == version.Versionsnummer);
                        if (modulVersion == null)
                        {
                            // Neue Version anlegen
                            var neueVersion = new ModulVersion
                            {
                                ModulId = version.ModulId,
                                Versionsnummer = version.Versionsnummer,
                                GueltigAbSemester = version.GueltigAbSemester,
                                ModulStatus = version.ModulStatus,
                                LetzteAenderung = DateTime.Now,
                                WorkloadPraesenz = version.WorkloadPraesenz,
                                WorkloadSelbststudium = version.WorkloadSelbststudium,
                                EctsPunkte = version.EctsPunkte,
                                Pruefungsform = version.Pruefungsform,
                                Literatur = version.Literatur,
                                Ersteller = version.Ersteller,
                                Lernergebnisse = version.Lernergebnisse,
                                Inhaltsgliederung = version.Inhaltsgliederung,
                                hatKommentar = version.hatKommentar,
                            };
                            var mod1 = db.Modul.FirstOrDefault(m => m.ModulID == version.ModulId);
                            mod1.Turnus = version.Modul.Turnus;
                            mod1.ModulnameDE = version.Modul.ModulnameDE;
                            mod1.ModulnameEN = version.Modul.ModulnameEN;
                            mod1.Modultyp = version.Modul.Modultyp;
                            mod1.PruefungsForm = version.Modul.PruefungsForm;
                            mod1.EmpfohlenesSemester = version.Modul.EmpfohlenesSemester;
                            mod1.GueltigAb = version.Modul.GueltigAb;
                            mod1.DauerInSemestern = version.Modul.DauerInSemestern;
                            mod1.Voraussetzungen = version.Modul.Voraussetzungen;
                            mod1.Studiengang = version.Modul.Studiengang;
                            db.ModulVersion.Add(neueVersion);
                        }
                        else
                        {
                            // Bestehende Version aktualisieren
                            modulVersion.GueltigAbSemester = version.GueltigAbSemester;
                            modulVersion.ModulStatus = version.ModulStatus;
                            modulVersion.LetzteAenderung = DateTime.Now;
                            modulVersion.WorkloadPraesenz = version.WorkloadPraesenz;
                            modulVersion.WorkloadSelbststudium = version.WorkloadSelbststudium;
                            modulVersion.EctsPunkte = version.EctsPunkte;
                            modulVersion.Pruefungsform = version.Pruefungsform;
                            modulVersion.Literatur = version.Literatur;
                            modulVersion.Ersteller = version.Ersteller;
                            modulVersion.Lernergebnisse = version.Lernergebnisse;
                            modulVersion.Inhaltsgliederung = version.Inhaltsgliederung;
                            modulVersion.hatKommentar = version.hatKommentar;

                            var mod2 = db.Modul.FirstOrDefault(m => m.ModulID == version.ModulId);
                            mod2.Turnus = version.Modul.Turnus;
                            mod2.ModulnameDE = version.Modul.ModulnameDE;
                            mod2.ModulnameEN = version.Modul.ModulnameEN;
                            mod2.Modultyp = version.Modul.Modultyp;
                            mod2.PruefungsForm = version.Modul.PruefungsForm;
                            mod2.EmpfohlenesSemester = version.Modul.EmpfohlenesSemester;
                            mod2.GueltigAb = version.Modul.GueltigAb;
                            mod2.DauerInSemestern = version.Modul.DauerInSemestern;
                            mod2.Voraussetzungen = version.Modul.Voraussetzungen;
                            mod2.Studiengang = version.Modul.Studiengang;
                        }
                        db.SaveChanges();
                        return true;
                    }

                    // 3. Modul existiert, Version ist freigegeben/archiviert/sonst: Neue Version anlegen
                    if (version.ModulStatus == ModulVersion.Status.Freigegeben || version.ModulStatus == ModulVersion.Status.Archiviert)
                    {
                        int hoechsteVersionsnummer = db.ModulVersion
                            .Where(v => v.ModulId == version.ModulId)
                            .Select(v => v.Versionsnummer)
                            .DefaultIfEmpty(0)
                            .Max();

                        int neueVersionsnummer = (hoechsteVersionsnummer / 10 + 1) * 10;

                        var neueModulVersion = new ModulVersion
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
                        db.ModulVersion.Add(neueModulVersion);
                        var mod = db.Modul.FirstOrDefault(m => m.ModulID == version.ModulId);
                        mod.Turnus = version.Modul.Turnus;
                        mod.ModulnameDE = version.Modul.ModulnameDE;
                        mod.ModulnameEN = version.Modul.ModulnameEN;
                        mod.Modultyp = version.Modul.Modultyp;
                        mod.PruefungsForm = version.Modul.PruefungsForm;
                        mod.EmpfohlenesSemester = version.Modul.EmpfohlenesSemester;
                        mod.GueltigAb = version.Modul.GueltigAb;
                        mod.DauerInSemestern = version.Modul.DauerInSemestern;
                        mod.Voraussetzungen = version.Modul.Voraussetzungen;
                        mod.Studiengang = version.Modul.Studiengang;
                        db.SaveChanges();
                        return true;
                    }
                    else
                        MessageBox.Show("Module mit Status InPruefung dürfen nicht bearbeitet werden.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ein Fehler ist aufgetreten");
                return false;
            }
        }
        public static List<Modul> sucheModule(string suchbegriff)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(suchbegriff))
                {
                    MessageBox.Show("Suchbegriff darf nicht leer sein.");
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ein Fehler ist aufgetreten"); ;
                return null;
            }
        }
        public static List<Modul> getAllModule()
        {
            try
            {
                using (var db = new Services.DatabaseContext())
                {
                    var result = db.Modul
                        .Where(m =>
                            (m.GueltigAb != null && m.GueltigAb < DateTime.Now))
                        .ToList();

                    return result;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ein Fehler ist aufgetreten"); ;
                return null;
            }
        } // alle gültigen Module abrufen

        /// <summary>
        /// Gibt Module zurück die für den aktuellen Benutzer sichtbar sind (basierend auf Status und Rolle)
        /// </summary>
        public static List<Modul> GetModuleForUser()
        {
            try
            {
                using (var db = new Services.DatabaseContext())
                {
                    string currentUser = Benutzer.CurrentUser?.Name;
                    string rolle = Benutzer.CurrentUser?.RollenName ?? "Gast";

                    // Admin, Koordination und Gremium sehen ALLE Module
                    if (rolle == "Admin" || rolle == "Koordination" || rolle == "Gremium")
                    {
                        return db.Modul
                            .Include("ModulVersionen")
                            .Where(m => m.GueltigAb != null && m.GueltigAb < DateTime.Now)
                            .OrderBy(m => m.ModulnameDE)
                            .ToList();
                    }

                    // Dozent: Eigene Module (alle Stati) + Freigegebene Module anderer
                    if (rolle == "Dozent")
                    {
                        var modulIds = db.ModulVersion
                            .Where(v =>
                                // Eigene Module (alle Stati)
                                v.Ersteller == currentUser ||
                                // ODER: Freigegebene Module
                                v.ModulStatus == ModulVersion.Status.Freigegeben)
                            .Select(v => v.ModulId)
                            .Distinct()
                            .ToList();

                        return db.Modul
                            .Include("ModulVersionen")
                            .Where(m => modulIds.Contains(m.ModulID) &&
                                       m.GueltigAb != null && m.GueltigAb < DateTime.Now)
                            .OrderBy(m => m.ModulnameDE)
                            .ToList();
                    }

                    // Gast: NUR freigegebene Module
                    var freigegebeneModulIds = db.ModulVersion
                        .Where(v => v.ModulStatus == ModulVersion.Status.Freigegeben)
                        .Select(v => v.ModulId)
                        .Distinct()
                        .ToList();

                    return db.Modul
                        .Include("ModulVersionen")
                        .Where(m => freigegebeneModulIds.Contains(m.ModulID) &&
                                   m.GueltigAb != null && m.GueltigAb < DateTime.Now)
                        .OrderBy(m => m.ModulnameDE)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ein Fehler ist aufgetreten"); ;
                return null;
            }
        }
        public static int addModul(Modul modul)
        {
            if (Benutzer.CurrentUser.AktuelleRolle.DarfBearbeiten == false)
            {
                MessageBox.Show("Der aktuelle Benutzer hat keine Berechtigung zum Anlegen von Modulen.");
                return -1;
            }
            try
            {
                using (var db = new Services.DatabaseContext())
                {
                    if (modul == null)
                    {
                        MessageBox.Show($"Modul hat keinen Inhalt: {nameof(modul)}");
                        return -1;
                    }
                    if (modul.GueltigAb == default)
                    {
                        modul.GueltigAb = DateTime.Now;
                    }
                    db.Modul.Add(modul);
                    db.SaveChanges();
                    return modul.ModulID;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return -1;
            }
        }
    }
}
